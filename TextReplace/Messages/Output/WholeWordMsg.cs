using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    public class WholeWordMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
