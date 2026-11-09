using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Weather
    {
        [Key]
        public int WeatherId { get; set; }

        // Foreign Key
        public int EventId { get; set; }

        [Required]
        public decimal? Temperature { get; set; }

        [Required, StringLength(20)]
        public string? WeatherCondition { get; set; } = string.Empty;

        public decimal? WindSpeed { get; set; }

        public Event Event { get; set; } = null!;

    }
}
