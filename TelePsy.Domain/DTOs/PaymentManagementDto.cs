using System;
using System.Collections.Generic;

namespace TelePsy.Domain.DTOs
{
    public class PaymentManagementDto
    {
        public int InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public int PsychologistId { get; set; }
        public string? PsychologistName { get; set; }
        public string? ServiceName { get; set; }
        public decimal TotalAmount { get; set; } // Paid by Patient
        public decimal PsychologistShare { get; set; } // What the psychologist earns
        public decimal PlatformCommission { get; set; } // Platform's cut
        public bool IsPaidToPsychologist { get; set; }
        public int AppointmentId { get; set; }
    }

    public class PsychologistPayoutRequestDto
    {
        public int PsychologistId { get; set; }
        public List<int> AppointmentIds { get; set; } = new List<int>();
    }
}
