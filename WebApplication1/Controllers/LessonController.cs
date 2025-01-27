using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models.Entities;

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

            Lesson lesson = new()
            {
                Title = "",
                ClassSectionId = classSectionId,
            };

            return View(lesson);
        }

    }
}
