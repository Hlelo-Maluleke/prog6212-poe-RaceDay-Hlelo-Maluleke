using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;

        public AccountController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var response = await _api.PostAsync("api/auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();
            var auth = JsonSerializer.Deserialize<AuthResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Store in session
            HttpContext.Session.SetString("JwtToken", auth!.Token);
            HttpContext.Session.SetInt32("UserID", auth.UserID);
            HttpContext.Session.SetString("FullName", auth.FullName);
            HttpContext.Session.SetString("Email", auth.Email);
            HttpContext.Session.SetString("IsOrganiser", auth.IsOrganiser.ToString());
            HttpContext.Session.SetString("IsParticipant", auth.IsParticipant.ToString());

            // Redirect based on role
            if (auth.IsOrganiser) return RedirectToAction("Index", "Organiser");
            if (auth.IsParticipant) return RedirectToAction("Index", "Participant");

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
