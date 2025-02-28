using WebApplication1.Models.DTOs;
using WebApplication1.Models.Entities.CodingFiles;

namespace WebApplication1.Models.VMs
{
    public class CodingIDEVM
    {
        public CodingTask Task { get; set; }
        public List<CodingFileDTO> Files { get; set; }
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
    }
}
