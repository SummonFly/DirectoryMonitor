using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models.Conditions;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using DirectoryMonitor.Services.Interfaces;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DirectoryMonitor.Services
{
    public class RuleEngine : IRuleEngine
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly INotificationService _notificationService;
        private readonly ISystemLogService _systemLogService;
        private List<Rule> _rules = new();
        private readonly JsonSerializerSettings _jsonSettings;

        public RuleEngine(IRuleRepository ruleRepository, 
            INotificationService notificationService,
            ISystemLogService systemLogService)
        {
            _ruleRepository = ruleRepository;
            _notificationService = notificationService;
            _systemLogService = systemLogService;

            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
        }

        public async Task ReloadRulesAsync()
        {
            _rules = await _ruleRepository.GetActiveAsync();
            await _systemLogService.InfoAsync("RuleEngine", $"Reloaded {_rules.Count} rules");
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
            var test = await _ruleRepository.GetActiveAsync();

            var applicableRules = _rules
                .Where(r => r.IsActive && r.EventType == eventType)
                .ToList();

            var details = new StringBuilder();
            foreach(var r in applicableRules)
            {
                details.Append($"Rule: {r.Name} has {r.WatchedPathRules.Count} attached path\n");
            }
            await _systemLogService.InfoAsync("RuleEngine", $"Applicable rule has {applicableRules.Count} rules", details.ToString());

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
                    await _systemLogService.DebugAsync("RuleEngine", $"Rule '{rule.Name}' executed", $"Path: {args.FullPath}");
                }
                catch (Exception ex)
                {
                    success = false;
                    errorMessage = ex.Message;

                    // Log to system log
                    await _systemLogService.ErrorAsync(
                        "RuleEngine",
                        $"Rule '{rule.Name}' execution failed",
                        $"RuleId: {rule.Id}, EventPath: {args.FullPath}, Error: {ex.Message}");
                }
                finally
                {
                    stopwatch.Stop();

                    await _systemLogService.InfoAsync("RuleEngine",
                    $"Rule '{rule.Name}' executed on {args.FullPath}, Success: {success}, Time: {stopwatch.ElapsedMilliseconds}ms" +
                    (errorMessage != null ? $", Error: {errorMessage}" : ""));
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
                    var titleTemplate = parameters.GetValueOrDefault("Title")?.ToString() ?? "Directory Monitor";
                    var messageTemplate = parameters.GetValueOrDefault("Message")?.ToString() ?? string.Empty;
                    var title = ReplaceVariables(titleTemplate, args);
                    var message = ReplaceVariables(messageTemplate, args);
                    _notificationService.ShowToast(title, message);
                    break;

                case ActionType.RunProgram:
                    var programPath = parameters.GetValueOrDefault("ProgramPath")?.ToString() ?? string.Empty;
                    var argumentsTemplate = parameters.GetValueOrDefault("Arguments")?.ToString() ?? string.Empty;
                    var arguments = ReplaceVariables(argumentsTemplate, args);
                    await RunProgramAsync(programPath, arguments);
                    break;

                case ActionType.CopyFile:
                    var sourceTemplate = parameters.GetValueOrDefault("SourcePath")?.ToString() ?? "{FilePath}";
                    var destTemplate = parameters.GetValueOrDefault("DestinationPath")?.ToString() ?? "";
                    var overwrite = parameters.GetValueOrDefault("Overwrite") as bool? ?? false;

                    var sourcePath = ReplaceVariables(sourceTemplate, args);
                    var destPath = ReplaceVariables(destTemplate, args);

                    await CopyFileAsync(sourcePath, destPath, overwrite);
                    break;

                case ActionType.DeleteFile:
                    var filePathTemplate = parameters.GetValueOrDefault("FilePath")?.ToString() ?? "{FilePath}";
                    var filePath = ReplaceVariables(filePathTemplate, args);
                    await DeleteFileAsync(filePath);
                    break;
                case ActionType.MoveFile:
                    var moveSourceTemplate = parameters.GetValueOrDefault("SourcePath")?.ToString() ?? "{FilePath}";
                    var moveDestTemplate = parameters.GetValueOrDefault("DestinationPath")?.ToString() ?? "";
                    var moveSource = ReplaceVariables(moveSourceTemplate, args);
                    var moveDest = ReplaceVariables(moveDestTemplate, args);
                    await MoveFileAsync(moveSource, moveDest);
                    break;

                case ActionType.RenameFile:
                    var renameFilePathTemplate = parameters.GetValueOrDefault("FilePath")?.ToString() ?? "{FilePath}";
                    var newNameTemplate = parameters.GetValueOrDefault("NewName")?.ToString() ?? "{FileName}_renamed";
                    var renameFilePath = ReplaceVariables(renameFilePathTemplate, args);
                    var newName = ReplaceVariables(newNameTemplate, args);
                    await RenameFileAsync(renameFilePath, newName);
                    break;
                case ActionType.CreateDirectory:
                    var createDirTemplate = parameters.GetValueOrDefault("DirectoryPath")?.ToString() ?? "{DirectoryPath}\\NewFolder";
                    var createDirPath = ReplaceVariables(createDirTemplate, args);
                    await CreateDirectoryAsync(createDirPath);
                    break;

                case ActionType.DeleteDirectory:
                    var deleteDirTemplate = parameters.GetValueOrDefault("DirectoryPath")?.ToString() ?? "{DirectoryPath}";
                    var deleteDirPath = ReplaceVariables(deleteDirTemplate, args);
                    await DeleteDirectoryAsync(deleteDirPath);
                    break;
            }
        }

        private string ReplaceVariables(string template, FileSystemEventArgs args)
        {
            var fileName = Path.GetFileName(args.FullPath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(args.FullPath);
            var extension = Path.GetExtension(args.FullPath).TrimStart('.');
            var directory = Path.GetDirectoryName(args.FullPath) ?? "";
            var eventType = args.ChangeType.ToString();
            var now = DateTime.Now;

            var result = template
                .Replace("{FilePath}", args.FullPath)
                .Replace("{FileName}", fileName)
                .Replace("{FileNameWithoutExt}", fileNameWithoutExt)
                .Replace("{Extension}", extension)
                .Replace("{DirectoryPath}", directory)
                .Replace("{EventType}", eventType)
                .Replace("{Timestamp}", now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{Date}", now.ToString("yyyy-MM-dd"))
                .Replace("{Time}", now.ToString("HH:mm:ss"))
                .Replace("{Year}", now.Year.ToString())
                .Replace("{Month}", now.Month.ToString())
                .Replace("{Day}", now.Day.ToString())
                .Replace("{Hour}", now.Hour.ToString())
                .Replace("{Minute}", now.Minute.ToString())
                .Replace("{Second}", now.Second.ToString());

            if (args is RenamedEventArgs renamed)
            {
                result = result.Replace("{OldFilePath}", renamed.OldFullPath);
            }

            return result;
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

        private Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite)
        {
            try
            {
                var destDir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                File.Copy(sourcePath, destinationPath, overwrite);
                Debug.WriteLine($"Copied: {sourcePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to copy file: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }

        private Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    System.Diagnostics.Debug.WriteLine($"Deleted file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete file: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }
        private Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    Debug.WriteLine($"Source file not found: {sourcePath}");
                    return Task.CompletedTask;
                }

                var destDir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                File.Move(sourcePath, destinationPath);
                Debug.WriteLine($"Moved: {sourcePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to move file: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }

        private Task RenameFileAsync(string filePath, string newName)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"File not found: {filePath}");
                    return Task.CompletedTask;
                }

                var directory = Path.GetDirectoryName(filePath) ?? "";
                var newPath = Path.Combine(directory, newName);

                File.Move(filePath, newPath);
                Debug.WriteLine($"Renamed: {filePath} -> {newPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to rename file: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }
        private Task CreateDirectoryAsync(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Debug.WriteLine($"Created directory: {directoryPath}");
                }
                else
                {
                    Debug.WriteLine($"Directory already exists: {directoryPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create directory: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }

        private Task DeleteDirectoryAsync(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true); // recursive delete
                    Debug.WriteLine($"Deleted directory: {directoryPath}");
                }
                else
                {
                    Debug.WriteLine($"Directory not found: {directoryPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to delete directory: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }
    }
}
