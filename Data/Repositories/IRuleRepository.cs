using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.Data.Repositories
{
    public interface IRuleRepository
    {
        Task<List<Rule>> GetAllAsync();
        Task<List<Rule>> GetActiveAsync();
        Task<Rule?> GetByIdAsync(int id);
        Task<Rule?> GetRuleWithActionsAsync(int id);
        Task AddAsync(Rule rule);
        Task UpdateAsync(Rule rule);
        Task DeleteAsync(int id);
    }
}
