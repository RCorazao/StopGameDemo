using Microsoft.AspNetCore.SignalR;
using StopGame.Application.Interfaces;
using StopGame.Web.Hubs;

namespace StopGame.Web.Services;

public class SignalRService : ISignalRService
{
    private readonly IHubContext<GameHub> _hubContext;

    public SignalRService(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToGroupAsync(string groupName, string method, object data)
    {
        await _hubContext.Clients.Group(groupName).SendAsync(method, data);
    }
}