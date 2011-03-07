using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripButton"/> that allows the user to modify the tags for a 
    /// file.
    /// </summary>
    [DefaultEvent("TagSelected")]
    public partial class TagFileToolStripButton : ToolStripButtonBase
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(TagFileToolStripButton).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The context menu that is displayed when the button is clicked.
        /// </summary>
        FileTagDropDown _dropDown;

        /// <summary>
        /// The file id of the tags to display.
        /// </summary>
        int _fileId;

        /// <summary>
        /// The database from which to read and write tags.
        /// </summary>
        IFileProcessingDB _database;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="TagFileToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public TagFileToolStripButton()
            : base(typeof(TagFileToolStripButton), "Resources.TagButton.png")
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28712",
                    _OBJECT_NAME);

                InitializeComponent();

                base.Enabled = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28746", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the file processing id of the tags to modify.
        /// </summary>
        /// <value>The file processing id of the tags to modify.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int FileId
        {
            get
            {
                return _fileId;
            }
            set
            {
                _fileId = value;
            }
        }

        /// <summary>
        /// Gets or sets the file processing database used to read the tags.
        /// </summary>
        /// <value>The file processing database used to read the tags.</value>
        [Browsable(false)]
        [CLSCompliant(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFileProcessingDB Database
        {
            get
            {
                return _database;
            }
            set
            {
                try
                {
                    if (_database != value)
                    {
                        _database = value;

                        Enabled = _database != null;
                    }
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI28730",
                        "Unable to set database.", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parent control of the 
        /// <see cref="TagFileToolStripButton"/> is enabled. 
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the parent control of the 
        /// <see cref="TagFileToolStripButton"/> is enabled; otherwise, <see langword="false"/>. 
        /// </returns>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                try
                {

                    if (value && _database == null)
                    {
                        throw new ExtractException("ELI28742",
                            "Cannot enable tag file button without database.");
                    }

                    base.Enabled = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28743", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the file tags that are applied to the specified <paramref name="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file processing ID from which to obtain file tags.</param>
        /// <returns>The file tags that are applied to <paramref name="fileId"/>.</returns>
        string[] GetTagsForFileId(int fileId)
        {
            VariantVector vector = _database.GetTagsOnFile(fileId);

            string[] tags = GetVectorAsArray(vector);
            
            return tags;
        }

        /// <summary>
        /// Gets the file tags associated with the current file id.
        /// </summary>
        /// <returns>The file tags associated with the current file id.</returns>
        FileTag[] GetFileTags()
        {
            // If there is no database, return null
            if (_database == null)
            {
                return null;
            }

            // Get the file tags from the database
            StrToStrMap nameToDescription = _database.GetTags();
            FileTag[] tags = GetMapAsFileTagArray(nameToDescription);

            // Sort the tags alphabetically
            Array.Sort(tags, new FileTagComparer());

            return tags;
        }

        /// <summary>
        /// Creates an array of file tags from the specified map of tag names to tag descriptions.
        /// </summary>
        /// <param name="nameToDescription">A map of file tag names to file tag descriptions.</param>
        /// <returns>An array of file tags from the specified <paramref name="nameToDescription"/> 
        /// map.</returns>
        static FileTag[] GetMapAsFileTagArray(IStrToStrMap nameToDescription)
        {
            // Construct the array to hold the result
            int count = nameToDescription.Size;
            FileTag[] tags = new FileTag[count];

            // Iterate through each item in the map
            for (int i = 0; i < count; i++)
            {
                string name;
                string description;
                nameToDescription.GetKeyValue(i, out name, out description);

                tags[i] = new FileTag(name, description);
            }

            return tags;
        }

        /// <summary>
        /// Creates an array of strings from the specified variant vector.
        /// </summary>
        /// <param name="vector">The variant vector from which to create an array.</param>
        /// <returns>An array of strings from <paramref name="vector"/>.</returns>
        static string[] GetVectorAsArray(IVariantVector vector)
        {
            // Return null if the vector is null
            if (vector == null)
            {
                return null;
            }

            // Cast each element of the variant vector to a string
            int count = vector.Size;
            string[] tags = new string[count];
            for (int i = 0; i < count; i++)
            {
                tags[i] = (string)vector[i];
            }

            return tags;
        }

        /// <summary>
        /// Creates a drop down context menu strip to display to the user.
        /// </summary>
        /// <returns>A drop down context menu strip to display to the user.</returns>
        FileTagDropDown CreateDropDown()
        {
            FileTagDropDown dropDown = null;
            try
            {
                // Get the parameters
                FileTag[] tags = GetFileTags();
                string[] checkedTags = GetTagsForFileId(_fileId);
                bool applyNewTags = _database.AllowDynamicTagCreation();

                // Create the drop down
                dropDown = new FileTagDropDown(tags, checkedTags, applyNewTags);

                // Add event handlers
                dropDown.FileTagAdded += HandleFileTagAdded;
                dropDown.FileTagClicked += HandleFileTagClicked;

                return dropDown;
            }
            catch (Exception)
            {
                if (dropDown != null)
                {
                    dropDown.Dispose();
                }

                throw;
            }
        }


        /// <summary>
        /// Displays the drop down context menu.
        /// </summary>
        void ShowDropDown()
        {
            // Show the context menu at the left-bottom of the button
            Point leftBottom = new Point(Bounds.Left, Bounds.Bottom);

            _dropDown.Show(Parent.PointToScreen(leftBottom));
        }

        #endregion Methods

        #region OnEvents

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> 
        /// event.</param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                base.OnClick(e);

                // If there is no database, do nothing
                if (_database == null)
                {
                    return;
                }

                // Create the drop down
                if (_dropDown != null)
                {
                    _dropDown.Dispose();
                }
                _dropDown = CreateDropDown();

                // Show drop down
                ShowDropDown();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28725", ex);
            }
        }

        #endregion OnEvents

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="FileTagDropDown.FileTagAdded"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="FileTagDropDown.FileTagAdded"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="FileTagDropDown.FileTagAdded"/> event.</param>
        void HandleFileTagAdded(object sender, FileTagAddedEventArgs e)
        {
            try
            {
                // Add and tag the file
                FileTag tag = e.FileTag;
                _database.AddTag(tag.Name, tag.Description, true);
                _database.TagFile(_fileId, tag.Name);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28726", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="FileTagDropDown.FileTagClicked"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="FileTagDropDown.FileTagClicked"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="FileTagDropDown.FileTagClicked"/> event.</param>
        void HandleFileTagClicked(object sender, FileTagClickedEventArgs e)
        {
            try
            {
                _database.ToggleTagOnFile(_fileId, e.Name);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28761", ex);
            }
        }

        #endregion Event Handlers
    }
}
