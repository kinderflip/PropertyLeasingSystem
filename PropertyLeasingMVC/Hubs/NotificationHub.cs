using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PropertyLeasingMVC.Hubs
{
    // Hub used to push live notifications to one specific user.
    [Authorize]
    public class NotificationHub : Hub
    {
        // Client subscribes to its own user channel.
        public Task JoinUserGroup(string userId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
    }
}
