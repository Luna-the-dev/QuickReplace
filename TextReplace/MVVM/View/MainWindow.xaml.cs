using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Effects;
using TextReplace.Messages;
using TextReplace.MVVM.View.PopupWindows;
using TextReplace.MVVM.ViewModel;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private HowToUseWindow? _htuWindow = null;

        public MainWindow()
        {
            InitializeComponent();

            var viewModel = (MainViewModel)DataContext;
            Loaded += (s, e) => viewModel.IsActive = true;
            Unloaded += (s, e) => viewModel.IsActive = false;

            // close the "How to use" window if it is open
            // and the application is clicked on
            MouseDown += (s, e) =>
            {
                if (_htuWindow != null)
                {
                    _htuWindow.Close();
                    _htuWindow = null;

                    // unblur the main window
                    Opacity = 1;
                    Effect = null;

                    ChildContent.IsHitTestVisible = true;
                }
            };

            SizeChanged += (s, e) =>
            {
                var leftOffset = Left + (ActualWidth * (0.15 / 2));
                var topOffset = Top + (ActualHeight * (0.15 / 2));
                WeakReferenceMessenger.Default.Send(new WindowLocationMsg((leftOffset, topOffset)));
                WeakReferenceMessenger.Default.Send(new WindowSizeMsg((Width * 0.85, Height * 0.85)));
            };

            LocationChanged += (s, e) =>
            {
                var leftOffset = Left + (ActualWidth * (0.15 / 2));
                var topOffset = Top + (ActualHeight * (0.15 / 2));
                WeakReferenceMessenger.Default.Send(new WindowLocationMsg((leftOffset, topOffset)));
                WeakReferenceMessenger.Default.Send(new WindowSizeMsg((Width * 0.85, Height * 0.85)));
            };
        }

        private void OpenHowToUseWindow_OnClick(object sender, RoutedEventArgs e)
        {
            // the button should do nothing if the how to use window is already active
            if (_htuWindow != null)
            {
                return;
            }

            var window = GetWindow(this);

            // blur the main window
            Opacity = 0.5;
            Effect = new BlurEffect();

            _htuWindow = new HowToUseWindow(window, (Width * 0.85), (Height * 0.85));
            _htuWindow.Closing += (sender, args) => { _htuWindow.Owner = null; };

            ChildContent.IsHitTestVisible = false;

            // Open the child window
            _htuWindow.Show();
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

                var dialog = new InProgressConfirmWindow(window, title, body);
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

                var dialog = new InProgressConfirmWindow(window, title, body);
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

        private void Border_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}