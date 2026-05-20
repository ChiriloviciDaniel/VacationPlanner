using VacationPlanner.Web.Models.Domain;

public interface IWeatherService
{
    Task<List<WeatherRecord>> GetHistoricalWeatherAsync(string location, DateTime startDate, DateTime endDate);
    Task SaveWeatherRecordAsync(List<WeatherRecord> records);
    
}