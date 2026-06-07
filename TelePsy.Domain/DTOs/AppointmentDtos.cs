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

    public class PatientAppointmentDto
    {
        public int Id { get; set; }
        public int PsychologistId { get; set; }
        public string? PsychologistName { get; set; }
        public string? PsychologistSpecialty { get; set; }
        public int TherapyId { get; set; }
        public string? TherapyName { get; set; }
        public DateTime ScheduledTime { get; set; }
        public int DurationMinutes { get; set; }
        public int Status { get; set; }
        public string? VideoLink { get; set; }
        public bool IsPackage { get; set; }
        public int? SessionPackageId { get; set; }
    }

    public class PersonSummaryDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class PatientSummaryDto
    {
        public int Id { get; set; }
        public PersonSummaryDto? Person { get; set; }
    }

    public class PsychologistAppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public PatientSummaryDto? Patient { get; set; }
        public int TherapyId { get; set; }
        public string? TherapyName { get; set; }
        public DateTime ScheduledTime { get; set; }
        public int DurationMinutes { get; set; }
        public int Status { get; set; }
        public string? VideoLink { get; set; }
        public bool IsPackage { get; set; }
        public int? SessionPackageId { get; set; }
    }
}
