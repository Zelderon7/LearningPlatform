using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Models.DTOs;
using WebApplication1.Models.Entities.CodingFiles;

namespace WebApplication1.Services
{
    public class PodmanService
    {
        private readonly string _podmanExecutable;
        ApplicationDbContext _context;

        public PodmanService(ApplicationDbContext context)
        {
            // Default Podman executable path
            _podmanExecutable = "podman"; // If podman is in your system path. Adjust if needed.
            _context = context;
        }

        public async Task<(string output, string error)> RunFolderAsync(string language, string folderPath)
        {

            string[] fileNames = Directory.GetFiles(folderPath)
                .Select(x => Path.GetFileName(x))
                .ToArray();
            
            string starterFile = GetStartFileFromLanguage(language, fileNames);
            string imageName = GetImageFromLanguage(language, starterFile);
            
            
            if (string.IsNullOrEmpty(starterFile))
            {
                throw new FileNotFoundException("No starter file found");
            }


            // Create the podman container and map the folder
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _podmanExecutable,
                Arguments = $"run --rm -v {folderPath}:/app -w /app {imageName} {starterFile}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start Podman process.");
                    }

                    // Read the output and error streams
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    // Wait for the process to finish
                    await process.WaitForExitAsync();

                    return (output, error);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while running the Podman container: {ex.Message}", ex);
            }
        }

        private string GetStartFileFromLanguage(string language, string[] fileNames)
        {
            if (fileNames == null || fileNames.Length == 0)
                return string.Empty;

            switch (language.ToLower())
            {
                case "python":
                    return fileNames.SingleOrDefault(x => x == "script.py" || x == "tests.py") ?? string.Empty;

                default:
                    throw new NotSupportedException($"{language} is not supported yet");
            }
        }

        private string GetImageFromLanguage(string language, string starter)
        {
            switch (language.ToLower())
            {
                case "python":
                    return $"python-image {(starter == "tests.py" ? "pytest" : "python")}";

                default:
                    throw new NotSupportedException($"{language} is not supported yet");
            }
        }

        internal async Task<CodeExecutionResponse> ExecuteFolderAsync(int folderId)
        {
            CodingFolder? folder = await _context.CodingFolders
                .Include(f => f.Files)
                .SingleOrDefaultAsync(x => x.Id == folderId);

            if (folder == null)
                throw new Exception("Folder not found");

            CodeExecutionResponse response = new CodeExecutionResponse()
            {
                Output = "",
                Error = "Internal server error! :(",
                Success = false,
            };

            string folderPath = SaveInTempFolder(folder);
            try
            {
                var result = await RunFolderAsync("python", folderPath);
                response.Output = result.output;
                response.Error = result.error;
                response.Success = result.output.Length > 0 && result.error.Length == 0;
            }
            catch (Exception ex)
            {
                response.Error = "(500) " + ex.Message;
            }
            finally
            {
                Directory.Delete(folderPath, true);
            }

            return response;
        }

        private string SaveInTempFolder(CodingFolder folder)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            foreach (var file in folder.Files)
            {
                // Create a unique file path
                string filePath = Path.Combine(tempPath, file.Name + file.Type);

                // Write the byte array to a file
                File.WriteAllBytes(filePath, file.Data);
            }

            return tempPath;

        }
    }
}
