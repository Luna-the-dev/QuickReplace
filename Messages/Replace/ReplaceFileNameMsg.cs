using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class ReplaceFileNameMsg(string value) : ValueChangedMessage<string>(value)
    {
    }
}
