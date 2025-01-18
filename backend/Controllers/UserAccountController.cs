using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Services;
using backend.Utilities;
using backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserAccountController : AccountControllerTemplate {
    private readonly RecommendationService _recommendationService;

    public UserAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings,
                                 RecommendationService recommendationService) :
        base(context, appSettings) {
        _accountType = AccountType.user;
        _recommendationService = recommendationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SignUp(AccountDto accountDto, AccountType type, Guid? createdBy = null) {
        if (ModelState.IsValid) {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
            if (firebaseUid == null) {
                return Forbid("Unauthorized user");
            }

            try {
                var errors = new Dictionary<string, string[]> { };
                if (!Enum.TryParse<ProviderType>(accountDto.Provider, out var provider)) {
                    errors["Provider"] = new[] { $"Provider {accountDto.Provider} is not supported" };
                } //not sure if this can be changed to a smarter way

                if (errors.Count > 0) {
                    return BadRequest(new {
                        success = false,
                        error = new {
                            code = "ValidationError",
                            message = "Some fields contain invalid data",
                            details = errors
                        }
                    });
                }

                var account = new Account {
                    Username = accountDto.Username,
                    Email = accountDto.Email,
                    FirstName = accountDto.FirstName,
                    LastName = accountDto.LastName,
                    Type = type,
                    Provider = provider,
                    OuterUid = accountDto.OuterUid,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountDto.Password),
                    IsEmailVerified = accountDto.IsEmailVerified,
                    CreatedBy = createdBy
                };
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                var role = account.Type.ToString();
                var response = new AuthResponse() {
                    Username = account.Username,
                    Email = account.Email,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Role = role,
                };

                return Ok(response);
            } catch (Exception e) {
                return StatusCode(500, new {
                    success = false,
                    error = new {
                        code = "SignupError",
                        message = ErrorMessage.ErrorMessages["SignUpError"]
                    }
                });
            }
        }

        return BadRequest(new {
            success = false,
            error = new {
                code = "ValidationError",
                message = ErrorMessage.ErrorMessages["ValidationError"],
            }
        });
    }

    [HttpGet("requests")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetRequests() {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        var request = _context.Requests
                              .Include(r => r.User)
                              .Where(r => r.User.OuterUid == firebaseUid)
                              .Select(r => new {
                                  Id = r.Id,
                                  RequestedAt = r.RequestedAt,
                                  Name = r.Name,
                                  Preferences = new {
                                      MinPrice = r.MinMembershipPrice,
                                      MinRating = r.MinRating,
                                      MinCongestion = r.MinCongestionRating,
                                      PriceTimeRatio = r.TotalCostPriority,
                                      MembershipLength = r.MembType.ToString(),
                                      DepartureTime = r.DepartureTime,
                                      ArrivalTime = r.ArrivalTime,
                                  }
                              })
                              .OrderByDescending(r => r.RequestedAt)
                              .ToList();

        return Ok(request);
    }
    
    [HttpPut]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> UpdateAccount(AccountPutDto accountPutDto) {
        return await base.UpdateAccount(accountPutDto, _accountType);
    }

    [HttpDelete]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> DeleteAccount() {
        return await base.DeleteAccount(_accountType);
    }

    [HttpPost("login")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> Login() {
        return await base.Login(_accountType);
    }

    [HttpDelete("logout")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> Logout() {
        return await base.Logout(_accountType);
    }

    /// <summary>
    /// Retrieves all requests associated with a specific user.
    /// Accessible only by accounts with the User role.
    /// </summary>
    /// <returns>A list of RequestDto objects.</returns>
    [HttpGet("history")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetRequestsHistory() {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        try {
            // Retrieve requests using the recommendation service
            var requests = await _recommendationService.GetRequestsByUsernameAsync(firebaseUid);

            return Ok(requests);
        } catch (KeyNotFoundException knfEx) {
            return NotFound(new { message = knfEx.Message });
        }
    }
    [HttpGet("{username}/requests")]
public async Task<IActionResult> GetRequests(string username)
{
    // Ensure the user exists
    var user = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == username && a.Type == AccountType.user);
    if (user == null)
    {
        return NotFound(new { message = "User not found or not of type 'user'." });
    }

    // Retrieve requests for the user
    var requests = await _context.Requests
        .Where(r => r.UserId == user.Id)
        .Select(r => new
        {
            r.Id,
            r.RequestedAt,
            r.OriginLatitude,
            r.OriginLongitude,
            r.TimePriority,
            r.TotalCostPriority,
            r.MinCongestionRating,
            r.MinRating,
            r.MinMembershipPrice,
            r.Name
        })
        .ToListAsync();

    // Return the list of requests
    return Ok(requests);
}
    [HttpPut("{username}/requests/{requestId}")]
public async Task<IActionResult> UpdateRequest(string username, Guid requestId, UpdateRequestDto updateRequestDto) {
    // Fetch the user and validate the username and account type
    var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Username == username && u.Type == AccountType.user);
    if (user == null) {
        return NotFound("User not found or account type is not 'user'.");
    }

    // Fetch the request by ID and ensure it belongs to the user
    var request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == user.Id);
    if (request == null) {
        return NotFound("Request not found or does not belong to the user.");
    }

    // Validate the Name field in the DTO
    if (string.IsNullOrWhiteSpace(updateRequestDto.Name)) {
        return BadRequest("The 'Name' field cannot be null or empty.");
    }

    // Update only the Name field of the request
    request.Name = updateRequestDto.Name;

    // Save changes to the database
    try {
        await _context.SaveChangesAsync();
    } catch (DbUpdateException) {
        return StatusCode(500, "An error occurred while updating the request.");
    }

        return Ok(new 
    {
        message = "Request updated successfully.",
        updatedName = request.Name
    });
}
}