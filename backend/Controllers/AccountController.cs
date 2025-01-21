using backend.DTO;
using backend.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using backend.Models;
using backend.Utilities;
using backend.ViewModels;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Query;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AccountController : AccountControllerTemplate {
    public AccountController(GymrecommenderContext context, HttpClient httpClient,
                             IOptions<AppSettings> appSettings) : base(context, httpClient, appSettings) { }

    [HttpGet("role")]
    [Authorize]
    public async Task<IActionResult> GetRole() {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var account = await _context.Accounts.AsNoTracking()
                                        .Where(a => a.OuterUid == firebaseUid)
                                        .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new {
                    success = false, error = new {
                        code = "UIDError",
                        message = ErrorMessage.ErrorMessages["TokenError"] //?
                    }
                });
            }

            return Ok(new {
                Role = account.Type.ToString(),
                Username = account.Username
            });
        } catch (Exception e) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = e.Message
                }
            });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUser() {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
            
            var account = _context.Accounts.AsTracking().First(a => a.OuterUid == firebaseUid);
            
            return Ok(new {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                Username = account.Username,
            });
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while retrieving the account's data" }
            });
        }
    }
    
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateUser([FromBody]AccountUpdateDto accountUpdateDto) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
            
            var account = _context.Accounts.AsTracking().First(a => a.OuterUid == firebaseUid);
            
            if (accountUpdateDto.FirstName != null) account.FirstName = accountUpdateDto.FirstName;
            if (accountUpdateDto.LastName != null) account.LastName = accountUpdateDto.LastName;
            
            await _context.SaveChangesAsync();
            
            return Ok(new {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                Username = account.Username
            });
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while updating the account's data"}
            });
        }
    }
    
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteUser([FromBody]AccountPwdDto accountPwdDto) {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
            
            var account = _context.Accounts.AsTracking().First(a => a.OuterUid == firebaseUid);
            if (!BCrypt.Net.BCrypt.Verify(accountPwdDto.Password, account.PasswordHash)) {
                return BadRequest("Incorrect password");
            }

            if (account.Type == AccountType.gym) {
                //Remove all ownerships so that other gym accounts can apply for managing those gyms
                _context.Gyms.AsTracking().Where(g => g.OwnedBy == account.Id).ToList().ForEach(g => g.OwnedBy = null);
                _context.SaveChanges();
            }
            
            var firebaseApiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY")
                                 ?? throw new InvalidOperationException("FIREBASE_API_KEY not set.");
            var firebaseUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:delete?key={firebaseApiKey}";
            var payload = new
            {
                idToken = HttpContext.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "")
            };

            var response = await _httpClient.PostAsJsonAsync(firebaseUrl, payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { message = "Failed to delete user in Firebase", error = errorResponse });
            }
            
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            return StatusCode(204);
        } catch (Exception _) {
            await transaction.RollbackAsync();
            
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while deleting the account"}
            });
        }
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword([FromBody]AccountPwdUpdateDto accountPwdUpdateDto) {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
            
            var account = _context.Accounts.AsTracking().First(a => a.OuterUid == firebaseUid);

            if (!BCrypt.Net.BCrypt.Verify(accountPwdUpdateDto.CurrentPassword, account.PasswordHash)) {
                return BadRequest(new { message = "Incorrect password" });
            }
            if (BCrypt.Net.BCrypt.Verify(accountPwdUpdateDto.NewPassword, account.PasswordHash)) {
                return BadRequest(new { message = "The password must not match the current one" });
            }

            var firebaseApiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY")
                                 ?? throw new InvalidOperationException("FIREBASE_API_KEY not set.");
            var firebaseUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={firebaseApiKey}";
            var payload = new {
                idToken = HttpContext.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""),
                password = accountPwdUpdateDto.NewPassword,
                returnSecureToken = true
            };
            var response = await _httpClient.PostAsJsonAsync(firebaseUrl, payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { message = "Failed to update password in Firebase", error = errorResponse });
            }
            
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountPwdUpdateDto.NewPassword);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();

            return Ok();
        } catch (Exception _) {
            await transaction.RollbackAsync();
            
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while updating the password"}
            });
        }
    }
}