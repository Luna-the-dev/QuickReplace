using MahApps.Metro.Controls;
using System.ComponentModel;
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

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainViewModel.SaveUserSettings();
        }
    }
}