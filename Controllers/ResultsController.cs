
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api/results")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ====================================================================================
        // POST: api/results
        // Organiser records a participant's result.

        [HttpPost]
        public async Task<IActionResult> CreateResult(
            CreateResultRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            // Find the registration.
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r =>
                    r.RegistrationId == request.RegistrationId);

            if (registration == null)
                return NotFound("Registration not found.");

            // Check that the organiser owns the event.
            if (registration.Event.OrganizerId != userId.Value)
                return Forbid();

            // Only confirmed registrations can receive results.
            if (registration.RegistrationStatus != "Confirmed")
                return BadRequest(
                    "Only confirmed registrations can receive results.");

            // Check whether a result already exists.
            bool resultExists = await _context.Results
                .AnyAsync(r =>
                    r.RegistrationId == request.RegistrationId);

            if (resultExists)
                return Conflict(
                    "A result already exists for this registration.");

            // Create the result.
            var result = new Result
            {
                RegistrationId = request.RegistrationId,
                FinishTime = request.FinishTime,
                Position = request.Position,
                ResultStatus = request.ResultStatus
            };

            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Result recorded successfully.",
                result.ResultId,
                result.RegistrationId,
                result.FinishTime,
                result.Position,
                result.ResultStatus
            });
        }

        // ====================================================================================
        // GET: api/results/mine
        // Participant views their own results.

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyResults()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Participant")
                return Forbid();

            var results = await _context.Results
                .Include(r => r.Registration)
                    .ThenInclude(r => r.Event)
                .Include(r => r.Registration)
                    .ThenInclude(r => r.Activity)
                .Where(r =>
                    r.Registration.ParticipantId == userId.Value)
                .Select(r => new
                {
                    r.ResultId,
                    r.RegistrationId,

                    EventName = r.Registration.Event.EventsName,
                    EventDate = r.Registration.Event.EventDate,

                    ActivityName =
                        r.Registration.Activity.ActivityName,

                    r.FinishTime,
                    r.Position,
                    r.ResultStatus
                })
                .ToListAsync();

            return Ok(results);
        }

        // ====================================================================================
        // GET: api/results/event/5
        // Organiser views results for their own event.


        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            // Check event ownership.
            if (eventItem.OrganizerId != userId.Value)
                return Forbid();

            var results = await _context.Results
                .Include(r => r.Registration)
                    .ThenInclude(r => r.Participant)
                .Include(r => r.Registration)
                    .ThenInclude(r => r.Activity)
                .Where(r => r.Registration.EventId == eventId)
                .OrderBy(r => r.Position)
                .Select(r => new
                {
                    r.ResultId,
                    r.RegistrationId,

                    ParticipantName =
                        r.Registration.Participant.Name,

                    ParticipantLastname =
                        r.Registration.Participant.Lastname,

                    ActivityName =
                        r.Registration.Activity.ActivityName,

                    r.Registration.BibNumber,
                    r.FinishTime,
                    r.Position,
                    r.ResultStatus
                })
                .ToListAsync();

            return Ok(results);
        }

        // ========================================== REQUEST MODEL ==========================================

        public class CreateResultRequest
        {
            [Range(1, int.MaxValue)]
            public int RegistrationId { get; set; }

            public TimeOnly? FinishTime { get; set; }

            [Range(1, int.MaxValue)]
            public int? Position { get; set; }

            [Required, StringLength(20)]
            public string ResultStatus { get; set; }
                = string.Empty;
        }
    }
}
