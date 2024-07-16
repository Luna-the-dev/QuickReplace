using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Replace
{
    public class ReplacePhrasesMsg(List<ReplacePhrase> value) : ValueChangedMessage<List<ReplacePhrase>>(value)
    {
    }
}
