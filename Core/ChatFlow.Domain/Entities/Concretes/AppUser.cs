using ChatFlow.Domain.Entities.Commons;

namespace ChatFlow.Domain.Entities.Concretes;

public class AppUser : BaseEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    // PasswordSalt => bizim ucun hem passwordun hash formasini saxlayacaq,
    // hemde o passwordun hansi key-e gore hash olunubsa onu saxlayacaq.
    public byte[] PasswordSalt { get; set; }
    public string Role { get; set; }
    public bool EmailConfirm { get; set; } = false;
    public string? ConnectionId { get; set; }

    // Navigation Property
    public Token Token { get; set; }
    public ICollection<UserAgent> UserAgents { get; set; }
    public EmailConfirmToken EmailConfirmToken { get; set; }
    public RefreshToken RefreshToken { get; set; }
    public RePasswordToken RePasswordToken { get; set; }
    public ICollection<Group> Groups { get; set; }
    public ICollection<Message> Messages { get; set; }
}