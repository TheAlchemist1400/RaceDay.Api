
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ActivitiesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events/5/categories
        // Both roles can view categories.

        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            string? role = HttpContext.Session.GetString("Role");

            if (HttpContext.Session.GetInt32("UserId") == null ||
                role == null)
                return Unauthorized("Please log in first.");

            if (role != "Participant" && role != "Organizer")
                return Forbid();

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var activities = await _context.Activities
                .Where(a => a.EventId == eventId)
                .Select(a => new
                {
                    a.ActivityId,
                    a.EventId,
                    a.ActivityName,
                    a.DistanceKm,
                    a.EntryFee,
                    a.StartTime,
                    a.MaximumEntrants
                })
                .ToListAsync();

            return Ok(activities);
        }

        // POST: api/events/5/categories
        // Only the event owner can create categories.

        [HttpPost("events/{eventId}/categories")]
        public async Task<IActionResult> CreateCategory(
            int eventId,
            ActivityRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
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

            var duplicate = await _context.Activities.AnyAsync(a =>
                a.EventId == eventId &&
                a.ActivityName == request.ActivityName);

            if (duplicate)
                return Conflict("This category already exists for this event.");

            var activity = new Activity
            {
                EventId = eventId,
                ActivityName = request.ActivityName,
                DistanceKm = request.DistanceKm,
                EntryFee = request.EntryFee,
                StartTime = request.StartTime,
                MaximumEntrants = request.MaximumEntrants
            };

            _context.Activities.Add(activity);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Category created successfully.",
                activity.ActivityId,
                activity.EventId,
                activity.ActivityName,
                activity.DistanceKm,
                activity.EntryFee,
                activity.StartTime,
                activity.MaximumEntrants
            });
        }

        // PUT: api/categories/5
        // Only the event owner can update a category.

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            ActivityRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            var activity = await _context.Activities
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound("Category not found.");

            if (activity.Event.OrganizerId != userId.Value)
                return Forbid();

            var duplicate = await _context.Activities.AnyAsync(a =>
                a.EventId == activity.EventId &&
                a.ActivityName == request.ActivityName &&
                a.ActivityId != id);

            if (duplicate)
                return Conflict("This category name already exists.");

            activity.ActivityName = request.ActivityName;
            activity.DistanceKm = request.DistanceKm;
            activity.EntryFee = request.EntryFee;
            activity.StartTime = request.StartTime;
            activity.MaximumEntrants = request.MaximumEntrants;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully.",
                activity.ActivityId,
                activity.EventId,
                activity.ActivityName,
                activity.DistanceKm,
                activity.EntryFee,
                activity.StartTime,
                activity.MaximumEntrants
            });
        }

        // DELETE: api/categories/5
        // Only the event owner can delete a category.

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return Forbid();

            var activity = await _context.Activities
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound("Category not found.");

            if (activity.Event.OrganizerId != userId.Value)
                return Forbid();

            _context.Activities.Remove(activity);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }

        // Request model

        public class ActivityRequest
        {
            [Required, StringLength(100)]
            public string ActivityName { get; set; } = string.Empty;

            [Range(typeof(decimal), "0.01", "999999.99",
                ParseLimitsInInvariantCulture = true,
                ConvertValueInInvariantCulture = true)]
            public decimal DistanceKm { get; set; }

            [Range(typeof(decimal), "0", "999999.99",
                ParseLimitsInInvariantCulture = true,
                ConvertValueInInvariantCulture = true)]
            public decimal EntryFee { get; set; }

            public TimeOnly StartTime { get; set; }

            [Range(1, int.MaxValue)]
            public int MaximumEntrants { get; set; }
        }
    }
}
