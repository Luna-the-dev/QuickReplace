using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for TopBorderView.xaml
    /// </summary>
    public partial class TopBorderView : UserControl
    {
        public static readonly DependencyProperty _windowNameProperty =
            DependencyProperty.Register("WindowName", typeof(string), typeof(UserControl), new FrameworkPropertyMetadata(null));
        public string? WindowName
        {
            get { return (string)GetValue(_windowNameProperty); }
            set { SetValue(_windowNameProperty, value); }
        }
        public string? TitleText
        {
            get
            {
                if (Title.Content == null)
                {
                    return string.Empty;
                }
                return Title.Content.ToString();
            }
            set { Title.Content = value; }
        }

        public TopBorderView()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                TitleText = WindowName;
            };
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window.GetWindow(sender as DependencyObject).DragMove();
            }
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(sender as DependencyObject).Close();
        }
    }
}
