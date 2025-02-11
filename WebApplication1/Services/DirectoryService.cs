
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication1.Services
{
    public class DirectoryService
    {
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

        public async Task ResetDirectory(string folderDir, string taskPath)
        {
            if (!Directory.Exists(folderDir) || !Directory.Exists(taskPath))
                throw new Exception("Directory not found");

            Directory.Delete(folderDir);
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
        }
    }
}
