using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class WatchedPathRuleRepository : IWatchedPathRuleRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public WatchedPathRuleRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Rule>> GetRulesForWatchedPathAsync(int watchedPathId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.WatchedPathRules
                .Where(wpr => wpr.WatchedPathId == watchedPathId)
                .Include(wpr => wpr.Rule)
                .Select(wpr => wpr.Rule)
                .ToListAsync();
        }

        public async Task<List<int>> GetRuleIdsForWatchedPathAsync(int watchedPathId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.WatchedPathRules
                .Where(wpr => wpr.WatchedPathId == watchedPathId)
                .Select(wpr => wpr.RuleId)
                .ToListAsync();
        }

        public async Task UpdateRulesForWatchedPathAsync(int watchedPathId, List<int> selectedRuleIds)
        {
            using var context = _contextFactory.CreateDbContext();

            // Remove old associations
            var oldAssociations = await context.WatchedPathRules
                .Where(wpr => wpr.WatchedPathId == watchedPathId)
                .ToListAsync();
            context.WatchedPathRules.RemoveRange(oldAssociations);

            // Add new associations
            foreach (var ruleId in selectedRuleIds)
            {
                context.WatchedPathRules.Add(new WatchedPathRule
                {
                    WatchedPathId = watchedPathId,
                    RuleId = ruleId
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
