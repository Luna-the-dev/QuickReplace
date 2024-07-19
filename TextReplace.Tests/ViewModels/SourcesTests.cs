using TextReplace.MVVM.Model;
using TextReplace.MVVM.ViewModel;
using TextReplace.Tests.Common;

namespace TextReplace.Tests.ViewModels
{
    public class SourcesTests
    {
        [Fact]
        public void OnSourceFilesChanged_ChangingSourceFIless_UpdatesIsSourceFileUploaded()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();
            vm.IsSourceFileUploaded = false;

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };

            // Act
            var firstActual = vm.IsSourceFileUploaded;

            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            var secondActual = vm.IsSourceFileUploaded;

            vm.RemoveSourceFile(0);
            var thirdActual = vm.IsSourceFileUploaded;

            SourcesViewModel.RemoveAllSourceFiles();
            var fourthActual = vm.IsSourceFileUploaded;

            // Assert
            Assert.False(firstActual);
            Assert.True(secondActual);
            Assert.True(thirdActual);
            Assert.False(fourthActual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void OnSelectedFileChanged_ChangedSelectedFile_UpdatesIsFileSelected()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();
            vm.IsFileSelected = false;

            // Act
            vm.SelectedFile = new SourceFileWrapper("filename", "", "");

            // Assert
            Assert.True(vm.IsFileSelected);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData("hello")]
        public void OnSearchTextChanged_SearchTextChanged_FiltersSourceFilesByFileName(string filter)
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";


