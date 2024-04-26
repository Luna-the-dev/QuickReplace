using TextReplace.Core;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace TextReplace.MVVM.Model
{
    class ReplaceData
    {
        private static List<(string, string)> _replacePhrases = new List<(string, string)>();

        public static List<(string, string)> ReplacePhrases
        {
            get => _replacePhrases;
            set
            {
                if (DataValidation.AreReplacePhrasesValid(value))
                {
                    _replacePhrases = value;
                }
                else
                {
                    throw new Exception("Replace phrases are not valid.");
                }
            }
        }

        public static bool SaveReplacePhrases()
        {
            try
            {
                // if file is a XSV
                ReplacePhrases = ParseXSV(ReplaceFile.FileName);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses "seperated value" files such as .csv or .tsv. Defaults to .csv files.
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns>
        /// A list of pairs of the values from the file. If one of the lines in the file has an
        /// incorrect number of values or if the operation fails for another reason, return an empty list.
        /// </returns>
        public static List<(string, string)> ParseXSV(string fileName, string delimiter = ",")
        {
            var phrases = new List<(string, string)>();

            // open the file with the parser
            using (var parser = new TextFieldParser(fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // parse the file line by line
                // store the values from each line into an array
                // if the line doesnt have two values, return an empty list
                string[]? line;
                while (!parser.EndOfData)
                {
                    try
                    {
                        line = parser.ReadFields();
                        if (line == null || line.Length != 2)
                        {
                            return new List<(string, string)>();
                        }
                        phrases.Add((line[0], line[1]));
                    }
                    catch (MalformedLineException ex)
                    {
                        return new List<(string, string)>();
                    }
                }
            }

            return phrases;
        }

    }
}
