using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication1.Data;
using WebApplication1.Models.Entities;
using WebApplication1.Models.Entities.CodingFiles;
using WebApplication1.Models.VMs;
using WebApplication1.Models.DTOs;
using WebApplication1.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

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

            List<TaskTemplate> templates = await _context.TaskTemplates
                .ToListAsync();

            ViewBag.Templates = templates;

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
            CodingTask? task = await _context.CodingTasks
                .SingleOrDefaultAsync(c => c.Id == id);

            if(task == null)
                return NotFound();

            User? user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            return RedirectToAction("Index", "Coding", new {taskId = task.Id});
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

            List<SelectListItem> templates = await _context.TaskTemplates
                .Select(t => new SelectListItem(t.Name, t.Id.ToString()))
                .ToListAsync();

            ViewBag.Templates = templates;

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
        public async Task<IActionResult> Create(CodingTask data, string templateId, int classId)
        {
            User user = await _userManager.GetUserAsync(User);
            data.AuthorId = user.Id;
            if(classId > 0)
            {
                data.ClassId = classId;
                data.MaxPoints = 100;
            }

            _context.CodingTasks.Add(data);
            _context.SaveChanges();

            
            try
            {
                int tId = int.Parse(templateId);
                await _directoryService.InitializeTaskFolder(data);

                if(tId > 0)
                {
                    await _directoryService.CopyTemplateToTask(tId, data.Id);
                }
            }
            catch (Exception ex)
            {
                int id = data.Id;
                _context.Remove(data);
                _context.SaveChanges(true);
                throw ex;
            }
            

            return RedirectToAction("OpenChallenge", new { id = data.Id });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public IActionResult CreateTemplate()
        {
            TaskTemplate model = new TaskTemplate();
            ViewBag.Languages = new List<SelectListItem>
            {
                new SelectListItem { Value = "python", Text = "Python" }
            };

            return View(model);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> CreateTemplate(TaskTemplate data)
        {
            if(data == null)
                throw new ArgumentNullException("data");
            
            await _directoryService.InitializeTaskTemplate(data);            

            return RedirectToAction("OpenTemplate", new { id = data.Id });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> OpenTemplate(int id)
        {
            TaskTemplate? template = await _context.TaskTemplates
                .Include(x => x.Folder)
                .SingleOrDefaultAsync(x => x.Id == id);
            if(template == null)
                return NotFound();

           
            return RedirectToAction("GetTemplate", "Coding", new {templateId = id});
        }
    }
}