            var sourceFiles = new List<string>()
            {
                relativeFilepath + "hello.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "xxHello!xx.txt"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            vm.SearchText = filter;

            // Assert
            Assert.Equal(2, vm.SourceFiles.Count);
            Assert.Equal(relativeFilepath + "hello.txt", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "xxHello!xx.txt", vm.SourceFiles[1].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SetSelectedFile_ValidFile_FileIsSetAsSelectedFile()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            vm.SetSelectedFileCommand.Execute(vm.SourceFiles[1]);

            // Assert
            Assert.Equal(relativeFilepath + "source.docx", vm.SelectedFile.FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewSourceFiles_SingleFile_SourceFileAdded()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFile = new List<string>()
            {
                relativeFilepath + "source.txt"
            };

            // Act
            SourcesViewModel.AddNewSourceFiles(sourceFile);

            // Assert
            Assert.Single(vm.SourceFiles);
            Assert.Equal(relativeFilepath + "source.txt", vm.SourceFiles[0].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewSourceFiles_MultipleFiles_SourceFilesAdded()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };

            // Act
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Assert
            Assert.Equal(3, vm.SourceFiles.Count);
            Assert.Equal(relativeFilepath + "source.txt", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "source.docx", vm.SourceFiles[1].FileName);
            Assert.Equal(relativeFilepath + "source.xlsx", vm.SourceFiles[2].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewSourceFiles_CustomDefaultOutputDirectory_NewSourceFilesHaveCustomOutputDirectory()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };

            SourcesViewModel.UpdateAllSourceFileOutputDirectories("directory-name/");

            // Act
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Assert
            Assert.Equal("directory-name/", vm.SourceFiles[0].OutputDirectory);
            Assert.Equal("directory-name/", vm.SourceFiles[1].OutputDirectory);
            Assert.Equal("directory-name/", vm.SourceFiles[2].OutputDirectory);
            Assert.Equal("directory-name/", vm.SelectedFile.OutputDirectory);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewSourceFiles_CustomDefaultSuffix_NewSourceFilesHaveCustomSuffix()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };

            SourcesViewModel.UpdateAllSourceFileSuffixes("-replacify");

            // Act
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Assert
            Assert.Equal("-replacify", vm.SourceFiles[0].Suffix);
            Assert.Equal("-replacify", vm.SourceFiles[1].Suffix);
            Assert.Equal("-replacify", vm.SourceFiles[2].Suffix);
            Assert.Equal("-replacify", vm.SelectedFile.Suffix);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void RemoveSourceFile_FirstIndex_SourceFileRemovedFromList()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            vm.RemoveSourceFile(0);

            // Assert
            Assert.Equal(2, vm.SourceFiles.Count);
            Assert.Equal(relativeFilepath + "source.docx", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "source.xlsx", vm.SourceFiles[1].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        public void RemoveSourceFile_InvalidIndex_ReturnFalse(int index)
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            var actual = vm.RemoveSourceFile(index);

            // Assert
            Assert.False(actual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void RemoveAllSourceFiles_IsCalled_SourceFilesAreEmpty()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            SourcesViewModel.RemoveAllSourceFiles();

            // Assert
            Assert.Empty(vm.SourceFiles);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void MoveSourceFile_LastToFirst_SourceFilesInCorrectOrder()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            SourcesViewModel.MoveSourceFile(oldIndex: 2, newIndex: 0);

            // Assert
            Assert.Equal(relativeFilepath + "source.xlsx", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "source.txt", vm.SourceFiles[1].FileName);
            Assert.Equal(relativeFilepath + "source.docx", vm.SourceFiles[2].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void MoveSourceFile_FirstToMiddle_SourceFilesInCorrectOrder()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            SourcesViewModel.MoveSourceFile(oldIndex: 0, newIndex: 2);

            // Assert
            Assert.Equal(relativeFilepath + "source.docx", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "source.txt", vm.SourceFiles[1].FileName);
            Assert.Equal(relativeFilepath + "source.xlsx", vm.SourceFiles[2].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void MoveSourceFile_FirstToFirst_SourceFilesInCorrectOrder()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            SourcesViewModel.MoveSourceFile(oldIndex: 0, newIndex: 0);

            // Assert
            Assert.Equal(relativeFilepath + "source.txt", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "source.docx", vm.SourceFiles[1].FileName);
            Assert.Equal(relativeFilepath + "source.xlsx", vm.SourceFiles[2].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void MoveSourceFile_LastToLast_SourceFilesInCorrectOrder()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            SourcesViewModel.MoveSourceFile(oldIndex: 2, newIndex: 2);

            // Assert
            Assert.Equal(relativeFilepath + "source.txt", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "source.docx", vm.SourceFiles[1].FileName);
            Assert.Equal(relativeFilepath + "source.xlsx", vm.SourceFiles[2].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void MoveSourceFile_InvalidOldIndex_ReturnsFalse(int oldIndex)
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            var actual = SourcesViewModel.MoveSourceFile(oldIndex: oldIndex, newIndex: 2);

            // Assert
            Assert.False(actual);
            Assert.Equal(relativeFilepath + "source.txt", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "source.docx", vm.SourceFiles[1].FileName);
            Assert.Equal(relativeFilepath + "source.xlsx", vm.SourceFiles[2].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void MoveSourceFile_InvalidNewIndex_ReturnsFalse(int newIndex)
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            var actual = SourcesViewModel.MoveSourceFile(oldIndex: 0, newIndex: newIndex);

            // Assert
            Assert.False(actual);
            Assert.Equal(relativeFilepath + "source.txt", vm.SourceFiles[0].FileName);
            Assert.Equal(relativeFilepath + "source.docx", vm.SourceFiles[1].FileName);
            Assert.Equal(relativeFilepath + "source.xlsx", vm.SourceFiles[2].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void UpdateSourceFileOutputDirectory_ValidDirectory_SelectedSourceFileOutputDirectoryUpdated()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            vm.SetSelectedFileCommand.Execute(vm.SourceFiles[1]);

            // Act
            vm.UpdateSourceFileOutputDirectory("directory-name/");

            // Assert
            Assert.Equal("directory-name/", vm.SourceFiles[1].OutputDirectory);
            Assert.Equal("directory-name/", vm.SelectedFile.OutputDirectory);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void UpdateAllSourceFileOutputDirectories_ValidDirectory_AllSourceFilesOutputDirectoryUpdated()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            SourcesViewModel.UpdateAllSourceFileOutputDirectories("directory-name/");

            // Assert
            Assert.Equal("directory-name/", vm.SourceFiles[0].OutputDirectory);
            Assert.Equal("directory-name/", vm.SourceFiles[1].OutputDirectory);
            Assert.Equal("directory-name/", vm.SourceFiles[2].OutputDirectory);
            Assert.Equal("directory-name/", vm.SelectedFile.OutputDirectory);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void UpdateSourceFileSuffix_ValidSuffix_SelectedSourceFileSuffixUpdated()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            vm.SetSelectedFileCommand.Execute(vm.SourceFiles[1]);

            // Act
            vm.UpdateSourceFileSuffix("-replacify");

            // Assert
            Assert.Equal("-replacify", vm.SourceFiles[1].Suffix);
            Assert.Equal("-replacify", vm.SelectedFile.Suffix);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void UpdateAllSourceFileSuffixes_ValidSuffix_AllSourceFileSuffixesUpdated()
        {
            // Arrange
            var vm = new SourcesViewModel();
            VMHelper.RegisterMessenger(vm);
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var relativeFilepath = "../../../MockFiles/MockSources/";

            var sourceFiles = new List<string>()
            {
                relativeFilepath + "source.txt",
                relativeFilepath + "source.docx",
                relativeFilepath + "source.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);

            // Act
            SourcesViewModel.UpdateAllSourceFileSuffixes("-replacify");

            // Assert
            Assert.Equal("-replacify", vm.SourceFiles[0].Suffix);
            Assert.Equal("-replacify", vm.SourceFiles[1].Suffix);
            Assert.Equal("-replacify", vm.SourceFiles[2].Suffix);
            Assert.Equal("-replacify", vm.SelectedFile.Suffix);

            VMHelper.UnregisterMessenger(vm);
        }
    }
}
