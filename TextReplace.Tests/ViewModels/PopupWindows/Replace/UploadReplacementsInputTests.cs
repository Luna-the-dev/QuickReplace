using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.Tests.ViewModels.PopupWindows.Replace
{
    public class UploadReplacementsInputTests
    {
        private static readonly string RelativeReplacementsPath = "../../../MockFiles/ReplaceTests/";

        [Theory]
        [InlineData("replacements.csv")]
        [InlineData("replacements.tsv")]
        [InlineData("replacements.xlsx")]
        [InlineData("replacements-pound-delimiter.txt")]
        [InlineData("replacements-semicolon-delimiter.txt")]
        public void ValidateFile_ValidReplacePhrases_FileIsValid(string filename)
        {
            // Arrange
            var mockFileName = RelativeReplacementsPath + filename;

            var vm = new UploadReplacementsInputViewModel();

            // Act
            vm.ValidateFile(mockFileName);

            // Assert
            Assert.True(vm.ShowFileName);
            Assert.True(vm.FileIsValid);
            Assert.False(vm.FileIsInvalid);
            Assert.True(vm.ConfirmIsClickable);
        }

        [Theory]
        [InlineData("invalid-replacements-empty-item1.csv")]
        [InlineData("invalid-replacements-duplicate-item1.tsv")]
        public void ValidateFile_InvalidReplacePhrases_FileIsInvalid(string filename)
        {
            // Arrange
            var mockFileName = RelativeReplacementsPath + filename;

            var vm = new UploadReplacementsInputViewModel();

            // Act
            vm.ValidateFile(mockFileName);

            // Assert
            Assert.True(vm.ShowFileName);
            Assert.False(vm.FileIsValid);
            Assert.True(vm.FileIsInvalid);
            Assert.False(vm.ConfirmIsClickable);
        }
    }
}
