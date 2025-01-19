using asp_server.Models.Entity.JoinTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models.Entities;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ClassController : Controller
    {
        ApplicationDbContext _context;
        UserManager<User> _userManager;

        public ClassController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create(int instId)
        {
            ViewData["InstitutionId"] = instId;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Class @class)
        {
            User user = await _userManager.GetUserAsync(User);
            if (!_context.UserInstitutions.Any(ui => ui.UserId == user.Id && ui.InstitutionId == @class.InstitutionId))
            {
                return Forbid();
            }

            _context.Classes.Add(@class);
            _context.SaveChanges();

            UserClass uc = new UserClass
            {
                ClassId = @class.ClassId,
                UserId = user.Id
            };
            _context.UserClasses.Add(uc);
            _context.SaveChanges(true);

            return RedirectToAction("GetInstitution", "Institution", new { id = @class.InstitutionId });
        }
    }
}
