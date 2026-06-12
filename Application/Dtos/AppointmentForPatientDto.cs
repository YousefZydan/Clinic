namespace Application.Dtos
{
    public class AppointmentForPatientDto
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Hour { get; set; }
        public string? DoctorName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? CategoryName { get; set; }
        public Guid DoctorId { get; set; }
 


    }
}

