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
using WebApplication1.Models.Entities.CodingFiles;

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
            
            if (user == null)
            {
                return Challenge(); // Redirects to login if user is not authenticated
            }
            
            var userId = user.Id;
            var institutions = await _context.Institutions
                .Include(i => i.UserInstitutions)
                .Where(i => i.UserInstitutions != null && i.UserInstitutions.Any(ui => ui != null && ui.UserId == userId))
                .Select(i => new InstitutionDTO(i))
                .ToListAsync();

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
            User user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Challenge(); // Redirects to login if user is not authenticated
            }
            
            bool showPrivate = await _userManager.IsInRoleAsync(user, "ADMIN");
            var userId = user.Id;

            var data = await _context.Institutions
                .Include(i => i.JoinInstitutionRequests)
                .Include(i => i.UserInstitutions)
                .Where(i => showPrivate || i.IsPublic)
                .Where(i => (i.JoinInstitutionRequests == null || !i.JoinInstitutionRequests.Any(jir => jir != null && jir.UserId == userId))
                    && (i.UserInstitutions == null || !i.UserInstitutions.Any(ui => ui != null && ui.UserId == userId)))
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
                
                if (user == null)
                {
                    return Challenge(); // Redirects to login if user is not authenticated
                }
                
                bool filterPrivateClasses = !await _userManager.IsInRoleAsync(user, "ADMIN");
                var userId = user.Id;

                Institution? inst = await _context.Institutions
                    .Include(i => i.Classes)
                        .ThenInclude(c => c.UserClasses)
                    .FirstOrDefaultAsync(i => i.InstitutionId == id);

                if (inst == null)
                    return NotFound();

                if (inst.Classes != null)
                {
                    inst.Classes = inst.Classes
                        .Where(c => c != null && (!filterPrivateClasses || c.IsPublic || 
                            (c.UserClasses != null && c.UserClasses.Any(uc => uc != null && uc.UserId == userId))))
                        .ToList();
                }

                if(!await _context.UserInstitutions
                    .AnyAsync(ui => ui.InstitutionId == id && ui.UserId == userId))
                {
                    return Unauthorized();
                }

                return View("Institution", new FullInstitutionDTO(inst));
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "ADMIN,TEACHER")]
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

        [Authorize(Roles = "ADMIN,TEACHER")]
        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            JoinInstitutionRequest? request = await _context.JoinInstitutionRequests
                .Include(jir => jir.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null)
                return NotFound();
            User user = await _userManager.GetUserAsync(User);
            if(!await _userManager.IsInRoleAsync(user, "ADMIN"))
            {
                if (await _userManager.IsInRoleAsync(request.User, "TEACHER"))
                    return Unauthorized();
            }

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
            var userInstitutionId = await _context.UserInstitutions
                .Where(ui => ui.UserId == userId && ui.InstitutionId == instId)
                .Select(ui => ui.UserInstitutionId)
                .FirstOrDefaultAsync();

            if (userInstitutionId == 0) return NotFound();

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC DeleteUserInstitution @p0", userInstitutionId);

            return Redirect(Request.Headers["Referer"].ToString());
        }

    }
}
