using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebApplication1.Data;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Entities.CodingFiles;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using WebApplication1.Models.DTOs;
namespace WebApplication1.Services
{
    public class CodeExecutionService
    {
        private readonly string apiBaseUrl;
        private readonly int apiPort;
        private readonly string apiPath = "/api/codeexecution/execute";
        private readonly ApplicationDbContext _context;

        public CodeExecutionService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;

            // Get configuration values with defaults if not specified
            apiBaseUrl = configuration.GetValue<string>("CodeExecution:BaseUrl") ?? "http://4.232.129.225";
            apiPort = configuration.GetValue<int>("CodeExecution:Port", 5223);
        }

        private string GetFullApiUrl()
        {
            return $"{apiBaseUrl}:{apiPort}{apiPath}";
        }

        public async Task<CodeExecutionResponse> ExecuteFolderAsync(int folderId)
        {
            CodingFolder? folder = await _context.CodingFolders
                .Include(x => x.Files)
                .SingleOrDefaultAsync(x => x.Id == folderId);

            if (folder == null)
                throw new ArgumentException($"Folder with id: {folderId} does not exist");

            // Step 1: Send the files to the API
            using (var client = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                // Step 2: Add the files to the MultipartFormDataContent
                foreach (var file in folder.Files)
                {
                    var fileContent = new ByteArrayContent(file.Data); // Convert the byte array to a StreamContent
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    // Adding the file to the multipart form with the file name and type
                    content.Add(fileContent, "Files", file.Name + file.Type);
                }

                // Step 3: Send the POST request to the API
                try
                {
                    var apiUrl = GetFullApiUrl();
                    var response = await client.PostAsync(apiUrl, content);
                    Console.WriteLine(response);
                    // Step 4: Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        CodeExecutionResponse result = JsonConvert.DeserializeObject<CodeExecutionResponse>(responseString);
                        // Check if the execution was successful
                        if (result.Success)
                        {
                            Console.WriteLine("Execution succeeded!");
                            Console.WriteLine($"Output: {result.Output}");
                        }
                        else
                        {
                            Console.WriteLine("Execution failed!");
                            Console.WriteLine($"Error: {result.Error}");
                        }
                        return result;
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString()); // Error occurred in API execution
                    }
                }
                catch (Exception e)
                {
                    // Handle exception (e.g., network issue, API unavailable)
                    throw e;
                }
            }
        }
    }
}