using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Lastname { get; set; } = string.Empty;

        [Required, EmailAddress ,StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        // Nullable because existing Participants have no profile picture.
        [StringLength(2048)]
        public string? ProfileImageUrl { get; set; }
    }
}
