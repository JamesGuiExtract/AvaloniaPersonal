using System.Windows;
using System.Windows.Controls;

namespace Extract.Utilities.WPF
{
    /// <summary>
    /// Attached behavior to allow setting focus to a specific control when a button is clicked
    /// </summary>
    public static class EventFocusAttachment
    {
        public static Control? GetElementToFocus(Button button)
        {
            return button?.GetValue(ElementToFocusProperty) as Control;
        }

        public static void SetElementToFocus(Button button, Control value)
        {
            button?.SetValue(ElementToFocusProperty, value);
        }

        public static readonly DependencyProperty ElementToFocusProperty =
            DependencyProperty.RegisterAttached("ElementToFocus", typeof(Control), 
            typeof(EventFocusAttachment), new UIPropertyMetadata(null, ElementToFocusPropertyChanged));

        public static void ElementToFocusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            button.Click += (s, args) =>
                button.Dispatcher.BeginInvoke(() => GetElementToFocus(button)?.Focus());
        }
    }
}
