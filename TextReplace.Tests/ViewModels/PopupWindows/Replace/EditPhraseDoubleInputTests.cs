using TextReplace.MVVM.Model;
using TextReplace.MVVM.ViewModel;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.Tests.ViewModels.PopupWindows.Replace
{
    public class EditPhraseDoubleInputTests
    {
        [Theory]
        [InlineData("Item1")]
        [InlineData("New Item1")]
        public void OnTopInputTextChanged_ValidItem1_ConfirmIsClickableIsTrue(string item1Input)
        {
            // Arrange
            var replaceVm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;
            replaceVm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            var vm = new EditPhraseDoubleInputViewModel();

            // Act
            vm.TopInputText = item1Input;

            // Assert
            Assert.True(vm.ConfirmIsClickable);
        }
    }
}
