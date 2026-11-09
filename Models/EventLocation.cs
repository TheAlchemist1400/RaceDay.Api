using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class EventLocation
    {
        [Key]
        public int LocationId { get; set; }

        [Required, StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Province { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Venue { get; set; } = string.Empty;

    }
}
