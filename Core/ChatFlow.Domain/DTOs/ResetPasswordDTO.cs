using System.ComponentModel.DataAnnotations;

namespace ChatFlow.Domain.DTOs;

public class ResetPasswordDTO
{
    public string Password { get; set; }
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}
