using WebApplication1.Models.Entities.CodingFiles;
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
            string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(tempDirectory);

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
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public async Task MultiFile()
        {
            string code1 = "def sayHi():\n    print('Hello, world!')";
            string code2 = "from helper import sayHi \nsayHi()";
            // Specify the name of the temp file with .py extension
            string fileName1 = "helper.py";
            string fileName2 = "main.py";

            // Get the path to the system temp directory
            string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(tempDirectory);

            // Combine the temp directory with the desired file name
            string tempFilePath1 = Path.Combine(tempDirectory, fileName1);
            // Combine the temp directory with the desired file name
            string tempFilePath2 = Path.Combine(tempDirectory, fileName2);

            File.WriteAllText(tempFilePath1, code1);
            File.WriteAllText(tempFilePath2, code2);

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
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public async Task NumpyTest()
        {
            string code = """
import numpy as np
arr = np.array([1, 2, 3, 4])
print(np.sum(arr))
""";

            // Specify the name of the temp file with .py extension
            string fileName = "main.py";

            // Get the path to the system temp directory
            string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);

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

                Assert.That(result.output, Is.EqualTo("10\n"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
                throw;
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

    }
}