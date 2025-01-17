namespace asp_server.Models.Entity.JoinTables
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using WebApplication1.Models.Entities;

    public class UserClass
    {
        [Key]
        public int UserClassId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public Class Class { get; set; }
    }

}
