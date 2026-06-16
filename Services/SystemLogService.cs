using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using DirectoryMonitor.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Services
{
    public class SystemLogService : ISystemLogService
    {
        private readonly ISystemLogRepository _repository;

        public SystemLogService(ISystemLogRepository repository)
        {
            _repository = repository;
        }

        public async Task InfoAsync(string source, string message, string? details = null)
        {
            await AddLog(LogLevel.Info, source, message, details);
        }

        public async Task WarningAsync(string source, string message, string? details = null)
        {
            await AddLog(LogLevel.Warning, source, message, details);
        }

        public async Task ErrorAsync(string source, string message, string? details = null)
        {
            await AddLog(LogLevel.Error, source, message, details);
        }

        public async Task DebugAsync(string source, string message, string? details = null)
        {
            await AddLog(LogLevel.Debug, source, message, details);
        }

        private async Task AddLog(LogLevel level, string source, string message, string? details)
        {
            var entry = new SystemLogEntry
            {
                Level = level,
                Source = source,
                Message = message,
                Details = details
            };
            await _repository.AddAsync(entry);
        }
    }
}
