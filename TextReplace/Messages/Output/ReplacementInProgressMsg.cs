using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Output
{
    public class ReplacementInProgressMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
