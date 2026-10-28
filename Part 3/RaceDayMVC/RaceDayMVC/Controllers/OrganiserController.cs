using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserController(ApiClient api) => _api = api;

        private bool IsOrganiser()
            => HttpContext.Session.GetString("IsOrganiser") == "True";

        // GET: /Organiser
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("api/organisers/user/events");
            var json = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<EventViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch enrolment counts for each event
            var dashboard = new List<OrganiserDashboardViewModel>();
            foreach (var ev in events!)
            {
                var enrolResponse = await _api.GetAsync($"api/organisers/user/events/{ev.EventID}/enrolments");
                var enrolJson = await enrolResponse.Content.ReadAsStringAsync();
                var enrolments = JsonSerializer.Deserialize<List<EnrolmentViewModel>>(enrolJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                dashboard.Add(new OrganiserDashboardViewModel
                {
                    Event = ev,
                    TotalEnrolments = enrolments?.Count ?? 0
                });
            }

            return View(dashboard);
        }

        // GET: /Organiser/CreateEvent
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: /Organiser/CreateEvent
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.PostAsync("api/events", new
            {
                name = model.Name,
                description = model.Description,
                eventDate = model.EventDate,
                startTime = model.StartTime,
                location = model.Location,
                eventType = model.EventType,
                status = model.Status ?? "draft"
            });

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to create event.";
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();
            var createdEvent = JsonSerializer.Deserialize<EventViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Upload banner if provided (Phase 6)
            if (model.BannerImage != null && model.BannerImage.Length > 0)
            {
                await UploadBanner(createdEvent!.EventID, model.BannerImage);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Organiser/EditEvent/5
        public async Task<IActionResult> EditEvent(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"api/events/{id}");
            var json = await response.Content.ReadAsStringAsync();
            var eventDetail = JsonSerializer.Deserialize<EventDetailViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(eventDetail);
        }

        // POST: /Organiser/EditEvent/5
        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, EventDetailViewModel model, IFormFile? bannerImage)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.PutAsync($"api/events/{id}", new
            {
                name = model.Name,
                description = model.Description,
                eventDate = model.EventDate,
                location = model.Location,
                eventType = model.EventType,
                status = model.Status
            });

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to update event.";
                return View(model);
            }

            if (bannerImage != null && bannerImage.Length > 0)
            {
                await UploadBanner(id, bannerImage);
            }

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction("Index");
        }

        // POST: /Organiser/DeleteEvent/5
        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            await _api.DeleteAsync($"api/events/{id}");
            TempData["Success"] = "Event deleted.";
            return RedirectToAction("Index");
        }

        // GET: /Organiser/Categories/5
        public async Task<IActionResult> Categories(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"api/events/{eventId}/categories");
            var json = await response.Content.ReadAsStringAsync();
            var categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.EventId = eventId;
            return View(categories);
        }

        // POST: /Organiser/AddCategory
        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, CategoryViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.PostAsync($"api/events/{eventId}/categories", new
            {
                name = model.Name,
                distanceKm = model.DistanceKm,
                entryFee = model.EntryFee,
                maxParticipants = model.MaxParticipants
            });

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to add category.";
            }
            else
            {
                TempData["Success"] = "Category added!";
            }

            return RedirectToAction("Categories", new { eventId });
        }

        // GET: /Organiser/Enrolments/5
        public async Task<IActionResult> Enrolments(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"api/organisers/user/events/{eventId}/enrolments");
            var json = await response.Content.ReadAsStringAsync();
            var enrolments = JsonSerializer.Deserialize<List<EnrolmentViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.EventId = eventId;
            return View(enrolments);
        }

        // GET: /Organiser/CaptureResults/5
        public async Task<IActionResult> CaptureResults(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"api/organisers/user/events/{eventId}/enrolments");
            var json = await response.Content.ReadAsStringAsync();
            var enrolments = JsonSerializer.Deserialize<List<EnrolmentViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.EventId = eventId;
            return View(enrolments);
        }

        // POST: /Organiser/CaptureResult
        [HttpPost]
        public async Task<IActionResult> CaptureResult(int enrolmentId, int finishTimeSeconds, int position, string status, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.PostAsync($"api/enrolments/{enrolmentId}/results", new
            {
                finishTimeSeconds,
                position,
                status
            });

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to capture result.";
            }
            else
            {
                TempData["Success"] = "Result captured!";
            }

            return RedirectToAction("CaptureResults", new { eventId });
        }

        // Phase 6
        private async Task UploadBanner(int eventId, IFormFile file)
        {
            var response = await _api.PostMultipartAsync($"api/events/{eventId}/banner", file, "file");

            if (!response.IsSuccessStatusCode)
            {
                // log or set TempData
                TempData["Error"] = "Banner upload failed.";
            }
        }
    }
}