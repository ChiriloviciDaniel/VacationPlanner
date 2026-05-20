namespace VacationPlanner.Web.Models.Domain
{
    public class WeatherRecord
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public DateTime Date { get; set; }
        public double MinTemp { get; set; }
        public double MaxTemp { get; set; }
        public double AvgTemp { get; set; }
        public string Conditions { get; set; } = string.Empty;

        // Navigation property
        public City City { get; set; } = null!;
    }
}