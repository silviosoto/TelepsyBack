using System;
using System.Collections.Generic;

namespace TelePsy.Domain.Entities
{
    public class ServiceAttentionRecord
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int PsychologistId { get; set; }
        public Psychologist? Psychologist { get; set; }

        public int TherapyId { get; set; }
        public Therapy? Therapy { get; set; }

        public int? InitialAppointmentId { get; set; }
        public Appointment? InitialAppointment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }

        // Status: EnTratamiento, EnEvaluacion, Finalizado, Suspendido
        public string Status { get; set; } = "EnTratamiento";

        // Encrypted JSON payload for initial evaluation:
        // - Examen físico / somático
        // - Factores relacionados (familiar, social, académico, individual)
        // - Examen del Estado Mental completo
        // - Concepto psicológico de ingreso (análisis, objetivos, recomendaciones, compromisos)
        public string EvaluationData { get; set; } = string.Empty;

        // Structured searchable fields
        public string MainDiagnosis { get; set; } = string.Empty;
        public string TreatmentPlan { get; set; } = string.Empty;

        // Evolution notes (Level 3) for this therapy process
        public ICollection<PsychologyNote> EvolutionNotes { get; set; } = new List<PsychologyNote>();
    }
}
