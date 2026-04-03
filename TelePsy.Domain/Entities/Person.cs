using System;

namespace TelePsy.Domain.Entities
{
    public class Person
    {
        public int Id { get; set; }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        
        public string? DocumentType { get; set; } // CC, TI, CE, NIT, RC
        public string? DocumentNumber { get; set; }
        
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
