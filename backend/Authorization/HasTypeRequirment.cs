namespace backend.Authorization;

using Microsoft.AspNetCore.Authorization;

public class HasTypeRequierment : IAuthorizationRequirement
{
    public string[] RequiredRoles { get; }

    public HasTypeRequierment(params string[] roles)
    {
        RequiredRoles = roles;
    }
}
