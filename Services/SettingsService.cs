using DirectoryMonitor.Models;
using DirectoryMonitor.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace DirectoryMonitor.Services
{

    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private AppSettings _settings = new();

        public AppSettings Settings => _settings;

        public SettingsService()
        {
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        }

        public async Task LoadAsync()
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsPath, json);
        }

        public async Task UpdateAsync(Action<AppSettings> updateAction)
        {
            updateAction(_settings);
            await SaveAsync();
        }
    }
}
