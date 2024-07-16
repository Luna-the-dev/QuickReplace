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
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Upload";
            string body = "Upload a file for the replacement phrases.";

            var dialog = new PopupWindows.UploadReplacementsInputWindow(window, title, body);

            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                ((TopBarViewModel)DataContext).SetNewReplaceFile(dialog.FullFileName);
                TopBarViewModel.SetActiveContentView("replace");
            }
        }

        private void UploadSourceFile_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Upload";
            string body = "Upload Text or Document files for the sources to perform the replacements on.";

            var dialog = new PopupWindows.UploadSourceFilesInputWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                ((TopBarViewModel)DataContext).SourceFiles(dialog.FullFileNames);
                TopBarViewModel.SetActiveContentView("sources");
            }
        }

        private async void PerformReplacementsOnAllFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Replacify";
            string body = "Perform replacements on all files.";

            var dialog = new PopupWindows.ReplaceFilesWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnCancel.IsChecked == false)
            {
                return;
            }

            string filePath = await TopBarViewModel.ReplaceAll(dialog.OpenFileLocation);

            // if the user selected to open the file location,
            // open the file explorer and highlight the first generated file
            if (dialog.OpenFileLocation)
            {
                Process.Start("explorer.exe", "/select, " + filePath);
            }

            TopBarViewModel.SetActiveContentView("output");
        }

        private void OpenGlobalFileTypeWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Output File Type";
            string body = "Choose the type each output file will be converted to.\n" +
                "<u>Note:</u> Excel files will not be converted";

            var dialog = new PopupWindows.SetOutputFileTypeWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                // yes i know calling another VM from this view is bad.
                // ill come up with a better solution if i need to add more of this
                OutputViewModel.SetAllOutputFileTypes(dialog.OutputFileType);
            }
        }

        private void OpenOutputStylingWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Styling";
            string body = "Select the styling properties that will be applied to the replacements in the output files.\n" +
                "(For Document and Excel files only.)";

            var dialog = new PopupWindows.SetOutputStylingWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                // yes i know calling another VM from this view is bad.
                // ill come up with a better solution if i need to add more of this
                OutputViewModel.SetOutputFilesStyling(dialog.Bold, dialog.Italics, dialog.Underline,
                    dialog.Strikethrough, dialog.IsHighlighted, dialog.IsTextColored, dialog.HighlightColor, dialog.TextColor);
            }
        }
    }
}
