using System.Collections.Concurrent;
using System.Net.WebSockets;
using MiniUrl.Services.Websockets;

namespace MiniUrl.Websockets;

public class WebSocketManager : IWebSocketManager
{
    private readonly ILogger<WebSocketManager> _logger;
    private readonly ConcurrentDictionary<Guid, WebSocketClient> _adminSockets;
    private readonly ConcurrentDictionary<Guid, WebSocketClient> _userSockets;
    private readonly TimeSpan _pingInterval;
    private readonly TimeSpan _pongTimeout;

    public WebSocketManager(ILogger<WebSocketManager> logger)
    {
        _logger = logger;
        _adminSockets = new();
        _userSockets = new();
        _pingInterval = TimeSpan.FromSeconds(30);
        _pongTimeout = TimeSpan.FromSeconds(10);
    }

    public async Task AddAdminConnectionAsync(Guid userId, WebSocket socket)
    {
        var client = new WebSocketClient(userId, socket);
        if (_adminSockets.TryAdd(userId, client))
        {
            _logger.LogInformation("Added admin websocket with userId: {UserId}", userId);
            await ListenForPongAsync(client, RemoveAdminConnectionAsync);
            return;
        }

        _logger.LogWarning("Failed to add admin websocket with userId: {UserId}", userId);
    }

    public async Task RemoveAdminConnectionAsync(Guid userId)
    {
        try
        {
            await RemoveConnectionAsync(userId, _adminSockets);
            _logger.LogInformation("Removed admin websocket with userId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove admin websocket with userId: {UserId}", userId);
        }
    }

    public async Task AddUserConnectionAsync(Guid userId, WebSocket socket)
    {
        var client = new WebSocketClient(userId, socket);
        if (_userSockets.TryAdd(userId, client))
        {
            _logger.LogInformation("Added user websocket with userId: {UserId}", userId);
            await ListenForPongAsync(client, RemoveUserConnectionAsync);
        }

        _logger.LogWarning("Failed to add user websocket with userId: {UserId}", userId);
    }

