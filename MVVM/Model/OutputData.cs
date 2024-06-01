using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.AhoCorasick;
using TextReplace.Messages.Replace;

namespace TextReplace.MVVM.Model
{
    class OutputData
    {
        private static List<OutputFile> _outputFiles = [];
        public static List<OutputFile> OutputFiles
        {
            get { return _outputFiles; }
            set
            {
                _outputFiles = value;
                WeakReferenceMessenger.Default.Send(new OutputFilesMsg(value));
            }
        }

        private static OutputFile _selectedFile = new OutputFile();
        public static OutputFile SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
                WeakReferenceMessenger.Default.Send(new SelectedOutputFileMsg(value));
            }
        }

        private static bool _wholeWord = false;
        public static bool WholeWord
        {
            get { return _wholeWord; }
            set
            {
                _wholeWord = value;
                WeakReferenceMessenger.Default.Send(new WholeWordMsg(value));
            }
        }

        private static bool _caseSensitive = false;
        public static bool CaseSensitive
        {
            get { return _caseSensitive; }
            set
            {
                _caseSensitive = value;
                WeakReferenceMessenger.Default.Send(new CaseSensitiveMsg(value));
            }
        }

        // delimiters that decide what seperates whole words
        private const string WORD_DELIMITERS = " \t/\\()\"'-:,.;<>~!@#$%^&*|+=[]{}?│";

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
        /// <param name="wholeWord"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public static bool PerformReplacements(
            Dictionary<string, string> replacePhrases,
            List<string> srcFiles,
            List<string> destFiles,
            bool wholeWord,
            bool caseSensitive)
        {
            if (srcFiles.Count == 0 || destFiles.Count == 0)
            {
                Debug.WriteLine("srcFiles or destFiles is empty");
                return false;
            }

            // construct the automaton and fill it with the phrases to search for
            // also create a list of the replacement phrases to go alongside the 
            AhoCorasickStringSearcher matcher = new AhoCorasickStringSearcher(caseSensitive);
            foreach (var searchWord in replacePhrases)
            {
                matcher.AddItem(searchWord.Key);
            }
            matcher.CreateFailureFunction();

            // do the search on each file
            bool didEverythingSucceed = true;
            for (int i = 0; i < srcFiles.Count; i++)
            {
                bool res = WriteReplacementsToFile(replacePhrases, srcFiles[i], destFiles[i], matcher, wholeWord);
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
        /// <param name="replacePhrases"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="isWholeWord"></param>
        /// <returns>False is some exception was thrown.</returns>
        private static bool WriteReplacementsToFile(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord)
        {
            try
            {
                using var sw = new StreamWriter(dest);

                // making these two seperate functions in order to cut down on if/else checks
                // inside the foreach loops (which could potentially loop millions of times each)
                if (isWholeWord)
                {
                    MatchAndWriteWholeWord(replacePhrases, src, sw, matcher);
                }
                else
                {
                    MatchAndWrite(replacePhrases, src, sw, matcher);
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
        private static void MatchAndWrite(Dictionary<string, string> replacePhrases,
            string src, StreamWriter sw, AhoCorasickStringSearcher matcher)
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
                                             .Insert(m.Position + offset, replacePhrases[m.Text]);
                    offset += replacePhrases[m.Text].Length - m.Text.Length;
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
        private static void MatchAndWriteWholeWord(Dictionary<string, string> replacePhrases,
            string src, StreamWriter sw, AhoCorasickStringSearcher matcher)
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
                                             .Insert(m.Position + offset, replacePhrases[m.Text]);
                    offset += replacePhrases[m.Text].Length - m.Text.Length;
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
            if ((indexBefore >= 0
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

        public static void UpdateOutputFiles(List<SourceFile> files)
        {
            OutputFiles = new List<OutputFile>(files.Select(x => new OutputFile(x)));
        }
    }

    class OutputFile
    {
        public string FileName { get; set; }
        public string ShortFileName { get; set; }
        public string SourceFileName { get; set; }
        public int NumOfReplacements { get; set; }

        public OutputFile()
        {
            FileName = string.Empty;
            ShortFileName = string.Empty;
            SourceFileName = string.Empty;
            NumOfReplacements = -1;
        }

        public OutputFile(SourceFile file)
        {
            FileName = GenerateDestFileName(file);
            ShortFileName = Path.GetFileName(FileName);
            SourceFileName = file.FileName;
            NumOfReplacements = -1;
        }

        public OutputFile(
            string fileName,
            string shortFileName,
            string sourceFileName,
            int numOfReplacements)
        {
            FileName = fileName;
            ShortFileName = shortFileName;
            SourceFileName = sourceFileName;
            NumOfReplacements = numOfReplacements;
        }

        private static string GenerateDestFileName(SourceFile file)
        {
            string? directory = (file.OutputDirectory == string.Empty) ?
                Path.GetDirectoryName(file.FileName) :
                file.OutputDirectory;

            string suffix = (file.Suffix == string.Empty) ?
                "-replacify" :
                file.Suffix;

            return string.Format(@"{0}\{1}{2}{3}",
                                 directory,
                                 Path.GetFileNameWithoutExtension(file.FileName),
                                 suffix,
                                 Path.GetExtension(file.FileName));
        }
    }
}
