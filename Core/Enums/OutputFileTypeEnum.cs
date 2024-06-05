using System.IO;

namespace TextReplace.Core.Enums
{
    public enum OutputFileTypeEnum
    {
        KeepFileType,
        Document,
        Text,
        Pdf
    }

    class OutputFileTypeClass
    {
        public static string OutputFileTypeString(OutputFileTypeEnum fileType, string fileName)
        {
            return fileType switch
            {
                OutputFileTypeEnum.KeepFileType => Path.GetExtension(fileName),
                OutputFileTypeEnum.Text => ".txt",
                OutputFileTypeEnum.Document => ".docx",
                OutputFileTypeEnum.Pdf => ".pdf",
                _ => throw new NotImplementedException($"{fileType} is not implemented in OutputFileTypeString()")
            };
        }
    }

}
