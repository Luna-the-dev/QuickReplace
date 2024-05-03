using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static System.Net.Mime.MediaTypeNames;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
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
        public string InputWatermarkText
        {
            get { return InputWatermark.Text.ToString() ?? string.Empty; }
            set
            {
                InputWatermark.Text = value;
            }
        }
        public string InputText
        {
            get { return InputTextBox.Text.ToString() ?? string.Empty; }
            set
            {
                InputTextBox.Text = value;
            }
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
            InputWatermarkText = input;
            Owner = owner;
        }

        private void SetInputText(object sender, RoutedEventArgs e)
        {
            InputText = (sender as TextBox)?.Text.ToString() ?? string.Empty;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // if the user didnt enter a value, dont replace
            // the delimiter with an empty string.
            if (InputText == string.Empty)
            {
                BtnCancel.IsChecked = true;
            }
            Close();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            InputText = string.Empty;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsChecked = true;
            InputText = string.Empty;
            Close();
        }
    }
}
