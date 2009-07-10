using EnvDTE;
using EnvDTE80;
using Extract.VisualStudio.AddIns;
using System;
using System.Windows.Forms;

namespace LICode
{
    /// <summary>
    /// Represents a command that inserts a location identifier code.
    /// </summary>
    public class InsertLI : ICommand
    {
        #region InsertLI Fields

        readonly DTE2 _dte;
        readonly LIType _liType;
        readonly string _name;

        #endregion InsertLI Fields

        #region InsertLI Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertLI"/> class.
        /// </summary>
        public InsertLI(DTE2 dte, LIType liType)
        {
            _dte = dte;
            _liType = liType;
            _name = "Insert" + GetPrefix(liType);
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

        /// <summary>
        /// Returns the prefix associated with the location identifier type.
        /// </summary>
        /// <param name="liType">The prefix associated with the location identifier type.
        /// </param>
        /// <returns></returns>
        static string GetPrefix(LIType liType)
        {
            return liType == LIType.Method ? "MLI" : "ELI";
        }

        #endregion InsertLI Methods

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
                return _name;
            }
        }

        /// <summary>
        /// Performs the action of the settings.
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

        /// <summary>
        /// Retrieves the user interface settings for the command.
        /// </summary>
        /// <returns>The user interface settings for the command.</returns>
        public CommandUISettings GetUISettings()
        {
            string shortcutKey = _liType == LIType.Exception ? "E" : "M";

            CommandUISettings settings = new CommandUISettings(Name);

            settings.IsOnCodeWindowMenu = true;
            settings.ToolTip = Name;
            settings.Bindings = "Text Editor::Shift+Alt+" + shortcutKey;

            return settings;
        }

        #endregion
    }
}
