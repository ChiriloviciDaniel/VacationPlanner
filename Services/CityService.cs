using Microsoft.EntityFrameworkCore;
using VacationPlanner.Web.Data;
using VacationPlanner.Web.Models.Domain;

public class CityService : ICityService
{

    private readonly AppDbContext _context;

    public CityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<City?> GetCityByNameAsync(string cityName)
    {
        return await _context.Cities
            .Include(c => c.WeatherRecords)
            .Include(c => c.Attractions)
            .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower());
    }

    public async Task<City> SaveCityAsync(string cityname, double latitude, double longitude, string country)
    {
        var city = new City
        {
            Name = cityname,
            Latitude = latitude,
            Longitude = longitude,
            Country = country
        };
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();
        return city;
    }
}