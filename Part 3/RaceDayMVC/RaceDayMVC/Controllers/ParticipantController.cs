using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiClient _api;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ParticipantController(ApiClient api) => _api = api;

        private bool IsParticipant()
            => string.Equals(HttpContext.Session.GetString("IsParticipant"), "True", StringComparison.OrdinalIgnoreCase);

        // GET: /Participant
        public async Task<IActionResult> Index()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("api/events");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load events at this time.";
                return View(new List<EventViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var events = string.IsNullOrWhiteSpace(json)
                ? new List<EventViewModel>()
                : JsonSerializer.Deserialize<List<EventViewModel>>(json, _jsonOptions) ?? new List<EventViewModel>();

            return View(events);
        }

        // GET: /Participant/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"api/events/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var eventDetail = JsonSerializer.Deserialize<EventDetailViewModel>(json, _jsonOptions);
            if (eventDetail == null)
            {
                TempData["Error"] = "Unable to load event details.";
                return RedirectToAction(nameof(Index));
            }

            return View(eventDetail);
        }

        // POST: /Participant/Enrol
        [HttpPost]
        public async Task<IActionResult> Enrol(int categoryId, int eventId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.PostAsync("api/enrolments", new { categoryID = categoryId });

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["Error"] = !string.IsNullOrWhiteSpace(errorBody)
                    ? $"Enrolment failed: {errorBody}"
                    : "Enrolment failed. You may already be enrolled.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            TempData["Success"] = "Successfully enrolled!";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        // GET: /Participant/MyEnrolments
        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("api/participants/user/enrolments");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load your enrolments.";
                return View(new List<EnrolmentViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var enrolments = string.IsNullOrWhiteSpace(json)
                ? new List<EnrolmentViewModel>()
                : JsonSerializer.Deserialize<List<EnrolmentViewModel>>(json, _jsonOptions) ?? new List<EnrolmentViewModel>();

            return View(enrolments);
        }

        // GET: /Participant/MyResults
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("api/participants/user/results");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load your results.";
                return View(new List<ResultViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var results = string.IsNullOrWhiteSpace(json)
                ? new List<ResultViewModel>()
                : JsonSerializer.Deserialize<List<ResultViewModel>>(json, _jsonOptions) ?? new List<ResultViewModel>();

            return View(results);
        }

        // GET: /Participant/Profile
        public async Task<IActionResult> Profile()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("api/participants/user");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load your profile.";
                return View(new ParticipantProfileViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();
            var profile = string.IsNullOrWhiteSpace(json)
                ? new ParticipantProfileViewModel()
                : JsonSerializer.Deserialize<ParticipantProfileViewModel>(json, _jsonOptions) ?? new ParticipantProfileViewModel();

            return View(profile);
        }

        // POST: /Participant/Profile
        [HttpPost]
        public async Task<IActionResult> Profile(ParticipantProfileViewModel model, IFormFile? profilePicture)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            // Update profile via API
            var response = await _api.PutAsync("api/participants/user", new
            {
                fullName = model.FullName,
                phone = model.Phone,
                email = model.Email,
                dateOfBirth = model.DateOfBirth,
                idNumber = model.IDNumber,
                emergencyContact = model.EmergencyContact
            });

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Profile update failed.";
                return View(model);
            }

            // Profile picture upload – calls API endpoint (Phase 6)
            if (profilePicture != null && profilePicture.Length > 0)
            {
                // To be implemented in Phase 6
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
    }
}