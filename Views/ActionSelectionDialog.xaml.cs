using DirectoryMonitor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
