using System;

namespace TelePsy.Domain.DTOs
{
    public class PatientListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ProfilePicturePath { get; set; }
        public DateTime? LastAppointmentDate { get; set; }
        public int SessionCount { get; set; }
    }
}
