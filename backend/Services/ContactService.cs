using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Entities;
using MyChat.Models.DTOs.Auth;
using MyChat.Models.DTOs.Contacts;

namespace MyChat.Services;

public class ContactService(ChatDbContext context, ILogger<ContactService> logger) : IContactService
{
    public async Task<ContactDto> SendInvitationAsync(int userId, string targetUsername, string? message = null)
    {
        // Find target user
        var targetUser = await context.Users
            .FirstOrDefaultAsync(u => u.Username == targetUsername);
        
        if (targetUser == null)
        {
            throw new ArgumentException("User not found", nameof(targetUsername));
        }
        
        if (targetUser.Id == userId)
        {
            throw new InvalidOperationException("Cannot send invitation to yourself");
        }
        
        // Check if relationship already exists
        var existingContact = await context.Contacts
            .FirstOrDefaultAsync(c => 
                (c.UserId == userId && c.ContactUserId == targetUser.Id) ||
                (c.UserId == targetUser.Id && c.ContactUserId == userId));
        
        if (existingContact != null)
        {
            throw new InvalidOperationException("Contact relationship already exists");
        }
        
        // Create invitation
        var contact = new Contact
        {
            UserId = userId,
            ContactUserId = targetUser.Id,
            Status = ContactStatus.Pending,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Contact invitation sent from user {UserId} to user {TargetUserId}", 
            userId, targetUser.Id);
        
        return new ContactDto
        {
            Id = contact.Id,
            User = new UserDto
            {
                Id = targetUser.Id,
                Username = targetUser.Username,
                CreatedAt = targetUser.CreatedAt
            },
            Status = contact.Status,
            CreatedAt = contact.CreatedAt,
            Message = contact.Message
        };
    }
    
