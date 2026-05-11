using DirectoryMonitor.Data;
using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Services;
using DirectoryMonitor.Services.Interfaces;
using DirectoryMonitor.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace DirectoryMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private IConfiguration? _configuration;

        private System.Windows.Forms.NotifyIcon? _trayIcon;

        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load configuration
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            _configuration = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

            // Setup DI
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            ServiceProvider = _serviceProvider;

            // Auto-migrate database
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();


            var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            await settingsService.LoadAsync();

            // Apply saved theme
            var theme = settingsService.Settings.Theme;
            var themeFileName = theme == "Dark" ? "DarkTheme.xaml" : "LightTheme.xaml";
            var themeUri = new Uri($"/Themes/{themeFileName}", UriKind.Relative);

            var currentThemeDict = Application.Current.Resources.MergedDictionaries
                .ElementAtOrDefault(1);

            if (currentThemeDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(currentThemeDict);
            }

            var newThemeDict = new ResourceDictionary { Source = themeUri };
            Application.Current.Resources.MergedDictionaries.Insert(1, newThemeDict);

            // Create and show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            var watcherManager = _serviceProvider.GetRequiredService<IWatcherManager>();
            await watcherManager.StartAllAsync();

            var ruleEngine = _serviceProvider.GetRequiredService<IRuleEngine>();
            await ruleEngine.ReloadRulesAsync();
        }

        private void InitializeTray()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon("icon.ico"),
                Visible = true,
                Text = "Directory Monitor"
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, args) => ShowMainWindow());
            contextMenu.Items.Add("Exit", null, (s, args) => Application.Current.Shutdown());
            _trayIcon.ContextMenuStrip = contextMenu;

            _trayIcon.DoubleClick += (s, args) => ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            var mainWindow = _serviceProvider?.GetRequiredService<MainWindow>();
            mainWindow?.Show();
            if (mainWindow != null) mainWindow.WindowState = WindowState.Normal;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database
            var connectionString = _configuration!.GetConnectionString("DefaultConnection");
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            // Repositories
            services.AddScoped<IWatchedPathRepository, WatchedPathRepository>();
            services.AddScoped<IEventLogRepository, EventLogRepository>();
            services.AddScoped<IRuleRepository, RuleRepository>();
            services.AddScoped<IActionRepository, ActionRepository>();
            services.AddScoped<IRuleExecutionLogRepository, RuleExecutionLogRepository>();
            services.AddScoped<IRuleActionRepository, RuleActionRepository>();
            services.AddScoped<IWatchedPathRuleRepository, WatchedPathRuleRepository>();

            // Services
            services.AddSingleton<IJournalService, JournalService>();
            services.AddSingleton<IRuleEngine, RuleEngine>();
            services.AddSingleton<IWatcherManager, WatcherManager>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<ISettingsService, SettingsService>();


            // ViewModels
            services.AddSingleton<RulesViewModel>();
            services.AddSingleton<RuleLogViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ActionsViewModel>();
            services.AddSingleton<TrayIconViewModel>();


            // Main window
            services.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }

}
