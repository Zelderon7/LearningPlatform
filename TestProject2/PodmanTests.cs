using WebApplication1.Models.Entities;
using WebApplication1.Services;

namespace TestProject2
{
    public class PodmanTests
    {
        PodmanService _service;

        [SetUp]
        public void Setup()
        {
            _service = new PodmanService();
        }

        [Test]
        public async Task HelloWorld()
        {
            string code = "print('Hello, world!')";
            // Specify the name of the temp file with .py extension
            string fileName = "main.py";

            // Get the path to the system temp directory
            string tempDirectory = Path.GetTempPath();

            // Combine the temp directory with the desired file name
            string tempFilePath = Path.Combine(tempDirectory, fileName);

            File.WriteAllText(tempFilePath, code);

            try
            {
                CodingTask task = new CodingTask
                {
                    Id = 0,
                    Language = "python"
                };
                (string output, string error) result = await _service.RunFolderAsync(task, tempDirectory);

                Assert.That(result.output, Is.EqualTo("Hello, world!\n")
                    .Or.EqualTo("Hello, world!\n"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
                throw;
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }
    }
}