namespace WeatherApp.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
