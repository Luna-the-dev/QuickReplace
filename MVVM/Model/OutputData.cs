using CommunityToolkit.Mvvm.Messaging;
using DocumentFormat.OpenXml.Packaging;
using Wordprocessing = DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.AhoCorasick;
using TextReplace.Core.Validation;
using TextReplace.Messages.Replace;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using TextReplace.Core.Enums;
using DocumentFormat.OpenXml.Bibliography;

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
                bool res = WriteReplacementsToFile(replacePhrases, srcFiles[i], destFiles[i], matcher, wholeWord, preserveCase);
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
        /// <param name="wholeWord"></param>
        /// <param name="preserveCase"></param>
        /// <returns>False is some exception was thrown.</returns>
        private static bool WriteReplacementsToFile(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool wholeWord, bool preserveCase)
        {
            try
            {
                // source file is csv, tsv, or text
                if (FileValidation.IsCsvTsvFile(src) || FileValidation.IsTextFile(src))
                {
                    // output file type:
                    if (FileValidation.IsCsvTsvFile(dest) || FileValidation.IsTextFile(dest))
                    {
                        ReadFromTextCsvTsvWriteToTextCsvTsv(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    }
                    else if (FileValidation.IsDocxFile(dest))
                    {
                        ReadFromTextCsvTsvWriteToDocx(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    }
                    return true;
                }

                // source file is docx
                if (FileValidation.IsDocxFile(src))
                {
                    // output file type:
                    if (FileValidation.IsCsvTsvFile(dest) || FileValidation.IsTextFile(dest))
                    {
                        ReadFromDocxWriteToTextCsvTsv(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    }
                    else if (FileValidation.IsDocxFile(dest))
                    {
                        ReadFromDocxWriteToDocx(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    }
                    return true;
                }

                // if source file is excel, only write to excel.
                // doesnt really make sense to write from excel to docx or something
                if (FileValidation.IsExcelFile(src))
                {
                    ReadFromExcelWriteToExcel(replacePhrases, src, dest, matcher, wholeWord, preserveCase);
                    return true;
                }

                throw new NotSupportedException($"Replace operation not supported for file type {Path.GetExtension(src)}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to write from {src} to {dest} due to {e.Message}");
                return false;
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
        private static async void ReadFromTextCsvTsvWriteToTextCsvTsv(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord, bool isPreserveCase)
        {
            await Task.Run(() =>
            {
                // decide whhich function should be used to substitute the matches
                // this is to reduce the number of unneccesary checks in these functions
                var subMatches = AhoCorasickHelper.SelectSubstituteMatchesMethod(isWholeWord, isPreserveCase);

                using var sw = new StreamWriter(dest);

                // write the substitutes to the new file
                foreach (string line in File.ReadLines(src))
                {
                    sw.WriteLine(subMatches(replacePhrases, line, matcher));
                }
            }, default);
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
        private static async void ReadFromTextCsvTsvWriteToDocx(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord, bool isPreserveCase)
        {
            await Task.Run(() =>
            {
                // decide whhich function should be used to substitute the matches
                // this is to reduce the number of unneccesary checks in these functions
                var subMatches = AhoCorasickHelper.SelectSubstituteMatchesMethod(isWholeWord, isPreserveCase);

                // create the new document
                using var document = WordprocessingDocument.Create(dest, WordprocessingDocumentType.Document);
                MainDocumentPart mainDocPart = document.AddMainDocumentPart();
                mainDocPart.Document = new Wordprocessing.Document();
                var body = mainDocPart.Document.AppendChild(new Wordprocessing.Body());

                foreach (string line in File.ReadLines(src))
                {
                    // create a new paragraph
                    Wordprocessing.Paragraph paragraph = body.AppendChild(new Wordprocessing.Paragraph());

                    // start the run within the paragraph
                    var run = paragraph.AppendChild(new Wordprocessing.Run());

                    // set the run properties
                    var runProperties = new Wordprocessing.RunProperties();
                    var font = new Wordprocessing.RunFonts() { Ascii = "Arial" };
                    var color = new Wordprocessing.Color { Val = "000000" };
                    runProperties.Append(font);
                    runProperties.Append(color);

                    run.PrependChild(runProperties);

                    run.AppendChild(new Wordprocessing.Text(subMatches(replacePhrases, line, matcher)));
                }

                mainDocPart.Document.Save();
            }, default);
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
        private static async void ReadFromDocxWriteToTextCsvTsv(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord, bool isPreserveCase)
        {
            await Task.Run(() =>
            {
                // decide whhich function should be used to substitute the matches
                // this is to reduce the number of unneccesary checks in these functions
                var subMatches = AhoCorasickHelper.SelectSubstituteMatchesMethod(isWholeWord, isPreserveCase);

                using var sw = new StreamWriter(dest);

                using var document = WordprocessingDocument.Open(src, false);

                if (document.MainDocumentPart == null || document.MainDocumentPart.Document.Body == null)
                {
                    Debug.WriteLine("ReadFromDocxWriteToTextCsvTsv(): MainDocumentPart or its body is null");
                    return;
                }

                var textNodes = document.MainDocumentPart.Document.Body.Descendants<Wordprocessing.Text>();

                foreach (var textNode in textNodes)
                {
                    sw.WriteLine(subMatches(replacePhrases, textNode.InnerText, matcher));
                }
            }, default);
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
        private static async void ReadFromDocxWriteToDocx(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord, bool isPreserveCase)
        {
            await Task.Run(() =>
            {
                // decide whhich function should be used to substitute the matches
                // this is to reduce the number of unneccesary checks in these functions
                var subMatches = AhoCorasickHelper.SelectSubstituteMatchesMethod(isWholeWord, isPreserveCase);

                File.Copy(src, dest, true);

                using var document = WordprocessingDocument.Open(dest, true);

                if (document.MainDocumentPart == null || document.MainDocumentPart.Document.Body == null)
                {
                    Debug.WriteLine("ReadFromDocxWriteToDocx(): MainDocumentPart or its body is null");
                    return;
                }

                var textNodes = document.MainDocumentPart.Document.Body.Descendants<Wordprocessing.Text>();

                foreach (var textNode in textNodes)
                {
                    textNode.Text = subMatches(replacePhrases, textNode.InnerText, matcher);
                }

                document.MainDocumentPart.Document.Save();
            }, default);
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
        private static async void ReadFromExcelWriteToExcel(Dictionary<string, string> replacePhrases,
            string src, string dest, AhoCorasickStringSearcher matcher, bool isWholeWord, bool isPreserveCase)
        {
            await Task.Run(() =>
            {
                // decide whhich function should be used to substitute the matches
                // this is to reduce the number of unneccesary checks in these functions
                var subMatches = AhoCorasickHelper.SelectSubstituteMatchesMethod(isWholeWord, isPreserveCase);

                File.Copy(src, dest, true);

                using var document = SpreadsheetDocument.Open(dest, true);

                if (document.WorkbookPart == null || document.WorkbookPart.Workbook.Sheets == null)
                {
                    throw new NullReferenceException("ReadFromExcelWriteToExcel(): WorkbookPart or its sheets is null");
                }

                WorkbookPart wbPart = document.WorkbookPart;
                List<Sheet> sheets = wbPart.Workbook.Sheets.Elements<Sheet>().ToList();

                foreach (var sheet in sheets)
                {
                    if (sheet.Id?.Value == null)
                    {
                        continue;
                    }

                    WorksheetPart wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id.Value);
                    var cells = wsPart.Worksheet.Descendants<Cell>();
                    int id = -1;

                    foreach (var cell in cells)
                    {
                        if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                        {
                            if (Int32.TryParse(cell.InnerText, out id))
                            {
                                string? text = wbPart.SharedStringTablePart?.SharedStringTable.Elements<SharedStringItem>().ElementAt(id).InnerText;
                                if (text == null)
                                {
                                    continue;
                                }

                                cell.DataType = CellValues.String;
                                cell.CellValue = new CellValue(subMatches(replacePhrases, text, matcher));
                            }
                        }
                        else if (cell.DataType != null && cell.DataType == CellValues.String)
                        {
                            cell.CellValue = new CellValue(subMatches(replacePhrases, cell.InnerText, matcher));
                        }
                    }

                    document.Save();
                }
            }, default);
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

            OutputFiles[i].FileName = string.Format(@"{0}\{1}{2}",
                                                    Path.GetDirectoryName(OutputFiles[i].FileName),
                                                    Path.GetFileNameWithoutExtension(OutputFiles[i].FileName),
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
                outputFile.FileName = string.Format(@"{0}\{1}{2}",
                                                    Path.GetDirectoryName(outputFile.FileName),
                                                    Path.GetFileNameWithoutExtension(outputFile.FileName),
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
}
