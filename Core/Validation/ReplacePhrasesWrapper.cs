namespace TextReplace.Core.Validation
{
    /// <summary>
    /// Wrapper class for the replace phrases dictionary in MVVM.Model.ReplaceData.
    /// This wrapper exists only to read in data with CsvHelper.
    /// </summary>
    public class ReplacePhrasesWrapper
    {
        public string OldText { get; set; } = "";
        public string NewText { get; set; } = "";
    }
}
