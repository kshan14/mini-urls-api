using MiniUrl.Services.Websockets;
using MiniUrl.Websockets;

namespace MiniUrl.Services.Background;

public class WebSocketManagerScheduler : BackgroundService
{
    private readonly ILogger<WebSocketManagerScheduler> _logger;
    private readonly IWebSocketManager _webSocketManager;
    private readonly TimeSpan _pingInterval;

    public WebSocketManagerScheduler(ILogger<WebSocketManagerScheduler> logger, IWebSocketManager webSocketManager)
    {
        _logger = logger;
        _webSocketManager = webSocketManager;
        _pingInterval = TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WebSocketManagerScheduler started");
        while (!stoppingToken.IsCancellationRequested)
        {
            await _webSocketManager.PingAndRemoveAllConnections();
            await Task.Delay(_pingInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WebSocketManagerScheduler stopped");
        await _webSocketManager.StopAndClearAllConnections();
    }
}
