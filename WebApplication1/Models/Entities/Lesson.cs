using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Title { get; set; }

        [Required]
        public int OrderIndex { get; set; }

        [ForeignKey("Section")]
        public int ClassSectionId { get; set; }

        //Navigation properties
        public ClassSection Section { get; set; }
        public ICollection<LessonContent> Contents { get; set; }
    }
}
