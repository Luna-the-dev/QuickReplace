using System.Diagnostics;
using TextReplace.Core;

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
                // open a file dialogue for the user and update the source files
                bool result = Model.ReplaceData.SaveReplacePhrases();

                if (result == false)
                {
                    Debug.WriteLine("Replace phrases could not be parsed.");
                }

                Debug.WriteLine("Starting...");

                foreach(var pair in Model.ReplaceData.ReplacePhrases)
                {
                    Debug.WriteLine($"{pair.Item1}\t{pair.Item2}");
                }
            });
        }
    }
}
