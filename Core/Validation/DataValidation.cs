using System.Diagnostics;
using CsvHelper;
using System.IO;
using System.Globalization;
using CsvHelper.Configuration;

namespace TextReplace.Core.Validation
{
    class DataValidation
    {
        /// <summary>
        /// Verifies that the replace phrases are valid by checking if the
        /// dictionary has entries and that all keys are non-empty.
        /// </summary>
        /// <param name="phrases"></param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool AreReplacePhrasesValid(Dictionary<string, string> phrases)
        {
            if (phrases.Count == 0)
            {
                return false;
            }

            foreach (var phrase in phrases)
            {
                // if the thing to replace is empty, its invalid
                if (phrase.Key == string.Empty)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Verifies that the replace phrases are valid by checking if the
        /// dictionary has entries and that all keys are non-empty.
        /// </summary>
        /// <param name="phrases"></param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool AreReplacePhrasesValid(List<(string, string)> phrases)
        {
            if (phrases.Count == 0)
            {
                return false;
            }

            foreach (var phrase in phrases)
            {
                // if the thing to replace is empty, its invalid
                if (phrase.Item1 == string.Empty)
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
                if (record.OldText == string.Empty)
                {
                    Debug.WriteLine($"A field within the first column of the replace file is empty.");
                    continue;
                }

                phrases[record.OldText] = record.NewText;
            }

            return phrases;
        }
    }

    /// <summary>
    /// Wrapper class for the replace phrases dictionary.
    /// This wrapper exists only to read in data with CsvHelper.
    /// </summary>
    public class ReplacePhrasesWrapper
    {
        public string OldText { get; set; } = "";
        public string NewText { get; set; } = "";
    }
}
