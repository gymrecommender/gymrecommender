using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AdminAccountController : AccountControllerTemplate {
    
    private readonly GymrecommenderContext _context;
    public AdminAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) :
        base(context, appSettings) {
        _accountType = AccountType.admin;
        _context = context;
    }

    [HttpGet]// GET /api/adminaccount
    public async Task<IActionResult> GetAdminData(int page = 1, int sort = 1, bool ascending = true) {
        return await base.GetData(page, sort, ascending, _accountType);
    }
    
    [HttpPost]// POST /api/adminaccount
    public async Task<IActionResult> SignUpAdmin(AccountDto accountDto) {
        return await SignUp(accountDto, _accountType);
    }
    
    [HttpGet("{username}")] // GET /api/adminaccount/{username}
    public async Task<IActionResult> GetAdminByUsername(string username) {
        return await base.GetByUsername(username, _accountType);
    }
    
    [HttpPut("{username}")] // PUT /api/adminaccount/{username}
    public async Task<IActionResult> UpdateByUsername(string username, AccountPutDto accountPutDto) {
        return await base.UpdateByUsername(username, accountPutDto, _accountType);
    }
    
    [HttpDelete("{username}")] // DELETE /api/adminaccount/{username}
    public async Task<IActionResult> DeleteByUsername(string username) {
        return await base.DeleteByUsername(username, _accountType);
    }
    
    [HttpPost("{username}/login")]
    public async Task<IActionResult> Login(string username) {
        return await base.Login(username, _accountType);
    }
    
    [HttpDelete("{username}/logout")] // DELETE /api/adminaccount/{username}/logout
    public async Task<IActionResult> Logout(string username) {
        return await base.Logout(username, _accountType);
    }
    
    [HttpPut("{username}/requests/{requestId}")]
    public async Task<IActionResult> UpdateOwnershipRequest(string username, Guid requestId, [FromBody] UpdateOwnershipRequestDto updateDto)
    {
        if (updateDto == null)
        {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["InvalidRequest"]
                }
            });
        }
        
        var admin = _context.Accounts.FirstOrDefault(a => a.Username == username && a.Type == AccountType.admin);
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
        }
        ownershipRequest.Message = updateDto.Message;
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