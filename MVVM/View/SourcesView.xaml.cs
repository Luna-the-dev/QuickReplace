using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TextReplace.MVVM.ViewModel;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for SourcesView.xaml
    /// </summary>
    public partial class SourcesView : UserControl
    {
        public SourcesView()
        {
            InitializeComponent();
        }

        private void OpenUploadWindow_OnClick(object sender, RoutedEventArgs e)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(uploadOption.Text);

            var dialog = new PopupWindows.UploadSourceFilesInputWindow(window, title);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                SourcesViewModel.SetNewSourceFiles(dialog.FullFileNames);
            }
        }

        private void RemoveSelectedFile_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Remove File";
            string body = "Are you sure you would like to remove the selected file?";

            var dialog = new PopupWindows.ConfirmWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                int index = listBox.Items.IndexOf(((Button)sender).DataContext);
                ((SourcesViewModel)DataContext).RemoveSourceFile(index);
            }
        }

        private void RemoveAllFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Remove Files";
            string body = "Are you sure you would like to remove all files?";

            var dialog = new PopupWindows.ConfirmWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                SourcesViewModel.RemoveAllSourceFiles();
            }
        }

        private void SetOutputDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Set Directory";
            string body = "Please select a directory which the files will be saved to.";

            var dialog = new PopupWindows.SetOutputDirectoryWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                ((SourcesViewModel)DataContext).UpdateSourceFileOutputDirectory(dialog.DirectoryName);
            }

            if (dialog.BtnDefault.IsChecked == true)
            {
                ((SourcesViewModel)DataContext).UpdateSourceFileOutputDirectory("");
            }
        }

        private void SetGlobalOutputDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Set Directory";
            string body = "Please select a directory which the files will be saved to.";

            var dialog = new PopupWindows.SetOutputDirectoryWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                SourcesViewModel.UpdateAllSourceFileOutputDirectories(dialog.DirectoryName);
            }

            if (dialog.BtnDefault.IsChecked == true)
            {
                SourcesViewModel.UpdateAllSourceFileOutputDirectories("");
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
