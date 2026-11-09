using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Controllers
{
    public class ParticipantsController : Controller
    {
        private static readonly List<Participant> Participants = new List<Participant>
        {
            new Participant
            {
                ParticipantId = 1,
                Name = "Amando",
                Surname = "Moabi",
                Email = "ComradesMA@gmail.com'",
                PasswordHash = "Password123",
                PhoneNumber = "0826312561"
            },

            new Participant
            {
                ParticipantId = 2,
                Name = "Thabo",
                Surname = "Sky",
                Email = "CapeTownCTT@gmail.com",
                PasswordHash = "Password321",
                PhoneNumber = "0824562354"
            }
        };
    }
}
