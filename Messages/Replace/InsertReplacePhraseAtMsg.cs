using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.Core.Enums;

namespace TextReplace.Messages.Replace
{
    class InsertReplacePhraseAtMsg : ValueChangedMessage<InsertReplacePhraseAtEnum>
    {
        public InsertReplacePhraseAtMsg(InsertReplacePhraseAtEnum value) : base(value)
        {
        }
    }
}
