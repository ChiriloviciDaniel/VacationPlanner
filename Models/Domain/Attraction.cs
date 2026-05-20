namespace VacationPlanner.Web.Models.Domain
{
    public class Attraction
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? ImageUrl { get; set; } 
        public double? Rating { get; set; }
        public string? Kinds { get; set; }

        // Navigation property
        public City City { get; set; } = null!;

    }
}