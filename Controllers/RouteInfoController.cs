
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api/routes")]
    [ApiController]
    public class RouteInfoController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RouteInfoController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ====================================================================================
        // GET: api/routes/event/5
        // Both roles can view event routes.

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventRoute(int eventId)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsValidRole())
                return Forbid();

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var route = await _context.RouteInfos
                .Where(r => r.EventId == eventId)
                .Select(r => new
                {
                    r.RouteId,
                    r.EventId,
                    r.DistanceKm,
                    r.StartPoint,
                    r.EndPoint,
                    r.RouteDescription
                })
                .FirstOrDefaultAsync();

            if (route == null)
                return NotFound("Route information not found.");

            return Ok(route);
        }

        // ====================================================================================
        // POST: api/routes/event/5
        // Only the event owner can create a route.

        [HttpPost("event/{eventId}")]
        public async Task<IActionResult> CreateRoute(
            int eventId,
            RouteRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganizerId !=
                HttpContext.Session.GetInt32("UserId"))
                return Forbid();

            // Only one route is allowed per event.
            bool routeExists = await _context.RouteInfos
                .AnyAsync(r => r.EventId == eventId);

            if (routeExists)
                return Conflict(
                    "Route information already exists for this event.");

            var route = new RouteInfo
            {
                EventId = eventId,
                DistanceKm = request.DistanceKm,
                StartPoint = request.StartPoint,
                EndPoint = request.EndPoint,
                RouteDescription = request.RouteDescription
            };

            _context.RouteInfos.Add(route);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Route information created successfully.",
                route.RouteId,
                route.EventId,
                route.DistanceKm,
                route.StartPoint,
                route.EndPoint,
                route.RouteDescription
            });
        }

        // ====================================================================================
        // PUT: api/routes/5
        // Only the event owner can update a route.

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(
            int id,
            RouteRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var route = await _context.RouteInfos
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RouteId == id);

            if (route == null)
                return NotFound("Route information not found.");

            if (route.Event.OrganizerId != userId)
                return Forbid();

            route.DistanceKm = request.DistanceKm;
            route.StartPoint = request.StartPoint;
            route.EndPoint = request.EndPoint;
            route.RouteDescription = request.RouteDescription;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Route information updated successfully.",
                route.RouteId,
                route.EventId,
                route.DistanceKm,
                route.StartPoint,
                route.EndPoint,
                route.RouteDescription
            });
        }

        // ====================================================================================
        // DELETE: api/routes/5
        // Only the event owner can delete a route.

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var route = await _context.RouteInfos
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RouteId == id);

            if (route == null)
                return NotFound("Route information not found.");

            if (route.Event.OrganizerId != userId)
                return Forbid();

            _context.RouteInfos.Remove(route);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Route information deleted successfully."
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

        private bool IsValidRole()
        {
            string? role = HttpContext.Session.GetString("Role");

            return role == "Participant" || role == "Organizer";
        }

        // ========================================== REQUEST MODEL ==========================================

        public class RouteRequest
        {
            [Range(typeof(decimal), "0.01", "999999.99")]
            public decimal DistanceKm { get; set; }

            [Required, StringLength(100)]
            public string StartPoint { get; set; } = string.Empty;

            [Required, StringLength(100)]
            public string EndPoint { get; set; } = string.Empty;

            [Required, StringLength(500)]
            public string RouteDescription { get; set; } = string.Empty;
        }
    }
}
