using CaseManagementSystem.Data.Seed;
using CaseManagementSystem.Data;
using CaseManagementSystem.Models;
using CaseManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Added By Emad For GitHub Ingegration
            // Added bt khaled

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Login / Access Denied paths
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Application Services
            builder.Services.AddScoped<ICaseService, CaseService>();
            builder.Services.AddScoped<ISLAService, SLAService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IWorkflowService, WorkflowService>();
            builder.Services.AddScoped<IExcelImportService, ExcelImportService>();

            var app = builder.Build();

            // Seed initial application data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                await IdentitySeedData.SeedAsync(services);
                await WorkflowSeedData.SeedAsync(services);
                await SLASeedData.SeedAsync(services);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}