using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class SetReplacePhrasesMsg(List<(string, string)> value) : ValueChangedMessage<List<(string, string)>>(value)
    {
    }
}
