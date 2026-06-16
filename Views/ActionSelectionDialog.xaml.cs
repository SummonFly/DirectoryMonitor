using DirectoryMonitor.Helpers;
using System.Windows;

namespace DirectoryMonitor.Views
{
    /// <summary>
    /// Логика взаимодействия для ActionSelectionDialog.xaml
    /// </summary>
    public partial class ActionSelectionDialog : Window
    {
        private List<ActionSelectionItem> _actions;

        public List<int> SelectedActionIds { get; private set; } = new();

        public ActionSelectionDialog(List<ActionSelectionItem> actions)
        {
            InitializeComponent();
            _actions = actions;
            ActionsListBox.ItemsSource = actions;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedActionIds = ActionsListBox.SelectedItems
                .Cast<ActionSelectionItem>()
                .Select(a => a.Id)
                .ToList();

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
