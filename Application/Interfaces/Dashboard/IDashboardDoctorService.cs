using Application.Dtos;
using Application.Helpers;

namespace Application.Interfaces.Dashboard
{
    public interface IDashboardDoctorService
    {
        Task<Result<LoginDocDto>> Login(LoginDoctorDto input);
        Task<Result<List<BookingDto>>> GetAppointments(string userId, string status);
        Task<Result<string>> MakeAppointmentAvailable(Guid WorkingTimeId);
        Task<userDto?> GetProfileInfo(string userId);

    }
}


public class LoginDocDto
{
    public string? Token { get; set; }

}

public class BookingDto
{
    public string PatientName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime Date { get; set; }
    public BookStatus Status { get; set; }
}


