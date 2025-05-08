using Microsoft.AspNetCore.SignalR;
using NBTIS.Core.Interfaces;
using NBTIS.Web.Hubs;

namespace NBTIS.Web.Services
{
    public class SignalRProgressNotifier : IProgressNotifier
    {
        private readonly IHubContext<ProgressHub> _hubContext;

        public SignalRProgressNotifier(IHubContext<ProgressHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task UpdateProgressAsync(string connectionId, int percent)
        {
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("GetPercentComplete", percent);
        }
    }
}
