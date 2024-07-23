using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using TextReplace.MVVM.ViewModel;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = (MainViewModel)DataContext;
            Loaded += (s, e) => viewModel.IsActive = true;
            Unloaded += (s, e) => viewModel.IsActive = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;

            // if the window is being closed while the replace phrases are being saved
            if (viewModel.IsSavingReplacementsInProgress)
            {
                var window = GetWindow(sender as DependencyObject);
                string title = "Saving Replacements in Progress";
                string body = "<u>Warning:</u> Saving replacement phrases in progress.\n\n" +
                    "If you close the window before this is finished, the replacement phrases may not be saved. " +
                    "Are you sure you would like to exit?";

                var dialog = new PopupWindows.InProgressConfirmWindow(window, title, body);
                dialog.ShowDialog();

                if (dialog.BtnCancel.IsChecked == false)
                {
                    e.Cancel = true;
                }
            }

            // if the window is being closed while the replacements are being done
            if (viewModel.IsReplacementInProgress)
            {
                var window = GetWindow(sender as DependencyObject);
                string title = "Replacements in Progress";
                string body = "<u>Warning:</u> Replacements in progress.\n\n" +
                    "If you close the window before this is finished, the replacements may not be made. " +
                    "Are you sure you would like to exit?";

                var dialog = new PopupWindows.InProgressConfirmWindow(window, title, body);
                dialog.ShowDialog();

                if (dialog.BtnCancel.IsChecked == false)
                {
                    e.Cancel = true;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainViewModel.SaveUserSettings();
        }
    }
}