using Microsoft.AspNetCore.Authorization;
using SharedKernel.Identity;

namespace WebApi.Identity;

public class ScopeAuthorizationHandler
    : AuthorizationHandler<ScopeAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeAuthorizationRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == Auth0ClaimTypes.Scope && c.Issuer == requirement.Issuer))
            return Task.CompletedTask;

        var scopes = context.User
            .FindFirst(c => c.Type == Auth0ClaimTypes.Scope && c.Issuer == requirement.Issuer)?.Value
            .Split(' ');

        if (scopes is not null && Array.Exists(scopes, element => element == requirement.Scope))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}