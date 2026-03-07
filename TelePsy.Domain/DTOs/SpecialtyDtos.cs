namespace TelePsy.Domain.DTOs
{
    public class SpecialtyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdatePsychologistSpecialtyDto
    {
        public int SpecialtyId { get; set; }
        public bool IsActive { get; set; }
    }
}
