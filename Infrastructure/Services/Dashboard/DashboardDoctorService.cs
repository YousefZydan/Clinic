using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Dashboard;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Dashboard
{
    public class DashboardDoctorService(
        UserManager<User> _userManger,
        ApplicationDbContext _context,
        IJwt _jwt,
        IMapper _mapping,
        UserManager<User> _userManager
        ) : IDashboardDoctorService
    {

        public async Task<Result<LoginDocDto>> Login(LoginDoctorDto input)
        {
            var doctor = await _context.Doctors.Include(x => x.User).FirstOrDefaultAsync(x => x.Code == input.Code);
            if (doctor == null)
                return Result<LoginDocDto>.Fail("This doctor not found in the sysytem yet");

            var user = await _userManger.FindByIdAsync(doctor.UserId);
            if (user == null)
                return Result<LoginDocDto>.Fail("This doctor not found in the sysytem yet");

            var pass = await _userManger.CheckPasswordAsync(user, input.Password!);
            if (!pass)
                return Result<LoginDocDto>.Fail("Invalid id or password");

            var token = await _jwt.GenerateToken(user);

            var res = new LoginDocDto
            {
                Token = token
            };

            return Result<LoginDocDto>.Success(res);

        }
        public async Task<userDto?> GetProfileInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return null;

            return _mapping.Map<userDto>(user);

        }
        public async Task<Result<string>> MakeAppointmentAvailable(Guid WorkingTimeId)
        {
            var appointment = await _context.WorkingTimes
                .FirstOrDefaultAsync(a => a.Id == WorkingTimeId);

            if (appointment == null)
                return Result<string>.Fail("Appointment not found");

            try
            {
                appointment.BookStatus = BookStatus.Available;
                appointment.PatientId = null;
                await _context.SaveChangesAsync();
                return Result<string>.Success("Appointment cancelled successfully");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
        }
        public async Task<Result<List<BookingDto>>> GetAppointments(string userId, string status)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
                return Result<List<BookingDto>>.Fail("Doctor not found");


            if (!Enum.TryParse<BookStatus>(status, true, out var parsedStatus))
                return Result<List<BookingDto>>.Fail("Invalid appointment status");

            var appointments = _context.WorkingTimes
                .Where(a => a.DoctorDetails.Doctor.Id == doctor.Id && a.BookStatus == parsedStatus);

            var mapped = await appointments
                .ProjectTo<BookingDto>(_mapping.ConfigurationProvider)
                .ToListAsync();

            return Result<List<BookingDto>>.Success(mapped);
        }

    }
}


