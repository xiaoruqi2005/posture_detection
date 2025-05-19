using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace WebAccess.Hubs
{
    public class PostureHub : Hub
    {
        // 主要由服务器主动推送，所以这个 Hub 可能比较空
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected to PostureHub: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client disconnected from PostureHub: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
