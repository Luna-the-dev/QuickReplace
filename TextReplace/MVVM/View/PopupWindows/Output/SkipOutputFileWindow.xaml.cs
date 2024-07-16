using System.Windows;
using System.Windows.Documents;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for SkipOutputFileWindow.xaml
    /// </summary>
    public partial class SkipOutputFileWindow : Window
    {
        public string WindowName
        {
            get { return WindowName; }
            set { TopBorder.WindowName = value; }
        }

        public string BodyText
        {
            get { return BodyTextBox.Text; }
            set
            {
                BodyTextBox.Text = "";
                string[] separator = ["<u>", "</u>"];
                var parts = value.Split(separator, StringSplitOptions.None);
                bool isUnderline = false; // Start in normal mode
                foreach (var part in parts)
                {
                    if (isUnderline)
                        BodyTextBox.Inlines.Add(new Underline(new Run(part)));
                    else
                        BodyTextBox.Inlines.Add(new Run(part));

                    isUnderline = !isUnderline; // toggle between bold and not bold
                }
            }
        }

        public SkipOutputFileWindow()
        {
            InitializeComponent();
            Owner = new Window();
            WindowName = string.Empty;
            BodyText = string.Empty;
        }

        public SkipOutputFileWindow(Window owner, string title, string body)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            BodyText = body;
        }

        private void BtnRetry_Click(object sender, RoutedEventArgs e)
        {
            BtnRetry.IsEnabled = true;
            Close();
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            BtnSkip.IsChecked = true;
            Close();
        }
    }
}