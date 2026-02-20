using Application.Dtos;

namespace Application.Interfaces
{
    public interface IDoctorDetailsService
    {
        Task<List<DoctorDetailsDto>> GetByDoctorId(Guid doctorId);
    }
}
