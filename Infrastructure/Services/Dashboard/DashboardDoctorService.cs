using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Dashboard;
using Application.Repository;
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
        IGenericRepository<Appointment> _appointments,
        IGenericRepository<Doctor> _doctors,
        IJwt _jwt,
        IMapper _mapping,
        UserManager<User> _userManager,
        INotificationService _notificationService,
        IGenericRepository<Prescription> _per,
        ICloudinaryService _cloudinaryService
        ) : IDashboardDoctorService
    {
        public async Task<Result<string>> AddingPrescription(string userId, PrescriptionCreateDto dto)
        {
            try
            {
                var doctor = await _doctors.GetByAsync(x => x.UserId == userId);

                if (!doctor.Any())
                    return Result<string>.Fail("you are not found as a doctor in our clinic");


                var photo = await _cloudinaryService.AddPhotoAsync(dto.Prescription!);

                if (photo.Error != null)
                    return Result<string>.Fail(photo.Error.Message);


                var prescription = _mapping.Map<Prescription>(dto);

                prescription.PrescriptionUrl = photo?.Url?.ToString();
                prescription.PublicId = photo?.PublicId;
                prescription.DoctorId = doctor[0].Id;


                var c = await _per.CreateAsync(prescription);

                if (!c.Succeeded)
                    return Result<string>.Fail(c.Error!);


                return Result<string>.Success(c.Data!);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"Error: {ex.Message}");
            }
        }

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

        public async Task<Result<string>> BookingStatus(Guid bookingId, string status,string userId)
        {
            var appointment = await _context.Appointments.Include(x => x.Patient).FirstOrDefaultAsync(x => x.Id == bookingId);


            if (appointment == null)
                return Result<string>.Fail("Booking not found");

            if (!Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
                return Result<string>.Fail("Invalid status value");

            appointment.Status = parsedStatus;

            var updated = await _appointments.UpdateAsync(appointment);
            if (!updated.Succeeded)
                return Result<string>.Fail(updated.Error!);

            var createdNotification = new NotificationCreateDto()
            {
                Message = $"You {status} the appointment with patient {appointment.Patient!.Name}",
                Title = "Appointment Changed",
                UserId = userId,
                IsRead = false
            };

            var res = await _notificationService.AddNotification(createdNotification);
            if (!res.Succeeded)
                return Result<string>.Fail(res.Error!);

            return Result<string>.Success(res.Data!);
        }



        public async Task<Result<List<AppointmentOfDoctorDto>>> GetMyBookings(string userId, string status)
        {
            var doctor = await _doctors
                .GetByAsync(x => x.UserId == userId);

            if (!doctor.Any())
                return Result<List<AppointmentOfDoctorDto>>.Fail("Doctor not found");

            var query = _context.Appointments
                .Where(x => x.DoctorId == doctor[0].Id);

            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
                    return Result<List<AppointmentOfDoctorDto>>.Fail("Invalid status value");

                query = query.Where(x => x.Status == parsedStatus);
            }



            var appointments = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<AppointmentOfDoctorDto>(_mapping.ConfigurationProvider).ToListAsync();
               


            return Result<List<AppointmentOfDoctorDto>>.Success(appointments);
        }
    }
}


