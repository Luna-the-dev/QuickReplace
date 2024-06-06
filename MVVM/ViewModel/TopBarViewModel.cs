using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Windows;
using TextReplace.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages.Sources;
using TextReplace.Core.Validation;
using TextReplace.Messages.Replace;

namespace TextReplace.MVVM.ViewModel
{
    partial class TopBarViewModel : ObservableObject,
        IRecipient<WholeWordMsg>,
        IRecipient<CaseSensitiveMsg>,
        IRecipient<DefaultSourceFileOptionsMsg>,
        IRecipient<ReplacePhrasesMsg>,
        IRecipient<SourceFilesMsg>
    {
        [ObservableProperty]
        private Visibility _caseSensitive =
            (OutputData.CaseSensitive) ? Visibility.Visible : Visibility.Hidden;

        [ObservableProperty]
        private Visibility _wholeWord =
            (OutputData.WholeWord) ? Visibility.Visible : Visibility.Hidden;

        [ObservableProperty]
        private string _suffix = SourceFilesData.DefaultSourceFileOptions.Suffix ?? "";
        partial void OnSuffixChanged(string value)
        {
            SourceFilesData.UpdateAllSourceFileOptions(suffix: value);
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
        private bool _replaceIsClickable = false;

        // commands
        public RelayCommand ChangeOutputDirectory => new RelayCommand(ChangeOutputDirectoryCmd);
        public RelayCommand ToggleCaseSensitiveCommand => new RelayCommand(ToggleCaseSensitive);
        public RelayCommand ToggleWholeWordCommand => new RelayCommand(ToggleWholeWord);

        public TopBarViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        /// <summary>
        /// Wrapper for ReplaceData.SetNewReplaceFile and updates the replace button clickability.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>False if new replace file was not set.</returns>
        public bool SetNewReplaceFile(string fileName)
        {
            // open a file dialogue for the user and update the replace file
            bool result = ReplaceData.SetNewReplaceFile(fileName);

            if (result)
            {
                ReplaceFileReadSuccess = Visibility.Visible;
                ReplaceFileReadFail = Visibility.Hidden;
            }
            else
            {
                Debug.WriteLine("ReplaceFile either could not be read or parsed.");
                ReplaceFileReadSuccess = Visibility.Hidden;
                ReplaceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();

            return result;
        }

        public void SourceFiles(List<string> fileNames)
        {
            // open a file dialogue for the user and update the source files
            bool result = SourceFilesData.AddNewSourceFiles(fileNames);

            if (result == true)
            {
                SourceFileReadSuccess = Visibility.Visible;
                SourceFileReadFail = Visibility.Hidden;
            }
            else
            {
                Debug.WriteLine("SourceFile could not be read.");
                SourceFileReadSuccess = Visibility.Hidden;
                SourceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();
        }   

        public static string ReplaceAll(bool openFileLocation)
        {
            if (ReplaceData.FileName == string.Empty || SourceFilesData.SourceFiles.Count == 0)
            {
                throw new ApplicationException("Replace file or source files were empty. This should never be reached...");
            }

            OutputData.OpenFileLocation = openFileLocation;

            // perform the text replacements
            bool result = OutputData.PerformReplacements(
                ReplaceData.ReplacePhrasesDict,
                SourceFilesData.SourceFiles.Select(x => x.FileName).ToList(),
                OutputData.OutputFiles.Select(x => x.FileName).ToList(),
                OutputData.WholeWord,
                OutputData.CaseSensitive,
                OutputData.PreserveCase);

            Debug.WriteLine("Output file names:");
            OutputData.OutputFiles.ForEach(o => Debug.WriteLine($"\t{o.FileName}"));

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
            else
            {
                Debug.WriteLine("Replacements successfully performed.");
            }

            return OutputData.OutputFiles[0].FileName;
        }

        private void ToggleCaseSensitive()
        {
            OutputData.CaseSensitive = !OutputData.CaseSensitive;
        }

        private void ToggleWholeWord()
        {
            OutputData.WholeWord = !OutputData.WholeWord;
        }

        private void ChangeOutputDirectoryCmd()
        {
            // configure open file dialog box
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = "Select Folder",
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                Debug.WriteLine("Change default file path window was closed");
                return;
            }

            SourceFilesData.UpdateAllSourceFileOptions(outputDirectory: dialog.FileName);
        }

        /// <summary>
        /// Checks to see if the suffix is valid, and then sets the suffix
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public bool SetSuffix(string suffix)
        {
            if (DataValidation.IsSuffixValid(suffix))
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
        /// Sets the replace button visibility based on whether replace/source files were successfully uploaded.
        /// </summary>
        private void SetReplaceButtonClickability()
        {
            ReplaceIsClickable = (ReplaceFileReadSuccess == Visibility.Visible &&
                                  SourceFileReadSuccess == Visibility.Visible);
        }

        public void Receive(DefaultSourceFileOptionsMsg message)
        {
            Suffix = message.Value.Suffix;
        }

        public void Receive(WholeWordMsg message)
        {
            WholeWord = (message.Value) ? Visibility.Visible : Visibility.Hidden;
            Debug.WriteLine(message.Value);
        }

        public void Receive(CaseSensitiveMsg message)
        {
            CaseSensitive = (message.Value) ? Visibility.Visible : Visibility.Hidden;
            Debug.WriteLine(message.Value);
        }

        public void Receive(ReplacePhrasesMsg message)
        {
            if (message.Value.Count > 0)
            {
                ReplaceFileReadSuccess = Visibility.Visible;
                ReplaceFileReadFail = Visibility.Hidden;
                SetReplaceButtonClickability();
                return;
            }
            ReplaceFileReadSuccess = Visibility.Hidden;
            ReplaceFileReadFail = Visibility.Visible;
            SetReplaceButtonClickability();
        }

        public void Receive(SourceFilesMsg message)
        {
            if (message.Value.Count > 0)
            {
                SourceFileReadSuccess = Visibility.Visible;
                SourceFileReadFail = Visibility.Hidden;
                SetReplaceButtonClickability();
                return;
            }
            SourceFileReadSuccess = Visibility.Hidden;
            SourceFileReadFail = Visibility.Visible;
            SetReplaceButtonClickability();
        }
    }
}
