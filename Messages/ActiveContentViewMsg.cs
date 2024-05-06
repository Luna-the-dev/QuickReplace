using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages
{
    class ActiveContentViewMsg : ValueChangedMessage<object>
    {
        public ActiveContentViewMsg(object value) : base(value)
        {
        }
    }
}
