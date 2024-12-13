using backend.Enums;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Authorization;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;


public class AuthorizationRequestHandler : AuthorizationHandler<HasTypeRequirement>
{
    private readonly GymrecommenderContext _dbContext;
    private readonly ILogger<AuthorizationRequestHandler> _logger;

    public AuthorizationRequestHandler(GymrecommenderContext dbContext, ILogger<AuthorizationRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasTypeRequirement requirement)
    {
        _logger.LogInformation("Checking for authorization request");

        if (context.User.Identity?.IsAuthenticated != true)
        {
            _logger.LogError("Authentication failed.");
            return;
        }

        var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;

        if (userEmail == null)
        {
            _logger.LogWarning("User Email not found in claims.");
            return;
        }
            
        Account user = await _dbContext.Accounts.SingleOrDefaultAsync(u => u.Email == userEmail);
        
        if (user == null)
        {
            _logger.LogWarning("User not found in claims.");
            return;
        }
        
        foreach (string role in requirement.RequiredRoles)
        {
            if (user.Type.ToString().Equals(role))
            {
                _logger.LogInformation("Authorization succeeded for role: {Role}", role);
                context.Succeed(requirement);
                return;
            }
        }
        _logger.LogInformation("Authorization requirement handling completed, user Email: {userEmail} doesn't have the required role.", userEmail);

    }
}
