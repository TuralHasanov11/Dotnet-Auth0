using Auth0.AspNetCore.Authentication;
using Auth0.AspNetCore.Authentication.BackchannelLogout;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using SharedKernel.Identity;
using WebApp.Identity;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

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

builder.Services
   .AddAuth0WebAppAuthentication(options =>
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
               Console.WriteLine(context.TokenEndpointResponse.Scope);
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseBackchannelLogout();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


//app.MapGet("/api/courses", async (HttpContext httpContext, IHttpClientFactory httpClientFactory) =>
//{
//    var httpClient = httpClientFactory.CreateClient();

//    var accessToken = await httpContext.GetTokenAsync("access_token");

//    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

//    using var response = await httpClient.GetAsync("https://localhost:7096/api/courses");

//    return await response.Content.ReadAsStringAsync();
//});

app.MapReverseProxy();

await app.RunAsync();
