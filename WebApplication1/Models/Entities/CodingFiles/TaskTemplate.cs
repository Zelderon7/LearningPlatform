using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities.CodingFiles
{
    public class TaskTemplate
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [ForeignKey("Folder")]
        public int FolderId { get; set; }

        //navs
        public CodingFolder Folder { get; set; }
    }
}
