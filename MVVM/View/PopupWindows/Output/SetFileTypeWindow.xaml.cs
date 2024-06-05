using System.Windows;
using System.Windows.Documents;
using TextReplace.Core.Enums;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for SetOutputFileTypeWindow.xaml
    /// </summary>
    public partial class SetOutputFileTypeWindow : Window
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

        public OutputFileTypeEnum OutputFileType
        {
            get
            {
                int fileType = ((SetOutputFileTypeViewModel)DataContext).OutputFileType;
                return (OutputFileTypeEnum)fileType;
            }
            set
            {
                ((SetOutputFileTypeViewModel)DataContext).OutputFileType = (int)value;
            }
        }

        public SetOutputFileTypeWindow(Window owner, string title, string body)
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