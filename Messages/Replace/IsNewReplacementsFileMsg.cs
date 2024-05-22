using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class IsNewReplacementsFileMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
