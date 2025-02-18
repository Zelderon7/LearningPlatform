using asp_server.Models.Entity.JoinTables;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApplication1.Models.Entities;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        DbSet<User> Users { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonContent> LessonsContent { get; set; }
        public DbSet<JoinInstitutionRequest> JoinInstitutionRequests { get; set; }
        public DbSet<JoinClassRequest> JoinClassRequests { get; set; }
        public DbSet<ClassSection> ClassSections { get; set; }
        public DbSet<CodingTask> CodingTasks { get; set; }
        public DbSet<TaskSubmission> CodingTaskSubmissions { get; set; }

        // Join Tables
        
        public DbSet<UserInstitution> UserInstitutions { get; set; }
        public DbSet<UserClass> UserClasses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

    }
}
