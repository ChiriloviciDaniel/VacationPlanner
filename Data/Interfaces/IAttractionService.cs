using VacationPlanner.Web.Models.Domain;

public interface IAttractionService
{
    Task<List<Attraction>> GetAttractionsByLocationAsync(string location);
    Task SaveAttractionAsync(List<Attraction> attractions);
}