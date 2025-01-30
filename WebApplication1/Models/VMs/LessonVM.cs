using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models.VMs
{
    public class LessonVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Required]
        public int OrderIndex { get; set; }

        [Required]
        public int ClassSectionId { get; set; }

        public List<LessonContentVM> Contents { get; set; } = new List<LessonContentVM>();
    }
}
