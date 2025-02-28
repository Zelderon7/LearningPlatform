
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WebApplication1.Models.DTOs;
using WebApplication1.Models.Entities.CodingFiles;
using WebApplication1.Data;
using CloudinaryDotNet.Core;
using System.Text;
using WebApplication1.Models.JoinTables;
using WebApplication1.Models.Entities;
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
            if (folder.Files == null ||
                folder.Files.Count == 0)
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
                newFile.FolderId = (int)task.FolderId;
                _context.CodingFiles.Add(newFile);
            }

            await _context.SaveChangesAsync();
        }

        public async Task CreateUserTask(int userId, int taskId)
        {
            CodingTask task = await _context.CodingTasks
                .Include(t => t.Folder)
                    .ThenInclude(f => f.Files)
                .SingleOrDefaultAsync(t => t.Id == taskId) ?? throw new ArgumentException(nameof(taskId));

            User user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId) ?? throw new ArgumentException(nameof(userId));

            CodingFolder folder = new CodingFolder()
            {
                Name = user.UserName + "_" + task.Name,
            };

            _context.CodingFolders.Add(folder);
            await _context.SaveChangesAsync();

            UserTask userTask = new UserTask()
            {
                UserId = userId,
                TaskId = taskId,
                FolderId = folder.Id,
            };

            _context.UserTasks.Add(userTask);

            foreach(var file in GetOriginalFilteredFilesByRestriction(task.Folder)
                .Select(f => f.Clone() as CodingFile))
            {
                file.FolderId = folder.Id;
                _context.CodingFiles.Add(file);
            }

            await _context.SaveChangesAsync();
        }

        private List<CodingFile> GetOriginalFilteredFilesByRestriction(CodingFolder folder)
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

        public async Task<List<CodingFileDTO>> GetFilteredFilesByRestriction(int folderId)
        {
            CodingFolder folder = await _context.CodingFolders
                .Include(f => f.Files)
                .SingleOrDefaultAsync(f => f.Id == folderId) ?? throw new ArgumentException(nameof(folderId));

            return GetFilteredFilesByRestriction(folder);
        }

        private List<CodingFileDTO> GetFilteredFilesByRestriction(CodingFolder folder)
        {
            return GetOriginalFilteredFilesByRestriction(folder)
                .Select(f => new CodingFileDTO(f))
                .ToList();
        }

        internal async Task InitializeTaskFolder(CodingTask data)
        {
            CodingFolder folder = new CodingFolder()
            {
                Name = data.Id + "_" + data.Name,
            };

            _context.CodingFolders.Add(folder);
            await _context.SaveChangesAsync();

            data.FolderId = folder.Id;
            _context.CodingTasks.Update(data);
            await _context.SaveChangesAsync();
        }

        internal async Task<List<CodingFileDTO>> OpenUserTask(int taskId, int userId)
        {
            UserTask? ut = await _context.UserTasks
                .Include(u => u.Folder)
                        .ThenInclude(f => f.Files)
                .SingleOrDefaultAsync(u => u.TaskId == taskId && u.UserId == userId);

            if (ut == null)
            {
                await CreateUserTask(userId, taskId);
                ut = await _context.UserTasks
                    .Include(u => u.Folder)
                        .ThenInclude(f => f.Files)
                    .SingleOrDefaultAsync(u => u.TaskId == taskId && u.UserId == userId);
            }

            return GetFilteredFilesByRestriction(ut!.Folder);
        }

        internal async Task ResetDirectory(int taskId, int userId)
        {
            UserTask? ut = await _context.UserTasks
                .Include(u => u.Folder)
                .SingleOrDefaultAsync(u => u.TaskId == taskId && u.UserId == userId);

            _context.UserTasks.Remove(ut);
            _context.SaveChanges();

            await CreateUserTask(userId, taskId);
        }

        internal async Task<CodingFile> CreateFileAsync(int folderId, string fileName)
        {
            string[] splitedName = fileName.Split('.');
            if (splitedName.Length != 2) throw new ArgumentException(nameof(fileName));

            CodingFile file = new CodingFile
            {
                Name = splitedName[0],
                Type = "." + splitedName[1],
                FolderId = folderId,
                Data = Encoding.UTF8.GetBytes("")
            };

            _context.CodingFiles.Add(file);
            await _context.SaveChangesAsync();

            return file;
        }

        public async Task SaveFileAsync(SaveCodeRequest request)
        {
            CodingFile file = await _context.CodingFiles
                .SingleOrDefaultAsync(f => f.Id == request.FileId) ?? throw new ArgumentException(nameof(request));

            file.Data = Encoding.UTF8.GetBytes(request.Content);
            _context.CodingFiles.Update(file);
            await _context.SaveChangesAsync();
        }
    }
}
