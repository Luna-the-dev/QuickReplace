using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Sources
{
    public class SourceFilesMsg(List<SourceFile> value) : ValueChangedMessage<List<SourceFile>>(value)
    {
    }
}
