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
using WebApplication1.Models.Entities.CodingFiles;

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


        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public IActionResult GetTemplate()
        {
            if (Request.Cookies.TryGetValue("CodingIDETemplateVMModel", out string data))
            {
                var model = JsonConvert.DeserializeObject<CodingIDETemplateVM>(data);
                if (model.Output.Contains("FAILED") || model.Output.Length == 0)
                {
                    ViewData["IsValidSolution"] = false;
                }
                else
                {
                    ViewData["IsValidSolution"] = true;
                }
                return View("IDETemplate", model);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> SaveCode([FromBody] SaveCodeRequest request)
        {
            try
            {
                ViewData["IsValidSolution"] = false;

                await _directoryService.SaveFileAsync(request);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> RunCode(CodingIDEVM data)
        {

            throw new NotImplementedException();
            (string output, string error) result = ("", "");

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
        
        [Authorize]
        public async Task<IActionResult> ResetCode(int taskId)
        {
            throw new NotImplementedException();
            User user = await _userManager.GetUserAsync(User);
            await _directoryService.ResetDirectory(taskId, user.Id);
            return RedirectToAction("OpenChallenge", "CodingChallenges", new { id = taskId });
        }
        
        [Authorize(Roles = "TEACHER,ADMIN")]
        public async Task<IActionResult> NewFile(string fileName, int taskId)
        {
            CodingTask? task = await _context.CodingTasks
                .SingleOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound();

            try
            {
                await _directoryService.CreateFileAsync((int)task.FolderId, fileName);
            }
            catch (Exception ex)
            {

            }
            

            return RedirectToAction("OpenChallenge", "CodingChallenges", new { id = taskId });
        }
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> TemplateNewFile(string fileName, int templateId)
        {
            TaskTemplate? template = await _context.TaskTemplates
                .SingleOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return NotFound();

            try
            {
                await _directoryService.CreateFileAsync((int)template.FolderId, fileName);
            }
            catch (Exception ex)
            {

            }


            return RedirectToAction("OpenTemplate", "CodingChallenges", new { id = templateId });
        }
        public async Task<IActionResult> SubmitTask(int taskId)
        {

            throw new NotImplementedException();
            User user = await _userManager.GetUserAsync(User);

            TaskSubmission submission = new TaskSubmission();
            submission.TaskId = taskId;
            submission.AuthorId = user.Id;

            _context.CodingTaskSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            //await _directoryService.SaveSubmission(taskId, user.Id, submission.Id);

            return RedirectToAction("OpenChallenge", "CodingChallenges", new { id = taskId });
        }

    }
}
