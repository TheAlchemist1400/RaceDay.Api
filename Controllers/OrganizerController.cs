using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizerController : ControllerBase
    {
        //private static readonly List<EventOrganizer> Organizers = new List<EventOrganizer>
        //{
        //    new EventOrganizer
        //    {
        //        OrganizerId = 1,
        //        OrganizationName = "Comrades Marathon Association",
        //        Email = "ComradesMA@gmail.com'",
        //        PasswordHash = "Password123",
        //        PhoneNumber = "0826312561"
        //    },

        //    new EventOrganizer
        //    {
        //        OrganizerId = 2,
        //        OrganizationName = "Cape Town Cycle Tour Trust",
        //        Email = "CapeTownCTT@gmail.com",
        //        PasswordHash = "Password321",
        //        PhoneNumber = "0824562354"
        //    }
        //};


        private readonly RaceDayDbContext _context;
        public OrganizerController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Returns All organizers
        [HttpGet]
        public async Task<ActionResult<List<EventOrganizer>>> GetOrganizer()
        {
            return Ok(await _context.EventOrganizers.ToListAsync());
        }

        //Return an organizer
       [HttpGet("{id}")]
        public async Task<ActionResult<List<EventOrganizer>>> GetOrganizerbyId(int id)
        {
            var EventOrganizer = await _context.EventOrganizers.FindAsync(id);

            // Returns 404
            if (EventOrganizer == null) return NotFound();

            return Ok(EventOrganizer);
        }

        // Creates an Organizer
        [HttpPost]
        public async Task<ActionResult<EventOrganizer>> addOrganizer(EventOrganizer newOrganizer)
        {
            // Rturns 400
            if (newOrganizer == null) return BadRequest();

            // Returns 201
            _context.EventOrganizers.Add(newOrganizer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganizerbyId), new { id = newOrganizer.OrganizerId }, newOrganizer);
        }

        // Editsban Organizer
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrganizer(int id, EventOrganizer updatedOrganizer)
        {
            var EventOrganizer = await _context.EventOrganizers.FindAsync(id);

            // Returns 404
            if (EventOrganizer == null) return NotFound();

            EventOrganizer.OrganizationName = updatedOrganizer.OrganizationName;
            EventOrganizer.Email = updatedOrganizer.Email;
            EventOrganizer.PasswordHash = updatedOrganizer.PasswordHash;
            EventOrganizer.PhoneNumber = updatedOrganizer.PhoneNumber;

            await _context.SaveChangesAsync();

            // Return 204
            return NoContent();
        }

        // Delete Organizer
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrganizer(int id)
        {
            var EventOrganizer = await _context.EventOrganizers.FindAsync(id);

            // Returns 404
            if (EventOrganizer == null) return NotFound();

            _context.EventOrganizers.Remove(EventOrganizer);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
