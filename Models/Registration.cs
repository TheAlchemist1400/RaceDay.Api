using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }


        // Foreign Key
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int ActivityId { get; set; }


        [Required]
        public DateOnly RegistrationDate { get; set; }

        [Range(1, int.MaxValue)]
        public int BibNumber { get; set; }

        [Required, StringLength(20)]
        public string RgistrationStatus { get; set; } = "Confirmed";

        
        public Participant Participant { get; set; } = null!;
        public Event Event { get; set; } = null!;
        public Activity Activity { get; set; } = null!;

    }
}
