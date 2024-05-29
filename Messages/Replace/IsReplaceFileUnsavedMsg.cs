using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.Core.Validation;

namespace TextReplace.Messages.Replace
{
    class IsReplaceFileUnsavedMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
