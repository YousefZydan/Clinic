using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Firebase;
using Application.Repository;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class NotificationService(IHubContext<NotificationHub> _hubContext,
     IMapper _mapping, IGenericRepository<Notification> _repo,
     ApplicationDbContext _db,
     IFirebaseNotificationService _firebaseService
    ) : INotificationService
{
    public async Task SendToUserAsync(string userId, string message)
    {
        await _hubContext.Clients.User(userId)
            .SendAsync("ReceiveNotification", message);
    }


    public async Task<Result<string>> AddNotification(NotificationCreateDto input)
    {
        var rec = _mapping.Map<Notification>(input);

        var res = await _repo.CreateAsync(rec);

        if (res.Succeeded)
        {
            await SendToUserAsync(input.UserId, input.Message);

            var users = await _db.UserDevices
                .Where(x => x.UserId == input.UserId &&
                            !string.IsNullOrEmpty(x.FcmToken))
                .ToListAsync();

            if (users.Any())
            {
                foreach (var userDevice in users)
                {
                    await _firebaseService.SendNotificationAsync(
                        userDevice.FcmToken,
                        "Clinic",
                        input.Message);
                }
            }

            return Result<string>.Success(
                "Notification added and sent successfully");
        }
        else
        {
            return Result<string>.Fail(
                res.Error ?? "Failed to add notification");
        }
    }



    public async Task<Result<List<NotificationDto>>> GetNotifications(string userId)
    {
        var recs = await _db.Notifications.Where(x => x.UserId == userId)
            .OrderBy(x =>x.CreatedAt).ToListAsync();

        var dtoRecs = _mapping.Map<List<NotificationDto>>(recs);

        return Result<List<NotificationDto>>.Success(dtoRecs);
    }

    public async Task<Result<string>> MarkNotificationAsRead(Guid notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId );

        if (notification == null)
            return Result<string>.Fail("Notification not found.");

        notification.IsRead = true;

        var res = await _repo.UpdateAsync(notification);
        if (!res.Succeeded)
            return Result<string>.Fail(res.Error ?? "Failed to mark notification as read.");

        return Result<string>.Success("Notification marked as read successfully.");

    }

}





