using DirectoryMonitor.Data;
using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models;
using DirectoryMonitor.Models.Conditions;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using DirectoryMonitor.Services;
using DirectoryMonitor.Services.Interfaces;
using DirectoryMonitor.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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


            // Just test
            //var ruleRepo = scope.ServiceProvider.GetRequiredService<IRuleRepository>();
            //var existingRules = await ruleRepo.GetAllAsync();

            //if (!existingRules.Any())
            //{
            //    var testRule = CreateTestRule();
            //    await ruleRepo.AddAsync(testRule);
            //}

            //var ruleEngine = scope.ServiceProvider.GetRequiredService<IRuleEngine>();
            //await ruleEngine.ReloadRulesAsync();


            // Initialize tray
            InitializeTray();

            // Create and show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            var watcherManager = _serviceProvider.GetRequiredService<IWatcherManager>();
            await watcherManager.StartAllAsync();
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

            // Services
            services.AddSingleton<IJournalService, JournalService>();
            services.AddSingleton<IRuleEngine, RuleEngine>();
            services.AddSingleton<IWatcherManager, WatcherManager>();


            // ViewModels
            services.AddSingleton<RulesViewModel>();
            services.AddSingleton<RuleLogViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ActionsViewModel>();

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
