namespace TextReplace.Core.Validation
{
    /// <summary>
    /// Wrapper class for the replace phrases dictionary.
    /// This wrapper exists only to read in data with CsvHelper.
    /// Note: Keep the variables in the constructor the exact same as the fields.
    /// This is to make CsvHelper work without a default constructor.
    /// </summary>
    public class ReplacePhrasesWrapper(string item1, string item2)
    {
        public string Item1 { get; set; } = item1;
        public string Item2 { get; set; } = item2;
    }
}
