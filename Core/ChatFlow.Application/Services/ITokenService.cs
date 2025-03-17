using ChatFlow.Domain.Entities.Concretes;

namespace ChatFlow.Application.Services;

public interface ITokenService
{
    string CreateAccessToken(AppUser user);
    RefreshToken CreateRefreshToken();
    RefreshToken CreateRepasswordToken();
    RefreshToken CreateEmailConfirmToken();
}
