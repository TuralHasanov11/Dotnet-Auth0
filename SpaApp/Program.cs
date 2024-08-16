using Auth0.AspNetCore.Authentication;
using Auth0.AspNetCore.Authentication.BackchannelLogout;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using SharedKernel.Identity;
using SpaApp.Identity;
using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(async request =>
        {
            var accessToken = await request.HttpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.ProxyRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        });
    });

builder.Services.AddAuth0WebAppAuthentication(options =>
   {
       options.Domain = builder.Configuration["Auth0:Domain"]!;
       options.ClientId = builder.Configuration["Auth0:ClientId"]!;
       options.ClientSecret = builder.Configuration["Auth0:ClientSecret"]!;

       options.Scope = $"{Auth0Scopes.Openid} {Auth0Scopes.Profile} {Auth0Scopes.Email} {Auth0Scopes.ViewCourse}";

       options.AccessDeniedPath = "/Home/AccessDenied";

       options.OpenIdConnectEvents = new OpenIdConnectEvents
       {
           OnTokenValidated = async context =>
           {
               var permissions = context.TokenEndpointResponse.Scope.Split(' ');

               var claims = permissions.Select(permission => new Claim(IdentityClaimTypes.Permissions, permission));

               var identity = context.Principal.Identities.First();

               identity?.AddClaims(claims);
           }
       };
   }).WithAccessToken(options =>
    {
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.UseRefreshTokens = true;
        options.Events = new Auth0WebAppWithAccessTokenEvents
        {
            OnMissingRefreshToken = async (context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var authenticationProperties = new LogoutAuthenticationPropertiesBuilder().WithRedirectUri("/").Build();
                await context.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, ApplicationAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();

builder.Services.AddHttpClient();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{

}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseBackchannelLogout();

app.MapGet("/api/authentication/register", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithParameter("screen_hint", "signup")
        .WithRedirectUri(returnUrl)
        .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/api/authentication/login", async (HttpContext httpContext, string returnUrl = "/") =>
    {
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(returnUrl)
        .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/api/authentication/logout", async (HttpContext httpContext) => {
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri("/")
            .Build();

    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapGet("/api/authentication/user-info", async (HttpContext httpContext) =>
{
    return TypedResults.Ok(new UserInfoResponse(
        httpContext.User.Identity.Name,
        httpContext.User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value,
        httpContext.User.FindFirst(c => c.Type == Auth0Scopes.Picture)?.Value,
        httpContext.User.FindAll(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray(),
        httpContext.User.FindAll(c => c.Type == IdentityClaimTypes.Permissions).Select(c => c.Value).ToArray()));
}).RequireAuthorization();


app.MapReverseProxy();

app.MapForwarder("/{**catch-all}", "http://localhost:5173");

await app.RunAsync();