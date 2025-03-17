using ChatFlow.MVC.Data;
using ChatFlow.MVC.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatFlow.MVC.Hubs;

public class ChatHub : Hub
{
    public async Task GetClientName(string clientName)
    {
        Client client = new()
        {
            ConnectionId = Context.ConnectionId,
            ClientName = clientName
        };

        ClientSource.Clients.Add(client);
        await Clients.Others.SendAsync("ReceiveClientName", clientName);
        await Clients.All.SendAsync("GetClients", ClientSource.Clients);
    }

    public async Task SendMessageAsync(string message, string clientName)
    {
        clientName = clientName.Trim();
        Client clientSender = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!;

        if (clientName == "All")
        {
            await Clients.Others.SendAsync("ReceiveMessage", message, clientSender.ClientName);
        }
        else
        {
            Client client = ClientSource.Clients.FirstOrDefault(c => c.ClientName == clientName)!;
            await Clients.Client(client.ConnectionId).SendAsync("ReceiveMessage", message, clientSender.ClientName);
        }
    }

    public async Task AddGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        Group group = new Group { GroupName = groupName};
        group.Clients.Add(ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!);

        GroupSource.Groups.Add(group);
        await Clients.All.SendAsync("Groups", GroupSource.Groups);
    }

    public async Task AddClientToGroup(IEnumerable<string> groupNames)
    {
        Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!;

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
        {
            await Clients.Caller.SendAsync("GetClients", group.Clients);
        }
    }

    public async Task SendMessageToGroupAsync(string message, string groupName)
    {
        Client clientSender = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)!;
        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", message, clientSender.ClientName, groupName);
    }
}
