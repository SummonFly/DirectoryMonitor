using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.Data.Repositories
{
    public interface IRuleExecutionLogRepository
    {
        Task AddAsync(RuleExecutionLogEntry entry);
        Task<List<RuleExecutionLogEntry>> GetByRuleIdAsync(int ruleId, int limit = 100);
        Task<List<RuleExecutionLogEntry>> GetRecentAsync(int limit = 100);
        Task<int> DeleteOldAsync(DateTime before);
    }
}
