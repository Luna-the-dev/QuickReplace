using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    public class SavingReplacementsInProgressMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
