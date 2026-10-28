using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Services;

namespace RaceDayManagementAPI.Controllers
{
    /// <summary>
    /// Events controller for retrieving and querying race events in the system.
    /// Provides endpoints for filtering events by status, type, and date.
    /// </summary>
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly RaceDayDbContext _db;
        private readonly IBlobService _blobService;

        public EventsController(JwtService jwtService, RaceDayDbContext db, IBlobService blobService)
        {
            _jwtService = jwtService;
            _db = db;
            _blobService = blobService;
        }

        // 1. LIST ALL EVENTS  →  GET /api/events
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetEvents(
            [FromQuery] string? status = null,
            [FromQuery] string? type = null,
            [FromQuery] DateTime? fromDate = null)
        {
            var query = _db.Events.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status != null && e.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrEmpty(type))
                query = query.Where(e => e.EventType != null && e.EventType.ToLower() == type.ToLower());

            if (fromDate.HasValue)
                query = query.Where(e => e.EventDate >= fromDate.Value);

            var eventsList = query.ToList();
            var organisers = _db.Organisers.ToList();
            var users = _db.Users.ToList();
            var categories = _db.Categories.ToList();

            var events = eventsList.Select(e =>
            {
                var organiser = organisers.FirstOrDefault(o => o.OrganiserID == e.OrganiserID);
                var user = organiser != null ? users.FirstOrDefault(u => u.UserID == organiser.UserID) : null;

                return new
                {
                    e.EventID,
                    e.Name,
                    EventName = e.Name,
                    e.Description,
                    e.EventDate,
                    e.StartTime,
                    e.Location,
                    e.EventType,
                    e.Status,
                    e.BannerUrl,
                    e.OrganiserID,
                    Organiser = user?.FullName,
                    OrganiserName = user?.FullName,
                    OrganiserCompany = organiser?.CompanyName,
                    Categories = categories
                        .Where(c => c.EventID == e.EventID)
                        .Select(c => new
                        {
                            c.CategoryID,
                            c.EventID,
                            c.Name,
                            CategoryName = c.Name,
                            c.Description,
                            c.DistanceKm,
                            c.EntryFee,
                            c.MaxParticipants,
                            c.StartTime
                        })
                        .ToList()
                };
            }).ToList();

