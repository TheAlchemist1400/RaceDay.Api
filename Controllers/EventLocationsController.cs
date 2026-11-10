
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api/locations")]
    [ApiController]
    public class EventLocationsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventLocationsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ====================================================================================
        // GET: api/locations
        // Both roles can view locations.

        [HttpGet]
        public async Task<IActionResult> GetLocations()
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            var locations = await _context.EventLocations
                .Select(l => new
                {
                    l.LocationId,
                    l.City,
                    l.Province,
                    l.Venue
                })
                .ToListAsync();

            return Ok(locations);
        }

        // ====================================================================================
        // GET: api/locations/5
        // View a specific location.

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocation(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            var location = await _context.EventLocations
                .Where(l => l.LocationId == id)
                .Select(l => new
                {
                    l.LocationId,
                    l.City,
                    l.Province,
                    l.Venue
                })
                .FirstOrDefaultAsync();

            if (location == null)
                return NotFound("Location not found.");

            return Ok(location);
        }

        // ====================================================================================
        // POST: api/locations
        // Only Organisers can create locations.

        [HttpPost]
        public async Task<IActionResult> CreateLocation(
            LocationRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            var location = new EventLocation
            {
                City = request.City,
                Province = request.Province,
                Venue = request.Venue
            };

            _context.EventLocations.Add(location);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Location created successfully.",
                location.LocationId,
                location.City,
                location.Province,
                location.Venue
            });
        }

        // ====================================================================================
        // PUT: api/locations/5
        // Only Organisers can update locations.

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(
            int id,
            LocationRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var location = await _context.EventLocations
                .FirstOrDefaultAsync(l => l.LocationId == id);

            if (location == null)
                return NotFound("Location not found.");

            // Check whether another organiser owns an event
            // using this location.
            bool belongsToAnotherOrganizer =
                await _context.Events.AnyAsync(e =>
                    e.LocationId == id &&
                    e.OrganizerId != userId);

            if (belongsToAnotherOrganizer)
                return Forbid();

            location.City = request.City;
            location.Province = request.Province;
            location.Venue = request.Venue;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Location updated successfully.",
                location.LocationId,
                location.City,
                location.Province,
                location.Venue
            });
        }

        // ====================================================================================
        // DELETE: api/locations/5
        // Only Organisers can delete locations.

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var location = await _context.EventLocations
                .FirstOrDefaultAsync(l => l.LocationId == id);

            if (location == null)
                return NotFound("Location not found.");

            // Prevent deleting a location used by
            // another organiser's event.
            bool belongsToAnotherOrganizer =
                await _context.Events.AnyAsync(e =>
                    e.LocationId == id &&
                    e.OrganizerId != userId);

            if (belongsToAnotherOrganizer)
                return Forbid();

            // Prevent deleting a location currently
            // associated with any event.
            bool locationInUse = await _context.Events
                .AnyAsync(e => e.LocationId == id);

            if (locationInUse)
                return Conflict(
                    "This location is currently being used by an event.");

            _context.EventLocations.Remove(location);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Location deleted successfully."
            });
        }

        // ========================================== SESSION CHECKS ==========================================

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetInt32("UserId") != null
                && HttpContext.Session.GetString("Role") != null;
        }

        private bool IsOrganizer()
        {
            return HttpContext.Session.GetString("Role") == "Organizer";
        }

        // ========================================== REQUEST MODEL ==========================================

        public class LocationRequest
        {
            [Required, StringLength(50)]
            public string City { get; set; } = string.Empty;

            [Required, StringLength(50)]
            public string Province { get; set; } = string.Empty;

            [Required, StringLength(100)]
            public string Venue { get; set; } = string.Empty;
        }
    }
}
