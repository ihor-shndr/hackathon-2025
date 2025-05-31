using MyChat.Models.DTOs.Messages;

namespace MyChat.Services;

public interface ISignalRService
{
    /// <summary>
    /// Broadcast a direct message to the recipient and sender
    /// </summary>
    Task SendDirectMessageAsync(int senderId, int recipientId, MessageDto message);

    /// <summary>
    /// Broadcast a group message to all group members
    /// </summary>
    Task SendGroupMessageAsync(int senderId, int groupId, MessageDto message);

    /// <summary>
    /// Notify users about conversation updates
    /// </summary>
    Task NotifyConversationUpdateAsync(int userId, ConversationDto conversation);

    /// <summary>
    /// Join a user to a group for real-time updates
    /// </summary>
    Task JoinGroupAsync(int userId, int groupId);

    /// <summary>
    /// Remove a user from a group
    /// </summary>
    Task LeaveGroupAsync(int userId, int groupId);

    /// <summary>
    /// Check if a user is currently online
    /// </summary>
    bool IsUserOnline(int userId);
}
