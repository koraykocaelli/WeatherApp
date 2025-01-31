using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;
using WeatherApp.Services;
using WeatherInfoModel = WeatherApp.Models.WeatherInfo;

namespace WeatherApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly WeatherService _weatherService;

        public UsersController(AppDbContext context, WeatherService weatherService)
        {
            _context = context;
            _weatherService = weatherService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                TempData["ErrorMessage"] = "This email is already registered.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(user.Name) || user.Name.Length < 3 ||
                string.IsNullOrWhiteSpace(user.Surname) || user.Surname.Length < 3 ||
                string.IsNullOrWhiteSpace(user.UserName) || user.UserName.Length < 3 ||
                string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 6)
            {
                TempData["ErrorMessage"] = "Validation failed. Ensure all fields are correctly filled.";
                return View();
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "Email and password cannot be empty.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid credentials. Please try again.";
                return View();
            }

            // Set session and cookie
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Ensure cookies are sent over HTTPS
                Expires = DateTime.UtcNow.AddHours(8),
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("UserSession", token, cookieOptions);

            TempData["SuccessMessage"] = "Login successful! Welcome.";
            return RedirectToAction("Profile");
        }

        public IActionResult Logout()
        {
            // Clear session and cookies
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UserSession");
            Response.Cookies.Delete(".AspNetCore.Session"); 

            TempData["SuccessMessage"] = "You have been successfully logged out.";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "You need to log in to access your profile.";
                return RedirectToAction("Login");
            }

            var user = await _context.Users.Include(u => u.Favorites).FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login");
            }

            var weatherInfoList = new List<WeatherInfoModel>();
            foreach (var favorite in user.Favorites)
            {
                var weather = await _weatherService.GetWeatherAsync(favorite.CityName);
                if (weather != null)
                {
                    weatherInfoList.Add(new WeatherInfoModel
                    {
                        City = weather.City,
                        Description = weather.Description,
                        Icon = weather.Icon,
                        Temperature = weather.Temperature
                    });
                }
            }

            ViewBag.WeatherInfoList = weatherInfoList;

            return View(user);
        }

        [HttpPost]
        public JsonResult AddToFavorites([FromBody] Favorite favorite)
        {
            if (string.IsNullOrWhiteSpace(favorite.CityName))
            {
                return Json(new { success = false, message = "City name cannot be empty." });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, redirectUrl = Url.Action("Login", "Users") });
            }

            var user = _context.Users.Include(u => u.Favorites).FirstOrDefault(u => u.Id == userId.Value);
            if (user != null)
            {
                if (!user.Favorites.Any(f => f.CityName.Equals(favorite.CityName, StringComparison.OrdinalIgnoreCase)))
                {
                    user.Favorites.Add(new Favorite
                    {
                        CityName = favorite.CityName,
                        UserId = user.Id
                    });
                    _context.SaveChanges();
                    return Json(new { success = true, message = $"{favorite.CityName} added to favorites." });
                }
                else
                {
                    return Json(new { success = false, message = "City is already in favorites." });
                }
            }

            return Json(new { success = false, message = "User not found." });
        }

        [HttpPost]
        public JsonResult RemoveFavorite([FromBody] string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return Json(new { success = false, message = "City name cannot be empty." });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var user = _context.Users.Include(u => u.Favorites).FirstOrDefault(u => u.Id == userId.Value);
            if (user != null)
            {
                var favoriteToRemove = user.Favorites.FirstOrDefault(f => f.CityName.Equals(cityName, StringComparison.OrdinalIgnoreCase));
                if (favoriteToRemove != null)
                {
                    user.Favorites.Remove(favoriteToRemove);
                    _context.SaveChanges();
                    return Json(new { success = true, message = $"{cityName} removed from favorites." });
                }
                return Json(new { success = false, message = "City not found in favorites." });
            }

            return Json(new { success = false, message = "User not found." });
        }
    }
}
