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
        private TextBox? _deleteFilePathBox;
        private TextBox? _sourcePathBox;
        private TextBox? _destPathBox;
        private TextBox? _moveSourcePathBox;
        private TextBox? _moveDestPathBox;
        private TextBox? _renameFilePathBox;
        private TextBox? _renameNewNameBox;
        private TextBox? _createDirectoryPathBox;
        private TextBox? _deleteDirectoryPathBox;


        private static string _actionVariables => "Variables: {FilePath}, {FileName}, {FileNameWithoutExt}, {Extension}, {DirectoryPath}, {EventType}, {OldFilePath}, {Timestamp}, {Date}, {Time}, {Year}, {Month}, {Day}, {Hour}, {Minute}, {Second}";


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
                    ParametersPanel.Children.Add(new TextBlock
                    {
                        Text = _actionVariables,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontSize = 11,
                        TextWrapping = TextWrapping.Wrap
                    });
                    break;

                case ActionType.RunProgram:
                    _programPathBox = new TextBox { Text = parameters.GetValueOrDefault("ProgramPath")?.ToString() ?? "", Margin = new Thickness(0, 5, 0, 5) };
                    _argumentsBox = new TextBox { Text = parameters.GetValueOrDefault("Arguments")?.ToString() ?? "", Margin = new Thickness(0, 5, 0, 5) };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "Program Path:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_programPathBox);
                    ParametersPanel.Children.Add(new Label { Content = "Arguments:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 0) });
                    ParametersPanel.Children.Add(_argumentsBox);
                    ParametersPanel.Children.Add(new TextBlock
                    {
                        Text = _actionVariables,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontSize = 11,
                        TextWrapping = TextWrapping.Wrap
                    });
                    break;

                case ActionType.CopyFile:
                    _sourcePathBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("SourcePath")?.ToString() ?? "{FilePath}",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    _destPathBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("DestinationPath")?.ToString() ?? "",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    _overwriteBox = new CheckBox
                    {
                        Content = "Overwrite",
                        IsChecked = parameters.GetValueOrDefault("Overwrite") as bool? ?? false,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "Source Path Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_sourcePathBox);
                    ParametersPanel.Children.Add(new Label { Content = "Destination Path Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_destPathBox);
                    ParametersPanel.Children.Add(_overwriteBox);
                    ParametersPanel.Children.Add(new TextBlock
                    {
                        Text = _actionVariables,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontSize = 11
                    });
                    break;
                case ActionType.DeleteFile:
                    _deleteFilePathBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("FilePath")?.ToString() ?? "{FilePath}",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "File Path Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_deleteFilePathBox);
                    ParametersPanel.Children.Add(new TextBlock
                    {
                        Text = _actionVariables,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontSize = 11
                    });
                    break;
                case ActionType.MoveFile:
                    _moveSourcePathBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("SourcePath")?.ToString() ?? "{FilePath}",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    _moveDestPathBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("DestinationPath")?.ToString() ?? "",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "Source Path Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_moveSourcePathBox);
                    ParametersPanel.Children.Add(new Label { Content = "Destination Path Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_moveDestPathBox);
                    ParametersPanel.Children.Add(new TextBlock
                    {
                        Text = _actionVariables,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontSize = 11
                    });
                    break;

                case ActionType.RenameFile:
                    _renameFilePathBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("FilePath")?.ToString() ?? "{FilePath}",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    _renameNewNameBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("NewName")?.ToString() ?? "{FileName}_renamed",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "File Path Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_renameFilePathBox);
                    ParametersPanel.Children.Add(new Label { Content = "New Name Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_renameNewNameBox);
                    ParametersPanel.Children.Add(new TextBlock
                    {
                        Text = _actionVariables,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontSize = 11
                    });
                    break;
                case ActionType.CreateDirectory:
                    _createDirectoryPathBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("DirectoryPath")?.ToString() ?? "{DirectoryPath}\\NewFolder",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "Directory Path Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_createDirectoryPathBox);
                    ParametersPanel.Children.Add(new TextBlock
                    {
                        Text = _actionVariables,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontSize = 11
                    });
                    break;

                case ActionType.DeleteDirectory:
                    _deleteDirectoryPathBox = new TextBox
                    {
                        Text = parameters.GetValueOrDefault("DirectoryPath")?.ToString() ?? "{DirectoryPath}",
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    ParametersPanel.Children.Clear();
                    ParametersPanel.Children.Add(new Label { Content = "Directory Path Template:", FontWeight = FontWeights.Bold });
                    ParametersPanel.Children.Add(_deleteDirectoryPathBox);
                    ParametersPanel.Children.Add(new TextBlock
                    {
                        Text = _actionVariables,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontSize = 11
                    });
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
                    parameters["SourcePath"] = _sourcePathBox?.Text ?? "{FilePath}";
                    parameters["DestinationPath"] = _destPathBox?.Text ?? "";
                    parameters["Overwrite"] = _overwriteBox?.IsChecked ?? false;
                    break;
                case ActionType.DeleteFile:
                    parameters["FilePath"] = _deleteFilePathBox?.Text ?? "{FilePath}";
                    break;
                case ActionType.MoveFile:
                    parameters["SourcePath"] = _moveSourcePathBox?.Text ?? "{FilePath}";
                    parameters["DestinationPath"] = _moveDestPathBox?.Text ?? "";
                    break;

                case ActionType.RenameFile:
                    parameters["FilePath"] = _renameFilePathBox?.Text ?? "{FilePath}";
                    parameters["NewName"] = _renameNewNameBox?.Text ?? "{FileName}_renamed";
                    break;
                case ActionType.CreateDirectory:
                    parameters["DirectoryPath"] = _createDirectoryPathBox?.Text ?? "{DirectoryPath}\\NewFolder";
                    break;

                case ActionType.DeleteDirectory:
                    parameters["DirectoryPath"] = _deleteDirectoryPathBox?.Text ?? "{DirectoryPath}";
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
