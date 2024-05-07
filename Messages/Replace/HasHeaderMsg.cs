using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class HasHeaderMsg : ValueChangedMessage<bool>
    {
        public HasHeaderMsg(bool value) : base(value)
        {
        }
    }
}
