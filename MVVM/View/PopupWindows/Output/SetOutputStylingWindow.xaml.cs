using System.Windows;
using System.Windows.Documents;
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

        public bool IsHighlighted
        {
            get { return ((SetOutputStylingViewModel)DataContext).IsHighlighted; }
            set { ((SetOutputStylingViewModel)DataContext).IsHighlighted = value; }
        }

        public bool IsTextColored
        {
            get { return ((SetOutputStylingViewModel)DataContext).IsTextColored; }
            set { ((SetOutputStylingViewModel)DataContext).IsTextColored = value; }
        }

        public Color HighlightColor
        {
            get { return ((SetOutputStylingViewModel)DataContext).HighlightColor; }
            set { ((SetOutputStylingViewModel)DataContext).HighlightColor = value; }
        }

        public Color TextColor
        {
            get { return ((SetOutputStylingViewModel)DataContext).TextColor; }
            set { ((SetOutputStylingViewModel)DataContext).TextColor = value; }
        }

        public SetOutputStylingWindow()
        {
            InitializeComponent();
            Owner = new Window();
            WindowName = string.Empty;
            BodyText = string.Empty;
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
            var viewModel = (SetOutputStylingViewModel)DataContext;

            var window = GetWindow(sender as DependencyObject);
            string title = "Color Picker";
            Color color = viewModel.HighlightColor;
            color.A = 0xFF;

            var dialog = new ColorPickerWindow(window, title, color);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                viewModel.HighlightColor = dialog.SelectedColor;
                viewModel.IsHighlighted = true;
            }
            else if (dialog.BtnReset.IsChecked == true)
            {
                viewModel.HighlightColor = new Color();
                viewModel.IsHighlighted = false;
            }
        }

        private void SetTextColor_OnClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (SetOutputStylingViewModel)DataContext;

            var window = GetWindow(sender as DependencyObject);
            string title = "Color Picker";
            Color color = viewModel.TextColor;
            color.A = 0xFF;

            var dialog = new ColorPickerWindow(window, title, color);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                viewModel.TextColor = dialog.SelectedColor;
                viewModel.IsTextColored = true;
            }
            else if (dialog.BtnReset.IsChecked == true)
            {
                viewModel.TextColor = new Color();
                viewModel.IsTextColored = false;
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