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
            List<CodingTask> model = await _context.CodingTasks.ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> OpenChallenge(int id)
        {
            CodingTask? task = await _context.CodingTasks.SingleOrDefaultAsync(c => c.Id == id);
            if(task == null)
                return NotFound();

            string folderDir = Path.Combine(AppConstants.CodingTasksDir, task.Id.ToString());
            if (!Directory.Exists(folderDir))
                return NotFound("Task directory does not exists");

            string[] files = Directory.GetFiles(folderDir);

            if(files.Length == 0)
                return NotFound("Task directory is empty");

            CodingIDEVM model = new CodingIDEVM
            {
                Task = task,
                FolderDir = folderDir,
                FilePaths = files
            };

            TempData["CodingIDEVMModel"] = JsonConvert.SerializeObject(model);

            return RedirectToAction("Index", "Coding");
        }


        [HttpGet]
        public IActionResult Create()
        {
            CodingTask model = new CodingTask();
            ViewBag.Languages = new List<SelectListItem>
            {
                new SelectListItem { Value = "python", Text = "Python" }
            };

            ViewBag.Templates = Directory
                .EnumerateDirectories(AppConstants.LanguageTemplatesDir)
                .Skip(1)
                .Select(d => new SelectListItem(Path.GetFileName(d), Path.GetFileName(d)))
                .ToList();

            return View(model);
        }

        [Authorize(Roles = "TEACHER,ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create(CodingTask data, string template)
        {
            User user = await _userManager.GetUserAsync(User);
            data.AuthorId = user.Id;

            _context.Add(data);
            _context.SaveChanges();

            string folderDir = Path.Combine(AppConstants.CodingTasksDir, data.Id.ToString());
            try
            {
                await _directoryService.InitializeTaskFolder(folderDir, data.Language, template);
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
