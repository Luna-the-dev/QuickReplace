using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    public class AreReplacePhrasesSortedMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
