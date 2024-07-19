using TextReplace.MVVM.ViewModel.PopupWindows;
using TextReplace.Tests.Common;

namespace TextReplace.Tests.ViewModels.PopupWindows.Source
{
    public class SetOutputDirectoryTests
    {
        [Fact]
        public void OnDirectoryNameChanged_ValidDirectoryName_UpdatesShowDirectoryName()
        {
            // Arrange
            var vm = new SetOutputDirectoryViewModel();
            VMHelper.RegisterMessenger(vm);
            vm.DirectoryName = "";
            vm.ShowDirectoryName = false;
            vm.ConfirmIsClickable = false;

            // Act
            vm.DirectoryName = "directory-name/";

            // Assert
            Assert.True(vm.ShowDirectoryName);
            Assert.True(vm.ConfirmIsClickable);

            VMHelper.UnregisterMessenger(vm);
        }
    }
}
