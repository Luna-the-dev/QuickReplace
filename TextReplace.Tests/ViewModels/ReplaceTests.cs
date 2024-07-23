using TextReplace.MVVM.ViewModel;
using TextReplace.MVVM.Model;
using TextReplace.Tests.Common;
using System.Diagnostics;
using System.Text;

namespace TextReplace.Tests.ViewModels
{
    public class ReplaceTests
    {
        private static readonly string RelativeReplacementsPath = "../../../MockFiles/ReplaceTests/";
        private static readonly string RelativeGeneratedFilePath = "../../GeneratedTestFiles/ReplaceTests/";

        [Fact]
        public void OnFullFileNameChanged_ValidFileName_FileNameIsParsed()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            vm.FileName = "";
            vm.IsFileSelected = false;

            var tempFileName = Path.GetTempFileName();
            var expected = Path.GetFileName(tempFileName);

            // Act
            vm.FullFileName = tempFileName;

            // Assert
            Assert.Equal(expected, vm.FileName);
            Assert.True(vm.IsFileSelected);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void OnReplacePhrasesChanged_ChangingReplacePhrases_UpdatesDoesReplacePhraseExist()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            // Act
            var firstActual = vm.DoesReplacePhraseExist;

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            var secondActual = vm.DoesReplacePhraseExist;

            vm.RemoveSelectedPhrase();
            var thirdActual = vm.DoesReplacePhraseExist;

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            var fourthActual = vm.DoesReplacePhraseExist;

            ReplaceViewModel.RemoveAllPhrases();
            var fifthActual = vm.DoesReplacePhraseExist;

