using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Output
{
    public class IsStyledMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
