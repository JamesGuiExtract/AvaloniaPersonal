using System.Windows;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    /// The result of a message dialog (which button the user clicked)
    public enum MessageDialogResult
    {
        Yes,
        No
    }

    /// Service to wrap a message dialog. Used to facility view model unit testing
    public interface IMessageDialogService
    {
        /// <summary>
        /// Show a dialog with buttons for yes and no responses.
        /// </summary>
        /// <param name="title">The title for the dialog</param>
        /// <param name="message">The message text</param>
        /// <returns>The result representing the user's choice</returns>
        MessageDialogResult ShowYesNoDialog(string title, string message);
    }

    /// <inheritdoc/>
    public class MessageDialogService : IMessageDialogService
    {
        public MessageDialogResult ShowYesNoDialog(string title, string message)
        {
            return new YesNoDialog(title, message)
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog().GetValueOrDefault()
                ? MessageDialogResult.Yes
                : MessageDialogResult.No;
        }
    }
}
