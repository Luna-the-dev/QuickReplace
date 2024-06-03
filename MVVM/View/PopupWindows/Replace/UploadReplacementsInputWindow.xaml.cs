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

        public string FullFileName
        {
            get { return ((UploadReplacementsInputViewModel)DataContext).FullFileName; }
            set { ((UploadReplacementsInputViewModel)DataContext).FullFileName = value; }
        }

        public UploadReplacementsInputWindow(Window owner, string title, string body)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            DefaultBodyText = body;
            BodyText = DefaultBodyText;
        }

        private void BtnUpload_OnClick(object sender, RoutedEventArgs e)
        {
            string filter = "All files (*.*)|*.*|" +
                "CSV File (*.csv)|*.csv|" +
                "TSV File (*.tsv)|*.tsv|" +
                "Excel File (*.xlsx)|*.xlsx|" +
                "Text Document (*.txt)|*.txt";
            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Text File",
                FileName = "Document", // Default file name
                DefaultExt = ".txt", // Default file extension
                Filter = filter // Filter files by extension
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //Calculate half of the offset to move the form
            if (sizeInfo.HeightChanged)
                Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;

            if (sizeInfo.WidthChanged)
                Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
        }
    }
}