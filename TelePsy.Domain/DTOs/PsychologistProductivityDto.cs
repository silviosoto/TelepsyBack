using System;

namespace TelePsy.Domain.DTOs
{
    public class PsychologistProductivityDto
    {
        public int AppointmentId { get; set; }
        public DateTime Date { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string TherapyName { get; set; } = string.Empty;
        public decimal GrossAmount { get; set; }
        public decimal Commission { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool PatientAttended { get; set; }
        public bool PsychologistAttended { get; set; }
    }

    public class ProductivityReportResponseDto
    {
        public List<PsychologistProductivityDto> Items { get; set; } = new();
        public decimal TotalGross { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalNet { get; set; }
        public int TotalSessions { get; set; }
    }
}
