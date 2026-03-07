using System;

namespace TelePsy.Domain.Entities
{
    public class ClinicalRecord
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        
        public int PsychologistId { get; set; }
        public Psychologist? Psychologist { get; set; } // Author of the record
        
        public DateTime Date { get; set; }
        public string Notes { get; set; } // This should be encrypted in BLL/DAL before saving
        public string Diagnosis { get; set; } // Encrypted
        public string TreatmentPlan { get; set; } // Encrypted
    }
}
