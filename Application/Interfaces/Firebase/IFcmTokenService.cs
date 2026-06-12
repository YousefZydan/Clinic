namespace Application.Interfaces.Firebase
{
    public interface IFcmTokenService
    {
        Task SaveTokenAsync(
            string userId,
            string token);

    }
}
