using Microsoft.AspNetCore.Identity;
using System;

namespace TelePsy.Domain.Entities
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public Person Person { get; set; }
    }
}
