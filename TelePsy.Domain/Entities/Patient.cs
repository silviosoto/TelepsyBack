using System;
using System.Collections.Generic;

namespace TelePsy.Domain.Entities
{
    public class Patient
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
        
        public bool IsActive { get; set; } = true;
        public string Occupation { get; set; }
        public string EmergencyContact { get; set; }
        
        // Match preferences
        public string PreferredGender { get; set; }
        public string Interests { get; set; } // Comma separated or JSON
        
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<ClinicalRecord> ClinicalRecords { get; set; }
    }
}
