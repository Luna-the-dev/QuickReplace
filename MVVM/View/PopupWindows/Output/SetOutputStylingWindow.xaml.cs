using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for SetOutputStylingWindow.xaml
    /// </summary>
    public partial class SetOutputStylingWindow : Window
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

        public bool Bold
        {
            get { return boldCheckBox.IsChecked ?? false; }
            set { boldCheckBox.IsChecked = value; }
        }

        public bool Italics
        {
            get { return italicsCheckBox.IsChecked ?? false; }
            set { italicsCheckBox.IsChecked = value; }
        }

        public bool Underline
        {
            get { return underlineCheckBox.IsChecked ?? false; }
            set { underlineCheckBox.IsChecked = value; }
        }

        public bool Strikethrough
        {
            get { return strikethroughCheckBox.IsChecked ?? false; }
            set { strikethroughCheckBox.IsChecked = value; }
        }

        public bool isHighlighted
        {
            get { return ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.IsHighlighted; }
            set { ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.IsHighlighted = value; }
        }

        public bool isTextColored
        {
            get { return ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.IsTextColored; }
            set { ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.IsTextColored = value; }
        }

        public Color HighlightColor
        {
            get { return ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.HighlightColor; }
            set { ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.HighlightColor = value; }
        }

        public Color TextColor
        {
            get { return ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.TextColor; }
            set { ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.TextColor = value; }
        }

        public SetOutputStylingWindow(Window owner, string title, string body)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            BodyText = body;
        }

        private void SetHighlightColor_OnClick(object sender, RoutedEventArgs e)
        {
            var window = GetWindow(sender as DependencyObject);
            string title = "Color Picker";
            Color color = ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.HighlightColor;

            var dialog = new ColorPickerWindow(window, title, color);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                var viewModel = (SetOutputStylingViewModel)DataContext;
                viewModel.OutputFilesStyling.HighlightColor = dialog.SelectedColor;
                viewModel.OutputFilesStyling.IsHighlighted = true;
            }
            else if (dialog.BtnReset.IsChecked == true)
            {
                ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.IsHighlighted = false;
            }
        }

        private void SetTextColor_OnClick(object sender, RoutedEventArgs e)
        {
            var window = GetWindow(sender as DependencyObject);
            string title = "Color Picker";

            var dialog = new ColorPickerWindow(window, title);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                var viewModel = (SetOutputStylingViewModel)DataContext;
                viewModel.OutputFilesStyling.TextColor = dialog.SelectedColor;
                viewModel.OutputFilesStyling.IsTextColored = true;
            }
            else if (dialog.BtnReset.IsChecked == true)
            {
                ((SetOutputStylingViewModel)DataContext).OutputFilesStyling.IsTextColored = false;
            }
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