using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Services;

namespace RaceDayManagementAPI.Controllers
{
    /// <summary>
    /// Enrolments controller for managing participant event enrolments.
    /// Provides endpoints for participants to enrol in race categories and manage their entries.
    /// </summary>
    [ApiController]
    [Route("api/enrolments")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly RaceDayDbContext _db;

        public EnrolmentsController(JwtService jwtService, RaceDayDbContext db)
        {
            _jwtService = jwtService;
            _db = db;
        }

        // 1. ENROL IN EVENT  →  POST /api/enrolments
        [HttpPost]
        public IActionResult Enrol([FromBody] EnrolRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var participant = _db.Participants.FirstOrDefault(p => p.UserID == userId);

            if (participant == null)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only participants can enrol in events." });

            var category = _db.Categories.FirstOrDefault(c => c.CategoryID == request.CategoryID);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            var eventEntity = _db.Events.FirstOrDefault(e => e.EventID == category.EventID);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found." });

            if (string.Equals(eventEntity.Status, "draft", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(eventEntity.Status, "cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(eventEntity.Status, "completed", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "This event is not open for enrolment." });
            }

            // Capacity check
            var activeCount = _db.Enrolments.Count(e =>
                e.CategoryID == request.CategoryID && e.Status != "withdrawn");

            if (category.MaxParticipants.HasValue && activeCount >= category.MaxParticipants)
                return BadRequest(new { message = "This category is full." });

            // Duplicate check
            var existing = _db.Enrolments.FirstOrDefault(e =>
                e.ParticipantID == participant.ParticipantID &&
                e.CategoryID == request.CategoryID);

            if (existing != null && existing.Status != "withdrawn")
                return Conflict(new { message = "You are already enrolled in this category." });

            Enrolment enrolment;
            if (existing != null && existing.Status == "withdrawn")
            {
                existing.Status = "confirmed";
                existing.EnrolmentDate = DateTime.UtcNow;
                existing.UpdatedAt = DateTime.UtcNow;
                enrolment = existing;
            }
            else
            {
                enrolment = new Enrolment
                {
                    ParticipantID = participant.ParticipantID,
                    CategoryID = request.CategoryID,
                    EnrolmentDate = DateTime.UtcNow,
                    Status = "confirmed",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Enrolments.Add(enrolment);
            }

            _db.SaveChanges();

            return CreatedAtAction(nameof(GetEnrolment), new { id = enrolment.EnrolmentID }, new
            {
                enrolment.EnrolmentID,
                enrolment.ParticipantID,
                enrolment.CategoryID,
                enrolment.EnrolmentDate,
                enrolment.Status,
                Event = new
                {
                    eventEntity.EventID,
                    eventEntity.Name,
                    eventEntity.EventDate
                },
                Category = new
                {
                    category.CategoryID,
                    category.Name,
                    category.DistanceKm,
                    category.EntryFee
                }
            });
        }

        // 2. GET ENROLMENT BY ID  →  GET /api/enrolments/{id}
        [HttpGet("{id}")]
        public IActionResult GetEnrolment(int id)
        {
            var enrolment = _db.Enrolments.FirstOrDefault(e => e.EnrolmentID == id);
            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            var userId = _jwtService.GetUserIdFromToken(User);

            var participant = _db.Participants.FirstOrDefault(p => p.ParticipantID == enrolment.ParticipantID);
            var category = _db.Categories.FirstOrDefault(c => c.CategoryID == enrolment.CategoryID);
            var eventEntity = category != null ? _db.Events.FirstOrDefault(ev => ev.EventID == category.EventID) : null;
            var organiser = eventEntity != null ? _db.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID) : null;

            bool isOwner = participant?.UserID == userId;
            bool isEventOrganiser = organiser?.UserID == userId;

            if (!isOwner && !isEventOrganiser)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have access to this enrolment." });

            return Ok(new
            {
                enrolment.EnrolmentID,
                enrolment.ParticipantID,
                enrolment.CategoryID,
                enrolment.EnrolmentDate,
                enrolment.Status,
                Event = eventEntity == null ? null : new
                {
                    eventEntity.EventID,
                    eventEntity.Name,
                    eventEntity.EventDate,
                    eventEntity.Location
                },
                Category = category == null ? null : new
                {
                    category.CategoryID,
                    category.Name,
                    category.DistanceKm,
                    category.EntryFee
                }
            });
        }

        // 3. WITHDRAW FROM EVENT  →  DELETE /api/enrolments/{id}
        [HttpDelete("{id}")]
        public IActionResult WithdrawEnrolment(int id)
        {
            var enrolment = _db.Enrolments.FirstOrDefault(e => e.EnrolmentID == id);
            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var participant = _db.Participants.FirstOrDefault(p => p.ParticipantID == enrolment.ParticipantID);

            if (participant?.UserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the participant can withdraw this enrolment." });

            if (enrolment.Status == "withdrawn")
                return BadRequest(new { message = "This enrolment is already withdrawn." });

            enrolment.Status = "withdrawn";
            enrolment.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            return NoContent();
        }
    }

    public class EnrolRequest
    {
        public int CategoryID { get; set; }
    }
}
