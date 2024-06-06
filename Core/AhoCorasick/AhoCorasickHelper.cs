
namespace TextReplace.Core.AhoCorasick
{
    public class AhoCorasickHelper
    {
        // delimiters that decide what seperates whole words
        private const string WORD_DELIMITERS = " \t/\\()\"'-:,.;<>~!@#$%^&*|+=[]{}?│";

        // delegate for return type of SelectSubstituteMatchesMethod
        public delegate string Foo<T1, T2, T3, T4>(T1 replacePhrases, T2 line, T3 matcher, out T4 numOfMatches);

        public static Foo<Dictionary<string, string>, string, AhoCorasickStringSearcher, int>
            SelectSubstituteMatchesMethod(bool wholeWord, bool preserveCase)
        {
            switch (wholeWord, preserveCase)
            {
                case (false, false):
                    return SubstituteMatches;
                case (true, false):
                    return SubstituteMatchesWholeWord;
                case (false, true):
                    return SubstituteMatchesPreserveCase;
                case (true, true):
                    return SubstituteMatchesWholeWordPreserveCase;
            }
        }

        /// <summary>
        /// Uses the Aho-Corasick algorithm to search a file for any substring matches
        /// in a source file.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="line"></param>
        /// <param name="matcher"></param>
        /// <param name="numOfMatches"></param>
        private static string SubstituteMatches(Dictionary<string, string> replacePhrases,
            string line, AhoCorasickStringSearcher matcher, out int numOfMatches)
        {
            // search the current line for any text that should be replaced
            var matches = matcher.Search(line);
            numOfMatches = 0;

            // save an offset to remember how much the position of each replacement
            // should be shifted if a replacement was already made on the same line
            int offset = 0;
            string updatedLine = line;
            foreach (var m in matches)
            {
                numOfMatches += 1;
                updatedLine = updatedLine.Remove(m.Position + offset, m.Text.Length)
                                         .Insert(m.Position + offset, replacePhrases[m.Text]);
                offset += replacePhrases[m.Text].Length - m.Text.Length;
            }
            return updatedLine;
        }

        /// <summary>
        /// Uses the Aho-Corasick algorithm to search a file for any whole-word matches in a source file.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="line"></param>
        /// <param name="matcher"></param>
        /// <param name="numOfMatches"></param>
        private static string SubstituteMatchesWholeWord(Dictionary<string, string> replacePhrases,
            string line, AhoCorasickStringSearcher matcher, out int numOfMatches)
        {
            // search the current line for any text that should be replaced
            var matches = matcher.Search(line);
            numOfMatches = 0;

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
                numOfMatches += 1;
                updatedLine = updatedLine.Remove(m.Position + offset, m.Text.Length)
                                         .Insert(m.Position + offset, replacePhrases[m.Text]);
                offset += replacePhrases[m.Text].Length - m.Text.Length;
            }
            return updatedLine;
        }

        /// <summary>
        /// Uses the Aho-Corasick algorithm to search a file for any substring matches
        /// in a source file and preserves the case of the first letter of the original substring.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="line"></param>
        /// <param name="matcher"></param>
        /// <param name="numOfMatches"></param>
        private static string SubstituteMatchesPreserveCase(Dictionary<string, string> replacePhrases,
            string line, AhoCorasickStringSearcher matcher, out int numOfMatches)
        {
            // search the current line for any text that should be replaced
            var matches = matcher.Search(line);
            numOfMatches = 0;

            // save an offset to remember how much the position of each replacement
            // should be shifted if a replacement was already made on the same line
            int offset = 0;
            string updatedLine = line;
            foreach (var m in matches)
            {
                numOfMatches += 1;
                updatedLine = updatedLine.Remove(m.Position + offset, m.Text.Length)
                                         .Insert(m.Position + offset,
                                                 SetMatchCase(replacePhrases[m.Text], char.IsUpper(updatedLine[m.Position + offset]))
                                                 );
                offset += replacePhrases[m.Text].Length - m.Text.Length;
            }
            return updatedLine;
        }

        /// <summary>
        /// Uses the Aho-Corasick algorithm to search a file for any whole-word substring matches
        /// in a source file and preserves the case of the first letter of the original substring.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="line"></param>
        /// <param name="matcher"></param>
        /// <param name="numOfMatches"></param>
        private static string SubstituteMatchesWholeWordPreserveCase(Dictionary<string, string> replacePhrases,
            string line, AhoCorasickStringSearcher matcher, out int numOfMatches)
        {
            // search the current line for any text that should be replaced
            var matches = matcher.Search(line);
            numOfMatches = 0;

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
                numOfMatches += 1;
                updatedLine = updatedLine.Remove(m.Position + offset, m.Text.Length)
                                         .Insert(m.Position + offset,
                                                 SetMatchCase(replacePhrases[m.Text], char.IsUpper(updatedLine[m.Position + offset]))
                                                 );
                offset += replacePhrases[m.Text].Length - m.Text.Length;
            }
            return updatedLine;
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
            // yes, i know this is ugly. this can be boiled down to the following:
            //
            // match is *not* a whole word if:
            //     there is a char before the match AND it is not in the list of word delimiters
            //     OR
            //     there is a char after the match AND it is not found in the list of word delimiters
            int indexBefore = pos - 1;
            int indexAfter = pos + text.Length;
            if ((indexBefore >= 0 && WORD_DELIMITERS.Contains(line[indexBefore]) == false) ||
                (indexAfter < line.Length && WORD_DELIMITERS.Contains(line[indexAfter]) == false))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sets the case of the first letter of a string based off of a bool
        /// </summary>
        /// <param name="match"></param>
        /// <param name="toUppercase"></param>
        /// <returns>The original string with the new case</returns>
        private static string SetMatchCase(string match, bool toUppercase)
        {
            if (match == string.Empty)
            {
                return string.Empty;
            }

            return (toUppercase) ? char.ToUpper(match[0]) + match.Substring(1) : char.ToLower(match[0]) + match.Substring(1);
        }
    }
}
