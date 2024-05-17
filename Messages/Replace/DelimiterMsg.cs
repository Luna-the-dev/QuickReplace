using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class DelimiterMsg(string value) : ValueChangedMessage<string>(value)
    {
    }
}
