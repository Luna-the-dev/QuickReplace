using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages
{
    class WindowLocationMsg((double, double) value) : ValueChangedMessage<(double, double)>(value)
    {
    }
}
