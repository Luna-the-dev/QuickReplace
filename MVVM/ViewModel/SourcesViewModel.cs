using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class SourcesViewModel : ObservableRecipient,
        IRecipient<SourceFileNamesMsg>
    {
        [ObservableProperty]
        private ObservableCollection<string> _fullFileNames =
            new ObservableCollection<string>(SourceFilesData.FileNames);

        /// <summary>
        /// Wrapper for SourceFilesData.SetNewSourceFiles
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static bool SetNewSourceFiles(List<string> fileNames)
        {
            return SourceFilesData.SetNewSourceFiles(fileNames);
        }

        public void Receive(SourceFileNamesMsg message)
        {
            FullFileNames = new ObservableCollection<string>(SourceFilesData.FileNames);
        }
    }
}
