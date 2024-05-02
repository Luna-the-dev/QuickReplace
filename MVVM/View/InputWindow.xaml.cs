using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

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
            set
            {
                var parts = value.Split(new[] { "<u>", "</u>" }, StringSplitOptions.None);
                bool isUnderline = false; // Start in normal mode
                foreach (var part in parts)
                {
                    if (isUnderline)
                        BodyTextBox.Inlines.Add(new Underline(new Run(part)));
                    else
                        BodyTextBox.Inlines.Add(new Run(part));

                    isUnderline = !isUnderline; // toggle between bold and not bold
                }
            }
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
