using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Repository;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.Services
{
    public class NotifaicationService(IMapper _mapping, IGenericRepository<Notification> _repo) : INotifaicationService
    {
        public async Task<Result<string>> AddNotification(NotificationCreateDto input)
        {
            var rec = _mapping.Map<NotificationCreateDto, Notification>(input);
            return await _repo.CreateAsync(rec);
        }

   
        public async Task<Result<List<NotificationDto>>> GetNotifications(string userId)
        {
            var recs = await _repo.GetByAsync(n => n.UserId == userId);
            var dtoRecs = _mapping.Map<List<Notification>, List<NotificationDto>>(recs);

            if (dtoRecs is null || dtoRecs.Count < 1)
            {
                return Result<List<NotificationDto>>.Fail("No notifications found");
            }
            return Result<List<NotificationDto>>.Success(dtoRecs);
        }

        }
    }
