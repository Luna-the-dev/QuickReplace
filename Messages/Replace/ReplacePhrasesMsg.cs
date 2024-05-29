using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.Core.Validation;

namespace TextReplace.Messages.Replace
{
    class ReplacePhrasesMsg(List<ReplacePhrasesWrapper> value) : ValueChangedMessage<List<ReplacePhrasesWrapper>>(value)
    {
    }
}
