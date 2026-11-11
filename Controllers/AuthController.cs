using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;
using RaceDay.Contracts;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.AccessControl;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RaceDay.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        private readonly PasswordHasher<Participant> _participantHasher = new PasswordHasher<Participant>();

        private readonly PasswordHasher<EventOrganizer> _organizerHasher = new PasswordHasher<EventOrganizer>();

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ========================================== REGISTER ==========================================
        // POST: api/auth/register

        [HttpPost("register")]
        public async Task<IActionResult> Register(RaceDay.Contracts.RegisterRequest request)
        {
            // Check if email already exists
            bool participantExists = await _context.Participants.AnyAsync(p => p.Email == request.Email);

            bool organizerExists = await _context.EventOrganizers.AnyAsync(o => o.Email == request.Email);

            if (participantExists || organizerExists) return Conflict("Email address already exists.");

            // Register Participant
            if (request.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Lastname) || string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return BadRequest("Please provide all participant details.");
                }

                var participant = new Participant
                {
                    Name = request.Name,
                    Lastname = request.Lastname,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber
                };

                // Hash password
                participant.PasswordHash = _participantHasher.HashPassword(participant, request.Password);

                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();

                return StatusCode(201, new
                {
                    message = "Participant registered successfully.",
                    participant.ParticipantId,
                    participant.Name,
                    participant.Email,
                    Role = "Participant"
                });
            }

            // Register Organizer
            if (request.Role.Equals("Organizer", StringComparison.OrdinalIgnoreCase) || request.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.OrganizationName) || string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return BadRequest("Please provide all organizer details.");
                }

                var organizer = new EventOrganizer
                {
                    OrganizationName = request.OrganizationName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber
                };

                // Hash password
                organizer.PasswordHash = _organizerHasher.HashPassword(organizer, request.Password);

                _context.EventOrganizers.Add(organizer);
                await _context.SaveChangesAsync();

                return StatusCode(201, new
                {
                    message = "Organizer registered successfully.",
                    organizer.OrganizerId,
                    organizer.OrganizationName,
                    organizer.Email,
                    Role = "Organizer"
                });
            }

            return BadRequest("Invalid role selected.");
        }

        // ========================================== LOGIN ==========================================
        // POST: api/auth/login

        [HttpPost("login")]
        public async Task<IActionResult> Login(RaceDay.Contracts.LoginRequest request)
        {
            // Find participant and organizer by email
            var participant = await _context.Participants
                .SingleOrDefaultAsync(p => p.Email == request.Email);

            var organizer = await _context.EventOrganizers
                .SingleOrDefaultAsync(o => o.Email == request.Email);

            // An email must identify only one account
            if (participant != null && organizer != null)
                return Unauthorized("Invalid email or password.");

            // Login Participant
            if (participant != null)
            {
                var result = _participantHasher.VerifyHashedPassword(
                    participant,
                    participant.PasswordHash,
                    request.Password
                );

                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized("Invalid email or password.");

                // Store session information
                HttpContext.Session.SetInt32("UserId", participant.ParticipantId);

                HttpContext.Session.SetString("Role", "Participant");

                return Ok(new
                {
                    Message = "Login successful.",
                    UserId = participant.ParticipantId,
                    DisplayName = participant.Name,
                    Email = participant.Email,
                    Role = "Participant"
                });
            }

            // Login Organizer
            if (organizer != null)
            {
                var result = _organizerHasher.VerifyHashedPassword(
                    organizer,
                    organizer.PasswordHash,
                    request.Password
                );

                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized("Invalid email or password.");

                // Store session information
                HttpContext.Session.SetInt32(
                    "UserId", organizer.OrganizerId);

                HttpContext.Session.SetString(
                    "Role", "Organizer");

                return Ok(new
                {
                    Message = "Login successful.",
                    UserId = organizer.OrganizerId,
                    DisplayName = organizer.OrganizationName,
                    Email = organizer.Email,
                    Role = "Organizer"
                });
            }

            return Unauthorized("Invalid email or password");
        }

        // ========================================== LOGOUT ==========================================
        // POST: api/auth/logout

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();

            return Ok(new
            {
                message = "Logout successful."
            });
        }
    }
}
