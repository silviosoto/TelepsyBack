using System;

namespace TelePsy.Domain.DTOs
{
    public class InitiateBookingDto
    {
        public int PsychologistId { get; set; }
        public int TherapyId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public int? PackageSessions { get; set; }
    }

    public class BookingResponseDto
    {
        public int AppointmentId { get; set; }
        public string Message { get; set; }
    }

    public class CheckoutSummaryDto
    {
        public int AppointmentId { get; set; }
        public int InvoiceId { get; set; }
        public string PsychologistName { get; set; }
        public string TherapyName { get; set; }
        public DateTime ScheduledTime { get; set; }
        public decimal Amount { get; set; }
    }
}
