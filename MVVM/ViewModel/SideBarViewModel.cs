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
        private SelectedViewEnum _selectedView = SelectedViewEnum.ReplaceView;

        public static RelayCommand ReplaceViewCommand => new RelayCommand(ReplaceView);
        public static RelayCommand SourcesViewCommand => new RelayCommand(SourcesView);
        public static RelayCommand OutputViewCommand => new RelayCommand(OutputView);

        public static bool isRegistered = false;

        public SideBarViewModel()
        {
            if (isRegistered == false)
            {
                WeakReferenceMessenger.Default.Register(this);
            }
        }

        public static void ReplaceView()
        {
            WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(new ReplaceViewModel()));
        }

        public static void SourcesView()
        {
            WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(new SourcesViewModel()));
        }

        public static void OutputView()
        {
            WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(new OutputViewModel()));
        }

        public void Receive(ActiveContentViewMsg message)
        {
            SelectedView = message.Value switch
            {
                ReplaceViewModel => SelectedViewEnum.ReplaceView,
                SourcesViewModel => SelectedViewEnum.SourcesView,
                OutputViewModel => SelectedViewEnum.OutputView,
                _ => throw new NotImplementedException("SelectedViewEnum does not contain the class supplied by ActiveContentViewMsg")
            };
        }
    }

    public enum SelectedViewEnum
    {
        ReplaceView,
        SourcesView,
        OutputView
    }
}
