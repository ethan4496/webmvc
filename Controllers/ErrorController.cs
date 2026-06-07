using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/401")]
        public IActionResult UnauthorizedAccess()
        {
            return View();
        }
        [Route("Error/400")]
        public IActionResult BadRequestPage()
        {
            return View();
        }
    }
}
