using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace MyChat.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // Store user connections - In production, use Redis or database for scaling
    private static readonly ConcurrentDictionary<int, HashSet<string>> UserConnections = new();
    private static readonly ConcurrentDictionary<string, int> ConnectionUsers = new();

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            var connectionId = Context.ConnectionId;
            
            // Add user connection
            UserConnections.AddOrUpdate(userId.Value,
                new HashSet<string> { connectionId },
                (key, existing) =>
                {
                    existing.Add(connectionId);
                    return existing;
                });

            ConnectionUsers[connectionId] = userId.Value;

            Console.WriteLine($"User {userId} connected with connection {connectionId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        
        if (ConnectionUsers.TryRemove(connectionId, out var userId))
        {
            if (UserConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    UserConnections.TryRemove(userId, out _);
                }
            }

            Console.WriteLine($"User {userId} disconnected from connection {connectionId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a user to a specific group for group messaging
    /// </summary>
    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{groupId}");
        Console.WriteLine($"Connection {Context.ConnectionId} joined group {groupId}");
    }

    /// <summary>
    /// Leave a specific group
    /// </summary>
    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_{groupId}");
        Console.WriteLine($"Connection {Context.ConnectionId} left group {groupId}");
    }

    /// <summary>
    /// Send a direct message to a specific user
    /// </summary>
    public async Task SendDirectMessage(int recipientId, object message)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        // Send to recipient
        await SendToUser(recipientId, "ReceiveDirectMessage", message);
        
        // Send back to sender (for multi-device sync)
        await SendToUser(senderId.Value, "ReceiveDirectMessage", message);

        Console.WriteLine($"Direct message sent from {senderId} to {recipientId}");
    }

    /// <summary>
    /// Send a group message to all group members
    /// </summary>
    public async Task SendGroupMessage(int groupId, object message)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        // Send to all users in the group
        await Clients.Group($"Group_{groupId}").SendAsync("ReceiveGroupMessage", message);

        Console.WriteLine($"Group message sent from {senderId} to group {groupId}");
    }

    /// <summary>
    /// Notify users about conversation updates (new message in conversation list)
    /// </summary>
    public async Task NotifyConversationUpdate(int userId, object conversation)
    {
        await SendToUser(userId, "ConversationUpdated", conversation);
    }

    /// <summary>
    /// Send a message to a specific user across all their connections
    /// </summary>
    private async Task SendToUser(int userId, string method, object message)
    {
        if (UserConnections.TryGetValue(userId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync(method, message);
            }
        }
    }

    /// <summary>
    /// Get the current user ID from JWT claims
    /// </summary>
    private int? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Get all connected users (for admin/debugging purposes)
    /// </summary>
    public int GetConnectedUsersCount()
    {
        return UserConnections.Count;
    }

    /// <summary>
    /// Check if a user is online
    /// </summary>
    public static bool IsUserOnline(int userId)
    {
        return UserConnections.ContainsKey(userId);
    }

    /// <summary>
    /// Get connection IDs for a specific user
    /// </summary>
    public static HashSet<string>? GetUserConnections(int userId)
    {
        UserConnections.TryGetValue(userId, out var connections);
        return connections;
    }
}
