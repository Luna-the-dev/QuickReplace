using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DocumentFormat.OpenXml.EMMA;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.Enums;
using TextReplace.Core.Validation;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    public partial class ReplaceViewModel : ObservableRecipient,
        IRecipient<ReplaceFileNameMsg>,
        IRecipient<IsNewReplacementsFileMsg>,
        IRecipient<ReplacePhrasesMsg>,
        IRecipient<SelectedReplacePhraseMsg>,
        IRecipient<IsReplaceFileUnsavedMsg>,
        IRecipient<AreReplacePhrasesSortedMsg>,
        IDropTarget
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
        private ObservableCollection<ReplacePhraseWrapper> _replacePhrases =
            new ObservableCollection<ReplacePhraseWrapper>(ReplaceData.ReplacePhrasesList.Select(ReplacePhraseWrapper.WrapReplacePhrase));
        partial void OnReplacePhrasesChanged(ObservableCollection<ReplacePhraseWrapper> value)
        {
            DoesReplacePhraseExist = value.Count > 0;
        }

        [ObservableProperty]
        private bool _doesReplacePhraseExist = (ReplaceData.ReplacePhrasesList.Count > 0);

        [ObservableProperty]
        private bool _sortReplacePhrases = ReplaceData.IsSorted;

        [ObservableProperty]
        private ReplacePhraseWrapper _selectedPhrase = new ReplacePhraseWrapper();
        partial void OnSelectedPhraseChanged(ReplacePhraseWrapper value)
        {
            IsPhraseSelected = value.Item1 != string.Empty;
        }

        [ObservableProperty]
        private bool _isPhraseSelected = (ReplaceData.SelectedPhrase.Item1 != "");

        [ObservableProperty]
        private string _searchText = string.Empty;
        partial void OnSearchTextChanged(string value)
        {
            UpdateReplacePhrasesView(SelectedPhrase.Item1);
        }

        public RelayCommand ToggleSortCommand => new RelayCommand(ToggleSort);
        public RelayCommand<object> SetSelectedPhraseCommand => new RelayCommand<object>(SetSelectedPhrase);

        public static bool isRegistered = false;
        
        public ReplaceViewModel()
        {
            if (isRegistered == false)
            {
                WeakReferenceMessenger.Default.RegisterAll(this);
            }
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
            ReplacePhraseWrapper p = (ReplacePhraseWrapper)phrase;
            ReplaceData.SelectedPhrase = ReplacePhraseWrapper.UnwrapReplacePhrase(p);
        }

        /// <summary>
        /// Saves the replace phrases list to the file system.
        /// </summary>
        /// <param name="newFileName"></param>
        /// <param name="newDelimiter"></param>
        /// <returns></returns>
        public bool SavePhrasesToFile(string? newFileName = null, string newDelimiter = "")
        {
            string fileName = newFileName ?? FullFileName;

            string delimiter = Path.GetExtension(fileName).ToLower() switch
            {
                ".csv" => ",",
                ".tsv" => "\t",
                ".txt" => newDelimiter,
                ".xlsx" => string.Empty,
                _ => throw new NotSupportedException($"SavePhrasesToFile(): The file type is not supported.")
            };

            // check if the file type is suppoerted
            if (FileValidation.IsReplaceFileTypeValid(fileName) == false)
            {
                Debug.WriteLine("File type is not supported, file not saved.");
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
                ReplaceData.SavePhrasesToFile(fileName, SortReplacePhrases, delimiter);

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

        /// <summary>
        /// Adds a new replace phrase to the ReplacePhrases List/Dict, and updates the SelectedPhrase to this phrase.
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <param name="insertAt"></param>
        /// <returns>True is successful, false if something went wrong.</returns>
        public bool AddNewPhrase(string item1, string item2, InsertReplacePhraseAtEnum insertAt)
        {
            int index = InsertReplacePhraseIndex(insertAt);
            if (index == -1)
            {
                Debug.WriteLine("Selected phrase index could not be found, new phrase not added.");
                return false;
            }

            bool res = ReplaceData.AddReplacePhrase(item1, item2, index);
            if (res == false)
            {
                Debug.WriteLine("New phrase could not be added.");
                return false;
            }
            UpdateReplacePhrasesView(item1);
            ReplaceData.IsUnsaved = true;
            return true;
        }

        /// <summary>
        /// Updates the second item in the replacement phrases of the selected phrase
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns>True is successful, false if something went wrong.</returns>
        public bool EditSelectedPhrase(string item1, string item2)
        {
            if (item1 == string.Empty)
            {
                Debug.WriteLine("Item1 from EditSelectedPhrase() is empty.");
                return false;
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
                return false;
            }
            ReplaceData.SelectedPhrase = new ReplacePhrase(item1, item2);
            UpdateReplacePhrasesView(item1);
            ReplaceData.IsUnsaved = true;
            return true;
        }

        /// <summary>
        /// Removes the selected phrase from the ReplacePhrases List/Dict
        /// </summary>
        /// <returns>True is successful, false if something went wrong.</returns>
        public bool RemoveSelectedPhrase()
        {
            if (string.IsNullOrEmpty(SelectedPhrase.Item1))
            {
                Debug.WriteLine("Selected phrase is empty");
                return false;
            }

            bool res = ReplaceData.RemoveReplacePhrase(SelectedPhrase.Item1);
            if (res == false)
            {
                Debug.WriteLine("Phrase could not be removed.");
                return false;
            }
            ReplaceData.SelectedPhrase = new ReplacePhrase();
            UpdateReplacePhrasesView("");
            ReplaceData.IsUnsaved = true;
            return true;
        }

        /// <summary>
        /// Removes all phrases from the ReplacePhrases List/Dict
        /// </summary>
        public static void RemoveAllPhrases()
        {
            ReplaceData.RemoveAllReplacePhrases();
        }

        /// <summary>
        /// Moves a replace phrase in the ReplacePhrasesList from its current position to a new index
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        public void MoveReplacePhrase(int oldIndex, int newIndex)
        {
            // the item was dropped into the same position was it was in before. do nothing
            if (newIndex == oldIndex || newIndex == oldIndex + 1)
            {
                return;
            }

            ReplaceData.MoveReplacePhrase(oldIndex, newIndex);
            UpdateReplacePhrasesView(SelectedPhrase.Item1);
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
                    new ObservableCollection<ReplacePhraseWrapper>(
                        ReplaceData.ReplacePhrasesList.Select(ReplacePhraseWrapper.WrapReplacePhrase).OrderBy(x => x.Item1)) :
                    new ObservableCollection<ReplacePhraseWrapper>(
                        ReplaceData.ReplacePhrasesList.Select(ReplacePhraseWrapper.WrapReplacePhrase));
            }
            else
            {
                ReplacePhrases = (SortReplacePhrases) ?
                    new ObservableCollection<ReplacePhraseWrapper>(
                        ReplaceData.ReplacePhrasesList.Select(ReplacePhraseWrapper.WrapReplacePhrase)
                        .Where(x => {
                            return x.Item1.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   x.Item2.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                        })
                        .OrderBy(x => x.Item1)) :
                    new ObservableCollection<ReplacePhraseWrapper>(
                        ReplaceData.ReplacePhrasesList.Select(ReplacePhraseWrapper.WrapReplacePhrase)
                        .Where(x => {
                            return x.Item1.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   x.Item2.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                        }));
            }

            // if the selected phrase is not in the search, clear the selected phrase
            if (ReplacePhrases.Any(x => x.Item1 == selectedItem1) == false)
            {
                ReplaceData.SelectedPhrase = new ReplacePhrase();
            }

            DoesReplacePhraseExist = ReplacePhrases.Count > 0;
        }

        /// <summary>
        /// Used to determine the index where a new phrase should be inserted into the list of
        /// replace phrases based off of the InsertReplacePhraseAtEnum value
        /// </summary>
        /// <returns>
        /// The where the phrase should be inserted,
        /// -1 if a wrong value is assigned to InsertReplacePhraseAt
        /// </returns>
        private int InsertReplacePhraseIndex(InsertReplacePhraseAtEnum insertAt)
        {
            return insertAt switch
            {
                InsertReplacePhraseAtEnum.Top => 0,
                InsertReplacePhraseAtEnum.Bottom => ReplacePhrases.Count,
                InsertReplacePhraseAtEnum.AboveSelection =>
                    ReplaceData.ReplacePhrasesList.FindIndex(x => x.Item1 == SelectedPhrase.Item1),
                InsertReplacePhraseAtEnum.BelowSelection =>
                    ReplaceData.ReplacePhrasesList.FindIndex(x => x.Item1 == SelectedPhrase.Item1) + 1,
                _ => -1,
            };
        }

        /// <summary>
        /// Wrapper for ReplaceData.SetNewReplaceFile.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>False if new replace file was not set.</returns>
        public static bool SetNewReplaceFile(string fileName)
        {
            return ReplaceData.SetNewReplaceFile(fileName);
        }

        /// <summary>
        /// Wrapper for ReplaceData.CreateNewReplaceFile.
        /// </summary>
        public void CreateNewReplaceFile()
        {
            ReplaceData.IsUnsaved = false;
            ReplaceData.CreateNewReplaceFile();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = System.Windows.DragDropEffects.Copy;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var droppedItem = (ReplacePhraseWrapper)dropInfo.Data;

            // grab the old index of the replace phrase
            int oldIndex = ReplacePhrases.IndexOf(droppedItem);
            if (oldIndex < 0)
            {
                Debug.WriteLine("Dropped item does not exist.");
                return;
            }

            MoveReplacePhrase(oldIndex, dropInfo.InsertIndex);
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

        public void Receive(ReplacePhrasesMsg message)
        {
            ReplacePhrases = new ObservableCollection<ReplacePhraseWrapper>(message.Value.Select(ReplacePhraseWrapper.WrapReplacePhrase));
            UpdateReplacePhrasesView(ReplaceData.SelectedPhrase.Item1);
        }

        public void Receive(SelectedReplacePhraseMsg message)
        {
            SelectedPhrase = ReplacePhraseWrapper.WrapReplacePhrase(message.Value);
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
    public class ReplacePhraseWrapper
    {
        public string Item1 { get; set; }
        public string Item2 { get; set; }
        public bool IsSelected { get; set; }

        public ReplacePhraseWrapper()
        {
            Item1 = string.Empty;
            Item2 = string.Empty;
            IsSelected = false;
        }

        public ReplacePhraseWrapper(string item1, string item2, bool isSelected = false)
        {
            Item1 = item1;
            Item2 = item2;
            IsSelected = isSelected;
        }

        public ReplacePhraseWrapper(ReplacePhrase phrase, bool isSelected = false)
        {
            Item1 = phrase.Item1;
            Item2 = phrase.Item2;
            IsSelected = isSelected;
        }

        public static ReplacePhraseWrapper WrapReplacePhrase(ReplacePhrase phrase)
        {
            if (phrase.Item1 == ReplaceData.SelectedPhrase.Item1)
            {
                return new ReplacePhraseWrapper(phrase, true);
            }
            return new ReplacePhraseWrapper(phrase);
        }

        public static ReplacePhrase UnwrapReplacePhrase(ReplacePhraseWrapper phrase)
        {
            return new ReplacePhrase(phrase.Item1, phrase.Item2);
        }
    }

    struct ComboBoxData(int index, string value)
    {
        public int Index { get; set; } = index;
        public string Value { get; set; } = value;
    }
}
