using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.Data.Repositories
{
    public interface IWatchedPathRepository
    {
        Task<List<WatchedPath>> GetAllAsync();
        Task<List<WatchedPath>> GetActiveAsync();
        Task<WatchedPath?> GetByIdAsync(int id);
        Task AddAsync(WatchedPath watchedPath);
        Task UpdateAsync(WatchedPath watchedPath);
        Task DeleteAsync(int id);
    }
}
