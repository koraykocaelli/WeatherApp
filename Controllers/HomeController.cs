using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;
using WeatherApp.Services;
using WeatherInfoModel = WeatherApp.Models.WeatherInfo;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly WeatherService _weatherService;
        private readonly AppDbContext _context;

        public HomeController(WeatherService weatherService, AppDbContext context)
        {
            _weatherService = weatherService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cities = new List<string>
            {
                "New York", "London", "Paris", "Tokyo", "Dubai",
                "Los Angeles", "Singapore", "Hong Kong", "Bangkok", "Istanbul",
                "Seoul", "Barcelona", "Moscow", "Berlin", "Sydney",
                "Rome", "Shanghai", "San Francisco", "Beijing", "Madrid",
                "Toronto", "Chicago", "Mexico City", "São Paulo", "Mumbai",
                "Vienna", "Amsterdam", "Rio de Janeiro", "Kuala Lumpur", "Cairo"
            };

            var weatherInfoList = await GetWeatherInfoListAsync(cities);
            ViewBag.WeatherInfoList = weatherInfoList;

            return View();
        }

        [HttpPost]
        public JsonResult AddToFavorites([FromBody] Favorite favorite)
        {
            if (string.IsNullOrWhiteSpace(favorite.CityName))
            {
                return Json(new { success = false, message = "City name cannot be empty." });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = _context.Users.Include(u => u.Favorites).FirstOrDefault(u => u.Id == userId.Value);
                if (user != null)
                {
                    if (!user.Favorites.Any(f => f.CityName == favorite.CityName))
                    {
                        user.Favorites.Add(new Favorite
                        {
                            CityName = favorite.CityName,
                            UserId = user.Id
                        });
                        _context.SaveChanges();
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "City is already in favorites." });
                    }
                }
                return Json(new { success = false, message = "User not found." });
            }

            return Json(new { success = false, redirectUrl = Url.Action("Login", "Users") });
        }

        [HttpPost]
        public async Task<IActionResult> Search(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                ViewBag.Error = "Please enter a valid city name.";
                ViewBag.WeatherInfoList = await GetWeatherInfoListAsync(new List<string>());
                return View("Index");
            }

            var weather = await _weatherService.GetWeatherAsync(city);

            if (weather != null)
            {
                ViewBag.SearchResult = new WeatherInfoModel
                {
                    City = weather.City,
                    Description = weather.Description,
                    Icon = weather.Icon,
                    Temperature = Math.Round(weather.Temperature)
                };
            }
            else
            {
                ViewBag.Error = $"Weather information for {city} could not be found.";
            }

            var cities = new List<string> { city };
            ViewBag.WeatherInfoList = await GetWeatherInfoListAsync(cities);
            return View("Index");
        }

        private async Task<List<WeatherInfoModel>> GetWeatherInfoListAsync(List<string> cities)
        {
            var weatherInfoList = new List<WeatherInfoModel>();

            foreach (var city in cities)
            {
                var weather = await _weatherService.GetWeatherAsync(city);
                if (weather != null)
                {
                    weatherInfoList.Add(new WeatherInfoModel
                    {
                        City = weather.City,
                        Description = weather.Description,
                        Icon = weather.Icon,
                        Temperature = Math.Round(weather.Temperature)
                    });
                }
            }

            return weatherInfoList;
        }
    }

    public class FavoriteRequest
    {
        public string? CityName { get; internal set; }
    }
}
