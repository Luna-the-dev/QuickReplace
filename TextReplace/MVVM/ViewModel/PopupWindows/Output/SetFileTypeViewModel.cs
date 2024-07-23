using CommunityToolkit.Mvvm.ComponentModel;
using TextReplace.Core.Enums;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    public partial class SetOutputFileTypeViewModel : ObservableRecipient
    {

        [ObservableProperty]
        private int _outputFileType = 0;
        partial void OnOutputFileTypeChanged(int value)
        {
            // check if value exists in the enum
            if (!Enum.IsDefined(typeof(OutputFileTypeEnum), value))
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
