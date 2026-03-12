using System.ComponentModel.DataAnnotations;

namespace TelePsy.Domain.DTOs
{
    public class PsychologistProfileDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public string State { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string LicenseNumber { get; set; } = string.Empty;
        
        public string Specialization { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal SessionRate { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string Hobbies { get; set; } = string.Empty;
    }

    public class TherapyTypeDto
    {
        public int TherapyTypeId { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }
}
