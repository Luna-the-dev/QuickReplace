using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Replace
{
    public class SourceFilesMsg(List<SourceFile> value) : ValueChangedMessage<List<SourceFile>>(value)
    {
    }
}
