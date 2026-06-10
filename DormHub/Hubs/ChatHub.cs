using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DormHub.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonim";
            await Clients.Others.SendAsync("UserJoined", userName);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonim";
            await Clients.Others.SendAsync("UserLeft", userName);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonim";
            var now = DateTime.Now;

            await Clients.All.SendAsync("ReceiveMessage",
                userName, message.Trim(), now.ToString("HH:mm"));
        }
    }
}
