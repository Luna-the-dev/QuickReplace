using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using TextReplace.Core.Validation;
using TextReplace.MVVM.ViewModel.PopupWindows;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for UploadReplacementsInputWindow.xaml
    /// </summary>
    public partial class SetOutputDirectoryWindow : Window
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
        public string DefaultBodyText { get; set; } = string.Empty;

        public string DirectoryName
        {
            get { return DirectoryNameTextBox.Text.ToString() ?? string.Empty; }
            set { DirectoryNameTextBox.Text = value; }
        }


        public SetOutputDirectoryWindow(Window owner, string title, string body)
        {
            InitializeComponent();
            Owner = owner;
            WindowName = title;
            DefaultBodyText = body;
            BodyText = DefaultBodyText;
        }

        private void BtnDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            // configure open file dialog box
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = "Select Folder",
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                Debug.WriteLine("Change default file path window was closed");
                return;
            }

            ((SetOutputDirectoryViewModel)DataContext).ValidateOutputDirectory(dialog.FileName);
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            BtnOk.IsChecked = true;
            Close();
        }

        private void BtnDefault_OnClick(object sender, RoutedEventArgs e)
        {
            BtnDefault.IsChecked = true;
            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsChecked = true;
            Close();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //Calculate half of the offset to move the form
            if (sizeInfo.HeightChanged)
                Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;

            if (sizeInfo.WidthChanged)
                Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
        }
    }
}