using Auth0.AspNetCore.Authentication;
using Auth0.AspNetCore.Authentication.BackchannelLogout;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;
using SharedKernel.Identity;
using System.Net.Http.Headers;
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddSingleton<IAuthorizationPolicyProvider, ApplicationAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();

builder.Services.AddHttpClient();

builder.Services.AddHttpClient("NoSslVerificationClient", options =>
{
    options.BaseAddress = new Uri("https://localhost:7096");
})
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Disabling SSL certificate validation
        ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
    })
//.AddResilienceHandler("ApiService", configuration =>
//{
//    configuration.AddConcurrencyLimiter(100);

//    configuration.AddRetry(new HttpRetryStrategyOptions
//    {
//        MaxRetryAttempts = 5,
//        BackoffType = DelayBackoffType.Exponential,
//        UseJitter = true,
//        Delay = TimeSpan.Zero
//    });

//    configuration.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
//    {
//        SamplingDuration = TimeSpan.FromSeconds(5),
//        FailureRatio = 0.9,
//        MinimumThroughput = 5,
//        BreakDuration = TimeSpan.FromSeconds(5)
//    });

//    configuration.AddTimeout(TimeSpan.FromSeconds(5));
//});
//.AddStandardResilienceHandler()
//.Configure(options => {
//    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

//    options.Retry.MaxRetryAttempts = 5;
//    options.Retry.Delay = TimeSpan.Zero;

//    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
//    options.CircuitBreaker.FailureRatio = 0.9;
//    options.CircuitBreaker.MinimumThroughput = 5;
//    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

//    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
//});
.AddStandardHedgingHandler()
.Configure(options =>
{
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

    options.Hedging.MaxHedgedAttempts = 5;
    options.Hedging.Delay = TimeSpan.FromMicroseconds(1);

    options.Endpoint.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
    options.Endpoint.CircuitBreaker.FailureRatio = 0.9;
    options.Endpoint.CircuitBreaker.MinimumThroughput = 5;
    options.Endpoint.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

    options.Endpoint.Timeout.Timeout = TimeSpan.FromSeconds(1);
});

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

app.MapGet("/client/courses", async (HttpContext httpContext, IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient("NoSslVerificationClient");

    var accessToken = await httpContext.GetTokenAsync("access_token");

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    using var response = await httpClient.GetAsync("api/courses");

    if(!response.IsSuccessStatusCode)
    {
        return "Forbidden";
    }

    return await response.Content.ReadAsStringAsync();
});

app.MapReverseProxy();

await app.RunAsync();