using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TextReplace.Core.Enums;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class ReplaceViewModel : ObservableRecipient,
        IRecipient<FileNameMsg>,
        IRecipient<HasHeaderMsg>,
        IRecipient<DelimiterMsg>,
        IRecipient<SetReplacePhrasesMsg>,
        IRecipient<IsPhraseSelectedMsg>,
        IRecipient<InsertReplacePhraseAtMsg>
    {
        [ObservableProperty]
        private string _fileName = string.Empty;
        partial void OnFileNameChanged(string value)
        {
            IsFileSelected = (value != string.Empty);
        }
        [ObservableProperty]
        private bool _isFileSelected = (ReplaceData.FileName != string.Empty);

        [ObservableProperty]
        private bool _hasHeader = ReplaceData.HasHeader;
        partial void OnHasHeaderChanged(bool value)
        {
            ReplaceData.HasHeader = HasHeader;
        }

        [ObservableProperty]
        private string _delimiter = ReplaceData.Delimiter;

        [ObservableProperty]
        private ObservableCollection<ReplacePhrase> _replacePhrases =
            new ObservableCollection<ReplacePhrase>(ReplaceData.ReplacePhrasesDict.Select(x => new ReplacePhrase(x.Key, x.Value)));
        
        private bool SortReplacePhrases = false;

        [ObservableProperty]
        private ReplacePhrase _selectedPhrase = new ReplacePhrase();
        partial void OnSelectedPhraseChanged(ReplacePhrase value)
        {
            ReplaceData.IsPhraseSelected = !Equals(value, default(ReplacePhrase));
        }

        [ObservableProperty]
        private bool _isPhraseSelected = ReplaceData.IsPhraseSelected;

        private InsertReplacePhraseAtEnum _insertReplacePhraseAt = InsertReplacePhraseAtEnum.Top;

        [ObservableProperty]
        private string _searchText = string.Empty;
        partial void OnSearchTextChanged(string value)
        {
            UpdateReplacePhrases(SelectedPhrase.Item1);
        }


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
            UpdateReplacePhrases(SelectedPhrase.Item1);
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

        public void AddNewPhrase(string item1, string item2)
        {
            int index = InsertReplacePhraseIndex();
            if (index == -1)
            {
                Debug.WriteLine("Selected phrase index could not be found, new phrase not added.");
            }

            bool res = ReplaceData.AddReplacePhrase(item1, item2, index);
            if (res == false)
            {
                Debug.WriteLine("New phrase could not be added.");
                return;
            }
            UpdateReplacePhrases(item1);
        }

        /// <summary>
        /// Updates the second item in the replacement phrases of the selected phrase
        /// </summary>
        /// <param name="item2"></param>
        public void EditSelectedPhrase(string item2)
        {
            if (string.IsNullOrEmpty(SelectedPhrase.Item1))
            {
                Debug.WriteLine("Selected phrase is null");
                return;
            }

            bool res = ReplaceData.EditReplacePhrase(SelectedPhrase.Item1, item2);
            if (res == false)
            {
                Debug.WriteLine("Phrase could not be edited.");
                return;
            }
            UpdateReplacePhrases(SelectedPhrase.Item1);
        }

        public void RemoveSelectedPhrase()
        {
            if (string.IsNullOrEmpty(SelectedPhrase.Item1))
            {
                Debug.WriteLine("Selected phrase is null");
                return;
            }

            bool res = ReplaceData.RemoveReplacePhrase(SelectedPhrase.Item1);
            if (res == false)
            {
                Debug.WriteLine("Phrase could not be removed.");
                return;
            }
            UpdateReplacePhrases("");
        }

        /// <summary>
        /// Updates the replace phrases view by whether or not it should be sorted or
        /// if the user is searching for a specific phrase. Pass an empty or null
        /// string to remove the selected phrase
        /// </summary>
        /// <param name="selectedPhrase"></param>
        private void UpdateReplacePhrases(string selectedPhrase)
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
        /// Converts ReplacePhrasesList into an enumerable of ReplacePhrase objects, marks the
        /// selected phrase if there is one, and updates SelectedPhrase.
        /// </summary>
        /// <returns>Returns ReplaceData.ReplacePhrases as an enumerable of ReplacePhrase objects</returns>
        private IEnumerable<ReplacePhrase> GetReplacePhrases(string selectedPhrase = "")
        {
            // simply get the replacement phrases if no selected phrase is specified
            if (string.IsNullOrEmpty(selectedPhrase))
            {
                SelectedPhrase = new ReplacePhrase();
                return ReplaceData.ReplacePhrasesList.Select(x => new ReplacePhrase(x.Item1, x.Item2));
            }

            // if a selected phrase is specified, mark that specific phrase as selected
            return ReplaceData.ReplacePhrasesList.Select(x => {
                if (x.Item1 == selectedPhrase)
                {
                    SelectedPhrase = new ReplacePhrase(x.Item1, x.Item2, true);
                    return new ReplacePhrase(x.Item1, x.Item2, true);
                }
                return new ReplacePhrase(x.Item1, x.Item2, false);
            });
        }
        
        private int InsertReplacePhraseIndex()
        {
            switch (_insertReplacePhraseAt)
            {
                case InsertReplacePhraseAtEnum.Top:
                    return 0;
                case InsertReplacePhraseAtEnum.Bottom:
                    return ReplacePhrases.Count;
                case InsertReplacePhraseAtEnum.AboveSelection:
                    return ReplacePhrases.IndexOf(ReplacePhrases.Where(x => x.Item1 == SelectedPhrase.Item1).FirstOrDefault());
                case InsertReplacePhraseAtEnum.BelowSelection:
                    return ReplacePhrases.IndexOf(ReplacePhrases.Where(x => x.Item1 == SelectedPhrase.Item1).FirstOrDefault()) + 1;
                default:
                    return -1;
            }
        }

        // Message receivers

        public void Receive(FileNameMsg message)
        {
            FileName = message.Value;
        }

        public void Receive(HasHeaderMsg message)
        {
            HasHeader = message.Value;
        }

        public void Receive(DelimiterMsg message)
        {
            Delimiter = message.Value;
        }

        public void Receive(SetReplacePhrasesMsg message)
        {
            ReplacePhrases = new ObservableCollection<ReplacePhrase>(message.Value.Select(x => new ReplacePhrase(x.Item1, x.Item2)));
        }

        public void Receive(IsPhraseSelectedMsg message)
        {
            IsPhraseSelected = message.Value;
        }

        public void Receive(InsertReplacePhraseAtMsg message)
        {
            _insertReplacePhraseAt = message.Value;
        }
    }

    /// <summary>
    /// Struct representing a single replacement phrase from ReplaceData.ReplacePhrases.
    /// Item1 is the original phrase, Item2 is the replacement, isSelected is used to mark
    /// a selected phrase when updating the UI with UpdateReplacePhrases()
    /// </summary>
    /// <param name="item1"></param>
    /// <param name="item2"></param>
    /// <param name="isSelected"></param>
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
