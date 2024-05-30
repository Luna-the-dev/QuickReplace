using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TextReplace.Core.Validation;
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
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Upload";
            string body = "Upload a file for the replacement phrases.";
            string delimiterBody = "Enter the character used to seperate the original phrases from the replacements:";
            string delimiterInputWatermark = "Ex. :, -, or ;";


            var dialog = new PopupWindows.UploadReplacementsInputWindow(window, title, body, delimiterBody, delimiterInputWatermark);

            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                if (FileValidation.IsTextFile(dialog.FullFileName))
                {
                    ((TopBarViewModel)DataContext).SetNewReplaceFile(dialog.FullFileName, dialog.DelimiterInputText);
                }
                else
                {
                    ((TopBarViewModel)DataContext).SetNewReplaceFile(dialog.FullFileName);
                }
            }
        }

        private void UploadSourceFile_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Upload";

            var dialog = new PopupWindows.UploadSourceFilesInputWindow(window, title);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                ((TopBarViewModel)DataContext).SourceFiles(dialog.FullFileNames);
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
            string watermark = "-replacify";

            var dialog = new PopupWindows.SetSuffixInputResetWindow(window, title, body, watermark);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true || dialog.BtnReset.IsChecked == true)
            {
                viewModel.SetSuffix(dialog.InputText);
            }
        }
    }
}
