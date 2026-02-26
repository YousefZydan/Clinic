using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Application.Dtos
{
    public class EditProfileDto
    {
        public required string Name { get; set; }
        public required string Nickname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; } 
        public  IFormFile? Photo { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string UserName { get; set; }

    }
}
