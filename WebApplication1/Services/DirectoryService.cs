
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication1.Services
{
    public class DirectoryService
    {
        public string GetTaskDirectory(int taskId)
        {
            return Path.Combine(AppConstants.CodingTasksDir, taskId.ToString());
        }

        public async Task InitializeTaskFolder(string folderDir, string language, string template = "")
        {
            if (Directory.Exists(folderDir))
            {
                throw new Exception("Directory already exists");
            }

            Directory.CreateDirectory(folderDir);

            if (template == "")
                template = language.ToLower();

            string templatePath = Path.Combine(AppConstants.LanguageTemplatesDir, template);

            foreach(string file in Directory.EnumerateFiles(templatePath))
            {
                File.Copy(file, Path.Combine(folderDir, Path.GetFileName(file)), false);
            }
        }

        internal async Task ResetDirectory(string folderDir, string taskPath)
        {
            if (!Directory.Exists(folderDir) || !Directory.Exists(taskPath))
                throw new Exception("Directory not found");

            Directory.Delete(folderDir, true);
            Directory.CreateDirectory(folderDir);

            foreach (var file in Directory.EnumerateFiles(taskPath))
            {
                File.Copy(file, Path.Combine(folderDir, Path.GetFileName(file)));
            }
        }

        public void DeleteTaskDirectory(int id)
        {
            string path = Path.Combine(AppConstants.CodingTasksDir, id.ToString());
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            //Delete any user's copy of this task
            foreach (var directory in Directory.EnumerateDirectories(AppConstants.UserCodingTasksDir))
            {
                if (Directory.Exists(Path.Combine(directory, id.ToString())))
                    Directory.Delete(Path.Combine(directory, id.ToString()), true);
            }
        }

        internal async Task<(string folderDir, string[] filePaths)> OpenTask(int taskId, int userId)
        {
            #region Check task
            string folderDir = Path.Combine(AppConstants.CodingTasksDir, taskId.ToString());
            if (!Directory.Exists(folderDir))
                throw new Exception("Task directory does not exists");

            string[] files = Directory.GetFiles(folderDir);

            if (files.Length == 0)
                throw new Exception("Task directory is empty");

            #endregion

            #region Check user's task

            string path = Path.Combine(AppConstants.UserCodingTasksDir, userId.ToString(), taskId.ToString());

            if (Directory.Exists(path))
            {
                files = GetFilteredFilesByRestriction(path);
                return (path,  files);
            }

            Directory.CreateDirectory(path);

            //Copy each file from the original task to the current user's version
            foreach (string file in files)
            {
                File.Copy(file, Path.Combine(path, Path.GetFileName(file)));
            }

            files = GetFilteredFilesByRestriction(path);
            return (path, files);

            #endregion

        }

        public string[] GetFilteredFilesByRestriction(string folderDir)
        {
            string[] files = Directory.GetFiles(folderDir);
            if (File.Exists(Path.Combine(folderDir, "restrictedFiles.txt")))
            {
                string[] restrictedFiles = File.ReadAllText(Path.Combine(folderDir, "restrictedFiles.txt")).Split([',', ' ']);
                //Excludes any restricted files
                files = files
                .Where(f => (!restrictedFiles
                    .Any(rf => rf == Path.GetFileName(f))) &&
                    Path.GetFileName(f) != "restrictedFiles.txt")
                .ToArray();
            }
            return files;
        }

        public async Task ResetDirectory(int taskId, int userId)
        {
            string taskDir = GetTaskDirectory(taskId);
            string folderDir = Path.Combine(AppConstants.UserCodingTasksDir, userId.ToString(), taskId.ToString());

            await ResetDirectory(folderDir, taskDir);
        }

        public async Task SaveSubmission(int taskId, int userId, int submissionId)
        {
            string folderDir = Path.Combine(AppConstants.UserCodingTasksDir, userId.ToString(), taskId.ToString());
            string submitionDir = Path.Combine(AppConstants.TaskSubmissionsDir, taskId.ToString(), submissionId.ToString());
            if (!Directory.Exists(folderDir))
                throw new NullReferenceException();

            if (Directory.Exists(submitionDir))
                Directory.Delete(submitionDir, true);

            Directory.CreateDirectory(submitionDir);

            foreach(string file in Directory.EnumerateFiles(folderDir))
            {
                File.Copy(file, Path.Combine(submitionDir, Path.GetFileName(file)), true);
            }
        }
    }
}
