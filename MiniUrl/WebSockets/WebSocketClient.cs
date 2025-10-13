using System.Net.WebSockets;

namespace MiniUrl.Services.Websockets;

public class WebSocketClient
{
    public Guid Id { get; set; }
    public WebSocket Socket { get; set; }
    public DateTime LastPongReceived { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }

    public WebSocketClient(Guid id, WebSocket socket)
    {
        Id = id;
        Socket = socket;
        LastPongReceived = DateTime.UtcNow;
        CancellationTokenSource = new CancellationTokenSource();
    }
}