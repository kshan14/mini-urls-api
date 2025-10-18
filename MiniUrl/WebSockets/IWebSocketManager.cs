using System.Net.WebSockets;

namespace MiniUrl.Websockets;

public interface IWebSocketManager
{
    Task AddAdminConnectionAsync(Guid userId, WebSocket socket);
    Task RemoveAdminConnectionAsync(Guid userId);
    Task AddUserConnectionAsync(Guid userId, WebSocket socket);
    Task RemoveUserConnectionAsync(Guid userId);
    Task PingAndRemoveAllConnections();
    Task StopAndClearAllConnections();
}
