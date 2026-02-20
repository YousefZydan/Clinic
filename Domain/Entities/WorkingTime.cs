
using Domain.Premitives;

namespace Domain.Entities
{
    public class WorkingTime : Audited
    {
        public string Day { get; set; } = string.Empty; 
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public Guid DoctorDetailsId { get; set; }
        public DoctorDetails DoctorDetails { get; set; } = null!;
    }
}

