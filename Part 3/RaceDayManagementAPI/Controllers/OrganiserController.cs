using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Services;

namespace RaceDayManagementAPI.Controllers
{
    /// <summary>
    /// Organiser controller for managing organiser profiles, events, and participant enrolments.
    /// Provides endpoints for event organisers to view/edit their profile and manage their event data.
    /// </summary>
    [ApiController]
    [Route("api/organisers")]
    [Authorize]
    public class OrganiserController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly RaceDayDbContext _db;

        public OrganiserController(JwtService jwtService, RaceDayDbContext db)
        {
            _jwtService = jwtService;
            _db = db;
        }

        /// <summary>
        /// Retrieves the authenticated organiser's profile information.
        /// </summary>
        /// <remarks>
        /// Returns detailed organiser profile including company name and associated user details.
        /// Requires authentication and organiser role.
        /// </remarks>
        /// <returns>Organiser profile with OrganiserID, CompanyName, User details, CreatedAt, and UpdatedAt</returns>
        /// <response code="200">Organiser profile successfully retrieved</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">User is not registered as an organiser</response>
        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetMyProfile()
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = _db.Organisers.FirstOrDefault(o => o.UserID == userId);

            if (organiser == null)
                return NotFound(new { message = "You are not registered as an organiser." });

            var user = _db.Users.FirstOrDefault(u => u.UserID == userId);

            return Ok(new
            {
                organiser.OrganiserID,
                organiser.CompanyName,
                User = new
                {
                    user?.UserID,
                    user?.Email,
                    user?.FullName,
                    user?.Phone
                },
                organiser.CreatedAt,
                organiser.UpdatedAt
            });
        }

        /// <summary>
        /// Updates the authenticated organiser's profile information.
        /// </summary>
        /// <remarks>
        /// Allows an organiser to update their company name and contact information.
        /// Requires authentication and organiser role.
        /// </remarks>
        /// <param name="request">Update request containing new company name and contact details</param>
        /// <returns>Updated organiser profile information</returns>
        /// <response code="200">Profile successfully updated</response>
        /// <response code="400">Company name is required</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">Organiser profile not found</response>
        [HttpPut("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateMyProfile([FromBody] UpdateOrganiserRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var user = _db.Users.FirstOrDefault(u => u.UserID == userId);
            var organiser = _db.Organisers.FirstOrDefault(o => o.UserID == userId);

            if (user == null || organiser == null)
                return NotFound(new { message = "Organiser not found." });

            // Update User fields
            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;
            if (!string.IsNullOrEmpty(request.Phone))
                user.Phone = request.Phone;
            if (!string.IsNullOrEmpty(request.Email))
            {
                var existing = _db.Users.FirstOrDefault(u => u.Email == request.Email && u.UserID != userId);
                if (existing != null)
                    return Conflict(new { message = "Email already in use." });
                user.Email = request.Email;
            }
            user.UpdatedAt = DateTime.UtcNow;

            // Update Organiser fields
            if (!string.IsNullOrEmpty(request.CompanyName))
                organiser.CompanyName = request.CompanyName;
            organiser.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(new
            {
                organiser.OrganiserID,
                organiser.CompanyName,
                User = new
                {
                    user.UserID,
                    user.Email,
                    user.FullName,
                    user.Phone
                },
                organiser.UpdatedAt
            });
        }

        /// <summary>
        /// Retrieves all events owned by the authenticated organiser.
        /// </summary>
        /// <remarks>
        /// Returns a comprehensive list of events created by the current organiser with their details and associated categories.
        /// Requires authentication and organiser role.
        /// </remarks>
        /// <returns>List of events with EventID, EventName, Date, Location, Description, Categories, CreatedAt, and UpdatedAt</returns>
        /// <response code="200">Successfully retrieved organiser's events</response>
        /// <response code="401">User is not authenticated</response>
        [HttpGet("user/events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMyEvents()
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = _db.Organisers.FirstOrDefault(o => o.UserID == userId);

            if (organiser == null)
                return NotFound(new { message = "Organiser not found." });

            var events = _db.Events
                .Where(e => e.OrganiserID == organiser.OrganiserID)
                .ToList();

            return Ok(events);
        }

        /// <summary>
        /// Retrieves all participant enrolments for a specific event owned by the organiser.
        /// </summary>
        /// <remarks>
        /// Returns detailed enrolment data including participant information and enrolment dates for all categories in the event.
        /// Only organisers who own the event can retrieve its enrolments.
        /// Requires authentication and organiser role.
        /// </remarks>
        /// <param name="eventId">The unique identifier of the event</param>
        /// <returns>List of enrolments with EnrolmentID, ParticipantID, CategoryID, ParticipantName, email, phone, EnrolmentDate, and Status</returns>
        /// <response code="200">Successfully retrieved event enrolments</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">Event not found or organiser does not own it</response>
        [HttpGet("user/events/{eventId}/enrolments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEventEnrolments(int eventId)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = _db.Organisers.FirstOrDefault(o => o.UserID == userId);

            if (organiser == null)
                return NotFound(new { message = "Organiser not found." });

            var eventEntity = _db.Events
                .FirstOrDefault(e => e.EventID == eventId && e.OrganiserID == organiser.OrganiserID);

            if (eventEntity == null)
                return NotFound(new { message = "Event not found or you do not own it." });

            var categoryIds = _db.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => c.CategoryID)
                .ToList();

            var enrolments = _db.Enrolments
                .Where(e => categoryIds.Contains(e.CategoryID))
                .ToList();

            return Ok(enrolments);
        }
    }

    public class UpdateOrganiserRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
    }
}