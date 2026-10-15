using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public EventsController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        // 1. LIST ALL EVENTS  →  GET /api/events
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetEvents(
            [FromQuery] string? status = null,
            [FromQuery] string? type = null,
            [FromQuery] DateTime? fromDate = null)
        {
            var query = RaceDayDB.Events.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(e => e.EventType == type);

            if (fromDate.HasValue)
                query = query.Where(e => e.EventDate >= fromDate.Value);

            var events = query.ToList().Select(e => 
            {
                var organiser = RaceDayDB.Organisers.FirstOrDefault(o => o.OrganiserID == e.OrganiserID);
                var user = organiser != null ? RaceDayDB.Users.FirstOrDefault(u => u.UserID == organiser.UserID) : null;

                return new
                {
                    e.EventID,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.StartTime,
                    e.Location,
                    e.EventType,
                    e.Status,
                    Organiser = user == null ? null : user.FullName,
                    Categories = RaceDayDB.Categories
                        .Where(c => c.EventID == e.EventID)
                        .Select(c => new { c.CategoryID, c.Name, c.DistanceKm, c.EntryFee })
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
            var eventEntity = RaceDayDB.Events.FirstOrDefault(e => e.EventID == id);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found." });

            var organiser = RaceDayDB.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID);
            var user = organiser != null ? RaceDayDB.Users.FirstOrDefault(u => u.UserID == organiser.UserID) : null;

            var categories = RaceDayDB.Categories
                .Where(c => c.EventID == id)
                .Select(c =>
                {
                    var route = RaceDayDB.Routes.FirstOrDefault(r => r.CategoryID == c.CategoryID);
                    return new
                    {
                        c.CategoryID,
                        c.Name,
                        c.Description,
                        c.DistanceKm,
                        c.EntryFee,
                        c.MaxParticipants,
                        c.StartTime,
                        Route = route == null ? null : new
                        {
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
                eventEntity.Description,
                eventEntity.EventDate,
                eventEntity.StartTime,
                eventEntity.Location,
                eventEntity.EventType,
                eventEntity.Status,
                Organiser = user?.FullName,
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
            var organiser = RaceDayDB.Organisers.FirstOrDefault(o => o.UserID == userId);

            if (organiser == null)
                return Forbid("Only organisers can create events.");

            if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Location))
                return BadRequest(new { message = "Name and location are required." });

            var newEvent = new Event
            {
                EventID = RaceDayDB.NextEventID++,
                OrganiserID = organiser.OrganiserID,
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                StartTime = request.StartTime,
                Location = request.Location,
                EventType = request.EventType ?? "run",
                Status = request.Status ?? "draft"
            };

            RaceDayDB.Events.Add(newEvent);

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventID }, newEvent);
        }

        // 4. UPDATE EVENT  →  PUT /api/events/{id}
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult UpdateEvent(int id, [FromBody] UpdateEventRequest request)
        {
            var eventToUpdate = RaceDayDB.Events.FirstOrDefault(e => e.EventID == id);
            if (eventToUpdate == null)
                return NotFound(new { message = "Event not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = RaceDayDB.Organisers.FirstOrDefault(o => o.OrganiserID == eventToUpdate.OrganiserID);
            if (organiser?.UserID != userId)
                return Forbid("Only the event owner can update this event.");

            if (!string.IsNullOrEmpty(request.Name))
                eventToUpdate.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
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

            eventToUpdate.UpdatedAt = DateTime.Now;
            return Ok(eventToUpdate);
        }

        // 5. DELETE EVENT  →  DELETE /api/events/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteEvent(int id)
        {
            var eventToDelete = RaceDayDB.Events.FirstOrDefault(e => e.EventID == id);
            if (eventToDelete == null)
                return NotFound(new { message = "Event not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = RaceDayDB.Organisers.FirstOrDefault(o => o.OrganiserID == eventToDelete.OrganiserID);
            if (organiser?.UserID != userId)
                return Forbid("Only the event owner can delete this event.");

            var categoriesToDelete = RaceDayDB.Categories.Where(c => c.EventID == id).ToList();
            foreach (var category in categoriesToDelete)
            {
                RaceDayDB.Routes.RemoveAll(r => r.CategoryID == category.CategoryID);

                var enrolmentsToDelete = RaceDayDB.Enrolments.Where(e => e.CategoryID == category.CategoryID).ToList();
                foreach (var enrolment in enrolmentsToDelete)
                {
                    RaceDayDB.Results.RemoveAll(r => r.EnrolmentID == enrolment.EnrolmentID);
                    RaceDayDB.Enrolments.Remove(enrolment);
                }
                RaceDayDB.Categories.Remove(category);
            }

            RaceDayDB.Events.Remove(eventToDelete);
            return NoContent();
        }

        // 6. LIST CATEGORIES FOR EVENT  →  GET /api/events/{eventId}/categories
        [HttpGet("{eventId}/categories")]
        [AllowAnonymous]
        public IActionResult GetCategories(int eventId)
        {
            var eventExists = RaceDayDB.Events.Any(e => e.EventID == eventId);
            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var categories = RaceDayDB.Categories
                .Where(c => c.EventID == eventId)
                .Select(c =>
                {
                    var route = RaceDayDB.Routes.FirstOrDefault(r => r.CategoryID == c.CategoryID);
                    return new
                    {
                        c.CategoryID,
                        c.Name,
                        c.Description,
                        c.DistanceKm,
                        c.EntryFee,
                        c.MaxParticipants,
                        c.StartTime,
                        Route = route == null ? null : new
                        {
                            route.MapUrl,
                            route.ElevationGain,
                            route.Description
                        }
                    };
                })
                .ToList();

            return Ok(categories);
        }

        // 7. CREATE CATEGORY  →  POST /api/events/{eventId}/categories
        [HttpPost("{eventId}/categories")]
        [Authorize]
        public IActionResult CreateCategory(int eventId, [FromBody] CreateCategoryRequest request)
        {
            var eventEntity = RaceDayDB.Events.FirstOrDefault(e => e.EventID == eventId);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = RaceDayDB.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID);
            if (organiser?.UserID != userId)
                return Forbid("Only the event owner can add categories.");

            if (string.IsNullOrEmpty(request.Name) || request.DistanceKm <= 0)
                return BadRequest(new { message = "Name and distance are required." });

            var existingCategory = RaceDayDB.Categories.FirstOrDefault(c => c.EventID == eventId && c.Name == request.Name);
            if (existingCategory != null)
                return Conflict(new { message = "A category with this name already exists for this event." });

            var category = new Category
            {
                CategoryID = RaceDayDB.NextCategoryID++,
                EventID = eventId,
                Name = request.Name,
                Description = request.Description,
                DistanceKm = request.DistanceKm,
                EntryFee = request.EntryFee ?? 0,
                MaxParticipants = request.MaxParticipants,
                StartTime = request.StartTime
            };
            RaceDayDB.Categories.Add(category);

            if (request.Route != null)
            {
                var route = new RaceDayManagementAPI.Models.Route
                {
                    RouteID = RaceDayDB.NextRouteID++,
                    CategoryID = category.CategoryID,
                    MapUrl = request.Route.MapUrl,
                    ElevationGain = request.Route.ElevationGain,
                    Description = request.Route.Description
                };
                RaceDayDB.Routes.Add(route);
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