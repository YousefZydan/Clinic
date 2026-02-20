using Application.Dtos;
using FluentValidation;

namespace Application.Validators
{
    public class AddToRoleDtoValidator : AbstractValidator<AddToRoleDto>
    {
        public AddToRoleDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("RoleName is required")
                .Must(role => new[] { "Admin", "Doctor", "Patient" }.Contains(role))
                .WithMessage("Invalid role name");
        }
    }
}


