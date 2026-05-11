using DirectoryMonitor.Models;
using DirectoryMonitor.Services.Interfaces;
using System.Windows;

namespace DirectoryMonitor.Views
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly ISettingsService _settingsService;
        private AppSettings _originalSettings;

        public SettingsWindow(ISettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _originalSettings = new AppSettings();

            Loaded += (s, e) => LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.Settings;

            // Copy for rollback
            _originalSettings.Theme = settings.Theme;
            _originalSettings.DebounceMilliseconds = settings.DebounceMilliseconds;
            _originalSettings.MaxLogEntries = settings.MaxLogEntries;
            _originalSettings.LogRetentionDays = settings.LogRetentionDays;

            ThemeCombo.SelectedIndex = settings.Theme == "Dark" ? 1 : 0;
            DebounceBox.Text = settings.DebounceMilliseconds.ToString();
            MaxLogEntriesBox.Text = settings.MaxLogEntries.ToString();
            LogRetentionBox.Text = settings.LogRetentionDays.ToString();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await _settingsService.UpdateAsync(settings =>
            {
                settings.Theme = (ThemeCombo.SelectedIndex == 1) ? "Dark" : "Light";
                settings.DebounceMilliseconds = int.TryParse(DebounceBox.Text, out var deb) ? deb : 300;
                settings.MaxLogEntries = int.TryParse(MaxLogEntriesBox.Text, out var max) ? max : 200;
                settings.LogRetentionDays = int.TryParse(LogRetentionBox.Text, out var days) ? days : 30;
            });

            // Apply theme
            ApplyTheme();

            DialogResult = true;
            Close();
        }

        private void ApplyTheme()
        {
            var isDark = _settingsService.Settings.Theme == "Dark";
            var themeFileName = isDark ? "DarkTheme.xaml" : "LightTheme.xaml";
            var themeUri = new Uri($"/Themes/{themeFileName}", UriKind.Relative);

            var app = Application.Current;

            // Find theme dictionary (index 1)
            var themeDict = app.Resources.MergedDictionaries
                .ElementAtOrDefault(1);

            if (themeDict != null)
            {
                app.Resources.MergedDictionaries.Remove(themeDict);
            }

            var newThemeDict = new ResourceDictionary { Source = themeUri };
            app.Resources.MergedDictionaries.Insert(1, newThemeDict);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
