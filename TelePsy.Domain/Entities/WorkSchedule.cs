using System;

namespace TelePsy.Domain.Entities
{
    public class WorkSchedule
    {
        public int Id { get; set; }
        
        public int PsychologistId { get; set; }
        public Psychologist Psychologist { get; set; }
        
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
