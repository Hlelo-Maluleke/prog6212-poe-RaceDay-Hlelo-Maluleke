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
                UserID = user?.UserID ?? userId,
                FullName = user?.FullName,
                Email = user?.Email,
                Phone = user?.Phone,
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

            if (!string.IsNullOrEmpty(request.CompanyName))
                organiser.CompanyName = request.CompanyName;
            organiser.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(new
            {
                organiser.OrganiserID,
                organiser.CompanyName,
                user.UserID,
                user.FullName,
                user.Email,
                user.Phone,
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
        /// Retrieves all events owned by the authenticated organiser, including categories.
        /// </summary>
        [HttpGet("user/events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMyEvents()
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var organiser = _db.Organisers.FirstOrDefault(o => o.UserID == userId);

            if (organiser == null)
                return NotFound(new { message = "Organiser not found." });

            var organiserEvents = _db.Events
                .Where(e => e.OrganiserID == organiser.OrganiserID)
                .ToList();

            var categories = _db.Categories.ToList();

            var events = organiserEvents.Select(e => new
            {
                e.EventID,
                e.OrganiserID,
                e.Name,
                EventName = e.Name,
                e.Description,
                e.EventDate,
                e.StartTime,
                e.Location,
                e.EventType,
                e.Status,
                e.BannerUrl,
                e.CreatedAt,
                e.UpdatedAt,
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
            }).ToList();

            return Ok(events);
        }

        /// <summary>
        /// Retrieves all participant enrolments for a specific event owned by the organiser, including captured results.
        /// </summary>
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

            var categories = _db.Categories
                .Where(c => c.EventID == eventId)
                .ToList();

            var categoryIds = categories.Select(c => c.CategoryID).ToList();

            var rawEnrolments = _db.Enrolments
                .Where(e => categoryIds.Contains(e.CategoryID))
                .ToList();

            var enrolmentIds = rawEnrolments.Select(e => e.EnrolmentID).ToList();
            var results = _db.Results
                .Where(r => enrolmentIds.Contains(r.EnrolmentID))
                .ToList();

            var participants = _db.Participants.ToList();
            var users = _db.Users.ToList();

            var enrolments = rawEnrolments.Select(e =>
            {
                var participant = participants.FirstOrDefault(p => p.ParticipantID == e.ParticipantID);
                var pUser = participant != null ? users.FirstOrDefault(u => u.UserID == participant.UserID) : null;
                var category = categories.FirstOrDefault(c => c.CategoryID == e.CategoryID);
                var result = results.FirstOrDefault(r => r.EnrolmentID == e.EnrolmentID);

                return new
                {
                    e.EnrolmentID,
                    e.ParticipantID,
                    e.CategoryID,
                    e.EnrolmentDate,
                    e.Status,
                    ParticipantName = pUser?.FullName ?? string.Empty,
                    Email = pUser?.Email ?? string.Empty,
                    Phone = pUser?.Phone ?? string.Empty,
                    CategoryName = category?.Name ?? string.Empty,
                    Participant = participant == null ? null : new
                    {
                        participant.ParticipantID,
                        UserID = participant.UserID,
                        FullName = pUser?.FullName ?? string.Empty,
                        Email = pUser?.Email ?? string.Empty,
                        Phone = pUser?.Phone ?? string.Empty
                    },
                    Category = category == null ? null : new
                    {
                        category.CategoryID,
                        category.Name,
                        category.DistanceKm,
                        category.EntryFee
                    },
                    Result = result == null ? null : new
                    {
                        result.ResultID,
                        result.EnrolmentID,
                        result.Position,
                        result.FinishTimeSeconds,
                        result.Status
                    }
                };
            }).ToList();

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