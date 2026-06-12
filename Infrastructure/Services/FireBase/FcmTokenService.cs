using Application.Interfaces.Firebase;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class FcmTokenService : IFcmTokenService
    {
        private readonly ApplicationDbContext _db;

        public FcmTokenService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SaveTokenAsync(
            string userId,
            string token)
        {
            // منع تكرار نفس التوكن
            var exists = await _db.UserDevices
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.FcmToken == token);

            if (exists)
                return;

            var userDevice = new UserDevice
            {
                UserId = userId,
                FcmToken = token
            };

            await _db.UserDevices
                .AddAsync(userDevice);

            await _db.SaveChangesAsync();
        }

    }
}

