using DirectoryMonitor.Models.Enums;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;

namespace DirectoryMonitor.Views
{
    /// <summary>
    /// Логика взаимодействия для ActionEditorWindow.xaml
    /// </summary>
    public partial class ActionEditorWindow : Window
    {
        private Models.Entities.Action? _editingAction;
        private TextBox? _titleBox;
        private TextBox? _messageBox;
        private TextBox? _programPathBox;
        private TextBox? _argumentsBox;
        private TextBox? _destFolderBox;
        private CheckBox? _overwriteBox;

        public ActionEditorWindow(Models.Entities.Action? action = null)
        {
            InitializeComponent();
            _editingAction = action;

            if (action != null)
            {
                NameBox.Text = action.Name;
                ActionTypeCombo.SelectedIndex = (int)action.ActionType;
                LoadParameters(action.ParametersJson, action.ActionType);
            }
            else
            {
                ActionTypeCombo.SelectedIndex = 0;
            }
        }

        private void LoadParameters(string parametersJson, ActionType actionType)
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parametersJson)
                             ?? new Dictionary<string, object>();

            switch (actionType)
            {
                case ActionType.Notification:
                    _titleBox = new TextBox { Text = parameters.GetValueOrDefault("Title")?.ToString() ?? "", Margin = new Thickness(0, 5, 0, 5) };
                    _messageBox = new TextBox { Text = parameters.GetValueOrDefault("Message")?.ToString() ?? "", Margin = new Thickness(0, 5, 0, 5), Height = 60, TextWrapping = TextWrapping.Wrap };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "Title:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_titleBox);
                    ParametersPanel.Children.Add(new Label { Content = "Message:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 0) });
                    ParametersPanel.Children.Add(_messageBox);
                    break;

                case ActionType.RunProgram:
                    _programPathBox = new TextBox { Text = parameters.GetValueOrDefault("ProgramPath")?.ToString() ?? "", Margin = new Thickness(0, 5, 0, 5) };
                    _argumentsBox = new TextBox { Text = parameters.GetValueOrDefault("Arguments")?.ToString() ?? "", Margin = new Thickness(0, 5, 0, 5) };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "Program Path:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_programPathBox);
                    ParametersPanel.Children.Add(new Label { Content = "Arguments:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 0) });
                    ParametersPanel.Children.Add(_argumentsBox);
                    break;

                case ActionType.CopyFile:
                    _destFolderBox = new TextBox { Text = parameters.GetValueOrDefault("DestinationFolder")?.ToString() ?? "", Margin = new Thickness(0, 5, 0, 5) };
                    _overwriteBox = new CheckBox { Content = "Overwrite", IsChecked = parameters.GetValueOrDefault("Overwrite") as bool? ?? false, Margin = new Thickness(0, 10, 0, 0) };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "Destination Folder:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_destFolderBox);
                    ParametersPanel.Children.Add(_overwriteBox);
                    break;
            }
        }

        private void ActionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var actionType = (ActionType)ActionTypeCombo.SelectedIndex;
            LoadParameters("{}", actionType);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public Models.Entities.Action GetAction()
        {
            var actionType = (ActionType)ActionTypeCombo.SelectedIndex;
            var parameters = new Dictionary<string, object>();

            switch (actionType)
            {
                case ActionType.Notification:
                    parameters["Title"] = _titleBox?.Text ?? "";
                    parameters["Message"] = _messageBox?.Text ?? "";
                    break;

                case ActionType.RunProgram:
                    parameters["ProgramPath"] = _programPathBox?.Text ?? "";
                    parameters["Arguments"] = _argumentsBox?.Text ?? "";
                    break;

                case ActionType.CopyFile:
                    parameters["DestinationFolder"] = _destFolderBox?.Text ?? "";
                    parameters["Overwrite"] = _overwriteBox?.IsChecked ?? false;
                    break;
            }

            return new Models.Entities.Action
            {
                Name = NameBox.Text,
                ActionType = actionType,
                ParametersJson = JsonConvert.SerializeObject(parameters, Formatting.Indented),
                CreatedAt = _editingAction?.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
