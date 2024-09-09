using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

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

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                name: "Text",
                propertyType: typeof(string),
                ownerType: typeof(RollingText),
                typeMetadata: new PropertyMetadata(string.Empty));

        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register(
                name: "Duration",
                propertyType: typeof(Duration),
                ownerType: typeof(RollingText),
                typeMetadata: new PropertyMetadata(new Duration(TimeSpan.FromSeconds(8))));

        public TimeSpan EndAnimationTime
        {
            get { return TimeSpan.FromSeconds(2) + Duration.TimeSpan; }
        }

        public RollingText()
        {
            InitializeComponent();
        }

        public int counter = 0;

        private void UpdateAnimation_OnSizeChanged(object sender, RoutedEventArgs e)
        {
            rollingText.RenderTransform = new TranslateTransform
            {
                X = 0,
            };

            var storyboard = CreateAnimation();
            storyboard.Begin();
        }

        private Storyboard CreateAnimation()
        {
            double animationWidth = CalculateAnimationWidth(rootGrid.ActualWidth, rollingText.ActualWidth);

            var sb = new Storyboard();
            sb.RepeatBehavior = RepeatBehavior.Forever;

            var begin = new DoubleAnimation
            {
                From = 0,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(2))
            };
            sb.Children.Add(begin);
            Storyboard.SetTarget(begin, rollingText);
            Storyboard.SetTargetProperty(begin, new PropertyPath("RenderTransform.X"));

            var middle = new DoubleAnimation
            {
                BeginTime = TimeSpan.FromSeconds(2),
                From = 0,
                To = animationWidth,
                Duration = Duration,

            };
            sb.Children.Add(middle);
            Storyboard.SetTarget(middle, rollingText);
            Storyboard.SetTargetProperty(middle, new PropertyPath("RenderTransform.X"));

            var end = new DoubleAnimation
            {
                BeginTime = EndAnimationTime,
                From = animationWidth,
                To = animationWidth,
                Duration = new Duration(TimeSpan.FromSeconds(2))
            };
            sb.Children.Add(end);
            Storyboard.SetTarget(end, rollingText);
            Storyboard.SetTargetProperty(end, new PropertyPath("RenderTransform.X"));

            return sb;
        }

        private static double CalculateAnimationWidth(double gridWidth, double textWidth)
        {
            // if the length of the label is shorter than the parent grid, dont animate
            if (-textWidth + gridWidth > 0)
            {
                return 0;
            }

            // animate to the difference between the width of the parent element and the text
            return -textWidth + gridWidth;
        }
    }
}
