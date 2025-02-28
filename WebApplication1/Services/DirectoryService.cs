
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WebApplication1.Models.DTOs;
using WebApplication1.Models.Entities.CodingFiles;
using WebApplication1.Data;
using CloudinaryDotNet.Core;
using System.Text;
namespace WebApplication1.Services
{
    public class DirectoryService
    {
        ApplicationDbContext _context;

        public DirectoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        private List<CodingFile> GetOriginalFilesFromFolder(CodingFolder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            if (folder.Files == null) throw new NullReferenceException(nameof(folder.Files));
            if (folder.Files.Count == 0)
                return new List<CodingFile>();

            return folder.Files
                .ToList();
        }

        public List<CodingFileDTO> GetFilesFromFolder(CodingFolder folder)
        {
            return GetOriginalFilesFromFolder(folder)
                .Select(x => new CodingFileDTO(x))
                .ToList();
        }

        public async Task CopyTemplateToTask(int templateId, int taskId)
        {
            TaskTemplate template = await _context.TaskTemplates
                .Include(t => t.Folder)
                    .ThenInclude(f => f.Files)
                .SingleOrDefaultAsync(t => t.Id == templateId) ?? throw new ArgumentException(nameof(templateId));

            CodingTask task = await _context.CodingTasks
                .Include(t => t.Folder)
                    .ThenInclude(f => f.Files)
                .SingleOrDefaultAsync(t => t.Id == taskId) ?? throw new ArgumentException(nameof(taskId));


            foreach (var file in template.Folder.Files)
            {
                var newFile = (CodingFile)file.Clone();
                newFile.FolderId = task.FolderId;
                _context.Files.Add(newFile);
            }

            await _context.SaveChangesAsync();
        }

        public async Task CreateUserTask(int userId, int taskId)
        {
            CodingTask task = await _context.CodingTasks
                .Include(t => t.Folder)
                    .ThenInclude(f => f.Files)
                .SingleOrDefaultAsync(t => t.Id == taskId) ?? throw new ArgumentException(nameof(taskId));

            if(await _context.Users.SingleOrDefaultAsync(u => u.Id == userId) == null) throw new ArgumentException(nameof(userId));

            //UserTask
        }

        public List<CodingFile> GetFilteredFilesByRestriction(CodingFolder folder)
        {
            List<CodingFile> files = GetOriginalFilesFromFolder(folder);

            CodingFile? restricted = files.SingleOrDefault(f => f.Name == "restrictions" && f.Type == ".txt");

            if (restricted == null)
                return files;

            string restrictedData = Encoding.UTF8.GetString(restricted.Data);
            string[] restrictedNames = restrictedData.Split([',', ' ']);

            List<CodingFile> remainingFiles = files
                .Where(f => !restrictedNames.Contains(f.Name) &&
                    !restrictedNames.Contains(f.Name + f.Type) && 
                    f.Name + f.Type != "restricted.txt")
                .ToList();

            return remainingFiles;
        }

        
    }
}
