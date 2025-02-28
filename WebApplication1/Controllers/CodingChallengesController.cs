using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication1.Data;
using WebApplication1.Models.Entities;
using WebApplication1.Models.VMs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class CodingChallengesController : Controller
    {
        ApplicationDbContext _context;
        UserManager<User> _userManager;
        DirectoryService _directoryService;
        public CodingChallengesController(ApplicationDbContext context, UserManager<User> userManager, DirectoryService directoryService)
        {
            _context = context;
            _userManager = userManager;
            _directoryService = directoryService;
        }

        public async Task<IActionResult> Index()
        {
            List<CodingTask> model = await _context.CodingTasks
                .Where(x => x.ClassId == null)
                .ToListAsync();

             return View(model);
        }

        [Authorize]
        public async Task<IActionResult> GetAssignments()
        {
            User user = await _userManager.GetUserAsync(User);

            List<CodingTask> tasks = await _context.CodingTasks
                .Include(ct => ct.Class)
                    .ThenInclude(c => c.UserClasses)
                .Where(ct => ct.ClassId != null)
                .Where(ct => ct.Class.UserClasses
                    .Any(uc => uc.UserId == user.Id))
                .ToListAsync();

            return View("Index", tasks);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OpenChallenge(int id)
        {
            CodingTask? task = await _context.CodingTasks.SingleOrDefaultAsync(c => c.Id == id);
            if(task == null)
                return NotFound();

            User user = await _userManager.GetUserAsync(User);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(2)
            };

            if (task.AuthorId == user.Id)
            {
                #region Open original directory

                string folderDir = Path.Combine(AppConstants.CodingTasksDir, task.Id.ToString());
                string[] files = Directory.GetFiles(folderDir);

                CodingIDEVM model1 = new CodingIDEVM
                {
                    Task = task,
                    FolderDir = folderDir,
                    FilePaths = files
                };

                Response.Cookies.Append("CodingIDEVMModel", JsonConvert.SerializeObject(model1), cookieOptions);

                return RedirectToAction("Index", "Coding");

                #endregion
            }

            (string folderDir, string[] filePaths) data = await _directoryService.OpenTask(id, user.Id);

            CodingIDEVM model = new CodingIDEVM
            {
                Task = task,
                FolderDir = data.folderDir,
                FilePaths = data.filePaths
            };

            

            Response.Cookies.Append("CodingIDEVMModel", JsonConvert.SerializeObject(model), cookieOptions);

            return RedirectToAction("Index", "Coding");
        }

        [Authorize(Roles = "ADMIN,TEACHER")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CodingTask model = new CodingTask();
            ViewBag.Languages = new List<SelectListItem>
            {
                new SelectListItem { Value = "python", Text = "Python" }
            };

            User user = await _userManager.GetUserAsync(User);

            ViewBag.Templates = Directory
                .EnumerateDirectories(AppConstants.LanguageTemplatesDir)
                .Skip(1)
                .Select(d => new SelectListItem(Path.GetFileName(d), Path.GetFileName(d)))
                .ToList();

            ViewBag.Classes = await _context.UserClasses
                .Include(uc => uc.User)
                .Include(uc => uc.Class)
                .Where(uc => uc.UserId == user.Id)
                .Select(uc => uc.Class)
                .ToListAsync();

            return View(model);
        }

        [Authorize(Roles = "TEACHER,ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create(CodingTask data, string template, int classId)
        {
            User user = await _userManager.GetUserAsync(User);
            data.AuthorId = user.Id;
            if(classId > 0)
            {
                data.ClassId = classId;
                data.Deadline = DateTime.UtcNow.AddDays(7);
                data.MaxPoints = 100;
            }

            _context.Add(data);
            _context.SaveChanges();

            string folderDir = Path.Combine(AppConstants.CodingTasksDir, data.Id.ToString());
            try
            {
                   await _directoryService.InitializeTaskFolder(folderDir, data.Language, template ?? "");
            }
            catch (Exception ex)
            {
                int id = data.Id;
                _context.Remove(data);
                _context.SaveChanges(true);
                throw ex;
            }
            

            return RedirectToAction("OpenChallenge", data.Id);
        }
    }
}
