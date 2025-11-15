namespace HealthcareAppointmentSystem.Models
{
    public class Availability
    {
        public string DayOfWeek { get; set; } // Monday, Tuesday, etc.
        public string StartTime { get; set; } // 09:00
        public string EndTime { get; set; }   // 17:00
        public bool IsAvailable { get; set; } = true;
    }
}
