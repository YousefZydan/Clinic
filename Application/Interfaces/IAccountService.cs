using Application.Dtos;
using Application.Helpers;

public interface IAccountService
{
    Task<Result<CurrentUser>> Register(RegisterDto input);
    Task<Result<CurrentUser>> Login(LoginDto input);
    Task<Result<List<string>>> UpdateUserRolesAsync(UpdateRolesDto input);
    Task<Result<List<string>>> GetRolesOfUser(string userId);

    Task<Result<string>> ForgotPassword(ForgotPasswordDto input);
    Task<Result<bool>> VerifyOtpAsync(VerifyOtpDto input);
    Task<Result<bool>> ResetPassword(ResetPasswordDto input);

    Task<userDto?> GetProfileInfo(string userId);
}
