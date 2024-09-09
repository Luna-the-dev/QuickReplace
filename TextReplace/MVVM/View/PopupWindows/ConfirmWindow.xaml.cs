using System.Windows;
using System.Windows.Documents;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for ConfirmWindow.xaml
    /// </summary>
    public partial class ConfirmWindow : Window
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

        public ConfirmWindow()
        {
            InitializeComponent();
            Owner = new Window();
            WindowName = string.Empty;
            BodyText = string.Empty;
        }

        public ConfirmWindow(Window owner, string title, string body)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            BodyText = body;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsChecked = true;
            Close();
        }
    }
}