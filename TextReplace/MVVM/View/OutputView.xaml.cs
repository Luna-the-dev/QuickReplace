using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TextReplace.Messages.Output;
using TextReplace.MVVM.ViewModel;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView : UserControl,
        IRecipient<SkipOutputFileMsg>
    {
        public OutputView()
        {
            InitializeComponent();

            var viewModel = (OutputViewModel)DataContext;
            Loaded += (s, e) =>
            {
                viewModel.IsActive = true;
                WeakReferenceMessenger.Default.RegisterAll(this);
            };

            Unloaded += (s, e) =>
            {
                viewModel.IsActive = false;
                WeakReferenceMessenger.Default.UnregisterAll(this);
            };
        }

        private async void PerformReplacementsOnAllFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "QuickReplace";
            string body = "Perform replacements on all files.";

            var dialog = new PopupWindows.ReplaceFilesWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == false)
            {
                return;
            }

            await OutputViewModel.ReplaceAll(dialog.OpenFileLocation);

            if (dialog.OpenFileLocation == false)
            {
                return;
            }

            // if the user selected to topen the file location,
            // open the file explorer and highlight the first generated file
            var viewModel = (OutputViewModel)DataContext;
            string filePath = "";

            foreach (var file in viewModel.OutputFiles)
            {
                // if there was an error when writing to the file, dont attempt to open it
                if (file.NumOfReplacements < 0)
                {
                    continue;
                }
                filePath = file.FileName;
            }

            if (filePath != string.Empty)
            {
                filePath = filePath.Replace("/", "\\");
                Process.Start("explorer.exe", "/select, " + filePath);
            }
        }

        private async void PerformReplacementsOnSelectedFile_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "QuickReplace";
            string body = "Perform replacements on the selected file.";

            var dialog = new PopupWindows.ReplaceFilesWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == false)
            {
                return;
            }

            await OutputViewModel.ReplaceSelected(dialog.OpenFileLocation);

            Debug.WriteLine("hey!");

            if (dialog.OpenFileLocation == false)
            {
                return;
            }

            // if the user selected to topen the file location,
            // open the file explorer and highlight the first generated file
            var viewModel = (OutputViewModel)DataContext;

            // if there was an error when writing to the file, dont attempt to open it
            if (viewModel.SelectedFile.NumOfReplacements < 0)
            {
                return;
            }

            string? filePath = viewModel.SelectedFile.FileName;
            filePath = filePath.Replace("/", "\\");
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
                    dialog.Strikethrough, dialog.IsHighlighted, dialog.IsTextColored, dialog.HighlightColor, dialog.TextColor);
            }
        }

        private void OpenSkipOutputFileWindow(string fileName, bool fileIsInUse)
        {
            var window = Window.GetWindow(this);
            string title = "Error";
            string body;
            
            if (fileIsInUse)
            {
                body = $"Replacements could not be performed on <u>{fileName}</u> because the file is already in use.";
            }
            else
            {
                body = $"There was an unexpected error while performing the replacements on <u>{fileName}</u>.";
            }

            var dialog = new PopupWindows.SkipOutputFileWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnRetry.IsChecked == true)
            {
                OutputViewModel.SetRetryReplacementsOnFile(true);
            }
            else if (dialog.BtnSkip.IsChecked == true)
            {
                OutputViewModel.SetRetryReplacementsOnFile(false);
            }
        }

        void IRecipient<SkipOutputFileMsg>.Receive(SkipOutputFileMsg message)
        {
            // this is needed because this message is likely sent from another thread
            Dispatcher.Invoke(() =>
            {
                OpenSkipOutputFileWindow(fileName: message.Value.Item1, fileIsInUse: message.Value.Item2);
            });
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
