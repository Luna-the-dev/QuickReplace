using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Windows;
using TextReplace.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages.Replace;
using System.IO;

namespace TextReplace.MVVM.ViewModel
{
    partial class TopBarViewModel : ObservableObject,
        IRecipient<HasHeaderMsg>,
        IRecipient<DelimiterMsg>
    {
        [ObservableProperty]
        private Visibility _hasHeader = (ReplaceData.HasHeader) ? Visibility.Visible : Visibility.Hidden;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ToggleCaseSensitiveCommand))]
        private Visibility _caseSensitive = Visibility.Hidden;

        [ObservableProperty]
        private string _delimiter = ReplaceData.Delimiter;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ToggleWholeWordCommand))]
        private Visibility _wholeWord = Visibility.Hidden;

        [ObservableProperty]
        private string _suffix = string.Empty;
        partial void OnSuffixChanging(string value)
        {
            SourceFilesData.Suffix = value;
        }

        // visibility flags for top bar components
        [ObservableProperty]
        private Visibility _replaceFileReadSuccess = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _replaceFileReadFail = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _sourceFileReadSuccess = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _sourceFileReadFail = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _replaceisClickable = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _replaceisUnclickable = Visibility.Visible;

        private const string INVALID_SUFFIX_CHARS = "<>:\"/\\|?*\n\t";

        // commands
        public RelayCommand ReplaceFile => new RelayCommand(ReplaceFileCmd);
        public RelayCommand SourceFiles => new RelayCommand(SourceFilesCmd);
        public RelayCommand Replace => new RelayCommand(ReplaceCmd);
        public RelayCommand ToggleHasHeaderCommand => new RelayCommand(() => { ReplaceData.HasHeader = !ReplaceData.HasHeader; });
        public RelayCommand ChangeOutputDirectory => new RelayCommand(ChangeOutputDirectoryCmd);

        public TopBarViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void ReplaceFileCmd()
        {
            // open a file dialogue for the user and update the replace file
            bool? result = ReplaceData.SetNewReplaceFileFromUser();

            if (result == true)
            {
                Debug.Write("Replace file name:\t");
                Debug.WriteLine(ReplaceData.FileName);
                ReplaceFileReadSuccess = Visibility.Visible;
                ReplaceFileReadFail = Visibility.Hidden;
            }
            else if (result == false) 
            {
                Debug.WriteLine("ReplaceFile either could not be read or parsed.");
                ReplaceFileReadSuccess = Visibility.Hidden;
                ReplaceFileReadFail = Visibility.Visible;
            }

            WeakReferenceMessenger.Default.Send(new FileNameMsg(Path.GetFileName(ReplaceData.FileName)));

            SetReplaceButtonClickability();
        }

        private void SourceFilesCmd()
        {
            // open a file dialogue for the user and update the source files
            bool? result = SourceFilesData.SetNewSourceFilesFromUser();

            if (result == true)
            {
                Debug.Write("Source file name(s):");
                SourceFilesData.FileNames.ForEach(i => Debug.WriteLine($"\t{i}"));
                SourceFileReadSuccess = Visibility.Visible;
                SourceFileReadFail = Visibility.Hidden;
            }
            else if (result == false)
            {
                Debug.WriteLine("SourceFile could not be read.");
                SourceFileReadSuccess = Visibility.Hidden;
                SourceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();
        }

        private void ReplaceCmd()
        {
            if (ReplaceData.FileName == string.Empty || SourceFilesData.FileNames.Count == 0)
            {
                Debug.WriteLine("Replace file or source files were empty. This should never be reached...");
                return;
            }

            bool caseSensitive = (CaseSensitive == Visibility.Visible) ? true : false;
            ReplaceData replaceData = new ReplaceData(caseSensitive);

            // create a list of destination file names
            List<string> destFileNames = SourceFilesData.GenerateDestFileNames();

            // perform the text replacements
            bool wholeWord = (WholeWord == Visibility.Visible) ? true : false;
            bool result = replaceData.PerformReplacements(SourceFilesData.FileNames, destFileNames, wholeWord);
            SourceFilesData.FileNames.ForEach(o => Debug.WriteLine(o));
            destFileNames.ForEach(o => Debug.WriteLine(o));

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
            else
            {
                Debug.WriteLine("Replacements successfully performed.");
            }
        }

        [RelayCommand]
        private void ToggleCaseSensitive()
        {
            CaseSensitive = (CaseSensitive == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
        }

        [RelayCommand]
        private void ToggleWholeWord()
        {
            WholeWord = (WholeWord == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
        }

        private void ChangeOutputDirectoryCmd()
        {
            // configure open file dialog box
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Select Folder";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                Debug.WriteLine("Change default file path window was closed");
                return;
            }
            
            SourceFilesData.OutputDirectory = dialog.FileName;
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

        /// <summary>
        /// Checks to see if the suffix is valid, and then sets the suffix
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public bool SetSuffix(string suffix)
        {
            if (IsSuffixValid(suffix))
            {
                Suffix = suffix;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the suffix string contains any invalid characters.
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns>True if the string is empty or does not contain any invalid characters.</returns>
        private bool IsSuffixValid(string suffix)
        {
            foreach (char c in suffix)
            {
                if (INVALID_SUFFIX_CHARS.Contains(c) || char.IsControl(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets the replace button visibility based on whether replace/source files were successfully uploaded.
        /// </summary>
        private void SetReplaceButtonClickability()
        {
            if (ReplaceFileReadSuccess == Visibility.Visible &&
                SourceFileReadSuccess == Visibility.Visible)
            {
                ReplaceisClickable = Visibility.Visible;
                ReplaceisUnclickable = Visibility.Hidden;
            }
            else
            {
                ReplaceisClickable = Visibility.Hidden;
                ReplaceisUnclickable = Visibility.Visible;
            }
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
