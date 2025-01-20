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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            User user = await _userManager.GetUserAsync(User);

            var institutions = await _context.Institutions
                .Include(i => i.UserInstitutions)
                .Where(i => i.UserInstitutions.Any(ui => ui.UserId == user.Id))
                .Select(i => new InstitutionDTO(i))
                .ToListAsync(); // Fetch institutions if needed

            return View(institutions);
        }

        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {
            try
            {
                var inst = await _context.Institutions.FirstOrDefaultAsync(i => i.InstitutionId == id);
                if (inst == null)
                {
                    ViewData["Error"] = "Institution not found.";
                    return View("Index", await _context.Institutions.Select(i => new FullInstitutionDTO(i)).ToListAsync());
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge(); // Redirects to login if user is not authenticated
                }

                if (await _context.JoinInstitutionRequests.AnyAsync(r => r.UserId == user.Id && r.InstitutionId == inst.InstitutionId))
                {
                    // Get the referer URL from the request headers
                    return Redirect(Request.Headers["Referer"].ToString());
                }

                var request = new JoinInstitutionRequest(user, inst);
                _context.JoinInstitutionRequests.Add(request);
                await _context.SaveChangesAsync();

                // Get the referer URL from the request headers
                var refererUrl = Request.Headers["Referer"].ToString();
                return Redirect(refererUrl);
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "An error occurred. Please try again.";
                return View("Index", await _context.Institutions.Select(i => new FullInstitutionDTO(i)).ToListAsync());
            }
        }

        [HttpPost]
        public async Task<IActionResult> JoinWithCode(string code)
        {
            try
            {
                var inst = await _context.Institutions.FirstAsync(i => i.Code == code);
                if (inst == null)
                {
                    ViewData["Error"] = "Institution not found.";
                    return View("Index", await _context.Institutions.Select(i => new FullInstitutionDTO(i)).ToListAsync());
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge(); // Redirects to login if user is not authenticated
                }

                if(await _context.JoinInstitutionRequests.AnyAsync(r => r.UserId == user.Id && r.InstitutionId == inst.InstitutionId))
                {
                    return RedirectToAction("Index");
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
                return View("Index", await _context.Institutions.Select(i => new FullInstitutionDTO(i)).ToListAsync());
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
                User user = await _userManager.GetUserAsync(User);
                bool filterPrivateClasses = !await _userManager.IsInRoleAsync(user, "ADMIN");

                Institution? inst = await _context.Institutions
                    .Include(i => i.Classes)
                        .ThenInclude(c => c.UserClasses)
                    .FirstOrDefaultAsync(i => i.InstitutionId == id);

                inst.Classes = inst.Classes
                    .Where(c => filterPrivateClasses ? c.IsPublic || c.UserClasses.Any(uc => uc.UserId == user.Id) : true)
                    .ToList();

                if (inst == null)
                    return NotFound();

                if(!await _context.UserInstitutions
                    .AnyAsync(ui => ui.InstitutionId == id && ui.UserId == user.Id))
                {
                    return Unauthorized();
                }

                return View("Institution", new FullInstitutionDTO(inst));
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetRequests(int? instId)
        {
            List<JoinInstitutionRequest> requests;
            if(instId == null)
            {
                requests = await _context.JoinInstitutionRequests
                    .Include(ji => ji.User)
                    .Include(ji => ji.Institution)
                    .ToListAsync();
            }
            else
            {
                requests = await _context.JoinInstitutionRequests
                    .Include(ji => ji.User)
                    .Include(ji => ji.Institution)
                    .Where(ji => ji.InstitutionId == instId)
                    .ToListAsync();
            }
            

            return View("RequestsList", requests);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            JoinInstitutionRequest? request = await _context.JoinInstitutionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null)
                return NotFound();

            UserInstitution ui = new UserInstitution
            {
                InstitutionId = request.InstitutionId,
                UserId = request.UserId,
                Role = "STUDENT"
            };

            if(!await _context.UserInstitutions
                .AnyAsync(x => x.InstitutionId == ui.InstitutionId && x.UserId == ui.UserId))
            {
                _context.UserInstitutions.Add(ui);
            }
            _context.JoinInstitutionRequests.Remove(request);
            _context.SaveChanges();


            // Get the referer URL from the request headers
            var refererUrl = Request.Headers["Referer"].ToString();
            return Redirect(refererUrl);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            JoinInstitutionRequest? request = await _context.JoinInstitutionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null)
                return NotFound();

            _context.JoinInstitutionRequests.Remove(request);
            _context.SaveChanges();


            // Get the referer URL from the request headers
            var refererUrl = Request.Headers["Referer"].ToString();
            return Redirect(refererUrl);
        }

        [HttpGet]
        public async Task<IActionResult> GetParticipants(int instId)
        {
            List<User> participants = await _context.UserInstitutions
                .Include(ui => ui.User)
                .Where(ui => ui.InstitutionId == instId)
                .Select(ui => ui.User)
                .ToListAsync();

            ViewData["instId"] = instId;
            return View("ParticipantsList", participants);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> RemoveFromInstitution(int userId, int instId)
        {
            UserInstitution? ui = await _context.UserInstitutions
                .FirstOrDefaultAsync(x => x.InstitutionId == instId && x.UserId == userId);
            
            if(ui == null) return NotFound();

            _context.UserInstitutions.Remove(ui);
            _context.SaveChanges();

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
