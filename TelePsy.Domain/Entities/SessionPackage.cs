using System;
using System.Collections.Generic;

namespace TelePsy.Domain.Entities
{
    public class SessionPackage
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int PsychologistId { get; set; }
        public Psychologist? Psychologist { get; set; }

        public int TherapyId { get; set; }
        public Therapy? Therapy { get; set; }

        public decimal OriginalTotalAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal FinalAmount { get; set; }

        public int TotalSessions { get; set; }
        public int UsedSessions { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment>? Appointments { get; set; }
    }
}
