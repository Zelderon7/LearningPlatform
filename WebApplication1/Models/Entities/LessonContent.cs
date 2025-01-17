using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    public enum LessonContentTypes
    {
        Text,
        Paragraph,
        Bulletpoints,
        Title,
        SubTitle,
        Code,
        image
    }
    public class LessonContent
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Type { get; set; }
        [Required]
        public int OrderIndex { get; set; }
        [Required]
        public string Context { get; set; }

        [ForeignKey("Lesson")]
        public int LessonId { get; set; }

        //Navigation property
        public Lesson Lesson { get; set; }
    }
}
