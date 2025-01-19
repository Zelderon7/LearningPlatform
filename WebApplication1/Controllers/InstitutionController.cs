using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models.Entities;

namespace WebApplication1.Controllers
{
    public class InstitutionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public InstitutionController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var institutions = _context.Institutions.ToList(); // Fetch institutions if needed
            return View(institutions);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Join(int code)
        {
            try
            {
                var inst = await _context.Institutions.FindAsync(code);
                if (inst == null)
                {
                    ViewData["Error"] = "Institution not found.";
                    return View("Index", await _context.Institutions.ToListAsync());
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge(); // Redirects to login if user is not authenticated
                }

                var request = new JoinInstitutionRequest(user, inst);
                _context.JoinInstitutionRequests.Add(request);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "An error occurred. Please try again.";
                return View("Index", await _context.Institutions.ToListAsync());
            }
        }
    }
}
