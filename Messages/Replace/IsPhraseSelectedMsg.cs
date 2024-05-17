using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class SelectedPhraseMsg((string, string) value) : ValueChangedMessage<(string, string)>(value)
    {
    }
}
