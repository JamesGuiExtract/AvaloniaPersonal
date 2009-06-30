using Extract.VisualStudio.AddIns;
using Extract.SourceControl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace LICode
{
    /// <summary>
    /// Class for refreshing the source control connection
    /// </summary>
    public class RefreshSourceConnection : ICommandAction
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshSourceConnection"/> class.
        /// </summary>
        public RefreshSourceConnection()
        {
        }

        #endregion Constructor

        #region ICommandAction Members

        /// <summary>
        /// Causes the source control connection to be refreshed
        /// </summary>
        public void Execute()
        {
            try
            {
                // Get the source control database
                ISourceControl sourceDB = SourceControlMethods.OpenSourceControlDatabase();

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

        #endregion
    }
}
