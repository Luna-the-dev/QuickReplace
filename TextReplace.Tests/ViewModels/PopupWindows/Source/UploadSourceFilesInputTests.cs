﻿using TextReplace.MVVM.ViewModel.PopupWindows;
using TextReplace.Tests.Common;

namespace TextReplace.Tests.ViewModels.PopupWindows.Source
{
    public class UploadSourceFilesInputTests
    {
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

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var fileNames = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.csv",
                relativeFilepath + "source.tsv",
                relativeFilepath + "source.xlsx"
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

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var fileName = new List<string>()
            {
                relativeFilepath + "source.txt"
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
        public void ValidateFiles_InvalidFile_SetFieldsForInvalidFiles(string filename)
        {
            // Arrange
            var vm = new UploadSourceFilesInputViewModel();
            VMHelper.RegisterMessenger(vm);

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var fileName = new List<string>()
            {
                relativeFilepath + filename
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