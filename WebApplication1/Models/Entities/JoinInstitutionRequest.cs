using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    public class JoinInstitutionRequest
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("Institution")]
        public int InstitutionId { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public string? Message { get; set; }


        public Institution Institution { get; set; }
        public User User { get; set; }

        public JoinInstitutionRequest()
        {
            
        }

        public JoinInstitutionRequest(User user, Institution institution)
        {
            InstitutionId = institution.InstitutionId;
            User = user;
            Institution = institution;
            UserId = user.Id;
        }
    }
}
