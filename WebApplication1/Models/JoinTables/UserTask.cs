using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Entities;
using WebApplication1.Models.Entities.CodingFiles;

namespace WebApplication1.Models.JoinTables
{
    public class UserTask
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        [ForeignKey("Folder")]
        public int FolderId { get; set; }

        //Navs

        public User User { get; set; }
        public CodingTask Task { get; set; }
        public CodingFolder Folder { get; set; }


        
    }
}
