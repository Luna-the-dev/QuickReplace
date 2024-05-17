using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class IsPhraseSelectedMsg : ValueChangedMessage<bool>
    {
        public IsPhraseSelectedMsg(bool value) : base(value)
        {
        }
    }
}
