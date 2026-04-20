using System;
using TelePsy.Domain.Enums;

namespace TelePsy.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int PsychologistId { get; set; }
        public Psychologist Psychologist { get; set; }

        public int TherapyId { get; set; }
        public Therapy Therapy { get; set; }

        public decimal Rate { get; set; } // Historical rate at the moment of booking

        public DateTime ScheduledTime { get; set; }
        public int DurationMinutes { get; set; } = 45;

        public AppointmentStatus Status { get; set; } // Pending, Confirmed, Completed, Cancelled

        public string VideoLink { get; set; }

        public DateTime? PatientJoinedAt { get; set; }
        public DateTime? PsychologistJoinedAt { get; set; }

        public int? PaymentId { get; set; }
        public Payment Payment { get; set; }

        public int? PsychologistInvoiceId { get; set; }
        public Invoice PsychologistInvoice { get; set; }

        public int? SessionPackageId { get; set; }
        public SessionPackage? SessionPackage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
