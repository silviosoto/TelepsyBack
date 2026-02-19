using System;

namespace TelePsy.Domain.Entities
{
    public class BlockedSlot
    {
        public int Id { get; set; }
        
        public int PsychologistId { get; set; }
        public Psychologist Psychologist { get; set; }
        
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        
        public string Reason { get; set; }
    }
}
