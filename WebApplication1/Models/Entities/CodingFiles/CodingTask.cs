using CloudinaryDotNet.Actions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace WebApplication1.Models.Entities.CodingFiles
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
        [AllowNull, Range(1, 100)]
        public int? MaxPoints { get; set; }

        //Foreign keys
        [ForeignKey("Class"), AllowNull]
        public int? ClassId { get; set; }

        [ForeignKey("Author")]
        public int AuthorId { get; set; }

        [ForeignKey("Folder"), AllowNull]
        public int? FolderId { get; set; }

        //Navigation properties
        public Class Class { get; set; }

        public CodingFolder Folder { get; set; }
        public string Language { get; set; }

        //Navigation properties
        public User Author { get; set; }

    }
}
