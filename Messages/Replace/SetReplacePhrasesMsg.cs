using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class SetReplacePhrasesMsg : ValueChangedMessage<List<(string, string)>>
    {
        public SetReplacePhrasesMsg(List<(string, string)> value) : base(value)
        {
        }
    }
}
