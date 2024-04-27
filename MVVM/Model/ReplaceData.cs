using TextReplace.Core;
using Microsoft.VisualBasic.FileIO;
using TextReplace.Core.AhoCorasick;
using System.Diagnostics;
using System.IO;

namespace TextReplace.MVVM.Model
{
    class ReplaceData
    {
        // key is the phrase to replace, value is what it is being replaced with
        private Dictionary<string, string> _replacePhrases = new Dictionary<string, string>();
        public Dictionary<string, string> ReplacePhrases
        {
            get { return _replacePhrases; }
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

        private bool _caseSensitive = false;
        public bool CaseSensitive
        {
            get { return _caseSensitive; }
            set { _caseSensitive = value; }
        }

        public ReplaceData(bool caseSensitive)
        {
            CaseSensitive = caseSensitive;
        }

        public bool SaveReplacePhrases()
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
        /// A dictionary of pairs of the values from the file. If one of the lines in the file has an
        /// incorrect number of values or if the operation fails for another reason, return an empty list.
        /// </returns>
        public Dictionary<string, string> ParseXSV(string fileName, string delimiter = ",")
        {
            var phrases = new Dictionary<string, string>();

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
                            return new Dictionary<string, string>();
                        }
                        phrases[line[0]] = line[1];
                    }
                    catch (MalformedLineException ex)
                    {
                        return new Dictionary<string, string>();
                    }
                }
            }
            return phrases;
        }

        /// <summary>
        /// Searches through a list of source files, looking for instances of keys from 
        /// the ReplacePhrases dict, replacing them with the associated value, and then
        /// saving the resulting text off to a list of destination files.
        /// 
        /// Note: if writing to one of the files failed, the function will continue to
        /// write to the other files in the list.
        /// </summary>
        /// <param name="srcFiles"></param>
        /// <param name="destFiles"></param>
        /// <returns>False if writing to one of the files failed.</returns>
        public bool PerformReplacements(List<string> srcFiles, List<string> destFiles)
        {
            // construct the automaton and fill it with the phrases to search for
            // also create a list of the replacement phrases to go alongside the 
            AhoCorasickStringSearcher matcher = new AhoCorasickStringSearcher(CaseSensitive);
            foreach (var searchWord in ReplacePhrases)
            {
                matcher.AddItem(searchWord.Key);
            }
            matcher.CreateFailureFunction();

            // do the search on each file
            bool didEverythingSucceed = true;
            for (int i = 0; i < srcFiles.Count; i++)
            {
                bool res = WriteReplacementsToFile(srcFiles[i], destFiles[i], matcher);
                if (res == false)
                {
                    Debug.WriteLine("Something went wrong in PerformReplacements()");
                    didEverythingSucceed = false;
                }
            }

            return didEverythingSucceed;
        }

        /// <summary>
        /// Utility function used by PerformReplacements which will use the
        /// Aho-Corasick string searcher to replace any matches found in the
        /// source file to the destination file.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <returns>False is some exception was thrown.</returns>
        private bool WriteReplacementsToFile(string src, string dest, AhoCorasickStringSearcher matcher)
        {
            try
            {
                using (var sw = new StreamWriter(dest))
                {
                    foreach (string line in File.ReadLines(src))
                    {
                        // search the current line for any text that should be replaced
                        var matches = matcher.Search(line);

                        // save an offset to remember how much the position of each replacement
                        // should be shifted if a replacement was already made on the same line
                        int offset = 0;
                        string updatedLine = line;
                        foreach (var m in matches)
                        {
                            updatedLine = updatedLine.Remove(m.Position + offset, m.Text.Length)
                                                     .Insert(m.Position + offset, ReplacePhrases[m.Text]);
                            offset += ReplacePhrases[m.Text].Length - m.Text.Length;
                        }
                        sw.WriteLine(updatedLine);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to write from {src} to {dest} due to {e.Message}");
                return false;
            }

            return true;
        }

    }
}
