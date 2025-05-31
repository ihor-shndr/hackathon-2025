using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models.DTOs.Common;
using MyChat.Models.DTOs.Contacts;
using MyChat.Services;
using System.Security.Claims;

namespace MyChat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactsController(IContactService contactService, ILogger<ContactsController> logger) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    [HttpPost("invite")]
    public async Task<ActionResult<ApiResponse<ContactDto>>> SendInvitation(SendInviteDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<ContactDto>.ErrorResponse("Validation failed", errors));
            }

            var userId = GetCurrentUserId();
            var result = await contactService.SendInvitationAsync(userId, request.Username, request.Message);
            
            return Ok(ApiResponse<ContactDto>.SuccessResponse(result, "Invitation sent successfully"));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid invitation request from user {UserId}", GetCurrentUserId());
            return BadRequest(ApiResponse<ContactDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid invitation operation from user {UserId}", GetCurrentUserId());
            return Conflict(ApiResponse<ContactDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error sending invitation from user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<ContactDto>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpPut("invitation/{invitationId}/respond")]
    public async Task<ActionResult<ApiResponse<ContactDto>>> RespondToInvitation(int invitationId, RespondToInviteDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<ContactDto>.ErrorResponse("Validation failed", errors));
            }

            var userId = GetCurrentUserId();
            
            if (request.Accept)
            {
                var result = await contactService.AcceptInvitationAsync(userId, invitationId);
                return Ok(ApiResponse<ContactDto>.SuccessResponse(result, "Invitation accepted successfully"));
            }
            else
            {
                var success = await contactService.RejectInvitationAsync(userId, invitationId);
                if (!success)
                {
                    return NotFound(ApiResponse<ContactDto>.ErrorResponse("Invitation not found"));
                }
                return Ok(ApiResponse<ContactDto>.SuccessResponse(null!, "Invitation rejected successfully"));
            }
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid invitation response from user {UserId} for invitation {InvitationId}", 
                GetCurrentUserId(), invitationId);
            return BadRequest(ApiResponse<ContactDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error responding to invitation {InvitationId} from user {UserId}", 
                invitationId, GetCurrentUserId());
            return StatusCode(500, ApiResponse<ContactDto>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ContactDto>>>> GetContacts()
    {
        try
        {
            var userId = GetCurrentUserId();
            var contacts = await contactService.GetContactsAsync(userId);
            
            return Ok(ApiResponse<List<ContactDto>>.SuccessResponse(contacts, "Contacts retrieved successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving contacts for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<List<ContactDto>>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpGet("invitations")]
    public async Task<ActionResult<ApiResponse<List<ContactInvitationDto>>>> GetPendingInvitations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var invitations = await contactService.GetPendingInvitationsAsync(userId);
            
            return Ok(ApiResponse<List<ContactInvitationDto>>.SuccessResponse(invitations, "Pending invitations retrieved successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving pending invitations for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<List<ContactInvitationDto>>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpGet("invitations/sent")]
    public async Task<ActionResult<ApiResponse<List<ContactInvitationDto>>>> GetSentInvitations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var invitations = await contactService.GetSentInvitationsAsync(userId);
            
            return Ok(ApiResponse<List<ContactInvitationDto>>.SuccessResponse(invitations, "Sent invitations retrieved successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving sent invitations for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<List<ContactInvitationDto>>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpDelete("{contactId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveContact(int contactId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await contactService.RemoveContactAsync(userId, contactId);
            
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Contact not found"));
            }
            
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Contact removed successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error removing contact {ContactId} for user {UserId}", 
                contactId, GetCurrentUserId());
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpPut("{contactId}/block")]
    public async Task<ActionResult<ApiResponse<bool>>> BlockContact(int contactId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await contactService.BlockContactAsync(userId, contactId);
            
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Contact not found"));
            }
            
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Contact blocked successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error blocking contact {ContactId} for user {UserId}", 
                contactId, GetCurrentUserId());
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<UserSearchDto>>>> SearchUsers([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(ApiResponse<List<UserSearchDto>>.ErrorResponse("Search query is required"));
            }

            var userId = GetCurrentUserId();
            var results = await contactService.SearchUsersAsync(userId, query);
            
            return Ok(ApiResponse<List<UserSearchDto>>.SuccessResponse(results, "Search completed successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error searching users for user {UserId} with query '{Query}'", 
                GetCurrentUserId(), query);
            return StatusCode(500, ApiResponse<List<UserSearchDto>>.ErrorResponse("An unexpected error occurred"));
        }
    }
}
