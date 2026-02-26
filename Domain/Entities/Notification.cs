using Domain.Premitives;

namespace Domain.Entities
{
    public class Notification : Audited<Guid>
    {
        private Notification() { }
        public Notification(string userId, string title, string message)
        {
            UserId = userId;
            Title = title;
            Message = message;
        }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
    }
}

