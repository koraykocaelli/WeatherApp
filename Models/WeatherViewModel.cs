namespace WeatherApp.Models
{
    public class WeatherViewModel
    {
        public List<WeatherInfo> WeatherInfoList { get; set; } = new();
    }

    public class WeatherInfo
    {
        public string City { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 
        public string Icon { get; set; } = string.Empty; 

         public double Temperature { get; set; } = 0.0;
    }
}
