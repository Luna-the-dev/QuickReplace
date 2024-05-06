using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages
{
    class FileNameMsg : ValueChangedMessage<string>
    {
        public FileNameMsg(string value) : base(value)
        {
        }
    }
}
