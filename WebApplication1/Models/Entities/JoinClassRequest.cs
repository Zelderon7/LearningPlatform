using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Entities
{
    public class JoinClassRequest
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("Class")]
        public int ClassId { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public string? Message { get; set; }


        public Class Class { get; set; }
        public User User { get; set; }
    }
}
