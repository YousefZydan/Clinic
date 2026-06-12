using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {

    }
}

