using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class ForgotPasswordDto
    {
        public required string Email { get; set; }

    }
}

