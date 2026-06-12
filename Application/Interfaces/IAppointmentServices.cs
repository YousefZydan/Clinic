using Application.Dtos;
using Application.Helpers;

namespace Application.Interfaces
{
    public interface IAppointmentServices
    {
        Task<Result<string>> AddToBooking(Guid doctorId,string userId, AppoinmentCreate_EditDto dto);
        Task<Result<string>> EditBooking(Guid bookingId,string userId, AppoinmentCreate_EditDto dto);
        Task<Result<string>> CancelBooking(Guid bookingId,string userId);
        Task<Result<List<AppointmentForPatientDto>>> GetUserBookings(string userId,string status);
        Task<Result<List<NonAvailableAppointmentsDto>>> NonAvailableAppointments(Guid doctorId);


    }
}

public class NonAvailableAppointmentsDto
{
    public DateOnly Date { get; set; }
    public TimeOnly Hour { get; set; }
}

