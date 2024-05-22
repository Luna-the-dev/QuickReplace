﻿using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TextReplace.Core.Validation;
using TextReplace.MVVM.ViewModel;

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

        private void UploadReplaceFile_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            string title = "Upload";
            string body = "Upload a file for the replacement phrases";

            var dialog = new PopupWindows.UploadReplacementsInputWindow(window, title, body);

            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true)
            {
                if (DataValidation.IsTextFile(dialog.FullFileName))
                {
                    ((TopBarViewModel)DataContext).SetNewReplaceFile(dialog.FullFileName, dialog.DelimiterInputText);
                }
                else
                {
                    ((TopBarViewModel)DataContext).SetNewReplaceFile(dialog.FullFileName);
                }
            }
        }

        private void UploadSourceFile_OnClick(object sender, RoutedEventArgs e)
        {
            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Text Files",
                FileName = "Document", // Default file name
                DefaultExt = ".txt", // Default file extension
                Filter = "Text documents (.txt)|*.txt", // Filter files by extension
                Multiselect = true
            };

            // open file dialog box
            if (dialog.ShowDialog() != true)
            {
                Debug.WriteLine("Replace file upload window was closed.");
                return;
            }

            ((TopBarViewModel)DataContext).SourceFiles(dialog.FileNames);
        }

        private void OpenFileSuffixInputWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = (TopBarViewModel)(DataContext);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var window = Window.GetWindow(sender as DependencyObject);
            string title = textInfo.ToTitleCase(suffixMenuOption.Text);
            string body;
            if (viewModel.Suffix != string.Empty)
            {
                body = $"<u>Current suffix:</u> {viewModel.Suffix}";
            }
            else
            {
                body = "Enter suffix which will be appended onto the output file names.\n" +
                "<u>Note:</u> This defaults to \"-replacify\"";
            }
            string defaultInputTest = "-replacify";

            var dialog = new PopupWindows.InputResetWindow(window, title, body, defaultInputTest);
            dialog.ShowDialog();

            if (dialog.BtnOk.IsChecked == true || dialog.BtnReset.IsChecked == true)
            {
                viewModel.SetSuffix(dialog.InputText);
            }
        }
    }
}
