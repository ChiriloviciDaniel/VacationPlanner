using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VacationPlanner.Web.Data;
using VacationPlanner.Web.Models.Domain;

public class AttractionService : IAttractionService
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICityService _cityService;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public AttractionService(AppDbContext context, IHttpClientFactory httpClientFactory, ICityService cityService, ILogger<AttractionService> logger, IConfiguration config)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _cityService = cityService;
        _logger = logger;
        _config = config;
    }

    public async Task<List<Attraction>> GetAttractionsByLocationAsync(string location)
    {

        // 1. Get city from database
        var city = await _cityService.GetCityByNameAsync(location);

        if (city == null)
        {
            _logger.LogWarning($"City '{location}' not found in the database.");
            return new List<Attraction>();
        }

        //2. Check if we have cached attractions for this city
        var cachedAttractions = await _context.Attractions
        .Where(a => a.CityId == city.Id)
        .ToListAsync();

        if (cachedAttractions.Any())
        {
            _logger.LogInformation($"Returning cached attractions for city '{location}'.");
            return cachedAttractions;
        }

        //3. Fetch from OpenTripMap API

        var attractions = await FetchAttractionsFromOpenTripMapAsync(city);

        //4. Save to database for caching
        if (attractions.Any())
        {
            await SaveAttractionAsync(attractions);
        }
        return attractions;
    }

    private async Task<List<Attraction>> FetchAttractionsFromOpenTripMapAsync(City city)
    {
        var apiKey = _config["ApiKeys:OpenTripMap"];
        var httpClient = _httpClientFactory.CreateClient();
        var attractions = new List<Attraction>();

        try
        {
            var radius = 10000; // 10 km radius
            var requestUrl = $"https://api.opentripmap.com/0.1/en/places/radius?" +
                            $"radius={radius}&lon={city.Longitude}&lat={city.Latitude}" +
                            $"&kinds=interesting_places,tourist_facilities" +
                            $"&rate=3&limit=20&apikey ={apiKey}";

            var response = await httpClient.GetAsync(requestUrl);
            // Process the API response and populate the attractions list
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            // Deserialize the content and map it to the Attraction model
            var places = JsonSerializer.Deserialize<List<OpenTripMapPlace>>(content);

            if (places == null || !places.Any())
            {
                _logger.LogWarning("No attractions found for the specified location.");
                return new List<Attraction>();
            }

            //2. get details for each place 
            foreach (var place in places.Take(10))
            {
                if (string.IsNullOrEmpty(place.Xid))
                    continue;

                var detailsUrl = $"https://api.opentripmap.com/0.1/en/places/xid/{place.Xid}?apikey={apiKey}";
                var detailsResponse = await httpClient.GetAsync(detailsUrl);
                if (!detailsResponse.IsSuccessStatusCode)
                        continue;

                var detailsContent = await detailsResponse.Content.ReadAsStringAsync();
                var placeDetails = JsonSerializer.Deserialize<OpenTripMapDetails>(detailsContent);

                if (placeDetails != null)
                {
                    attractions.Add(new Attraction
                    {
                        Name = placeDetails.Name ?? "Unknown",
                        Description = placeDetails.Wikipedia_extracts ?? "No description available",
                        ImageUrl = placeDetails.Preview ?? string.Empty,
                        Rating = placeDetails.Rating ?? 0,
                        Kinds = placeDetails.Kinds ?? "Unknown",
                        CityId = city.Id
                    });
                }

                await Task.Delay(100); // To avoid hitting API rate limits
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching attractions from OpenTripMap API.");
        }

        return attractions;
    }

    public async Task SaveAttractionAsync(List<Attraction> attractions)
    {
        _context.Attractions.AddRange(attractions);
        await _context.SaveChangesAsync();
    }
    private class OpenTripMapPlace
    {
        public string? Xid { get; set; }
        public string? Name { get; set; }
        public double? rate { get; set; }
        public string? Kinds { get; set; }
    }

    private class OpenTripMapDetails
    {
        public string? Xid { get; set; }
        public string? Name { get; set; }
        public string? Wikipedia_extracts { get; set; }
        public string? Preview { get; set; }
        public double? Rating { get; set; }
        public string? Kinds { get; set; }
    }
}

