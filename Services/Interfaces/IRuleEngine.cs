using DirectoryMonitor.Models.Entities;
using System.IO;

namespace DirectoryMonitor.Services.Interfaces
{
    public interface IRuleEngine
    {
        Task ReloadRulesAsync();
        Task EvaluateAndExecuteAsync(FileSystemEventArgs args, int? watchedPathId = null);
    }
}
