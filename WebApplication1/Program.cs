using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Areas.Identity;
using WebApplication1.Data;
using WebApplication1.Models.Entities;
using WebApplication1.Services;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string connectionString;
#if RELEASE
            // Add services to the container.
            connectionString = builder.Configuration.GetConnectionString("ReleaseConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
#else
            connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
#endif
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddScoped<RoleManager<Role>>();
            builder.Services.AddScoped<UserManager<User>>();

            builder.Services.AddScoped<IRoleStore<Role>, RoleStore<Role, ApplicationDbContext, int>>();
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            });
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomUserClaimsPrincipalFactory>();

            builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<Role>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<PodmanService>();
            builder.Services.AddScoped<DirectoryService>();
            builder.Services.AddScoped<CodeExecutionService>();



            var account = new Account(
                builder.Configuration.GetSection("Cloudinary")["CloudName"],
                builder.Configuration.GetSection("Cloudinary")["ApiKey"],
                builder.Configuration.GetSection("Cloudinary")["ApiSecret"]
                );

            var cloudinary = new Cloudinary(account);
            builder.Services.AddSingleton(cloudinary);


            builder.Services.AddControllersWithViews();

            
            Task.Factory.StartNew(() => InitializeRoles(builder));
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                
                // Add detailed error page for troubleshooting the Institution controller issue
                app.UseDeveloperExceptionPage();
                
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();
            

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }

        private static async Task InitializeRoles(WebApplicationBuilder builder)
        {
            using(var serviceProvider = builder.Services.BuildServiceProvider()){
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                    string[] roleNames = { "ADMIN", "USER", "STUDENT", "TEACHER" };
                    foreach (var roleName in roleNames)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            var result = await roleManager.CreateAsync(new Role { Name = roleName });
                            if (result.Succeeded)
                            {
                                logger.LogInformation($"Role '{roleName}' created successfully.");
                            }
                            else
                            {
                                logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                            logger.LogInformation($"Role '{roleName}' already exists.");
                        }
                    }
                }
            }
            
        }
    }
}
