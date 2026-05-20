
using Microsoft.EntityFrameworkCore;
using VacationPlanner.Web.Data;
using VacationPlanner.Web.Models.Domain;

public class WeatherService : IWeatherService
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICityService _cityService;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(AppDbContext context, IHttpClientFactory httpClientFactory, ICityService cityService, ILogger<WeatherService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _cityService = cityService;
        _logger = logger;
    }
    
    public async Task<List<WeatherRecord>> GetHistoricalWeatherAsync(string location, DateTime startDate, DateTime endDate)
    {
        var city = await _cityService.GetCityByNameAsync(location);

        if (city == null)
        {
            _logger.LogWarning("City not found: {Location}", location);
            return new List<WeatherRecord>();
        }

        var existingRecords = await _context.WeatherRecords
            .Where(w => w.CityId == city.Id && w.Date >= startDate && w.Date <= endDate)
            .ToListAsync();

        if (existingRecords.Any())
        {
            _logger.LogInformation("Found {Count} existing weather records for {Location} between {StartDate} and {EndDate}", existingRecords.Count, location, startDate, endDate);
            return existingRecords;
        }

        var records = await FetchWeatherFromOpenMeteoAsync(city, startDate, endDate);

        if (records.Any())
        {
            await SaveWeatherRecordAsync(records);
        }
        
        return records;
    }

    private async Task<List<WeatherRecord>> FetchWeatherFromOpenMeteoAsync(City city, DateTime startDate, DateTime endDate)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var records = new List<WeatherRecord>();

        var url = $"https://api.open-meteo.com/v1/forecast?latitude={city.Latitude}&longitude={city.Longitude}&start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&hourly=temperature_2m,precipitation_sum,windspeed_10m&timezone=auto";
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var weatherData = System.Text.Json.JsonSerializer.Deserialize<OpenMeteoResponse>(content);

            if (weatherData?.Hourly?.Time != null)
            {
                for (int i = 0; i < weatherData.Hourly.Time.Length; i++)
                {
                    var record = new WeatherRecord
                    {
                        CityId = city.Id,
                        Date = DateTime.Parse(weatherData.Hourly.Time[i]),
                        MaxTemp = weatherData.Hourly.Temperature2mMax?[i] ?? 0,
                        MinTemp = weatherData.Hourly.Temperature2mMin?[i] ?? 0,
                        AvgTemp = weatherData.Hourly.Temperature2mMean?[i] ?? 0
                    };
                    records.Add(record);
                }
            }


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data for {Location}", city.Name);
        }
        
        return records;
    }

    public Task SaveWeatherRecordAsync(List<WeatherRecord> record)
    {
        _context.WeatherRecords.AddRange(record);
        return _context.SaveChangesAsync();
    }
}

internal class OpenMeteoResponse
{
    public DailyData? Hourly { get; set; }
}

public class DailyData
{
    public string[]? Time { get; set; }
    public double[]? Temperature2mMax { get; set; }
    public double[]? Temperature2mMin { get; set; } 
    public double[]? Temperature2mMean { get; set; }
}