using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class DelimiterMsg : ValueChangedMessage<string>
    {
        public DelimiterMsg(string value) : base(value)
        {
        }
    }
}
