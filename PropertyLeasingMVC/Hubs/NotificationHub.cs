using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace PropertyLeasingMVC.Hubs
{
    // L11: pushes new in-system notifications to a per-user group so the bell badge
    // and toast can update without a full page reload.
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
