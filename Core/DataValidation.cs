namespace TextReplace.Core
{
    class DataValidation
    {
        public static bool AreReplacePhrasesValid(Dictionary<string, string> phrases)
        {
            foreach (var phrase in phrases)
            {
                // if the thing to replace is empty, its invalid
                if (phrase.Key == string.Empty)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
