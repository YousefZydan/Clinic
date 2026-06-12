using Application.Dtos;
using Application.Helpers;

namespace Application.Interfaces
{
    public interface INotificationService
    {
        Task SendToUserAsync(string userId, string message);
        Task<Result<string>> AddNotification(NotificationCreateDto input);
        Task<Result<List<NotificationDto>>> GetNotifications(string userId);

        Task<Result<string>> MarkNotificationAsRead(Guid notificationId);

    }
}


