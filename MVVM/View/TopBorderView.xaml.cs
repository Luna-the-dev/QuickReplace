using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextReplace.UserControls;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for TopBorderView.xaml
    /// </summary>
    public partial class TopBorderView : UserControl
    {
        public string? WindowName
        {
            get { return (string)GetValue(WindowNameProperty); }
            set { SetValue(WindowNameProperty, value); }
        }
        public static readonly DependencyProperty WindowNameProperty =
            DependencyProperty.Register(
                name: "WindowName",
                propertyType: typeof(string),
                ownerType: typeof(TopBorderView),
                typeMetadata: new FrameworkPropertyMetadata(null));

        public Visibility MinimizeButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonOnlyProperty); }
            set { SetValue(CloseButtonOnlyProperty, Visibility); }
        }
        public static readonly DependencyProperty CloseButtonOnlyProperty =
            DependencyProperty.Register(
                name: "CloseButtonOnly",
                propertyType: typeof(Visibility),
                ownerType: typeof(TopBorderView),
                typeMetadata: new PropertyMetadata(Visibility.Collapsed));

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
