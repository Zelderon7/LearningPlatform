using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    public class ClassSection
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        //Navigation properties

        public Class Class { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
    }
}
