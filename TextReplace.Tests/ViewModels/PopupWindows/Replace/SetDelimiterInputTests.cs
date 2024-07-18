using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.Tests.ViewModels.PopupWindows.Replace
{
    public class SetDelimiterInputTests
    {
        [Fact]
        public void OnInputTextChanged_ValidDelimiter_ConfirmIsClickableIsTrue()
        {
            // Arrange
            var vm = new SetDelimiterInputViewModel();

            // Act
            vm.InputText = ",";

            // Assert
            Assert.True(vm.ConfirmIsClickable);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\n")]
        public void OnInputTextChanged_InvalidDelimiter_ConfirmIsClickableIsFalse(string delimiter)
        {
            // Arrange
            var vm = new SetDelimiterInputViewModel();

            // Act
            vm.InputText = delimiter;

            // Assert
            Assert.False(vm.ConfirmIsClickable);
        }
    }
}
