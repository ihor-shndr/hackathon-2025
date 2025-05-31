using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Entities;
using MyChat.Models.DTOs.Auth;
using MyChat.Models.DTOs.Common;
using MyChat.Models.DTOs.Groups;

namespace MyChat.Services;

public class GroupService : IGroupService
{
    private readonly ChatDbContext _context;

    public GroupService(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<GroupDto>> CreateGroupAsync(CreateGroupDto request, int userId)
    {
        try
        {
            // Create the group
            var group = new Group
            {
                Name = request.Name,
                Description = request.Description,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Add owner as first member
            var ownerMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };
            _context.GroupMembers.Add(ownerMember);

            // Add initial members if provided
            if (request.InitialMemberIds?.Any() == true)
            {
                // Validate that initial members are contacts of the owner
                var validContactIds = await _context.Contacts
                    .Where(c => c.UserId == userId && 
                               c.Status == ContactStatus.Accepted &&
                               request.InitialMemberIds.Contains(c.ContactUserId))
                    .Select(c => c.ContactUserId)
                    .ToListAsync();

                foreach (var memberId in validContactIds)
                {
                    var member = new GroupMember
                    {
                        GroupId = group.Id,
                        UserId = memberId,
                        JoinedAt = DateTime.UtcNow
                    };
                    _context.GroupMembers.Add(member);
                }
            }

            await _context.SaveChangesAsync();

            // Return the created group
            var groupDto = await GetGroupDtoAsync(group.Id, userId);
            return ApiResponse<GroupDto>.SuccessResponse(groupDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<GroupDto>.ErrorResponse($"Failed to create group: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateGroupAsync(int groupId, UpdateGroupDto request, int userId)
    {
        try
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

            if (group == null)
            {
                return ApiResponse.ErrorResult("Group not found");
            }

            if (group.OwnerId != userId)
            {
                return ApiResponse.ErrorResult("Only group owner can update group details");
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                group.Name = request.Name;
            }

            if (request.Description != null)
            {
                group.Description = request.Description;
            }

            await _context.SaveChangesAsync();
            return ApiResponse.SuccessResult("Group updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to update group: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteGroupAsync(int groupId, int userId)
    {
        try
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

            if (group == null)
            {
                return ApiResponse.ErrorResult("Group not found");
            }

            if (group.OwnerId != userId)
            {
                return ApiResponse.ErrorResult("Only group owner can delete the group");
            }

            // Soft delete the group
            group.IsActive = false;

            // Deactivate all memberships
            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.IsActive)
                .ToListAsync();

            foreach (var member in members)
            {
                member.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return ApiResponse.SuccessResult("Group deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to delete group: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<GroupDto>>> GetUserGroupsAsync(int userId)
    {
        try
        {
            var userGroupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == userId && gm.IsActive)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var groups = new List<GroupDto>();

            foreach (var groupId in userGroupIds)
            {
                var groupDto = await GetGroupDtoAsync(groupId, userId);
                if (groupDto != null)
                {
                    groups.Add(groupDto);
                }
            }

            return ApiResponse<IEnumerable<GroupDto>>.SuccessResponse(groups.OrderByDescending(g => g.CreatedAt));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<GroupDto>>.ErrorResponse($"Failed to get user groups: {ex.Message}");
        }
    }

    public async Task<ApiResponse<GroupDto>> GetGroupAsync(int groupId, int userId)
    {
        try
        {
            // Check if user is a member of the group
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

            if (!isMember)
            {
                return ApiResponse<GroupDto>.ErrorResponse("You are not a member of this group");
            }

            var groupDto = await GetGroupDtoAsync(groupId, userId);
            if (groupDto == null)
            {
                return ApiResponse<GroupDto>.ErrorResponse("Group not found");
            }

            return ApiResponse<GroupDto>.SuccessResponse(groupDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<GroupDto>.ErrorResponse($"Failed to get group: {ex.Message}");
        }
    }

    public async Task<ApiResponse> AddMemberAsync(int groupId, int memberUserId, int requestUserId)
    {
        try
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

            if (group == null)
            {
                return ApiResponse.ErrorResult("Group not found");
            }

            if (group.OwnerId != requestUserId)
            {
                return ApiResponse.ErrorResult("Only group owner can add members");
            }

            // Check if users are contacts
            var areContacts = await _context.Contacts
                .AnyAsync(c => c.UserId == requestUserId && 
                              c.ContactUserId == memberUserId && 
                              c.Status == ContactStatus.Accepted);

            if (!areContacts)
            {
                return ApiResponse.ErrorResult("Can only add contacts to group");
            }

            // Check if user is already a member
            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == memberUserId);

            if (existingMember != null)
            {
                if (existingMember.IsActive)
                {
                    return ApiResponse.ErrorResult("User is already a group member");
                }
                else
                {
                    // Reactivate membership
                    existingMember.IsActive = true;
                    existingMember.JoinedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Add new member
                var newMember = new GroupMember
                {
                    GroupId = groupId,
                    UserId = memberUserId,
                    JoinedAt = DateTime.UtcNow
                };
                _context.GroupMembers.Add(newMember);
            }

            await _context.SaveChangesAsync();
            return ApiResponse.SuccessResult("Member added to group successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to add member: {ex.Message}");
        }
    }

    public async Task<ApiResponse> RemoveMemberAsync(int groupId, int memberUserId, int requestUserId)
    {
        try
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

            if (group == null)
            {
                return ApiResponse.ErrorResult("Group not found");
            }

            if (group.OwnerId != requestUserId)
            {
                return ApiResponse.ErrorResult("Only group owner can remove members");
            }

            if (memberUserId == requestUserId)
            {
                return ApiResponse.ErrorResult("Owner cannot remove themselves. Delete the group instead");
            }

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == memberUserId && gm.IsActive);

            if (member == null)
            {
                return ApiResponse.ErrorResult("User is not a member of this group");
            }

            member.IsActive = false;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Member removed from group successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to remove member: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<GroupMemberDto>>> GetGroupMembersAsync(int groupId, int userId)
    {
        try
        {
            // Check if user is a member of the group
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

            if (!isMember)
            {
                return ApiResponse<IEnumerable<GroupMemberDto>>.ErrorResponse("You are not a member of this group");
            }

            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

            if (group == null)
            {
                return ApiResponse<IEnumerable<GroupMemberDto>>.ErrorResponse("Group not found");
            }

            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.IsActive)
                .Include(gm => gm.User)
                .Select(gm => new GroupMemberDto
                {
                    UserId = gm.UserId,
                    Username = gm.User.Username,
                    JoinedAt = gm.JoinedAt,
                    IsOwner = gm.UserId == group.OwnerId
                })
                .OrderBy(gm => gm.JoinedAt)
                .ToListAsync();

            return ApiResponse<IEnumerable<GroupMemberDto>>.SuccessResponse(members);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<GroupMemberDto>>.ErrorResponse($"Failed to get group members: {ex.Message}");
        }
    }

    public async Task<ApiResponse> LeaveGroupAsync(int groupId, int userId)
    {
        try
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

            if (group == null)
            {
                return ApiResponse.ErrorResult("Group not found");
            }

            if (group.OwnerId == userId)
            {
                return ApiResponse.ErrorResult("Group owner cannot leave. Delete the group instead");
            }

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

            if (member == null)
            {
                return ApiResponse.ErrorResult("You are not a member of this group");
            }

            member.IsActive = false;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResult("Left group successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.ErrorResult($"Failed to leave group: {ex.Message}");
        }
    }

    private async Task<GroupDto?> GetGroupDtoAsync(int groupId, int userId)
    {
        var group = await _context.Groups
            .Include(g => g.Owner)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

        if (group == null) return null;

        var memberCount = await _context.GroupMembers
            .CountAsync(gm => gm.GroupId == groupId && gm.IsActive);

        // Get last message (simplified for now - will be enhanced with MessageService)
        // TODO: Implement last message retrieval when MessageService is ready

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Owner = new UserDto
            {
                Id = group.Owner.Id,
                Username = group.Owner.Username
            },
            CreatedAt = group.CreatedAt,
            MemberCount = memberCount,
            LastMessage = null // TODO: Implement when MessageService is ready
        };
    }
}
