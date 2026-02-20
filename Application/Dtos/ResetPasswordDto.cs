using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class ResetPasswordDto
    {
            public required string Email { get; set; }

            public required string Code { get; set; }

            public required string NewPassword { get; set; }

            public required string ConfirmPassword { get; set; }
        }
    }

