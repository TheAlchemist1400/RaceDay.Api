using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Activity
    {

        [Key]
        public int ActivityId { get; set; }

        // Foreign Key
        public int EventId { get; set; }

        [Required, StringLength(100)]
        public string ActivityName { get; set; } = string.Empty;

        [Required, Range(typeof(decimal), "0.01", "999999.99")]
        public decimal DistanceKm { get; set; }

        [Required, Range(typeof(decimal), "0", "999999.99")]
        public decimal EntryFee { get; set; }

        public TimeOnly StartTime { get; set; }

        [Range(1, int.MaxValue)]
        public int MaximumEntrants { get; set; }

        public Event Event { get; set; } = null!;
    }
}
