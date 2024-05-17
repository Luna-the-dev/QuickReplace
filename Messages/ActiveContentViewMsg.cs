using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages
{
    class ActiveContentViewMsg(object value) : ValueChangedMessage<object>(value)
    {
    }
}
