using System.Diagnostics;
using System.IO;
using TextReplace.Core.Validation;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages.Replace;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Text;
using CsvHelper.Configuration.Attributes;
using ExcelDataReader;
using System.Data;

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
        private static List<ReplacePhrase> _replacePhrasesList = [];
        public static List<ReplacePhrase> ReplacePhrasesList
        {
            get { return _replacePhrasesList; }
            set
            {
                _replacePhrasesList = value;
                WeakReferenceMessenger.Default.Send(new ReplacePhrasesMsg(value));
            }
        }
        // the delimiter used for parsing the replace file
        private static string _delimiter = string.Empty;
        public static string Delimiter
        {
            get { return _delimiter; }
            set { _delimiter = value; }
        }
        // a flag to denote whether a phrase is selected in the replace view
        private static ReplacePhrase _selectedPhrase = new ReplacePhrase();
        public static ReplacePhrase SelectedPhrase
        {
            get { return _selectedPhrase; }
            set
            {
                _selectedPhrase = value;
                WeakReferenceMessenger.Default.Send(new SelectedReplacePhraseMsg(value));
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

        /// <summary>
        /// Parses the replacements from a file. Supports excel files as well as value-seperated files
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns>
        /// A dictionary of pairs of the values from the file. If one of the lines in the file has an
        /// incorrect number of values or if the operation fails for another reason, return an empty list.
        /// </returns>
        public static Dictionary<string, string> ParseReplacements(string fileName)
        {
            var phrases = new Dictionary<string, string>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read);

            using var reader = (Path.GetExtension(fileName).Equals(".xlsx", StringComparison.CurrentCultureIgnoreCase) ||
                Path.GetExtension(fileName).Equals(".xls", StringComparison.CurrentCultureIgnoreCase)) ?
                ExcelReaderFactory.CreateReader(stream) :
                ExcelReaderFactory.CreateCsvReader(stream);

            while (reader.Read())
            {
                if (reader.GetString(0) == string.Empty)
                {
                    Debug.WriteLine("A field within the first column of the replace file is empty.");
                    continue;
                }

                phrases[reader.GetString(0)] = reader.GetString(1);
            }

            if (phrases.Count == 0)
            {
                throw new InvalidOperationException("The dictionary returned by ParseReplacements() is empty.");
            }

            return phrases;
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
                    ParseReplacements(fileName, delimiter);
                    return true;
                }

                // parse through phrases and attempt to save them
                ReplacePhrasesDict = ParseReplacements(fileName, delimiter);

                // put copy the dict into a list which gets used by the view models
                ReplacePhrasesList = ReplacePhrasesDict.Select(x => new ReplacePhrase(x.Key, x.Value)).ToList();

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
            catch (NotSupportedException e)
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
            ReplacePhrasesList.Insert(index, new ReplacePhrase(item1, item2));
            WeakReferenceMessenger.Default.Send(new ReplacePhrasesMsg(ReplacePhrasesList));
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
            ReplacePhrasesList[index] = new ReplacePhrase(item1, item2);
            WeakReferenceMessenger.Default.Send(new ReplacePhrasesMsg(ReplacePhrasesList));
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
            ReplacePhrasesList[index] = new ReplacePhrase(item1, item2);
            WeakReferenceMessenger.Default.Send(new ReplacePhrasesMsg(ReplacePhrasesList));
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
            WeakReferenceMessenger.Default.Send(new ReplacePhrasesMsg(ReplacePhrasesList));
            return true;
        }
    }

    /// <summary>
    /// Wrapper class for the replace phrases dictionary.
    /// This wrapper exists only to read in data with CsvHelper.
    /// Note: Keep the variables in the constructor the exact same as the fields.
    /// This is to make CsvHelper work without a default constructor.
    /// </summary>
    public class ReplacePhrase
    {
        [Index(0)]
        public string Item1 { get; set; }

        [Index(1)]
        public string Item2 { get; set; }

        public ReplacePhrase()
        {
            Item1 = string.Empty;
            Item2 = string.Empty;
        }

        public ReplacePhrase(string item1, string item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
