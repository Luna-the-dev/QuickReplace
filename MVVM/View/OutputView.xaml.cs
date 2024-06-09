using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TextReplace.MVVM.ViewModel;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView : UserControl
    {
        public OutputView()
        {
            InitializeComponent();
        }

        private void PerformReplacementsOnAllFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Replacify";
            string body = "Perform replacements on all files.";

            var dialog = new PopupWindows.ReplaceFilesWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == false)
            {
                return;
            }

            OutputViewModel.ReplaceAll(dialog.OpenFileLocation);

            if (dialog.OpenFileLocation == false)
            {
                return;
            }

            // if the user selected to topen the file location,
            // open the file explorer and highlight the first generated file
            string filePath = ((OutputViewModel)DataContext).OutputFiles[0].FileName;
            Process.Start("explorer.exe", "/select, " + filePath);
        }

        private void PerformReplacementsOnSelectedFile_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Replacify";
            string body = "Perform replacements on the selected file.";

            var dialog = new PopupWindows.ReplaceFilesWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == false)
            {
                return;
            }

            OutputViewModel.ReplaceSelected(dialog.OpenFileLocation);

            if (dialog.OpenFileLocation == false)
            {
                return;
            }

            // if the user selected to topen the file location,
            // open the file explorer and highlight the first generated file
            string? filePath = ((OutputViewModel)DataContext).SelectedFile.FileName;
            Process.Start("explorer.exe", "/select, " + filePath);
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
                OutputViewModel.SetAllOutputFileTypes(dialog.OutputFileType);
            }
        }

        private void OpenSelectedFileTypeWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Output File Type";
            string body = "Choose the type that the selected file will be converted to.\n" +
                "<u>Note:</u> Excel files will not be converted";

            var dialog = new PopupWindows.SetOutputFileTypeWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                ((OutputViewModel)DataContext).SetSelectedOutputFileType(dialog.OutputFileType);
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
                OutputViewModel.SetOutputFilesStyling(dialog.Bold, dialog.Italics, dialog.Underline,
                    dialog.Strikethrough, dialog.HighlightColor, dialog.TextColor);
            }
        }

        /// <summary>
        /// This function exists to make scroll wheel work for a scroll viewer with a list box
        /// inside of it. It normally doesnt work out of the box because a list box's template
        /// already contains a scroll viewer, but that has some of its own problems (namely the
        /// inability to drag the scroll bar when an item is selected). This also allows me to
        /// control the styling of the scroll viewer independently.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta / 6));
            e.Handled = true;
        }
    }
}
