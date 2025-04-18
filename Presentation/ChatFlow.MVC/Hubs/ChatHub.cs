﻿using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Application.Repositories.Writes;
using ChatFlow.Application.Services;
using ChatFlow.Domain.Entities.Concretes;
using ChatFlow.Domain.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace ChatFlow.MVC.Hubs;

public class ChatHub : Hub
{
    private readonly IAppUserService _appUserService;
    private readonly IGroupService _groupService;
    private readonly IAppUserReadRepository _readAppUserRepository;
    private readonly IGroupReadRepository _readGroupRepository;
    private readonly IGroupWriteRepository _writeGroupRepository;
    private List<AppUser> clients;

    public ChatHub(IAppUserService appUserService, IAppUserReadRepository readAppUserRepository,
        IGroupReadRepository readGroupRepository, IGroupService groupService, IGroupWriteRepository writeGroupRepository)
    {
        _appUserService = appUserService;
        _readAppUserRepository = readAppUserRepository;
        _readGroupRepository = readGroupRepository;
        _groupService = groupService;
        _writeGroupRepository = writeGroupRepository;
        clients = new List<AppUser>();
    }

    private async Task LoadClientsAsync()
    {
        clients = await _appUserService.GetAllClientsAsync();
    }

    public async Task GetClientName(string clientName)
    {
        var user = await _readAppUserRepository.GetUserByUserName(clientName);
        if (user == null)
            return;

        await _appUserService.UpdateUserConnectionIdAsync(user.Id, Context.ConnectionId);
        await Clients.Others.SendAsync("ReceiveClientName", clientName);

        await LoadClientsAsync();
        await Clients.All.SendAsync("GetClients", clients);
    }

    public async Task DisconnectUser(string clientName)
    {
        await Clients.Others.SendAsync("UserDisconnected", clientName);
        await LoadClientsAsync();
        var disconnectedUser = clients.FirstOrDefault(c => c.UserName == clientName);

        if (disconnectedUser != null)
        {
            clients.Remove(disconnectedUser);

            var user = await _readAppUserRepository.GetUserByUserName(clientName);
            if (user != null)
            {
                var groups = await _readGroupRepository.GetGroupsByUserIdAsync(user.Id);
                foreach (var group in groups)
                {
                    group.AppUsers.Remove(user);
                    await _writeGroupRepository.UpdateAsync(group);
                    await _writeGroupRepository.SaveChangesAsync();
                }

                user.ConnectionId = null; 
                await _appUserService.UpdateUserConnectionIdAsync(user.Id, null);
            }
            var userGroups = await _readGroupRepository.GetGroupsByUserIdAsync(disconnectedUser.Id);
            foreach (var group in userGroups)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.GroupName);
            }

