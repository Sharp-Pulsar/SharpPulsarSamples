using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace WorkerService.Hubs
{
    public class EchoHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("Connection", Context.ConnectionId, DateTime.Now.ToString());
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync("Disconnection", Context.ConnectionId, DateTime.Now.ToString());
            await base.OnDisconnectedAsync(exception);
        }
    }
}
