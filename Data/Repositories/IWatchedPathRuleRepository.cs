using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.Data.Repositories
{
    public interface IWatchedPathRuleRepository
    {
        Task<List<Rule>> GetRulesForWatchedPathAsync(int watchedPathId);
        Task<List<int>> GetRuleIdsForWatchedPathAsync(int watchedPathId);
        Task UpdateRulesForWatchedPathAsync(int watchedPathId, List<int> selectedRuleIds);
    }
}
