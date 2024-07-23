using Config.Net;

namespace TextReplace.Core.Config
{
    public interface IUserSettings
    {
        // output settings
        bool WholeWord { get; set; }

        bool CaseSensitive { get; set; }

        bool PreserveCase { get; set; }

        bool OpenFileLocation { get; set; }

        IStyling Styling { get; set; }
    }
}
