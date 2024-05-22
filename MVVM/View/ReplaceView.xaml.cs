using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TextReplace.Core.Validation;
using TextReplace.MVVM.ViewModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for ReplaceView.xaml
    /// </summary>
    public partial class ReplaceView : UserControl
    {
        public ReplaceView()
        {
            InitializeComponent();
        }

        private void OpenUploadWindow_OnClick(object sender, RoutedEventArgs e)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(uploadOption.Text);
            string body = "Upload a file for the replacement phrases.";

            var dialog = new PopupWindows.UploadReplacementsInputWindow(window, title, body);

            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                if (DataValidation.IsTextFile(dialog.FullFileName))
                {
                    ReplaceViewModel.SetNewReplaceFile(dialog.FullFileName, dialog.DelimiterInputText);
                }
                else
                {
                    ReplaceViewModel.SetNewReplaceFile(dialog.FullFileName);
                }
            }
        }

        private void OpenCreateWindow_OnClick(object sender, RoutedEventArgs e)
        {
            ReplaceViewModel.CreateNewReplaceFile();
        }

        private void OpenEditWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (ReplaceViewModel)DataContext;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(editMenuOption.Text);
            string body = "Edit the replacement phrase.";
            string topWatermark = "Original";
            string bottomWatermark = "Replacement";
            string topInputText = viewModel.SelectedPhrase.Item1 ?? string.Empty;
            string bottomInputText = viewModel.SelectedPhrase.Item2 ?? string.Empty;

            var dialog = new PopupWindows.EditPhraseDoubleInputWindow(window, title, body,
                                                                      topWatermark, bottomWatermark,
                                                                      topInputText, bottomInputText);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                viewModel.EditSelectedPhrase(dialog.TopInputText, dialog.BottomInputText);
            }
        }

        private void OpenRemoveWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (ReplaceViewModel)DataContext;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(editMenuOption.Text);
            string body = "Are you sure you would like to remove the selected phrase?";

            var dialog = new PopupWindows.ConfirmWindow(window, title, body);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                viewModel.RemoveSelectedPhrase();
            }
        }

        private void OpenAddWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (ReplaceViewModel)DataContext;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(editMenuOption.Text);
            string body = "Add a replacement phrase.";
            string topWatermark = "Original";
            string bottomWatermark = "Replace with";

            var dialog = new PopupWindows.AddPhraseDoubleInputWindow(window, title, body, topWatermark, bottomWatermark);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                viewModel.AddNewPhrase(dialog.TopInputText, dialog.BottomInputText);
            }
        }

        private void OpenSaveAsWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (ReplaceViewModel)DataContext;
            string extension = Path.GetExtension(viewModel.FileName).ToLower();

            string csv = "CSV File (*.csv)|*.csv|";
            string tsv = "TSV File (*.tsv)|*.tsv|";
            string xls = "Excel File (*.xlsx)|*.xls;*.xlsx|";
            string txt = "Text Document (*.txt)|*.txt|";
            string all = "All files (*.*)|*.*|";

            string filter = extension switch
            {
                ".csv" =>            csv + tsv + xls + txt + all,
                ".tsv" =>            tsv + csv + xls + txt + all,
                ".xls" or ".xlsx" => xls + csv + tsv + txt + all,
                ".txt" =>            txt + csv + tsv + xls + all,
                _ =>                 all + csv + tsv + xls + txt
            };

            filter = filter.Remove(filter.Length - 1);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save FIle As",
                FileName = viewModel.FileName, // Default file name
                DefaultExt = extension, // Default file extension
                Filter = filter // Filter files by extension
            };

            // open file dialog box
            if (dialog.ShowDialog() != true)
            {
                Debug.WriteLine("New file window was closed.");
                return;
            }

            string? delimiter = null;

            if (DataValidation.IsTextFile(dialog.FileName))
            {
                // get a new delimiter
                var window = Window.GetWindow(sender as DependencyObject);
                string title = "Text File Delimiter";

                var delimiterDialog = new PopupWindows.SetDelimiterInputWindow(window, title);
                delimiterDialog.ShowDialog();

                if (delimiterDialog.BtnOk.IsChecked == false)
                {
                    Debug.WriteLine("No delimiter was set, save aborted.");
                    return;
                }

                delimiter = delimiterDialog.InputText;
            }

            // save the replace phrases to the new file name
            viewModel.SavePhrasesToFile(dialog.FileName, delimiter);
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
