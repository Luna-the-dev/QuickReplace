using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class FileNameMsg : ValueChangedMessage<string>
    {
        public FileNameMsg(string value) : base(value)
        {
        }
    }
}
