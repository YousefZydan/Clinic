using Application.Interfaces.Firebase;
using FirebaseAdmin.Messaging;

namespace Infrastructure.Services
{
    public class FirebaseNotificationService(IFcmTokenService _fcmTokenService)
        : IFirebaseNotificationService
    {
        public async Task SendNotificationAsync(string token, string title, string body)
        {
            if (string.IsNullOrEmpty(token))
                return;

            if (FirebaseMessaging.DefaultInstance == null)
                throw new Exception("Firebase not initialized");

            var message = new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (FirebaseMessagingException ex)
            {
                // لو التوكن مش شغال → تجاهله وخلاص
                if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                    return;

                throw;
            }
        }
    }
}
