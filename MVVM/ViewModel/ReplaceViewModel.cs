using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.Enums;
using TextReplace.Core.Validation;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class ReplaceViewModel : ObservableRecipient,
        IRecipient<ReplaceFileNameMsg>,
        IRecipient<IsNewReplacementsFileMsg>,
        IRecipient<SetReplacePhrasesMsg>,
        IRecipient<SelectedPhraseMsg>,
        IRecipient<InsertReplacePhraseAtMsg>,
        IRecipient<IsReplaceFileUnsavedMsg>,
        IRecipient<AreReplacePhrasesSortedMsg>
    {
        [ObservableProperty]
        private string _fullFileName = ReplaceData.FileName;
        partial void OnFullFileNameChanged(string value)
        {
            FileName = Path.GetFileName(value);
            IsFileSelected = (value != string.Empty);
        }
        [ObservableProperty]
        private string _fileName = Path.GetFileName(ReplaceData.FileName);
        [ObservableProperty]
        private bool _isFileSelected = (ReplaceData.FileName != string.Empty);
        [ObservableProperty]
        private bool _isUnsaved = ReplaceData.IsUnsaved;
        public bool IsNewFile = ReplaceData.IsNewFile;

        [ObservableProperty]
        private ObservableCollection<ReplacePhrase> _replacePhrases =
            new ObservableCollection<ReplacePhrase>(ReplaceData.ReplacePhrasesDict.Select(x => new ReplacePhrase(x.Key, x.Value)));

        [ObservableProperty]
        private bool _sortReplacePhrases = ReplaceData.IsSorted;

        [ObservableProperty]
        private ReplacePhrase _selectedPhrase =
            new ReplacePhrase(ReplaceData.SelectedPhrase.Item1, ReplaceData.SelectedPhrase.Item2, true);
        partial void OnSelectedPhraseChanged(ReplacePhrase value)
        {
            ReplaceData.SelectedPhrase = !Equals(value, default(ReplacePhrase)) ?
                (value.Item1, value.Item2) :
                ("", "");
        }

        [ObservableProperty]
        private bool _isPhraseSelected = (ReplaceData.SelectedPhrase.Item1 != "");

        private InsertReplacePhraseAtEnum _insertReplacePhraseAt = InsertReplacePhraseAtEnum.Top;

        [ObservableProperty]
        private string _searchText = string.Empty;
        partial void OnSearchTextChanged(string value)
        {
            UpdateReplacePhrasesView(SelectedPhrase.Item1);
        }

        public RelayCommand ToggleSortCommand => new RelayCommand(ToggleSort);
        public RelayCommand<object> SetSelectedPhraseCommand => new RelayCommand<object>(SetSelectedPhrase);
        
        public ReplaceViewModel()
        {
            // highlight the previously selected phrase
            UpdateReplacePhrasesView(SelectedPhrase.Item1);
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        /// <summary>
        /// Toggle whether the phrases should be sorted and then update the replace phrases view
        /// </summary>
        private void ToggleSort()
        {
            ReplaceData.IsSorted = !ReplaceData.IsSorted;
            UpdateReplacePhrasesView(SelectedPhrase.Item1);
            ReplaceData.IsUnsaved = true;
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
        /// Saves the replace phrases list to the file system.
        /// </summary>
        /// <param name="newFileName"></param>
        /// <param name="newDelimiter"></param>
        /// <returns></returns>
        public bool SavePhrasesToFile(string? newFileName = null, string? newDelimiter = null)
        {
            string fileName = newFileName ?? FullFileName;

            // check if the file type is suppoerted
            if (FileValidation.IsReplaceFileTypeValid(fileName) == false)
            {
                Debug.WriteLine("File type is not supported, file not saved.");
                return false;
            }
            // check if the user has write perms to the directory
            if (FileValidation.IsDirectoryWritable(Path.GetDirectoryName(fileName) ?? "") == false)
            {
                Debug.WriteLine("Cannot write to directory, file not saved.");
                return false;
            }
            // if a new file is *not* being created, check if the user has write perms
            if (newFileName == null && FileValidation.IsInputFileReadWriteable(fileName) == false)
            {
                Debug.WriteLine("Cannot write to directory, file not saved.");
                return false;
            }

            try
            {
                ReplaceData.SavePhrasesToFile(fileName, SortReplacePhrases, newDelimiter);

                ReplaceData.IsUnsaved = false;
                return true;
            }
            // exception thrown if the file type is invalid
            catch (NotSupportedException e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public void AddNewPhrase(string item1, string item2)
        {
            int index = InsertReplacePhraseIndex();
            if (index == -1)
            {
                Debug.WriteLine("Selected phrase index could not be found, new phrase not added.");
                return;
            }

            bool res = ReplaceData.AddReplacePhrase(item1, item2, index);
            if (res == false)
            {
                Debug.WriteLine("New phrase could not be added.");
                return;
            }
            UpdateReplacePhrasesView(item1);
            ReplaceData.IsUnsaved = true;
        }

        /// <summary>
        /// Updates the second item in the replacement phrases of the selected phrase
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        public void EditSelectedPhrase(string item1, string item2)
        {
            if (item1 == string.Empty)
            {
                Debug.WriteLine("Item1 from EditSelectedPhrase() is empty.");
                return;
            }

            bool res;
            // if a new item1 was not specified, then swap out the old
            // ReplacePhrases.Item2 with the new item2 value
            if (item1 == SelectedPhrase.Item1)
            {
                res = ReplaceData.EditReplacePhrase(item1, item2);
            }
            // if a new item1 was specified, remove the old ReplacePhrases entry and
            // replace it with a new one with the specified item1 and item2
            else
            {
                res = ReplaceData.SwapReplacePhrase(SelectedPhrase.Item1, item1, item2);
            }

            if (res == false)
            {
                Debug.WriteLine("Phrase could not be edited.");
                return;
            }
            UpdateReplacePhrasesView(item1);
            ReplaceData.IsUnsaved = true;
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
            UpdateReplacePhrasesView("");
            ReplaceData.IsUnsaved = true;
        }

        /// <summary>
        /// Updates the replace phrases view by whether or not it should be sorted or
        /// if the user is searching for a specific phrase. Pass an empty string to deselect the phrase
        /// </summary>
        /// <param name="selectedItem1"></param>
        private void UpdateReplacePhrasesView(string selectedItem1)
        {
            // if there is no search text, display the replace phrases like normal
            if (SearchText == string.Empty)
            {
                ReplacePhrases = (SortReplacePhrases) ?
                    new ObservableCollection<ReplacePhrase>(GetReplacePhrases(selectedItem1).OrderBy(x => x.Item1)) :
                    new ObservableCollection<ReplacePhrase>(GetReplacePhrases(selectedItem1));
            }
            else
            {
                ReplacePhrases = (SortReplacePhrases) ?
                new ObservableCollection<ReplacePhrase>(GetReplacePhrases(selectedItem1)
                        .Where(x => {
                            return x.Item1.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   x.Item2.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                        })
                        .OrderBy(x => x.Item1)) :
                new ObservableCollection<ReplacePhrase>(GetReplacePhrases(selectedItem1)
                        .Where(x => {
                            return x.Item1.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   x.Item2.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                        }));

                // if the selected phrase is not in the search, clear the selected phrase
                if (ReplacePhrases.Any(x => x.Item1 == selectedItem1) == false)
                {
                    SelectedPhrase = new ReplacePhrase();
                }
            }
        }

        /// <summary>
        /// Converts ReplacePhrasesList into an enumerable of ReplacePhrase objects, marks the
        /// selected phrase if there is one, and updates SelectedPhrase.
        /// </summary>
        /// <returns>Returns ReplaceData.ReplacePhrases as an enumerable of ReplacePhrase objects</returns>
        private IEnumerable<ReplacePhrase> GetReplacePhrases(string selectedItem1 = "")
        {
            // simply get the replacement phrases if no selected phrase is specified
            if (selectedItem1 == string.Empty)
            {
                SelectedPhrase = new ReplacePhrase();
                return ReplaceData.ReplacePhrasesList.Select(x => new ReplacePhrase(x.Item1, x.Item2));
            }

            // if a selected phrase is specified, mark that specific phrase as selected
            return ReplaceData.ReplacePhrasesList.Select(x => {
                if (x.Item1 == selectedItem1)
                {
                    SelectedPhrase = new ReplacePhrase(x.Item1, x.Item2, true);
                    return new ReplacePhrase(x.Item1, x.Item2, true);
                }
                return new ReplacePhrase(x.Item1, x.Item2, false);
            });
        }

        /// <summary>
        /// Used to determine the index where a new phrase should be inserted into the list of
        /// replace phrases based off of the InsertReplacePhraseAtEnum value
        /// </summary>
        /// <returns>
        /// The where the phrase should be inserted,
        /// -1 if a wrong value is assigned to InsertReplacePhraseAt
        /// </returns>
        private int InsertReplacePhraseIndex()
        {
            return _insertReplacePhraseAt switch
            {
                InsertReplacePhraseAtEnum.Top => 0,
                InsertReplacePhraseAtEnum.Bottom => ReplacePhrases.Count,
                InsertReplacePhraseAtEnum.AboveSelection =>
                    ReplacePhrases.IndexOf(ReplacePhrases.Where(x => x.Item1 == SelectedPhrase.Item1).FirstOrDefault()),
                InsertReplacePhraseAtEnum.BelowSelection =>
                    ReplacePhrases.IndexOf(ReplacePhrases.Where(x => x.Item1 == SelectedPhrase.Item1).FirstOrDefault()) + 1,
                _ => -1,
            };
        }

        /// <summary>
        /// Wrapper for ReplaceData.SetNewReplaceFile.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newDelimiter"></param>
        /// <returns>False if new replace file was not set.</returns>
        public static bool SetNewReplaceFile(string fileName, string? newDelimiter = null)
        {
            return ReplaceData.SetNewReplaceFile(fileName, newDelimiter);
        }

        /// <summary>
        /// Wrapper for ReplaceData.CreateNewReplaceFile.
        /// </summary>
        public void CreateNewReplaceFile()
        {
            ReplaceData.IsUnsaved = false;
            ReplaceData.CreateNewReplaceFile();
        }

        // Message receivers

        public void Receive(ReplaceFileNameMsg message)
        {
            FullFileName = message.Value;
        }

        public void Receive(IsNewReplacementsFileMsg message)
        {
            IsNewFile = message.Value;
        }

        public void Receive(SetReplacePhrasesMsg message)
        {
            ReplacePhrases = new ObservableCollection<ReplacePhrase>(message.Value.Select(x => new ReplacePhrase(x.Item1, x.Item2)));
        }

        public void Receive(SelectedPhraseMsg message)
        {
            IsPhraseSelected = (message.Value.Item1 != "");
        }

        public void Receive(InsertReplacePhraseAtMsg message)
        {
            _insertReplacePhraseAt = message.Value;
        }

        public void Receive(IsReplaceFileUnsavedMsg message)
        {
            IsUnsaved = message.Value;
        }

        public void Receive(AreReplacePhrasesSortedMsg message)
        {
            SortReplacePhrases = message.Value;
        }
    }

    /// <summary>
    /// Struct representing a single replacement phrase from ReplaceData.ReplacePhrases.
    /// Item1 is the original phrase, Item2 is the replacement, isSelected is used to mark
    /// a selected phrase when updating the UI with UpdateReplacePhrasesView()
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
