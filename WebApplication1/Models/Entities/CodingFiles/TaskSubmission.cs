using CloudinaryDotNet.Actions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace WebApplication1.Models.Entities.CodingFiles
{
    public class TaskSubmission
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime CreatedAt { get; internal set; }

        [ForeignKey("Author")]
        public int AuthorId { get; set; }
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        [ForeignKey("Folder"), AllowNull]
        public int? FolderId { get; set; }

        //Nav props
        public User Author { get; set; }
        public CodingTask Task { get; set; }
        public CodingFolder Folder { get; set; }

        public TaskSubmission()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
