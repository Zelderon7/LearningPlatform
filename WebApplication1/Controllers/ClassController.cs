using asp_server.Models.Entity.JoinTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            UserInstitution? userInstitution = await _context.UserInstitutions
                .FirstOrDefaultAsync(ui => ui.UserId == user.Id && ui.InstitutionId == @class.InstitutionId);
            if (userInstitution == null)
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

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Remove(int id)
        {
            Class? @class = await _context.Classes.FindAsync(id);
            if(@class == null) 
                return NotFound();

            int instId = @class.InstitutionId;

            _context.Classes.Remove(@class);
            _context.SaveChanges();

            return RedirectToAction("GetInstitution", "Institution", new { id = instId });
        }

        [HttpGet]
        public async Task<IActionResult> GetClass(int id)
        {
            User user = await _userManager.GetUserAsync(User);

            Class? @class = await _context.Classes
                .Include(c => c.ClassSections)
                .Include(c => c.UserClasses)
                .Where(c => c.ClassId == id)
                .Where(c => c.UserClasses.Any(uic => uic.UserId == user.Id) || c.IsPublic)
                .FirstOrDefaultAsync();


            if(@class == null)
                return NotFound();

            return View("Class", @class);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetUsersToAdd(int classId)
        {
            Class? @class = await _context.Classes
                .Include(c => c.UserClasses)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (@class == null)
                return NotFound();

            Institution? inst = await _context.Institutions
                .Include(i => i.UserInstitutions)
                    .ThenInclude(ui => ui.User)
                .FirstAsync(i => i.InstitutionId == @class.InstitutionId);

            if (inst == null)
                return NotFound();

            User[] users = inst.UserInstitutions
                .Select(ui => ui.User)
                .Where(u => !@class.UserClasses.Any(uc => uc.UserId == u.Id)) //Exclude users that are already in the class
                .ToArray();

            ViewData["classId"] = classId;
            return View("AddParticipants", users);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> AddUserToClass(int classId, int userId)
        {
            User? user = await _context.Users
                .FindAsync(userId);

            if(user == null) return NotFound();

            Class @class = await _context.Classes
                .Include(c => c.UserClasses)
                .Include(c => c.Institution)
                    .ThenInclude(i => i.UserInstitutions)
                .FirstAsync(c => c.ClassId == classId);

            if(@class.UserClasses.Any(x => x.UserId == userId)) //User is already in the class
                return Redirect(Request.Headers["Referer"].ToString());

            UserInstitution? userInstitution = @class.Institution.UserInstitutions
                .FirstOrDefault(x => x.UserId == userId);
            if (userInstitution == null)
                return Forbid(); //User is not in the institution

            UserClass userClass = new UserClass
            {
                UserId = userId,
                ClassId = classId,
            };

            _context.UserClasses.Add(userClass);
            _context.SaveChanges();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public IActionResult CreateSection(int classId)
        {
            ViewData["classId"] = classId;
            return View();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> CreateSection(ClassSection section)
        {
            try
            {
                _context.ClassSections.Add(section);
                _context.SaveChanges();

                return RedirectToAction("GetClass", new {id = section.ClassId});
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSection(int id)
        {
            ClassSection? section = _context.ClassSections
                .Include(x => x.Lessons)
                .FirstOrDefault(s => s.Id == id);

            if (section == null)
                return NotFound();

            return View("ClassSection", section);
        }
    }
}
