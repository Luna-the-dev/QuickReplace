using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class ReplaceViewModel : ObservableRecipient,
        IRecipient<FileNameMsg>,
        IRecipient<HasHeaderMsg>,
        IRecipient<DelimiterMsg>,
        IRecipient<SetReplacePhrasesMsg>
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

        [ObservableProperty]
        private ObservableCollection<ReplacePhrase> _replacePhrases =
            new ObservableCollection<ReplacePhrase>(ReplaceData.ReplacePhrases.Select(x => new ReplacePhrase(x.Key, x.Value)));
        [ObservableProperty]
        private Visibility _isUnsortedPhrasesVisible = Visibility.Visible;

        [ObservableProperty]
        private ObservableCollection<ReplacePhrase> _replacePhrasesSorted =
            new ObservableCollection<ReplacePhrase>(ReplaceData.ReplacePhrases.OrderBy(x => x.Key).Select(x => new ReplacePhrase(x.Key, x.Value)));
        [ObservableProperty]
        private Visibility _isSortedPhrasesVisible = Visibility.Hidden;

        [ObservableProperty]
        private Visibility _isEditGUIVisible = Visibility.Visible;
        [ObservableProperty]
        private Visibility _isAddGUIVisible = Visibility.Hidden;

        [ObservableProperty]
        private int _replaceViewFunction = 0;
        partial void OnReplaceViewFunctionChanged(int value)
        {
            if (value == 0)
            {
                IsEditGUIVisible = Visibility.Visible;
                IsAddGUIVisible = Visibility.Hidden;
            }
            else if (value == 1)
            {
                IsEditGUIVisible = Visibility.Hidden;
                IsAddGUIVisible = Visibility.Visible;
            }
        }

        [ObservableProperty]
        private string _searchText = string.Empty;
        partial void OnSearchTextChanged(string value)
        {
            if (value == string.Empty)
            {
                return;
            }

        }


        public RelayCommand ToggleHasHeaderCommand => new RelayCommand(() => { ReplaceData.HasHeader = !ReplaceData.HasHeader; });
        public RelayCommand ToggleSortCommand => new RelayCommand(ToggleSort);

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

        private void ToggleSort()
        {
            IsUnsortedPhrasesVisible = (IsUnsortedPhrasesVisible == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
            IsSortedPhrasesVisible = (IsSortedPhrasesVisible == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
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

        public void Receive(SetReplacePhrasesMsg message)
        {
            ReplacePhrases = new ObservableCollection<ReplacePhrase>(message.Value.Select(x => new ReplacePhrase(x.Key, x.Value)));
        }
    }

    /// <summary>
    /// Struct representing a single replacement phrase from ReplaceData.ReplacePhrases.
    /// Item1 is the original phrase, Item2 is the replacement
    /// </summary>
    /// <param name="item1"></param>
    /// <param name="item2"></param>
    struct ReplacePhrase(string item1, string item2)
    {
        public string Item1 { get; set; } = item1;
        public string Item2 { get; set; } = item2;
    }

    struct ComboBoxData(int index, string value)
    {
        public int Index { get; set; } = index;
        public string Value { get; set; } = value;
    }
}
