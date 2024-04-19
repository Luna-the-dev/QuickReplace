using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TextReplace.Core
{
    // used for updating the UI when binding. call OnPropertyChanged() in the setter for a property
    // it seems similar to react states
    class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
