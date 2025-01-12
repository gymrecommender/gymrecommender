using System.Security.Claims;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GymrecommenderContext _dbContext;

    public AuthenticationService()
    {
    }

    public AuthenticationService(IHttpContextAccessor httpContextAccessor, GymrecommenderContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Retrieves the current authenticated user's ID by extracting the email from the auth token and querying the Account entity.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user's Guid ID.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated.</exception>
    /// <exception cref="Exception">Thrown when the email claim is missing or the user is not found in the database.</exception>
    public async Task<Guid> GetCurrentUserIdAsync()
    {
        // Check if the HttpContext is available
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // Access the current user principal
        var user = _httpContextAccessor.HttpContext.User;

        string userEmail;
        // Check if the user is authenticated
        if (user.Identity?.IsAuthenticated != true)
        {
            //throw new UnauthorizedAccessException("User is not authenticated.");
            //TODO: temporary for test purposes, remove it later
            userEmail = "ilyarotan2@gmail.com";
        }
        else
        {
            // Attempt to retrieve the email claim from the token
            userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        }

        if (string.IsNullOrEmpty(userEmail))
        {
            throw new Exception("User email not found in claims.");
        }

        // Query the Account entity to find the user by email
        Account account = await _dbContext.Accounts.SingleOrDefaultAsync(u => u.Email == userEmail);

        if (account == null)
        {
            throw new Exception("User email not found in db.");
            ;
        }

        return account.Id;
    }
}