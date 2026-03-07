using System;

namespace TelePsy.Domain.Entities
{
    public class PsychologyNote
    {
        public int Id { get; set; }
        
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        
        public int PsychologistId { get; set; }
        public Psychologist? Psychologist { get; set; }

        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public DateTime Date { get; set; }
        public int SessionNumber { get; set; }
        public string ReasonForSession { get; set; } = string.Empty;
        public string Evolution { get; set; } = string.Empty;
        public string Interventions { get; set; } = string.Empty;
        public string TherapeuticPlan { get; set; } = string.Empty;
        public DateTime? NextAppointmentDate { get; set; }
        public string ProfessionalSignature { get; set; } = string.Empty;
    }
}
