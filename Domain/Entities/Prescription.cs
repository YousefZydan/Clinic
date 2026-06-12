using Domain.Premitives;

namespace Domain.Entities
{
    public class Prescription : Audited<Guid>
    {
        private Prescription() { }

        public string? PrescriptionUrl { get; set; }
        public string? PublicId { get; set; }

        public Doctor? Doctor { get; set; }
        public Guid DoctorId { get; set; }
        public User? Patient { get; set; }
        public string UserId { get; set; }

        



    }
}

