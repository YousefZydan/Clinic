using Application.Dtos;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    internal class DoctorDetailsService(ApplicationDbContext _db,IMapper _mapper) : IDoctorDetailsService
    {
        public async Task<List<DoctorDetailsDto>> GetByDoctorId(Guid doctorId)
        {
            var records = _db.DoctorDetails
                .Where(d => d.DoctorId == doctorId);
                

              var  mappedRecords = await records.ProjectTo<DoctorDetailsDto>(_mapper.ConfigurationProvider)
                           .ToListAsync();

            return mappedRecords;
        }
    }
}

