using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Sources
{
    class DefaultSourceFileOptionsMsg(SourceFile value) : ValueChangedMessage<SourceFile>(value)
    {
    }
}
