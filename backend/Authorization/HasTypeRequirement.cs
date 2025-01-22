namespace backend.Authorization;

using Microsoft.AspNetCore.Authorization;

public class HasTypeRequirement : IAuthorizationRequirement
{
    public string[] RequiredRoles { get; }

    public HasTypeRequirement(params string[] roles)
    {
        RequiredRoles = roles;
    }
}
