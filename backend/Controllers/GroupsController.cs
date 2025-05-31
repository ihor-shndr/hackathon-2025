using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models.DTOs.Groups;
using MyChat.Services;
using System.Security.Claims;

namespace MyChat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _groupService.CreateGroupAsync(request, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserGroups()
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.GetUserGroupsAsync(userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("{groupId}")]
    public async Task<IActionResult> GetGroup(int groupId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.GetGroupAsync(groupId, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPut("{groupId}")]
    public async Task<IActionResult> UpdateGroup(int groupId, [FromBody] UpdateGroupDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _groupService.UpdateGroupAsync(groupId, request, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.DeleteGroupAsync(groupId, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("{groupId}/members")]
    public async Task<IActionResult> GetGroupMembers(int groupId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.GetGroupMembersAsync(groupId, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("{groupId}/members/{memberUserId}")]
    public async Task<IActionResult> AddMember(int groupId, int memberUserId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.AddMemberAsync(groupId, memberUserId, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpDelete("{groupId}/members/{memberUserId}")]
    public async Task<IActionResult> RemoveMember(int groupId, int memberUserId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.RemoveMemberAsync(groupId, memberUserId, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("{groupId}/leave")]
    public async Task<IActionResult> LeaveGroup(int groupId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.LeaveGroupAsync(groupId, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}