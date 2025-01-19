using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models.DTOs;
using WebApplication1.Models.Entities;

namespace WebApplication1.Controllers
{
    [Authorize]
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

        [HttpPost]
        public async Task<IActionResult> Join(string code)
        {
            try
            {
                var inst = await _context.Institutions.FirstAsync(i => i.Code == code);
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

                ViewData["Success"] = "Request created successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "An error occurred. Please try again.";
                return View("Index", await _context.Institutions.ToListAsync());
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create(InstitutionDTO inst)
        {
            try
            {
                Institution institution = inst.ToInstitution();
                _context.Institutions.Add(institution);
                _context.SaveChanges();

                ViewData["Success"] = "Institution created successfully";
                return RedirectToAction("Index");
            }
            catch
            {
                ViewData["Error"] = "Unexpected error occurred, please try again";
                return View("Create");
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
    }
}
