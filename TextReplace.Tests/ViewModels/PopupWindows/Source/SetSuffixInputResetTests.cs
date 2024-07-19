using TextReplace.MVVM.ViewModel.PopupWindows;
using TextReplace.Tests.Common;

namespace TextReplace.Tests.ViewModels.PopupWindows.Source
{
    public class SetSuffixInputResetTests
    {
        [Fact]
        public void OnInputTextChanged_ValidInput_ConfirmIsClickableIsTrue()
        {
            // Arrange
            var vm = new SetSuffixInputResetViewModel();
            VMHelper.RegisterMessenger(vm);
            vm.InputText = "";
            vm.ConfirmIsClickable = false;

            // Act
            vm.InputText = "-replacify";

            // Assert
            Assert.True(vm.ConfirmIsClickable);

            VMHelper.UnregisterMessenger(vm);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\n")]
        public void OnInputTextChanged_InvalidInput_ConfirmIsClickableIsFalse(string suffix)
        {
            // Arrange
            var vm = new SetSuffixInputResetViewModel();
            VMHelper.RegisterMessenger(vm);
            vm.InputText = "";
            vm.ConfirmIsClickable = false;

            // Act
            vm.InputText = suffix;

            // Assert
            Assert.False(vm.ConfirmIsClickable);

            VMHelper.UnregisterMessenger(vm);
        }
    }
}
