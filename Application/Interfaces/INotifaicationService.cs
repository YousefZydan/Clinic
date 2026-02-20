using Application.Dtos;
using Application.Helpers;

namespace Application.Interfaces
{
    public interface INotifaicationService
    {
        Task<Result<string>> AddNotification(NotificationCreateDto input);
        Task<Result<List<NotificationDto>>> GetNotifications(string userId);
    }
}

