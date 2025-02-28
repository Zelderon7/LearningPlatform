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
            CodingTask? task = await _context.CodingTasks
                .SingleOrDefaultAsync(c => c.Id == id);

            if(task == null)
                return NotFound();

            User user = await _userManager.GetUserAsync(User);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(2)
            };

            List<CodingFileDTO> files;

            if (task.AuthorId == user.Id)
            {
                #region Open original directory

                files = await _directoryService.GetFilteredFilesByRestriction((int)task.FolderId);

                task.Folder = null; //Ensures no self referencing loops
                CodingIDEVM model1 = new CodingIDEVM
                {
                    Task = task,
                    Files = files
                };

                Response.Cookies.Append("CodingIDEVMModel", JsonConvert.SerializeObject(model1), cookieOptions);

                return RedirectToAction("Index", "Coding");

                #endregion
            }

            files = await _directoryService.OpenUserTask(id, user.Id);

            task.Folder = null; //Ensures no self referencing loops
            CodingIDEVM model = new CodingIDEVM
            {
                Task = task,
                Files = files,
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

            ViewBag.Templates = new List<SelectListItem>() { };

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
                data.MaxPoints = 100;
            }

            _context.CodingTasks.Add(data);
            _context.SaveChanges();

            
            try
            {
                await _directoryService.InitializeTaskFolder(data);
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
