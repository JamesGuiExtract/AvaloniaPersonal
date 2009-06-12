using System;
using EnvDTE80;
using Extract.VisualStudio.AddIns;
using System.Windows.Forms;
using EnvDTE;

namespace LICode
{
    /// <summary>
    /// Represents a command that inserts a location identifier code.
    /// </summary>
    public class InsertLI : ICommandAction
    {
        #region InsertLI Fields

        readonly DTE2 _dte;
        readonly LIType _liType;

        #endregion InsertLI Fields

        #region InsertLI Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertLI"/> class.
        /// </summary>
        public InsertLI(DTE2 dte, LIType liType)
        {
            _dte = dte;
            _liType = liType;
        }

        #endregion InsertLI Constructors

        #region InsertLI Methods

        void InsertLICode()
        {
            // Get the selected text
            TextSelection selection = (TextSelection)_dte.ActiveDocument.Selection;

            // Instantiate an LICodeHandler to retrieve LI code
            using (LICodeHandler LIHandler = new LICodeHandler())
            {
                // Get the next LI code
                string LICode = (string)(_liType == LIType.Exception ? LIHandler.NextELICode : LIHandler.NextMLICode);

                // Insert the LI code, replacing text if any is selected
                selection.Insert(LICode, (int)vsInsertFlags.vsInsertFlagsContainNewText);

                // Deselect the inserted text and set the active cursor 
                // to the right of the recently inserted text
                selection.CharRight(false, 1);

                // Commit the changes to LI dat file
                LIHandler.CommitChanges();
            }
        }

        #endregion InsertLI Methods

        #region ICommandAction Members

        /// <summary>
        /// Performs the action of the command.
        /// </summary>
        public void Execute()
        {
            try
            {
                InsertLICode();
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
                return _dte.ActiveDocument != null;
            }
        }

        #endregion
    }
}
