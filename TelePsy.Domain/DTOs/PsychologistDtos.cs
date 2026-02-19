using System.ComponentModel.DataAnnotations;

namespace TelePsy.Domain.DTOs
{
    public class PsychologistProfileDto
    {
        [Required]
        public string LicenseNumber { get; set; }
        
        public string Specialization { get; set; }
        public string University { get; set; }
        public int ExperienceYears { get; set; }
        public decimal SessionRate { get; set; }
        public string Bio { get; set; }
        public string Hobbies { get; set; }
    }

    public class TherapyTypeDto
    {
        public int TherapyTypeId { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }
}
