using Microsoft.AspNetCore.SignalR;

namespace PropertyLeasingMVC.Hubs
{
    // Plan B13: use groups so status updates aren't broadcast to everyone.
    // "live-board"    → dashboards / index pages that want every update.
    // "request-{id}"  → pages for a specific maintenance ticket.
    public class MaintenanceHub : Hub
    {
        public Task JoinLiveBoard() => Groups.AddToGroupAsync(Context.ConnectionId, "live-board");
        public Task LeaveLiveBoard() => Groups.RemoveFromGroupAsync(Context.ConnectionId, "live-board");

        public Task JoinRequestGroup(int requestId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"request-{requestId}");
        public Task LeaveRequestGroup(int requestId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"request-{requestId}");

        // Back-compat: existing clients can still call SendStatusUpdate directly.
        public async Task SendStatusUpdate(int requestId, string status, string title, string unitNumber = "")
        {
            await Clients.Group("live-board").SendAsync("ReceiveStatusUpdate", requestId, status, title, unitNumber);
            await Clients.Group($"request-{requestId}").SendAsync("ReceiveStatusUpdate", requestId, status, title, unitNumber);
        }
    }
}
