using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TextReplace.MVVM.ViewModel;

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

        private void OpenEditInputWindow(object sender, RoutedEventArgs e)
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

        private void OpenRemoveWindow(object sender, RoutedEventArgs e)
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

        private void OpenAddWindow(object sender, RoutedEventArgs e)
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
