using Microsoft.AspNetCore.Mvc;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly WeatherService _weatherService;

        public FavoritesController(AppDbContext context, WeatherService weatherService)
        {
            _context = context;
            _weatherService = weatherService;
        }

        public async Task<IActionResult> Index()
        {
            var favorites = _context.Favorites.ToList();
            var weatherInfoList = new List<WeatherApp.Models.WeatherInfo>(); 

            foreach (var favorite in favorites)
            {
                var weatherInfo = await _weatherService.GetWeatherAsync(favorite.CityName);

                if (weatherInfo != null)
                {
                    weatherInfoList.Add(new WeatherApp.Models.WeatherInfo
                    {
                        City = weatherInfo.City,
                        Description = weatherInfo.Description,
                        Icon = weatherInfo.Icon
                    });
                }
                else
                {
                    weatherInfoList.Add(new WeatherApp.Models.WeatherInfo
                    {
                        City = favorite.CityName,
                        Description = "Weather information not available.",
                        Icon = "unknown"
                    });
                }
            }

            ViewBag.WeatherInfoList = weatherInfoList;
            return View();
        }

        [HttpPost]
        public IActionResult Add(string cityName, int userId)
        {
            if (!string.IsNullOrWhiteSpace(cityName))
            {
                _context.Favorites.Add(new Favorite { CityName = cityName, UserId = userId });
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
