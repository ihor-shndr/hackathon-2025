using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Hubs;
using MyChat.Models.DTOs.Messages;

namespace MyChat.Services;

public class SignalRService : ISignalRService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ChatDbContext _context;

    public SignalRService(IHubContext<ChatHub> hubContext, ChatDbContext context)
    {
        _hubContext = hubContext;
        _context = context;
    }

    public async Task SendDirectMessageAsync(int senderId, int recipientId, MessageDto message)
    {
        try
        {
            // Send to recipient
            await SendToUser(recipientId, "ReceiveDirectMessage", message);
            
            // Send back to sender (for multi-device sync)
            await SendToUser(senderId, "ReceiveDirectMessage", message);

            Console.WriteLine($"SignalR: Direct message sent from {senderId} to {recipientId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Error sending direct message: {ex.Message}");
        }
    }

    public async Task SendGroupMessageAsync(int senderId, int groupId, MessageDto message)
    {
        try
        {
            // Get all group members
            var groupMemberIds = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.IsActive)
                .Select(gm => gm.UserId)
                .ToListAsync();

            // Send to all group members
            foreach (var memberId in groupMemberIds)
            {
                await SendToUser(memberId, "ReceiveGroupMessage", message);
            }

            Console.WriteLine($"SignalR: Group message sent from {senderId} to group {groupId} ({groupMemberIds.Count} members)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Error sending group message: {ex.Message}");
        }
    }

    public async Task NotifyConversationUpdateAsync(int userId, ConversationDto conversation)
    {
        try
        {
            await SendToUser(userId, "ConversationUpdated", conversation);
            Console.WriteLine($"SignalR: Conversation update sent to user {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Error sending conversation update: {ex.Message}");
        }
    }

    public async Task JoinGroupAsync(int userId, int groupId)
    {
        try
        {
            // Get user's connections
            var connections = ChatHub.GetUserConnections(userId);
            if (connections != null)
            {
                foreach (var connectionId in connections)
                {
                    await _hubContext.Groups.AddToGroupAsync(connectionId, $"Group_{groupId}");
                }
                Console.WriteLine($"SignalR: User {userId} joined group {groupId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Error joining group: {ex.Message}");
        }
    }

    public async Task LeaveGroupAsync(int userId, int groupId)
    {
        try
        {
            // Get user's connections
            var connections = ChatHub.GetUserConnections(userId);
            if (connections != null)
            {
                foreach (var connectionId in connections)
                {
                    await _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"Group_{groupId}");
                }
                Console.WriteLine($"SignalR: User {userId} left group {groupId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Error leaving group: {ex.Message}");
        }
    }

    public bool IsUserOnline(int userId)
    {
        return ChatHub.IsUserOnline(userId);
    }

    /// <summary>
    /// Send a message to a specific user across all their connections
    /// </summary>
    private async Task SendToUser(int userId, string method, object message)
    {
        var connections = ChatHub.GetUserConnections(userId);
        if (connections != null)
        {
            foreach (var connectionId in connections)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(method, message);
            }
        }
    }
}
