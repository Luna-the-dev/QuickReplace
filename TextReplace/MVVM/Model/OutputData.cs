using CommunityToolkit.Mvvm.Messaging;
using Config.Net;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.AhoCorasick;
using TextReplace.Core.Config;
using TextReplace.Core.Enums;
using TextReplace.Core.Validation;
using TextReplace.Messages.Output;
using Spreadsheet = DocumentFormat.OpenXml.Spreadsheet;
using Wordprocessing = DocumentFormat.OpenXml.Wordprocessing;

namespace TextReplace.MVVM.Model
{
    static class OutputData
    {
        private static IUserSettings _userSettings;
        public static IUserSettings UserSettings
        {
            get { return _userSettings; }
            set { _userSettings = value; }
        }

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

        private static OutputFileStyling _outputFilesStyling;
        public static OutputFileStyling OutputFilesStyling
        {
            get { return _outputFilesStyling; }
            set
            {
                _outputFilesStyling = value;

                var isStyled = value.Bold ||
                    value.Italics ||
                    value.Underline ||
                    value.Strikethrough ||
                    value.IsHighlighted ||
                    value.IsTextColored;
                WeakReferenceMessenger.Default.Send(new IsStyledMsg(isStyled));
            }
        }

        private static bool _wholeWord;
        public static bool WholeWord
        {
            get { return _wholeWord; }
            set
            {
                _wholeWord = value;
                WeakReferenceMessenger.Default.Send(new WholeWordMsg(value));
            }
        }

        private static bool _caseSensitive;
        public static bool CaseSensitive
        {
            get { return _caseSensitive; }
            set
            {
                _caseSensitive = value;
                WeakReferenceMessenger.Default.Send(new CaseSensitiveMsg(value));
            }
        }

        private static bool _preserveCase;
        public static bool PreserveCase
        {
            get { return _preserveCase; }
            set
            {
                _preserveCase = value;
                WeakReferenceMessenger.Default.Send(new PreserveCaseMsg(value));
            }
        }

        private static bool _openFileLocation;
        public static bool OpenFileLocation
        {
            get { return _openFileLocation; }
            set { _openFileLocation = value; }
        }

        // flag used in PerformReplacements() to determine whether to skip the file or retry
        // the replacements after an exception was thrown
        private static bool _retryReplacementsOnFile = false;
        public static bool RetryReplacementsOnFile
        {
            get { return _retryReplacementsOnFile; }
            set { _retryReplacementsOnFile = value; }
        }

        private static bool _isReplacementInProgress;
        public static bool IsReplacementInProgress
        {
            get { return _isReplacementInProgress; }
            set
            {
                _isReplacementInProgress = value;
                WeakReferenceMessenger.Default.Send(new ReplacementInProgressMsg(value));
            }
        }


        static OutputData()
        {
            _userSettings = new ConfigurationBuilder<IUserSettings>()
                .UseIniFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/QuickReplace/UserSettings.ini")
                .Build();

            _wholeWord = UserSettings.WholeWord;
            _caseSensitive = UserSettings.CaseSensitive;
            _preserveCase = UserSettings.PreserveCase;
            _openFileLocation = UserSettings.OpenFileLocation;
            _outputFilesStyling = new OutputFileStyling(
                UserSettings.Styling.Bold,
                UserSettings.Styling.Italics,
                UserSettings.Styling.Underline,
                UserSettings.Styling.Strikethrough,
                UserSettings.Styling.IsHighlighted,
                UserSettings.Styling.IsTextColored,
                UserSettings.Styling.HighlightColor,
                UserSettings.Styling.TextColor);
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
        /// <param name="wholeWord"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public static bool PerformReplacements(
            Dictionary<string, string> replacePhrases,
            List<string> srcFiles,
            List<string> destFiles,
            bool wholeWord,
            bool caseSensitive,
            bool preserveCase)
        {
            IsReplacementInProgress = true;

            if (srcFiles.Count == 0 || destFiles.Count == 0)
            {
                Debug.WriteLine("srcFiles or destFiles is empty");
                IsReplacementInProgress = false;
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
                int numOfReplacements = WriteReplacementsToFile(replacePhrases, srcFiles[i], destFiles[i], matcher, wholeWord, preserveCase);
                if (numOfReplacements == -1)
                {
                    Debug.WriteLine("Something went wrong in PerformReplacements()");
                    didEverythingSucceed = false;
                    continue;
                }

                // updadte the number of replacements made on this file
                var index = OutputFiles.FindIndex(x => x.FileName == destFiles[i]);
                if (index != -1)
                {
                    OutputFiles[index].NumOfReplacements = numOfReplacements;
                }

                // if this file is the selected file, update the number of replacements done on SelectedFile too
                if (SelectedFile.FileName == destFiles[i])
                {
                    SelectedFile.NumOfReplacements = numOfReplacements;
                    WeakReferenceMessenger.Default.Send(new SelectedOutputFileMsg(SelectedFile));
                }

                Debug.WriteLine($"replacements for {Path.GetFileName(srcFiles[i])}: {numOfReplacements}");
            }

            WeakReferenceMessenger.Default.Send(new OutputFilesMsg(OutputFiles));
            IsReplacementInProgress = false;
            return didEverythingSucceed;
        }

