using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Services;

namespace RaceDayManagementAPI.Controllers
{
    /// <summary>
    /// Results controller for managing and recording race results.
    /// Provides endpoints for organisers to capture/update participant results and retrieve result data.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public ResultsController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        // 1. CAPTURE/UPDATE RESULT (UPSERT)
        // POST /api/enrolments/{enrolmentId}/results
        [HttpPost("enrolments/{enrolmentId}/results")]
        [Authorize]
        public IActionResult CaptureResult(int enrolmentId, [FromBody] ResultRequest request)
        {
            var enrolment = RaceDayDB.Enrolments.FirstOrDefault(e => e.EnrolmentID == enrolmentId);
            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            var category = RaceDayDB.Categories.FirstOrDefault(c => c.CategoryID == enrolment.CategoryID);
            var eventEntity = category != null ? RaceDayDB.Events.FirstOrDefault(e => e.EventID == category.EventID) : null;
            var organiser = eventEntity != null ? RaceDayDB.Organisers.FirstOrDefault(o => o.OrganiserID == eventEntity.OrganiserID) : null;

            var userId = _jwtService.GetUserIdFromToken(User);
            if (organiser?.UserID != userId)
                return Forbid("Only the event organiser can capture results.");

            var existingResult = RaceDayDB.Results.FirstOrDefault(r => r.EnrolmentID == enrolmentId);

            if (existingResult != null)
            {
                // Update
                existingResult.FinishTimeSeconds = request.FinishTimeSeconds;
                existingResult.Position = request.Position;
                existingResult.Status = request.Status ?? "finished";
                existingResult.UpdatedAt = DateTime.Now;

                return Ok(existingResult);
            }
            else
            {
                // Create
                var result = new Result
                {
                    ResultID = RaceDayDB.NextResultID++,
                    EnrolmentID = enrolmentId,
                    OrganiserID = organiser.OrganiserID,
                    FinishTimeSeconds = request.FinishTimeSeconds,
                    Position = request.Position,
                    Status = request.Status ?? "finished"
                };
                RaceDayDB.Results.Add(result);

                return CreatedAtAction(nameof(GetEventResults), new { eventId = eventEntity?.EventID }, result);
            }
        }

        // 2. GET RESULTS FOR EVENT (PUBLIC)
        // GET /api/events/{eventId}/results
        [HttpGet("events/{eventId}/results")]
        [AllowAnonymous]
        public IActionResult GetEventResults(int eventId)
        {
            var eventExists = RaceDayDB.Events.Any(e => e.EventID == eventId);
            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var categoryIds = RaceDayDB.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => c.CategoryID)
                .ToList();

            var enrolmentIds = RaceDayDB.Enrolments
                .Where(e => categoryIds.Contains(e.CategoryID))
                .Select(e => e.EnrolmentID)
                .ToList();

            var results = RaceDayDB.Results
                .Where(r => enrolmentIds.Contains(r.EnrolmentID))
                .OrderBy(r => r.Position)
                .Select(r =>
                {
                    var enrolment = RaceDayDB.Enrolments.FirstOrDefault(e => e.EnrolmentID == r.EnrolmentID);
                    var participant = enrolment != null ? RaceDayDB.Participants.FirstOrDefault(p => p.ParticipantID == enrolment.ParticipantID) : null;
                    var pUser = participant != null ? RaceDayDB.Users.FirstOrDefault(u => u.UserID == participant.UserID) : null;
                    var category = enrolment != null ? RaceDayDB.Categories.FirstOrDefault(c => c.CategoryID == enrolment.CategoryID) : null;

                    return new
                    {
                        r.ResultID,
                        r.Position,
                        r.FinishTimeSeconds,
                        r.Status,
                        Participant = new
                        {
                            ParticipantID = participant?.ParticipantID,
                            FullName = pUser?.FullName
                        },
                        Category = new
                        {
                            CategoryID = category?.CategoryID,
                            Name = category?.Name,
                            DistanceKm = category?.DistanceKm
                        }
                    };
                })
                .ToList();

            return Ok(results);
        }

        // 3. GET MY RESULTS (PARTICIPANT)
        // GET /api/participants/user/results  (already in ParticipantController)
        [HttpGet("results/user")]
        [Authorize]
        public IActionResult GetMyResults()
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var participant = RaceDayDB.Participants.FirstOrDefault(p => p.UserID == userId);

            if (participant == null)
                return Forbid("Only participants can view their results.");

            var enrolmentIds = RaceDayDB.Enrolments
                .Where(e => e.ParticipantID == participant.ParticipantID)
                .Select(e => e.EnrolmentID)
                .ToList();

            var results = RaceDayDB.Results
                .Where(r => enrolmentIds.Contains(r.EnrolmentID))
                .Select(r =>
                {
                    var enrolment = RaceDayDB.Enrolments.FirstOrDefault(e => e.EnrolmentID == r.EnrolmentID);
                    var category = enrolment != null ? RaceDayDB.Categories.FirstOrDefault(c => c.CategoryID == enrolment.CategoryID) : null;
                    var eventEntity = category != null ? RaceDayDB.Events.FirstOrDefault(ev => ev.EventID == category.EventID) : null;

                    return new
                    {
                        r.ResultID,
                        r.Position,
                        r.FinishTimeSeconds,
                        r.Status,
                        r.CreatedAt,
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
                            category.DistanceKm
                        }
                    };
                })
                .ToList();

            return Ok(results);
        }

        // 4. GET SINGLE RESULT -  GET /api/results/{id}
        [HttpGet("results/{id}")]
        [AllowAnonymous]
        public IActionResult GetResult(int id)
        {
            var result = RaceDayDB.Results.FirstOrDefault(r => r.ResultID == id);
            if (result == null)
                return NotFound(new { message = "Result not found." });

            return Ok(result);
        }
    }

    // Request Models
    public class ResultRequest
    {
        public int? FinishTimeSeconds { get; set; }
        public int? Position { get; set; }
        public string? Status { get; set; }
    }
}