using System;
using System.Windows;

namespace Extract.Utilities.WPF
{
    /// <summary>
    /// The result of a message dialog (which button the user clicked)
    /// </summary>
    public enum MessageDialogResult
    {
        Yes,
        No
    }

    /// <summary>
    /// Service to wrap a message dialog. Used to facility view model unit testing
    /// </summary>
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
        public MessageDialogService(Action<Window> setOwner)
        {
            SetOwner = setOwner;
        }

        public Action<Window> SetOwner { get; }

        public MessageDialogResult ShowYesNoDialog(string title, string message)
        {
            Window thisWindow = new YesNoDialog(title, message);
            SetOwner(thisWindow);

            return thisWindow.ShowDialog().GetValueOrDefault()
                ? MessageDialogResult.Yes
                : MessageDialogResult.No;
        }
    }
}
