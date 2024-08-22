using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Identity;

namespace WebApp.Controllers
{
    public class CoursesController : Controller
    {
        [Authorize(Policy = Auth0Scopes.ViewCourse)]
        public IActionResult Index()
        {
            return View();
        }
    }
}