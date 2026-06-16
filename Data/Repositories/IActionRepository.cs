using DirectoryMonitor.Helpers;
using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.Data.Repositories
{
    public interface IActionRepository
    {
        Task<List<Models.Entities.Action>> GetAllAsync();
        Task<Models.Entities.Action?> GetByIdAsync(int id);
        Task<Rule?> GetRuleWithActionsAsync(int ruleId);
        Task<Rule> AddRuleWithActionsAsync(Rule rule, List<RuleActionItem> actions);
        Task UpdateRuleWithActionsAsync(Rule rule, List<RuleActionItem> actions);
        Task AddAsync(Models.Entities.Action action);
        Task UpdateAsync(Models.Entities.Action action);
        Task DeleteAsync(int id);
    }
}
