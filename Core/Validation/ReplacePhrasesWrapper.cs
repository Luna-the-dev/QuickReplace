namespace TextReplace.Core.Validation
{
    /// <summary>
    /// Wrapper class for the replace phrases dictionary in MVVM.Model.ReplaceFileData.
    /// This wrapper exists only to read in data with CsvHelper.
    /// </summary>
    public class ReplacePhrasesWrapper
    {
        public string oldText { get; set; }
        public string newText { get; set; }
    }
}
