using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Output
{
    public class OutputFilesMsg(List<OutputFile> value) : ValueChangedMessage<List<OutputFile>>(value)
    {
    }
}
