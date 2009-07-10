using Extract.VisualStudio.AddIns;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SourceControl
{
    /// <summary>
    /// Represents a command that emails a get message.
    /// </summary>
    public class EmailGetMessage : ICommand
    {
        #region EmailGetMessage Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailGetMessage"/> class.
        /// </summary>
        public EmailGetMessage()
        {
        }

        #endregion EmailGetMessage Constructors

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
                return "EmailGetMessage";
            }
        }

        /// <summary>
        /// Performs the action of the settings.
        /// </summary>
        public void Execute()
        {
            try
            {
                // Get the get message
                string message = GetMessage.GetSince(RegistryManager.LastGetMessageTime);

                // If message is empty, open empty email
                string subject = "get";
                if (string.IsNullOrEmpty(message))
                {
                    subject = "";
                    message = "";
                }

                // Send the email
                NativeMethods.SendEmail(subject, message, RegistryManager.GetMessageRecipients);

                // Store the current time as the last get message send time
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
            CommandUISettings settings = new CommandUISettings(Name);
            settings.Bindings = "Global::Shift+Alt+G";

            return settings;
        }

        #endregion ICommand Members
    }
}
