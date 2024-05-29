using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.IO;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class SourcesViewModel : ObservableRecipient,
        IRecipient<SourceFilesMsg>,
        IRecipient<SelectedSourceFileMsg>
    {
        [ObservableProperty]
        private ObservableCollection<SourceFileWrapper> _sourceFiles =
            new ObservableCollection<SourceFileWrapper>(SourceFilesData.SourceFiles.Select(SourceFileWrapper.WrapSourceFile));

        [ObservableProperty]
        private SourceFileWrapper _selectedFile = new SourceFileWrapper("", "", "");
        partial void OnSelectedFileChanged(SourceFileWrapper value)
        {
            SourceFilesData.SelectedFile = SourceFileWrapper.UnwrapSourceFile(value);
        }

        public RelayCommand<object> SetSelectedFileCommand => new RelayCommand<object>(SetSelectedFile);

        public SourcesViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void SetSelectedFile(object? file)
        {
            // for some reason if i pass in the SourceFileWrapper, this doesn't fire
            // on the first click, however if i pass it as a generic obect and
            // cast it to ReplacePhrase, it works. probably some MVVM community toolkit quirk
            if (file == null)
            {
                return;
            }
            SourceFileWrapper f = (SourceFileWrapper)file;
            f.IsSelected = true;
            SelectedFile = f;
        }

        /// <summary>
        /// Wrapper for SourceFilesData.SetNewSourceFiles
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static bool SetNewSourceFiles(List<string> fileNames)
        {
            return SourceFilesData.SetNewSourceFiles(fileNames);
        }

        public void Receive(SourceFilesMsg message)
        {
            // if the source file has no custom output directory or suffix, replace the empty string with "Default"
            SourceFiles = new ObservableCollection<SourceFileWrapper>(message.Value.Select(x => {
                x.OutputDirectory = (x.OutputDirectory == string.Empty) ? "Default" : x.OutputDirectory;
                x.Suffix = (x.Suffix == string.Empty) ? "Default" : x.Suffix;
                return SourceFileWrapper.WrapSourceFile(x);
            }));
        }

        public void Receive(SelectedSourceFileMsg message)
        {
            SelectedFile = SourceFileWrapper.WrapSourceFile(message.Value);
        }
    }

    class SourceFileWrapper(string fileName, string outputDirectory, string suffix, bool isSelected = false)
    {
        public string FileName { get; set; } = fileName;
        public string ShortFileName { get; set; } = Path.GetFileName(fileName);
        public string OutputDirectory { get; set; } = outputDirectory;
        public string Suffix { get; set; } = suffix;
        public bool IsSelected { get; set; } = isSelected;

        public static SourceFileWrapper WrapSourceFile(SourceFile file)
        {
            if (file.FileName == SourceFilesData.SelectedFile.FileName)
            {
                return new SourceFileWrapper(file.FileName, file.OutputDirectory, file.Suffix, true);
            }
            return new SourceFileWrapper(file.FileName, file.OutputDirectory, file.Suffix);
        }

        public static SourceFile UnwrapSourceFile(SourceFileWrapper file)
        {
            return new SourceFile(file.FileName, file.OutputDirectory, file.Suffix);
        }
    }
}
