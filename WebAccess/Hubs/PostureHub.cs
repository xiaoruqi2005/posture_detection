using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace WebAccess.Hubs
{
    public class PostureHub : Hub
    {
        // 主要由服务器主动推送，所以这个 Hub 可能比较空
        private readonly ILogger<PostureHub> _logger;

        public PostureHub(ILogger<PostureHub> logger)
        {
            _logger = logger;
        }
        public override Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected to PostureHub: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected from PostureHub: {Context.ConnectionId}");
            if (exception != null)
            {
                _logger.LogError(exception, $"Client {Context.ConnectionId} disconnected with error.");
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
