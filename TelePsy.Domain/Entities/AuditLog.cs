using System;

namespace TelePsy.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Who performed the action
        public string Action { get; set; } // Create, Read, Update, Delete, Login, etc.
        public string EntityName { get; set; } // Table affected
        public string EntityId { get; set; } // ID of the record affected
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Details { get; set; } // JSON or specific details
        public string IPAddress { get; set; }
    }
}
