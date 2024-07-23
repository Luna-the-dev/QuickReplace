using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Output
{
    public class PreserveCaseMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
