using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.Core.Enums;

namespace TextReplace.Messages.Replace
{
    class InsertReplacePhraseAtMsg(InsertReplacePhraseAtEnum value) : ValueChangedMessage<InsertReplacePhraseAtEnum>(value)
    {
    }
}
