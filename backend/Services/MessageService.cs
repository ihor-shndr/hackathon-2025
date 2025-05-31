using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Entities;
using MyChat.Models.DTOs.Auth;
using MyChat.Models.DTOs.Common;
using MyChat.Models.DTOs.Messages;

namespace MyChat.Services;

public class MessageService : IMessageService
{
    private readonly ChatDbContext _context;
    private readonly IContactService _contactService;

    public MessageService(ChatDbContext context, IContactService contactService)
    {
        _context = context;
        _contactService = contactService;
    }

    public async Task<ApiResponse<MessageDto>> SendDirectMessageAsync(SendDirectMessageDto request, int senderId)
    {
        try
        {
            // Validate that users are contacts
            var areContacts = await _contactService.AreContactsAsync(senderId, request.RecipientId);
            if (!areContacts)
            {
                return ApiResponse<MessageDto>.ErrorResponse("You can only send messages to your contacts");
            }

            var message = new Message
            {
                SenderId = senderId,
                RecipientId = request.RecipientId,
                Content = request.Content,
                Type = request.Type,
                AttachmentUrl = request.AttachmentUrl,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageDto = await GetMessageDtoAsync(message.Id, senderId);
            return ApiResponse<MessageDto>.SuccessResponse(messageDto!);
        }
        catch (Exception ex)
        {
            return ApiResponse<MessageDto>.ErrorResponse($"Failed to send message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MessageDto>> SendGroupMessageAsync(SendGroupMessageDto request, int senderId)
    {
        try
        {
            // Validate that user is a member of the group
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == request.GroupId && gm.UserId == senderId && gm.IsActive);

            if (!isMember)
            {
                return ApiResponse<MessageDto>.ErrorResponse("You are not a member of this group");
            }

            var message = new Message
            {
                SenderId = senderId,
                GroupId = request.GroupId,
                Content = request.Content,
                Type = request.Type,
                AttachmentUrl = request.AttachmentUrl,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageDto = await GetMessageDtoAsync(message.Id, senderId);
            return ApiResponse<MessageDto>.SuccessResponse(messageDto!);
        }
        catch (Exception ex)
        {
            return ApiResponse<MessageDto>.ErrorResponse($"Failed to send group message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> GetDirectMessagesAsync(int userId, int contactId, int page = 1, int pageSize = 50)
    {
        try
        {
            // Validate that users are contacts
            var areContacts = await _contactService.AreContactsAsync(userId, contactId);
            if (!areContacts)
            {
                return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("You can only view messages with your contacts");
            }

            var messages = await _context.Messages
                .Where(m => !m.IsDeleted && 
                           ((m.SenderId == userId && m.RecipientId == contactId) ||
                            (m.SenderId == contactId && m.RecipientId == userId)))
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var messageDtos = messages.Select(m => MapToMessageDto(m, userId)).ToList();
            return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(messageDtos.AsEnumerable().Reverse());
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse($"Failed to get messages: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> GetGroupMessagesAsync(int groupId, int userId, int page = 1, int pageSize = 50)
    {
        try
        {
            // Validate that user is a member of the group
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

            if (!isMember)
            {
                return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("You are not a member of this group");
            }

            var messages = await _context.Messages
                .Where(m => m.GroupId == groupId && !m.IsDeleted)
                .Include(m => m.Sender)
                .Include(m => m.Group)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var messageDtos = messages.Select(m => MapToMessageDto(m, userId)).ToList();
            return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(messageDtos.AsEnumerable().Reverse());
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse($"Failed to get group messages: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<ConversationDto>>> GetUserConversationsAsync(int userId)
    {
        try
        {
            var conversations = new List<ConversationDto>();

            // Get direct message conversations
            var directConversations = await GetDirectConversationsAsync(userId);
            conversations.AddRange(directConversations);

            // Get group conversations
            var groupConversations = await GetGroupConversationsAsync(userId);
            conversations.AddRange(groupConversations);

            // Sort by last activity
            var sortedConversations = conversations
                .OrderByDescending(c => c.LastActivity)
                .ToList();

            return ApiResponse<IEnumerable<ConversationDto>>.SuccessResponse(sortedConversations);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ConversationDto>>.ErrorResponse($"Failed to get conversations: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteMessageAsync(int messageId, int userId)
    {
        try
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId && !m.IsDeleted);

            if (message == null)
            {
                return ApiResponse.ErrorResult("Message not found or you don't have permission to delete it");
            }

            message.IsDeleted = true;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Message deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to delete message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> SearchMessagesAsync(string query, int userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(new List<MessageDto>());
            }

            // Get user's group IDs
            var userGroupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == userId && gm.IsActive)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            // Get user's contact IDs
            var contactIds = await _context.Contacts
                .Where(c => c.UserId == userId && c.Status == ContactStatus.Accepted)
                .Select(c => c.ContactUserId)
                .ToListAsync();

            var messages = await _context.Messages
                .Where(m => !m.IsDeleted && 
                           m.Content.Contains(query) &&
                           ((m.GroupId.HasValue && userGroupIds.Contains(m.GroupId.Value)) ||
                            (m.RecipientId.HasValue && 
                             ((m.SenderId == userId && contactIds.Contains(m.RecipientId.Value)) ||
                              (m.RecipientId == userId && contactIds.Contains(m.SenderId))))))
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Include(m => m.Group)
                .OrderByDescending(m => m.SentAt)
                .Take(100)
                .ToListAsync();

            var messageDtos = messages.Select(m => MapToMessageDto(m, userId)).ToList();
            return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(messageDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse($"Failed to search messages: {ex.Message}");
        }
    }

    private async Task<MessageDto?> GetMessageDtoAsync(int messageId, int userId)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        return message != null ? MapToMessageDto(message, userId) : null;
    }

    private MessageDto MapToMessageDto(Message message, int currentUserId)
    {
        return new MessageDto
        {
            Id = message.Id,
            Sender = new UserDto
            {
                Id = message.Sender.Id,
                Username = message.Sender.Username
            },
            Content = message.Content,
            Type = message.Type,
            AttachmentUrl = message.AttachmentUrl,
            SentAt = message.SentAt,
            IsOwn = message.SenderId == currentUserId,
            GroupId = message.GroupId,
            GroupName = message.Group?.Name,
            RecipientId = message.RecipientId,
            RecipientUsername = message.Recipient?.Username
        };
    }

    private async Task<List<ConversationDto>> GetDirectConversationsAsync(int userId)
    {
        var conversations = new List<ConversationDto>();

        // Get contacts
        var contacts = await _contactService.GetContactsAsync(userId);

        foreach (var contact in contacts)
        {
            // Get last message with this contact
            var lastMessage = await _context.Messages
                .Where(m => !m.IsDeleted &&
                           ((m.SenderId == userId && m.RecipientId == contact.User.Id) ||
                            (m.SenderId == contact.User.Id && m.RecipientId == userId)))
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            if (lastMessage != null)
            {
                conversations.Add(new ConversationDto
                {
                    Id = contact.User.Id,
                    Type = ConversationType.Direct,
                    Name = contact.User.Username,
                    ContactId = contact.User.Id,
                    LastMessage = MapToMessageDto(lastMessage, userId),
                    LastActivity = lastMessage.SentAt,
                    UnreadCount = 0 // TODO: Implement unread count
                });
            }
        }

        return conversations;
    }

    private async Task<List<ConversationDto>> GetGroupConversationsAsync(int userId)
    {
        var conversations = new List<ConversationDto>();

        // Get user's groups
        var userGroupIds = await _context.GroupMembers
            .Where(gm => gm.UserId == userId && gm.IsActive)
            .Select(gm => gm.GroupId)
            .ToListAsync();

        foreach (var groupId in userGroupIds)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

            if (group != null)
            {
                // Get last message in this group
                var lastMessage = await _context.Messages
                    .Where(m => m.GroupId == groupId && !m.IsDeleted)
                    .Include(m => m.Sender)
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                // Get member count
                var memberCount = await _context.GroupMembers
                    .CountAsync(gm => gm.GroupId == groupId && gm.IsActive);

                conversations.Add(new ConversationDto
                {
                    Id = group.Id,
                    Type = ConversationType.Group,
                    Name = group.Name,
                    GroupId = group.Id,
                    MemberCount = memberCount,
                    LastMessage = lastMessage != null ? MapToMessageDto(lastMessage, userId) : null,
                    LastActivity = lastMessage?.SentAt ?? group.CreatedAt,
                    UnreadCount = 0 // TODO: Implement unread count
                });
            }
        }

        return conversations;
    }
}
