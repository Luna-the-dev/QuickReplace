using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class SetReplacePhrasesMsg : ValueChangedMessage<Dictionary<string, string>>
    {
        public SetReplacePhrasesMsg(Dictionary<string, string> value) : base(value)
        {
        }
    }
}
