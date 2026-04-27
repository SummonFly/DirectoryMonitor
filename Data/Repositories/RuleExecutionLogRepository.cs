using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class RuleExecutionLogRepository : IRuleExecutionLogRepository
    {
        IDbContextFactory<AppDbContext> _contextFactory;

        public RuleExecutionLogRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(RuleExecutionLogEntry entry)
        {
            using var context = _contextFactory.CreateDbContext();
            entry.Timestamp = DateTime.UtcNow;
            await context.RuleExecutionLogs.AddAsync(entry);
            await context.SaveChangesAsync();
        }

        public async Task<List<RuleExecutionLogEntry>> GetByRuleIdAsync(int ruleId, int limit = 100)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.RuleExecutionLogs
                .Where(l => l.RuleId == ruleId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<RuleExecutionLogEntry>> GetRecentAsync(int limit = 100)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.RuleExecutionLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> DeleteOldAsync(DateTime before)
        {
            using var context = _contextFactory.CreateDbContext();
            var oldEntries = context.RuleExecutionLogs.Where(l => l.Timestamp < before);
            var count = await oldEntries.CountAsync();
            context.RuleExecutionLogs.RemoveRange(oldEntries);
            await context.SaveChangesAsync();
            return count;
        }
    }
}
