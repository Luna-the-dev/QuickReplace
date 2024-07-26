using System.Diagnostics;
using System.Windows.Media;
using TextReplace.MVVM.ViewModel;
using TextReplace.MVVM.Model;
using TextReplace.Tests.Common;
using TextReplace.Core.Enums;

namespace TextReplace.Tests.ViewModels
{
    public class OutputTests
    {
        private static readonly string RelativeReplacementsPath = "../../../MockFiles/OutputTests/Replacements/";
        private static readonly string RelativeSourcesPath = "../../../MockFiles/OutputTests/Sources/";
        private static readonly string RelativeOutputsPath = "../../../MockFiles/OutputTests/Outputs/";
        private static readonly string RelativeGeneratedFilePath = "../../GeneratedTestFiles/OutputTests/";

        [Fact]
        public void OnSelectedFileChanged_ValidFile_UpdatesIsFileSelected()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);
            vm.IsFileSelected = false;

            // Act
            vm.SelectedFile = new OutputFileWrapper("filename", "", "", -1);

            // Assert
            Assert.True(vm.IsFileSelected);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData("hello")]
        public void OnSearchTextChanged_SearchTextChanged_FiltersOutputFilesByFileName(string filter)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var suffix = "-testing";

            var sourceFiles = new List<string>()
            {
                sourcePath + "hello.txt",
                sourcePath + "source-resume.docx",
                sourcePath + "xxHello!xx.txt"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);

            var firstExpected = string.Format("{0}hello{1}.txt", RelativeGeneratedFilePath, suffix);
            var secondExpected = string.Format("{0}xxHello!xx{1}.txt", RelativeGeneratedFilePath, suffix);

            // Act
            vm.SearchText = filter;

            // Assert
            Assert.Equal(2, vm.OutputFiles.Count);
            Assert.Equal(firstExpected, vm.OutputFiles[0].FileName);
            Assert.Equal(secondExpected, vm.OutputFiles[1].FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SetSelectedFile_ValidFile_FileIsSetAsSelectedFile()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var suffix = "-testing";

            var sourceFiles = new List<string>()
            {
                sourcePath + "source-resume.txt",
                sourcePath + "source-resume.docx",
                sourcePath + "source-resume.csv"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);

            var expected = string.Format("{0}source-resume{1}.docx", RelativeGeneratedFilePath, suffix);

            // Act
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles[1]);

            // Assert
            Assert.Equal(expected, vm.SelectedFile.FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SetSelectedFile_OutputDirectoryWithoutTrailingSlash_FileIsSetAsSelectedFile()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var suffix = "-testing";
            var outputDirectory = "../../GeneratedTestFiles/OutputTests";

            var sourceFiles = new List<string>()
            {
                sourcePath + "source-resume.txt",
                sourcePath + "source-resume.docx",
                sourcePath + "source-resume.csv"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(outputDirectory);

            Debug.WriteLine(sourceFiles[0]);

            var expected = string.Format("{0}/source-resume{1}.docx", outputDirectory, suffix);

            // Act
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles[1]);

            // Assert
            Assert.Equal(expected, vm.SelectedFile.FileName);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void ToggleWholeWord_IsToggled_UpdatesIsWholeWord()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);
            vm.IsWholeWord = false;

            // Act
            OutputViewModel.ToggleWholeWordCommand.Execute(null);
            var firstExpected = vm.IsWholeWord;

            OutputViewModel.ToggleWholeWordCommand.Execute(null);
            var secondExpected = vm.IsWholeWord;

            // Assert
            Assert.True(firstExpected);
            Assert.False(secondExpected);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void ToggleCaseSensitive_IsToggled_UpdatesIsCaseSensitive()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);
            vm.IsCaseSensitive = false;

            // Act
            OutputViewModel.ToggleCaseSensitiveCommand.Execute(null);
            var firstExpected = vm.IsCaseSensitive;

            OutputViewModel.ToggleCaseSensitiveCommand.Execute(null);
            var secondExpected = vm.IsCaseSensitive;

            // Assert
            Assert.True(firstExpected);
            Assert.False(secondExpected);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void TogglePreserveCase_IsToggled_UpdatesIsPreserveCase()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);
            vm.IsPreserveCase = false;

            // Act
            OutputViewModel.TogglePreserveCaseCommand.Execute(null);
            var firstExpected = vm.IsPreserveCase;

            OutputViewModel.TogglePreserveCaseCommand.Execute(null);
            var secondExpected = vm.IsPreserveCase;

            // Assert
            Assert.True(firstExpected);
            Assert.False(secondExpected);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.txt")]
        [InlineData("source-resume.csv")]
        [InlineData("source-resume.tsv")]
        [InlineData("source-resume.docx")]
        public void SetSelectedOutputFileType_TextOrDocxFile_ChangesOutputFIleType(string sourceFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes("");

            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            // Act
            vm.SetSelectedOutputFileType(OutputFileTypeEnum.KeepFileType);
            var expected1 = Path.GetExtension(sourceFileName);
            var actual1 = Path.GetExtension(vm.SelectedFile.FileName);

            vm.SetSelectedOutputFileType(OutputFileTypeEnum.Text);
            var expected2 = Path.GetExtension(".txt");
            var actual2 = Path.GetExtension(vm.SelectedFile.FileName);

            vm.SetSelectedOutputFileType(OutputFileTypeEnum.Document);
            var expected3 = Path.GetExtension(".docx");
            var actual3 = Path.GetExtension(vm.SelectedFile.FileName);

            // Assert
            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SetSelectedOutputFileType_ExcelFile_ChangesOutputFIleType()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";

            var sourceFiles = new List<string>()
            {
                sourcePath + "source-financial-sample.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes("");

            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            // Act
            vm.SetSelectedOutputFileType(OutputFileTypeEnum.KeepFileType);
            var expected1 = Path.GetExtension(".xlsx");
            var actual1 = Path.GetExtension(vm.SelectedFile.FileName);

            vm.SetSelectedOutputFileType(OutputFileTypeEnum.Text);
            var expected2 = Path.GetExtension(".xlsx");
            var actual2 = Path.GetExtension(vm.SelectedFile.FileName);

            vm.SetSelectedOutputFileType(OutputFileTypeEnum.Document);
            var expected3 = Path.GetExtension(".xlsx");
            var actual3 = Path.GetExtension(vm.SelectedFile.FileName);

            // Assert
            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.txt")]
        [InlineData("source-resume.csv")]
        [InlineData("source-resume.tsv")]
        [InlineData("source-resume.docx")]
        public void SetAllOutputFileTypes_TextOrDocxFile_ChangesOutputFIleType(string sourceFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes("");

            // Act
            OutputViewModel.SetAllOutputFileTypes(OutputFileTypeEnum.KeepFileType);
            var expected1 = Path.GetExtension(sourceFileName);
            var actual1 = Path.GetExtension(vm.OutputFiles[0].FileName);

            OutputViewModel.SetAllOutputFileTypes(OutputFileTypeEnum.Text);
            var expected2 = Path.GetExtension(".txt");
            var actual2 = Path.GetExtension(vm.OutputFiles[0].FileName);

            OutputViewModel.SetAllOutputFileTypes(OutputFileTypeEnum.Document);
            var expected3 = Path.GetExtension(".docx");
            var actual3 = Path.GetExtension(vm.OutputFiles[0].FileName);

            // Assert
            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SetAllOutputFileTypes_ExcelFile_ChangesOutputFIleType()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";

            var sourceFiles = new List<string>()
            {
                sourcePath + "source-financial-sample.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes("");

            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            // Act
            OutputViewModel.SetAllOutputFileTypes(OutputFileTypeEnum.KeepFileType);
            var expected1 = Path.GetExtension(".xlsx");
            var actual1 = Path.GetExtension(vm.SelectedFile.FileName);

            OutputViewModel.SetAllOutputFileTypes(OutputFileTypeEnum.Text);
            var expected2 = Path.GetExtension(".xlsx");
            var actual2 = Path.GetExtension(vm.SelectedFile.FileName);

            OutputViewModel.SetAllOutputFileTypes(OutputFileTypeEnum.Document);
            var expected3 = Path.GetExtension(".xlsx");
            var actual3 = Path.GetExtension(vm.SelectedFile.FileName);

            // Assert
            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.txt", "output-resume.txt")]
        [InlineData("source-resume.csv", "output-resume.csv")]
        [InlineData("source-resume.tsv", "output-resume.tsv")]
        [InlineData("source-resume.docx", "output-resume.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample.xlsx")]
        public async Task ReplaceSelected_ValidFile_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Normal/";
            var suffix = "-ReplaceSelected_ValidFile";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);

            // Compare the generated file to the mock file
            if (Path.GetExtension(vm.OutputFiles.First().FileName) == ".xlsx" ||
                Path.GetExtension(vm.OutputFiles.First().FileName) == ".docx")
            {
                Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));
            }
            else
            {
                Assert.True(FileComparer.FilesAreEqual(outputPath + MockFileName, generatedFileName));
            }

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume-nested-replacements.txt", "output-resume-nested-replacements.txt")]
        [InlineData("source-resume-nested-replacements.docx", "output-resume-nested-replacements.docx")]
        [InlineData("source-financial-sample-nested-replacements.xlsx", "output-financial-sample-nested-replacements.xlsx")]
        public async Task ReplaceSelected_NestedReplacements_ReturnsTrueAndReplacementsPerformedCorrectly(string sourceFileName, string MockFileName)
        {
            // If replacements are nested (such as "there" and "her", they are supposed to be handled as follows:
            //      if two matches are nexted, favor the one that starts first
            //          - ex: for "there" and "her", "there" is favored
            //      if two matches start at the same position, favor the longer one
            //          - ex: for "add" and "additional", favor "additional"

            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-nested.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "NestedReplacements/";
            var outputPath = RelativeOutputsPath + "NestedReplacements/";
            var suffix = "-ReplaceSelected_NestedReplacements";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);

            // Compare the generated file to the mock file
            if (Path.GetExtension(vm.OutputFiles.First().FileName) == ".xlsx" ||
                Path.GetExtension(vm.OutputFiles.First().FileName) == ".docx")
            {
                Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));
            }
            else
            {
                Assert.True(FileComparer.FilesAreEqual(outputPath + MockFileName, generatedFileName));
            }

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("invalid-source-file.txt")]
        [InlineData("invalid-file-extension.tmp")]
        public async Task ReplaceSelected_InvalidFile_ReturnsFalseAndReplacementsNotPerformed(string sourceFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            vm.OutputFiles.Add(new OutputFileWrapper(RelativeGeneratedFilePath + sourceFileName, "", "", -1));
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles[0]);

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.False(actual);
            Assert.False(File.Exists(generatedFileName));

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public async Task ReplaceSelected_FileIsInUse_ReturnsFalseAndReplacementsNotPerformed()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            vm.OutputFiles.Add(new OutputFileWrapper("source-resume.txt", "", "", -1));
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles[0]);

            var generatedFileName = vm.OutputFiles[0].FileName;
            
            if (File.Exists(generatedFileName) == false)
            {
                File.Create(generatedFileName);
            }

            SetOutputSettings(vm);

            // open a stream writer on the generated file to make sure it cant be written to
            using var writer = new StreamWriter(generatedFileName);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.False(actual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public async Task ReplaceAll_ValidFile_ReturnsTrueAndReplacementsPerformed()
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Normal/";
            var suffix = "-ReplaceAll_ValidFile";

            // Upload all of the source files
            var sourceFiles = new List<string>()
            {
                sourcePath + "source-resume.txt",
                sourcePath + "source-resume.csv",
                sourcePath + "source-resume.tsv",
                sourcePath + "source-resume.docx",
                sourcePath + "source-financial-sample.xlsx"
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);

            // Upload all of the mock files
            var mockFileNames = new List<string>()
            {
                outputPath + "output-resume.txt",
                outputPath + "output-resume.csv",
                outputPath + "output-resume.tsv",
                outputPath + "output-resume.docx",
                outputPath + "output-financial-sample.xlsx"
            };

            // create a list of the generated file names
            var generatedFileNames = new List<string>(vm.OutputFiles.Select(x => x.FileName));

            // zip the mock file names and the generated file names together
            var fileNamesList = mockFileNames.Zip(generatedFileNames, (m, g) => new { Mock = m, Generated = g });

            SetOutputSettings(vm);

            // Act
            bool actual = await OutputViewModel.ReplaceAll(false);

            // Assert
            Assert.True(actual);

            foreach (var fileName in fileNamesList)
            {
                // Compare the generated file to the mock file
                if (Path.GetExtension(vm.OutputFiles.First().FileName) == ".xlsx" ||
                    Path.GetExtension(vm.OutputFiles.First().FileName) == ".docx")
                {
                    Assert.True(FileComparer.FilesAreEqual_OpenXml(fileName.Mock, fileName.Generated));
                }
                else
                {
                    Assert.True(FileComparer.FilesAreEqual(fileName.Mock, fileName.Generated));
                }
            }

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume-split-runs.docx", "output-resume-split-runs.docx")]
        [InlineData("source-financial-sample-split-runs.xlsx", "output-financial-sample-split-runs.xlsx")]
        public async Task ReplaceSelected_SplitRunsInSources_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "SplitRuns/";
            var outputPath = RelativeOutputsPath + "SplitRuns/";
            var suffix = "-ReplaceSelected_SplitRunsInSources";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);

            // Compare the generated file to the mock file
            Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.txt", "output-resume-whole-word.txt")]
        [InlineData("source-resume.csv", "output-resume-whole-word.csv")]
        [InlineData("source-resume.tsv", "output-resume-whole-word.tsv")]
        [InlineData("source-resume.docx", "output-resume-whole-word.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-whole-word.xlsx")]
        public async Task ReplaceSelected_WholeWord_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "WholeWord/";
            var suffix = "-ReplaceSelected_WholeWord";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            // set whole word to true
            SetOutputSettings(vm, wholeWord: true);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);

            // Compare the generated file to the mock file
            if (Path.GetExtension(vm.OutputFiles.First().FileName) == ".xlsx" ||
                Path.GetExtension(vm.OutputFiles.First().FileName) == ".docx")
            {
                Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));
            }
            else
            {
                Assert.True(FileComparer.FilesAreEqual(outputPath + MockFileName, generatedFileName));
            }

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.txt", "output-resume-case-sensitive.txt")]
        [InlineData("source-resume.csv", "output-resume-case-sensitive.csv")]
        [InlineData("source-resume.tsv", "output-resume-case-sensitive.tsv")]
        [InlineData("source-resume.docx", "output-resume-case-sensitive.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-case-sensitive.xlsx")]
        public async Task ReplaceSelected_CaseSensitive_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "CaseSensitive/";
            var suffix = "-ReplaceSelected_CaseSensitive";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, caseSensitive: true);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);

            // Compare the generated file to the mock file
            if (Path.GetExtension(vm.OutputFiles.First().FileName) == ".xlsx" ||
                Path.GetExtension(vm.OutputFiles.First().FileName) == ".docx")
            {
                Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));
            }
            else
            {
                Assert.True(FileComparer.FilesAreEqual(outputPath + MockFileName, generatedFileName));
            }

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.txt", "output-resume-preserve-case.txt")]
        [InlineData("source-resume.csv", "output-resume-preserve-case.csv")]
        [InlineData("source-resume.tsv", "output-resume-preserve-case.tsv")]
        [InlineData("source-resume.docx", "output-resume-preserve-case.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-preserve-case.xlsx")]
        public async Task ReplaceSelected_PreserveCase_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "PreserveCase/";
            var suffix = "-ReplaceSelected_PreserveCase";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, preserveCase: true);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);

            // Compare the generated file to the mock file
            if (Path.GetExtension(vm.OutputFiles.First().FileName) == ".xlsx" ||
                Path.GetExtension(vm.OutputFiles.First().FileName) == ".docx")
            {
                Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));
            }
            else
            {
                Assert.True(FileComparer.FilesAreEqual(outputPath + MockFileName, generatedFileName));
            }

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.txt", "output-resume-wwccpc.txt")]
        [InlineData("source-resume.csv", "output-resume-wwccpc.csv")]
        [InlineData("source-resume.tsv", "output-resume-wwccpc.tsv")]
        [InlineData("source-resume.docx", "output-resume-wwccpc.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-wwccpc.xlsx")]
        public async Task ReplaceSelected_WholeWordCaseSensitivePreserveCase_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "WholeWordCaseSensitiveAndPreserveCase/";
            var suffix = "-ReplaceSelected_WholeWordCaseSensitivePreserveCase";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, wholeWord: true, caseSensitive: true, preserveCase: true);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);

            // Compare the generated file to the mock file
            if (Path.GetExtension(vm.OutputFiles.First().FileName) == ".xlsx" ||
                Path.GetExtension(vm.OutputFiles.First().FileName) == ".docx")
            {
                Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));
            }
            else
            {
                Assert.True(FileComparer.FilesAreEqual(outputPath + MockFileName, generatedFileName));
            }

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.docx", "output-resume-bold.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-bold.xlsx")]
        public async Task ReplaceSelected_Bold_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Styling/";
            var suffix = "-ReplaceSelected_Bold";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, bold: true);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);
            Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.docx", "output-resume-italics.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-italics.xlsx")]
        public async Task ReplaceSelected_Italics_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Styling/";
            var suffix = "-ReplaceSelected_Italics";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, italics: true);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);
            Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.docx", "output-resume-underline.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-underline.xlsx")]
        public async Task ReplaceSelected_Underline_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Styling/";
            var suffix = "-ReplaceSelected_Underline";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, underline: true);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);
            Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.docx", "output-resume-strikethrough.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-strikethrough.xlsx")]
        public async Task ReplaceSelected_Strikethrough_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Styling/";
            var suffix = "-ReplaceSelected_Strikethrough";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, strikethrough: true);

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);
            Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.docx", "output-resume-highlight-red.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-highlight-red.xlsx")]
        public async Task ReplaceSelected_HighlightRed_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Styling/";
            var suffix = "-ReplaceSelected_HighlightRed";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, highlightColor: new Color() { A = 255, R = 255, G = 0, B = 0 });

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);
            Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.docx", "output-resume-text-color-blue.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-text-color-blue.xlsx")]
        public async Task ReplaceSelected_TextColorBlue_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Styling/";
            var suffix = "-ReplaceSelected_TextColorBlue";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(vm, textColor: new Color() { A = 255, R = 0, G = 0, B = 255 });

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);
            Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("source-resume.docx", "output-resume-all-styles.docx")]
        [InlineData("source-financial-sample.xlsx", "output-financial-sample-all-styles.xlsx")]
        public async Task ReplaceSelected_AllStyling_ReturnsTrueAndReplacementsPerformed(string sourceFileName, string MockFileName)
        {
            // Arrange
            var vm = new OutputViewModel();
            VMHelper.RegisterMessenger(vm);

            // set up the replacements
            ReplaceViewModel.SetNewReplacePhrasesFromFile(RelativeReplacementsPath + "replacements-abbreviations.csv");

            // OutputFiles gets updated by the SourceFiles VM/Model
            SourcesViewModel.RemoveAllSourceFiles();

            // relative file path to the TextReplace.Tests folder
            var sourcePath = RelativeSourcesPath + "Normal/";
            var outputPath = RelativeOutputsPath + "Styling/";
            var suffix = "-ReplaceSelected_AllStyling";

            var sourceFiles = new List<string>()
            {
                sourcePath + sourceFileName
            };
            SourcesViewModel.AddNewSourceFiles(sourceFiles);
            SourcesViewModel.UpdateAllSourceFileSuffixes(suffix);
            SourcesViewModel.UpdateAllSourceFileOutputDirectories(RelativeGeneratedFilePath);
            OutputViewModel.SetSelectedFileCommand.Execute(vm.OutputFiles.First());

            var generatedFileName = vm.OutputFiles[0].FileName;

            SetOutputSettings(
                vm, bold: true, italics: true, underline: true, strikethrough: true,
                highlightColor: new Color() { A = 255, R = 255, G = 0, B = 0 },
                textColor: new Color() { A = 255, R = 0, G = 0, B = 255 });

            // Act
            bool actual = await OutputViewModel.ReplaceSelected(false);

            // Assert
            Assert.True(actual);
            Assert.True(FileComparer.FilesAreEqual_OpenXml(outputPath + MockFileName, generatedFileName));

            // Cleanup
            // File.Delete(generatedFileName);
            VMHelper.UnregisterMessenger(vm);
        }

        private static void SetOutputSettings(OutputViewModel vm,
            bool wholeWord = false,
            bool caseSensitive = false,
            bool preserveCase = false,
            bool bold = false,
            bool italics = false,
            bool underline = false,
            bool strikethrough = false,
            Color? highlightColor = null,
            Color? textColor = null)
        {
            if (vm.IsWholeWord != wholeWord)
            {
                OutputViewModel.ToggleWholeWordCommand.Execute(null);
            }

            if (vm.IsCaseSensitive != caseSensitive)
            {
                OutputViewModel.ToggleCaseSensitiveCommand.Execute(null);
            }

            if (vm.IsPreserveCase != preserveCase)
            {
                OutputViewModel.TogglePreserveCaseCommand.Execute(null);
            }

            var isHighlighted = highlightColor != null;
            var isTextColored = textColor != null;

            OutputViewModel.SetOutputFilesStyling(bold, italics, underline, strikethrough, isHighlighted, isTextColored, highlightColor ?? new Color(), textColor ?? new Color());
        }
    }
}
