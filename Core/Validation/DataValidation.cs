using System.Diagnostics;
using CsvHelper;
using System.IO;
using System.Globalization;
using CsvHelper.Configuration;

namespace TextReplace.Core.Validation
{
    class DataValidation
    {
        private const string INVALID_DELIMITER_CHARS = "\n";
        private const string INVALID_SUFFIX_CHARS = "<>:\"/\\|?*\n\t";

        /// <summary>
        /// Checks if the delimiter string is empty or contains any invalid characters.
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns>True if the string is empty or does not contain any invalid characters.</returns>
        public static bool IsDelimiterValid(string delimiter)
        {
            if (delimiter == string.Empty)
            {
                return false;
            }

            foreach (char c in delimiter)
            {
                if (INVALID_DELIMITER_CHARS.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if the suffix string contains any invalid characters.
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns>True if the string is empty or does not contain any invalid characters.</returns>
        public static bool IsSuffixValid(string suffix)
        {
            foreach (char c in suffix)
            {
                if (INVALID_SUFFIX_CHARS.Contains(c) || char.IsControl(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Parses "delimiter seperated value" files such as .csv or .tsv. Defaults to .csv files.
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns>
        /// A dictionary of pairs of the values from the file. If one of the lines in the file has an
        /// incorrect number of values or if the operation fails for another reason, return an empty list.
        /// </returns>
        public static Dictionary<string, string> ParseDSV(string fileName,
                                                          string delimiter = ",",
                                                          TrimOptions trimOptions = TrimOptions.None)
        {
            var phrases = new Dictionary<string, string>();

            using var reader = new StreamReader(fileName);
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                HasHeaderRecord = false,
                TrimOptions = trimOptions
            };
            using var csv = new CsvReader(reader, csvConfig);

            var records = csv.GetRecords<ReplacePhrasesWrapper>();

            foreach (var record in records)
            {
                if (record.Item1 == string.Empty)
                {
                    Debug.WriteLine("A field within the first column of the replace file is empty.");
                    continue;
                }

                phrases[record.Item1] = record.Item2;
            }

            if (phrases.Count == 0)
            {
                throw new InvalidOperationException("The dictionary returned by ParseDSV is empty.");
            }

            return phrases;
        }
    }
 }
