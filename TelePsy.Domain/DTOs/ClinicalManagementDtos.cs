using System;
using System.Collections.Generic;

namespace TelePsy.Domain.DTOs
{
    // ==========================================
    // NIVEL 1: HISTORIA CLÍNICA (EXPEDIENTE MADRE)
    // ==========================================
    public class ClinicalHistoryMentalHealthDto
    {
        public string CognitiveDisability { get; set; } = string.Empty;
        public string Epilepsy { get; set; } = string.Empty;
        public string HereditaryFamilyDiseases { get; set; } = string.Empty;
        public string PsychoactiveSubstanceConsumption { get; set; } = string.Empty;
        public string AlcoholConsumption { get; set; } = string.Empty;
        public string MentalHealthDiagnosis { get; set; } = string.Empty;
        public string SuicideAttempts { get; set; } = string.Empty;
        public string TraumaticEvents { get; set; } = string.Empty;
        public string IsMedicated { get; set; } = string.Empty;
        public string SelfHarmHistory { get; set; } = string.Empty;
        public string PsychiatricHospitalizations { get; set; } = string.Empty;
        public string MentalHealthObservations { get; set; } = string.Empty;
    }

    public class ClinicalHistoryAcademicDto
    {
        public string SchoolEntryAge { get; set; } = string.Empty;
        public string InitialAdaptation { get; set; } = string.Empty;
        public string AcademicPerformance { get; set; } = string.Empty;
        public string LostYears { get; set; } = string.Empty;
        public string CurrentGrade { get; set; } = string.Empty;
        public string EasyAndDifficultSubjects { get; set; } = string.Empty;
        public string DailyStudyTimeHours { get; set; } = string.Empty;
    }

    public class ClinicalHistoryPsychosexualDto
    {
        public string DevelopmentAge { get; set; } = string.Empty;
        public string SexualIdentity { get; set; } = string.Empty;
        public string SexualOrientation { get; set; } = string.Empty;
        public string AgeOfFirstSexualIntercourse { get; set; } = string.Empty;
        public string FamilyPlanningMethodsUse { get; set; } = string.Empty;
        public string HasBeenSexuallyAbused { get; set; } = string.Empty;
        public string SexuallyTransmittedDiseases { get; set; } = string.Empty;
        public string TeenageParent { get; set; } = string.Empty;
    }

    public class ClinicalHistoryDto
    {
        public int PatientId { get; set; }
        public ClinicalHistoryMentalHealthDto MentalHealth { get; set; } = new();
        public ClinicalHistoryAcademicDto Academic { get; set; } = new();
        public ClinicalHistoryPsychosexualDto Psychosexual { get; set; } = new();
        public DateTime? LastUpdatedAt { get; set; }
    }

    // ========================================================
    // NIVEL 2: REGISTRO DE ATENCIÓN (VALORACIÓN INICIAL SERVICIO)
    // ========================================================
    public class PhysicalExamDto
    {
        public string Lacerations { get; set; } = string.Empty;
        public string Hematomas { get; set; } = string.Empty;
        public string Burns { get; set; } = string.Empty;
        public string Scars { get; set; } = string.Empty;
        public string Fractures { get; set; } = string.Empty;
        public string PhysicalObservations { get; set; } = string.Empty;
    }

    public class RelatedFactorsDto
    {
        public string FamilyEnvironmentRelated { get; set; } = string.Empty;
        public string SocialEnvironmentRelated { get; set; } = string.Empty;
        public string AcademicEnvironmentRelated { get; set; } = string.Empty;
        public string IndividualCharacteristicsRelated { get; set; } = string.Empty;
    }

