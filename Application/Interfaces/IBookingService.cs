using Application.Helpers;

namespace Application.Interfaces
{
    public interface IBookingService
    {
        Task<Result<string>> Booking(Guid WorkingTimeId, string userId);
        Task<Result<string>> GetAllBooking(string userId);
        Task<Result<string>> CancelBooking(Guid WorkingTimeId, string userId);

    }
}


