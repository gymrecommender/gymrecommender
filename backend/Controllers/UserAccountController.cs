using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserAccountController : AccountControllerTemplate {
    public UserAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) :
        base(context, appSettings) {
        _accountType = AccountType.user;
    }

    [HttpGet]
    public async Task<IActionResult> GetData(int page = 1, int sort = 1, bool ascending = true) {
        return await base.GetData(page, sort, ascending, _accountType);
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(AccountDto accountDto) {
        return await SignUp(accountDto, _accountType);
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetByUsername(string username, AccountType? accountType) {
        return await base.GetByUsername(username, _accountType);
    }

    [HttpPut("{username}")]
    public async Task<IActionResult> UpdateByUsername(string username, AccountPutDto accountPutDto) {
        return await base.UpdateByUsername(username, accountPutDto, _accountType);
    }

    [HttpDelete("{username}")]
    public async Task<IActionResult> DeleteByUsername(string username) {
        return await base.DeleteByUsername(username, _accountType);
    }

    [HttpGet("{username}/token")]
    public async Task<IActionResult> GetTokenByUsername(string username) {
        return await base.GetTokenByUsername(username, _accountType);
    }

    [HttpPut("{username}/token")]
    public async Task<IActionResult> UpdateTokenByUsername(string username, AccountTokenDto accountTokenDto) {
        return await base.UpdateTokenByUsername(username, accountTokenDto, _accountType);
    }

    [HttpPost("{username}/login")]
    public async Task<IActionResult> Login(string username, AccountTokenDto logInDto) {
        return await base.Login(username, logInDto, _accountType);
    }
    
    [HttpDelete("{username}/logout")]
    public async Task<IActionResult> Logout(string username) {
        return await base.Logout(username, _accountType);
    }

    [HttpPut("{requestId}/update-name")]
    public async Task<IActionResult> UpdateRequestName(Guid requestId, [FromBody] UpdateRequestNameDto updateDto)
    {
        // Validate input
        if (requestId == Guid.Empty)
        {
            return BadRequest("Invalid request ID.");
        }

        if (string.IsNullOrWhiteSpace(updateDto?.Name))
        {
            return BadRequest("Request name cannot be null or empty.");
        }

        try
        {
            // Fetch the request from the database
            var request = await _context.Requests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            // Check if the request exists
            if (request == null)
            {
                return NotFound($"No request found with ID {requestId}.");
            }

            // Validate that the associated account type is 'user'
            if (request.User.AccountType != AccountType.user)
            {
                return Forbid("Only user accounts can update request names.");
            }

            // Update only the Name property
            request.Name = updateDto.Name;

            // Save changes to the database
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();

            // Return a success response
            return Ok(new { message = "Request name updated successfully." });
        }
        catch (Exception ex)
        {
            // Log the exception (use a logging framework)
            Console.Error.WriteLine(ex.Message);

            // Return a generic error response
            return StatusCode(500, "An error occurred while updating the request.");
        }
    }
    [HttpGet("requests")]
    public async Task<IActionResult> GetUserRequests()
    {
        try
        {
            // Get the authenticated user ID
            var userId = GetAuthenticatedUserId();
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Ensure the account type is 'user'
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == userId && a.AccountType == AccountType.user);
            if (account == null)
            {
                return Forbid("Only user accounts can access this functionality.");
            }

            // Fetch requests associated with the user
            var requests = await _context.Requests
                .Where(r => r.UserId == userId)
                .Select(r => new {
                    r.Id,
                    r.Name,
                    r.RequestedAt,
                    r.OriginLatitude,
                    r.OriginLongitude,
                    r.TimePriority,
                    r.TotalCostPriority,
                    r.MinCongestionRating,
                    r.MinRating,
                    r.MinMembershipPrice
                })
                .ToListAsync();

            // Return the requests
            return Ok(requests);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.Error.WriteLine(ex.Message);

            // Return a generic error response
            return StatusCode(500, "An error occurred while retrieving user requests.");
        }
    }
    [HttpGet("pause/{pauseId}")]
    public async Task<IActionResult> GetPauseById(Guid pauseId)
    {
        try
        {
            // Query the RequestPause by ID
            var pause = await _context.RequestPauses
                .Include(rp => rp.User)
                .FirstOrDefaultAsync(rp => rp.Id == pauseId);

            if (pause == null)
            {
                return NotFound("Pause not found.");
            }

            // Ensure the pause belongs to a user account
            if (pause.User == null || pause.User.Type != AccountType.user)
            {
                return BadRequest("Pause does not belong to a user account.");
            }

            // Return the pause details
            return Ok(new
            {
                PauseId = pause.Id,
                Username = pause.User.Username,
                IpAddress = pause.Ip != null ? System.Text.Encoding.UTF8.GetString(pause.Ip) : null,
                StartedAt = pause.StartedAt
            });
        }
        catch (Exception ex)
        {
            // Log the error
            Console.Error.WriteLine(ex.Message);

            return StatusCode(500, "An error occurred while retrieving the pause.");
        }
    }
    [HttpGet("pause/by-ip")]
    public async Task<IActionResult> GetPauseByIp([FromQuery] string ipAddress)
    {
        try
        {
            // Convert the IP address to a byte array
            var ipBytes = System.Text.Encoding.UTF8.GetBytes(ipAddress);

            // Query the RequestPause by IP
            var pause = await _context.RequestPauses
                .Include(rp => rp.User)
                .FirstOrDefaultAsync(rp => rp.Ip != null && rp.Ip.SequenceEqual(ipBytes));

            if (pause == null)
            {
                return NotFound("Pause not found for the given IP address.");
            }

            // Ensure the pause belongs to a user account
            if (pause.User == null || pause.User.Type != AccountType.user)
            {
                return BadRequest("Pause does not belong to a user account.");
            }

            // Return the pause details
            return Ok(new
            {
                PauseId = pause.Id,
                Username = pause.User.Username,
                IpAddress = ipAddress,
                StartedAt = pause.StartedAt
            });
        }
        catch (Exception ex)
        {
            // Log the error
            Console.Error.WriteLine(ex.Message);

            return StatusCode(500, "An error occurred while retrieving the pause.");
        }
    }
    [HttpPost("pause")]
    public async Task<IActionResult> SavePause([FromBody] SavePauseDto pauseDto)
    {
        try
        {
            // Validate the user
            var user = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == pauseDto.UserId && a.Type == AccountType.user);

            if (user == null)
            {
                return BadRequest("User not found or not of type 'user'.");
            }

            // Convert the IP address to a byte array
            var ipBytes = System.Text.Encoding.UTF8.GetBytes(pauseDto.IpAddress);

            // Create a new RequestPause entity
            var requestPause = new RequestPause
            {
                Id = Guid.NewGuid(),
                UserId = pauseDto.UserId,
                Ip = ipBytes,
                StartedAt = DateTime.UtcNow
            };

            // Save to the database
            _context.RequestPauses.Add(requestPause);
            await _context.SaveChangesAsync();

            // Return the created pause details
            return CreatedAtAction(nameof(GetPauseById), new { pauseId = requestPause.Id }, new
            {
                PauseId = requestPause.Id,
                UserId = requestPause.UserId,
                IpAddress = pauseDto.IpAddress,
                StartedAt = requestPause.StartedAt
            });
        }
        catch (Exception ex)
        {
            // Log the error
            Console.Error.WriteLine(ex.Message);

            return StatusCode(500, "An error occurred while saving the pause.");
        }
    }
}