    public class MentalStateExamDto
    {
        public string ConsciousnessLevel { get; set; } = string.Empty;
        public string Attention { get; set; } = string.Empty;
        public string SensoryPerception { get; set; } = string.Empty;
        public string Affect { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Orientation { get; set; } = string.Empty;
        public string Sleep { get; set; } = string.Empty;
        public string Thought { get; set; } = string.Empty;
        public string MotorConduct { get; set; } = string.Empty;
        public string Memory { get; set; } = string.Empty;
        public string FeedingPattern { get; set; } = string.Empty;
        public string Intelligence { get; set; } = string.Empty;
        public string ReasoningLevel { get; set; } = string.Empty;
        public string PostureAndAttitude { get; set; } = string.Empty;
    }

    public class InitialConceptDto
    {
        public string Analysis { get; set; } = string.Empty;
        public string MainDiagnosis { get; set; } = string.Empty;
        public string TreatmentPlan { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public string Commitments { get; set; } = string.Empty;
    }

    public class CreateServiceAttentionRecordDto
    {
        public int PatientId { get; set; }
        public int TherapyId { get; set; }
        public int? InitialAppointmentId { get; set; }
        public string Status { get; set; } = "EnTratamiento";

        public PhysicalExamDto PhysicalExam { get; set; } = new();
        public RelatedFactorsDto RelatedFactors { get; set; } = new();
        public MentalStateExamDto MentalStateExam { get; set; } = new();
        public InitialConceptDto InitialConcept { get; set; } = new();
    }

    public class UpdateServiceAttentionRecordDto
    {
        public string Status { get; set; } = "EnTratamiento";
        public PhysicalExamDto PhysicalExam { get; set; } = new();
        public RelatedFactorsDto RelatedFactors { get; set; } = new();
        public MentalStateExamDto MentalStateExam { get; set; } = new();
        public InitialConceptDto InitialConcept { get; set; } = new();
    }

    public class ServiceAttentionRecordDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PsychologistId { get; set; }
        public string PsychologistName { get; set; } = string.Empty;
        public int TherapyId { get; set; }
        public string TherapyName { get; set; } = string.Empty;
        public int? InitialAppointmentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        public PhysicalExamDto PhysicalExam { get; set; } = new();
        public RelatedFactorsDto RelatedFactors { get; set; } = new();
        public MentalStateExamDto MentalStateExam { get; set; } = new();
        public InitialConceptDto InitialConcept { get; set; } = new();

        public int EvolutionNotesCount { get; set; }
    }

    // ========================================================
    // NIVEL 3: NOTAS DE EVOLUCIÓN (SEGUIMIENTO POR SESIÓN)
    // ========================================================
    public class CreateEvolutionNoteDto
    {
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public int? ServiceAttentionRecordId { get; set; }
        public int SessionNumber { get; set; }
        public string ReasonForSession { get; set; } = string.Empty; // Observaciones de sesión / Subjetivo
        public string Interventions { get; set; } = string.Empty; // Intervenciones y técnicas
        public string FocusedMentalExam { get; set; } = string.Empty; // Examen mental focalizado
        public string Evolution { get; set; } = string.Empty; // Apreciación y evolución diagnóstica
        public string TherapeuticPlan { get; set; } = string.Empty; // Compromisos / Tareas en casa
        public DateTime? NextAppointmentDate { get; set; }
        public string ProfessionalSignature { get; set; } = string.Empty;
    }

    public class EvolutionNoteDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PsychologistId { get; set; }
        public string PsychologistName { get; set; } = string.Empty;
        public int? AppointmentId { get; set; }
        public int? ServiceAttentionRecordId { get; set; }
        public int? TherapyId { get; set; }
        public string? TherapyName { get; set; }
        public DateTime Date { get; set; }
        public int SessionNumber { get; set; }
        public string ReasonForSession { get; set; } = string.Empty;
        public string Interventions { get; set; } = string.Empty;
        public string FocusedMentalExam { get; set; } = string.Empty;
        public string Evolution { get; set; } = string.Empty;
        public string TherapeuticPlan { get; set; } = string.Empty;
        public DateTime? NextAppointmentDate { get; set; }
        public string ProfessionalSignature { get; set; } = string.Empty;
    }
}