    public async Task RemoveUserConnectionAsync(Guid userId)
    {
        try
        {
            await RemoveConnectionAsync(userId, _userSockets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove user websocket with userId: {UserId}", userId);
        }
    }

    public Task StopAndClearAllConnections()
    {
        // get all admins and users websocket connections and close one by one
        var allWebSockets = _adminSockets.Concat(_userSockets)
            .Select(x => x.Value)
            .ToList();
        foreach (var clientWebSocket in allWebSockets)
        {
            clientWebSocket.CancellationTokenSource.Cancel();
            clientWebSocket.Socket.Dispose();
            clientWebSocket.CancellationTokenSource.Dispose();
        }

        _adminSockets.Clear();
        _userSockets.Clear();
        return Task.CompletedTask;
    }

    public async Task PingAndRemoveAllConnections()
    {
        try
        {
            _logger.LogInformation("Running ping and remove all admins and users websockets");
            await Task.WhenAll(PingAndRemoveAllAdmins(), PingAndRemoveAllUsers());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running ping and remove all admins and users websockets");
        }
    }

    public async Task SendToAllAdminsAsync(byte[] message)
    {
        var tasks = _adminSockets.Values.Select(adminSocket => Task.Run(async () =>
        {
            try
            {
                await adminSocket.Socket.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogDebug("Sent admin websocket message: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin websocket message: {Message}", message);
            }
        })).ToList();
        await Task.WhenAll(tasks);
        _logger.LogInformation("Sent message to all admin websockets");
    }

    public async Task SendToUserAsync(Guid userId, byte[] message)
    {
        var isExists = _userSockets.TryGetValue(userId, out var client);
        if (!isExists || client == null)
        {
            _logger.LogDebug("User with userId: {UserId} does not exist", userId);
            return;
        }

        try
        {
            await client.Socket.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
            _logger.LogDebug("Sent user websocket message: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending user websocket message: {Message}", message);
        }
    }

    private async Task PingAndRemoveAllAdmins()
    {
        _logger.LogInformation("PingAndRemoveAllAdmins started");
        await PingAndRemoveWebSockets(_adminSockets);
        _logger.LogInformation("PingAndRemoveAllAdmins finished");
    }

    private async Task PingAndRemoveAllUsers()
    {
        _logger.LogInformation("PingAndRemoveAllUsers started");
        await PingAndRemoveWebSockets(_userSockets);
        _logger.LogInformation("PingAndRemoveAllUsers finished");
    }

    private async Task PingAndRemoveWebSockets(ConcurrentDictionary<Guid, WebSocketClient> sockets)
    {
        var now = DateTime.UtcNow;
        var tasks = new List<Task>();
        var connectionsToRemove = new ConcurrentBag<Guid>();

        // iterate through each socket connection. Getting blocked at individual socket should not interrupt/delay other socket connections.
        foreach (var (_, socketClient) in sockets)
        {
            tasks.Add(Task.Run(async () =>
            {
                // check if client didn't respond back within ping interval + pong timeout or client connection is no longer open
                var pongTimeoutExceeds = now - socketClient.LastPongReceived > _pingInterval + _pongTimeout;
                var isConnectionAlreadyClosed = socketClient.Socket.State != WebSocketState.Open;
                if (pongTimeoutExceeds || isConnectionAlreadyClosed)
                {
                    _logger.LogInformation(
                        "Going to remove websocket with userId: {UserId} as pong timeout exceeds or connection already closed",
                        socketClient.Id);
                    connectionsToRemove.Add(socketClient.Id);
                    return;
                }

                // not timeout yet, ping again
                try
                {
                    await socketClient.Socket.SendAsync(new ArraySegment<byte>([]), WebSocketMessageType.Text, true,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ping websocket with userId: {UserId} and going to remove",
                        socketClient.Id);
                    connectionsToRemove.Add(socketClient.Id);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // now connections to be removed is collected. Begin removing.
        foreach (var userId in connectionsToRemove)
        {
            try
            {
                await RemoveConnectionAsync(userId, sockets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove websocket with userId: {UserId}", userId);
            }
        }
    }

    private async Task RemoveConnectionAsync(Guid userId, ConcurrentDictionary<Guid, WebSocketClient> map)
    {
        var isExist = map.Remove(userId, out var client);
        if (!isExist || client == null)
        {
            return;
        }

        await client.CancellationTokenSource.CancelAsync();
        if (client.Socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by server",
                CancellationToken.None);
        }

        client.Socket.Dispose();
        client.CancellationTokenSource.Dispose();
    }

    private async Task ListenForPongAsync(WebSocketClient webSocketClient, Func<Guid, Task> removeWebSocketFunc)
    {
        var buffer = new byte[1024 * 4];
        try
        {
            // listens messages coming from client. If client sends Close message, terminate and clean up resources.
            while (webSocketClient.Socket.State == WebSocketState.Open &&
                   !webSocketClient.CancellationTokenSource.IsCancellationRequested)
            {
                var result = await webSocketClient.Socket.ReceiveAsync(new ArraySegment<byte>(buffer),
                    webSocketClient.CancellationTokenSource.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Websocket closed");
                    await removeWebSocketFunc(webSocketClient.Id);
                    break;
                }

                webSocketClient.LastPongReceived = DateTime.UtcNow;
                _logger.LogDebug("Pong received from {UserId}", webSocketClient.Id);
            }
        }
        catch (OperationCanceledException) // cancellation initiated by other logic so no need to close
        {
            _logger.LogInformation("Listener cancelled for {UserId}", webSocketClient.Id);
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket error for {UserId}", webSocketClient.Id);
            await removeWebSocketFunc(webSocketClient.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while listening for {UserId}", webSocketClient.Id);
            await removeWebSocketFunc(webSocketClient.Id);
        }
    }
}
