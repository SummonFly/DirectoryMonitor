using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class EventLogRepository : IEventLogRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public EventLogRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(EventLogEntry entry)
        {
            using var context = _contextFactory.CreateDbContext();
            entry.Timestamp = DateTime.UtcNow;
            await context.EventLogEntries.AddAsync(entry);
            await context.SaveChangesAsync();
        }

        public async Task<List<EventLogEntry>> GetRecentAsync(int count)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.EventLogEntries
                .Include(e => e.WatchedPath)
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<EventLogEntry>> GetByFilterAsync(EventType? eventType, string? searchPath, DateTime? from, DateTime? to)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.EventLogEntries
                .Include(e => e.WatchedPath)
                .AsQueryable();

            if (eventType.HasValue)
            {
                query = query.Where(e => e.EventType == eventType.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchPath))
            {
                query = query.Where(e => e.Path.Contains(searchPath) ||
                                         (e.OldPath != null && e.OldPath.Contains(searchPath)));
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.Timestamp <= to.Value);
            }

            return await query.OrderByDescending(e => e.Timestamp).ToListAsync();
        }

        public async Task<int> DeleteOldAsync(DateTime before)
        {
            using var context = _contextFactory.CreateDbContext();
            var oldEntries = context.EventLogEntries.Where(e => e.Timestamp < before);
            var count = await oldEntries.CountAsync();
            context.EventLogEntries.RemoveRange(oldEntries);
            await context.SaveChangesAsync();
            return count;
        }
    }
}
