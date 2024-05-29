using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.Core.Validation;

namespace TextReplace.Messages.Replace
{
    class AreReplacePhrasesSortedMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
