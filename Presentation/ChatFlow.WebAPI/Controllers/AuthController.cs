using ChatFlow.Application.Services;
using ChatFlow.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatFlow.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Register([FromBody] AppUserDTO appUserDTO)
    {
        if (appUserDTO == null)
            return BadRequest("User data is required.");

        var result = await _authService.AddUserAsync(appUserDTO);
        return Ok(result);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var result = await _authService.LoginAsync(loginDTO);
        return Ok(result);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> EmailConfirm([FromQuery] string token)
    {
        var result = await _authService.EmailConfirmAsync(token);
        return Ok(result);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDTO forgetPasswordDTO)
    {
        var result = await _authService.ForgetPasswordAsync(forgetPasswordDTO);
        return Ok(result);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromBody] ResetPasswordDTO resetPasswordDTO)
    {
        var result = await _authService.ResetPasswordAsync(token, resetPasswordDTO);
        return Ok(result);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("[action]")]
    public IActionResult GetUserDatas()
    {
        var user = _authService.GetUserDatas();
        if (user == null)
            return Unauthorized(new { success = false, message = "Invalid user data" });

        return Ok(user);
    }
}
