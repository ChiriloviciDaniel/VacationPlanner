namespace VacationPlanner.Web.Models.Domain
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Navigation properties
        public ICollection<WeatherRecord> WeatherRecords { get; set; } = new List<WeatherRecord>();
        public ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
    }
}