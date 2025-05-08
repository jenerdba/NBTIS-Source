using Microsoft.AspNetCore.SignalR;

namespace NBTIS.Core.Hub
{
    public class MessageHub : Hub<IMessageHubClient>
    {
        public async Task SendProgress(List<string> message)
        {
            await Clients.All.SendProgress(message);
        }
    }
}

