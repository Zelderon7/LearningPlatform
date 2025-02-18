using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
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

        //Nav props
        public User Author { get; set; }
        public CodingTask Task { get; set; }
    }
}
