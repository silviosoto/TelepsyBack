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

    public class PatientProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? City { get; set; }
        public string? Hobbies { get; set; }
    }
}
