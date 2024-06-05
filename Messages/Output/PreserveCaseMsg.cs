using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class PreserveCaseMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
