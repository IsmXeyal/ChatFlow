using ChatFlow.Domain.DTOs;
using ChatFlow.Domain.Entities.Concretes;
using ChatFlow.Domain.ViewModels;

namespace ChatFlow.Application.Services;

public interface IAppUserService
{
    AppUser? GetUserDatas();
    Task<bool> EditUserAsync(int userId, EditUserDTO editUserDTO);
    Task<bool> UpdateUserConnectionIdAsync(int userId, string connectionId);
    Task<List<AppUserVM>> GetAllClientsAsync();
}
