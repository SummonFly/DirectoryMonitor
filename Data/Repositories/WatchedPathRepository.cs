using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace DirectoryMonitor.Data.Repositories
{
    public class WatchedPathRepository : IWatchedPathRepository
    {
        IDbContextFactory<AppDbContext> _contextFactory;

        public WatchedPathRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<WatchedPath>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.WatchedPaths.ToListAsync();
        }

        public async Task<List<WatchedPath>> GetActiveAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.WatchedPaths
                .Where(w => w.IsActive)
                .ToListAsync();
        }

        public async Task<WatchedPath?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.WatchedPaths.FindAsync(id);
        }

        public async Task AddAsync(WatchedPath watchedPath)
        {
            using var context = _contextFactory.CreateDbContext();
            watchedPath.CreatedAt = DateTime.UtcNow;
            await context.WatchedPaths.AddAsync(watchedPath);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WatchedPath watchedPath)
        {
            using var context = _contextFactory.CreateDbContext();
            watchedPath.UpdatedAt = DateTime.UtcNow;
            context.WatchedPaths.Update(watchedPath);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var path = await GetByIdAsync(id);
            if (path != null)
            {
                context.WatchedPaths.Remove(path);
                await context.SaveChangesAsync();
            }
        }
    }
}
