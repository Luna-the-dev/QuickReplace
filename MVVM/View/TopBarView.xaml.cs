﻿using System.Windows;
using System.Windows.Controls;
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

        private void OpenDelimiterInputWindow(object sender, RoutedEventArgs e)
        {
            // Yes, i know it is bad to call the view model directly from the code behind.
            // The only way to avoid this is by setting the delimiter in the InputWindow view model,
            // which would 1. make the InputWindow non-independant from this view, and
            // 2. make two view models talk to eachother. I could also do this in the TopBarViewModel
            // class, but then that view model would be talking with multiple views. I'd rather just
            // do a teeny tiny little MVVM violation by referencing it here.
            var viewModel = ((TopBarViewModel)(this.DataContext));

            var window = Window.GetWindow(sender as DependencyObject);
            string title = (sender as Button)?.Content.ToString() ?? string.Empty;
            string body;
            if (viewModel.Delimiter != string.Empty)
            {
                body = $"<u>Current delimiter:</u> {viewModel.Delimiter}";
            }
            else
            {
                body = "Enter the delimiter used to seperate the original and replacement words.\n" +
                "<u>Note:</u> This defaults to a comma for .csv files and a tab character for .tsv files.";
            }
            string defaultInputTest = "Ex. :, -, or ;";

            var dialog = new InputWindow(window, title, body, defaultInputTest);
            dialog.ShowDialog();

            // if the cancel button was checked and is non-null
            if (dialog.BtnCancel.IsChecked == false)
            {
                viewModel.SetDelimiter(dialog.InputText);
            }
        }
    }
}
