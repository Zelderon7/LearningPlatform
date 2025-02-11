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

namespace WebApplication1.Controllers
{
    [Authorize]
    public class CodingController : Controller
    {
        PodmanService _podmanService;
        UserManager<User> _userManager;
        public CodingController(PodmanService podmanService, UserManager<User> userManager)
        {
            _podmanService = podmanService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData["CodingIDEVMModel"] != null)
            {
                var model = JsonConvert.DeserializeObject<CodingIDEVM>((string)TempData["CodingIDEVMModel"]);
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
            TempData["CodingIDEVMModel"] = JsonConvert.SerializeObject(data);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ResetCode(int taskId)
        {
            throw new NotImplementedException();
            User user = await _userManager.GetUserAsync(User);
        }

    }
}
