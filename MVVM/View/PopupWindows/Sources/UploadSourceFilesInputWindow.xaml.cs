using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for UploadSourceFilesInputWindow.xaml
    /// </summary>
    public partial class UploadSourceFilesInputWindow
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

        public List<string> FullFileNames
        {
            get { return ((UploadSourceFilesInputViewModel)DataContext).FullFileNames; }
            set { ((UploadSourceFilesInputViewModel)DataContext).FullFileNames = value; }
        }

        public UploadSourceFilesInputWindow()
        {
            InitializeComponent();
            Owner = new Window();
            WindowName = string.Empty;
            DefaultBodyText = string.Empty;
            BodyText = string.Empty;
        }

        public UploadSourceFilesInputWindow(Window owner, string title, string body)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            DefaultBodyText = body;
            BodyText = DefaultBodyText;
        }

        private void BtnUpload_OnClick(object sender, RoutedEventArgs e)
        {
            string filter = "Document (*.docx), Text Document (*.txt), Excel (*.xlsx), CSV (*.csv), TSV (*.tsv)|" +
                "*.docx;*.txt;*.xlsx;*.csv;*.tsv";

            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Files",
                FileName = "Document", // Default file name
                DefaultExt = ".docx", // Default file extension
                Filter = filter, // Filter files by extension
                Multiselect = true
            };

            // open file dialog box
            if (dialog.ShowDialog() != true)
            {
                Debug.WriteLine("Replace file upload window was closed.");
                return;
            }

            bool res = ((UploadSourceFilesInputViewModel)DataContext).ValidateFiles(dialog.FileNames.ToList());
            BodyText = (res) ? DefaultBodyText : "<u>File type is not supported or file does not have read permissions.</u>";
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