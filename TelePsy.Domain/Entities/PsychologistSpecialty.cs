using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelePsy.Domain.Entities
{
    public class PsychologistSpecialty
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Psychologist")]
        public int PsychologistId { get; set; }
        public Psychologist? Psychologist { get; set; }
        
        [ForeignKey("Specialty")]
        public int SpecialtyId { get; set; }
        public Specialty? Specialty { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
