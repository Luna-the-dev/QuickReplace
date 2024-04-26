using System.Diagnostics;
using TextReplace.Core;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    class ReplaceViewModel
    {
        // commands
        public RelayCommand Replace {  get; set; }

        public ReplaceViewModel()
        {
            Replace = new RelayCommand(o =>
            {
                ReplaceData replaceData = new ReplaceData();

                // open a file dialogue for the user and update the source files
                bool result = replaceData.SaveReplacePhrases();

                if (result == false)
                {
                    Debug.WriteLine("Replace phrases could not be parsed.");
                }

                foreach(var pair in replaceData.ReplacePhrases)
                {
                    Debug.WriteLine($"{pair.Item1}\t{pair.Item2}");
                }
            });
        }
    }
}
