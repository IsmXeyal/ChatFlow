using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Application.Repositories.Writes;
using ChatFlow.Application.Services;
using ChatFlow.Domain.DTOs;
using ChatFlow.Domain.Entities.Concretes;
using ChatFlow.Domain.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChatFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAppUserReadRepository _readAppUserRepository;
    private readonly IAppUserWriteRepository _writeAppUserRepository;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IAppUserReadRepository readAppUserRepository, ITokenService tokenService, IAppUserWriteRepository writeAppUserRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _readAppUserRepository = readAppUserRepository;
        _tokenService = tokenService;
        _writeAppUserRepository = writeAppUserRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<object> AddUserAsync(AppUserDTO appUserDTO)
    {
        var existingUser = await _readAppUserRepository.GetUserByUserName(appUserDTO.UserName);
        if (existingUser is not null)
            return new { success = false, message = "User already exists" };

        using var hmac = new HMACSHA256();
        var newUser = new AppUser()
        {
            UserName = appUserDTO.UserName,
            Email = appUserDTO.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(appUserDTO.Password)),
            PasswordSalt = hmac.Key,
            Role = appUserDTO.Role
        };

        var emailToken = _tokenService.CreateEmailConfirmToken();
        var confirmationLink = $"https://localhost:5001/api/Auth/EmailConfirm?token={emailToken.Token}";

        // Ensure the EmailConfirmToken is correctly associated with the user
        if (newUser.EmailConfirmToken == null)
            newUser.EmailConfirmToken = new EmailConfirmToken();

        newUser.EmailConfirmToken.Token = emailToken.Token;
        newUser.EmailConfirmToken.CreateTime = emailToken.CreateTime;
        newUser.EmailConfirmToken.ExpireTime = emailToken.ExpireTime;
        newUser.EmailConfirmToken.UserId = newUser.Id;

        await _writeAppUserRepository.AddAsync(newUser);
        await _writeAppUserRepository.SaveChangesAsync();

        return new { success = true, emailConfirmLink = confirmationLink };
    }

    public async Task<object> EmailConfirmAsync(string token)
    {
        var user = await _readAppUserRepository.GetUserByEmailConfirmToken(token);
        if (user is null)
            return new { success = false, message = "Invalid EmailConfirm Token" };

        if (user.EmailConfirmToken.ExpireTime < DateTime.UtcNow)
            return new { success = false, message = "EmailConfirm Token expired" };

        if (user.EmailConfirm)
            return new { success = false, message = "Email already confirmed" };

        user.EmailConfirm = true;
        await _writeAppUserRepository.UpdateAsync(user);
        await _writeAppUserRepository.SaveChangesAsync();

        return new { success = true, message = "Email confirmed" };
    }

    public async Task<object> ForgetPasswordAsync(ForgetPasswordDTO forgetPasswordDTO)
    {
        var user = await _readAppUserRepository.GetUserByEmailWithRelationsAsync(forgetPasswordDTO.Email);
        if (user is null)
            return new { success = false, message = "User not found" };

        var resetToken = _tokenService.CreateRepasswordToken();
        var resetLink = $"https://localhost:5001/api/Auth/ResetPassword?token={resetToken.Token}";

        if (user.RePasswordToken == null)
            user.RePasswordToken = new RePasswordToken();

        user.RePasswordToken.Token = resetToken.Token;
        user.RePasswordToken.CreateTime = resetToken.CreateTime;
        user.RePasswordToken.ExpireTime = resetToken.ExpireTime;

        await _writeAppUserRepository.UpdateAsync(user);
        await _writeAppUserRepository.SaveChangesAsync();

        return new { success = true, resetLink = resetLink };
    }

    public async Task<object> LoginAsync(LoginDTO loginDTO)
    {
        var user = await _readAppUserRepository.GetUserByUserNameWithRelationsAsync(loginDTO.UserName);
        if (user is null)
            return new { success = false, message = "Invalid username" };

        using var hmac = new HMACSHA256(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        if (!computedHash.SequenceEqual(user.PasswordHash))
            return new { success = false, message = "Invalid password" };

        if (!user.EmailConfirm)
            return new { success = false, message = "Please confirm your email" };

        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken();
        await SetRefreshToken(user, refreshToken);

        _httpContextAccessor.HttpContext?.Session.SetString("accessToken", accessToken);

        var userVm = new AppUserVM
        {
            UserName = user.UserName,
            ConnectionId = user.ConnectionId
        };

        _httpContextAccessor.HttpContext?.Session.SetString("userVm", JsonConvert.SerializeObject(userVm));

        return new { success = true, accessToken = accessToken };
    }

    public async Task<object> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return new { success = false, message = "Invalid refresh token" };

        var user = await _readAppUserRepository.GetUserByRefreshToken(refreshToken);
        if (user is null)
            return new { success = false, message = "Invalid refresh token" };

        var accessToken = _tokenService.CreateAccessToken(user);
        var newRefreshToken = _tokenService.CreateRefreshToken();
        await SetRefreshToken(user, newRefreshToken);

        return new { success = true, accessToken = accessToken };
    }

    public async Task<object> ResetPasswordAsync(string token, ResetPasswordDTO resetPasswordDTO)
    {
        var user = await _readAppUserRepository.GetUserByRePasswordToken(token);
        if (user is null)
            return new { success = false, message = "Invalid Reset Token" };

        if (user.RePasswordToken.ExpireTime < DateTime.UtcNow)
            return new { success = false, message = "Reset Token expired" };

        using var hmac = new HMACSHA256();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(resetPasswordDTO.Password));
        user.PasswordSalt = hmac.Key;

        await _writeAppUserRepository.UpdateAsync(user);
        await _writeAppUserRepository.SaveChangesAsync();
        return new { success = true, message = "Password reset successful" };
    }

    private async Task SetRefreshToken(AppUser user, RefreshToken refreshToken)
    {
        if (user.RefreshToken == null)
            user.RefreshToken = new RefreshToken();


        // Refresh token-i cookie-de yadda saxlamaliyiqki, access tokenin vaxti qurtaranda
        // ordan oxuyub istifade ede bilsin.

        var cookieOptions = new CookieOptions()
        {
            HttpOnly = true,
            Expires = refreshToken.ExpireTime
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

        user.RefreshToken.Token = refreshToken.Token;
        user.RefreshToken.CreateTime = refreshToken.CreateTime;
        user.RefreshToken.ExpireTime = refreshToken.ExpireTime;

        await _writeAppUserRepository.UpdateAsync(user);
        await _writeAppUserRepository.SaveChangesAsync();
    }

    public AppUser? GetUserDatas()
    {
        var accessToken = _httpContextAccessor.HttpContext?.Session.GetString("accessToken");
        var userVmJson = _httpContextAccessor.HttpContext?.Session.GetString("userVm");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(userVmJson))
            return null;

        var userVm = JsonConvert.DeserializeObject<AppUserVM>(userVmJson);

        return new AppUser()
        {
            UserName = userVm!.UserName,
            ConnectionId = userVm.ConnectionId
        };
    }

    public async Task<bool> EditUserAsync(int userId, EditUserDTO editUserDTO)
    {
        var user = await _readAppUserRepository.GetUserByIdAsync(userId);
        if (user == null)
            return false;

        if (!string.IsNullOrEmpty(editUserDTO.FirstName))
            user.FirstName = editUserDTO.FirstName;

        if (!string.IsNullOrEmpty(editUserDTO.LastName))
            user.LastName = editUserDTO.LastName;

        if (!string.IsNullOrEmpty(editUserDTO.UserName))
            user.UserName = editUserDTO.UserName;

        await _writeAppUserRepository.UpdateAsync(user);
        await _writeAppUserRepository.SaveChangesAsync();
        return true;
    }
}
