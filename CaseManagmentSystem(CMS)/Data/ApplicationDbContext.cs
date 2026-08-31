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

        // Session 3
        public DbSet<CaseUpdate> CaseUpdates { get; set; }

        // Session 4
        public DbSet<CaseChallenge> CaseChallenges => Set<CaseChallenge>();

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

            // Session 3 - CaseUpdate relationships
            builder.Entity<CaseUpdate>()
                .HasOne(x => x.Case)
                .WithMany(x => x.Updates)
                .HasForeignKey(x => x.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CaseUpdate>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Session 4 - CaseChallenge relationships
            builder.Entity<CaseChallenge>()
                .HasOne(x => x.Case)
                .WithMany(x => x.Challenges)
                .HasForeignKey(x => x.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CaseChallenge>()
                .HasOne(x => x.StartedByUser)
                .WithMany()
                .HasForeignKey(x => x.StartedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CaseChallenge>()
                .HasOne(x => x.ResolvedByUser)
                .WithMany()
                .HasForeignKey(x => x.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}