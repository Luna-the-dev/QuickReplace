using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TextReplace.UserControls
{
    /// <summary>
    /// Interaction logic for MarqueeTextUserControl.xaml
    /// </summary>
    public partial class RollingText : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TextProperty.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                name: "Text",
                propertyType: typeof(string),
                ownerType: typeof(RollingText),
                typeMetadata: new PropertyMetadata(string.Empty));

        public RollingText()
        {
            InitializeComponent();
        }
    }

    public class NegatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is Grid par && value is double val)
            {
                // if the length of the label is shorter than the parent grid, dont animate
                if (-val + (double)par.ActualWidth > 0)
                {
                    return 0;
                }

                // animate to the difference between the width of the parent element
                // and the text. the +5 is to compensate for the whitespace after the text
                // within its element.
                return -val + (double)par.ActualWidth + 5;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack not implemented.");
        }
    }
}
