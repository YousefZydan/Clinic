using Application.Dtos;
using FluentValidation;

namespace Infrastructure.Validation.Auth
{
    public class EditProfileDtoValidator : AbstractValidator<EditProfileDto>
    {
        public EditProfileDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 50).WithMessage("Name must be between 2 and 50 characters");

            RuleFor(x => x.Nickname)
                .NotEmpty().WithMessage("Nickname is required")
                .Length(2, 50).WithMessage("Nickname must be between 2 and 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 20).WithMessage("Username must be between 3 and 20 characters");

            RuleFor(x => x.DateOfBirth)
                .Must(d => d != default(DateTime)).WithMessage("Date of Birth is required")
                .LessThan(DateTime.Now).WithMessage("Date of Birth must be in the past");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Gender is required");

            //RuleFor(x => x.Photo)
            //    .NotNull().WithMessage("Photo is required");
        }
    }
}
