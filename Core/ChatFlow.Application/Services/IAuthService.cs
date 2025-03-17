using ChatFlow.Domain.DTOs;
using ChatFlow.Domain.Entities.Concretes;

namespace ChatFlow.Application.Services;

public interface IAuthService
{
    Task<object> LoginAsync(LoginDTO loginDTO);
    Task<object> RefreshTokenAsync(string refreshToken);
    Task<object> AddUserAsync(AppUserDTO appUserDTO);
    Task<object> EmailConfirmAsync(string token);
    Task<object> ForgetPasswordAsync(ForgetPasswordDTO forgetPasswordDTO);
    Task<object> ResetPasswordAsync(string token, ResetPasswordDTO resetPasswordDTO);
    AppUser? GetUserDatas();
}
