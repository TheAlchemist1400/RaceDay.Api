
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api/enrolments")]
    [ApiController]
    public class RegistrationsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RegistrationsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ====================================================================================
        // POST: api/enrolments
        // Participant registers for an event.

        [HttpPost]
        public async Task<IActionResult> CreateRegistration(
            CreateRegistrationRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Participant")
                return Forbid();

            // Check if event exists.
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == request.EventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            // Check if selected activity belongs to this event.
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a =>
                    a.ActivityId == request.ActivityId &&
                    a.EventId == request.EventId);

            if (activity == null)
                return BadRequest(
                    "The selected activity does not belong to this event.");

            // Prevent duplicate registrations for the same event.
            bool alreadyRegistered = await _context.Registrations
                .AnyAsync(r =>
                    r.ParticipantId == userId.Value &&
                    r.EventId == request.EventId);

            if (alreadyRegistered)
                return Conflict(
                    "You are already registered for this event.");

            // Check the event's maximum participant capacity.
            int currentRegistrations = await _context.Registrations
                .CountAsync(r =>
                    r.EventId == request.EventId &&
                    r.RegistrationStatus != "Cancelled");

            if (currentRegistrations >= eventItem.MaxParticipants)
                return Conflict("This event has reached maximum capacity.");

            // Check the activity's maximum entrants.
            int activityRegistrations = await _context.Registrations
                .CountAsync(r =>
                    r.ActivityId == request.ActivityId &&
                    r.EventId == request.EventId &&
                    r.RegistrationStatus != "Cancelled");

            if (activityRegistrations >= activity.MaximumEntrants)
                return Conflict("This activity is fully booked.");

            // Generate a unique bib number within the event.
            int bibNumber = 1;

            var existingBibNumbers = await _context.Registrations
                .Where(r => r.EventId == request.EventId)
                .Select(r => r.BibNumber)
                .ToListAsync();

            var usedNumbers = existingBibNumbers.ToHashSet();

            while (usedNumbers.Contains(bibNumber))
            {
                bibNumber++;
            }

            var registration = new Registration
            {
                ParticipantId = userId.Value,
                EventId = request.EventId,
                ActivityId = request.ActivityId,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
                BibNumber = bibNumber,
                RegistrationStatus = "Confirmed"
            };

            _context.Registrations.Add(registration);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Registration successful.",
                registration.RegistrationId,
                registration.ParticipantId,
                registration.EventId,
                registration.ActivityId,
                registration.RegistrationDate,
                registration.BibNumber,
                registration.RegistrationStatus
            });
        }

        // ==========================================
        // GET: api/enrolments/mine
        // Participant views their own registrations.
        // ==========================================

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyRegistrations()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Participant")
                return Forbid();

            var registrations = await _context.Registrations
                .Where(r => r.ParticipantId == userId.Value)
                .Include(r => r.Event)
                .Include(r => r.Activity)
                .Select(r => new
                {
                    r.RegistrationId,
                    r.EventId,
                    EventName = r.Event.EventsName,
                    r.ActivityId,
                    ActivityName = r.Activity.ActivityName,
                    r.RegistrationDate,
                    r.BibNumber,
                    r.RegistrationStatus
                })
                .ToListAsync();

            return Ok(registrations);
        }

        // ====================================================================================
        // GET: api/enrolments/event/5
        // Event owner views event registrations.


        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventRegistrations(int eventId)
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

            if (eventItem.OrganizerId != userId.Value)
                return Forbid();

            var registrations = await _context.Registrations
                .Where(r => r.EventId == eventId)
                .Include(r => r.Participant)
                .Include(r => r.Activity)
                .Select(r => new
                {
                    r.RegistrationId,
                    ParticipantName = r.Participant.Name,
                    ParticipantLastname = r.Participant.Lastname,
                    ParticipantEmail = r.Participant.Email,
                    r.ActivityId,
                    ActivityName = r.Activity.ActivityName,
                    r.RegistrationDate,
                    r.BibNumber,
                    r.RegistrationStatus
                })
                .ToListAsync();

            return Ok(registrations);
        }

        // ====================================================================================
        // PUT: api/enrolments/5/status
        // Event owner updates registration status.

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateRegistrationStatus(
            int id,
            UpdateStatusRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration == null)
                return NotFound("Registration not found.");

            // Check event ownership.
            if (registration.Event.OrganizerId != userId.Value)
                return Forbid();

            // Only allow valid registration statuses.
            string[] validStatuses =
            {
                "Pending",
                "Confirmed",
                "Cancelled"
            };

            if (!validStatuses.Contains(
                request.RegistrationStatus,
                StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid registration status.");
            }

            registration.RegistrationStatus =
                request.RegistrationStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration status updated successfully.",
                registration.RegistrationId,
                registration.RegistrationStatus
            });
        }

        // ========================================== REQUEST MODELS ==========================================
        public class CreateRegistrationRequest
        {
            [Range(1, int.MaxValue)]
            public int EventId { get; set; }

            [Range(1, int.MaxValue)]
            public int ActivityId { get; set; }
        }

        public class UpdateStatusRequest
        {
            [Required, StringLength(20)]
            public string RegistrationStatus { get; set; }
                = string.Empty;
        }
    }
}
