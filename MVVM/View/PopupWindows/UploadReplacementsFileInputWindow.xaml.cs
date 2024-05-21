using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
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

        public string FullFileName
        {
            get { return ((UploadReplacementsInputViewModel)DataContext).FullFileName; }
            set { ((UploadReplacementsInputViewModel)DataContext).FullFileName = value; }
        }

        public UploadReplacementsInputWindow()
        {
            InitializeComponent();
        }

        public UploadReplacementsInputWindow(Window owner, string title, string body)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            BodyText = body;
            DefaultBodyText = body;
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

            ((UploadReplacementsInputViewModel)DataContext).ValidateFile(dialog.FileName);
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
    }
}