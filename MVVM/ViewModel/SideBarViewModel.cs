using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages;

namespace TextReplace.MVVM.ViewModel
{
    partial class SideBarViewModel : ObservableRecipient,
        IRecipient<ActiveContentViewMsg>
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReplaceViewCommand))]
        [NotifyCanExecuteChangedFor(nameof(SourcesViewCommand))]
        [NotifyCanExecuteChangedFor(nameof(PerformReplaceViewCommand))]
        private object _selectedView;

        public ReplaceViewModel ReplaceVm = new ReplaceViewModel();
        public SourcesViewModel SourcesVm = new SourcesViewModel();
        public OutputViewModel PerformReplaceVm = new OutputViewModel();

        public SideBarViewModel()
        {
            SelectedView = ReplaceVm;
            WeakReferenceMessenger.Default.Register(this);
        }

        [RelayCommand]
        private void ReplaceView()
        {
            WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(ReplaceVm));
        }

        [RelayCommand]
        private void SourcesView()
        {
            WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(SourcesVm));
        }

        [RelayCommand]
        private void PerformReplaceView()
        {
            WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(PerformReplaceVm));
        }

        public void Receive(ActiveContentViewMsg message)
        {
            SelectedView = message.Value;
        }
    }
}
