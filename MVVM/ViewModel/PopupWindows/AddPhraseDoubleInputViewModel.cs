using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Core.Enums;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class AddPhraseDoubleInputViewModel : ObservableRecipient,
        IRecipient<IsPhraseSelectedMsg>
    {
        [ObservableProperty]
        private string _topInputText = string.Empty;
        partial void OnTopInputTextChanged(string value)
        {
            if (value == string.Empty || ReplaceData.ReplacePhrasesDict.ContainsKey(value))
            {
                ConfirmIsClickable = false;
            }
            else
            {
                ConfirmIsClickable = true;
            }
        }

        [ObservableProperty]
        private bool _confirmIsClickable = false;

        [ObservableProperty]
        private bool _isPhraseSelected = ReplaceData.IsPhraseSelected;

        [ObservableProperty]
        private int _insertAt = 0;
        partial void OnInsertAtChanged(int value)
        {
            // check if value exists in the enum
            if (!Enum.IsDefined(typeof(InsertReplacePhraseAtEnum), value))
            {
                throw new ArgumentOutOfRangeException();
            }
            WeakReferenceMessenger.Default.Send(new InsertReplacePhraseAtMsg((InsertReplacePhraseAtEnum)value));
        }

        public void Receive(IsPhraseSelectedMsg message)
        {
            IsPhraseSelected = message.Value;
        }
    }
}
