using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Identity;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            return View(new UserProfileViewModel(
                User.Identity.Name,
                User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value,
                User.FindFirst(c => c.Type == Auth0Scopes.Picture)?.Value));
        }
    }
}