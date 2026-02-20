namespace Application.Dtos
{
    public class WorkingTimeDto
    {
        public string Day { get; set; } = string.Empty;
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }

    }
}
