using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Event : Controller
    {
        [Key]
        public int EventId { get; set; }

        [Required, StringLength(50)]
        public string EventsName { get; set; } = string.Empty;

        [Required]
        public DateOnly EventDate { get; set; }

        [Required, StringLength(50)]
        public string Activity { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int MaxParticipants { get; set; }

        // Foreign Keys
        public int OrganizerId { get; set; }

        public int LocationId { get; set; }

        public EventOrganizer Organizer { get; set; } = null!;

        public EventLocation Location { get; set; } = null!;



    }
}
