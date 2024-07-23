using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    public class CaseSensitiveMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
