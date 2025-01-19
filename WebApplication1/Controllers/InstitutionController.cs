using asp_server.Models.Entity.JoinTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
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
            var institutions = _context.Institutions
                .Select(i => new InstitutionDTO(i))
                .ToList(); // Fetch institutions if needed
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

                User user = await _userManager.GetUserAsync(User);

                UserInstitution ui = new UserInstitution
                {
                    UserId = user.Id,
                    InstitutionId = institution.InstitutionId,
                    Role = "CREATOR"
                };

                _context.UserInstitutions.Add(ui);
                _context.SaveChanges();

                ViewData["Success"] = "Institution created successfully";
                return View("Index");
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

        [HttpGet]
        public async Task<IActionResult> Discover()
        {
            var data = await _context.Institutions.Where(i => i.IsPublic)
                .Select(i => new InstitutionDTO(i))
                .ToListAsync();


            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetInstitution(int id)
        {
            try
            {
                Institution? inst = await _context.Institutions.FindAsync(id);
                if (inst == null)
                    return NotFound();


                return View("Institution", new InstitutionDTO(inst));
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}
