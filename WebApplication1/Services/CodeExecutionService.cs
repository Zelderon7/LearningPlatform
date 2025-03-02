using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebApplication1.Data;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Entities.CodingFiles;
using System.Net.Http.Headers;


namespace WebApplication1.Services
{
    public class CodeExecutionService
    {
        private readonly string apiUrl = "http://4.232.129.225:5214/api/codeexecution/execute";
        ApplicationDbContext _context;

        public CodeExecutionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExecuteFolderAsync(int folderId)
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
                    var response = await client.PostAsync(apiUrl, content);

                    Console.WriteLine(response);

                    // Step 4: Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        return true; // Execution was successful
                    }
                    else
                    {
                        return false; // Error occurred in API execution
                    }
                }
                catch (Exception)
                {
                    // Handle exception (e.g., network issue, API unavailable)
                    return false;
                }
            }
        }
    }
}
