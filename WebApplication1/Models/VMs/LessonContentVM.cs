using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.VMs
{
    public class LessonContentVM
    {
        public int Id { get; set; }
        [Required]
        public int LessonId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public int OrderIndex { get; set; }

        [Required]
        public string Context { get; set; }
    }
}
