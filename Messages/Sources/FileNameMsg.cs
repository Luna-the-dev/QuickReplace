using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Sources
{
    class SuffixMsg(string value) : ValueChangedMessage<string>(value)
    {
    }
}
