using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Provides data for the <see cref="BrowseButton.PathSelected"/> event.
    /// </summary>
    public class PathSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// The path that was selected.
        /// </summary>
        readonly string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSelectedEventArgs"/> class.
        /// </summary>
        /// <param name="path">The path that was selected.</param>
        public PathSelectedEventArgs(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Gets the path that was selected.
        /// </summary>
        /// <returns>The path that was selected.</returns>
        public string Path
        {
            get
            {
                return _path;
            }
        }
    }

    /// <summary>
    /// Represents a button that displays either a <see cref="FolderBrowserDialog"/>
    /// or <see cref="OpenFileDialog"/> allowing the user to select a path. Optionally
    /// will set the value of a <see cref="TextBox"/> control with the selected data.
    /// </summary>
    [DefaultEvent("PathSelected")]
    [DefaultProperty("FolderBrowser")]
    public partial class BrowseButton : Button
    {
        #region Constants

        /// <summary>
        /// The default text for this button
        /// </summary>
        static readonly string _BUTTON_TEXT = "...";

        /// <summary>
        /// The default text to display when selecting a folder.
        /// </summary>
        const string _DEFAULT_FOLDER_DESCRIPTION = "Please select a folder";

        #endregion Constants

        #region Fields

        /// <summary>
        /// If <see langword="true"/> then the <see cref="Control.Click"/> event will display
        /// a <see cref="FolderBrowserDialog"/>, if <see langword="false"/>
        /// an <see cref="OpenFileDialog"/> will be displayed.
        /// </summary>
        bool _folderBrowser;

        /// <summary>
        /// The last path that was selected in a dialog (also used to set the initial folder
        /// for the dialogs).
        /// </summary>
        string _fileOrFolderPath;

        /// <summary>
        /// The description to display when selecting a folder.
        /// </summary>
        string _folderDescription = _DEFAULT_FOLDER_DESCRIPTION;

        /// <summary>
        /// The file filter that will be used in the <see cref="OpenFileDialog"/>. (Only applies
        /// if <see cref="FolderBrowser"/> is <see langword="false"/>.
        /// </summary>
        string _fileFilter;

        /// <summary>
        /// The default filter that will be displayed in the <see cref="OpenFileDialog"/>. (Only
        /// applies if <see cref="FolderBrowser"/> is <see langword="false"/> and
        /// <see cref="FileFilter"/> is not <see langword="null"/> or empty string.
        /// </summary>
        int _defaultFilterIndex;

        /// <summary>
        /// The <see cref="TextBox"/> that will be updated by the dialog result if the user
        /// selected OK.
        /// </summary>
        TextBox _textControl;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when a path is selected.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a path is selected.")]
        public event EventHandler<PathSelectedEventArgs> PathSelected;

        #endregion Events

        #region Constructors

        /// <overload>
        /// Initializes a new instance of the <see cref="BrowseButton"/> class.
        /// </overload>
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowseButton"/> class.
        /// Defaults to a <see cref="FolderBrowserDialog"/>.
        /// </summary>
        public BrowseButton()
            : this(false, null, null, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowseButton"/> class.
        /// </summary>
        /// <param name="folderBrowser">If <see langword="true"/> then will display
        /// a <see cref="FolderBrowserDialog"/>, if <see langword="false"/>
        /// will display an <see cref="OpenFileDialog"/>.</param>
        /// <param name="textControl">The <see cref="TextBox"/> that this browse dialog
        /// is associated with. May be <see langword="null"/>.  If a <see cref="TextBox"/>
        /// is specified, the text in the box will be overwritten with the result of
        /// the dialog display when <see cref="DialogResult"/> is OK.</param>
        /// <param name="fileFilter">A file filter string for use in the
        /// <see cref="OpenFileDialog"/>. May be <see langword="null"/>.</param>
        /// <param name="defaultFilterIndex">The default filter to display in the
        /// <see cref="OpenFileDialog"/>. Only applies if <paramref name="fileFilter"/>
        /// is specified.</param>
        public BrowseButton(bool folderBrowser, TextBox textControl,
            string fileFilter, int defaultFilterIndex)
        {
            InitializeComponent();

            _folderBrowser = folderBrowser;

            _textControl = textControl;

            _fileFilter = fileFilter;

            _defaultFilterIndex = defaultFilterIndex;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets whether to display a <see cref="FolderBrowserDialog"/> or an
        /// <see cref="OpenFileDialog"/>.
        /// </summary>
        /// <returns>Whether to display a <see cref="FolderBrowserDialog"/> or an
        /// <see cref="OpenFileDialog"/>.</returns>
        /// <value>Whether to display a <see cref="FolderBrowserDialog"/> or an
        /// <see cref="OpenFileDialog"/>.</value>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Whether a folder browser or a file browser should be displayed.")]
        public bool FolderBrowser
        {
            get
            {
                return _folderBrowser;
            }
            set
            {
                _folderBrowser = value;
            }
        }

        /// <summary>
        /// Gets or sets the description to display when selecting a folder.
        /// </summary>
        /// <value>The description to display when selecting a folder.</value>
        /// <returns>The description to display when selecting a folder.</returns>
        [Category("Behavior")]
        [Description("The description displayed when selecting a folder. Only used if FolderBrowser is true.")]
        public string FolderDescription
        {
            get
            {
                return _folderDescription;
            }
            set
            {
                _folderDescription = value;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="FolderDescription"/> property should be serialized.
        /// </summary>
        /// <returns><see langword="true"/> if the property should be serialized; 
        /// <see langword="false"/> if the property should not be serialized.</returns>
        bool ShouldSerializeFolderDescription()
        {
            return _folderBrowser && _folderDescription != _DEFAULT_FOLDER_DESCRIPTION;
        }

        /// <summary>
        /// Resets the <see cref="FolderDescription"/> property to its default value.
        /// </summary>
        void ResetFolderDescription()
        {
            _folderDescription = _DEFAULT_FOLDER_DESCRIPTION;
        }

        /// <summary>
        /// Gets/sets the file filter text for the <see cref="OpenFileDialog"/>.
        /// </summary>
        /// <returns>The file filter text for the <see cref="OpenFileDialog"/>.</returns>
        /// <value>The file filter text for the <see cref="OpenFileDialog"/>.</value>
        [Category("Behavior")]
        [Description("The file filter text. Only used if FolderBrowser is false.")]
        public string FileFilter
        {
            get
            {
                return _fileFilter;
            }
            set
            {
                _fileFilter = value;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="FileFilter"/> property should be serialized.
        /// </summary>
        /// <returns><see langword="true"/> if the property should be serialized; 
        /// <see langword="false"/> if the property should not be serialized.</returns>
        bool ShouldSerializeFileFilter()
        {
            return !_folderBrowser && _fileFilter != null;
        }

        /// <summary>
        /// Resets the <see cref="FileFilter"/> property to its default value.
        /// </summary>
        void ResetFileFilter()
        {
            _fileFilter = null;
        }

        /// <summary>
        /// Gets/sets the default filter index for the <see cref="OpenFileDialog"/>.
        /// </summary>
        /// <returns>The default file filter index for the <see cref="OpenFileDialog"/>.</returns>
        /// <value>The default file filter index for the <see cref="OpenFileDialog"/>.</value>
        [Category("Behavior")]
        [Description("The default filter index. Only used if FolderBrowser is false.")]
        public int DefaultFilterIndex
        {
            get
            {
                return _defaultFilterIndex;
            }
            set
            {
                _defaultFilterIndex = value;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="DefaultFilterIndex"/> property should be serialized.
        /// </summary>
        /// <returns><see langword="true"/> if the property should be serialized; 
        /// <see langword="false"/> if the property should not be serialized.</returns>
        bool ShouldSerializeDefaultFilterIndex()
        {
            return !_folderBrowser && _defaultFilterIndex != -1;
        }

        /// <summary>
        /// Resets the <see cref="DefaultFilterIndex"/> property to its default value.
        /// </summary>
        void ResetDefaultFilterIndex()
        {
            _defaultFilterIndex = -1;
        }

        /// <summary>
        /// Gets/sets the file or folder path that was returned from the dialog (also used
        /// to determine the initial folder displayed in the dialog).
        /// </summary>
        /// <returns>
        /// The file or folder path that was returned from the dialog (also used
        /// to determine the initial folder displayed in the dialog).
        /// </returns>
        /// <value>
        /// The file or folder path that was returned from the dialog (also used
        /// to determine the initial folder displayed in the dialog).
        /// </value>
        [Category("Behavior")]
        [DefaultValue(null)]
        [Description("Initial file or folder to display in dialog.")]
        public string FileOrFolderPath
        {
            get
            {
                return _fileOrFolderPath;
            }
            set
            {
                _fileOrFolderPath = value;
            }
        }

        /// <summary>
        /// Gets/sets the text control associated with this <see cref="BrowseButton"/>.
        /// </summary>
        /// <returns>The text control associated with this <see cref="BrowseButton"/>.</returns>
        /// <value>The text control associated with this <see cref="BrowseButton"/>.</value>
        [DefaultValue(null)]
        [Description("The text control to automatically update when a path is selected.")]
        public TextBox TextControl
        {
            get
            {
                return _textControl;
            }
            set
            {
                _textControl = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClick(EventArgs e)
        {
            // Call the base class so that registered listeners receive the event
            base.OnClick(e);

            try
            {
                // Determine the initial folder for the dialogs
                string initialFolder = string.IsNullOrEmpty(_fileOrFolderPath) ?
                    null : Path.GetFullPath(_fileOrFolderPath);

                // Build and display the desired dialog
                DialogResult result = _folderBrowser ? 
                    ShowFolderBrowser(initialFolder) : ShowFileDialog(initialFolder);

                // Check the dialog result
                if (result == DialogResult.OK)
                {
                    // If the result was OK and there is an associated text control, set its value
                    if (_textControl != null)
                    {
                        // Set the associated text control
                        _textControl.Text = _fileOrFolderPath ?? "";
                    }

                    // Raise the path selected event
                    OnPathSelected(new PathSelectedEventArgs(_fileOrFolderPath));
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26228", ex);
            }
        }

        /// <summary>
        /// Allows the user to select a folder using the folder browser.
        /// </summary>
        /// <param name="initialFolder">The initial folder for the folder browser.</param>
        /// <returns>The result of the user's selection.</returns>
        DialogResult ShowFolderBrowser(string initialFolder)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                // Set the initial folder if necessary
                if (!string.IsNullOrEmpty(initialFolder))
                {
                    folderBrowser.SelectedPath = initialFolder;
                }

                // Set the folder browser description
                folderBrowser.Description = _folderDescription;

                // Show the dialog
                DialogResult result = folderBrowser.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Store the value returned on OK
                    _fileOrFolderPath = folderBrowser.SelectedPath;
                }

                return result;
            }
        }

        /// <summary>
        /// Allows the user to select a file using the file dialog.
        /// </summary>
        /// <param name="initialFolder">The initial folder for the file browser.</param>
        /// <returns>The result of the user's selection.</returns>
        DialogResult ShowFileDialog(string initialFolder)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                // Set the initial folder if necessary
                if (!string.IsNullOrEmpty(initialFolder))
                {
                    openFile.InitialDirectory = initialFolder;
                }

                // Set the filter text and the initial filter
                if (!string.IsNullOrEmpty(_fileFilter))
                {
                    openFile.Filter = _fileFilter;
                    openFile.FilterIndex =
                        _defaultFilterIndex >= 0 ? _defaultFilterIndex : 0;
                    openFile.AddExtension = true;
                }
                else
                {
                    openFile.AddExtension = false;
                }

                // Set multi-select to false
                openFile.Multiselect = false;

                // Require that both the path and file exist
                openFile.CheckFileExists = true;
                openFile.CheckPathExists = true;

                // Show the dialog
                DialogResult result = openFile.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Store the value returned on OK
                    _fileOrFolderPath = openFile.FileName;
                }

                return result;
            }
        }

        /// <summary>
        /// Raises the <see cref="PathSelected"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected virtual void OnPathSelected(PathSelectedEventArgs e)
        {
            try
            {
                if (PathSelected != null)
                {
                    PathSelected(this, e);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26229", ex);
            }
        }

        #endregion Event Handlers
    }
}
