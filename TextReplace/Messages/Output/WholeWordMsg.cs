using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class WholeWordMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
