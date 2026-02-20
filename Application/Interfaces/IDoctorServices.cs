using Application.Dtos;

namespace Application.Interfaces
{
    public interface IDoctorServices
    {
        // get doctor by name
        Task<List<DoctorDto>> GetDoctorByName(string input);

        // get doctor by category 
        Task<List<DoctorDto>> GetDoctorByCategoryId(Guid CategoryId);

        Task<List<DoctorDto>> GetDoctorByCategoryName(string CategoryName);

        Task<List<DoctorDto>> GetAllDoctors();

        
    }
}


