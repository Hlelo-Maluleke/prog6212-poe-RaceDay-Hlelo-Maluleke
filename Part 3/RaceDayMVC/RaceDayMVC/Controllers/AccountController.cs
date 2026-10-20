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

        public AccountController(ApiClient api, ILogger<AccountController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation($"Login attempt for: {model.Email}");

            var response = await _api.PostAsync("api/auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Login failed for {model.Email}: {response.StatusCode}");
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Login response: {json}");

            var auth = JsonSerializer.Deserialize<AuthResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (auth == null)
            {
                _logger.LogError("Failed to deserialize auth response");
                ViewBag.Error = "Login failed: Invalid response from server.";
                return View(model);
            }

            _logger.LogInformation($"Login successful for {auth.Email}. UserID: {auth.UserID}, IsParticipant: {auth.IsParticipant}, IsOrganiser: {auth.IsOrganiser}");

            // Store in session
            HttpContext.Session.SetString("JwtToken", auth.Token);
            HttpContext.Session.SetInt32("UserID", auth.UserID);
            HttpContext.Session.SetString("FullName", auth.FullName);
            HttpContext.Session.SetString("Email", auth.Email);
            HttpContext.Session.SetString("IsOrganiser", auth.IsOrganiser.ToString());
            HttpContext.Session.SetString("IsParticipant", auth.IsParticipant.ToString());

            _logger.LogInformation($"Session data saved. IsParticipant stored as: {auth.IsParticipant.ToString()}");

            // Redirect based on role
            if (auth.IsOrganiser)
            {
                _logger.LogInformation($"Redirecting to Organiser dashboard");
                return RedirectToAction("Index", "Organiser");
            }
            if (auth.IsParticipant)
            {
                _logger.LogInformation($"Redirecting to Participant dashboard");
                return RedirectToAction("Index", "Participant");
            }

            _logger.LogWarning($"User {auth.Email} has no roles assigned");
            return RedirectToAction("Index", "Home");
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

            // Auto-login after registration
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
