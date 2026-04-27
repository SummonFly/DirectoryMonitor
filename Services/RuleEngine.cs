using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models;
using DirectoryMonitor.Models.Conditions;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using DirectoryMonitor.Services.Interfaces;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace DirectoryMonitor.Services
{
    public class RuleEngine : IRuleEngine
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly IRuleExecutionLogRepository _logRepository;
        private List<Rule> _rules = new();
        private readonly JsonSerializerSettings _jsonSettings;

        public RuleEngine(IRuleRepository ruleRepository, IRuleExecutionLogRepository logRepository)
        {
            _ruleRepository = ruleRepository;
            _logRepository = logRepository;

            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
        }

        public async Task ReloadRulesAsync()
        {
            _rules = await _ruleRepository.GetActiveAsync();
        }

        public async Task EvaluateAndExecuteAsync(FileSystemEventArgs args, int? watchedPathId = null)
        {
            var eventType = args.ChangeType switch
            {
                WatcherChangeTypes.Created => EventType.Created,
                WatcherChangeTypes.Changed => EventType.Changed,
                WatcherChangeTypes.Deleted => EventType.Deleted,
                WatcherChangeTypes.Renamed => EventType.Renamed,
                _ => EventType.Changed
            };

            // Get applicable rules (by event type and by WatchedPathId if provided)
            var applicableRules = _rules
                .Where(r => r.IsActive && r.EventType == eventType)
                .ToList();

            // If watchedPathId is provided, filter rules that are linked to this path
            if (watchedPathId.HasValue)
            {
                applicableRules = applicableRules
                    .Where(r => r.WatchedPathRules.Any(wpr => wpr.WatchedPathId == watchedPathId.Value))
                    .ToList();
            }

            applicableRules = applicableRules.OrderBy(r => r.Priority).ToList();

            foreach (var rule in applicableRules)
            {
                var stopwatch = Stopwatch.StartNew();
                bool success = true;
                string? errorMessage = null;

                try
                {
                    // Deserialize conditions from JSON
                    var condition = JsonConvert.DeserializeObject<ConditionNode>(rule.ConditionsJson, _jsonSettings);
                    if (condition == null)
                        continue;

                    // Check condition
                    var fileInfo = args.ChangeType != WatcherChangeTypes.Deleted
                        ? new FileInfo(args.FullPath)
                        : null;

                    if (!condition.IsMet(args, fileInfo))
                        continue;

                    // Load rule with actions
                    var ruleWithActions = await _ruleRepository.GetRuleWithActionsAsync(rule.Id);
                    if (ruleWithActions == null || !ruleWithActions.RuleActions.Any())
                        continue;

                    // Execute actions in order
                    var orderedActions = ruleWithActions.RuleActions.OrderBy(ra => ra.Order).ToList();
                    foreach (var ruleAction in orderedActions)
                    {
                        var action = ruleAction.Action;
                        if (action == null) continue;

                        await ExecuteActionAsync(action, args);
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    errorMessage = ex.Message;
                }
                finally
                {
                    stopwatch.Stop();

                    // Log execution
                    var logEntry = new RuleExecutionLogEntry
                    {
                        RuleId = rule.Id,
                        RuleName = rule.Name,
                        EventPath = args.FullPath,
                        EventType = eventType,
                        Success = success,
                        ErrorMessage = errorMessage,
                        ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds
                    };

                    await _logRepository.AddAsync(logEntry);
                }
            }
        }

        private async Task ExecuteActionAsync(Models.Entities.Action action, FileSystemEventArgs args)
        {
            // Deserialize parameters and execute based on ActionType
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(action.ParametersJson)
                             ?? new Dictionary<string, object>();

            switch (action.ActionType)
            {
                case ActionType.Notification:
                    var title = parameters.GetValueOrDefault("Title")?.ToString() ?? "Directory Monitor";
                    var message = parameters.GetValueOrDefault("Message")?.ToString() ?? string.Empty;
                    await ShowNotificationAsync(title, message);
                    break;

                case ActionType.RunProgram:
                    var programPath = parameters.GetValueOrDefault("ProgramPath")?.ToString() ?? string.Empty;
                    var arguments = parameters.GetValueOrDefault("Arguments")?.ToString() ?? string.Empty;
                    await RunProgramAsync(programPath, arguments);
                    break;

                case ActionType.CopyFile:
                    var destFolder = parameters.GetValueOrDefault("DestinationFolder")?.ToString() ?? string.Empty;
                    var overwrite = parameters.GetValueOrDefault("Overwrite") as bool? ?? false;
                    await CopyFileAsync(args.FullPath, destFolder, overwrite);
                    break;
            }
        }

        private Task ShowNotificationAsync(string title, string message)
        {
            // Will be implemented with INotificationService
            System.Diagnostics.Debug.WriteLine($"Notification: {title} - {message}");
            return Task.CompletedTask;
        }

        private Task RunProgramAsync(string programPath, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = programPath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to run program: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }

        private Task CopyFileAsync(string sourcePath, string destinationFolder, bool overwrite)
        {
            try
            {
                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                var fileName = Path.GetFileName(sourcePath);
                var destPath = Path.Combine(destinationFolder, fileName);
                File.Copy(sourcePath, destPath, overwrite);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to copy file: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }
}
}
