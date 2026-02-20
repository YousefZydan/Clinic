namespace Infrastructure.Services
{
    using Application.Helpers;
    using Application.Interfaces;
    using Domain.Entities;
    using Domain.Enums;
    using Microsoft.AspNetCore.Identity;

    public class OAuthService(UserManager<User> _userManager, IJwt _jwt) : IOAuthService
    {

        public async Task<Result<string>> HandleExternalLoginAsync(string email, string fullName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    Name = fullName ?? "Unknown",
                    Nickname = "",
                    DateOfBirth = DateTime.MinValue,
                    Gender = Gender.UnKnown,
                    PhotoUrl = ""
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return Result<string>.Fail($"User creation failed: {errors}");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, Roles.User.ToString());
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return Result<string>.Fail($"Adding role failed: {errors}");
                }
            }

            var token = await _jwt.GenerateToken(user);
            return Result<string>.Success(token);
        }

    }

}


