using Domain.Premitives;

namespace Domain.Entities
    {
        public class Appointment : Audited<Guid>
        {
            public DateOnly Date { get; set; }
            public TimeOnly Hour { get; set; }

            public Guid DoctorId { get; set; }
            public Doctor? Doctor { get; set; }

            public string? UserId { get; set; }
            public User? Patient { get; set; }
            public AppointmentStatus Status { get; set; } = AppointmentStatus.UpComming;

            public bool OneHourReminderSent { get; set; }
         
            public bool ThirtyMinutesReminderSent { get; set; }
    }
    }

public enum AppointmentStatus
{
    UpComming = 1,
    Completed,
    Cancelled
}




