using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizerController : ControllerBase
    {
        private static readonly List<EventOrganizer> Organizers = new List<EventOrganizer>
        {
            new EventOrganizer
            {
                OrganizerId = 1,
                OrganizationName = "Comrades Marathon Association",
                Email = "ComradesMA@gmail.com'",
                PasswordHash = "Password123",
                PhoneNumber = "0826312561"
            },

            new EventOrganizer
            {
                OrganizerId = 2,
                OrganizationName = "Cape Town Cycle Tour Trust",
                Email = "CapeTownCTT@gmail.com",
                PasswordHash = "Password321",
                PhoneNumber = "0824562354"
            }
        };

        // Returns All organizers
        [HttpGet]
        public ActionResult<List<EventOrganizer>> GetOrganizer()
        {
            return Ok(Organizers);
        }

        // Return an organizer
        [HttpGet("{id}")]
        public ActionResult<List<EventOrganizer>> GetOrganizerbyId(int id)
        {
            var EventOrganizer = Organizers.FirstOrDefault(Org => Org.OrganizerId == id);

            // Returns 404
            if (EventOrganizer == null) return NotFound();

            return Ok(EventOrganizer);
        }

        // Creates an Organizer
        [HttpPost]
        public ActionResult<EventOrganizer> addOrganizer(EventOrganizer newOrganizer)
        {
            // Rturns 400
            if (newOrganizer == null) return BadRequest();

            // Returns 201
            Organizers.Add(newOrganizer);
            return CreatedAtAction(nameof(GetOrganizerbyId), new { id = newOrganizer.OrganizerId }, newOrganizer);
        }

        // Editsban Organizer
        [HttpPut("{id}")]
        public ActionResult UpdateOrganizer(int id, EventOrganizer updatedOrganizer)
        {
            var EventOrganizer = Organizers.FirstOrDefault(Org => Org.OrganizerId == id);

            // Returns 404
            if (EventOrganizer == null) return NotFound();

            EventOrganizer.OrganizationName = updatedOrganizer.OrganizationName;
            EventOrganizer.Email = updatedOrganizer.Email;
            EventOrganizer.PasswordHash = updatedOrganizer.PasswordHash;
            EventOrganizer.PhoneNumber = updatedOrganizer.PhoneNumber;

            // Return 204
            return NoContent();
        }

        // Delete Organizer
        [HttpDelete("{id}")]
        public ActionResult DeleteOrganizer(int id)
        {
            var EventOrganizer = Organizers.FirstOrDefault(Org => Org.OrganizerId == id);

            // Returns 404
            if (EventOrganizer == null) return NotFound();

            Organizers.Remove(EventOrganizer);
            return NoContent();
       }
    }
}
