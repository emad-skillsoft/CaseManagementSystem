using CaseManagementSystem.Data;
using CaseManagementSystem.Enums;
using CaseManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem.Data.Seed
{
    public static class SLASeedData
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();

            var defaults = new Dictionary<CasePriority, int>
            {
                [CasePriority.P1] = 8,
                [CasePriority.P2] = 24,
                [CasePriority.P3] = 48,
                [CasePriority.P4] = 72
            };

            foreach (var item in defaults)
            {
                var existing = await db.SLAConfigurations
                    .FirstOrDefaultAsync(x => x.Priority == item.Key);

                if (existing == null)
                {
                    db.SLAConfigurations.Add(
                        new SLAConfiguration
                        {
                            Priority = item.Key,
                            AllowedHours = item.Value
                        });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}