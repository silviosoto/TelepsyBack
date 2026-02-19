namespace TelePsy.Domain.DTOs
{
    public class PsychologistTherapyDto
    {
        public int Id { get; set; }
        public int PsychologistId { get; set; }
        public int TherapyId { get; set; }
        public string TherapyName { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }
    }

    public class SetRateDto
    {
        public int TherapyId { get; set; }
        public decimal Rate { get; set; }
    }
}
