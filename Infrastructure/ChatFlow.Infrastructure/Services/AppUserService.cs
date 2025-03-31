using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Application.Repositories.Writes;
using ChatFlow.Application.Services;
using ChatFlow.Domain.DTOs;
using ChatFlow.Domain.Entities.Concretes;
using ChatFlow.Domain.ViewModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ChatFlow.Infrastructure.Services;

public class AppUserService : IAppUserService
{
    private readonly IAppUserReadRepository _readAppUserRepository;
    private readonly IAppUserWriteRepository _writeAppUserRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppUserService(IAppUserReadRepository readAppUserRepository, IAppUserWriteRepository writeAppUserRepository, 
        IHttpContextAccessor httpContextAccessor)
    {
        _readAppUserRepository = readAppUserRepository;
        _writeAppUserRepository = writeAppUserRepository;
        _httpContextAccessor = httpContextAccessor;
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

    public async Task<bool> UpdateUserConnectionIdAsync(int userId, string connectionId)
    {
        var user = await _readAppUserRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        user.ConnectionId = connectionId;

        await _writeAppUserRepository.UpdateAsync(user);
        await _writeAppUserRepository.SaveChangesAsync();

        return true;
    }

    public async Task<List<AppUserVM>> GetAllClientsAsync()
    {
        var users = await _readAppUserRepository.GetAllAsync();
        var validClients = users.Where(user => !string.IsNullOrEmpty(user.ConnectionId)).ToList();

        return validClients.Select(user => new AppUserVM
        {
            UserName = user.UserName,
            ConnectionId = user.ConnectionId
        }).ToList();
    }
}