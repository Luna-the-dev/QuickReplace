using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Replace
{
    public class SelectedSourceFileMsg(SourceFile value) : ValueChangedMessage<SourceFile>(value)
    {
    }
}
