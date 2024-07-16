using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    public class ReplaceFileNameMsg(string value) : ValueChangedMessage<string>(value)
    {
    }
}
