using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WeatherApp.Services
{
    public class WeatherService
    {
        private readonly string _apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? "";

        public async Task<WeatherInfo?> GetWeatherAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return null;
            }

            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = JObject.Parse(await response.Content.ReadAsStringAsync());

                        var description = data["weather"]?[0]?["description"]?.ToString();
                        var icon = data["weather"]?[0]?["icon"]?.ToString();
                        var temperature = data["main"]?["temp"]?.Value<double>() ?? 0.0;

                        return new WeatherInfo
                        {
                            City = data["name"]?.ToString() ?? city,
                            Description = description ?? "No description available",
                            Icon = icon ?? "unknown",
                            Temperature = Math.Round(temperature)
                        };
                    }
                    else
                    {
                        throw new HttpRequestException($"API Error: {response.ReasonPhrase}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching weather data: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }
    }

    public class WeatherInfo
    {
        public string City { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public double Temperature { get; set; }
    }
}
