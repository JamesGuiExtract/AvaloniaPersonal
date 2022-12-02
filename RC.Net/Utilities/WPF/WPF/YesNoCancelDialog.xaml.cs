using System.Windows;

namespace Extract.Utilities.WPF
{
    /// <summary>
    /// Interaction logic for YesNoCancelDialog.xaml
    /// </summary>
    public partial class YesNoCancelDialog : Window
    {
        public MessageDialogResult MessageDialogResult { get; set; }

        public YesNoCancelDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            textBlock.Text = message;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            MessageDialogResult = MessageDialogResult.Yes;
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            MessageDialogResult = MessageDialogResult.No;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            MessageDialogResult = MessageDialogResult.Cancel;
        }
    }
}
