using Microsoft.AspNetCore.SignalR;

namespace PropertyLeasingMVC.Hubs
{
    public class MaintenanceHub : Hub
    {
        public async Task SendStatusUpdate(int requestId, string status, string title)
        {
            await Clients.All.SendAsync("ReceiveStatusUpdate", requestId, status, title);
        }
    }
}