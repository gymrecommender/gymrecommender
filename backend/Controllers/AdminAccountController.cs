using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
    
    
    [HttpPut("ownership-requests/{requestId:guid}")]
    public async Task<IActionResult> UpdateOwnershipRequest(Guid requestId, [FromBody] UpdateOwnershipRequestDto updateDto)
    {
        // Validate the incoming request
        if (updateDto == null || updateDto.Decision == null)
        {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["InvalidRequest"]
                }
            });
        }

        // Find the ownership request by ID
        var ownershipRequest = await _context.Ownerships.FindAsync(requestId);
        if (ownershipRequest == null)
        {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["OwnershipError"]
                }
            });
        }
        
        ownershipRequest.Decision = updateDto.Decision.ToLower() switch
        {
            "approved" => OwnershipDecision.approved,
            "rejected" => OwnershipDecision.rejected,
            _ => throw new ArgumentException("Invalid decision value. Must be 'approved' or 'rejected'.")
        };

        ownershipRequest.RespondedAt = DateTime.UtcNow;
        ownershipRequest.Message = updateDto.Message;

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Return a success response
        return Ok(new
        {
            success = true,
            message = $"Ownership request {updateDto.Decision.ToLower()} successfully.",
            requestId = ownershipRequest.Id,
            decision = updateDto.Decision.ToLower(),
            respondedAt = ownershipRequest.RespondedAt.ToString(),
        });
    }
    [HttpDelete("ownership-requests/{requestId:guid}")]
    public async Task<IActionResult> DeleteOwnershipRequest(Guid requestId)
    {
        // Find the ownership request by ID
        var ownershipRequest = await _context.Ownerships.FindAsync(requestId);
        if (ownershipRequest == null)
        {
            // return NotFound(new
            // {
            //     success = false,
            //     message = $"Ownership request with ID {requestId} not found."
            // });
            
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["RequestError"]
                }
            });
        }

        // Remove the ownership request
        _context.Ownerships.Remove(ownershipRequest);
        await _context.SaveChangesAsync();

        // Return a success response
        return Ok(new
        {
            success = true,
            message = "Ownership request deleted successfully.",
            requestId = requestId
        });
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}//class bracket