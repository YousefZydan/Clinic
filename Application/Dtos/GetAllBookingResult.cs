namespace Application.Dtos
{
    public class GetAllBookingResult
    {
        public string Day { get; set; } = string.Empty;
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public string? DoctorName { get; set; }
    }
}
