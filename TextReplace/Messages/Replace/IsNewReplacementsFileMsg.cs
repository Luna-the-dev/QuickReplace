using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    public class IsNewReplacementsFileMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
