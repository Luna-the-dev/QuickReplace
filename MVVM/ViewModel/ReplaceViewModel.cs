using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class ReplaceViewModel : ObservableRecipient,
        IRecipient<FileNameMsg>,
        IRecipient<HasHeaderMsg>,
        IRecipient<DelimiterMsg>
    {
        [ObservableProperty]
        private string _fileName = string.Empty;
        partial void OnFileNameChanged(string value)
        {
            IsDefaultFileNameVisible = (value == string.Empty) ? Visibility.Visible : Visibility.Hidden;
            IsFileNameVisible =        (value == string.Empty) ? Visibility.Hidden : Visibility.Visible;
        }

        [ObservableProperty]
        private Visibility _isDefaultFileNameVisible = Visibility.Visible;
        [ObservableProperty]
        private Visibility _isFileNameVisible = Visibility.Hidden;

        [ObservableProperty]
        private Visibility _hasHeader = (ReplaceData.HasHeader) ? Visibility.Visible : Visibility.Hidden;
        [ObservableProperty]
        private string _delimiter = ReplaceData.Delimiter;


        public RelayCommand ToggleHasHeaderCommand => new RelayCommand(() => { ReplaceData.HasHeader = !ReplaceData.HasHeader; });

        public ReplaceViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        /// <summary>
        /// Wrapper function for ReplaceData.SetDelimiter
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public bool SetDelimiter(string delimiter)
        {
            return ReplaceData.SetDelimiter(delimiter);
        }

        public void Receive(FileNameMsg message)
        {
            FileName = message.Value;
        }

        public void Receive(HasHeaderMsg message)
        {
            HasHeader = (message.Value) ? Visibility.Visible : Visibility.Hidden;
        }

        public void Receive(DelimiterMsg message)
        {
            Delimiter = message.Value;
        }
    }
}
