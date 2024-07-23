using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages;
using TextReplace.Messages.Output;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class MainViewModel : ObservableRecipient,
        IRecipient<ActiveContentViewMsg>,
        IRecipient<ReplacementInProgressMsg>,
        IRecipient<SavingReplacementsInProgressMsg>
    {
        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private object _topBarView;

        [ObservableProperty]
        private object _sideBarView;

        public ReplaceViewModel ReplaceVm = new ReplaceViewModel();
        public TopBarViewModel TopBarVm = new TopBarViewModel();
        public SideBarViewModel SideBarVm = new SideBarViewModel();

        private bool _isReplacementInProgress = OutputData.IsReplacementInProgress;
        public bool IsReplacementInProgress
        {
            get { return _isReplacementInProgress; }
            set { _isReplacementInProgress = value; }
        }

        private bool _isSavingReplacementsInProgress = ReplaceData.IsSavingReplacementsInProgress;
        public bool IsSavingReplacementsInProgress
        {
            get { return _isSavingReplacementsInProgress; }
            set { _isSavingReplacementsInProgress = value; }
        }


        public MainViewModel()
        {
            // set the home view as default
            _currentView = ReplaceVm;

            _topBarView = TopBarVm;
            _sideBarView = SideBarVm;
        }

        protected override void OnActivated()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public static void SaveUserSettings()
        {
            OutputData.UserSettings.WholeWord = OutputData.WholeWord;
            OutputData.UserSettings.CaseSensitive = OutputData.CaseSensitive;
            OutputData.UserSettings.PreserveCase = OutputData.PreserveCase;
            OutputData.UserSettings.OpenFileLocation = OutputData.OpenFileLocation;

            OutputData.UserSettings.Styling.Bold = OutputData.OutputFilesStyling.Bold;
            OutputData.UserSettings.Styling.Italics = OutputData.OutputFilesStyling.Italics;
            OutputData.UserSettings.Styling.Underline = OutputData.OutputFilesStyling.Underline;
            OutputData.UserSettings.Styling.Strikethrough = OutputData.OutputFilesStyling.Strikethrough;
            OutputData.UserSettings.Styling.IsHighlighted = OutputData.OutputFilesStyling.IsHighlighted;
            OutputData.UserSettings.Styling.IsTextColored = OutputData.OutputFilesStyling.IsTextColored;
            OutputData.UserSettings.Styling.HighlightColor = OutputData.OutputFilesStyling.HighlightColor.ToString();
            OutputData.UserSettings.Styling.TextColor = OutputData.OutputFilesStyling.TextColor.ToString();
        }

        public void Receive(ActiveContentViewMsg message)
        {
            CurrentView = message.Value;
        }

        public void Receive(ReplacementInProgressMsg message)
        {
            IsReplacementInProgress = message.Value;
        }

        public void Receive(SavingReplacementsInProgressMsg message)
        {
            IsSavingReplacementsInProgress = message.Value;
        }
    }
}