            await Clients.All.SendAsync("GetClients", clients);
        }
    }

    public async Task SendMessageAsync(string message, string clientName)
    {
        await LoadClientsAsync();
        clientName = clientName.Trim();
        AppUser clientSender = clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!;

        if (clientName == "All")
            await Clients.Others.SendAsync("ReceiveMessage", message, clientSender.UserName);
        else
        {
            AppUser client = clients.FirstOrDefault(c => c.UserName == clientName)!;
            await Clients.Client(client.ConnectionId).SendAsync("ReceiveMessage", message, clientSender.UserName);
        }
    }

    public async Task AddGroup(string groupName)
    {
        await LoadClientsAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var user = await _readAppUserRepository.GetUserByConnectionIdWithRelationsAsync(Context.ConnectionId);

        var existingGroup = await _readGroupRepository.GetGroupByNameAsync(groupName);
        if (existingGroup != null)
        {
            await Clients.Caller.SendAsync("ReceiveMessageAlert", "Group name already exists.");
            return;
        }

        Group group = new()
        {
            GroupName = groupName,
            AppUsers = new List<AppUser> { user }
        };

        await _groupService.AddGroupAsync(group);

        var allGroups = await _readGroupRepository.GetAllGroupsAsync(); 
        var groupVMs = allGroups.Select(g => new GroupVM
        {
            GroupName = g.GroupName,
            AppUsers = g.AppUsers.Select(u => new AppUserVM { UserName = u.UserName }).ToList()
        }).ToList();

        await Clients.All.SendAsync("Groups", groupVMs);
    }

    public async Task GetGroups()
    {
        var groups = await _readGroupRepository.GetAllGroupsAsync();
        var groupVMs = groups.Select(g => new GroupVM
        {
            GroupName = g.GroupName,
            AppUsers = g.AppUsers.Select(u => new AppUserVM { UserName = u.UserName }).ToList()
        }).ToList();

        await Clients.Caller.SendAsync("Groups", groupVMs);
    }

    // Logout olduqdan sonra mesaj getmir
    public async Task AddClientToGroup(string[] groupNames)
    {
        await LoadClientsAsync();
        var user = await _readAppUserRepository.GetUserByConnectionIdWithRelationsAsync(Context.ConnectionId);

        foreach (var groupName in groupNames)
        {
            var _group = await _readGroupRepository.GetGroupByNameAsync(groupName);

            if (_group != null)
            {
                var result = _group.AppUsers.Any(g => g.ConnectionId == Context.ConnectionId);
                if (!result)
                {
                    _group.AppUsers.Add(user);
                    await _writeGroupRepository.UpdateAsync(_group);
                    await _writeGroupRepository.SaveChangesAsync();

                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    await GetClientInGroup(groupName);
                }
            }
        }
    }

    public override async Task OnConnectedAsync()
    {
        var user = await _readAppUserRepository.GetUserByConnectionIdWithRelationsAsync(Context.ConnectionId);
        if (user != null)
        {
            await _appUserService.UpdateUserConnectionIdAsync(user.Id, Context.ConnectionId);
            var groups = await _readGroupRepository.GetGroupsByUserIdAsync(user.Id);
            foreach (var group in groups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, group.GroupName);
            }
        }
        await LoadClientsAsync();
        await Clients.All.SendAsync("GetClients", clients);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await _readAppUserRepository.GetUserByConnectionIdWithRelationsAsync(Context.ConnectionId);
        if (user != null)
        {
            await _appUserService.UpdateUserConnectionIdAsync(user.Id, null);
            clients.RemoveAll(c => c.ConnectionId == Context.ConnectionId);

            var userGroups = await _readGroupRepository.GetGroupsByUserIdAsync(user.Id);

            foreach (var group in userGroups)
            {
                group.AppUsers.Remove(user);
                await _writeGroupRepository.UpdateAsync(group);
                await _writeGroupRepository.SaveChangesAsync();

                if (!group.AppUsers.Any())
                {
                    await _writeGroupRepository.DeleteAsync(group);
                    await _writeGroupRepository.SaveChangesAsync();
                }
                else
                {
                    await _writeGroupRepository.UpdateAsync(group);
                    await _writeGroupRepository.SaveChangesAsync();
                }
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.GroupName);
            }
        }
        await Clients.All.SendAsync("GetClients", clients);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetClientInGroup(string groupName)
    {
        var group = await _readGroupRepository.GetGroupByNameAsync(groupName);

        if (group == null)
        {
            await Clients.Caller.SendAsync("ReceiveMessageAlert", $"{groupName} not found.");
            return;
        }

        var clients = group.AppUsers.Select(u => new AppUserVM
        {
            UserName = u.UserName
        }).ToList();

        await Clients.Caller.SendAsync("GetClients", clients);
    }

    public async Task SendMessageToGroupAsync(string message, string groupName)
    {
        var sender = await _readAppUserRepository.GetUserByConnectionIdWithRelationsAsync(Context.ConnectionId);
        var group = await _readGroupRepository.GetGroupByNameAsync(groupName);

        if (!group.AppUsers.Any(u => u.Id == sender.Id))
        {
            await Clients.Caller.SendAsync("ReceiveMessageAlert", "You are not a member of this group.");
            return;
        }

        await Clients.Group(groupName).SendAsync("ReceiveGroupMessageAsync", message, sender.UserName, groupName);
    }
}
