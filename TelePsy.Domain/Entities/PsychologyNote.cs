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

        public int? ServiceAttentionRecordId { get; set; }
        public ServiceAttentionRecord? ServiceAttentionRecord { get; set; }

        public DateTime Date { get; set; }
        public int SessionNumber { get; set; }
        public string ReasonForSession { get; set; } = string.Empty; // Observaciones de la sesión / Subjetivo
        public string FocusedMentalExam { get; set; } = string.Empty; // Examen mental focalizado
        public string Evolution { get; set; } = string.Empty; // Apreciación y evolución diagnóstica
        public string Interventions { get; set; } = string.Empty; // Intervenciones y técnicas aplicadas
        public string TherapeuticPlan { get; set; } = string.Empty; // Compromisos / Plan / Tareas en casa
        public DateTime? NextAppointmentDate { get; set; }
        public string ProfessionalSignature { get; set; } = string.Empty;
    }
}
