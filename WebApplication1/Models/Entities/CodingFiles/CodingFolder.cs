using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WebApplication1.Models.Entities.CodingFiles
{
    public class CodingFolder
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        //Nav props

        public ICollection<CodingFile> Files { get; set; }
    }
}
