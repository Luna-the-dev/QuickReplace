using TextReplace.MVVM.Model;
using TextReplace.MVVM.ViewModel;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.Tests.ViewModels.PopupWindows.Replace
{
    public class AddPhraseDoubleInputTests
    {
        [Fact]
        public void OnTopInputTextChanged_ValidItem1_ConfirmIsClickableIsTrue()
        {
            // Arrange
            var replaceVm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;
            replaceVm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            var vm = new AddPhraseDoubleInputViewModel();

            // Act
            vm.TopInputText = "New Item1";

            // Assert
            Assert.True(vm.ConfirmIsClickable);
        }

        [Theory]
        [InlineData("Item1")]
        [InlineData("")]
        public void OnTopInputTextChanged_InvalidItem1_ConfirmIsClickableIsFalse(string item1Input)
        {
            // Arrange
            var replaceVm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;
            replaceVm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            var vm = new AddPhraseDoubleInputViewModel();

            // Act
            vm.TopInputText = item1Input;

            // Assert
            Assert.False(vm.ConfirmIsClickable);
        }

        [Fact]
        public void OnInsertAtChanged_InvalidEnumValue_ThrowsException()
        {
            // Arrange
            var vm = new AddPhraseDoubleInputViewModel();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => { vm.InsertAt = 4; });
        }
    }
}
