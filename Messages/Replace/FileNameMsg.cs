using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class FileNameMsg(string value) : ValueChangedMessage<string>(value)
    {
    }
}
