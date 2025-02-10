using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    public class CodingTask
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        [Required]
        public string Description { get; set; }

        [ForeignKey("Author")]
        public int AuthorId { get; set; }
        public string Language { get; set; }

        //Navigation properties
        public User Author { get; set; }
        
    }
}
