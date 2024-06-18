using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WinInterop = System.Windows.Interop;

namespace TextReplace.MVVM.View
{
    /// <summary>
    /// Interaction logic for TopBorderView.xaml
    /// </summary>
    public partial class TopBorderView : UserControl
    {
        public string? WindowName
        {
            get { return (string)GetValue(WindowNameProperty); }
            set { SetValue(WindowNameProperty, value); }
        }
        public static readonly DependencyProperty WindowNameProperty =
            DependencyProperty.Register(
                name: "WindowName",
                propertyType: typeof(string),
                ownerType: typeof(TopBorderView),
                typeMetadata: new FrameworkPropertyMetadata(null));

        public Visibility MinimizeButtonVisibility
        {
            get { return (Visibility)GetValue(MinimizeButtonProperty); }
            set { SetValue(MinimizeButtonProperty, Visibility); }
        }
        public static readonly DependencyProperty MinimizeButtonProperty =
            DependencyProperty.Register(
                name: "MinimizeButton",
                propertyType: typeof(Visibility),
                ownerType: typeof(TopBorderView),
                typeMetadata: new PropertyMetadata(Visibility.Collapsed));

        public Visibility MaximizeButtonVisibility
        {
            get { return (Visibility)GetValue(MaximizeButtonProperty); }
            set { SetValue(MaximizeButtonProperty, Visibility); }
        }
        public static readonly DependencyProperty MaximizeButtonProperty =
            DependencyProperty.Register(
                name: "MaximizeButton",
                propertyType: typeof(Visibility),
                ownerType: typeof(TopBorderView),
                typeMetadata: new PropertyMetadata(Visibility.Collapsed));

        public string? TitleText
        {
            get
            {
                if (Title.Content == null)
                {
                    return string.Empty;
                }
                return Title.Content.ToString();
            }
            set { Title.Content = value; }
        }

        public TopBorderView()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                TitleText = WindowName;
            };

            // the try catch block prevents an "object reference not set to an instance of an object"
            // exception from being thrown at design time for this user control's caller
            try
            {
                Application.Current.MainWindow.SourceInitialized += new EventHandler(Win_SourceInitialized);
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            catch { }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window.GetWindow(sender as DependencyObject).DragMove();
            }
        }
        
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(sender as DependencyObject).Close();
        }

        #region Avoid hiding task bar upon maximalisation

        private static nint WindowProc(
              nint hwnd,
              int msg,
              nint wParam,
              nint lParam,
              ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (nint)0;
        }

        private static void Win_SourceInitialized(object? sender, EventArgs e)
        {
            nint handle = (new WinInterop.WindowInteropHelper(Application.Current.MainWindow)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }

        private static void WmGetMinMaxInfo(nint hwnd, nint lParam)
        {
            var mmiStructure = Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            if (mmiStructure == null)
            {
                return;
            }
            MINMAXINFO mmi = (MINMAXINFO)mmiStructure;

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            nint monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != nint.Zero)
            {

                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        /// <summary>
        /// Construct a point of coordinates (x,y).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT(int x, int y)
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x = x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y = y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        void Win_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>            
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;
            /// <summary> Win32 </summary>
            public int top;
            /// <summary> Win32 </summary>
            public int right;
            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public readonly int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            /// <summary> Win32 </summary>
            public readonly int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public readonly bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            /// <summary> Return a user friendly representation of this struct </summary>
            public override readonly string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override readonly bool Equals(object? obj)
            {
                if (obj is not Rect) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override readonly int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }


        }

#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        #endregion
    }
}
