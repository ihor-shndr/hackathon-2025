using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models.DTOs.Messages;
using MyChat.Services;
using System.Security.Claims;

namespace MyChat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost("direct")]
    public async Task<IActionResult> SendDirectMessage([FromBody] SendDirectMessageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await messageService.SendDirectMessageAsync(request, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("group")]
    public async Task<IActionResult> SendGroupMessage([FromBody] SendGroupMessageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await messageService.SendGroupMessageAsync(request, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("direct/{contactId}")]
    public async Task<IActionResult> GetDirectMessages(int contactId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        var result = await messageService.GetDirectMessagesAsync(userId, contactId, page, pageSize);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetGroupMessages(int groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        var result = await messageService.GetGroupMessagesAsync(groupId, userId, page, pageSize);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetCurrentUserId();
        var result = await messageService.GetUserConversationsAsync(userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var userId = GetCurrentUserId();
        var result = await messageService.DeleteMessageAsync(messageId, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMessages([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter is required");
        }

        var userId = GetCurrentUserId();
        var result = await messageService.SearchMessagesAsync(query, userId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}