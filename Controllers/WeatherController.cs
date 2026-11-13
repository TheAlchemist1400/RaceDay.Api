
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api/weather")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public WeatherController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ====================================================================================
        // GET: api/weather/event/5
        // Both roles can view event weather.

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventWeather(int eventId)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsValidRole())
                return Forbid();

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var weather = await _context.WeatherRecords
                .Where(w => w.EventId == eventId)
                .Select(w => new
                {
                    w.WeatherId,
                    w.EventId,
                    w.Temperature,
                    w.WeatherCondition,
                    w.WindSpeed
                })
                .FirstOrDefaultAsync();

            if (weather == null)
                return NotFound("Weather information not found.");

            return Ok(weather);
        }

        // ====================================================================================
        // POST: api/weather/event/5
        // Only the event owner can add weather.

        [HttpPost("event/{eventId}")]
        public async Task<IActionResult> CreateWeather(
            int eventId,
            WeatherRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            if (eventItem.OrganizerId != userId)
                return Forbid();

            // One weather record per event.
            bool weatherExists = await _context.WeatherRecords
                .AnyAsync(w => w.EventId == eventId);

            if (weatherExists)
                return Conflict(
                    "Weather information already exists for this event.");

            var weather = new Weather
            {
                EventId = eventId,
                Temperature = request.Temperature,
                WeatherCondition = request.WeatherCondition,
                WindSpeed = request.WindSpeed
            };

            _context.WeatherRecords.Add(weather);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Weather information created successfully.",
                weather.WeatherId,
                weather.EventId,
                weather.Temperature,
                weather.WeatherCondition,
                weather.WindSpeed
            });
        }

        // ====================================================================================
        // PUT: api/weather/5
        // Only the event owner can update weather.

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeather(
            int id,
            WeatherRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var weather = await _context.WeatherRecords
                .Include(w => w.Event)
                .FirstOrDefaultAsync(w => w.WeatherId == id);

            if (weather == null)
                return NotFound("Weather information not found.");

            if (weather.Event.OrganizerId != userId)
                return Forbid();

            weather.Temperature = request.Temperature;
            weather.WeatherCondition = request.WeatherCondition;
            weather.WindSpeed = request.WindSpeed;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Weather information updated successfully.",
                weather.WeatherId,
                weather.EventId,
                weather.Temperature,
                weather.WeatherCondition,
                weather.WindSpeed
            });
        }

        // ====================================================================================
        // DELETE: api/weather/5
        // Only the event owner can delete weather.

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeather(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized("Please log in first.");

            if (!IsOrganizer())
                return Forbid();

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var weather = await _context.WeatherRecords
                .Include(w => w.Event)
                .FirstOrDefaultAsync(w => w.WeatherId == id);

            if (weather == null)
                return NotFound("Weather information not found.");

            if (weather.Event.OrganizerId != userId)
                return Forbid();

            _context.WeatherRecords.Remove(weather);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Weather information deleted successfully."
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
        public class WeatherRequest
        {
            [Required]
            [Range(typeof(decimal), "-100.0", "100.0",
                ParseLimitsInInvariantCulture = true,
                ConvertValueInInvariantCulture = true)]
            public decimal? Temperature { get; set; }

            [Required, StringLength(20)]
            public string? WeatherCondition { get; set; }

            [Range(typeof(decimal), "0", "999.9",
                ParseLimitsInInvariantCulture = true,
                ConvertValueInInvariantCulture = true)]
            public decimal? WindSpeed { get; set; }
        }
    }
}
