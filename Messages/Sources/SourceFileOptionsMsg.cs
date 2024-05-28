using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Sources
{
    class SourceFileOptionsMsg(Dictionary<string, SourceFileOptions> value) : ValueChangedMessage<Dictionary<string, SourceFileOptions>>(value)
    {
    }
}
