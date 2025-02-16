using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using WebApplication1.Models.Entities;
using WebApplication1.Models.VMs;
using System;
using System.IO;
using WebApplication1.Services;
using WebApplication1.Models.DTOs;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class CodingController : Controller
    {
        PodmanService _podmanService;
        UserManager<User> _userManager;
        ApplicationDbContext _context;
        DirectoryService _directoryService;
        public CodingController(PodmanService podmanService, UserManager<User> userManager, ApplicationDbContext context, DirectoryService directoryService)
        {
            _podmanService = podmanService;
            _userManager = userManager;
            _context = context;
            _directoryService = directoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (Request.Cookies.TryGetValue("CodingIDEVMModel", out string data))
            {
                var model = JsonConvert.DeserializeObject<CodingIDEVM>(data);
                if (model.Output.Contains("FAILED") || model.Output.Length == 0)
                {
                    ViewData["IsValidSolution"] = false;
                }
                else
                {
                    ViewData["IsValidSolution"] = true;
                }
                return View("IDE", model);
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult Test()
        {
            CodingTask codingTask = new CodingTask()
            {
                Id = 1,
                Name = "Hello, world in python",
                Description = "Write \"Hello, world\" in python",
                Language = "Python"
            };

            string tempFolderName = Guid.NewGuid().ToString();
            // Get the system's temp directory
            string tempFolderPath = Path.Combine(Path.GetTempPath(), tempFolderName);

            Directory.CreateDirectory(tempFolderPath);
            string code = "print('Hello, world!')";
            string filePath = Path.Combine(tempFolderPath, "main.py");

            System.IO.File.WriteAllText(filePath, code);

            CodingIDEVM model = new CodingIDEVM
            {
                Task = codingTask,
                FolderDir = tempFolderPath,
                FilePaths = [filePath]
            };

            return View("IDE", model);
        }

        [HttpPost]
        public IActionResult SaveCode([FromBody] SaveCodeRequest request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.FilePath) && !string.IsNullOrEmpty(request.Content))
                {
                    System.IO.File.WriteAllText(request.FilePath, request.Content);
                    ViewData["IsValidSolution"] = false;
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Invalid file path or content." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RunCode(CodingIDEVM data)
        {
            (string output, string error) result = await _podmanService.RunFolderAsync(data.Task, data.FolderDir);

            data.Output = result.output;
            data.Error = result.error;

            // Storing the complex model in TempData
            var options = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(2),
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("CodingIDEVMModel", JsonConvert.SerializeObject(data), options);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ResetCode(int taskId)
        {
            throw new NotImplementedException();
            User user = await _userManager.GetUserAsync(User);
        }

        [Authorize(Roles = "TEACHER,ADMIN")]
        public async Task<IActionResult> NewFile(string fileName, int taskId)
        {
            CodingTask? task = await _context.CodingTasks.SingleOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return NotFound();

            string folderDir = _directoryService.GetTaskDirectory(taskId);

            if (string.IsNullOrEmpty(fileName) ||
                !(fileName.Where(c => c == '.').Count() == 1 &&
                    AppConstants.AllowedFileExtentions.Contains("." + fileName.Split('.')[^1])))
                return BadRequest();

            if (Directory.EnumerateFiles(folderDir).Contains(fileName))
                return Conflict();

            using (FileStream _ = System.IO.File.Create(Path.Combine(folderDir, fileName))) { }

            return RedirectToAction("OpenChallenge", "CodingChallenges", new { id = taskId });
        }
    }
}
