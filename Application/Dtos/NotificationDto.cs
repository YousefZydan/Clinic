namespace Application.Dtos
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsRead { get; set; } = false;
        public TypeOfNotification Type { get; set; }

    }
}


