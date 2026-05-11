using DirectoryMonitor.Models;

namespace DirectoryMonitor.Services.Interfaces
{
    public interface ISettingsService
    {
        AppSettings Settings { get; }
        Task LoadAsync();
        Task SaveAsync();
        Task UpdateAsync(Action<AppSettings> updateAction);
    }
}
