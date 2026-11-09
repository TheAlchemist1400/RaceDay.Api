using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        // Foreign Key
        public int RegistrationId { get; set; }


        public TimeOnly? FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int? Position { get; set; }

        [Required, StringLength(20)]
        public string ResultStatus { get; set; } = String.Empty;


        public Registration Registration { get; set; } = null!;
    }
}
