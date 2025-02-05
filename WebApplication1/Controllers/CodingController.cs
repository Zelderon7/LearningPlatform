using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class CodingController : Controller
    {
        public IActionResult Index()
        {
            return View("IDE");
        }
    }
}
