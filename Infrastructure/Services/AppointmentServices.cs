using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Repository;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class AppointmentServices(IMapper _mapping,
        IGenericRepository<Doctor> _doctor,
        IGenericRepository<Appointment> _appointment,
        ApplicationDbContext _db,
        INotificationService _notificationService) : IAppointmentServices
    {
        public async Task<Result<string>> AddToBooking(Guid doctorId, string userId, AppoinmentCreate_EditDto dto)
        {
            var doctor = await _doctor.FindByIdAsync(doctorId);
            if (doctor == null)
                return Result<string>.Fail("Doctor not found");

            var newBooking = _mapping.Map<Appointment>(dto);
            newBooking.DoctorId = doctorId;
            newBooking.UserId = userId;
            newBooking.Status = AppointmentStatus.UpComming;

            var createResult = await _appointment.CreateAsync(newBooking);
            if (!createResult.Succeeded)
                return Result<string>.Fail(createResult.Error!);

            var createdNotification = new NotificationCreateDto()
            {
                Message = $"You have successfully Adding your appointment with Dr. {doctor.Name}",
                Title = "Appointment Success",
                UserId = newBooking.UserId,
                Type = TypeOfNotification.Succeeded,
                IsRead = false
            };

            var res = await _notificationService.AddNotification(createdNotification);
            if (!res.Succeeded)
                return Result<string>.Fail(res.Error!);

            return Result<string>.Success(res.Data!);
        }

        public async Task<Result<string>> CancelBooking(Guid bookingId, string userId)
        {
            var record = await _db.Appointments.Include(x => x.Doctor).
                FirstOrDefaultAsync(x => x.Id == bookingId && x.UserId == userId);

            if (record == null)
                return Result<string>.Fail("This booking is not found");

            record.Status = AppointmentStatus.Cancelled;

            var updated = await _appointment.UpdateAsync(record);
            if (!updated.Succeeded)
                return Result<string>.Fail(updated.Error!);


            var createdNotification = new NotificationCreateDto()
                {
                Message = $"You have successfully cancelled your appointment with Dr. {record.Doctor!.Name}",
                Title = "Appointment Cancelled",
                UserId = record.UserId!,
                Type = TypeOfNotification.Cancelled,
                IsRead = false
               };

            var res = await _notificationService.AddNotification(createdNotification);
            if (!res.Succeeded)
                return Result<string>.Fail(res.Error!);

            return Result<string>.Success(res.Data!);


        }

        public async Task<Result<string>> EditBooking(Guid bookingId, string userId, AppoinmentCreate_EditDto dto)
        {
            var records = await _appointment.GetByAsync(x => x.Id == bookingId && x.UserId == userId);
            if (!records.Any())
                return Result<string>.Fail("This booking is not found");

            var booking = records[0];
            _mapping.Map(dto, booking);

           var updated =  await _appointment.UpdateAsync(booking);
            if (!updated.Succeeded)
                return Result<string>.Fail(updated.Error!);


            var createdNotification = new NotificationCreateDto()
            {
                Message = $"You have successfully Changes your appointment to {booking.CreatedAt:yyyy-MM-dd HH:mm}",
                Title = "Appointment Changed",
                UserId = booking.UserId!,
                Type = TypeOfNotification.Rescheduled,
                IsRead = false
            };

           var res = await _notificationService.AddNotification(createdNotification);
            if (!res.Succeeded)
                return Result<string>.Fail(res.Error!);

            return Result<string>.Success(res.Data!);
        }

        public async Task<Result<List<AppointmentForPatientDto>>> GetUserBookings(string userId, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return Result<List<AppointmentForPatientDto>>.Fail("Status is required");

            if (!Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
                return Result<List<AppointmentForPatientDto>>.Fail("Invalid status value");

            var mapped = await _db.Appointments
                .Where(x => x.UserId == userId && x.Status == parsedStatus)
                .ProjectTo<AppointmentForPatientDto>(_mapping.ConfigurationProvider)
                .ToListAsync();

            return Result<List<AppointmentForPatientDto>>.Success(mapped);
        }

        public async Task<Result<List<NonAvailableAppointmentsDto>>> NonAvailableAppointments(Guid doctorId)
        {
            var records = await _appointment.GetByAsync(x => x.DoctorId == doctorId && x.Status == AppointmentStatus.UpComming);
            var mapped = _mapping.Map<List<NonAvailableAppointmentsDto>>(records);
            return Result<List<NonAvailableAppointmentsDto>>.Success(mapped);
        }
    }
}

