using Domain.Premitives;

namespace Domain.Entities
{
    public class WorkingTime : Audited<Guid>
    {
        private WorkingTime() { }
        public string Day { get; set; } = string.Empty; 
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public Guid DoctorDetailsId { get; set; }
        public DoctorDetails DoctorDetails { get; set; } = null!;
        public Guid? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public BookStatus BookStatus { get; set; } = BookStatus.Available;

    }
}

public enum BookStatus
{
    Available = 1,
    NotAvailable
}
