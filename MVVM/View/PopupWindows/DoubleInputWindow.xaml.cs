using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for DoubleInputWindow.xaml
    /// </summary>
    public partial class DoubleInputWindow : Window
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

        public string TopInputWatermarkText
        {
            get { return TopInputWatermark.Text.ToString() ?? string.Empty; }
            set
            {
                TopInputWatermark.Text = value;
            }
        }
        public string TopInputText
        {
            get { return TopInputTextBox.Text.ToString() ?? string.Empty; }
            set
            {
                TopInputTextBox.Text = value;
            }
        }

        public string BottomInputWatermarkText
        {
            get { return BottomInputWatermark.Text.ToString() ?? string.Empty; }
            set
            {
                BottomInputWatermark.Text = value;
            }
        }
        public string BottomInputText
        {
            get { return BottomInputTextBox.Text.ToString() ?? string.Empty; }
            set
            {
                BottomInputTextBox.Text = value;
            }
        }

        public DoubleInputWindow()
        {
            InitializeComponent();
        }

        public DoubleInputWindow(Window owner, string title, string body,
                                 string topWatermark, string bottomWatermark,
                                 string topInputText, string bottomInputText,
                                 int windowHeight = 200, int windowWidth = 300)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            BodyText = body;
            TopInputWatermarkText = topWatermark;
            BottomInputWatermarkText = bottomWatermark;
            TopInputText = topInputText;
            BottomInputText = bottomInputText;
            Height = windowHeight;
            Width = windowWidth;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // if the user didnt enter a value, dont replace
            // the delimiter with an empty string.
            if (TopInputText == string.Empty)
            {
                BtnCancel.IsChecked = true;
            }
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsChecked = true;
            TopInputText = string.Empty;
            BottomInputText = string.Empty;
            Close();
        }

        private void TopInputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            // if the user presses enter and has typed something in,
            // close window. if nothing is typed, do nothing
            if (e.Key == Key.Return && TopInputText != string.Empty)
            {
                BottomInputTextBox.Focus();
            }
            else if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }

        private void BottomInputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            // if the user presses enter and has typed something in,
            // close window. if nothing is typed, do nothing
            if (e.Key == Key.Return && BottomInputText != string.Empty)
            {
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }
    }
}