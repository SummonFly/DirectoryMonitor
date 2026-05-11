using CommunityToolkit.Mvvm.ComponentModel;
using DirectoryMonitor.Models.Conditions;
using DirectoryMonitor.Models.Entities;
using DirectoryMonitor.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DirectoryMonitor.ViewModels
{
    public partial class RuleItemViewModel : ObservableObject
    {
        private readonly Rule _model;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private int _priority;

        [ObservableProperty]
        private string _eventType;

        [ObservableProperty]
        private string _conditionDescription;

        public Rule Model => _model;

        public RuleItemViewModel(Rule model)
        {
            _model = model;
            _name = model.Name;
            _isActive = model.IsActive;
            _priority = model.Priority;
            _eventType = model.EventType.ToString();
            _conditionDescription = GetConditionDescription(model.ConditionsJson);
        }

        private string GetConditionDescription(string conditionsJson)
        {
            if (string.IsNullOrEmpty(conditionsJson) || conditionsJson == "{}")
                return "No conditions (always true)";

            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                var condition = JsonConvert.DeserializeObject<ConditionNode>(conditionsJson, settings);
                return condition?.ToString() ?? "Complex condition";
            }
            catch
            {
                return "Invalid condition";
            }
        }

        public void UpdateFromModel()
        {
            Name = _model.Name;
            IsActive = _model.IsActive;
            Priority = _model.Priority;
            EventType = _model.EventType.ToString();
            ConditionDescription = GetConditionDescription(_model.ConditionsJson);
        }
    }
}
