namespace TelePsy.Domain.Entities
{
    public class PsychologistTherapy
    {
        public int Id { get; set; }
        public int PsychologistId { get; set; }
        public Psychologist Psychologist { get; set; }

        public int TherapyId { get; set; }
        public Therapy Therapy { get; set; }

        public decimal Rate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
