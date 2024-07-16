using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class CaseSensitiveMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