        /// <summary>
        /// Continually attempts to perform the replacements on a source file and write it to a destination file.
        /// Will keep attempting until it is either successful or the RetryReplacementsOnFile flag is false.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="wholeWord"></param>
        /// <param name="preserveCase"></param>
        /// <returns>The number of replacements that were made. Returns -1 if the replacements could not be made.</returns>
        private static int WriteReplacementsToFile(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool wholeWord, bool preserveCase)
        {
            int numOfReplacements;

            do
            {
                // will be -1 if an error was thrown
                numOfReplacements = TryWriteReplacementsToFile(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
            }
            while (numOfReplacements == -1 && RetryReplacementsOnFile);

            return numOfReplacements;
        }

        /// <summary>
        /// Attempts to perform the replacements on a source file and write it to a destination file.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="wholeWord"></param>
        /// <param name="preserveCase"></param>
        /// <returns>The number of replacements that were made. Returns -1 if an exception was thrown.</returns>
        private static int TryWriteReplacementsToFile(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool wholeWord, bool preserveCase)
        {
            try
            {
                if (File.Exists(src) == false)
                {
                    throw new FileNotFoundException($"File \"{src}\" does not exist.");
                }

                var destFile = new FileInfo(dest);
                if (destFile.Directory == null)
                {
                    throw new DirectoryNotFoundException("TryWriteReplacementsToFile(): destination file directory could not be parsed.");
                }

                // source file is docx
                if (FileValidation.IsDocxFile(src))
                {
                    // output file type:
                    if (FileValidation.IsExcelFile(dest))
                    {
                        throw new NotSupportedException($"Replace operation not supported for file types \"{Path.GetExtension(src)}\" to \"{Path.GetExtension(src)}\"");
                    }

                    int numOfReplacements;
                    if (FileValidation.IsDocxFile(dest))
                    {
                        destFile.Directory.Create();
                        numOfReplacements = ReadFromDocxWriteToDocx(replacePhrases, src, dest, matcher, OutputFilesStyling, wholeWord, preserveCase);
                        return numOfReplacements;
                    }

                    // output file is plaintext
                    destFile.Directory.Create();
                    numOfReplacements = ReadFromDocxWriteToTextCsvTsv(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    return numOfReplacements;
                }

                // if source file is excel, only write to excel.
                // doesnt really make sense to write from excel to docx or something
                else if (FileValidation.IsExcelFile(src) && FileValidation.IsExcelFile(dest))
                {
                    destFile.Directory.Create();
                    int numOfReplacements = ReadFromExcelWriteToExcel(replacePhrases, src, dest, matcher, OutputFilesStyling, wholeWord, preserveCase);
                    return numOfReplacements;
                }

                // source file is plaintext
                else if (FileValidation.IsFileNonBinary(src))
                {
                    // output file type:
                    if (FileValidation.IsExcelFile(dest))
                    {
                        throw new NotSupportedException($"Replace operation not supported for file types \"{Path.GetExtension(src)}\" to \"{Path.GetExtension(src)}\"");
                    }

                    int numOfReplacements;
                    if (FileValidation.IsDocxFile(dest))
                    {
                        destFile.Directory.Create();
                        numOfReplacements = ReadFromTextCsvTsvWriteToDocx(replacePhrases, src, dest, matcher, OutputFilesStyling, wholeWord, preserveCase);
                        return numOfReplacements;
                    }

                    // output file is plaintext
                    destFile.Directory.Create();
                    numOfReplacements = ReadFromTextCsvTsvWriteToTextCsvTsv(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    return numOfReplacements;
                }

                throw new NotSupportedException($"Replace operation not supported for file type {Path.GetExtension(src)}");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debug.WriteLine($"Failed to write from {src} to {dest}");
                // the false is a flag for whether the exception is caused due to the file already being in use
                var message = (Path.GetFileName(dest), false);
                WeakReferenceMessenger.Default.Send(new SkipOutputFileMsg(message));
                return -1;
            }
        }

        /// <summary>
        /// Reads in text from a text/csv/tsv file, performs the replacements, and writes it out to a new text/csv/tsv file.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="isWholeWord"></param>
        /// <param name="isPreserveCase"></param>
        private static int ReadFromTextCsvTsvWriteToTextCsvTsv(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord, bool isPreserveCase)
        {
            int numOfMatches = 0;

            using var sw = new StreamWriter(dest);

            // write the substitutes to the new file
            foreach (string line in File.ReadLines(src))
            {
                sw.WriteLine(AhoCorasickHelper.SubstituteMatches(
                    replacePhrases, line, matcher, isWholeWord, isPreserveCase, out int currNumOfMatches));
                numOfMatches += currNumOfMatches;
            }

            return numOfMatches;
        }

        /// <summary>
        /// Reads in text from a docx file, performs the replacements, and writes it out to a new docx file.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="isWholeWord"></param>
        /// <param name="isPreserveCase"></param>
        private static int ReadFromTextCsvTsvWriteToDocx(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, OutputFileStyling styling, bool isWholeWord, bool isPreserveCase)
        {
            int numOfMatches = 0;
            bool styleReplacements =
                styling.Bold || styling.Italics || styling.Underline ||
                styling.Strikethrough || styling.IsHighlighted || styling.IsTextColored;

            // create the new document
            using var document = WordprocessingDocument.Create(dest, WordprocessingDocumentType.Document);
            MainDocumentPart mainDocPart = document.AddMainDocumentPart();
            mainDocPart.Document = new Wordprocessing.Document();
            var body = mainDocPart.Document.AppendChild(new Wordprocessing.Body());

            foreach (string line in File.ReadLines(src))
            {
                int currNumOfMatches = 0;

                // create a new paragraph
                Wordprocessing.Paragraph paragraph = body.AppendChild(new Wordprocessing.Paragraph());

                if (styleReplacements)
                {
                    var runs = AhoCorasickHelper.GenerateDocxRunsFromText(line, replacePhrases, matcher,
                        styling, isWholeWord, isPreserveCase, out currNumOfMatches);

                    numOfMatches += currNumOfMatches;

                    // add the new runs into the paragraph
                    foreach (var run in runs)
                    {
                        paragraph.AppendChild(run);
                    }
                }
                else
                {
                    // start the run within the paragraph
                    var run = paragraph.AppendChild(new Wordprocessing.Run());

                    // set the run properties
                    var runProperties = new Wordprocessing.RunProperties();
                    run.AppendChild(runProperties);

                    var runtext = new Wordprocessing.Text(AhoCorasickHelper.SubstituteMatches(
                        replacePhrases, line, matcher, isWholeWord, isPreserveCase, out currNumOfMatches))
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    };

                    run.AppendChild(runtext);
                }


                numOfMatches += currNumOfMatches;
            }

            mainDocPart.Document.Save();

            return numOfMatches;
        }

        /// <summary>
        /// Reads in text from a docx file, performs the replacements, and writes it out to a new text/csv/tsv file.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="isWholeWord"></param>
        /// <param name="isPreserveCase"></param>
        private static int ReadFromDocxWriteToTextCsvTsv(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord, bool isPreserveCase)
        {
            int numOfMatches = 0;

            using var sw = new StreamWriter(dest);

            using var document = WordprocessingDocument.Open(src, false);

            if (document.MainDocumentPart == null || document.MainDocumentPart.Document.Body == null)
            {
                throw new NullReferenceException("ReadFromDocxWriteToTextCsvTsv(): MainDocumentPart or its body is null");
            }

            var paragraphs = document.MainDocumentPart.Document.Body.Descendants<Wordprocessing.Paragraph>();

            foreach (var paragraph in paragraphs)
            {
                // list of new runs that make up the paragraph
                // if there is no custom styling required, combine replacement runs with the run
                // before them to cut down on the number of resultant runs
                List<Wordprocessing.Run> newRuns = AhoCorasickHelper.GenerateDocxRunsOriginalStyling(
                    paragraph, replacePhrases, matcher,
                    isWholeWord, isPreserveCase, out int currNumOfMatches, out List<int> newRunsToOldRuns);

                numOfMatches += currNumOfMatches;

                // add the new runs into the paragraph
                foreach (var run in newRuns)
                {
                    sw.Write(run.InnerText);
                }
                sw.WriteLine();
            }

            return numOfMatches;
        }

        /// <summary>
        /// Reads in text from a docx file, performs the replacements, and writes it out to a new docx file.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="isWholeWord"></param>
        /// <param name="isPreserveCase"></param>
        private static int ReadFromDocxWriteToDocx(
            Dictionary<string, string> replacePhrases,
            string src, string dest,
            AhoCorasickStringSearcher matcher,
            OutputFileStyling styling,
            bool isWholeWord, bool isPreserveCase)
        {
            int numOfMatches = 0;
            bool styleReplacements =
                styling.Bold || styling.Italics || styling.Underline ||
                styling.Strikethrough || styling.IsHighlighted || styling.IsTextColored;

            File.Copy(src, dest, true);

            using var document = WordprocessingDocument.Open(dest, true);

            if (document.MainDocumentPart == null || document.MainDocumentPart.Document.Body == null)
            {
                throw new NullReferenceException("ReadFromDocxWriteToDocx(): MainDocumentPart or its body is null");
            }

            var paragraphs = document.MainDocumentPart.Document.Body.Descendants<Wordprocessing.Paragraph>();

            foreach (var paragraph in paragraphs)
            {
                int currNumOfMatches = 0;
                var newRunsToOldRuns = new List<int>();

                // list of new runs that make up the paragraph
                // if there is no custom styling required, combine replacement runs with the run
                // before them to cut down on the number of resultant runs
                List<Wordprocessing.Run> newRuns = (styleReplacements) ?
                    AhoCorasickHelper.GenerateDocxRuns(paragraph, replacePhrases, matcher,
                        styling, isWholeWord, isPreserveCase, out currNumOfMatches, out newRunsToOldRuns) :
                    AhoCorasickHelper.GenerateDocxRunsOriginalStyling(paragraph, replacePhrases, matcher,
                        isWholeWord, isPreserveCase, out currNumOfMatches, out newRunsToOldRuns);

                // if no replacements were made in this paragraph, continue
                if (newRunsToOldRuns.Count == 0)
                {
                    continue;
                }

                numOfMatches += currNumOfMatches;

                var runs = paragraph.Descendants<Wordprocessing.Run>();
                var oldRuns = runs.ToList();
                var oldRunsCount = runs.Count();
                var oldRunsIndex = 0;
                var newRunsIndex = 0;
                var runsToSkip = 0;

                // loop through the old document runs and insert their associated new runs onto them
                foreach (var run in runs)
                {
                    // stop looping once you've gone through all old runs
                    if (oldRunsIndex >= oldRunsCount)
                    {
                        break;
                    }

                    // skip over any inserted new runs
                    if (runsToSkip > 0)
                    {
                        runsToSkip--;
                        continue;
                    }

                    var parent = run.Parent;
                    if (parent == null)
                    {
                        throw new InvalidOperationException("Invalid document structure: run has no parent element.");
                    }

                    // insert the new runs after their old run using the list of
                    // new runs to old runs that was generated by AhoCorasickHelper
                    while (newRunsIndex < newRuns.Count)
                    {
                        if (newRunsToOldRuns[newRunsIndex] == oldRunsIndex)
                        {
                            // if multiple new runs are added in a row, traverse
                            // through them to insert the current new run after
                            TraverseRunSiblings(run, runsToSkip).InsertAfterSelf(newRuns[newRunsIndex]);
                            runsToSkip++;
                            newRunsIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    oldRunsIndex++;
                }

                // remove the old runs
                foreach (var oldRun in oldRuns)
                {
                    runs.Where(run => run == oldRun).First().Remove();
                }
            }

            document.MainDocumentPart.Document.Save();

            return numOfMatches;
        }

        /// <summary>
        /// Reads in text from an excel file, performs the replacements, and writes it out to a new excel file.
        /// Note: this only performs replacements on cells of type String or SharedString. Other data types
        /// (such as floats) dont have an internal value that represents what the user sees on a spreadsheet,
        /// so trying to perform replacements on them feels inconsistent at best.
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="matcher"></param>
        /// <param name="isWholeWord"></param>
        /// <param name="isPreserveCase"></param>
        private static int ReadFromExcelWriteToExcel(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, OutputFileStyling styling, bool isWholeWord, bool isPreserveCase)
        {
            int numOfMatches = 0;
            bool styleReplacements =
                styling.Bold || styling.Italics || styling.Underline ||
                styling.Strikethrough || styling.IsHighlighted || styling.IsTextColored;

            // this keeps track of the shared string item that have been visited
            // key is the sharedstringitem id, value is the number of replacements made in it
            var sharedStringItemToNumOfReplacementsMap = new Dictionary<int, int>();

            // this maps the styleIndex of a cell's formatting to the index of a new cellFormat in the CellFormats tag,
            // which is identical to the old format but with the new highlight color. This is used to not wipe a cell's
            // formatting when applying a new highlight color, as well as to not needlessly add duplicate CellFormats
            // to the excel spreadsheet.
            var OldStyleIndexToReplacedStyleIndexMap = new Dictionary<UInt32Value, UInt32Value>();

            File.Copy(src, dest, true);

            using var document = SpreadsheetDocument.Open(dest, true);

            if (document.WorkbookPart == null || document.WorkbookPart.Workbook.Sheets == null)
            {
                throw new NullReferenceException("ReadFromExcelWriteToExcel(): WorkbookPart or its sheets is null");
            }

            WorkbookPart wbPart = document.WorkbookPart;
            List<Spreadsheet.Sheet> sheets = wbPart.Workbook.Sheets.Elements<Spreadsheet.Sheet>().ToList();

            // this is used to keep track of the id associated with the fill styling associated with the highlight color
            // OpenXml requires this to know how to highlight the cell with the given color
            UInt32Value highlightFIllId = 0;

            foreach (var sheet in sheets)
            {
                if (sheet.Id?.Value == null)
                {
                    continue;
                }

                WorksheetPart wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id.Value);
                var cells = wsPart.Worksheet.Descendants<Spreadsheet.Cell>();

                foreach (var cell in cells)
                {
                    int currNumOfMatches = 0;

                    // handle a cell of data type shared string
                    if (cell.DataType != null && cell.DataType == Spreadsheet.CellValues.SharedString)
                    {
                        if (Int32.TryParse(cell.InnerText, out int id))
                        {
                            var sharedStringItem = wbPart.SharedStringTablePart?.SharedStringTable.Elements<Spreadsheet.SharedStringItem>().ElementAt(id);
                            if (string.IsNullOrEmpty(sharedStringItem?.InnerText))
                            {
                                continue;
                            }

                            // if the shared string item id has already been visited, then replacements do not need to be made again
                            if (sharedStringItemToNumOfReplacementsMap.TryGetValue(id, out currNumOfMatches))
                            {
                                numOfMatches += currNumOfMatches;

                                // highlight the cell if highlighting is specified and a replacement was made
                                if (styling.IsHighlighted && currNumOfMatches > 0)
                                {
                                    // update the cell's formatting to point to the new formatting that includes the highlighted background
                                    cell.StyleIndex = UpdateCellFormatting(cell, OldStyleIndexToReplacedStyleIndexMap, wbPart, styling.HighlightColorString, ref highlightFIllId);
                                }

                                continue;
                            }

                            bool wereReplacementsMade;

                            var newRuns = (styleReplacements) ?
                                AhoCorasickHelper.GenerateExcelRuns(
                                    sharedStringItem, replacePhrases, matcher, styling,
                                    isWholeWord, isPreserveCase, out currNumOfMatches, out wereReplacementsMade) :
                                AhoCorasickHelper.GenerateExcelRunsOriginalStyling(
                                    sharedStringItem, replacePhrases, matcher,
                                    isWholeWord, isPreserveCase, out currNumOfMatches, out wereReplacementsMade);

                            // dont change the sharedstringitem if no replacements were made
                            if (wereReplacementsMade == false)
                            {
                                continue;
                            }

                            var newSharedStringItem = GenerateSharedStringItemFromRuns(newRuns);

                            // replace the old shared string with the new one
                            wbPart.SharedStringTablePart?.SharedStringTable.ReplaceChild(newSharedStringItem, sharedStringItem);

                            // highlight the cell if highlighting is specified and a replacement was made
                            if (styling.IsHighlighted && currNumOfMatches > 0)
                            {
                                // update the cell's formatting to point to the new formatting that includes the highlighted background
                                cell.StyleIndex = UpdateCellFormatting(cell, OldStyleIndexToReplacedStyleIndexMap, wbPart, styling.HighlightColorString, ref highlightFIllId);
                            }

                            // if the cell happens to be a table column header, update the name property of the table column header
                            foreach (var tableDefinitionPart in wsPart.TableDefinitionParts)
                            {
                                var table = tableDefinitionPart.Table;
                                if (table.TableColumns == null)
                                {
                                    continue;
                                }

                                // search through the columns in the table for any whose names match the current shared string item
                                var columns = table.TableColumns.Descendants<Spreadsheet.TableColumn>();
                                foreach (var column in columns)
                                {
                                    if (column.Name == sharedStringItem.InnerText)
                                    {
                                        column.Name = newSharedStringItem.InnerText;

                                        // we can break out of this and move on to the next table because each column name should be unique
                                        break;
                                    }
                                }
                            }

                            sharedStringItemToNumOfReplacementsMap[id] = currNumOfMatches;
                            numOfMatches += currNumOfMatches;
                        }
                    }
                    // very unlikely that this will ever be entered. Excel seems to always use shared strings
                    else if (cell.DataType != null && cell.DataType == Spreadsheet.CellValues.String)
                    {
                        // if the replacements are not styled, then the cell value can stay as a regular string
                        if (styleReplacements == false)
                        {
                            cell.CellValue = new Spreadsheet.CellValue(AhoCorasickHelper.SubstituteMatches(
                                replacePhrases, cell.InnerText, matcher, isWholeWord, isPreserveCase, out currNumOfMatches));

                            numOfMatches += currNumOfMatches;
                            continue;
                        }

                        int id = wbPart.SharedStringTablePart?.SharedStringTable.Elements<Spreadsheet.SharedStringItem>().Count() ?? 0;
                        cell.DataType = Spreadsheet.CellValues.SharedString;
                        cell.CellValue = new Spreadsheet.CellValue(id);

                        var sharedStringItem = new Spreadsheet.SharedStringItem(new Spreadsheet.Text(cell.InnerText));
                        var runs = AhoCorasickHelper.GenerateExcelRuns(
                            sharedStringItem, replacePhrases, matcher, styling,
                            isWholeWord, isPreserveCase, out currNumOfMatches, out bool wereReplacementsMade);

                        // dont change the sharedstringitem if no replacements were made
                        if (wereReplacementsMade == false)
                        {
                            continue;
                        }

                        var newSharedStringItem = GenerateSharedStringItemFromRuns(runs);

                        // insert the new shared string item into the table
                        wbPart.SharedStringTablePart?.SharedStringTable.AppendChild(newSharedStringItem);

                        // highlight the cell if highlighting is specified and a replacement was made
                        if (styling.IsHighlighted && currNumOfMatches > 0)
                        {
                            // update the cell's formatting to point to the new formatting that includes the highlighted background
                            cell.StyleIndex = UpdateCellFormatting(cell, OldStyleIndexToReplacedStyleIndexMap, wbPart, styling.HighlightColorString, ref highlightFIllId);
                        }

                        // if the cell happens to be a table column header, update the name property of the table column header
                        foreach (var tableDefinitionPart in wsPart.TableDefinitionParts)
                        {
                            var table = tableDefinitionPart.Table;
                            if (table.TableColumns == null)
                            {
                                continue;
                            }

                            // search through the columns in the table for any whose names match the current shared string item
                            var columns = table.TableColumns.Descendants<Spreadsheet.TableColumn>();
                            foreach (var column in columns)
                            {
                                if (column.Name == sharedStringItem.InnerText)
                                {
                                    column.Name = newSharedStringItem.InnerText;

                                    // we can break out of this and move on to the next table because each column name should be unique
                                    break;
                                }
                            }
                        }

                        sharedStringItemToNumOfReplacementsMap[id] = currNumOfMatches;
                        numOfMatches += currNumOfMatches;
                    }
                }
            }

            document.Save();

            return numOfMatches;
        }

        static private Wordprocessing.Run TraverseRunSiblings(Wordprocessing.Run run, int distance)
        {
            Debug.WriteLine(run.InnerText + " " + distance);
            if (distance <= 0)
            {
                return run;
            }

            var nextRun = run.NextSibling<Wordprocessing.Run>();
            if (nextRun == null)
            {
                Debug.WriteLine("peepee");
                return run;
            }

            return TraverseRunSiblings(nextRun, distance - 1);
        }

        private static Spreadsheet.SharedStringItem GenerateSharedStringItemFromRuns(List<Spreadsheet.Run> runs)
        {
            var sharedStringItem = new Spreadsheet.SharedStringItem();

            foreach (var run in runs)
            {
                var cloneRun = run.CloneNode(true);
                sharedStringItem.Append(cloneRun);
            }

            return sharedStringItem;
        }

        private static UInt32Value UpdateCellFormatting(
            Spreadsheet.Cell cell,
            Dictionary<UInt32Value, UInt32Value> OldStyleIndexToReplacedStyleIndexMap,
            WorkbookPart wbPart,
            string highlightColorString,
            ref UInt32Value highlightFillId)
        {
            var styleIndex = cell.StyleIndex ?? 0;

            // if a new cell formatting was *not* created yet for this cell's formatting,
            // create one and update the map to reflect it
            if (OldStyleIndexToReplacedStyleIndexMap.ContainsKey(styleIndex) == false)
            {
                var newStyleIndex = AppendSpreadSheetCellFormatHighlighting(wbPart, highlightColorString, styleIndex, ref highlightFillId);
                OldStyleIndexToReplacedStyleIndexMap[styleIndex] = newStyleIndex;
            }

            return OldStyleIndexToReplacedStyleIndexMap[styleIndex];
        }

        /// <summary>
        /// Appends a cell format onto the spreadsheet that highlights a cell with the given color
        /// </summary>
        /// <param name="wbPart"></param>
        /// <param name="argbBackground"></param>
        /// <returns>The id of the cell format that was created.</returns>
        private static uint AppendSpreadSheetCellFormatHighlighting(WorkbookPart wbPart, string argbBackground, UInt32Value styleindex, ref UInt32Value highlightFillId)
        {
            // get the style sheet or create one if it does not exist
            var workStylePart = wbPart.WorkbookStylesPart;
            Spreadsheet.Stylesheet stylesheet;
            if (workStylePart == null)
            {
                workStylePart = wbPart.AddNewPart<WorkbookStylesPart>();
                workStylePart.Stylesheet = new Spreadsheet.Stylesheet();
                stylesheet = workStylePart.Stylesheet;
            }
            else
            {
                stylesheet = workStylePart.Stylesheet;
            }

            // if no new fill to match our background color has been created,
            // create a new fill and save the id to reapply in other cells if needed
            if (highlightFillId == 0)
            {
                // create the background for the cell
                var fill = new Spreadsheet.Fill();

                var patternFill = new Spreadsheet.PatternFill() { PatternType = Spreadsheet.PatternValues.Solid };
                var foregroundColor = new Spreadsheet.ForegroundColor() { Rgb = argbBackground };
                var backgroundColor = new Spreadsheet.BackgroundColor() { Indexed = (UInt32Value)64U };

                patternFill.Append(foregroundColor);
                patternFill.Append(backgroundColor);
                fill.Append(patternFill);

                // if the cell didnt have a fills child, create one and append it to the stylesheet
                var fills = stylesheet.GetFirstChild<Spreadsheet.Fills>();
                if (fills == null)
                {
                    fills = new Spreadsheet.Fills();
                    fills.Append(fill);
                    stylesheet.AppendChild(fills);
                }
                else
                {
                    fills.Append(fill);
                }

                highlightFillId = fills.Count ?? 0;
            }

            // create new cell formats for if they dont already exist in the stylesheet
            var defaultCellFormat = new Spreadsheet.CellFormat()
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0
            };
            var defaultCellFormatHighlighted = new Spreadsheet.CellFormat()
            {
                FillId = highlightFillId,
                ApplyFill = true
            };

            var cellFormats = stylesheet.GetFirstChild<Spreadsheet.CellFormats>();

            // if the stylesheet does not have a cellformats tag, add it in and
            // append a default cell format as well as the new highlighted cell format
            if (cellFormats == null)
            {
                cellFormats = new Spreadsheet.CellFormats();
                cellFormats.Count = (cellFormats.Count ?? 0) + 2;
                cellFormats.Append(defaultCellFormat);
                cellFormats.Append(defaultCellFormatHighlighted);
                stylesheet.AppendChild(cellFormats);

                return cellFormats.Count - 1;
            }

            // get the cell formatting used by the given cell
            uint styleIndexUint = styleindex;
            var cellFormat = cellFormats.ToList()[(int)styleIndexUint] as Spreadsheet.CellFormat;

            // if the cell formatting could not be found, use default cell formatting with the highlighted background
            if (cellFormat == null)
            {
                cellFormats.Count = (cellFormats.Count ?? 0) + 1;
                cellFormats.Append(defaultCellFormatHighlighted);
                return cellFormats.Count - 1;
            }

            // create the new cell formatting
            var newCellFormat = cellFormat.CloneNode(true) as Spreadsheet.CellFormat;

            if (newCellFormat == null)
            {
                cellFormats.Count = (cellFormats.Count ?? 0) + 1;
                cellFormats.Append(defaultCellFormatHighlighted);
                return cellFormats.Count;
            }

            // apply the background color to the cells formatting
            newCellFormat.FillId = highlightFillId;
            newCellFormat.ApplyFill = true;
            cellFormats.Count = (cellFormats.Count ?? 0) + 1;
            cellFormats.Append(newCellFormat);

            return cellFormats.Count - 1;
        }

        public static void UpdateOutputFiles(List<SourceFile> files)
        {
            // if the file from the list of source files existed in the list of output files,
            // use the file extension of the one from the output file rather than the source file.
            // this prevents the file conversion settings from being reverted when the output files are updated.
            foreach (var outFile in OutputFiles)
            {
                files.Where(x => x.Id == outFile.Id)
                    .ToList()
                    .ForEach(x => x.FileName = Path.ChangeExtension(x.FileName, Path.GetExtension(outFile.FileName)));
            }

            OutputFiles = new List<OutputFile>(files.Select(x => new OutputFile(x)));
        }

        /// <summary>
        /// Moves an output file in the OutputFiles list from its current position to a new index.
        /// Updates the Source Files to be reflect this as well
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        public static void MoveOutputFile(int oldIndex, int newIndex)
        {
            // perform the move on the source files first. this method will automatically
            // update the output files list. this is done because the source files list
            // is what generates the output files list
            SourceFilesData.MoveSourceFile(oldIndex, newIndex);
        }

        /// <summary>
        /// Sets the file type of an output file by the name of its source file id
        /// (Since fileName is not unique)
        /// </summary>
        /// <param name="sourceFileId"></param>
        /// <param name="fileType"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void SetOutputFileType(string sourceFileId, OutputFileTypeEnum fileType)
        {
            int i = OutputFiles.FindIndex(x => x.Id == sourceFileId);
            if (i == -1)
            {
                throw new ArgumentException("SetOutputFileType() could not find name");
            }

            // dont change file type of .xlsx file
            if (FileValidation.IsExcelFile(OutputFiles[i].FileName))
            {
                return;
            }

            OutputFiles[i].FileName = Path.ChangeExtension(
                OutputFiles[i].FileName,
                OutputFileTypeClass.OutputFileTypeString(fileType, OutputFiles[i].FileName));
            OutputFiles[i].ShortFileName = Path.GetFileName(OutputFiles[i].FileName);

            if (SelectedFile.Id == sourceFileId)
            {
                SelectedFile.FileName = OutputFiles[i].FileName;
                SelectedFile.ShortFileName = OutputFiles[i].ShortFileName;
            }

            // update the selected file first to prevent issue with viewmodel
            // updating its data with an outdated selected file
            WeakReferenceMessenger.Default.Send(new SelectedOutputFileMsg(SelectedFile));
            WeakReferenceMessenger.Default.Send(new OutputFilesMsg(OutputFiles));
        }

        /// <summary>
        /// Sets the file type of all output files
        /// </summary>
        /// <param name="fileType"></param>
        public static void SetAllOutputFileTypes(OutputFileTypeEnum fileType)
        {
            foreach (var outputFile in OutputFiles)
            {
                // dont change file type of .xlsx file
                if (FileValidation.IsExcelFile(outputFile.FileName))
                {
                    continue;
                }

                outputFile.FileName = Path.ChangeExtension(
                    outputFile.FileName,
                    OutputFileTypeClass.OutputFileTypeString(fileType, outputFile.FileName));
                outputFile.ShortFileName = Path.GetFileName(outputFile.FileName);

                if (SelectedFile.Id == outputFile.Id)
                {
                    SelectedFile.FileName = outputFile.FileName;
                    SelectedFile.ShortFileName = outputFile.ShortFileName;
                }
            }

            // update the selected file first to prevent issue with viewmodel
            // updating its data with an outdated selected file
            WeakReferenceMessenger.Default.Send(new SelectedOutputFileMsg(SelectedFile));
            WeakReferenceMessenger.Default.Send(new OutputFilesMsg(OutputFiles));
        }
    }

    public class OutputFile
    {
        public string FileName { get; set; }
        public string ShortFileName { get; set; }
        public string SourceFileName { get; set; }
        public int NumOfReplacements { get; set; }
        public string Id { get; set; }

        public OutputFile()
        {
            FileName = string.Empty;
            ShortFileName = string.Empty;
            SourceFileName = string.Empty;
            NumOfReplacements = -1;
            Id = string.Empty;
        }

        public OutputFile(SourceFile file)
        {
            FileName = GenerateDestFileName(file);
            ShortFileName = Path.GetFileName(FileName);
            SourceFileName = file.FileName;
            NumOfReplacements = -1;
            Id = file.Id;
        }

        public OutputFile(
            string fileName,
            string shortFileName,
            string sourceFileName,
            int numOfReplacements,
            string id)
        {
            FileName = fileName;
            ShortFileName = shortFileName;
            SourceFileName = sourceFileName;
            NumOfReplacements = numOfReplacements;
            Id = id;
        }

        private static string GenerateDestFileName(SourceFile file)
        {
            string? directory = (file.OutputDirectory == string.Empty) ?
                Path.GetDirectoryName(file.FileName)?.Replace("\\", "/") :
                file.OutputDirectory;

            directory ??= "";

            string suffix = (file.Suffix == string.Empty) ?
                "-QuickReplace" :
                file.Suffix;

            // if the directory ends in a slash, dont include another one when generating the destination file
            var appendSlash = directory.EndsWith("/") ? "" : "/";

            return string.Format(@"{0}{1}{2}{3}{4}",
                                 directory,
                                 appendSlash,
                                 Path.GetFileNameWithoutExtension(file.FileName),
                                 suffix,
                                 Path.GetExtension(file.FileName));
        }
    }

    public class OutputFileStyling
    {
        public bool Bold { get; set; }
        public bool Italics { get; set; }
        public bool Underline { get; set; }
        public bool Strikethrough { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsTextColored { get; set; }
        private System.Windows.Media.Color _highlightColor;
        public System.Windows.Media.Color HighlightColor
        {
            get { return _highlightColor; }
            set
            {
                _highlightColor = value;
                HighlightColorString = (value.ToString()[0] == '#') ?
                    value.ToString().Substring(1) :
                    value.ToString();
            }
        }
        private System.Windows.Media.Color _textColor;
        public System.Windows.Media.Color TextColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                TextColorString = (value.ToString()[0] == '#') ?
                    value.ToString().Substring(1) :
                    value.ToString();
            }
        }
        public string HighlightColorString { get; set; }
        public string TextColorString { get; set; }

        public OutputFileStyling()
        {
            Bold = false;
            Italics = false;
            Underline = false;
            Strikethrough = false;
            IsHighlighted = false;
            IsTextColored = false;
            HighlightColor = new System.Windows.Media.Color();
            TextColor = new System.Windows.Media.Color();
            HighlightColorString = string.Empty;
            TextColorString = string.Empty;
        }

        public OutputFileStyling(
            bool bold,
            bool italics,
            bool underline,
            bool strikethrough,
            bool isHighlighted,
            bool isTextColored,
            System.Windows.Media.Color highlightColor,
            System.Windows.Media.Color textColor)
        {
            Bold = bold;
            Italics = italics;
            Underline = underline;
            Strikethrough = strikethrough;
            IsHighlighted = isHighlighted;
            IsTextColored = isTextColored;
            HighlightColor = highlightColor;
            TextColor = textColor;
            HighlightColorString = (highlightColor.ToString()[0] == '#') ?
                    highlightColor.ToString().Substring(3) :
                    highlightColor.ToString().Substring(1);
            TextColorString = (textColor.ToString()[0] == '#') ?
                    textColor.ToString().Substring(3) :
                    textColor.ToString().Substring(1);
        }

        public OutputFileStyling(
            bool bold,
            bool italics,
            bool underline,
            bool strikethrough,
            bool isHighlighted,
            bool isTextColored,
            string highlightColor,
            string textColor)
        {
            Bold = bold;
            Italics = italics;
            Underline = underline;
            Strikethrough = strikethrough;
            IsHighlighted = isHighlighted;
            IsTextColored = isTextColored;

            HighlightColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(highlightColor);
            TextColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(textColor);

            HighlightColorString = (highlightColor.ToString()[0] == '#') ?
                    highlightColor.ToString().Substring(3) :
                    highlightColor.ToString().Substring(1);
            TextColorString = (textColor.ToString()[0] == '#') ?
                    textColor.ToString().Substring(3) :
                    textColor.ToString().Substring(1);
        }

        public static Wordprocessing.RunProperties StyleRunProperties(
            Wordprocessing.RunProperties runProps, OutputFileStyling style)
        {
            if (style.Bold)
            {
                runProps.Bold = new Wordprocessing.Bold();
            }

            if (style.Italics)
            {
                runProps.Italic = new Wordprocessing.Italic();
            }

            if (style.Underline)
            {
                runProps.Underline = new Wordprocessing.Underline()
                {
                    Val = Wordprocessing.UnderlineValues.Single
                };
            }

            if (style.Strikethrough)
            {
                runProps.Strike = new Wordprocessing.Strike();
            }

            if (style.IsHighlighted)
            {
                runProps.Shading = new Wordprocessing.Shading()
                {
                    Fill = style.HighlightColorString,
                    Val = Wordprocessing.ShadingPatternValues.Clear,
                    Color = "auto"
                };
            }

            if (style.IsTextColored)
            {
                runProps.Color = new Wordprocessing.Color()
                {
                    Val = style.TextColorString,
                    ThemeColor = Wordprocessing.ThemeColorValues.Accent1,
                    ThemeShade = "BF"
                };
            }

            return runProps;
        }

        public static Spreadsheet.RunProperties StyleRunProperties(
            Spreadsheet.RunProperties runProps, OutputFileStyling style)
        {
            if (style.Bold)
            {
                runProps.AppendChild(new Spreadsheet.Bold());
            }

            if (style.Italics)
            {
                runProps.AppendChild(new Spreadsheet.Italic());
            }

            if (style.Underline)
            {
                runProps.AppendChild(new Spreadsheet.Underline()
                {
                    Val = Spreadsheet.UnderlineValues.Single
                });
            }

            if (style.Strikethrough)
            {
                runProps.AppendChild(new Spreadsheet.Strike());
            }

            // Excel spreadsheet highlighting has to be done on a cell rather than on a run,
            // so the method that constructs or edits the cells should set the highlighting value

            if (style.IsTextColored)
            {
                runProps.AppendChild(new Spreadsheet.Color()
                {
                    Rgb = "FF" + style.TextColorString
                });
            }

            return runProps;
        }
    }
}
