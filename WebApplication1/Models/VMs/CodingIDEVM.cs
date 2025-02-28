using WebApplication1.Models.Entities.CodingFiles;

namespace WebApplication1.Models.VMs
{
    public class CodingIDEVM
    {
        public CodingTask Task { get; set; }
        public string[] FilePaths { get; set; }
        public string FolderDir { get; set; }
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
    }
}
