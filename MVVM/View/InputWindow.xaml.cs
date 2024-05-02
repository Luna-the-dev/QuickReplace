using System.Windows;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for DelimiterInputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        public string WindowName
        {
            get { return WindowName; }
            set { TopBorder.WindowName = value; }
        }
        public string BodyText
        {
            get { return BodyTextBox.Text; }
            set { BodyTextBox.Text = value; }
        }
        public string InputText
        {
            get { return InputTextBox.Text; }
            set { InputTextBox.Text = value; }
        }

        public InputWindow()
        {
            InitializeComponent();
        }

        public InputWindow(Window owner, string title, string body, string input, int windowHeight = 200, int windowWidth = 300)
        {
            InitializeComponent();
            WindowName = title;
            BodyText = body;
            InputText = input;
            Owner = owner;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
