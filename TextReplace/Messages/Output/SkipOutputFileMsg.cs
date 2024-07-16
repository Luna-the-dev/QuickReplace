using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Output
{
    // the string is the file name, the bool is a flag for whether the error was due to the file being in use
    class SkipOutputFileMsg((string, bool) value) : ValueChangedMessage<(string, bool)>(value)
    {
    }
}
