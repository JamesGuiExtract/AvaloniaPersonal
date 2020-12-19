using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Extract.FileActionManager.Utilities.FAMServiceManager
{
    /// <summary>
    /// Interaction logic for NamePasswordDialog.xaml
    /// </summary>
    public partial class NamePasswordDialog : UserControl
    {
        public NamePasswordDialog()
        {
            InitializeComponent();
        }

        void SelectAll(object sender, RoutedEventArgs e)
        {
            switch (sender)
            {
                case PasswordBox pb:
                    pb.SelectAll();
                    return;
                case TextBox tb:
                    tb.SelectAll();
                    return;
            }
        }

        void SelectivelyIgnoreMouseButton(object sender,
            MouseButtonEventArgs e)
        {
            if (sender is UIElement el)
            {
                if (!el.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    el.Focus();
                }
            }
        }

        void SetFocusIfVisible(object sender, DependencyPropertyChangedEventArgs _)
        {
            if (sender is UIElement el && el.Visibility == Visibility.Visible)
            {
                Dispatcher.BeginInvoke((Action)(() => Keyboard.Focus(el)), DispatcherPriority.Render);
            }
        }
    }
}
