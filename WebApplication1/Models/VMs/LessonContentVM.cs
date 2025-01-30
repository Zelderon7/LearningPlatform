using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models.VMs
{
    public class LessonContentVM
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Type { get; set; }
        [Required]
        public int OrderIndex { get; set; }
        [Required]
        public string Context { get; set; }
        [Required]
        public int LessonId { get; set; }

        internal LessonContent ToLessonContent()
        {
            return new LessonContent
            {
                Id = Id,
                Type = Type,
                OrderIndex = OrderIndex,
                Context = Context,
                LessonId = LessonId
            };
        }
    }
}
