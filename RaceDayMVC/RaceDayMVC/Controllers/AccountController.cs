using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;
        private readonly ILogger<AccountController> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AccountController(ApiClient api, ILogger<AccountController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            var isLoggedIn = HttpContext.Session.GetString("JwtToken") != null;

            if (isLoggedIn)
            {
                if (string.Equals(HttpContext.Session.GetString("IsOrganiser"), "True", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Organiser");

                if (string.Equals(HttpContext.Session.GetString("IsParticipant"), "True", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Participant");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("Login attempt for: {Email}", model.Email);

            var response = await _api.PostAsync("api/auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login failed for {Email}: {StatusCode}", model.Email, response.StatusCode);
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();
            AuthResponse? auth = null;
            try
            {
                auth = JsonSerializer.Deserialize<AuthResponse>(json, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize login response: {Json}", json);
            }

            if (auth == null || string.IsNullOrEmpty(auth.Token))
            {
                ViewBag.Error = "Login failed: Invalid response from server.";
                return View(model);
            }

            HttpContext.Session.SetString("JwtToken", auth.Token);
            HttpContext.Session.SetInt32("UserID", auth.UserID);
            HttpContext.Session.SetString("FullName", auth.FullName);
            HttpContext.Session.SetString("Email", auth.Email);
            HttpContext.Session.SetString("IsOrganiser", auth.IsOrganiser.ToString());
            HttpContext.Session.SetString("IsParticipant", auth.IsParticipant.ToString());

            if (auth.IsOrganiser)
                return RedirectToAction("Index", "Organiser");

            if (auth.IsParticipant)
                return RedirectToAction("Index", "Participant");

            ViewBag.Error = "Your account does not have an assigned role.";
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var response = await _api.PostAsync("api/auth/register", new
            {
                email = model.Email,
                password = model.Password,
                fullName = model.FullName,
                phone = model.Phone,
                dateOfBirth = model.DateOfBirth,
                idNumber = model.IDNumber,
                emergencyContact = model.EmergencyContact
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Registration failed: " + error;
                return View(model);
            }

            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}