using TextReplace.MVVM.ViewModel.PopupWindows;
using TextReplace.Tests.Common;

namespace TextReplace.Tests.ViewModels.PopupWindows.Source
{
    public class UploadSourceFilesInputTests
    {
        private static readonly string RelativeSourcesPath = "../../../MockFiles/SourcesTests/";

        [Fact]
        public void OnFullFileNamesChanged_SingleFile_FileNameOrCountIsFileName()
        {
            // Arrange
            var vm = new UploadSourceFilesInputViewModel();
            VMHelper.RegisterMessenger(vm);

            var fileName = new List<string>()
            {
                "./directory/filename.txt"
            };

            // Act
            vm.FullFileNames = fileName;

            // Assert
            Assert.Equal("filename.txt", vm.FileNameOrCount);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void OnFullFileNamesChanged_MultipleFiles_FileNameOrCountIsFileName()
        {
            // Arrange
            var vm = new UploadSourceFilesInputViewModel();
            VMHelper.RegisterMessenger(vm);

            var fileNames = new List<string>()
            {
                RelativeSourcesPath + "source.txt",
                RelativeSourcesPath + "source.csv",
                RelativeSourcesPath + "source.tsv",
                RelativeSourcesPath + "source.xlsx"
            };

            var expected = $"{fileNames.Count} files uploaded.";

            // Act
            vm.FullFileNames = fileNames;

            // Assert
            Assert.Equal(expected, vm.FileNameOrCount);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void ValidateFiles_ValidFile_SetFieldsForValidFiles()
        {
            // Arrange
            var vm = new UploadSourceFilesInputViewModel();
            VMHelper.RegisterMessenger(vm);

            var fileName = new List<string>()
            {
                RelativeSourcesPath + "source.txt"
            };

            // Act
            var actual = vm.ValidateFiles(fileName);

            // Arrange
            Assert.True(actual);
            Assert.Equal(fileName, vm.FullFileNames);
            Assert.True(vm.ShowFileNameOrCount);
            Assert.True(vm.FileIsValid);
            Assert.False(vm.FileIsInvalid);
            Assert.True(vm.ConfirmIsClickable);
        }

        [Theory]
        [InlineData("invalid-file-type-source.bin")]
        [InlineData("unreadable-source.txt")]
        // NOTE: if the "unreadable-source.txt" test is failing, please make sure that the file is
        // *not* readable. the file is located in TextReplace/TextReplace.Tests/MockFiles/MockSources.
        public void ValidateFiles_InvalidFile_SetFieldsForInvalidFiles(string filename)
        {
            // Arrange
            var vm = new UploadSourceFilesInputViewModel();
            VMHelper.RegisterMessenger(vm);

            var fileName = new List<string>()
            {
                RelativeSourcesPath + filename
            };

            // Act
            var actual = vm.ValidateFiles(fileName);

            // Arrange
            Assert.False(actual);
            Assert.Equal(fileName, vm.FullFileNames);
            Assert.True(vm.ShowFileNameOrCount);
            Assert.False(vm.FileIsValid);
            Assert.True(vm.FileIsInvalid);
            Assert.False(vm.ConfirmIsClickable);
        }
    }
}
