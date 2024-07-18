using System.Diagnostics;
using System.Text;

namespace TextReplace.Tests.Common
{
    internal class FileComparer
    {
        const int BYTES_TO_READ = sizeof(Int64);

        /// <summary>
        /// Checks to see if two files are the same.
        /// </summary>
        /// <param name="firstPath"></param>
        /// <param name="secondPath"></param>
        /// <returns>Returns true if both files are the same.</returns>
        public static bool FilesAreEqual(string firstPath, string secondPath)
        {
            try
            {
                var first = new FileInfo(firstPath);
                var second = new FileInfo(secondPath);

                if (first.Length != second.Length)
                {
                    return false;
                }

                if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

                using FileStream fs1 = first.OpenRead();
                using FileStream fs2 = second.OpenRead();

                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Checks to see if two excel files are the same.
        /// Note: both files are loaded into memory, so only use this for
        /// testing on small/medium size excel files
        /// </summary>
        /// <param name="firstPath"></param>
        /// <param name="secondPath"></param>
        /// <returns>Returns true if both excel files are the same.</returns>
        public static bool FilesAreEqual_Excel(string firstPath, string secondPath)
        {
            try
            {
                if (Path.GetExtension(firstPath) != ".xlsx" || Path.GetExtension(secondPath) != ".xlsx")
                {
                    throw new NotSupportedException("This method is designed only for .xlsx file types.");
                }

                var firstUnzippedPath = string.Format(@"{0}\{1}",
                                                      Path.GetDirectoryName(firstPath),
                                                      Path.GetFileNameWithoutExtension(firstPath));
                var secondUnzippedPath = string.Format(@"{0}\{1}",
                                                       Path.GetDirectoryName(secondPath),
                                                       Path.GetFileNameWithoutExtension(secondPath));

                // delete unzipped paths if they already existed
                if (Directory.Exists(firstUnzippedPath))
                {
                    Directory.Delete(firstUnzippedPath, true);
                }

                if (Directory.Exists(secondUnzippedPath))
                {
                    Directory.Delete(secondUnzippedPath, true);
                }

                // extract the .xlsx files to compare the relevant .xml files within
                System.IO.Compression.ZipFile.ExtractToDirectory(firstPath, firstUnzippedPath);
                System.IO.Compression.ZipFile.ExtractToDirectory(secondPath, secondUnzippedPath);

                // compare the files in the ./xl/ directory
                if (CompareXlFiles(firstUnzippedPath, secondUnzippedPath) == false)
                {
                    Debug.WriteLine("poo1");
                    return false;
                }

                // compare files in the ./xl/themes/ directory
                var firstThemePath = firstUnzippedPath + @"\xl\theme";
                var secondThemePath = secondUnzippedPath + @"\xl\theme";
                if (CompareFilesInDirectory(firstThemePath, secondThemePath) == false)
                {
                    Debug.WriteLine("poo2");
                    return false;
                }

                // compare files in the ./xl/worksheets/ directory
                var firstWorksheetsPath = firstUnzippedPath + @"\xl\worksheets";
                var secondWorksheetsPath = secondUnzippedPath + @"\xl\worksheets";
                if (CompareFilesInDirectory(firstWorksheetsPath, secondWorksheetsPath) == false)
                {
                    Debug.WriteLine("poo3");
                    return false;
                }

                // cleanup the unzipped .xlsx directories
                Directory.Delete(firstUnzippedPath, true);
                Directory.Delete(secondUnzippedPath, true);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("peepee");
                Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Iterates through two directories and checks to see if each file within the first
        /// directory is the same as the corresponding file in the second directory.
        /// </summary>
        /// <param name="firstDirectory"></param>
        /// <param name="secondDirectory"></param>
        /// <returns>
        /// Returns true if each file in the first directory is the same as
        /// the corresponding file in the second directory
        /// </returns>
        static public bool CompareFilesInDirectory(string firstDirectory, string secondDirectory)
        {
            try
            {
                var firstThemeFiles = new DirectoryInfo(firstDirectory + @"\").GetFiles();
                var secondThemeFiles = new DirectoryInfo(secondDirectory + @"\").GetFiles();

                // if one directory has more files than the other, return false
                if (firstThemeFiles.Length != secondThemeFiles.Length)
                {
                    return false;
                }

                // combine the two lists of files into one
                var files = firstThemeFiles.Zip(secondThemeFiles, (f, s) => new { First = f, Second = s });

                foreach (var file in files)
                {
                    if (FilesAreEqual(file.First.FullName, file.Second.FullName) == false)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Helper method for FilesAreEqual_Excel to check the three files within the
        /// /xl/ directory of two .xlsx files are the same
        /// </summary>
        /// <param name="firstPath"></param>
        /// <param name="secondPath"></param>
        /// <returns>True if the files within the /xl/ directory of two .xlsx files are the same</returns>
        static private bool CompareXlFiles(string firstPath, string secondPath)
        {
            var firstXlPath = firstPath + @"\xl";
            var secondXlPath = secondPath + @"\xl";

            // compare sharedStrings file
            if (FilesAreEqual(firstXlPath + @"\sharedStrings.xml", secondXlPath + @"\sharedStrings.xml") == false)
            {
                return false;
            }

            // compare styles file
            if (FilesAreEqual(firstXlPath + @"\styles.xml", secondXlPath + @"\styles.xml") == false)
            {
                return false;
            }

            // compare workbook file
            if (FilesAreEqual(firstXlPath + @"\workbook.xml", secondXlPath + @"\workbook.xml") == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a byte array's data to a string, with each byte seperated by a comma.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>String containing the byte array's data, seperated by commas</returns>
        static private string StringifyByteArray(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            
            return sb.ToString();
        }
    }
}
