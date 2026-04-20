using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Models;
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

            var applicableRules = _rules
                .Where(r => r.IsActive && r.EventType == eventType)
                .OrderBy(r => r.Priority)
                .ToList();

            foreach (var rule in applicableRules)
            {
                var stopwatch = Stopwatch.StartNew();
                bool success = true;
                string? errorMessage = null;

                try
                {
                    var definition = JsonConvert.DeserializeObject<RuleDefinition>(rule.DefinitionJson, _jsonSettings);
                    if (definition == null)
                        continue;

                    // Check condition
                    var fileInfo = args.ChangeType != WatcherChangeTypes.Deleted
                        ? new FileInfo(args.FullPath)
                        : null;

                    if (!definition.Condition.IsMet(args, fileInfo))
                        continue;

                    // Execute actions
                    foreach (var action in definition.Actions)
                    {
                        await action.ExecuteAsync(args);
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
    }
}
