using Config.Net;

namespace TextReplace.Core.Config
{
    public interface IStyling
    {
        bool Bold { get; set; }

        bool Italics { get; set; }

        bool Underline { get; set; }

        bool Strikethrough { get; set; }

        bool IsHighlighted { get; set; }

        bool IsTextColored { get; set; }

        [Option(DefaultValue = "#FFFFFF")]
        string HighlightColor { get; set; }

        [Option(DefaultValue = "#FFFFFF")]
        string TextColor { get; set; }
    }
}
