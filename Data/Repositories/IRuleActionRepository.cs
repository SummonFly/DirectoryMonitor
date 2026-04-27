using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.Data.Repositories
{
    public interface IRuleActionRepository
    {
        Task AddAsync(RuleAction ruleAction);
        Task DeleteByRuleIdAsync(int ruleId);
        Task<List<RuleAction>> GetByRuleIdAsync(int ruleId);
    }
}
