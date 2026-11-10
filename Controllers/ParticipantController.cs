using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace RaceDay.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantController : ControllerBase
    {
        //private static readonly List<Participant> Participants = new List<Participant>
        //{
        //    new Participant
        //    {
        //        ParticipantId = 1,
        //        Name = "Amando",
        //        Lastname = "Moabi",
        //        Email = "ComradesMA@gmail.com'",
        //        PasswordHash = "Password123",
        //        PhoneNumber = "0826312561"
        //    },

        //    new Participant
        //    {
        //        ParticipantId = 2,
        //        Name = "Thabo",
        //        Lastname = "Sky",
        //        Email = "CapeTownCTT@gmail.com",
        //        PasswordHash = "Password321",
        //        PhoneNumber = "0824562354"
        //    }
        //};

        private readonly RaceDayDbContext _context;
        public ParticipantController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Returns All participants
        [HttpGet]
        public async Task<ActionResult<List<Participant>>> GetParticipant()
        {
            return Ok(await _context.Participants.ToListAsync());
        }

        //Return an participant
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Participant>>> GetParticipantById(int id)
        {
            var Participant = await _context.Participants.FindAsync(id);

            // Returns 404
            if (Participant == null) return NotFound();

            return Ok(Participant);
        }

        // Creates an participant
        [HttpPost]
        public async Task<ActionResult<Participant>> addParticipant(Participant newParticipant)
        {
            // Rturns 400
            if (newParticipant == null) return BadRequest();

            // Returns 201
            _context.Participants.Add(newParticipant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParticipantById), new { id = newParticipant.ParticipantId }, newParticipant);
        }

        // Edits an participant
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateParticipant(int id, Participant updatedParticipant)
        {
            var Participant = await _context.Participants.FindAsync(id);

            // Returns 404
            if (Participant == null) return NotFound();

            Participant.Name = updatedParticipant.Name;
            Participant.Email = updatedParticipant.Email;
            Participant.PasswordHash = updatedParticipant.PasswordHash;
            Participant.PhoneNumber = updatedParticipant.PhoneNumber;

            await _context.SaveChangesAsync();

            // Return 204
            return NoContent();
        }

        // Delete participant
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteParticipant(int id)
        {
            var Participant = await _context.Participants.FindAsync(id);

            // Returns 404
            if (Participant == null) return NotFound();

            _context.Participants.Remove(Participant);

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
