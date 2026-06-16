using DirectoryMonitor.Models.Conditions;
using DirectoryMonitor.Models.Enums;
using System.Windows;
using System.Windows.Controls;

namespace DirectoryMonitor.Views
{
    /// <summary>
    /// Логика взаимодействия для ConditionDialog.xaml
    /// </summary>
    public partial class ConditionDialog : Window
    {
        private ComboBox? _operatorCombo;
        private TextBox? _valueBox;
        private ComboBox? _unitCombo;
        private TextBox? _patternBox;
        private ComboBox? _matchTypeCombo;
        private ListBox? _extensionsList;
        private TextBox? _extensionInput;
        private ComboBox? _itemTypeCombo;

        public ConditionNode Result { get; private set; } = null!;

        public ConditionDialog(ConditionNode? existing = null)
        {
            InitializeComponent();

            if (existing != null)
            {
                // Load existing condition for editing
                // TODO: implement
            }
        }

        private void ConditionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = ConditionTypeCombo.SelectedIndex;
            ParametersPanel.Children.Clear();

            switch (selectedIndex)
            {
                case 0: // Extension
                    CreateExtensionUI();
                    break;
                case 1: // File Name
                    CreateFileNameUI();
                    break;
                case 2: // Size
                    CreateSizeUI();
                    break;
                case 3: // Item Type
                    CreateItemTypeUI();
                    break;
            }
        }

        private void CreateExtensionUI()
        {
            _extensionsList = new ListBox { Height = 100, Margin = new Thickness(0, 5, 0, 5) };
            _extensionInput = new TextBox { Margin = new Thickness(0, 5, 0, 5), IsEnabled = true};
            var addButton = new Button { Content = "Add", Width = 60, Margin = new Thickness(0, 5, 0, 5) };

            addButton.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(_extensionInput?.Text))
                {
                    _extensionsList?.Items.Add(_extensionInput.Text);
                    _extensionInput.Text = "";
                }
            };

            ParametersPanel.Children.Add(new Label { Content = "Extensions:" });
            ParametersPanel.Children.Add(_extensionsList);
            ParametersPanel.Children.Add(_extensionInput);
            ParametersPanel.Children.Add(addButton);
        }

        private void CreateFileNameUI()
        {
            _patternBox = new TextBox { Margin = new Thickness(0, 5, 0, 5) };
            _matchTypeCombo = new ComboBox { Margin = new Thickness(0, 5, 0, 5) };
            _matchTypeCombo.Items.Add("Equals");
            _matchTypeCombo.Items.Add("Contains");
            _matchTypeCombo.Items.Add("StartsWith");
            _matchTypeCombo.Items.Add("EndsWith");
            _matchTypeCombo.Items.Add("Regex");
            _matchTypeCombo.SelectedIndex = 1;

            ParametersPanel.Children.Add(new Label { Content = "Pattern:" });
            ParametersPanel.Children.Add(_patternBox);
            ParametersPanel.Children.Add(new Label { Content = "Match Type:" });
            ParametersPanel.Children.Add(_matchTypeCombo);
        }

        private void CreateSizeUI()
        {
            _operatorCombo = new ComboBox { Margin = new Thickness(0, 5, 0, 5) };
            _operatorCombo.Items.Add("Greater");
            _operatorCombo.Items.Add("Less");
            _operatorCombo.Items.Add("Equal");
            _operatorCombo.SelectedIndex = 0;

            _valueBox = new TextBox { Margin = new Thickness(0, 5, 0, 5) };

            _unitCombo = new ComboBox { Margin = new Thickness(0, 5, 0, 5) };
            _unitCombo.Items.Add("Bytes");
            _unitCombo.Items.Add("KB");
            _unitCombo.Items.Add("MB");
            _unitCombo.Items.Add("GB");
            _unitCombo.SelectedIndex = 0;

            ParametersPanel.Children.Add(new Label { Content = "Operator:" });
            ParametersPanel.Children.Add(_operatorCombo);
            ParametersPanel.Children.Add(new Label { Content = "Value:" });
            ParametersPanel.Children.Add(_valueBox);
            ParametersPanel.Children.Add(new Label { Content = "Unit:" });
            ParametersPanel.Children.Add(_unitCombo);
        }

        private void CreateItemTypeUI()
        {
            var comboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 5) };
            comboBox.Items.Add("File");
            comboBox.Items.Add("Directory");
            comboBox.SelectedIndex = 0;

            ParametersPanel.Children.Clear();
            ParametersPanel.Children.Add(new Label { Content = "Type:", FontWeight = FontWeights.Bold });
            ParametersPanel.Children.Add(comboBox);
            _itemTypeCombo = comboBox;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = ConditionTypeCombo.SelectedIndex;

            switch (selectedIndex)
            {
                case 0: // Extension
                    var extensions = _extensionsList?.Items.Cast<string>().ToList() ?? new List<string>();
                    Result = new ExtensionCondition
                    {
                        Extensions = extensions,
                        MatchType = ExtensionMatchType.Equals
                    };
                    break;

                case 1: // File Name
                    var matchType = _matchTypeCombo?.SelectedItem?.ToString() ?? "Contains";
                    var matchTypeEnum = matchType switch
                    {
                        "Equals" => StringMatchType.Equals,
                        "Contains" => StringMatchType.Contains,
                        "StartsWith" => StringMatchType.StartsWith,
                        "EndsWith" => StringMatchType.EndsWith,
                        "Regex" => StringMatchType.Regex,
                        _ => StringMatchType.Contains
                    };
                    Result = new FileNameCondition
                    {
                        Pattern = _patternBox?.Text ?? "",
                        MatchType = matchTypeEnum
                    };
                    break;

                case 2: // Size
                    var op = _operatorCombo?.SelectedItem?.ToString() ?? "Greater";
                    var sizeOp = op switch
                    {
                        "Greater" => SizeOperator.Greater,
                        "Less" => SizeOperator.Less,
                        "Equal" => SizeOperator.Equal,
                        _ => SizeOperator.Greater
                    };
                    var unit = _unitCombo?.SelectedItem?.ToString() ?? "Bytes";
                    var sizeUnit = unit switch
                    {
                        "KB" => SizeUnit.KB,
                        "MB" => SizeUnit.MB,
                        "GB" => SizeUnit.GB,
                        _ => SizeUnit.Bytes
                    };
                    Result = new SizeCondition
                    {
                        Operator = sizeOp,
                        Value = long.TryParse(_valueBox?.Text, out var val) ? val : 0,
                        Unit = sizeUnit
                    };
                    break;
                case 3: // Item Type
                    var selectedType = _itemTypeCombo?.SelectedItem?.ToString() ?? "File";
                    var itemType = selectedType == "Directory" ? ItemType.Directory : ItemType.File;
                    Result = new ItemTypeCondition { Type = itemType };
                    break;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
