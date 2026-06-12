using Application.Dtos;
using Application.Helpers;

namespace Application.Interfaces
{
    public interface IDoctorDetailsService
    {
        Task<List<DoctorDetailsDto>> GetByDoctorId(Guid doctorId);
        Task<Result<string>> Rating(Guid doctorId,double rate);

    }
}