    public async Task<ContactDto> AcceptInvitationAsync(int userId, int invitationId)
    {
        var invitation = await context.Contacts
            .Include(c => c.User)
            .Include(c => c.ContactUser)
            .FirstOrDefaultAsync(c => c.Id == invitationId && c.ContactUserId == userId && c.Status == ContactStatus.Pending);
        
        if (invitation == null)
        {
            throw new ArgumentException("Invitation not found or already processed", nameof(invitationId));
        }
        
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Update original invitation
            invitation.Status = ContactStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            
            // Create reciprocal contact
            var reciprocalContact = new Contact
            {
                UserId = userId,
                ContactUserId = invitation.UserId,
                Status = ContactStatus.Accepted,
                CreatedAt = invitation.CreatedAt,
                AcceptedAt = DateTime.UtcNow
            };
            
            context.Contacts.Add(reciprocalContact);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            logger.LogInformation("Contact invitation {InvitationId} accepted by user {UserId}", 
                invitationId, userId);
            
            return new ContactDto
            {
                Id = invitation.Id,
                User = new UserDto
                {
                    Id = invitation.User.Id,
                    Username = invitation.User.Username,
                    CreatedAt = invitation.User.CreatedAt
                },
                Status = invitation.Status,
                CreatedAt = invitation.CreatedAt,
                AcceptedAt = invitation.AcceptedAt,
                Message = invitation.Message
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<bool> RejectInvitationAsync(int userId, int invitationId)
    {
        var invitation = await context.Contacts
            .FirstOrDefaultAsync(c => c.Id == invitationId && c.ContactUserId == userId && c.Status == ContactStatus.Pending);
        
        if (invitation == null)
        {
            return false;
        }
        
        context.Contacts.Remove(invitation);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Contact invitation {InvitationId} rejected by user {UserId}", 
            invitationId, userId);
        
        return true;
    }
    
    public async Task<List<ContactDto>> GetContactsAsync(int userId)
    {
        var contacts = await context.Contacts
            .Where(c => c.UserId == userId && c.Status == ContactStatus.Accepted)
            .Include(c => c.ContactUser)
            .OrderBy(c => c.ContactUser.Username)
            .ToListAsync();
        
        return contacts.Select(c => new ContactDto
        {
            Id = c.Id,
            User = new UserDto
            {
                Id = c.ContactUser.Id,
                Username = c.ContactUser.Username,
                CreatedAt = c.ContactUser.CreatedAt
            },
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            AcceptedAt = c.AcceptedAt,
            Message = c.Message
        }).ToList();
    }
    
    public async Task<List<ContactInvitationDto>> GetPendingInvitationsAsync(int userId)
    {
        var invitations = await context.Contacts
            .Where(c => c.ContactUserId == userId && c.Status == ContactStatus.Pending)
            .Include(c => c.User)
            .Include(c => c.ContactUser)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        
        return invitations.Select(c => new ContactInvitationDto
        {
            Id = c.Id,
            FromUser = new UserDto
            {
                Id = c.User.Id,
                Username = c.User.Username,
                CreatedAt = c.User.CreatedAt
            },
            ToUser = new UserDto
            {
                Id = c.ContactUser.Id,
                Username = c.ContactUser.Username,
                CreatedAt = c.ContactUser.CreatedAt
            },
            Message = c.Message,
            SentAt = c.CreatedAt
        }).ToList();
    }
    
    public async Task<List<ContactInvitationDto>> GetSentInvitationsAsync(int userId)
    {
        var invitations = await context.Contacts
            .Where(c => c.UserId == userId && c.Status == ContactStatus.Pending)
            .Include(c => c.User)
            .Include(c => c.ContactUser)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        
        return invitations.Select(c => new ContactInvitationDto
        {
            Id = c.Id,
            FromUser = new UserDto
            {
                Id = c.User.Id,
                Username = c.User.Username,
                CreatedAt = c.User.CreatedAt
            },
            ToUser = new UserDto
            {
                Id = c.ContactUser.Id,
                Username = c.ContactUser.Username,
                CreatedAt = c.ContactUser.CreatedAt
            },
            Message = c.Message,
            SentAt = c.CreatedAt
        }).ToList();
    }
    
    public async Task<bool> RemoveContactAsync(int userId, int contactId)
    {
        var contact = await context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        
        if (contact == null)
        {
            return false;
        }
        
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Remove both directions of the relationship
            var reciprocalContact = await context.Contacts
                .FirstOrDefaultAsync(c => c.UserId == contact.ContactUserId && c.ContactUserId == userId);
            
            context.Contacts.Remove(contact);
            if (reciprocalContact != null)
            {
                context.Contacts.Remove(reciprocalContact);
            }
            
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            logger.LogInformation("Contact {ContactId} removed by user {UserId}", contactId, userId);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<bool> BlockContactAsync(int userId, int contactId)
    {
        var contact = await context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        
        if (contact == null)
        {
            return false;
        }
        
        contact.Status = ContactStatus.Blocked;
        await context.SaveChangesAsync();
        
        logger.LogInformation("Contact {ContactId} blocked by user {UserId}", contactId, userId);
        return true;
    }
    
    public async Task<List<UserSearchDto>> SearchUsersAsync(int userId, string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return new List<UserSearchDto>();
        }
        
        var users = await context.Users
            .Where(u => u.Id != userId && u.Username.Contains(query))
            .Take(20)
            .ToListAsync();
        
        var result = new List<UserSearchDto>();
        
        foreach (var user in users)
        {
            var contactRelationship = await context.Contacts
                .Where(c => 
                    (c.UserId == userId && c.ContactUserId == user.Id) ||
                    (c.UserId == user.Id && c.ContactUserId == userId))
                .ToListAsync();
            
            var isContact = contactRelationship.Any(c => c.Status == ContactStatus.Accepted);
            var hasPendingInvitation = contactRelationship.Any(c => 
                c.ContactUserId == userId && c.Status == ContactStatus.Pending);
            var isSentInvitation = contactRelationship.Any(c => 
                c.UserId == userId && c.Status == ContactStatus.Pending);
            
            result.Add(new UserSearchDto
            {
                Id = user.Id,
                Username = user.Username,
                IsContact = isContact,
                HasPendingInvitation = hasPendingInvitation,
                IsSentInvitation = isSentInvitation
            });
        }
        
        return result;
    }
    
    public async Task<bool> AreContactsAsync(int userId1, int userId2)
    {
        return await context.Contacts
            .AnyAsync(c => 
                c.UserId == userId1 && 
                c.ContactUserId == userId2 && 
                c.Status == ContactStatus.Accepted);
    }
}
