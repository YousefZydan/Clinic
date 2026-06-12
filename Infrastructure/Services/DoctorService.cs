using Application.Dtos;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Services
{
    internal class DoctorService : IDoctorServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DoctorService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<DoctorDto>> GetAllDoctors()
        {
            var doctors = await _context.Doctors
                           .ProjectTo<DoctorDto>(_mapper.ConfigurationProvider)
                           .ToListAsync();

            return doctors;
        }

        public async Task<List<DoctorDto>> GetDoctorByCategoryId(Guid categoryId)
        {
            var doctors = await _context.Doctors
                .Where(d => d.CategoryId == categoryId)
                .ProjectTo<DoctorDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return doctors;
        }

        public async Task<List<DoctorDto>> GetDoctorByCategoryName(string CategoryName)
        {
            var doctors = await _context.Doctors
                           .Where(d => d.Category.Name.Contains(CategoryName))
                           .ProjectTo<DoctorDto>(_mapper.ConfigurationProvider)
                           .ToListAsync();

            return doctors;
        }

        public async Task<List<DoctorDto>> GetDoctorByName(string input)
        {
            var doctors = await _context.Doctors
                .Where(d => d.Name.Contains(input))
                .ProjectTo<DoctorDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return doctors;
        }
    }
}


