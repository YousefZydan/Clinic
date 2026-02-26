namespace Application.Dtos
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsFav { get; set; }
        public string Name { get; set; } = "";
        public string About { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string CategoryName { get; private set; } = "";

    }
}


