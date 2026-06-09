using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DormHub.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static readonly List<ChatMessage> _messages = new();
        private static readonly object _lock = new();

        public override async Task OnConnectedAsync()
        {
            List<ChatMessage> snapshot;
            lock (_lock)
            {
                snapshot = _messages.ToList();
            }

            foreach (var msg in snapshot)
            {
                await Clients.Caller.SendAsync("ReceiveMessage",
                    msg.User, msg.Text, msg.SentAt.ToString("HH:mm"));
            }

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

            var chatMsg = new ChatMessage
            {
                User = userName,
                Text = message.Trim(),
                SentAt = now
            };

            lock (_lock)
            {
                _messages.Add(chatMsg);

                if (_messages.Count > 200)
                    _messages.RemoveAt(0);
            }

            await Clients.All.SendAsync("ReceiveMessage",
                chatMsg.User, chatMsg.Text, chatMsg.SentAt.ToString("HH:mm"));
        }
    }

    public class ChatMessage
    {
        public string User { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
