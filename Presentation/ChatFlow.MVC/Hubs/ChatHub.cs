using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Application.Services;
using ChatFlow.Domain.ViewModels;
using ChatFlow.MVC.Data;
using ChatFlow.MVC.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatFlow.MVC.Hubs;

public class ChatHub : Hub
{
    private readonly IAppUserService _appUserService;
    private readonly IAppUserReadRepository _readAppUserRepository;
    private List<AppUserVM> clients;

    public ChatHub(IAppUserService appUserService, IAppUserReadRepository readAppUserRepository)
    {
        _appUserService = appUserService;
        _readAppUserRepository = readAppUserRepository;
        clients = new List<AppUserVM>(); ;
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
                user.ConnectionId = null; 
                await _appUserService.UpdateUserConnectionIdAsync(user.Id, null);
            }

            await Clients.All.SendAsync("GetClients", clients);
        }
    }

    public async Task SendMessageAsync(string message, string clientName)
    {
        await LoadClientsAsync();
        clientName = clientName.Trim();
        AppUserVM clientSender = clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!;

        if (clientName == "All")
            await Clients.Others.SendAsync("ReceiveMessage", message, clientSender.UserName);
        else
        {
            AppUserVM client = clients.FirstOrDefault(c => c.UserName == clientName)!;
            await Clients.Client(client.ConnectionId).SendAsync("ReceiveMessage", message, clientSender.UserName);
        }
    }

    public async Task AddGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        Group group = new Group { GroupName = groupName};
        group.Clients.Add(clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!);

        GroupSource.Groups.Add(group);
        await Clients.All.SendAsync("Groups", GroupSource.Groups);
    }

    public async Task AddClientToGroup(IEnumerable<string> groupNames)
    {
        await LoadClientsAsync();
        AppUserVM client = clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!;

        foreach (var group in groupNames)
        {
            Group _group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == group)!;

            var result = _group.Clients.Any(g => g.ConnectionId == Context.ConnectionId);
            if (_group != null && !result)
            {
                _group.Clients.Add(client);
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
                // After adding the client to the group, call GetClientInGroup to update the clients
                await GetClientInGroup(group);
            }
        }
    }

    public async Task GetClientInGroup(string groupName)
    {
        Group group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName)!;

        if (group != null)
            await Clients.Caller.SendAsync("GetClients", group.Clients);
    }

    public async Task SendMessageToGroupAsync(string message, string groupName)
    {
        await LoadClientsAsync();
        AppUserVM clientSender = clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!;
        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", message, clientSender.UserName, groupName);
    }
}