            return Ok(events);
        }

        // 2. GET EVENT BY ID  →  GET /api/events/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetEvent(int id)
        {
            var eventEntity = _db.Events.FirstOrDefault(e => e.EventID == id);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found." });

            var organiser = _db.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID);
            var user = organiser != null ? _db.Users.FirstOrDefault(u => u.UserID == organiser.UserID) : null;

            var eventCategories = _db.Categories.Where(c => c.EventID == id).ToList();
            var routes = _db.Routes.ToList();

            var categories = eventCategories
                .Select(c =>
                {
                    var route = routes.FirstOrDefault(r => r.CategoryID == c.CategoryID);
                    return new
                    {
                        c.CategoryID,
                        c.EventID,
                        c.Name,
                        CategoryName = c.Name,
                        c.Description,
                        c.DistanceKm,
                        c.EntryFee,
                        c.MaxParticipants,
                        c.StartTime,
                        Route = route == null ? null : new
                        {
                            route.RouteID,
                            route.CategoryID,
                            route.MapUrl,
                            route.ElevationGain,
                            route.Description
                        }
                    };
                })
                .ToList();

            return Ok(new
            {
                eventEntity.EventID,
                eventEntity.Name,
                EventName = eventEntity.Name,
                eventEntity.Description,
                eventEntity.EventDate,
                eventEntity.StartTime,
                eventEntity.Location,
                eventEntity.EventType,
                eventEntity.Status,
                eventEntity.BannerUrl,
                eventEntity.OrganiserID,
                Organiser = user?.FullName,
                OrganiserName = user?.FullName,
                OrganiserCompany = organiser?.CompanyName,
                Categories = categories
            });
        }

        // 3. CREATE EVENT  →  POST /api/events
        [HttpPost]
        [Authorize]
        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = _db.Organisers.FirstOrDefault(o => o.UserID == userId);

            if (organiser == null)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only organisers can create events." });

            if (request == null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Location))
                return BadRequest(new { message = "Name and location are required." });

            var newEvent = new Event
            {
                OrganiserID = organiser.OrganiserID,
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                StartTime = request.StartTime,
                Location = request.Location,
                EventType = string.IsNullOrWhiteSpace(request.EventType) ? "run" : request.EventType,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "published" : request.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Events.Add(newEvent);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventID }, newEvent);
        }

        // 4. UPLOAD EVENT BANNER  →  POST /api/events/{id}/banner
        [HttpPost("{id}/banner")]
        [Authorize]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            var eventEntity = await _db.Events.FindAsync(id);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = await _db.Organisers
                .FirstOrDefaultAsync(o => o.OrganiserID == eventEntity.OrganiserID);

            if (organiser?.UserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the event owner can upload a banner." });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var fileName = $"event-{id}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var url = await _blobService.UploadAsync(file, fileName);

            // If local fallback URL was returned, prefix with current request base URL so MVC can render it directly
            if (url.StartsWith("/") && HttpContext?.Request != null && !string.IsNullOrEmpty(HttpContext.Request.Host.Value))
            {
                url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{url}";
            }

            eventEntity.BannerUrl = url;
            eventEntity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { bannerUrl = url });
        }

        // 5. UPDATE EVENT  →  PUT /api/events/{id}
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult UpdateEvent(int id, [FromBody] UpdateEventRequest request)
        {
            var eventToUpdate = _db.Events.FirstOrDefault(e => e.EventID == id);
            if (eventToUpdate == null)
                return NotFound(new { message = "Event not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = _db.Organisers.FirstOrDefault(o => o.OrganiserID == eventToUpdate.OrganiserID);
            if (organiser?.UserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the event owner can update this event." });

            if (!string.IsNullOrEmpty(request.Name))
                eventToUpdate.Name = request.Name;
            if (request.Description != null)
                eventToUpdate.Description = request.Description;
            if (request.EventDate.HasValue)
                eventToUpdate.EventDate = request.EventDate.Value;
            if (request.StartTime.HasValue)
                eventToUpdate.StartTime = request.StartTime;
            if (!string.IsNullOrEmpty(request.Location))
                eventToUpdate.Location = request.Location;
            if (!string.IsNullOrEmpty(request.EventType))
                eventToUpdate.EventType = request.EventType;
            if (!string.IsNullOrEmpty(request.Status))
                eventToUpdate.Status = request.Status;

            eventToUpdate.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            return Ok(eventToUpdate);
        }

        // 6. DELETE EVENT  →  DELETE /api/events/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteEvent(int id)
        {
            var eventToDelete = _db.Events.FirstOrDefault(e => e.EventID == id);
            if (eventToDelete == null)
                return NotFound(new { message = "Event not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = _db.Organisers.FirstOrDefault(o => o.OrganiserID == eventToDelete.OrganiserID);
            if (organiser?.UserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the event owner can delete this event." });

            var categoriesToDelete = _db.Categories.Where(c => c.EventID == id).ToList();
            foreach (var category in categoriesToDelete)
            {
                var routesToDelete = _db.Routes.Where(r => r.CategoryID == category.CategoryID).ToList();
                _db.Routes.RemoveRange(routesToDelete);

                var enrolmentsToDelete = _db.Enrolments.Where(e => e.CategoryID == category.CategoryID).ToList();
                foreach (var enrolment in enrolmentsToDelete)
                {
                    var resultsToDelete = _db.Results.Where(r => r.EnrolmentID == enrolment.EnrolmentID).ToList();
                    _db.Results.RemoveRange(resultsToDelete);
                    _db.Enrolments.Remove(enrolment);
                }
                _db.Categories.Remove(category);
            }

            _db.Events.Remove(eventToDelete);
            _db.SaveChanges();

            return NoContent();
        }

        // 7. LIST CATEGORIES FOR EVENT  →  GET /api/events/{eventId}/categories
        [HttpGet("{eventId}/categories")]
        [AllowAnonymous]
        public IActionResult GetCategories(int eventId)
        {
            var eventExists = _db.Events.Any(e => e.EventID == eventId);
            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var eventCategories = _db.Categories.Where(c => c.EventID == eventId).ToList();
            var routes = _db.Routes.ToList();

            var categories = eventCategories
                .Select(c =>
                {
                    var route = routes.FirstOrDefault(r => r.CategoryID == c.CategoryID);
                    return new
                    {
                        c.CategoryID,
                        c.EventID,
                        c.Name,
                        CategoryName = c.Name,
                        c.Description,
                        c.DistanceKm,
                        c.EntryFee,
                        c.MaxParticipants,
                        c.StartTime,
                        Route = route == null ? null : new
                        {
                            route.RouteID,
                            route.CategoryID,
                            route.MapUrl,
                            route.ElevationGain,
                            route.Description
                        }
                    };
                })
                .ToList();

            return Ok(categories);
        }

        // 8. CREATE CATEGORY  →  POST /api/events/{eventId}/categories
        [HttpPost("{eventId}/categories")]
        [Authorize]
        public IActionResult CreateCategory(int eventId, [FromBody] CreateCategoryRequest request)
        {
            var eventEntity = _db.Events.FirstOrDefault(e => e.EventID == eventId);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = _db.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID);
            if (organiser?.UserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the event owner can add categories." });

            if (request == null || string.IsNullOrWhiteSpace(request.Name) || request.DistanceKm <= 0)
                return BadRequest(new { message = "Name and distance are required." });

            var existingCategory = _db.Categories.FirstOrDefault(c => c.EventID == eventId && c.Name == request.Name);
            if (existingCategory != null)
                return Conflict(new { message = "A category with this name already exists for this event." });

            var category = new Category
            {
                EventID = eventId,
                Name = request.Name,
                Description = request.Description,
                DistanceKm = request.DistanceKm,
                EntryFee = request.EntryFee ?? 0,
                MaxParticipants = request.MaxParticipants,
                StartTime = request.StartTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            _db.SaveChanges();

            if (request.Route != null)
            {
                var route = new RaceDayManagementAPI.Models.Route
                {
                    CategoryID = category.CategoryID,
                    MapUrl = request.Route.MapUrl,
                    ElevationGain = request.Route.ElevationGain,
                    Description = request.Route.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Routes.Add(route);
                _db.SaveChanges();
            }

            return CreatedAtAction(nameof(GetCategories), new { eventId }, category);
        }
    }

    // Request Models
    public class CreateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = "run";
        public string? Status { get; set; }
    }

    public class UpdateEventRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? EventDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string? Location { get; set; }
        public string? EventType { get; set; }
        public string? Status { get; set; }
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DistanceKm { get; set; }
        public decimal? EntryFee { get; set; }
        public int? MaxParticipants { get; set; }
        public TimeSpan? StartTime { get; set; }
        public RouteRequest? Route { get; set; }
    }

    public class RouteRequest
    {
        public string? MapUrl { get; set; }
        public int? ElevationGain { get; set; }
        public string? Description { get; set; }
    }
}