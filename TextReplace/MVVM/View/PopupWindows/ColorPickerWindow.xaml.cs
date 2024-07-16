using System.Windows;
using System.Windows.Media;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public string WindowName
        {
            get { return WindowName; }
            set { TopBorder.WindowName = value; }
        }

        public Color SelectedColor
        {
            get { return colorPicker.SelectedColor; }
            set { colorPicker.SelectedColor = value; }
        }

        public ColorPickerWindow()
        {
            InitializeComponent();
            Owner = new Window();
            WindowName = string.Empty;
            SelectedColor = new Color();
        }

        public ColorPickerWindow(Window owner, string title, Color color = new Color())
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            SelectedColor = color;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = true;
            Close();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsChecked = true;
            Close();
        }
    }
}