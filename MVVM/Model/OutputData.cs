using CommunityToolkit.Mvvm.Messaging;
using DocumentFormat.OpenXml.Packaging;
using Wordprocessing = DocumentFormat.OpenXml.Wordprocessing;
using Spreadsheet = DocumentFormat.OpenXml.Spreadsheet;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.AhoCorasick;
using TextReplace.Core.Validation;
using TextReplace.Messages.Replace;
using DocumentFormat.OpenXml;
using TextReplace.Core.Enums;
using DocumentFormat.OpenXml.Office2010.Excel;

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

        private static OutputFileStyling _outputFilesStyling = new OutputFileStyling();
        public static OutputFileStyling OutputFilesStyling
        {
            get { return _outputFilesStyling; }
            set { _outputFilesStyling = value; }
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
                int numOfReplacements = WriteReplacementsToFile(replacePhrases, srcFiles[i], destFiles[i], matcher, wholeWord, preserveCase);
                if (numOfReplacements == -1)
                {
                    Debug.WriteLine("Something went wrong in PerformReplacements()");
                    didEverythingSucceed = false;
                    continue;
                }
                OutputFiles[i].NumOfReplacements = numOfReplacements;
                Debug.WriteLine($"replacements: {numOfReplacements}");
            }

            WeakReferenceMessenger.Default.Send(new OutputFilesMsg(OutputFiles));

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
        /// <param name="wholeWord"></param>
        /// <param name="preserveCase"></param>
        /// <returns>False is some exception was thrown.</returns>
        private static int WriteReplacementsToFile(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool wholeWord, bool preserveCase)
        {
            int numOfReplacements = -1;

            try
            {
                // source file is csv, tsv, or text
                if (FileValidation.IsCsvTsvFile(src) || FileValidation.IsTextFile(src))
                {
                    // output file type:
                    if (FileValidation.IsCsvTsvFile(dest) || FileValidation.IsTextFile(dest))
                    {
                        numOfReplacements = ReadFromTextCsvTsvWriteToTextCsvTsv(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    }
                    else if (FileValidation.IsDocxFile(dest))
                    {
                        numOfReplacements = ReadFromTextCsvTsvWriteToDocx(replacePhrases, src, dest, matcher, OutputFilesStyling, wholeWord, preserveCase);
                    }
                    return numOfReplacements;
                }

                // source file is docx
                if (FileValidation.IsDocxFile(src))
                {
                    // output file type:
                    if (FileValidation.IsCsvTsvFile(dest) || FileValidation.IsTextFile(dest))
                    {
                        numOfReplacements = ReadFromDocxWriteToTextCsvTsv(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    }
                    else if (FileValidation.IsDocxFile(dest))
                    {
                        numOfReplacements = ReadFromDocxWriteToDocx(replacePhrases, src, dest, matcher, OutputFilesStyling, wholeWord, preserveCase);
                    }
                    return numOfReplacements;
                }

                // if source file is excel, only write to excel.
                // doesnt really make sense to write from excel to docx or something
                if (FileValidation.IsExcelFile(src))
                {
                    numOfReplacements = ReadFromExcelWriteToExcel(replacePhrases, src, dest, matcher, OutputFilesStyling, wholeWord, preserveCase);
                    return numOfReplacements;
                }

                throw new NotSupportedException($"Replace operation not supported for file type {Path.GetExtension(src)}");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debug.WriteLine($"Failed to write from {src} to {dest}");
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
                    isWholeWord, isPreserveCase, out int currNumOfMatches);

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

                // list of new runs that make up the paragraph
                // if there is no custom styling required, combine replacement runs with the run
                // before them to cut down on the number of resultant runs
                List<Wordprocessing.Run> newRuns = (styleReplacements) ?
                    AhoCorasickHelper.GenerateDocxRuns(paragraph, replacePhrases, matcher,
                        styling, isWholeWord, isPreserveCase, out currNumOfMatches) :
                    AhoCorasickHelper.GenerateDocxRunsOriginalStyling(paragraph, replacePhrases, matcher,
                        isWholeWord, isPreserveCase, out currNumOfMatches);
                
                numOfMatches += currNumOfMatches;

                // remove all runs in the paragraph
                paragraph.RemoveAllChildren<Wordprocessing.Run>();

                // add the new runs into the paragraph
                foreach (var run in newRuns)
                {
                    paragraph.AppendChild(run);
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
            var visitedSharedStringItems = new Dictionary<int, int>();

            File.Copy(src, dest, true);

            using var document = SpreadsheetDocument.Open(dest, true);

            if (document.WorkbookPart == null || document.WorkbookPart.Workbook.Sheets == null)
            {
                throw new NullReferenceException("ReadFromExcelWriteToExcel(): WorkbookPart or its sheets is null");
            }

            WorkbookPart wbPart = document.WorkbookPart;
            List<Spreadsheet.Sheet> sheets = wbPart.Workbook.Sheets.Elements<Spreadsheet.Sheet>().ToList();

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
                            if (visitedSharedStringItems.TryGetValue(id, out currNumOfMatches ))
                            {
                                numOfMatches += currNumOfMatches;

                                // highlight the cell if highlighting is specified and a replacement was made
                                if (styling.IsHighlighted && currNumOfMatches > 0)
                                {
                                    uint cellFormatId = AppendSpreadSheetCellFormatHighlighting(wbPart, "FF" + styling.HighlightColorString);
                                    cell.StyleIndex = cellFormatId;
                                }

                                continue;
                            }

                            var newRuns = (styleReplacements) ?
                                AhoCorasickHelper.GenerateExcelRuns(
                                    sharedStringItem, replacePhrases, matcher, styling,
                                    isWholeWord, isPreserveCase, out currNumOfMatches) :
                                AhoCorasickHelper.GenerateExcelRunsOriginalStyling(
                                    sharedStringItem, replacePhrases, matcher,
                                    isWholeWord, isPreserveCase, out currNumOfMatches);

                            var newSharedStringItem = GenerateSharedStringItemFromRuns(newRuns);

                            // replace the old shared string with the new one
                            wbPart.SharedStringTablePart?.SharedStringTable.ReplaceChild(newSharedStringItem, sharedStringItem);

                            // highlight the cell if highlighting is specified and a replacement was made
                            if (styling.IsHighlighted && currNumOfMatches > 0)
                            {
                                uint cellFormatId = AppendSpreadSheetCellFormatHighlighting(wbPart, "FF" + styling.HighlightColorString);
                                cell.StyleIndex = cellFormatId;
                            }

                            visitedSharedStringItems[id] = currNumOfMatches;
                            numOfMatches += currNumOfMatches;
                        }
                    }
                    else if (cell.DataType != null && cell.DataType == Spreadsheet.CellValues.String)
                    {
                        Debug.WriteLine("poop!");

                        // if the replacements are not styled, then the cell value can stay as a regular string
                        if (styleReplacements == false)
                        {
                            Debug.WriteLine("1");
                            cell.CellValue = new Spreadsheet.CellValue(AhoCorasickHelper.SubstituteMatches(
                                replacePhrases, cell.InnerText, matcher, isWholeWord, isPreserveCase, out currNumOfMatches));

                            numOfMatches += currNumOfMatches;
                            continue;
                        }
                        Debug.WriteLine("2");

                        int id = wbPart.SharedStringTablePart?.SharedStringTable.Elements<Spreadsheet.SharedStringItem>().Count() ?? 0;
                        cell.DataType = Spreadsheet.CellValues.SharedString;
                        cell.CellValue = new Spreadsheet.CellValue(id);

                        var sharedStringItem = new Spreadsheet.SharedStringItem(new Spreadsheet.Text(cell.InnerText));
                        var runs = AhoCorasickHelper.GenerateExcelRuns(
                            sharedStringItem, replacePhrases, matcher, styling,
                            isWholeWord, isPreserveCase, out currNumOfMatches);

                        var newSharedStringItem = GenerateSharedStringItemFromRuns(runs);

                        // insert the new shared string item into the table
                        wbPart.SharedStringTablePart?.SharedStringTable.AppendChild(newSharedStringItem);

                        // highlight the cell if highlighting is specified and a replacement was made
                        if (styling.IsHighlighted && currNumOfMatches > 0)
                        {
                            uint cellFormatId = AppendSpreadSheetCellFormatHighlighting(wbPart, "FF" + styling.HighlightColorString);
                            cell.StyleIndex = cellFormatId;
                        }
                        
                        visitedSharedStringItems[id] = currNumOfMatches;
                        numOfMatches += currNumOfMatches;
                    }
                }

                document.Save();
            }
            return numOfMatches;
        }

        public static Spreadsheet.SharedStringItem GenerateSharedStringItemFromRuns(List<Spreadsheet.Run> runs)
        {
            var sharedStringItem = new Spreadsheet.SharedStringItem();

            foreach (var run in runs)
            {
                sharedStringItem.Append(run);
            }
            
            return sharedStringItem;
        }

        /// <summary>
        /// Appends a cell format onto the spreadsheet that highlights a cell with the given color
        /// </summary>
        /// <param name="wbPart"></param>
        /// <param name="argbBackground"></param>
        /// <returns>The id of the cell format that was created.</returns>
        public static uint AppendSpreadSheetCellFormatHighlighting(WorkbookPart wbPart, string argbBackground)
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

            // add the cell format onto the stylesheet
            var cellFormats = stylesheet.GetFirstChild<Spreadsheet.CellFormats>();
            var cellFormat1 = new Spreadsheet.CellFormat()
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0
            };
            var cellFormat2 = new Spreadsheet.CellFormat()
            {
                FillId = 2,
                ApplyFill = true
            };

            uint count;
            if (cellFormats == null)
            {
                cellFormats = new Spreadsheet.CellFormats();
                count = cellFormats.Count ?? 0;
                cellFormats.Count = count + 1;
                cellFormats.Append(cellFormat1);
                cellFormats.Append(cellFormat1);
                stylesheet.AppendChild(cellFormats);
            }
            else
            {
                count = cellFormats.Count ?? 0;
                cellFormats.Count = count + 1;
                cellFormats.Append(cellFormat2);
            }

            return cellFormats.Count;
        }

        public static void UpdateOutputFiles(List<SourceFile> files)
        {
            OutputFiles = new List<OutputFile>(files.Select(x => new OutputFile(x)));
        }

        /// <summary>
        /// Sets the file type of an output file by the name of its source file
        /// (Since fileName is not unique)
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="fileType"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void SetOutputFileType(string sourceFileName, OutputFileTypeEnum fileType)
        {
            int i = OutputFiles.FindIndex(x => x.SourceFileName == sourceFileName);
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
                OutputFileTypeClass.OutputFileTypeString(fileType, OutputFiles[i].SourceFileName));
            OutputFiles[i].ShortFileName = Path.GetFileName(OutputFiles[i].FileName);

            if (SelectedFile.SourceFileName == sourceFileName)
            {
                SelectedFile.FileName = OutputFiles[i].FileName;
                SelectedFile.ShortFileName = OutputFiles[i].ShortFileName;
                WeakReferenceMessenger.Default.Send(new SelectedOutputFileMsg(SelectedFile));
            }

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
                    OutputFileTypeClass.OutputFileTypeString(fileType, outputFile.SourceFileName));
                outputFile.ShortFileName = Path.GetFileName(outputFile.FileName);

                if (SelectedFile.SourceFileName == outputFile.SourceFileName)
                {
                    SelectedFile.FileName = outputFile.FileName;
                    SelectedFile.ShortFileName = outputFile.ShortFileName;
                    WeakReferenceMessenger.Default.Send(new SelectedOutputFileMsg(SelectedFile));
                }
            }

            WeakReferenceMessenger.Default.Send(new OutputFilesMsg(OutputFiles));
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
