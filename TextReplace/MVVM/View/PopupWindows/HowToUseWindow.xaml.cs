using System.Windows;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for HowToUseWindow.xaml
    /// </summary>
    public partial class HowToUseWindow : Window
    {
        public HowToUseWindow()
        {
            InitializeComponent();
        }

        public HowToUseWindow(Window owner, int width, int height)
        {
            InitializeComponent();
            Owner = owner;
            Width = width;
            Height = height;
        }
    }
}
