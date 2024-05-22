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
                File.Open(fileName, FileMode.Open, FileAccess.Read).Dispose();
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
        /// Checks to see if the provided file exists and is readable/writable.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if it is both readable and exists</returns>
        public static bool IsInputFileReadWriteable(string fileName)
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
        /// Checks to see if a directory is writable
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="throwIfFails"></param>
        /// <returns>Returns false if directory is not writable</returns>
        public static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                string path = Path.Combine(dirPath, Path.GetRandomFileName());
                using FileStream fs = File.Create(path, 1, FileOptions.DeleteOnClose);
                return true;
            }
            catch
            {
                if (throwIfFails)
                {
                    throw;
                }
                else
                {
                    return false;
                }
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

        /// <summary>
        /// Checks to see if the file is of a supported type
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>False if the file type is not supported</returns>
        public static bool IsFileTypeValid(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".csv" or ".tsv" or ".xls" or ".xlsx" or ".txt" or ".text" => true,
                _ => false
            };
        }
    }
}
