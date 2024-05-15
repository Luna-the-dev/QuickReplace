using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class SelectedPhraseMsg : ValueChangedMessage<(string, string)>
    {
        public SelectedPhraseMsg((string, string) value) : base(value)
        {
        }
    }
}
