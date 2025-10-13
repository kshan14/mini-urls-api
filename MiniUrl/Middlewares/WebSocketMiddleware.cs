using System.Net;
using MiniUrl.Entities;
using MiniUrl.Services;
using MiniUrl.Websockets;

namespace MiniUrl.Middlewares;

public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WebSocketMiddleware> _logger;
    private readonly ITokenService _tokenService;
    private readonly IWebSocketManager _webSocketManager;

    public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger, ITokenService tokenService,
        IWebSocketManager webSocketManager)
    {
        _next = next;
        _logger = logger;
        _tokenService = tokenService;
        _webSocketManager = webSocketManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Query["token"].ToString();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogInformation("token is missing in websocket request query");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        try
        {
            var user = _tokenService.GetUserFromToken(token);
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            if (user.Role.Equals(Role.Admin))
            {
                await _webSocketManager.AddAdminConnectionAsync(user.Id, socket);
            }
            else
            {
                await _webSocketManager.AddUserConnectionAsync(user.Id, socket);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while trying to parse user from token");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}