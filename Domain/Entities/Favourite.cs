using Domain.Premitives;

namespace Domain.Entities
{
    public class Favourite : Audited<Guid>
    {
        private Favourite() { }
        public Favourite(string? patientId, Guid doctorId)
        {
            UserId = patientId;
            DoctorId = doctorId;
        }
        
        public string? UserId { get; set; }
        public User? User { get; set; }
        public Guid DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
    }
}





