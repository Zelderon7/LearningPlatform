using System.ComponentModel.DataAnnotations;

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

        public List<LessonContentVM> Contents { get; set; } = new List<LessonContentVM>();
    }
}
