using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class SourceFileNamesMsg(List<string> value) : ValueChangedMessage<List<string>>(value)
    {
    }
}
