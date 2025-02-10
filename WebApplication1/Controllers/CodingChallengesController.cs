using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models.Entities;

namespace WebApplication1.Controllers
{
    public class CodingChallengesController : Controller
    {
        ApplicationDbContext _context;
        UserManager<User> _userManager;
        public CodingChallengesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            List<CodingTask> model = await _context.CodingTasks.ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> OpenChallenge(int id)
        {
            throw new NotImplementedException();
        }
        [HttpGet]
        public IActionResult Create()
        {
            CodingTask model = new CodingTask();
            ViewBag.Languages = new List<SelectListItem>
            {
                new SelectListItem { Value = "python", Text = "Python" }
            };
            return View(model);
        }

        [Authorize(Roles = "TEACHER,ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create(CodingTask data)
        {
            User user = await _userManager.GetUserAsync(User);
            data.AuthorId = user.Id;

            _context.Add(data);
            _context.SaveChanges();

            return RedirectToAction("OpenChallenge", data.Id);
        }
    }
}
