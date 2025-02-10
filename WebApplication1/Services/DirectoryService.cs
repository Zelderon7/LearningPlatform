
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication1.Services
{
    public class DirectoryService
    {
        internal async Task InitializeTaskFolder(string folderDir, string language)
        {
            if (Directory.Exists(folderDir))
            {
                throw new Exception("Directory already exists");
            }

            Directory.CreateDirectory(folderDir);
            string templatePath = Path.Combine(AppConstants.LanguageTemplatesDir, language.ToLower());

            foreach(string file in Directory.EnumerateFiles(templatePath))
            {
                File.Copy(file, Path.Combine(folderDir, Path.GetFileName(file)), false);
            }
        }
    }
}
