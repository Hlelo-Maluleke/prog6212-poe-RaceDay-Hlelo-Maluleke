using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Services;

namespace RaceDayManagementAPI.Controllers
{
    /// <summary>
    /// Categories controller for retrieving event category information.
    /// Provides endpoints for querying race categories and their routes by ID or event.
    /// </summary>
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly RaceDayDbContext _db;

        public CategoriesController(JwtService jwtService, RaceDayDbContext db)
        {
            _jwtService = jwtService;
            _db = db;
        }

        // 1. GET CATEGORY BY ID  →  GET /api/categories/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetCategory(int id)
        {
            var category = _db.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            var route = _db.Routes.FirstOrDefault(r => r.CategoryID == id);

            return Ok(new
            {
                category.CategoryID,
                category.EventID,
                category.Name,
                category.Description,
                category.DistanceKm,
                category.EntryFee,
                category.MaxParticipants,
                category.StartTime,
                Route = route == null ? null : new
                {
                    route.RouteID,
                    route.CategoryID,
                    route.MapUrl,
                    route.ElevationGain,
                    route.Description
                }
            });
        }

        // 2. UPDATE CATEGORY  →  PUT /api/categories/{id}
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            var category = _db.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            var eventEntity = _db.Events.FirstOrDefault(e => e.EventID == category.EventID);
            var organiser = eventEntity != null ? _db.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID) : null;

            var userId = _jwtService.GetUserIdFromToken(User);
            if (organiser?.UserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the event owner can update this category." });

            if (!string.IsNullOrEmpty(request.Name))
                category.Name = request.Name;
            if (request.Description != null)
                category.Description = request.Description;
            if (request.DistanceKm.HasValue)
                category.DistanceKm = request.DistanceKm.Value;
            if (request.EntryFee.HasValue)
                category.EntryFee = request.EntryFee.Value;
            if (request.MaxParticipants.HasValue)
                category.MaxParticipants = request.MaxParticipants;
            if (request.StartTime.HasValue)
                category.StartTime = request.StartTime;

            category.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            return Ok(category);
        }

        // 3. DELETE CATEGORY  →  DELETE /api/categories/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteCategory(int id)
        {
            var category = _db.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            var eventEntity = _db.Events.FirstOrDefault(e => e.EventID == category.EventID);
            var organiser = eventEntity != null ? _db.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID) : null;

            var userId = _jwtService.GetUserIdFromToken(User);
            if (organiser?.UserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the event owner can delete this category." });

            var hasEnrolments = _db.Enrolments.Any(e => e.CategoryID == id && e.Status != "withdrawn");
            if (hasEnrolments)
                return Conflict(new { message = "Cannot delete category with active enrolments." });

            var withdrawnEnrolments = _db.Enrolments.Where(e => e.CategoryID == id).ToList();
            foreach (var enrolment in withdrawnEnrolments)
            {
                var results = _db.Results.Where(r => r.EnrolmentID == enrolment.EnrolmentID).ToList();
                _db.Results.RemoveRange(results);
            }
            _db.Enrolments.RemoveRange(withdrawnEnrolments);

            var routes = _db.Routes.Where(r => r.CategoryID == id).ToList();
            _db.Routes.RemoveRange(routes);
            _db.Categories.Remove(category);
            _db.SaveChanges();

            return NoContent();
        }

        // 4. GET ROUTE FOR CATEGORY  →  GET /api/categories/{id}/route
        [HttpGet("{id}/route")]
        [AllowAnonymous]
        public IActionResult GetRoute(int id)
        {
            var categoryExists = _db.Categories.Any(c => c.CategoryID == id);
            if (!categoryExists)
                return NotFound(new { message = "Category not found." });

            var route = _db.Routes.FirstOrDefault(r => r.CategoryID == id);
            if (route == null)
                return NotFound(new { message = "No route information available for this category." });

            return Ok(new
            {
                route.RouteID,
                route.CategoryID,
                route.MapUrl,
                route.ElevationGain,
                route.Description,
                route.CreatedAt,
                route.UpdatedAt
            });
        }

        // 5. UPSERT ROUTE  →  PUT /api/categories/{id}/route
        [HttpPut("{id}/route")]
        [Authorize]
        public IActionResult UpsertRoute(int id, [FromBody] RouteRequest request)
        {
            var category = _db.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            var eventEntity = _db.Events.FirstOrDefault(e => e.EventID == category.EventID);
            var organiser = eventEntity != null ? _db.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID) : null;

            var userId = _jwtService.GetUserIdFromToken(User);
            if (organiser?.UserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the event owner can manage route information." });

            var existingRoute = _db.Routes.FirstOrDefault(r => r.CategoryID == id);

            if (existingRoute != null)
            {
                existingRoute.MapUrl = request.MapUrl ?? existingRoute.MapUrl;
                existingRoute.ElevationGain = request.ElevationGain ?? existingRoute.ElevationGain;
                existingRoute.Description = request.Description ?? existingRoute.Description;
                existingRoute.UpdatedAt = DateTime.UtcNow;

                _db.SaveChanges();
                return Ok(existingRoute);
            }
            else
            {
                var newRoute = new RaceDayManagementAPI.Models.Route
                {
                    CategoryID = id,
                    MapUrl = request.MapUrl,
                    ElevationGain = request.ElevationGain,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Routes.Add(newRoute);
                _db.SaveChanges();

                return CreatedAtAction(nameof(GetRoute), new { id }, newRoute);
            }
        }
    }

    public class UpdateCategoryRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? EntryFee { get; set; }
        public int? MaxParticipants { get; set; }
        public TimeSpan? StartTime { get; set; }
    }
}
