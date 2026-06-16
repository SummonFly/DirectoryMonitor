using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Data.Repositories
{
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public SystemLogRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(SystemLogEntry entry)
        {
            using var context = _contextFactory.CreateDbContext();
            entry.Timestamp = DateTime.UtcNow;
            await context.SystemLogs.AddAsync(entry);
            await context.SaveChangesAsync();
        }

        public async Task<List<SystemLogEntry>> GetRecentAsync(int limit)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.SystemLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<SystemLogEntry>> GetByLevelAsync(LogLevel level, int limit)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.SystemLogs
                .Where(l => l.Level == level)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task DeleteAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            context.SystemLogs.RemoveRange(context.SystemLogs);
            await context.SaveChangesAsync();
        }

        public async Task DeleteOldAsync(DateTime before)
        {
            using var context = _contextFactory.CreateDbContext();
            var old = context.SystemLogs.Where(l => l.Timestamp < before);
            context.SystemLogs.RemoveRange(old);
            await context.SaveChangesAsync();
        }
    }
}
