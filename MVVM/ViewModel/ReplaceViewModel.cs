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
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

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
        
        private bool SortReplacePhrases = false;

        [ObservableProperty]
        private ReplacePhrase _selectedPhrase = new ReplacePhrase();
        partial void OnSelectedPhraseChanged(ReplacePhrase value)
        {
            WeakReferenceMessenger.Default.Send(new SelectedPhraseMsg( (value.Item1, value.Item2) ));
        }

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
            UpdateReplacePhrases();
        }


        public RelayCommand ToggleHasHeaderCommand => new RelayCommand(() => { ReplaceData.HasHeader = !ReplaceData.HasHeader; });
        public RelayCommand ToggleSortCommand => new RelayCommand(ToggleSort);
        public RelayCommand<object> SetSelectedPhraseCommand => new RelayCommand<object>(SetSelectedPhrase);

        public ReplaceViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        /// <summary>
        /// Wrapper function for ReplaceData.SetDelimiter
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns>Returns trie if delimiter was set, false otherwise.</returns>
        public bool SetDelimiter(string delimiter)
        {
            return ReplaceData.SetDelimiter(delimiter);
        }

        /// <summary>
        /// Toggle whether the phrases should be sorted and then update the replace phrases view
        /// </summary>
        private void ToggleSort()
        {
            SortReplacePhrases = !SortReplacePhrases;
            UpdateReplacePhrases();
        }

        /// <summary>
        /// Sets the selected phrase
        /// </summary>
        /// <param name="phrase"></param>
        private void SetSelectedPhrase(object? phrase)
        {
            // for some reason if i pass in the ReplacePhrase, this doesn't fire
            // on the first click, however if i pass it as a generic obect and
            // cast it to ReplacePhrase, it works. probably some MVVM community toolkit quirk
            if (phrase == null)
            {
                return;
            }
            ReplacePhrase p = (ReplacePhrase)phrase;
            p.IsSelected = true;
            SelectedPhrase = p;
        }

        /// <summary>
        /// Updates the second item in the replacement phrases of the selected phrase
        /// </summary>
        /// <param name="item2"></param>
        public void ChangeSelectedPhrase(string item2)
        {
            if (string.IsNullOrEmpty(SelectedPhrase.Item1))
            {
                Debug.WriteLine("Selected phrase is null");
                return;
            }

            ReplaceData.ReplacePhrases[SelectedPhrase.Item1] = item2;
            UpdateReplacePhrases(SelectedPhrase.Item1);
            SelectedPhrase = new ReplacePhrase(SelectedPhrase.Item1, item2, true);
        }

        /// <summary>
        /// Updates the replace phrases view by whether or not it should be sorted or
        /// if the user is searching for a specific phrase
        /// </summary>
        /// <param name="selectedPhrase"></param>
        private void UpdateReplacePhrases(string selectedPhrase = "")
        {
            // if there is no search text, display the replace phrases like normal
            if (SearchText == string.Empty)
            {
                ReplacePhrases = (SortReplacePhrases) ?
                    new ObservableCollection<ReplacePhrase>(GetReplacePhrases(selectedPhrase).OrderBy(x => x.Item1)) :
                    new ObservableCollection<ReplacePhrase>(GetReplacePhrases(selectedPhrase));
            }
            else
            {
                ReplacePhrases = (SortReplacePhrases) ?
                new ObservableCollection<ReplacePhrase>(GetReplacePhrases(selectedPhrase)
                        .Where(x => {
                            return x.Item1.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   x.Item2.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                        })
                        .OrderBy(x => x.Item1)) :
                new ObservableCollection<ReplacePhrase>(GetReplacePhrases(selectedPhrase)
                        .Where(x => {
                            return x.Item1.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   x.Item2.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                        }));
            }
        }

        /// <summary>
        /// Utility function used by UpdatedReplacePhrases() to cut down on repeat code
        /// </summary>
        /// <returns>Returns ReplaceData.ReplacePhrases as an enumerable of ReplacePhrase objects</returns>
        private IEnumerable<ReplacePhrase> GetReplacePhrases(string selectedPhrase = "")
        {
            // simply get the replacement phrases if no selected phrase is specified
            if (selectedPhrase == string.Empty)
            {
                return ReplaceData.ReplacePhrases.Select(x => new ReplacePhrase(x.Key, x.Value));
            }

            // if a selected phrase is specified, mark that specific phrase as selected
            return ReplaceData.ReplacePhrases.Select(x => {

                return (x.Key == selectedPhrase) ?
                    new ReplacePhrase(x.Key, x.Value, true) :
                    new ReplacePhrase(x.Key, x.Value, false);
            });
        }

        // Message receivers

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
    struct ReplacePhrase(string item1, string item2, bool isSelected = false)
    {
        public string Item1 { get; set; } = item1;
        public string Item2 { get; set; } = item2;
        public bool IsSelected { get; set; } = isSelected;
    }

    struct ComboBoxData(int index, string value)
    {
        public int Index { get; set; } = index;
        public string Value { get; set; } = value;
    }
}
