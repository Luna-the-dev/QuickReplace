using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class SetDelimiterInputWindow : Window
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
                string[] separator = ["<u>", "</u>"];
                var parts = value.Split(separator, StringSplitOptions.None);
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
        public string DefaultBodyText = string.Empty;

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

        public SetDelimiterInputWindow(Window owner, string title)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            DefaultBodyText = "Enter the character you wish to use to seperate the original phrases from the replacements in the text file:";
            BodyText = DefaultBodyText;
            InputWatermarkText = "Ex. :, -, or ;";
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // if the user didnt enter a value, dont replace
            // the delimiter with an empty string.
            if (InputText == string.Empty)
            {
                BtnCancel.IsChecked = true;
            }
            else
            {
                BtnOk.IsChecked = true;
            }
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsChecked = true;
            Close();
        }

        private void InputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            // if the user presses enter and has typed something in,
            // close window. if nothing is typed, do nothing
            if (e.Key == Key.Return && InputText != string.Empty)
            {
                BtnOk.IsChecked = true;
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }
    }
}