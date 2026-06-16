using CommunityToolkit.Mvvm.ComponentModel;
using DirectoryMonitor.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DirectoryMonitor.Models.Entities;

namespace DirectoryMonitor.ViewModels
{
    public partial class ActionItemViewModel : ObservableObject
    {
        private readonly Models.Entities.Action _model;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _actionType;

        [ObservableProperty]
        private string _parametersSummary;

        public Models.Entities.Action Model => _model;

        public ActionItemViewModel(Models.Entities.Action model)
        {
            _model = model;
            _name = model.Name;
            _actionType = model.ActionType.ToString();
            _parametersSummary = GetParametersSummary(model.ParametersJson, model.ActionType);
        }

        private string GetParametersSummary(string parametersJson, ActionType actionType)
        {
            if (string.IsNullOrEmpty(parametersJson) || parametersJson == "{}")
                return "No parameters";

            try
            {
                var parameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(parametersJson);
                if (parameters == null) return "No parameters";

                return actionType switch
                {
                    Models.Enums.ActionType.Notification => $"Title: {parameters.GetValueOrDefault("Title")}",
                    Models.Enums.ActionType.RunProgram => $"Program: {parameters.GetValueOrDefault("ProgramPath")}",
                    Models.Enums.ActionType.CopyFile => $"Copy to: {parameters.GetValueOrDefault("DestinationPath")}",
                    Models.Enums.ActionType.DeleteFile => $"Delete: {parameters.GetValueOrDefault("FilePath")}",
                    Models.Enums.ActionType.MoveFile => $"Move to: {parameters.GetValueOrDefault("DestinationPath")}",
                    Models.Enums.ActionType.RenameFile => $"Rename to: {parameters.GetValueOrDefault("NewName")}",
                    Models.Enums.ActionType.CreateDirectory => $"Create: {parameters.GetValueOrDefault("DirectoryPath")}",
                    Models.Enums.ActionType.DeleteDirectory => $"Delete: {parameters.GetValueOrDefault("DirectoryPath")}",
                    _ => "Configured"
                };
            }
            catch
            {
                return "Complex parameters";
            }
        }

        public void UpdateFromModel()
        {
            Name = _model.Name;
            ActionType = _model.ActionType.ToString();
            ParametersSummary = GetParametersSummary(_model.ParametersJson, _model.ActionType);
        }
    }
}
