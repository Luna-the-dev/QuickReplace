using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using TextReplace.MVVM.Model;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for DoubleInputWindow.xaml
    /// </summary>
    public partial class EditPhraseDoubleInputWindow : Window
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
        public string DefaultBodyText { get; set; } = string.Empty;

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
        public string DefaultTopInputText { get; set; } = string.Empty;

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

        public EditPhraseDoubleInputWindow()
        {
            InitializeComponent();
        }

        public EditPhraseDoubleInputWindow(Window owner, string title, string body,
                                           string topWatermark, string bottomWatermark,
                                           string topInputText = "", string bottomInputText = "")
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            DefaultBodyText = body;
            BodyText = body;
            TopInputWatermarkText = topWatermark;
            BottomInputWatermarkText = bottomWatermark;
            DefaultTopInputText = topInputText;
            TopInputText = topInputText;
            BottomInputText = bottomInputText;
        }

        // please ignore this MVVM violation, breaking MVVM keeps this from getting too complicated.
        // doing this in here rather than in the view model so that it can keep underscore styling
        private void TopInputTextBoxTextChanged(object sender, EventArgs e)
        {
            if (TopInputText == DefaultTopInputText)
            {
                BodyText = DefaultBodyText;
                return;
            }

            BodyText = (ReplaceData.ReplacePhrasesDict.ContainsKey(TopInputText)) ?
                "<u>Original phrase already exists.</u>" :
                DefaultBodyText;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // if the user didnt enter a value, dont replace
            // the delimiter with an empty string.
            if (TopInputText == string.Empty)
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