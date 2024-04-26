using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextReplace.Core
{
    class DataValidation
    {
        public static bool AreReplacePhrasesValid(List<(string, string)> phrases)
        {
            foreach (var phrase in phrases)
            {
                // if the thing to replace is empty, its invalid
                if (phrase.Item1 == string.Empty)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
