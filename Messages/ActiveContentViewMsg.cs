using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.Core.Enums;

namespace TextReplace.Messages
{
    class ActiveContentViewMsg(object value) : ValueChangedMessage<object>(value)
    {
    }
}
