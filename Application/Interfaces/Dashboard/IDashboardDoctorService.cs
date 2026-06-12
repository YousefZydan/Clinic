using Application.Dtos;
using Application.Helpers;

namespace Application.Interfaces.Dashboard
{
    public interface IDashboardDoctorService
    {
        Task<Result<LoginDocDto>> Login(LoginDoctorDto input);
        Task<userDto?> GetProfileInfo(string userId);
        Task<Result<string>> BookingStatus(Guid bookingId,string status,string userId);
        Task<Result<List<AppointmentOfDoctorDto>>> GetMyBookings(string userId, string status);
        Task<Result<string>> AddingPrescription(string userId, PrescriptionCreateDto dto);


    }
}


public class LoginDocDto
{
    public string? Token { get; set; }

}



