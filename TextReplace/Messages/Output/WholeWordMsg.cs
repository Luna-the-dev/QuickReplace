using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Output
{
    public class WholeWordMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
