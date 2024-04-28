using System.Diagnostics;
using System.IO;

namespace TextReplace.Core.Validation
{
    class FileValidation
    {
        /// <summary>
        /// Checks to see if the provided file exists and is readable.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if it is both readable and exists</returns>
        public static bool IsInputFileReadable(string fileName)
        {
            try
            {
                File.Open(fileName, FileMode.Open, FileAccess.ReadWrite).Dispose();
                return true;
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("Replace file name is empty.");
                return false;
            }
            catch
            {
                Debug.WriteLine("Replace file could not be opened.");
                return false;
            }
        }

        /// <summary>
        /// Checks to see if all files in a list are readable
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns>False if the list is empty or if one of the files is not readable</returns>
        public static bool AreFileNamesValid(List<string> filenames)
        {
            if (filenames.Count == 0)
            {
                return false;
            }

            foreach (string filename in filenames)
            {
                if (IsInputFileReadable(filename) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
