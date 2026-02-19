using System;

namespace TelePsy.Domain.Entities
{
    public class GlobalConfiguration
    {
        public int Id { get; set; }
        public string Key { get; set; } // e.g., "CommissionRate"
        public string Value { get; set; } // e.g., "0.30"
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
