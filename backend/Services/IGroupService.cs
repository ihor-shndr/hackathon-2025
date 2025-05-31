using MyChat.Models.DTOs.Common;
using MyChat.Models.DTOs.Groups;

namespace MyChat.Services;

public interface IGroupService
{
    // Group management
    Task<ApiResponse<GroupDto>> CreateGroupAsync(CreateGroupDto request, int userId);
    Task<ApiResponse> UpdateGroupAsync(int groupId, UpdateGroupDto request, int userId);
    Task<ApiResponse> DeleteGroupAsync(int groupId, int userId);
    Task<ApiResponse<IEnumerable<GroupDto>>> GetUserGroupsAsync(int userId);
    Task<ApiResponse<GroupDto>> GetGroupAsync(int groupId, int userId);
    
    // Member management (owner-only operations)
    Task<ApiResponse> AddMemberAsync(int groupId, int memberUserId, int requestUserId);
    Task<ApiResponse> RemoveMemberAsync(int groupId, int memberUserId, int requestUserId);
    Task<ApiResponse<IEnumerable<GroupMemberDto>>> GetGroupMembersAsync(int groupId, int userId);
    
    // Member self-service
    Task<ApiResponse> LeaveGroupAsync(int groupId, int userId);
}
