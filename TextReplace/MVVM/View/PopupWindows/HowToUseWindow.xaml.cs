using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using TextReplace.Messages;

namespace TextReplace.MVVM.View.PopupWindows
{
    /// <summary>
    /// Interaction logic for HowToUseWindow.xaml
    /// </summary>
    public partial class HowToUseWindow : Window,
        IRecipient<WindowSizeMsg>,
        IRecipient<WindowLocationMsg>
    {
        public HowToUseWindow()
        {
            InitializeComponent();
        }

        public HowToUseWindow(Window owner, double width, double height)
        {
            InitializeComponent();
            Owner = owner;
            Width = width;
            Height = height;

            Loaded += (s, e) =>
            {
                WeakReferenceMessenger.Default.RegisterAll(this);
            };

            Unloaded += (s, e) =>
            {
                WeakReferenceMessenger.Default.UnregisterAll(this);
            };
        }

        void IRecipient<WindowSizeMsg>.Receive(WindowSizeMsg message)
        {
            Width = message.Value.Item1;
            Height = message.Value.Item2;
        }

        void IRecipient<WindowLocationMsg>.Receive(WindowLocationMsg message)
        {
            Left = message.Value.Item1;
            Top = message.Value.Item2;

        }
    }
}
