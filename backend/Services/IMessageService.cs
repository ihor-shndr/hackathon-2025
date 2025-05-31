using MyChat.Models.DTOs.Common;
using MyChat.Models.DTOs.Messages;

namespace MyChat.Services;

public interface IMessageService
{
    // Send messages
    Task<ApiResponse<MessageDto>> SendDirectMessageAsync(SendDirectMessageDto request, int senderId);
    Task<ApiResponse<MessageDto>> SendGroupMessageAsync(SendGroupMessageDto request, int senderId);
    
    // Retrieve conversations
    Task<ApiResponse<IEnumerable<MessageDto>>> GetDirectMessagesAsync(int userId, int contactId, int page = 1, int pageSize = 50);
    Task<ApiResponse<IEnumerable<MessageDto>>> GetGroupMessagesAsync(int groupId, int userId, int page = 1, int pageSize = 50);
    
    // Conversation management
    Task<ApiResponse<IEnumerable<ConversationDto>>> GetUserConversationsAsync(int userId);
    Task<ApiResponse> DeleteMessageAsync(int messageId, int userId);
    
    // Future: Search functionality
    Task<ApiResponse<IEnumerable<MessageDto>>> SearchMessagesAsync(string query, int userId);
}
