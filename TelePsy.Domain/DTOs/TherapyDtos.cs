namespace TelePsy.Domain.DTOs
{
    public class TherapyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateTherapyDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
