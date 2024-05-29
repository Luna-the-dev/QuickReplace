using TextReplace.Core.AhoCorasick;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.Validation;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages.Replace;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Text;
using System.ComponentModel.Design;

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
                _fileName = value;
                WeakReferenceMessenger.Default.Send(new ReplaceFileNameMsg(value));
            }
        }
        private static bool _isNewFile;
        public static bool IsNewFile
        {
            get { return _isNewFile; }
            set
            {
                _isNewFile = value;
                WeakReferenceMessenger.Default.Send(new IsNewReplacementsFileMsg(value));
            }
        }


        // key is the phrase to replace, value is what it is being replaced with
        private static Dictionary<string, string> _replacePhrasesDict = [];
        public static Dictionary<string, string> ReplacePhrasesDict
        {
            get { return _replacePhrasesDict; }
            set { _replacePhrasesDict = value; }
        }
        // key is the phrase to replace, value is what it is being replaced with
        private static List<ReplacePhrasesWrapper> _replacePhrasesList = [];
        public static List<ReplacePhrasesWrapper> ReplacePhrasesList
        {
            get { return _replacePhrasesList; }
            set
            {
                _replacePhrasesList = value;
                WeakReferenceMessenger.Default.Send(new SetReplacePhrasesMsg(value));
            }
        }
        // flag for whether the search should be case sensitive
        private bool _caseSensitive = false;
        public bool CaseSensitive
        {
            get { return _caseSensitive; }
            set { _caseSensitive = value; }
        } 
        // the delimiter used for parsing the replace file
        private static string _delimiter = string.Empty;
        public static string Delimiter
        {
            get { return _delimiter; }
            set { _delimiter = value; }
        }
        // a flag to denote whether a phrase is selected in the replace view
        private static (string, string) _selectedPhrase = ("", "");
        public static (string, string) SelectedPhrase
        {
            get { return _selectedPhrase; }
            set
            {
                _selectedPhrase = value;
                WeakReferenceMessenger.Default.Send(new SelectedPhraseMsg(value));
            }
        }
        // a flag to denote whether any modifications have been made but not saved to the file
        private static bool _isUnsaved;
        public static bool IsUnsaved
        {
            get { return _isUnsaved; }
            set
            {
                _isUnsaved = value;
                WeakReferenceMessenger.Default.Send(new IsReplaceFileUnsavedMsg(value));
            }
        }
        // a flag to denote whether the replace phrases are sorted
        private static bool _isSorted;
        public static bool IsSorted
        {
            get { return _isSorted; }
            set
            {
                _isSorted = value;
                WeakReferenceMessenger.Default.Send(new AreReplacePhrasesSortedMsg(value));
            }
        }


        // delimiters that decide what seperates whole words
        private const string WORD_DELIMITERS = " \t/\\()\"'-:,.;<>~!@#$%^&*|+=[]{}?│";

        /*
         * Constructor
         */
        public ReplaceData(bool caseSensitive)
        {
            CaseSensitive = caseSensitive;
        }

        /// <summary>
        /// Opens a file dialogue and replaces the ReplaceFile with whatever the user selects (if valid).
        /// Note: the newDelimiter parameter must be supplied if the fileName is a .txt or .text file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newDelimiter"></param>
        /// <param name="dryRun"></param>
        /// <returns>
        /// False if one of the files was invalid, null user closed the window without selecting a file.
        /// </returns>
        public static bool SetNewReplaceFile(string fileName, string? newDelimiter = null, bool dryRun = false)
        {
            // set the ReplaceFile name
            try
            {
                // check to see if file name is valid so that the phrases can be parsed
                // before setting the file name
                if (FileValidation.IsInputFileReadable(fileName) == false)
                {
                    throw new IOException("Input file is not readable in SetNewReplaceFileFromUser().");
                }

                // if the supplied file is a text file, check if there is a delimiter and if it is valid
                if (FileValidation.IsTextFile(fileName))
                {
                    if (newDelimiter == null)
                    {
                        Debug.WriteLine("Please supply a delimiter to use for a text file.");
                        return false;
                    }
                    
                    if (DataValidation.IsDelimiterValid(newDelimiter) == false)
                    {
                        Debug.WriteLine("Delimiter is invalid.");
                        return false;
                    }
                }

                // determine which delimiter should be used
                string delimiter = DetermineDelemiter(fileName, newDelimiter);

                // if caller specified that this should be a dry run,
                // then dont actually assign the parsed data to the dict
                if (dryRun)
                {
                    // will throw InvalidOperationException if it returns a dict of count == 0
                    DataValidation.ParseDSV(fileName, delimiter);
                    return true;
                }

                // parse through phrases and attempt to save them
                ReplacePhrasesDict = DataValidation.ParseDSV(fileName, delimiter);

                // put copy the dict into a list which gets used by the view models
                ReplacePhrasesList = ReplacePhrasesDict.Select(x => new ReplacePhrasesWrapper(x.Key, x.Value)).ToList();

                FileName = fileName;
                Delimiter = delimiter;
                IsNewFile = false;

                return true;
            }
            catch (IOException e)
            {
                Debug.WriteLine(e);
                return false;
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine(e);
                return false;
            }
            catch (CsvHelper.MissingFieldException)
            {
                Debug.WriteLine("CsvHelper could not parse the file with the given delimiter.");
                return false;
            }
            catch
            {
                Debug.WriteLine("Something unexpected happened in SetNewReplaceFileFromUser().");
                return false;
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
            if (srcFiles.Count == 0 || destFiles.Count == 0)
            {
                Debug.WriteLine("srcFiles or destFiles is empty");
                return false;
            }

            // construct the automaton and fill it with the phrases to search for
            // also create a list of the replacement phrases to go alongside the 
            AhoCorasickStringSearcher matcher = new AhoCorasickStringSearcher(CaseSensitive);
            foreach (var searchWord in ReplacePhrasesDict)
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
        private static bool WriteReplacementsToFile(string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord)
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
        private static void MatchAndWrite(string src, StreamWriter sw, AhoCorasickStringSearcher matcher)
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
                                             .Insert(m.Position + offset, ReplacePhrasesDict[m.Text]);
                    offset += ReplacePhrasesDict[m.Text].Length - m.Text.Length;
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
        private static void MatchAndWriteWholeWord(string src, StreamWriter sw, AhoCorasickStringSearcher matcher)
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
                                             .Insert(m.Position + offset, ReplacePhrasesDict[m.Text]);
                    offset += ReplacePhrasesDict[m.Text].Length - m.Text.Length;
                }
                sw.WriteLine(updatedLine);
            }
        }

        /// <summary>
        /// Saves the replace phrases list to the file system, performing a sort if requested.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="shouldSort"></param>
        /// <param name="delimiter"></param>
        public static void SavePhrasesToFile(string fileName, bool shouldSort, string? newDelimiter = null)
        {
            string delimiter = newDelimiter ?? Delimiter;

            using var writer = new StreamWriter(fileName);
            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = delimiter,
                HasHeaderRecord = false,
                Encoding = Encoding.UTF8
            };

            using var csvWriter = new CsvWriter(writer, csvConfig);
            if (shouldSort)
            {
                csvWriter.WriteRecords(ReplacePhrasesList.OrderBy(x => x.Item1).ToList());
            }
            else
            {
                csvWriter.WriteRecords(ReplacePhrasesList);
            }

            FileName = fileName;
            if (newDelimiter != null)
            {
                Delimiter = delimiter;
            }
        }

        public static void CreateNewReplaceFile()
        {
            IsNewFile = true;
            ReplacePhrasesDict = [];
            ReplacePhrasesList = [];
            FileName = "Untitled";
            Delimiter = "";
        }

        /// <summary>
        /// Checks to see if a match found by the AhoCorasickStringSearcher
        /// Search() method is a whole word.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <returns>False if the character before and after the match exists and isnt a delimiter.</returns>
        private static bool IsMatchWholeWord(string line, string text, int pos)
        {
            /*
             * yes, i know this is ugly. this can be boiled down to the following.
             * match is not a whole word if:
             *   there is a char before the match AND it is not in the list of word delimiters
             *   OR
             *   there is a char after the match AND it is not found in the list of word delimiters
             */
            int indexBefore = pos - 1;
            int indexAfter = pos + text.Length;
            if ( ( indexBefore >= 0
                   &&
                   WORD_DELIMITERS.Contains(line[indexBefore]) == false
                 )
                 ||
                 (indexAfter < line.Length
                 &&
                   WORD_DELIMITERS.Contains(line[indexAfter]) == false
                 )
               )
            {
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Determines what the delimiter should be based off of the file type
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>Returns the delimiter used by the given file type.</returns>
        /// <exception cref="NotSupportedException"></exception>
        private static string DetermineDelemiter(string fileName, string? newDelimiter = null)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".csv" => ",",
                ".tsv" => "\t",
                ".xlsx" or ".xls" or ".txt" or ".text" => newDelimiter ?? Delimiter,
                _ => throw new NotSupportedException($"The {extension} file type is not supported.")
            };
        }

        /// <summary>
        /// Adds a replace phrase to the ReplacePhrases dictionary and list at a specified
        /// index. Sends a message that the replace phrases list was updated as well.
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <param name="index"></param>
        /// <returns>False if the key for the phrase already exists</returns>
        public static bool AddReplacePhrase(string item1, string item2, int index)
        {
            if (ReplacePhrasesDict.ContainsKey(item1))
            {
                Debug.WriteLine($"Key already exists, {item1} could not be added.");
                return false;
            }
            if (ReplacePhrasesList.FindIndex(x => x.Item1 == item1) != -1)
            {
                Debug.WriteLine($"Key does not exist in list, {item1} could not be edited.");
                return false;
            }

            ReplacePhrasesDict[item1] = item2;
            ReplacePhrasesList.Insert(index, new ReplacePhrasesWrapper(item1, item2));
            WeakReferenceMessenger.Default.Send(new SetReplacePhrasesMsg(ReplacePhrasesList));
            return true;
        }

        /// <summary>
        /// Changes the value/Item2 of a replace phrase in the ReplacePhrases dictionary and list.
        /// Sends a message that the replace phrases list was updated as well.
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns>False if the key for the phrase does not exist</returns>
        public static bool EditReplacePhrase(string item1, string item2)
        {
            if (ReplacePhrasesDict.ContainsKey(item1) == false)
            {
                Debug.WriteLine($"Key does not exist in ReplacePhrasesDict, {item1} could not be edited.");
                return false;
            }
            int index = ReplacePhrasesList.FindIndex(x => x.Item1 == item1);
            if (index == -1)
            {
                Debug.WriteLine($"Key does not exist in ReplacePhrasesList, {item1} could not be edited.");
                return false;
            }

            ReplacePhrasesDict[item1] = item2;
            ReplacePhrasesList[index] = new ReplacePhrasesWrapper(item1, item2);
            WeakReferenceMessenger.Default.Send(new SetReplacePhrasesMsg(ReplacePhrasesList));
            return true;
        }

        /// <summary>
        /// Removes a ReplacePhrases entry, and adds a new one with the specified key/value.
        /// Inserts it into the same position as the removed one for ReplacePhrasesList.
        /// </summary>
        /// <param name="oldKey"></param>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns>False if the key for the phrase does not exist</returns>
        public static bool SwapReplacePhrase(string oldItem1, string item1, string item2)
        {
            if (ReplacePhrasesDict.ContainsKey(oldItem1) == false)
            {
                Debug.WriteLine($"Key does not exist in ReplacePhrasesDict, {oldItem1} could not be swapped.");
                return false;
            }
            if (ReplacePhrasesDict.ContainsKey(item1))
            {
                Debug.WriteLine($"{item1} already exists in ReplacePhrasesDict, aborting swap.");
                return false;
            }
            int index = ReplacePhrasesList.FindIndex(x => x.Item1 == oldItem1);
            if (index == -1)
            {
                Debug.WriteLine($"Key does not exist in ReplacePhrasesList, {oldItem1} could not be swapped.");
                return false;
            }

            ReplacePhrasesDict.Remove(item1);
            ReplacePhrasesDict[item1] = item2;
            ReplacePhrasesList[index] = new ReplacePhrasesWrapper(item1, item2);
            WeakReferenceMessenger.Default.Send(new SetReplacePhrasesMsg(ReplacePhrasesList));
            return true;
        }

        /// <summary>
        /// Removes a value/Item2 from the ReplacePhrases dictionary and list
        /// by searching for its key/Item1.
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns>False if the key for the phrase does not exist</returns>
        public static bool RemoveReplacePhrase(string item1)
        {
            if (ReplacePhrasesDict.ContainsKey(item1) == false)
            {
                Debug.WriteLine($"Key was not found in ReplacePhrasesDict, {item1} could not be removed.");
                return false;
            }

            int index = ReplacePhrasesList.FindIndex(x => x.Item1 == item1);
            if (index == -1)
            {
                Debug.WriteLine($"Key does not exist in ReplacePhrasesList, {item1} could not be removed.");
                return false;
            }

            ReplacePhrasesDict.Remove(item1);
            ReplacePhrasesList.RemoveAt(index);
            WeakReferenceMessenger.Default.Send(new SetReplacePhrasesMsg(ReplacePhrasesList));
            return true;
        }
    }
}
