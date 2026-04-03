using System;

namespace TelePsy.Domain.DTOs
{
    public class AdminPaymentViewDto
    {
        public int PaymentId { get; set; }
        public DateTime Date { get; set; }
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientIdentification { get; set; } // Map to a placeholder or real field if available
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? TransactionId { get; set; }
        public int AppointmentId { get; set; }
        public string? TherapyName { get; set; }
        public string? PsychologistName { get; set; }
        public string? PsychologistPaymentAccount { get; set; }
    }
}
