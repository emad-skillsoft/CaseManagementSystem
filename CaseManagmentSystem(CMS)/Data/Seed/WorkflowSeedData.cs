using CaseManagementSystem.Constants;
using CaseManagementSystem.Models;
using CaseManagmentSystem_CMS_.Data;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem.Data.Seed
{
    public static class WorkflowSeedData
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();

            var stages = new (string Name, string DisplayName, int Order)[]
            {
                (WorkflowStageNames.Assigned, "Assigned", 1),
                (WorkflowStageNames.InProgress, "In Progress", 2),
                (WorkflowStageNames.Challenge, "Challenge", 3),
                (WorkflowStageNames.CompletionPending, "Completion Pending", 4),
                (WorkflowStageNames.Completed, "Completed", 5)
            };

            foreach (var stage in stages)
            {
                var exists = await db.WorkflowStages
                    .AnyAsync(x => x.Name == stage.Name);

                if (!exists)
                {
                    db.WorkflowStages.Add(new WorkflowStage
                    {
                        Name = stage.Name,
                        DisplayName = stage.DisplayName,
                        Order = stage.Order
                    });
                }
            }

            await db.SaveChangesAsync();

            var stageMap = await db.WorkflowStages
                .ToDictionaryAsync(x => x.Name, x => x.Id);

            var transitions = new (string From, string To)[]
            {
                (WorkflowStageNames.Assigned, WorkflowStageNames.InProgress),
                (WorkflowStageNames.InProgress, WorkflowStageNames.Challenge),
                (WorkflowStageNames.Challenge, WorkflowStageNames.InProgress),
                (WorkflowStageNames.InProgress, WorkflowStageNames.CompletionPending),
                (WorkflowStageNames.CompletionPending, WorkflowStageNames.InProgress),
                (WorkflowStageNames.CompletionPending, WorkflowStageNames.Completed)
            };

            foreach (var transition in transitions)
            {
                var fromId = stageMap[transition.From];
                var toId = stageMap[transition.To];

                var exists = await db.WorkflowTransitions.AnyAsync(x =>
                    x.FromStageId == fromId &&
                    x.ToStageId == toId);

                if (!exists)
                {
                    db.WorkflowTransitions.Add(new WorkflowTransition
                    {
                        FromStageId = fromId,
                        ToStageId = toId
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
