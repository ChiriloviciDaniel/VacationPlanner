using VacationPlanner.Web.Models.Domain;

public interface ICityService
{
    Task<City?> GetCityByNameAsync(string cityName);
    Task<City> SaveCityAsync(string cityname, double latitude, double longitude, string country);
}