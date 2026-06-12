using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    internal class DoctorDetailsService(ApplicationDbContext _db, IMapper _mapper) : IDoctorDetailsService
    {
        public async Task<List<DoctorDetailsDto>> GetByDoctorId(Guid doctorId)
        {
            var records = _db.DoctorDetails
                .Where(d => d.DoctorId == doctorId);


            var mappedRecords = await records.ProjectTo<DoctorDetailsDto>(_mapper.ConfigurationProvider)
                         .ToListAsync();

            return mappedRecords;
        }

        public async Task<Result<string>> Rating(Guid doctorId, double rate)
        {
            try
            {
                var doctorDetails = await _db.DoctorDetails
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

                if (doctorDetails == null)
                    return Result<string>.Fail("Doctor details not found");

                var newCount = doctorDetails.ReviewsCount + 1;

                doctorDetails.Rating =
                    ((doctorDetails.Rating * doctorDetails.ReviewsCount) + rate)
                    / newCount;

                doctorDetails.ReviewsCount = newCount;

                var s = await _db.SaveChangesAsync();
                if(s <= 0)
                    return Result<string>.Fail("Failed to save rating");


                return Result<string>.Success("Rating added successfully");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"Error while saving rating: {ex.Message}");
            }
        }
    }

}




