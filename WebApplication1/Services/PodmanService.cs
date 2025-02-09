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
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                throw new ArgumentException("Invalid folder path.", nameof(folderPath));
            }

            string containerName = $"task-[{task.Id}]";
            string imageName = GetImageFromLanguage(task.Language);
            string containerCommand = GetStartFileFromLanguage(task.Language);
            string limitations = "--memory=512m --cpus=1";

            // Create the podman container and map the folder
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _podmanExecutable,
                Arguments = $"run --rm {limitations} -v {folderPath}:/app -w /app {containerName} {imageName} {containerCommand}",
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

        private string GetStartFileFromLanguage(string language)
        {
            switch (language.ToLower())
            {
                case "python":
                    return "main.py";

                default:
                    throw new NotSupportedException($"{language} is not supported yet");
            }
        }

        private string GetImageFromLanguage(string language)
        {
            switch (language.ToLower())
            {
                case "python":
                    return "python:3.11";

                default:
                    throw new NotSupportedException($"{language} is not supported yet");
            }
        }
    }
}
