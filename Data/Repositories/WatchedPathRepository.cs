using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class WatchedPathRepository : IWatchedPathRepository
    {
        private readonly AppDbContext _context;

        public WatchedPathRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WatchedPath>> GetAllAsync()
        {
            return await _context.WatchedPaths.ToListAsync();
        }

        public async Task<List<WatchedPath>> GetActiveAsync()
        {
            return await _context.WatchedPaths
                .Where(w => w.IsActive)
                .ToListAsync();
        }

        public async Task<WatchedPath?> GetByIdAsync(int id)
        {
            return await _context.WatchedPaths.FindAsync(id);
        }

        public async Task AddAsync(WatchedPath watchedPath)
        {
            watchedPath.CreatedAt = DateTime.UtcNow;
            await _context.WatchedPaths.AddAsync(watchedPath);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WatchedPath watchedPath)
        {
            watchedPath.UpdatedAt = DateTime.UtcNow;
            _context.WatchedPaths.Update(watchedPath);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var path = await GetByIdAsync(id);
            if (path != null)
            {
                _context.WatchedPaths.Remove(path);
                await _context.SaveChangesAsync();
            }
        }
    }
}
