using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class RouteInfo
    {
        [Key]
        public int RouteId { get; set; }

        // Foreign Key
        public int EventId { get; set; }

        [Range(typeof(decimal), "0.01", "999999.99",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public decimal DistanceKm { get; set; }

        [Required, StringLength(100)]
        public string StartPoint { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string EndPoint { get; set; } = string.Empty;
        
        [Required, StringLength(500)]
        public string RouteDescription { get; set; } = string.Empty;

        public Event Event { get; set; } = null!;

    }
}
