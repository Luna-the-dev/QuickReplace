using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using TextReplace.MVVM.Model;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class SetSuffixInputResetWindow : Window
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
                BodyTextBox.Text = "";
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
        public string DefaultBodyText { get; set; }

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

        public SetSuffixInputResetWindow(Window owner, string title, string body, string watermark, string inputText = "")
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            DefaultBodyText = body;
            BodyText = body;
            InputWatermarkText = watermark;
            InputText = inputText;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsChecked = true;
            Close();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            InputText = string.Empty;
            BtnReset.IsChecked = true;
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

        private void InputTextBoxTextChanged(object sender, EventArgs e)
        {
            BodyText = SetSuffixInputResetViewModel.IsSuffixValid(InputText) ?
                DefaultBodyText :
                "<u>Suffix is not valid.</u>";
        }
    }
}