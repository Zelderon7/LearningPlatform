using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models.Entities;
using WebApplication1.Models.VMs;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class LessonController : Controller
    {
        ApplicationDbContext _context;
        UserManager<User> _userManager;

        public LessonController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int lessonId)
        {
            User user = await _userManager.GetUserAsync(User);
            Lesson? lesson = await _context.Lessons
                .Include(l => l.Contents)
                .Include(l => l.Section)
                .Where(l => l.Id == lessonId)
                .FirstOrDefaultAsync();

            if(!await _context.UserClasses.AnyAsync(uc => uc.UserId == user.Id && uc.ClassId == lesson.Section.ClassId))
                return Unauthorized();

            if(lesson == null) return NotFound();

            return View(lesson);
        }

        [HttpGet]
        public async Task<IActionResult> GetLesson(int id)
        {
            User user = await _userManager.GetUserAsync(User);

            Lesson? lesson = _context.Lessons
                .Include(l => l.Contents)
                .Include(l => l.Section)
                    .ThenInclude(cs => cs.Class)
                        .ThenInclude(c => c.UserClasses)
                .Where(l => l.Id == id)
                .Where(l => l.Section.Class.UserClasses
                    .Any(uc => uc.UserId == user.Id))
                .FirstOrDefault();

            if (lesson == null)
                return NotFound();

            return View("Lesson", lesson);

        }

        [HttpGet]
        public async Task<IActionResult> Create(int classSectionId)
        {
            User user = await _userManager.GetUserAsync (User);
            ClassSection? classSection = await _context.ClassSections
                .Include(cs => cs.Class)
                    .ThenInclude(c => c.UserClasses)
                .Where(cs => cs.Id == classSectionId)
                .FirstOrDefaultAsync();

            if(classSection == null) return NotFound();

            if(!classSection.Class.UserClasses.Any(uc => uc.UserId == user.Id))
                return Unauthorized();

            var model = new LessonVM() 
            {
                ClassSectionId = classSectionId,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LessonVM lessonVM)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                    }
                }

                return View(lessonVM);
            }

            // Proceed with saving the lesson and contents
            var lesson = new Lesson
            {
                Title = lessonVM.Title,
                ClassSectionId = lessonVM.ClassSectionId,
                OrderIndex = 0, // Set this as needed
                Contents = lessonVM.Contents
                    .Select(c => c.ToLessonContent())
                    .ToList()
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { lessonId = lesson.Id });
        }
    }
}
