namespace WebApplication1.Models.Entities
{
    using asp_server.Models.Entity.JoinTables;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Security.Claims;

    public class Institution
    {
        [Key]
        public int InstitutionId { get; set; }

        [MaxLength(6)]
        public string Code { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string ContactEmail { get; set; }
        [Required]
        public bool IsPublic { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public Institution()
        {
            CreatedAt = DateTime.UtcNow;
            Code = string.Join("", InstitutionId.GetHashCode().ToString().Reverse().Take(6));
        }

        // Navigation Properties
        public ICollection<Class> Classes { get; set; }
        public ICollection<UserInstitution> UserInstitutions { get; set; }
        public ICollection<JoinInstitutionRequest> JoinInstitutionRequests { get; set; }
    }

}
