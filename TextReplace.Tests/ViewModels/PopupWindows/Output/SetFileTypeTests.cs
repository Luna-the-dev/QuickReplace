using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.Tests.ViewModels.PopupWindows.Output
{
    public class SetFileTypeTests
    {
        [Fact]
        public void OnOutputFileTypeChanged_InvalidEnumValue_ThrowsException()
        {
            // Arrange
            var vm = new SetOutputFileTypeViewModel();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => { vm.OutputFileType = 3; });
        }
    }
}
