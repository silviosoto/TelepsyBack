namespace TelePsy.Domain.Entities
{
    public class Admin
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string Department { get; set; }
    }
}
