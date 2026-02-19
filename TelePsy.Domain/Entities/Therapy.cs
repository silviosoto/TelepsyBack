using System.Collections.Generic;

namespace TelePsy.Domain.Entities
{
    public class Therapy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<PsychologistTherapy> PsychologistTherapies { get; set; }
    }
}
