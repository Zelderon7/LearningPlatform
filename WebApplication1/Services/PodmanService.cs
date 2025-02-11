using System.Diagnostics;
using WebApplication1.Models.Entities;

namespace WebApplication1.Services
{
    public class PodmanService
    {
        private readonly string _podmanExecutable;

        public PodmanService()
        {
            // Default Podman executable path
            _podmanExecutable = "podman"; // If podman is in your system path. Adjust if needed.
        }

        public async Task<(string output, string error)> RunFolderAsync(CodingTask task, string folderPath)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                throw new ArgumentException("Invalid folder path.", nameof(folderPath));
            }

            string[] fileNames = Directory.GetFiles(folderPath)
                .Select(x => Path.GetFileName(x))
                .ToArray();

            string imageName = GetImageFromLanguage(task.Language);
            string starterFile = GetStartFileFromLanguage(task.Language, fileNames);

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
                    return fileNames.SingleOrDefault(x => x == "main.py" || x == "tests.py") ?? string.Empty;

                default:
                    throw new NotSupportedException($"{language} is not supported yet");
            }
        }

        private string GetImageFromLanguage(string language)
        {
            switch (language.ToLower())
            {
                case "python":
                    return "python-image python";

                default:
                    throw new NotSupportedException($"{language} is not supported yet");
            }
        }
    }
}
