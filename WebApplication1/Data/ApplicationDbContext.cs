using asp_server.Models.Entity.JoinTables;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApplication1.Models.Entities;
using WebApplication1.Models.Entities.CodingFiles;
using WebApplication1.Models.JoinTables;

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
        public DbSet<CodingFile> CodingFiles { get; set; }
        public DbSet<CodingFolder> CodingFolders { get; set; }

        // Join Tables
        
        public DbSet<UserInstitution> UserInstitutions { get; set; }
        public DbSet<UserClass> UserClasses { get; set; }
        public DbSet<TaskTemplate> TaskTemplates { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserTask>()
                .HasOne(t => t.Task)
                .WithMany()
                .HasForeignKey(t => t.TaskId) // Correct way to define FK
                .OnDelete(DeleteBehavior.Restrict); // Use Restrict, ClientSetNull, or another valid option

            builder.Entity<TaskSubmission>()
                .HasOne(t => t.Folder)
                .WithOne()
                .HasForeignKey<TaskSubmission>(t => t.FolderId)
                .OnDelete(DeleteBehavior.Restrict);

        }


    }
}
