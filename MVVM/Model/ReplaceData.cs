using TextReplace.Core.AhoCorasick;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.Validation;

namespace TextReplace.MVVM.Model
{
    class ReplaceData
    {
        private static string _fileName = string.Empty;
        public static string FileName
        {
            get { return _fileName; }
            set
            {
                if (FileValidation.IsInputFileReadable(value))
                {
                    _fileName = value;
                }
                else
                {
                    throw new ArgumentException("Input file is not readable. ReplaceFile was not updated.");
                }
            }
        }
        // key is the phrase to replace, value is what it is being replaced with
        private static Dictionary<string, string> _replacePhrases = new Dictionary<string, string>();
        public static Dictionary<string, string> ReplacePhrases
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
                    throw new ArgumentException("Replace phrases are not valid.");
                }
            }
        }
        // flag for whether the search should be case sensitive
        private bool _caseSensitive = false;
        public bool CaseSensitive
        {
            get { return _caseSensitive; }
            set { _caseSensitive = value; }
        }
        // delimiters which decides what seperates whole words
        private const string DELIMITERS = " \t/\\()\"'-:,.;<>~!@#$%^&*|+=[]{}?│";


        /*
         * Constructor
         */
        public ReplaceData(bool caseSensitive)
        {
            CaseSensitive = caseSensitive;
        }

        /// <summary>
        /// Opens a file dialogue and replaces the ReplaceFile with whatever the user selects (if valid)
        /// </summary>
        /// <returns>False if the user didn't select a file or if the file was invalid. True otherwise</returns>
        public static bool SetNewReplaceFileFromUser()
        {
            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Open Text File";
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "All files (*.*)|*.*"; // Filter files by extension

            // open file dialog box
            if (dialog.ShowDialog() != true)
            {
                return false;
            }

            // set the ReplaceFile name
            try
            {
                // check to see if file name is valid so that the phrases can be parsed
                // before setting the file name
                if (FileValidation.IsInputFileReadable(dialog.FileName) == false)
                {
                    throw new IOException("Input file is not readable in SetNewReplaceFileFromUser().");
                }
                // parse through phrases and attempt to save them. parser will return an
                // empty dictionary if something was wrong, so setter will throw an error
                ReplacePhrases = ParseReplacePhrases(dialog.FileName);
                // set file name directly rather than with the setter because we just ran the
                // same validation as the setter anyways
                _fileName = dialog.FileName;
            }
            catch (IOException e)
            {
                Debug.WriteLine(e);
                return false;
            }
            catch (ArgumentException e)
            {
                Debug.WriteLine(e);
                return false;
            }
            catch
            {
                Debug.WriteLine("Somewthing unexpected happened in SetNewReplaceFileFromUser().");
                return false;
            }

            return true;
        }

        public static Dictionary<string, string> ParseReplacePhrases(string fileName)
        {
            try
            {
                // if file is formatted as a delimiter seperated value
                return DataValidation.ParseDSV(fileName);
            }
            catch
            {
                Debug.WriteLine("Something went wrong in ParseReplacePhrases()");
                return new Dictionary<string, string>(); ;
            }
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
        public bool PerformReplacements(List<string> srcFiles, List<string> destFiles, bool isWholeWord)
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
                bool res = WriteReplacementsToFile(srcFiles[i], destFiles[i], matcher, isWholeWord);
                if (res == false)
                {
                    Debug.WriteLine("Something went wrong in PerformReplacements()");
                    didEverythingSucceed = false;
                }
            }

            return didEverythingSucceed;
        }

        /// <summary>
        /// A wrapper function for MatchAndWrite() and MatchAndWriteWholeWord() in order
        /// to cut down on the number of if/else checks inside of loops.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="isWholeWord"></param>
        /// <returns>False is some exception was thrown.</returns>
        private bool WriteReplacementsToFile(string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord)
        {
            try
            {
                using var sw = new StreamWriter(dest);

                // making these two seperate functions in order to cut down on if/else checks
                // inside the foreach loops (which could potentially loop millions of times each)
                if (isWholeWord)
                {
                    MatchAndWriteWholeWord(src, sw, matcher);
                }
                else
                {
                    MatchAndWrite(src, sw, matcher);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to write from {src} to {dest} due to {e.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Uses the Aho-Corasick algorithm to search a file for any substring matches
        /// in a source file. This function is wrapped by WriteReplacementsToFile() and
        /// called from PerformReplacements().
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sw"></param>
        /// <param name="matcher"></param>
        private void MatchAndWrite(string src, StreamWriter sw, AhoCorasickStringSearcher matcher)
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

        /// <summary>
        /// Uses the Aho-Corasick algorithm to search a file for any whole-word matches
        /// in a source file. This function is wrapped by WriteReplacementsToFile() and
        /// called from PerformReplacements().
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sw"></param>
        /// <param name="matcher"></param>
        private void MatchAndWriteWholeWord(string src, StreamWriter sw, AhoCorasickStringSearcher matcher)
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
                    if (IsMatchWholeWord(line, m.Text, m.Position) == false)
                    {
                        continue;
                    }
                    updatedLine = updatedLine.Remove(m.Position + offset, m.Text.Length)
                                             .Insert(m.Position + offset, ReplacePhrases[m.Text]);
                    offset += ReplacePhrases[m.Text].Length - m.Text.Length;
                }
                sw.WriteLine(updatedLine);
            }
        }

        /// <summary>
        /// Checks to see if a match found by the AhoCorasickStringSearcher
        /// Search() method is a whole word.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <returns>False if the character before and after the match exists and isnt a delimiter.</returns>
        private bool IsMatchWholeWord(string line, string text, int pos)
        {
            /*
             * yes, i know this is ugly. this can be boiled down to the following.
             * match is not a whole word if:
             *   there is a char before the match AND it is not in the list of delimiters
             *   OR
             *   there is a char after the match AND it is not found in the list of delimiters
             */
            int indexBefore = pos - 1;
            int indexAfter = pos + text.Length;
            if ( ( indexBefore >= 0
                   &&
                   DELIMITERS.Contains(line[indexBefore]) == false
                 )
                 ||
                 (indexAfter < line.Length
                 &&
                   DELIMITERS.Contains(line[indexAfter]) == false
                 )
               )
            {
                return false;
            }
            return true;
        }


    }
}