            // Assert
            Assert.False(firstActual);
            Assert.True(secondActual);
            Assert.False(thirdActual);
            Assert.True(fourthActual);
            Assert.False(fifthActual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void OnSelectedPhraseChanged_ChangingSelectedPhrase_UpdatesIsPhraseSelected()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            // Act
            var actual = vm.IsPhraseSelected;

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            var secondActual = vm.IsPhraseSelected;

            // Assert
            Assert.False(actual);
            Assert.True(secondActual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData("hello")]
        public void OnSearchTextChanged_SearchTextChanged_FiltersReplacePhrasesByItem1(string filter)
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("hello", "this should exist", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("Item1", "this should get filtered out", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("xxHello!xx", "this should also exist", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.SearchText = filter;

            // Assert
            Assert.Equal(2, vm.ReplacePhrases.Count);
            Assert.Equal("hello", vm.ReplacePhrases[0].Item1);
            Assert.Equal("xxHello!xx", vm.ReplacePhrases[1].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData("hello")]
        public void OnSearchTextChanged_SearchTextChanged_FiltersReplacePhrasesByItem2(string filter)
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("this should exist", "hello", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("Item1", "this should get filtered out", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("this should also exist", "xxHello!xx", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.SearchText = filter;

            // Assert
            Assert.Equal(2, vm.ReplacePhrases.Count);
            Assert.Equal("this should exist", vm.ReplacePhrases[0].Item1);
            Assert.Equal("this should also exist", vm.ReplacePhrases[1].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewPhrase_AddPhrasesAtTop_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            // Act
            vm.AddNewPhrase("a", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.AddNewPhrase("c", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Assert
            Assert.Equal("c", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("a", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewPhrase_AddPhrasesAtBottom_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            // Act
            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewPhrase_AddPhrasesAboveSelected_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            // Act
            vm.AddNewPhrase("a", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.AboveSelection);
            vm.AddNewPhrase("c", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.AboveSelection);

            // Assert
            Assert.Equal("c", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("a", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewPhrase_AddPhrasesBelowSelected_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            // Act
            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.BelowSelection);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.BelowSelection);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void AddNewPhrase_AddDuplicateItem1_ReturnFalse()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;
            var duplicateItem1 = "Item1";

            // Act
            vm.AddNewPhrase(duplicateItem1, "this is fine", Core.Enums.InsertReplacePhraseAtEnum.Top);
            var actual = vm.AddNewPhrase(duplicateItem1, "this should return false", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Assert
            Assert.False(actual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void EditSelectedPhrase_ChangeItem1AndItem2_SelectedPhraseUpdated()
        {
            // Arrange
            var expectedItem1 = "new Item1";
            var expectedItem2 = "new Item2";

            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("original Item1", "original Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Act
            vm.EditSelectedPhrase(expectedItem1, expectedItem2);

            // Assert
            Assert.Equal(expectedItem1, vm.ReplacePhrases[0].Item1);
            Assert.Equal(expectedItem1, vm.SelectedPhrase.Item1);
            Assert.Equal(expectedItem2, vm.ReplacePhrases[0].Item2);
            Assert.Equal(expectedItem2, vm.SelectedPhrase.Item2);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void EditSelectedPhrase_ChangeOnlyItem2_SelectedPhraseUpdated()
        {
            // Arrange
            var expected = "new Item2";

            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "original Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Act
            vm.EditSelectedPhrase("Item1", expected);

            // Assert
            Assert.Equal(expected, vm.ReplacePhrases[0].Item2);
            Assert.Equal(expected, vm.SelectedPhrase.Item2);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void EditSelectedPhrase_EmptySelectedPhrase_ReturnFalse()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.SelectedPhrase = new();

            // Act
            var actual = vm.EditSelectedPhrase("aaa", "bbb");

            // Assert
            Assert.False(actual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void RemoveSelectedPhrase_ValidSelectedPhrase_SelectedPhraseRemoved()
        {
            // Arrange
            var expectedItem1 = "expected Item1";

            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase(expectedItem1, "expected Item2", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("non-expected Item1", "non-expected Item2", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.RemoveSelectedPhrase();

            // Assert
            Assert.Single(vm.ReplacePhrases);
            Assert.Equal(expectedItem1, vm.ReplacePhrases[0].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void RemoveSelectedPhrase_EmptySelectedPhrase_ReturnFalse()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.SelectedPhrase = new();

            // Act
            var actual = vm.RemoveSelectedPhrase();

            // Assert
            Assert.False(actual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void RemoveSelectedPhrase_NonexistentSelectedPhrase_ReturnFalse()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.SelectedPhrase = new("This phrase doesnt exist", "");

            // Act
            var actual = vm.RemoveSelectedPhrase();

            // Assert
            Assert.False(actual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void RemoveAllPhrases_AtLeastOnePhrase_PhrasesAreEmpty()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Act
            ReplaceViewModel.RemoveAllPhrases();

            // Assert
            Assert.Empty(vm.ReplacePhrases);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void RemoveAllPhrases_NoPhrases_PhrasesAreEmpty()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);

            // Act
            ReplaceViewModel.RemoveAllPhrases();

            // Assert
            Assert.Empty(vm.ReplacePhrases);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void MoveReplacePhrase_LastToFirst_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            ReplaceViewModel.MoveReplacePhrase(oldIndex: 2, newIndex: 0);

            // Assert
            Assert.Equal("c", vm.ReplacePhrases[0].Item1);
            Assert.Equal("a", vm.ReplacePhrases[1].Item1);
            Assert.Equal("b", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void MoveReplacePhrase_FirstToMiddle_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            ReplaceViewModel.MoveReplacePhrase(oldIndex: 0, newIndex: 2);

            // Assert
            Assert.Equal("b", vm.ReplacePhrases[0].Item1);
            Assert.Equal("a", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void MoveReplacePhrase_FirstToFirst_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            ReplaceViewModel.MoveReplacePhrase(oldIndex: 0, newIndex: 0);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void MoveReplacePhrase_LastToLast_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            ReplaceViewModel.MoveReplacePhrase(oldIndex: 2, newIndex: 2);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void MoveReplacePhrase_InvalidOldIndex_ReturnsFalse(int oldIndex)
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            var actual = ReplaceViewModel.MoveReplacePhrase(oldIndex: oldIndex, newIndex: 2);

            // Assert
            Assert.False(actual);
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void MoveReplacePhrase_InvalidNewIndex_ReturnsFalse(int newIndex)
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            var actual = ReplaceViewModel.MoveReplacePhrase(oldIndex: 0, newIndex: newIndex);

            // Assert
            Assert.False(actual);
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void ToggleSort_IsToggled_PhrasesAreSorted()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("b", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("a", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("d", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.ToggleSortCommand.Execute(null);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);
            Assert.Equal("d", vm.ReplacePhrases[3].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void ToggleSort_IsToggled_PhrasesAreUnsorted()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = true;

            vm.AddNewPhrase("b", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("a", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("d", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.ToggleSortCommand.Execute(null);

            // Assert
            Assert.Equal("b", vm.ReplacePhrases[0].Item1);
            Assert.Equal("a", vm.ReplacePhrases[1].Item1);
            Assert.Equal("d", vm.ReplacePhrases[2].Item1);
            Assert.Equal("c", vm.ReplacePhrases[3].Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SetSelectedPhrase_ValidPhrase_PhraseIsSetAsSelectedPhrase()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            // Act
            ReplaceViewModel.SetSelectedPhraseCommand.Execute(new ReplacePhraseWrapper("Item1", "Item2"));

            // Assert
            Assert.Equal("Item1", vm.SelectedPhrase.Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SetSelectedPhrase_InvalidPhrase_SelectedPhraseShouldNotChange()
        {
            // Arrange
            var expected = "original selected phrase Item1";

            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase(expected, "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            ReplaceViewModel.SetSelectedPhraseCommand.Execute(null);

            // Assert
            Assert.Equal(expected, vm.SelectedPhrase.Item1);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("replacements.csv")]
        [InlineData("replacements.tsv")]
        [InlineData("replacements.xlsx")]
        [InlineData("replacements-pound-delimiter.txt", "#")]
        [InlineData("replacements-semicolon-delimiter.txt", ";")]
        public void SavePhrasesToFile_ValidPhrases_PhrasesSavedToFile(string filename, string delimiter = "")
        {
            // Arrange
            var validReplacePhrases = new List<(string, string)>
            {
                ("basic-text", "basic-text1"),
                ("text with whitespace", "text with whitespace 1"),
                ("text,with,commas", "text,with,commas,1"),
                ("text,with\",commas\"and\"quotes,\"", "text,with\",commas\"and\"quotes,\"1"),
                ("text;with;semicolons", "text;with;semicolons1")
            };

            var verificationFilename = RelativeReplacementsPath + filename;
            var tempFilename = RelativeGeneratedFilePath + filename;

            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            foreach (var phrase in validReplacePhrases)
            {
                vm.AddNewPhrase(phrase.Item1, phrase.Item2, Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            }

            // Act
            vm.SavePhrasesToFile(tempFilename, delimiter);

            // Assert
            if (Path.GetExtension(filename) == ".xlsx")
            {
                // Custom method for comparing excel files due to
                // unique ids being generated for each file by OpenXML
                Assert.True(FileComparer.FilesAreEqual_OpenXml(verificationFilename, tempFilename));
            }
            else
            {
                Assert.True(FileComparer.FilesAreEqual(verificationFilename, tempFilename));
            }

            // Cleanup
            // File.Delete(tempFilename);
            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SavePhrasesToFile_InvalidFileType_ExceptionThrown()
        {
            // Arrange
            var filename = Path.GetTempFileName();
            var delimiter = "";

            var validReplacePhrases = new List<(string, string)>
            {
                ("basic-text", "basic-text1"),
                ("text with whitespace", "text with whitespace 1"),
                ("text,with,commas", "text,with,commas,1"),
                ("text,with\",commas\"and\"quotes,\"", "text,with\",commas\"and\"quotes,\"1"),
                ("text;with;semicolons", "text;with;semicolons1")
            };

            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            foreach (var phrase in validReplacePhrases)
            {
                vm.AddNewPhrase(phrase.Item1, phrase.Item2, Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            }

            // Act
            var actual = vm.SavePhrasesToFile(filename, delimiter);

            // Assert
            Assert.False(actual);

            // Cleanup
            File.Delete(filename);
            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\n")]
        public void SavePhrasesToFile_InvalidDelimiterForTextFile_ExceptionThrown(string delimiter)
        {
            // Arrange
            var filename = @"./filename.txt";

            var validReplacePhrases = new List<(string, string)>
            {
                ("basic-text", "basic-text1"),
                ("text with whitespace", "text with whitespace 1"),
                ("text,with,commas", "text,with,commas,1"),
                ("text,with\",commas\"and\"quotes,\"", "text,with\",commas\"and\"quotes,\"1"),
                ("text;with;semicolons", "text;with;semicolons1")
            };

            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            foreach (var phrase in validReplacePhrases)
            {
                vm.AddNewPhrase(phrase.Item1, phrase.Item2, Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            }

            // Act
            var actual = vm.SavePhrasesToFile(filename, delimiter);

            // Assert
            Assert.False(actual);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("replacements.csv")]
        [InlineData("replacements.tsv")]
        [InlineData("replacements.xlsx")]
        [InlineData("replacements-pound-delimiter.txt")]
        [InlineData("replacements-semicolon-delimiter.txt")]
        public void SetNewReplacePhrasesFromFile_ValidPhrasesValidFile_ReplacePhrasesAreParsed(string filename)
        {
            // Arrange
            var replacePhrases = new List<(string, string)>
            {
                ("basic-text", "basic-text1"),
                ("text with whitespace", "text with whitespace 1"),
                ("text,with,commas", "text,with,commas,1"),
                ("text,with\",commas\"and\"quotes,\"", "text,with\",commas\"and\"quotes,\"1"),
                ("text;with;semicolons", "text;with;semicolons1")
            };

            var mockFileName = RelativeReplacementsPath + filename;

            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);

            // Act
            ReplaceViewModel.SetNewReplacePhrasesFromFile(mockFileName);

            // convert the vm's ReplacePhrases into a List<(string, string)>
            var actualReplacePhrases = new List<(string, string)>(vm.ReplacePhrases.Select(x => (x.Item1, x.Item2)));
            var actualDryRun = ReplaceData.SetNewReplacePhrasesFromFile(mockFileName, dryRun: true);

            // Assert
            Assert.Equal(replacePhrases, actualReplacePhrases);
            Assert.True(actualDryRun);

            VMHelper.UnregisterMessenger(vm);
        }

        [Fact]
        public void SetNewReplacePhrasesFromFile_InvalidFileType_ReturnFalse()
        {
            // Arrange
            var filename = Path.GetTempFileName();

            // Act
            var actual = ReplaceViewModel.SetNewReplacePhrasesFromFile(filename);
            var actualDryRun = ReplaceData.SetNewReplacePhrasesFromFile(filename, dryRun: true);

            // Assert
            Assert.False(actual);
            Assert.False(actualDryRun);
        }

        [Theory]
        [InlineData("invalid-replacements-empty-item1.csv")]
        [InlineData("invalid-replacements-duplicate-item1.tsv")]
        public void SetNewReplacePhrasesFromFile_InvalidPhrases_ReturnFalse(string filename)
        {
            // Arrange
            var mockFileName = RelativeReplacementsPath + filename;

            // Act
            var actual = ReplaceViewModel.SetNewReplacePhrasesFromFile(mockFileName);
            var actualDryRun = ReplaceData.SetNewReplacePhrasesFromFile(mockFileName, dryRun: true);

            // Assert
            Assert.False(actual);
            Assert.False(actualDryRun);
        }

        [Fact]
        public void CreateNewReplacePhrasesAndFile_IsCalled_ReplacePhrasesAreEmpty()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            VMHelper.RegisterMessenger(vm);

            // Act
            ReplaceViewModel.CreateNewReplacePhrasesAndFile();

            // Assert
            Assert.Empty(vm.ReplacePhrases);
            Assert.Equal("Untitled", vm.FileName);

            VMHelper.UnregisterMessenger(vm);
        }
    }
}
