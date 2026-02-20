using Domain.Premitives;

namespace Domain.Entities
{
    public class Favourite : Audited
    {
        private Favourite() { }
        public Favourite(Guid? patientId, Guid doctorId)
        {
            PatientId = patientId;
            DoctorId = doctorId;
        }
        
        public Guid? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public Guid DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
    }
}





