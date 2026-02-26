using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class BookingService(ApplicationDbContext _context,IMapper _mapping) : IBookingService
    {
        public async Task<Result<string>> Booking(Guid WorkingTimeId, string userId)
        {
           var record = await _context.WorkingTimes.FirstOrDefaultAsync(x => x.Id == WorkingTimeId);
              if (record == null)
                return Result<string>.Fail("Working time not found.");

          if (record.BookStatus != BookStatus.Available)
                return Result<string>.Fail("This time is already booked.");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient == null)
                    return Result<string>.Fail("Patient not found.");

            record.BookStatus = BookStatus.NotAvailable;
            record.PatientId  = patient.Id;
            await _context.SaveChangesAsync();

            return Result<string>.Success("Booking successful.");
        }

        public async Task<Result<string>> CancelBooking(Guid WorkingTimeId, string userId)
        {
            var pateint = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (pateint == null)
                    return Result<string>.Fail("Patient not found.");

            var record = await _context.WorkingTimes
                .FirstOrDefaultAsync(x => x.Id == WorkingTimeId && x.PatientId == pateint.Id);
              if (record == null)
                return Result<string>.Fail("Working time not found.");

                record.BookStatus = BookStatus.Available;
                record.PatientId = null;
                await _context.SaveChangesAsync();

            return Result<string>.Success("Booking cancelled successfully.");
        }

        public async Task<Result<string>> GetAllBooking(string userId)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient == null)
                    return Result<string>.Fail("Patient not found.");

            var bookings = _context.WorkingTimes.Where(x => x.PatientId == patient.Id);

            var mapped = await bookings.ProjectTo<GetAllBookingResult>(_mapping.ConfigurationProvider).ToListAsync();

            if (mapped.Count == 0)
                return Result<string>.Success("No bookings found.");

            return Result<string>.Success("Bookings retrieved successfully.");


        }
    }
}


