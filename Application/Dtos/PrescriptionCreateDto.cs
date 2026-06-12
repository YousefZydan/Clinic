using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos
{
    public class PrescriptionCreateDto
    {
        public IFormFile? Prescription { get; set; }
        public Guid UserId { get; set; }

    }
}





