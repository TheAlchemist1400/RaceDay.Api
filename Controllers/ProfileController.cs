
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;

        public ProfileController(
            RaceDayDbContext context,
            BlobStorageService blobStorageService,
            IConfiguration configuration)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
        }

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            if (role == "Participant")
            {
                var participant = await _context.Participants.FindAsync(userId.Value);

                if (participant == null)
                    return NotFound("Participant not found.");

                return Ok(new
                {
                    participant.ParticipantId,
                    participant.Name,
                    participant.Lastname,
                    participant.Email,
                    participant.PhoneNumber,
                    participant.ProfileImageUrl,
                    Role = "Participant"
                });
            }

            if (role == "Organizer")
            {
                var organizer = await _context.EventOrganizers.FindAsync(userId.Value);

                if (organizer == null)
                    return NotFound("Organizer not found.");

                return Ok(new
                {
                    organizer.OrganizerId,
                    organizer.OrganizationName,
                    organizer.Email,
                    organizer.PhoneNumber,
                    Role = "Organizer"
                });
            }

            return Forbid();
        }

        // PUT: api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            // Update Participant profile
            if (role == "Participant")
            {
                var participant = await _context.Participants.FindAsync(userId.Value);

                if (participant == null)
                    return NotFound("Participant not found.");

                if (await _context.Participants.AnyAsync(p => p.Email == request.Email && p.ParticipantId != userId.Value) ||
                    await _context.EventOrganizers.AnyAsync(o => o.Email == request.Email))
                {
                    return Conflict("Email address already exists.");
                }

                participant.Name = request.Name;
                participant.Lastname = request.Lastname;
                participant.Email = request.Email;
                participant.PhoneNumber = request.PhoneNumber;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Participant profile updated successfully.",
                    participant.ParticipantId,
                    participant.Name,
                    participant.Lastname,
                    participant.Email,
                    participant.PhoneNumber
                });
            }

            // Update Organizer profile
            if (role == "Organizer")
            {
                var organizer = await _context.EventOrganizers.FindAsync(userId.Value);

                if (organizer == null)
                    return NotFound("Organizer not found.");

                if (await _context.EventOrganizers.AnyAsync(o => o.Email == request.Email && o.OrganizerId != userId.Value) ||
                    await _context.Participants.AnyAsync(p => p.Email == request.Email))
                {
                    return Conflict("Email address already exists.");
                }

                organizer.OrganizationName = request.OrganizationName;
                organizer.Email = request.Email;
                organizer.PhoneNumber = request.PhoneNumber;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Organizer profile updated successfully.",
                    organizer.OrganizerId,
                    organizer.OrganizationName,
                    organizer.Email,
                    organizer.PhoneNumber
                });
            }

            return Forbid();
        }


        // POST: api/profile/upload-image
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024 + 65536)]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            // STEP 1: Check the API session.
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
                return Unauthorized("Please log in first.");

            // STEP 2: Only Participants can upload profile pictures.
            if (role != "Participant")
                return StatusCode(403, "Only Participants can upload profile pictures.");

            // STEP 3: Retrieve the Participant.
            var participant = await _context.Participants.FindAsync(
                userId.Value
            );

            if (participant == null)
                return NotFound("Participant not found.");

            // STEP 4: Get the configured Azure container.
            string? containerName =
                _configuration["AzureBlobStorage:ProfileImagesContainer"];

            if (string.IsNullOrWhiteSpace(containerName))
            {
                return StatusCode(
                    500,
                    "Profile image container is not configured."
                );
            }

            try
            {
                // STEP 5: Upload image to Azure.
                string imageUrl = await _blobStorageService.UploadImageAsync(
                    file,
                    containerName
                );

                // STEP 6: Save the image reference in SQL Server.
                participant.ProfileImageUrl = imageUrl;

                await _context.SaveChangesAsync();

                // STEP 7: Return the result.
                return Ok(new
                {
                    message = "Profile picture uploaded successfully.",
                    participant.ParticipantId,
                    participant.ProfileImageUrl
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Request model
        public class UpdateProfileRequest
        {
            public string Name { get; set; } = string.Empty;

            public string Lastname { get; set; } = string.Empty;

            public string OrganizationName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string PhoneNumber { get; set; } = string.Empty;
        }
    }
}
