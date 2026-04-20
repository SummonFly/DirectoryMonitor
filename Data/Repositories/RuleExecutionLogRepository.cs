using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class RuleExecutionLogRepository : IRuleExecutionLogRepository
    {
        private readonly AppDbContext _context;

        public RuleExecutionLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RuleExecutionLogEntry entry)
        {
            entry.Timestamp = DateTime.UtcNow;
            await _context.RuleExecutionLogs.AddAsync(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RuleExecutionLogEntry>> GetByRuleIdAsync(int ruleId, int limit = 100)
        {
            return await _context.RuleExecutionLogs
                .Where(l => l.RuleId == ruleId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<RuleExecutionLogEntry>> GetRecentAsync(int limit = 100)
        {
            return await _context.RuleExecutionLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> DeleteOldAsync(DateTime before)
        {
            var oldEntries = _context.RuleExecutionLogs.Where(l => l.Timestamp < before);
            var count = await oldEntries.CountAsync();
            _context.RuleExecutionLogs.RemoveRange(oldEntries);
            await _context.SaveChangesAsync();
            return count;
        }
    }
}
