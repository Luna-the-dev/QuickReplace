using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.Core.Validation;

namespace TextReplace.Messages.Replace
{
    class SetReplacePhrasesMsg(List<ReplacePhrasesWrapper> value) : ValueChangedMessage<List<ReplacePhrasesWrapper>>(value)
    {
    }
}
