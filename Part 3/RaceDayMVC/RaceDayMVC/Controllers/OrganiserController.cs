using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;
        private readonly ILogger<OrganiserController> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OrganiserController(ApiClient api, ILogger<OrganiserController> logger)
        {
            _api = api;
            _logger = logger;
        }

        private bool IsOrganiser()
            => string.Equals(HttpContext.Session.GetString("IsOrganiser"), "True", StringComparison.OrdinalIgnoreCase);

        private T? DeserializeSafe<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed for {Type}. Payload: {Payload}", typeof(T).Name, json);
                return default;
            }
        }

        // GET: /Organiser
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("api/organisers/user/events");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load organiser events.";
                return View(new List<OrganiserDashboardViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var events = DeserializeSafe<List<EventViewModel>>(json) ?? new List<EventViewModel>();

            var dashboard = new List<OrganiserDashboardViewModel>();
            foreach (var ev in events)
            {
                int totalEnrolments = 0;
                var enrolResponse = await _api.GetAsync($"api/organisers/user/events/{ev.EventID}/enrolments");
                if (enrolResponse.IsSuccessStatusCode)
                {
                    var enrolJson = await enrolResponse.Content.ReadAsStringAsync();
                    var enrolments = DeserializeSafe<List<EnrolmentViewModel>>(enrolJson);
                    totalEnrolments = enrolments?.Count(e => e.Status != "withdrawn") ?? 0;
                }

                dashboard.Add(new OrganiserDashboardViewModel
                {
                    Event = ev,
                    TotalEnrolments = totalEnrolments
                });
            }

            return View(dashboard);
        }

        // GET: /Organiser/CreateEvent
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            return View(new CreateEventViewModel
            {
                EventDate = DateTime.Today.AddDays(7),
                StartTime = new TimeSpan(6, 0, 0),
                EventType = "run",
                Status = "published"
            });
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
                var err = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"Failed to create event ({(int)response.StatusCode}): {err}";
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();
            var createdEvent = DeserializeSafe<EventViewModel>(json);

            if (createdEvent != null && model.BannerImage != null && model.BannerImage.Length > 0)
            {
                await UploadBanner(createdEvent.EventID, model.BannerImage);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Organiser/EditEvent/5
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"api/events/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Event could not be loaded for editing.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var eventDetail = DeserializeSafe<EventDetailViewModel>(json);
            if (eventDetail == null)
            {
                TempData["Error"] = "Invalid event data received from API.";
                return RedirectToAction("Index");
            }

            return View(eventDetail);
        }

        // POST: /Organiser/EditEvent/5
        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, EventDetailViewModel model, IFormFile? bannerImage)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            model.EventID = id;

            var response = await _api.PutAsync($"api/events/{id}", new
            {
                name = model.Name,
                description = model.Description,
                eventDate = model.EventDate,
                startTime = model.StartTime,
                location = model.Location,
                eventType = model.EventType,
                status = model.Status
            });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"Failed to update event ({(int)response.StatusCode}): {err}";
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

            var response = await _api.DeleteAsync($"api/events/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to delete event.";
            }
            else
            {
                TempData["Success"] = "Event deleted.";
            }
            return RedirectToAction("Index");
        }

        // GET: /Organiser/Categories?eventId=5 (or /Organiser/Categories/5)
        public async Task<IActionResult> Categories(int eventId, int? id = null)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            int targetEventId = eventId > 0 ? eventId : (id ?? 0);
            var response = await _api.GetAsync($"api/events/{targetEventId}/categories");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load categories for this event.";
                ViewBag.EventId = targetEventId;
                return View(new List<CategoryViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var categories = DeserializeSafe<List<CategoryViewModel>>(json) ?? new List<CategoryViewModel>();

            ViewBag.EventId = targetEventId;
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
                description = model.Description,
                distanceKm = model.DistanceKm,
                entryFee = model.EntryFee,
                maxParticipants = model.MaxParticipants,
                startTime = model.StartTime
            });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to add category: {err}";
            }
            else
            {
                TempData["Success"] = "Category added!";
            }

            return RedirectToAction("Categories", new { eventId });
        }

        // POST: /Organiser/DeleteCategory
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int categoryId, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.DeleteAsync($"api/categories/{categoryId}");
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Cannot delete category: {err}";
            }
            else
            {
                TempData["Success"] = "Category deleted.";
            }

            return RedirectToAction("Categories", new { eventId });
        }

        // GET: /Organiser/Enrolments?eventId=5 (or /Organiser/Enrolments/5)
        public async Task<IActionResult> Enrolments(int eventId, int? id = null)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            int targetEventId = eventId > 0 ? eventId : (id ?? 0);
            var response = await _api.GetAsync($"api/organisers/user/events/{targetEventId}/enrolments");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load enrolments for this event.";
                ViewBag.EventId = targetEventId;
                return View(new List<EnrolmentViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var enrolments = DeserializeSafe<List<EnrolmentViewModel>>(json) ?? new List<EnrolmentViewModel>();

            ViewBag.EventId = targetEventId;
            return View(enrolments);
        }

        // GET: /Organiser/CaptureResults?eventId=5 (or /Organiser/CaptureResults/5)
        public async Task<IActionResult> CaptureResults(int eventId, int? id = null)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            int targetEventId = eventId > 0 ? eventId : (id ?? 0);
            var response = await _api.GetAsync($"api/organisers/user/events/{targetEventId}/enrolments");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load participants for result capture.";
                ViewBag.EventId = targetEventId;
                return View(new List<EnrolmentViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var enrolments = DeserializeSafe<List<EnrolmentViewModel>>(json) ?? new List<EnrolmentViewModel>();

            ViewBag.EventId = targetEventId;
            return View(enrolments);
        }

        // POST: /Organiser/CaptureResult
        [HttpPost]
        public async Task<IActionResult> CaptureResult(int enrolmentId, int? finishTimeSeconds, int? position, string status, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.PostAsync($"api/enrolments/{enrolmentId}/results", new
            {
                finishTimeSeconds = finishTimeSeconds > 0 ? finishTimeSeconds : null,
                position = position > 0 ? position : null,
                status = string.IsNullOrWhiteSpace(status) ? "finished" : status
            });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to capture result: {err}";
            }
            else
            {
                TempData["Success"] = "Result captured!";
            }

            return RedirectToAction("CaptureResults", new { eventId });
        }

        // Phase 6: Banner Upload
        private async Task UploadBanner(int eventId, IFormFile file)
        {
            try
            {
                var response = await _api.PostMultipartAsync($"api/events/{eventId}/banner", file, "file");
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Banner upload failed for event {EventId}: {Error}", eventId, err);
                    TempData["Error"] = "Event was saved, but the banner upload failed.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception uploading banner for event {EventId}", eventId);
                TempData["Error"] = "Event was saved, but an error occurred uploading the banner.";
            }
        }
    }
}
