using Domain.Entities;

namespace Application.Dtos
{
    public class PrescriptionDto
    {
        public string? PrescriptionUrl { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorCategory { get; set; }
        public string? UserId { get; set; }
        public Guid? DoctorId { get; set; }
        

    }
}

