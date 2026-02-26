using FluentValidation;
using Application.Dtos;
namespace Infrastructure.Validation.Auth
{
    public class LoginDoctorDtoValidator : AbstractValidator<LoginDoctorDto>
    {

        public LoginDoctorDtoValidator()
        {
            RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

            RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Password is required");

        }
    }
}
