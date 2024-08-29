using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages
{
    class WindowSizeMsg((double, double) value) : ValueChangedMessage<(double, double)>(value)
    {
    }
}
