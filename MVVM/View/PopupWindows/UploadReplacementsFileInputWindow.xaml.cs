using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using TextReplace.Core.Validation;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for UploadReplacementsInputWindow.xaml
    /// </summary>
    public partial class UploadReplacementsInputWindow : Window
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

        public string DelimiterBodyText
        {
            get { return DelimiterBodyTextBox.Text; }
            set
            {
                DelimiterBodyTextBox.Text = "";
                string[] separator = ["<u>", "</u>"];
                var parts = value.Split(separator, StringSplitOptions.None);
                bool isUnderline = false; // Start in normal mode
                foreach (var part in parts)
                {
                    if (isUnderline)
                        DelimiterBodyTextBox.Inlines.Add(new Underline(new Run(part)));
                    else
                        DelimiterBodyTextBox.Inlines.Add(new Run(part));

                    isUnderline = !isUnderline; // toggle between bold and not bold
                }
            }
        }
        public string DefaultDelimiterBodyText { get; set; } = string.Empty;

        public string DelimiterInputWatermarkText
        {
            get { return DelimiterInputWatermark.Text.ToString() ?? string.Empty; }
            set
            {
                DelimiterInputWatermark.Text = value;
            }
        }
        public string DelimiterInputText
        {
            get { return DelimiterInputTextBox.Text.ToString() ?? string.Empty; }
            set
            {
                DelimiterInputTextBox.Text = value;
            }
        }

        public string FullFileName
        {
            get { return ((UploadReplacementsInputViewModel)DataContext).FullFileName; }
            set { ((UploadReplacementsInputViewModel)DataContext).FullFileName = value; }
        }

        public UploadReplacementsInputWindow(Window owner, string title)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            DefaultBodyText = "Upload a file for the replacement phrases.";
            BodyText = DefaultBodyText;
            DefaultDelimiterBodyText = "Enter the character used to seperate the original phrases from the replacements:";
            DelimiterBodyText = DefaultDelimiterBodyText;
            DelimiterInputWatermarkText = "Ex. :, -, or ;";
        }

        private void BtnUpload_OnClick(object sender, RoutedEventArgs e)
        {
            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Text File",
                FileName = "Document", // Default file name
                DefaultExt = ".txt", // Default file extension
                Filter = "All files (*.*)|*.*" // Filter files by extension
            };

            // open file dialog box
            if (dialog.ShowDialog() != true)
            {
                Debug.WriteLine("Replace file upload window was closed.");
                return;
            }

            // reset the delimiter body and input text every time a new file is uploaded
            DelimiterBodyText = DefaultDelimiterBodyText;
            DelimiterInputText = string.Empty;

            var viewModel = (UploadReplacementsInputViewModel)DataContext;

            // show the delimiter input section if it is a text file
            if (DataValidation.IsTextFile(dialog.FileName))
            {
                viewModel.ShowDelimiter(dialog.FileName);
            }
            // else hide the delimiter input section and validate the uploaded file
            else
            {
                viewModel.HideDelimiter();
                viewModel.ValidateFile(dialog.FileName);
            }
        }

        private void BtnEnterDelimiter_OnClick(object sender, RoutedEventArgs e)
        {
            DelimiterBodyText = ((UploadReplacementsInputViewModel)DataContext).ValidateDelimiter() ?
                "<u>File parsed successfully.</u>" :
                "<u>File could not be parsed with this string.</u>";
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            BtnOk.IsChecked = true;
            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsChecked = true;
            Close();
        }

        private void DelimiterInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // if the user presses enter and has typed something in, trigger
            // BtnEnterDelimiter_OnClick. if nothing is typed, do nothing
            if (e.Key == Key.Return && DelimiterInputText != string.Empty)
            {
                BtnEnterDelimiter_OnClick(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }

        private void DelimiterInputTextBox_TextChanged(object sender, EventArgs e)
        {
            DelimiterBodyText = DefaultDelimiterBodyText;
        }
    }
}