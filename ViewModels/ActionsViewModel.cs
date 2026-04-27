using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectoryMonitor.Data.Repositories;
using DirectoryMonitor.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace DirectoryMonitor.ViewModels
{
    public partial class ActionsViewModel : ObservableObject
    {
        private readonly IActionRepository _actionRepository;

        [ObservableProperty]
        private ObservableCollection<Models.Entities.Action> _actions = new();

        [ObservableProperty]
        private Models.Entities.Action? _selectedAction;

        public ActionsViewModel(IActionRepository actionRepository)
        {
            _actionRepository = actionRepository;

            LoadActionsCommand = new AsyncRelayCommand(LoadActionsAsync);
            AddActionCommand = new AsyncRelayCommand(AddActionAsync);
            EditActionCommand = new AsyncRelayCommand(EditActionAsync, () => SelectedAction != null);
            DeleteActionCommand = new AsyncRelayCommand(DeleteActionAsync, () => SelectedAction != null);

            LoadActionsCommand.Execute(null);
        }

        public IAsyncRelayCommand LoadActionsCommand { get; }
        public IAsyncRelayCommand AddActionCommand { get; }
        public IAsyncRelayCommand EditActionCommand { get; }
        public IAsyncRelayCommand DeleteActionCommand { get; }

        partial void OnSelectedActionChanged(Models.Entities.Action? value)
        {
            EditActionCommand.NotifyCanExecuteChanged();
            DeleteActionCommand.NotifyCanExecuteChanged();
        }

        private async Task LoadActionsAsync()
        {
            var actions = await _actionRepository.GetAllAsync();
            Actions.Clear();
            foreach (var action in actions)
            {
                Actions.Add(action);
            }
        }

        private async Task AddActionAsync()
        {
            var dialog = new ActionEditorWindow();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                var newAction = dialog.GetAction();
                await _actionRepository.AddAsync(newAction);
                await LoadActionsAsync();
            }
        }

        private async Task EditActionAsync()
        {
            if (SelectedAction == null) return;

            var dialog = new ActionEditorWindow(SelectedAction);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                var updatedAction = dialog.GetAction();
                updatedAction.Id = SelectedAction.Id;
                updatedAction.CreatedAt = SelectedAction.CreatedAt;
                await _actionRepository.UpdateAsync(updatedAction);
                await LoadActionsAsync();
            }
        }

        private async Task DeleteActionAsync()
        {
            if (SelectedAction == null) return;

            var result = MessageBox.Show($"Delete action '{SelectedAction.Name}'?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _actionRepository.DeleteAsync(SelectedAction.Id);
                await LoadActionsAsync();
            }
        }
    }
}
