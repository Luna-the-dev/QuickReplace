using System.Windows;
using System.Windows.Controls;

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

        private void OpenDelimiterInputWindow(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = (sender as Button)?.Content.ToString() ?? string.Empty;
            string body = "Enter the delimiter used to seperate the original and replacement words.\n" +
                "<u>Note:</u> This defaults to a comma for .csv files and a tab character for .tsv files.";
            string defaultInputTest = "Ex. :, -, or ;";
            var dialog = new InputWindow(window, title, body, defaultInputTest);
            dialog.Show();
        }
    }
}
