using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class EventOrganizer
    {
        [Key]
        public int OrganizerId { get; set; }

        [Required, StringLength(100)]
        public string OrganizationName { get; set; } = string.Empty;
        
        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

    }
}
