namespace asp_server.Models.Entity.JoinTables
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using WebApplication1.Models.Entities;

    public class UserInstitution
    {
        [Key]
        public int UserInstitutionId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Institution")]
        public int InstitutionId { get; set; }

        [Required]
        public string Role { get; set; } // Role within the institution: Admin, Teacher, Student

        // Navigation Properties
        public User User { get; set; }
        public Institution Institution { get; set; }
    }

}
