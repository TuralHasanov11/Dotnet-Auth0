using Microsoft.AspNetCore.Authorization;

namespace WebApi.Identity;

public class ScopeAuthorizationRequirement(string scope, string issuer) : IAuthorizationRequirement
{
    public string Scope { get; } = scope;
    public string Issuer { get; } = issuer;
}