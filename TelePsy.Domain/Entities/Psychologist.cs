namespace TelePsy.Domain.Entities
{
    public class Psychologist
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }

        public string LicenseNumber { get; set; }
        public string Specialization { get; set; }
        public string University { get; set; }
        public int ExperienceYears { get; set; }
        public decimal SessionRate { get; set; } // Hourly rate
        public string? PaymentAccount { get; set; }

        public string Bio { get; set; }
        public bool IsActive { get; set; } = true;
        public string Hobbies { get; set; } // For matching

        public bool IsVerified { get; set; }

        public string? CvPath { get; set; } // Path to CV file
        public string? ProfilePicturePath { get; set; } // Path to profile picture

        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<PsychologistTherapy> Therapies { get; set; }
        public ICollection<PsychologistSpecialty> Specialties { get; set; }
    }
}
