using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    public class PreserveCaseMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
