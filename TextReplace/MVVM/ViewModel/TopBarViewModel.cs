using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using TextReplace.Core.Validation;
using TextReplace.Messages;
using TextReplace.Messages.Output;
using TextReplace.Messages.Replace;
using TextReplace.Messages.Sources;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class TopBarViewModel : ObservableRecipient,
        IRecipient<WholeWordMsg>,
        IRecipient<CaseSensitiveMsg>,
        IRecipient<PreserveCaseMsg>,
        IRecipient<DefaultSourceFileOptionsMsg>,
        IRecipient<ReplacePhrasesMsg>,
        IRecipient<SourceFilesMsg>
    {
        [ObservableProperty]
        private bool _wholeWord = OutputData.WholeWord;

        [ObservableProperty]
        private bool _caseSensitive = OutputData.CaseSensitive;

        [ObservableProperty]
        private bool _preserveCase = OutputData.PreserveCase;

        [ObservableProperty]
        private string _suffix = SourceFilesData.DefaultSourceFileOptions.Suffix ?? "";
        partial void OnSuffixChanged(string value)
        {
            SourceFilesData.UpdateAllSourceFileOptions(suffix: value);
        }

        // visibility flags for top bar components
        [ObservableProperty]
        private bool _replaceFileReadSuccess = false;
        [ObservableProperty]
        private bool _replaceFileReadFail = false;
        [ObservableProperty]
        private bool _sourceFileReadSuccess = false;
        [ObservableProperty]
        private bool _sourceFileReadFail = false;
        [ObservableProperty]
        private bool _replaceIsClickable = false;

        // commands
        public RelayCommand ChangeOutputDirectory => new RelayCommand(ChangeOutputDirectoryCmd);
        public RelayCommand ToggleWholeWordCommand => new RelayCommand(ToggleWholeWord);
        public RelayCommand ToggleCaseSensitiveCommand => new RelayCommand(ToggleCaseSensitive);
        public RelayCommand TogglePreserveCaseCommand => new RelayCommand(TogglePreserveCase);

        protected override void OnActivated()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void ToggleWholeWord()
        {
            OutputData.WholeWord = !OutputData.WholeWord;
        }

        private void ToggleCaseSensitive()
        {
            OutputData.CaseSensitive = !OutputData.CaseSensitive;
        }

        private void TogglePreserveCase()
        {
            OutputData.PreserveCase = !OutputData.PreserveCase;
        }

        /// <summary>
        /// Wrapper for ReplaceData.SetNewReplaceFile and updates the replace button clickability.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>False if new replace file was not set.</returns>
        public static void SetNewReplacePhrasesFromFile(string fileName)
        {
            ReplaceData.SetNewReplacePhrasesFromFile(fileName);
        }

        public void SourceFiles(List<string> fileNames)
        {
            // open a file dialogue for the user and update the source files
            bool result = SourceFilesData.AddNewSourceFiles(fileNames);

            if (result == true)
            {
                SourceFileReadSuccess = true;
                SourceFileReadFail = false;
            }
            else
            {
                Debug.WriteLine("SourceFile could not be read.");
                SourceFileReadSuccess = false;
                SourceFileReadFail = true;
            }

            SetReplaceButtonClickability();
        }

        public static async Task<string> ReplaceAll(bool openFileLocation)
        {
            return await Task.Run(() =>
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
            }).ConfigureAwait(false);
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

        public static void SetActiveContentView(string viewName)
        {
            if (viewName == "replace")
            {
                WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(new ReplaceViewModel()));
            }
            else if (viewName == "sources")
            {
                WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(new SourcesViewModel()));
            }
            else if (viewName == "output")
            {
                WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(new OutputViewModel()));
            }
            else
            {
                throw new NotImplementedException($"{viewName}: view model does not exist");
            }
        }

        /// <summary>
        /// Sets the replace button visibility based on whether replace/source files were successfully uploaded.
        /// </summary>
        private void SetReplaceButtonClickability()
        {
            ReplaceIsClickable = (ReplaceFileReadSuccess == true &&
                                  SourceFileReadSuccess == true);
        }

        public void Receive(DefaultSourceFileOptionsMsg message)
        {
            Suffix = message.Value.Suffix;
        }

        public void Receive(WholeWordMsg message)
        {
            WholeWord = message.Value;
        }

        public void Receive(CaseSensitiveMsg message)
        {
            CaseSensitive = message.Value;
        }

        public void Receive(PreserveCaseMsg message)
        {
            PreserveCase = message.Value;
        }

        public void Receive(ReplacePhrasesMsg message)
        {
            if (message.Value.Count > 0)
            {
                ReplaceFileReadSuccess = true;
                ReplaceFileReadFail = false;
                SetReplaceButtonClickability();
                return;
            }
            ReplaceFileReadSuccess = false;
            ReplaceFileReadFail = true;
            SetReplaceButtonClickability();
        }

        public void Receive(SourceFilesMsg message)
        {
            if (message.Value.Count > 0)
            {
                SourceFileReadSuccess = true;
                SourceFileReadFail = false;
                SetReplaceButtonClickability();
                return;
            }
            SourceFileReadSuccess = false;
            SourceFileReadFail = true;
            SetReplaceButtonClickability();
        }
    }
}
