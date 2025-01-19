namespace WebApplication1.Models.Entities
{
    using asp_server.Models.Entity.JoinTables;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics.CodeAnalysis;

    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [AllowNull]
        [MaxLength(250)]
        public string? ImageUrl { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("Institution")]
        public int InstitutionId { get; set; }

        public Class()
        {
            CreatedAt = DateTime.Now;
        }

        // Navigation Properties
        public Institution Institution { get; set; }
        public ICollection<UserClass> UserClasses { get; set; }
        public ICollection<ClassSection> ClassSections { get; set; }
    }

}
