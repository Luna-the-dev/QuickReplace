using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TextReplace.MVVM.ViewModel;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for TopBarView.xaml
    /// </summary>
    public partial class TopBarView : UserControl
    {
        public TopBarView()
        {
            InitializeComponent();
        }

        private void UploadReplaceFile_OnClick(object sender, RoutedEventArgs e)
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

            ((TopBarViewModel)DataContext).ReplaceFile(dialog.FileName);
        }

        private void UploadSourceFile_OnClick(object sender, RoutedEventArgs e)
        {
            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Text Files",
                FileName = "Document", // Default file name
                DefaultExt = ".txt", // Default file extension
                Filter = "Text documents (.txt)|*.txt", // Filter files by extension
                Multiselect = true
            };

            // open file dialog box
            if (dialog.ShowDialog() != true)
            {
                Debug.WriteLine("Replace file upload window was closed.");
                return;
            }

            ((TopBarViewModel)DataContext).SourceFiles(dialog.FileNames);
        }

        private void OpenDelimiterInputWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = (TopBarViewModel)(DataContext);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(delimiterMenuOption.Text);
            string body;
            if (viewModel.Delimiter != string.Empty)
            {
                body = $"<u>Current delimiter:</u> {viewModel.Delimiter}";
            }
            else
            {
                body = "Enter the delimiter used to seperate the original and replacement words.\n" +
                "<u>Note:</u> This defaults to a comma for .csv files and a tab character for .tsv files.";
            }
            string defaultInputTest = "Ex. :, -, or ;";

            var dialog = new PopupWindows.InputResetWindow(window, title, body, defaultInputTest);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true || dialog.BtnReset.IsChecked == true)
            {
                viewModel.SetDelimiter(dialog.InputText);
            }
        }

        private void OpenFileSuffixInputWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = (TopBarViewModel)(DataContext);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(suffixMenuOption.Text);
            string body;
            if (viewModel.Suffix != string.Empty)
            {
                body = $"<u>Current suffix:</u> {viewModel.Suffix}";
            }
            else
            {
                body = "Enter suffix which will be appended onto the output file names.\n" +
                "<u>Note:</u> This defaults to \"-replacify\"";
            }
            string defaultInputTest = "-replacify";

            var dialog = new PopupWindows.InputResetWindow(window, title, body, defaultInputTest);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true || dialog.BtnReset.IsChecked == true)
            {
                viewModel.SetSuffix(dialog.InputText);
            }
        }
    }
}
