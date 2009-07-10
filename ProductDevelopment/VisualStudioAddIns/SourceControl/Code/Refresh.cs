extern alias SC;

using EnvDTE;
using EnvDTE80;
using Extract.VisualStudio.AddIns;
using SC::Extract.SourceControl;
using System;
using System.Windows.Forms;

namespace SourceControl
{
    /// <summary>
    /// Represents a command that refreshes the source control connection.
    /// </summary>
    public class Refresh : ICommand
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Refresh"/> class.
        /// </summary>
        public Refresh()
        {
        }

        #endregion Constructor

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
                return "Refresh";
            }
        }

        /// <summary>
        /// Causes the source control connection to be refreshed
        /// </summary>
        public void Execute()
        {
            try
            {
                // Get the source control database
                ISourceControl sourceDB = SourceControlFactory.Create(new LogOnSettings());

                // Refresh the database connection
                sourceDB.RefreshConnection();
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
            settings.Bindings = "Global::Ctrl+K,Ctrl+Shift+R";
            settings.ToolTip = "Refresh Source Control Connection";

            return settings;
        }

        #endregion ICommand Members
    }
}
