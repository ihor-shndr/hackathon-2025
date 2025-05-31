using MyChat.Models.DTOs.Contacts;

namespace MyChat.Services;

public interface IContactService
{
    Task<ContactDto> SendInvitationAsync(int userId, string targetUsername, string? message = null);
    Task<ContactDto> AcceptInvitationAsync(int userId, int invitationId);
    Task<bool> RejectInvitationAsync(int userId, int invitationId);
    Task<List<ContactDto>> GetContactsAsync(int userId);
    Task<List<ContactInvitationDto>> GetPendingInvitationsAsync(int userId);
    Task<List<ContactInvitationDto>> GetSentInvitationsAsync(int userId);
    Task<bool> RemoveContactAsync(int userId, int contactId);
    Task<bool> BlockContactAsync(int userId, int contactId);
    Task<List<UserSearchDto>> SearchUsersAsync(int userId, string query);
    Task<bool> AreContactsAsync(int userId1, int userId2);
}
