using CommunityToolkit.Mvvm.ComponentModel;

namespace TextReplace.Tests.Common
{
    internal class VMHelper
    {
        public static void RegisterMessenger<T>(T viewModel) where T : ObservableRecipient
        {
            viewModel.IsActive = true;
        }

        public static void UnregisterMessenger<T>(T viewModel) where T : ObservableRecipient
        {
            viewModel.IsActive = false;
        }
    }
}
