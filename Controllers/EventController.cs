
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using System.ComponentModel.DataAnnotations;


namespace RaceDay.Api.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;

        public EventsController(
            RaceDayDbContext context,
            BlobStorageService blobStorageService,
            IConfiguration configuration)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
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
                    e.BannerImageUrl,
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
                    e.BannerImageUrl,
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
                    e.BannerImageUrl,
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

        // POST: api/events/5/upload-banner
        [HttpPost("{id:int}/upload-banner")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024 + 65536)]
        public async Task<IActionResult> UploadEventBanner(
            int id,
            IFormFile file)
        {
            // STEP 1: Check the Organizer's session.
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role != "Organizer")
                return StatusCode(403, "Only Organizers can upload banners.");

            // STEP 2: Find the event.
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound("Event not found.");

            // STEP 3: Verify event ownership.
            if (eventItem.OrganizerId != userId.Value)
                return StatusCode(403, "You do not own this event.");

            // STEP 4: Get the existing Azure container name.
            string? containerName =
                _configuration["AzureBlobStorage:EventBannersContainer"];

            if (string.IsNullOrWhiteSpace(containerName))
            {
                return StatusCode(
                    500,
                    "Event banner container is not configured."
                );
            }

            try
            {
                // STEP 5: Upload the image to Azure.
                string imageUrl = await _blobStorageService.UploadImageAsync(
                    file,
                    containerName
                );

                // STEP 6: Save the image reference in SQL Server.
                eventItem.BannerImageUrl = imageUrl;

                await _context.SaveChangesAsync();

                // STEP 7: Return the result.
                return Ok(new
                {
                    message = "Event banner uploaded successfully.",
                    eventItem.EventId,
                    eventItem.EventsName,
                    eventItem.BannerImageUrl
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/events/5/banner
        [HttpGet("{id:int}/banner")]
        [ResponseCache(
            NoStore = true,
            Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetEventBanner(int id)
        {
            // STEP 1: Find the event.
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound("Event not found.");

            // STEP 2: Check whether a banner exists.
            if (string.IsNullOrWhiteSpace(eventItem.BannerImageUrl))
                return NotFound("This event has no banner.");

            // STEP 3: Get the Azure container name.
            string? containerName =
                _configuration["AzureBlobStorage:EventBannersContainer"];

            if (string.IsNullOrWhiteSpace(containerName))
                return StatusCode(500, "Event banner container is not configured.");

            try
            {
                // STEP 4: Download from private Azure Blob Storage.
                var (content, contentType) =
                    await _blobStorageService.DownloadImageAsync(
                        eventItem.BannerImageUrl,
                        containerName
                    );

                // STEP 5: Return the image.
                return File(content, contentType);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound("Event banner not found in Azure.");
            }
            catch (ArgumentException)
            {
                return StatusCode(500, "Stored banner reference is invalid.");
            }
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
