using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Services;

namespace RaceDayManagementAPI.Controllers
{
    /// <summary>
    /// Participant controller for managing participant profiles, enrolments, and race results.
    /// Provides endpoints for race participants to view/edit their profile and track their event participation.
    /// </summary>
    [ApiController]
    [Route("api/participants")]
    [Authorize]
    public class ParticipantController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly RaceDayDbContext _db;

        public ParticipantController(JwtService jwtService, RaceDayDbContext db)
        {
            _jwtService = jwtService;
            _db = db;
        }

        /// <summary>
        /// Retrieves the authenticated participant's profile information.
        /// </summary>
        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetMyProfile()
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var participant = _db.Participants.FirstOrDefault(p => p.UserID == userId);

            if (participant == null)
                return NotFound(new { message = "You are not registered as a participant." });

            var user = _db.Users.FirstOrDefault(u => u.UserID == userId);

            return Ok(new
            {
                participant.ParticipantID,
                UserID = user?.UserID ?? userId,
                FullName = user?.FullName,
                Email = user?.Email,
                Phone = user?.Phone,
                participant.DateOfBirth,
                participant.IDNumber,
                participant.EmergencyContact,
                User = new
                {
                    user?.UserID,
                    user?.Email,
                    user?.FullName,
                    user?.Phone
                },
                participant.CreatedAt,
                participant.UpdatedAt
            });
        }

        /// <summary>
        /// Updates the authenticated participant's profile information.
        /// </summary>
        [HttpPut("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateMyProfile([FromBody] UpdateParticipantRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var user = _db.Users.FirstOrDefault(u => u.UserID == userId);
            var participant = _db.Participants.FirstOrDefault(p => p.UserID == userId);

            if (user == null || participant == null)
                return NotFound(new { message = "Participant not found." });

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

            // Update Participant fields
            if (request.DateOfBirth.HasValue)
                participant.DateOfBirth = request.DateOfBirth;
            if (!string.IsNullOrEmpty(request.IDNumber))
                participant.IDNumber = request.IDNumber;
            if (!string.IsNullOrEmpty(request.EmergencyContact))
                participant.EmergencyContact = request.EmergencyContact;
            participant.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(new
            {
                participant.ParticipantID,
                user.UserID,
                user.FullName,
                user.Email,
                user.Phone,
                participant.DateOfBirth,
                participant.IDNumber,
                participant.EmergencyContact,
                User = new
                {
                    user.UserID,
                    user.Email,
                    user.FullName,
                    user.Phone
                },
                participant.UpdatedAt
            });
        }

        /// <summary>
        /// Retrieves all race enrolments for the authenticated participant, including Event and Category details.
        /// </summary>
        [HttpGet("user/enrolments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMyEnrolments()
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var participant = _db.Participants.FirstOrDefault(p => p.UserID == userId);

            if (participant == null)
                return NotFound(new { message = "Participant not found." });

            var participantEnrolments = _db.Enrolments
                .Where(e => e.ParticipantID == participant.ParticipantID)
                .ToList();

            var categories = _db.Categories.ToList();
            var events = _db.Events.ToList();

            var enrolments = participantEnrolments.Select(e =>
            {
                var category = categories.FirstOrDefault(c => c.CategoryID == e.CategoryID);
                var eventEntity = category != null ? events.FirstOrDefault(ev => ev.EventID == category.EventID) : null;

                return new
                {
                    e.EnrolmentID,
                    e.ParticipantID,
                    e.CategoryID,
                    e.EnrolmentDate,
                    e.Status,
                    EventID = eventEntity?.EventID ?? 0,
                    EventName = eventEntity?.Name ?? string.Empty,
                    EventDate = eventEntity?.EventDate,
                    Location = eventEntity?.Location ?? string.Empty,
                    CategoryName = category?.Name ?? string.Empty,
                    DistanceKm = category?.DistanceKm ?? 0,
                    EntryFee = category?.EntryFee ?? 0,
                    Event = eventEntity == null ? null : new
                    {
                        eventEntity.EventID,
                        eventEntity.Name,
                        EventName = eventEntity.Name,
                        eventEntity.EventDate,
                        eventEntity.Location,
                        eventEntity.EventType,
                        eventEntity.Status
                    },
                    Category = category == null ? null : new
                    {
                        category.CategoryID,
                        category.Name,
                        CategoryName = category.Name,
                        category.DistanceKm,
                        category.EntryFee
                    }
                };
            }).ToList();

            return Ok(enrolments);
        }

        /// <summary>
        /// Retrieves all race results for the authenticated participant, including Event and Category details.
        /// </summary>
        [HttpGet("user/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMyResults()
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var participant = _db.Participants.FirstOrDefault(p => p.UserID == userId);

            if (participant == null)
                return NotFound(new { message = "Participant not found." });

            var participantEnrolments = _db.Enrolments
                .Where(e => e.ParticipantID == participant.ParticipantID)
                .ToList();

            var enrolmentIds = participantEnrolments.Select(e => e.EnrolmentID).ToList();

            var rawResults = _db.Results
                .Where(r => enrolmentIds.Contains(r.EnrolmentID))
                .ToList();

            var categories = _db.Categories.ToList();
            var events = _db.Events.ToList();

            var results = rawResults.Select(r =>
            {
                var enrolment = participantEnrolments.FirstOrDefault(e => e.EnrolmentID == r.EnrolmentID);
                var category = enrolment != null ? categories.FirstOrDefault(c => c.CategoryID == enrolment.CategoryID) : null;
                var eventEntity = category != null ? events.FirstOrDefault(ev => ev.EventID == category.EventID) : null;

                var formattedTime = r.FinishTimeSeconds.HasValue
                    ? TimeSpan.FromSeconds(r.FinishTimeSeconds.Value).ToString(@"hh\:mm\:ss")
                    : null;

                return new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    r.Position,
                    Placement = r.Position,
                    r.FinishTimeSeconds,
                    FinishTime = formattedTime,
                    r.Status,
                    r.CreatedAt,
                    EventID = eventEntity?.EventID ?? 0,
                    EventName = eventEntity?.Name ?? string.Empty,
                    EventDate = eventEntity?.EventDate,
                    Location = eventEntity?.Location ?? string.Empty,
                    CategoryID = category?.CategoryID ?? 0,
                    CategoryName = category?.Name ?? string.Empty,
                    DistanceKm = category?.DistanceKm ?? 0,
                    Event = eventEntity == null ? null : new
                    {
                        eventEntity.EventID,
                        eventEntity.Name,
                        EventName = eventEntity.Name,
                        eventEntity.EventDate,
                        eventEntity.Location
                    },
                    Category = category == null ? null : new
                    {
                        category.CategoryID,
                        category.Name,
                        CategoryName = category.Name,
                        category.DistanceKm
                    }
                };
            }).ToList();

            return Ok(results);
        }
    }

    public class UpdateParticipantRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? IDNumber { get; set; }
        public string? EmergencyContact { get; set; }
    }
}