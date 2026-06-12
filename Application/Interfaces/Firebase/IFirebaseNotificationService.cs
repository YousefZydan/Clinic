namespace Application.Interfaces.Firebase
{
    public interface IFirebaseNotificationService
    {
        Task SendNotificationAsync(
        string token,
        string title,
        string body);

    }
}

