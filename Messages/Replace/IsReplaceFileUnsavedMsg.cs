using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    public class IsReplaceFileUnsavedMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
