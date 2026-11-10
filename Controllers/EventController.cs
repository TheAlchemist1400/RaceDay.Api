
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events
        // Both Participants and Organizers can view events.

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Location)
                .Include(e => e.Organizer)
                .Select(e => new
                {
                    e.EventId,
                    e.EventsName,
                    e.EventDate,
                    e.MaxParticipants,
                    e.LocationId,
                    Location = e.Location.Venue,
                    City = e.Location.City,
                    Province = e.Location.Province,
                    Organizer = e.Organizer.OrganizationName
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/5

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Location)
                .Include(e => e.Organizer)
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.EventsName,
                    e.EventDate,
                    e.MaxParticipants,
                    e.LocationId,
                    Location = e.Location.Venue,
                    City = e.Location.City,
                    Province = e.Location.Province,
                    Organizer = e.Organizer.OrganizationName
                })
                .FirstOrDefaultAsync();

            if (eventItem == null)
                return NotFound("Event not found.");

            return Ok(eventItem);
        }

        // GET: api/events/mine
        // Organizers can only view their own events.

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyEvents()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            var events = await _context.Events
                .Where(e => e.OrganizerId == userId.Value)
                .Include(e => e.Location)
                .Select(e => new
                {
                    e.EventId,
                    e.EventsName,
                    e.EventDate,
                    e.MaxParticipants,
                    Location = e.Location.Venue,
                    e.Location.City,
                    e.Location.Province
                })
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/events
        // Only Organizers can create events.

        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            CreateEventRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            var location = await _context.EventLocations
                .FindAsync(request.LocationId);

            if (location == null)
                return BadRequest("Invalid event location.");

            var newEvent = new Event
            {
                EventsName = request.EventsName,
                EventDate = request.EventDate,
                MaxParticipants = request.MaxParticipants,
                LocationId = request.LocationId,

                // Get organizer ID from the session.
                OrganizerId = userId.Value
            };

            _context.Events.Add(newEvent);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = newEvent.EventId },
                new
                {
                    message = "Event created successfully.",
                    newEvent.EventId,
                    newEvent.EventsName,
                    newEvent.EventDate,
                    newEvent.MaxParticipants,
                    newEvent.LocationId
                });
        }

        // PUT: api/events/5
        // Only the owner can update an event.

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            UpdateEventRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganizerId != userId.Value)
                return Forbid();

            var location = await _context.EventLocations
                .FindAsync(request.LocationId);

            if (location == null)
                return BadRequest("Invalid event location.");

            eventItem.EventsName = request.EventsName;
            eventItem.EventDate = request.EventDate;
            eventItem.MaxParticipants = request.MaxParticipants;
            eventItem.LocationId = request.LocationId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                eventItem.EventId,
                eventItem.EventsName,
                eventItem.EventDate,
                eventItem.MaxParticipants,
                eventItem.LocationId
            });
        }

        // DELETE: api/events/5
        // Only the owner can delete an event.

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganizerId != userId.Value)
                return Forbid();

            _context.Events.Remove(eventItem);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }

        // Request models

        public class CreateEventRequest
        {
            [Required, StringLength(50)]
            public string EventsName { get; set; } = string.Empty;

            [Required]
            public DateOnly EventDate { get; set; }

            [Range(1, int.MaxValue)]
            public int MaxParticipants { get; set; }

            [Range(1, int.MaxValue)]
            public int LocationId { get; set; }
        }

        public class UpdateEventRequest
        {
            [Required, StringLength(50)]
            public string EventsName { get; set; } = string.Empty;

            [Required]
            public DateOnly EventDate { get; set; }

            [Range(1, int.MaxValue)]
            public int MaxParticipants { get; set; }

            [Range(1, int.MaxValue)]
            public int LocationId { get; set; }
        }
    }
}
