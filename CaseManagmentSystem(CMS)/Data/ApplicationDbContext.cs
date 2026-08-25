using CaseManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Case> Cases { get; set; }
        public DbSet<WorkflowStage> WorkflowStages { get; set; }
        public DbSet<WorkflowTransition> WorkflowTransitions { get; set; }
        public DbSet<CaseStatusHistory> CaseStatusHistories { get; set; }
        public DbSet<SLAConfiguration> SLAConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasIndex(x => x.EmployeeNumber)
                .IsUnique();

            builder.Entity<Case>()
                .HasIndex(x => x.ExternalCaseId)
                .IsUnique();

            builder.Entity<WorkflowTransition>()
    .HasOne(x => x.FromStage)
    .WithMany()
    .HasForeignKey(x => x.FromStageId)
    .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<WorkflowTransition>()
                .HasOne(x => x.ToStage)
                .WithMany()
                .HasForeignKey(x => x.ToStageId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}