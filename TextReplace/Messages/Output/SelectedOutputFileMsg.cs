using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Replace
{
    class SelectedOutputFileMsg(OutputFile value) : ValueChangedMessage<OutputFile>(value)
    {
    }
}
