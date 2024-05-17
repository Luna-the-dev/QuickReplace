using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class HasHeaderMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
