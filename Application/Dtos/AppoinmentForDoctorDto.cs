namespace Application.Dtos
{
    public class AppointmentOfDoctorDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Hour { get; set; }
        public string? PatientName { get; set; }
        public string? PatientPhone { get; set; }
        public string? PhotoUrl { get; set; }

    }
}

