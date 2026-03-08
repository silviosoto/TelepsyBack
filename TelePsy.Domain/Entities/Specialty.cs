using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TelePsy.Domain.Entities
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public ICollection<PsychologistSpecialty> PsychologistSpecialties { get; set; } = new List<PsychologistSpecialty>();
    }
}
