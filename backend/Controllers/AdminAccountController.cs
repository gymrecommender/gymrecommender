using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AdminAccountController : AccountControllerTemplate {
    public AdminAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) :
        base(context, appSettings) {
        _accountType = AccountType.admin;
    }
    
    [HttpGet("{username}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAdminByUsername(string username) {
        return await base.GetByUsername(username, _accountType);
    }
    
    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateAccount(AccountPutDto accountPutDto) {
        return await base.UpdateAccount(accountPutDto, _accountType);
    }
    
    [HttpDelete]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteAccount() {
        return await base.DeleteAccount(_accountType);
    }
    
    [HttpPost("login")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Login() {
        return await base.Login(_accountType);
    }
    
    [HttpDelete("logout")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Logout() {
        return await base.Logout(_accountType);
    }
    
    [HttpPut("/requests/{requestId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOwnershipRequest(Guid requestId, [FromBody] UpdateOwnershipRequestDto updateDto) {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        if (updateDto == null)
        {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["InvalidRequest"]
                }
            });
        }
        
        var admin = _context.Accounts.FirstOrDefault(a => a.OuterUid == firebaseUid && a.Type == AccountType.admin);
        if (admin == null) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["UsernameError"]
                }
            });
        }
        
        var ownershipRequest = _context.Ownerships.AsTracking()
                                       .Include(o => o.Gym)
                                       .FirstOrDefault(o => o.Id == requestId);
        
        if (ownershipRequest == null)
        {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["OwnershipError"]
                }
            });
        }

        if (updateDto.Decision != null) {
            ownershipRequest.Decision = updateDto.Decision.ToLower() switch {
                "approved" => OwnershipDecision.approved,
                "rejected" => OwnershipDecision.rejected,
                _ => throw new ArgumentException("Invalid decision value. Must be 'approved' or 'rejected'.")
            };
            ownershipRequest.RespondedAt = DateTime.UtcNow;
            ownershipRequest.RespondedBy = admin.Id;
            
            if (ownershipRequest.Decision == OwnershipDecision.approved) {
                var gym = _context.Gyms.AsTracking().First(g => g.Id == ownershipRequest.GymId);
                gym.OwnedBy = ownershipRequest.RequestedBy;
            }
        }
        if (ownershipRequest.Message != null) ownershipRequest.Message = updateDto.Message;
        await _context.SaveChangesAsync();
        
        return Ok(new {
            id = ownershipRequest.Id,
            requestedAt = ownershipRequest.RequestedAt,
            respondedAt = ownershipRequest.RespondedAt,
            decision = ownershipRequest.Decision?.ToString(),
            message = ownershipRequest.Message,
            gym = new {
                id = ownershipRequest.Gym.Id,
                name = ownershipRequest.Gym.Name,
                address = ownershipRequest.Gym.Address,
                latitude = ownershipRequest.Gym.Latitude,
                longitude = ownershipRequest.Gym.Longitude
            }
        });
    }
    
    
    [HttpGet("requests")]
    public async Task<IActionResult> GetOwnershipRequests() {
        //TODO with the get parameter for the function above this function is not needed
        // im npot sure what you mean by this, i fixed the above function but still 
        var ownershipRequests = await _context.Ownerships
            .Include(o => o.Gym)
            .ToListAsync();

        var response = ownershipRequests.Select(o => new {
            id = o.Id,
            requestedAt = o.RequestedAt,
            respondedAt = o.RespondedAt,
            decision = o.Decision?.ToString(),
            message = o.Message,
            gym = new {
                id = o.Gym.Id,
                name = o.Gym.Name,
                address = o.Gym.Address,
                latitude = o.Gym.Latitude,
                longitude = o.Gym.Longitude
            }
        }).ToList();

        // Return the result as JSON
        return Ok(response);
    }
}