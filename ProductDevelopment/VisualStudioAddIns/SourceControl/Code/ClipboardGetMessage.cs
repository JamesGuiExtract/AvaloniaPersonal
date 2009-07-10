using EnvDTE80;
using Extract.VisualStudio.AddIns;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SourceControl
{
    /// <summary>
    /// Represents a command that adds the get message to the clipboard.
    /// </summary>
    public class ClipboardGetMessage : ICommand
    {
        #region ClipboardGetMessage Fields

        readonly DTE2 _dte;

        #endregion ClipboardGetMessage Fields

        #region ClipboardGetMessage Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClipboardGetMessage"/> class.
        /// </summary>
        public ClipboardGetMessage(DTE2 dte)
        {
            _dte = dte;
        }

        #endregion ClipboardGetMessage Constructors

        #region ICommand Members

        /// <summary>
        /// Gets the name of the command prefixed with one or more categories separated by periods.
        /// </summary>
        /// <returns>The name of the command prefixed with one or more categories separated by 
        /// periods.
        /// </returns>
        public string Name
        {
            get
            {
                return "ClipboardGetMessage";
            }
        }

        /// <summary>
        /// Performs the action of the settings.
        /// </summary>
        public void Execute()
        {
            try
            {
                string message = GetMessage.GetSince(RegistryManager.LastGetMessageTime);

                // If message is empty, display message box
                if (string.IsNullOrEmpty(message))
                {
                    MessageBox.Show("No new changes since last message.", "No new get message", 
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    return;
                }

                Clipboard.SetText(message);

                // Update status bar
                _dte.StatusBar.Text = "Get message stored in clipboard";

                RegistryManager.LastGetMessageTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK,
                    MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
            }
        }

        /// <summary>
        /// Gets or sets whether the command is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the command is able to be executed;
        /// <see langword="false"/> if the command is not able to be executed.</value>
        /// <returns><see langword="true"/> if the command is able to be executed;
        /// <see langword="false"/> if the command is not able to be executed.</returns>
        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Retrieves the user interface settings for the command.
        /// </summary>
        /// <returns>The user interface settings for the command.</returns>
        public CommandUISettings GetUISettings()
        {
            return new CommandUISettings(Name);
        }

        #endregion ICommand Members
    }
}
