using System;
using System.Collections.Generic;

namespace TelePsy.Domain.DTOs
{
    public class WorkScheduleDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class AvailableSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class BlockSlotDto
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Reason { get; set; }
    }
}
