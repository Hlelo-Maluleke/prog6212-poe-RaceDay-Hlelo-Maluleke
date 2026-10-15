using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public EnrolmentsController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        // 1. ENROL IN EVENT  →  POST /api/enrolments
        [HttpPost]
        public IActionResult Enrol([FromBody] EnrolRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var participant = RaceDayDB.Participants.FirstOrDefault(p => p.UserID == userId);

            if (participant == null)
                return Forbid("Only participants can enrol in events.");

            var category = RaceDayDB.Categories.FirstOrDefault(c => c.CategoryID == request.CategoryID);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            var eventEntity = RaceDayDB.Events.FirstOrDefault(e => e.EventID == category.EventID);
            if (eventEntity?.Status != "published")
                return BadRequest(new { message = "This event is not open for enrolment." });

            // Capacity check
            var activeCount = RaceDayDB.Enrolments.Count(e =>
                e.CategoryID == request.CategoryID && e.Status != "withdrawn");

            if (category.MaxParticipants.HasValue && activeCount >= category.MaxParticipants)
                return BadRequest(new { message = "This category is full." });

            // Duplicate check
            var existing = RaceDayDB.Enrolments.FirstOrDefault(e =>
                e.ParticipantID == participant.ParticipantID &&
                e.CategoryID == request.CategoryID &&
                e.Status != "withdrawn");

            if (existing != null)
                return Conflict(new { message = "You are already enrolled in this category." });

            var enrolment = new Enrolment
            {
                EnrolmentID = RaceDayDB.NextEnrolmentID++,
                ParticipantID = participant.ParticipantID,
                CategoryID = request.CategoryID,
                EnrolmentDate = DateTime.Now,
                Status = "pending"
            };

            RaceDayDB.Enrolments.Add(enrolment);

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
            var enrolment = RaceDayDB.Enrolments.FirstOrDefault(e => e.EnrolmentID == id);
            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            var userId = _jwtService.GetUserIdFromToken(User);

            // Authorisation: participant who owns it OR organiser who owns the event
            var participant = RaceDayDB.Participants.FirstOrDefault(p => p.ParticipantID == enrolment.ParticipantID);
            var category = RaceDayDB.Categories.FirstOrDefault(c => c.CategoryID == enrolment.CategoryID);
            var eventEntity = category != null ? RaceDayDB.Events.FirstOrDefault(ev => ev.EventID == category.EventID) : null;
            var organiser = eventEntity != null ? RaceDayDB.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID) : null;

            bool isOwner = participant?.UserID == userId;
            bool isEventOrganiser = organiser?.UserID == userId;

            if (!isOwner && !isEventOrganiser)
                return Forbid("You do not have access to this enrolment.");

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
            var enrolment = RaceDayDB.Enrolments.FirstOrDefault(e => e.EnrolmentID == id);
            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            var userId = _jwtService.GetUserIdFromToken(User);
            var participant = RaceDayDB.Participants.FirstOrDefault(p => p.ParticipantID == enrolment.ParticipantID);

            if (participant?.UserID != userId)
                return Forbid("Only the participant can withdraw this enrolment.");

            if (enrolment.Status == "withdrawn")
                return BadRequest(new { message = "This enrolment is already withdrawn." });

            // Soft delete – update status
            enrolment.Status = "withdrawn";
            enrolment.UpdatedAt = DateTime.Now;

            return NoContent();
        }
    }

    // Request Models
    public class EnrolRequest
    {
        public int CategoryID { get; set; }
    }
}