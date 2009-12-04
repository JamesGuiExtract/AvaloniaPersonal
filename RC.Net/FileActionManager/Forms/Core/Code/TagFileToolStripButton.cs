using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
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
        ContextMenuStrip _dropDown;

        /// <summary>
        /// The file id of the tags to display.
        /// </summary>
        int _fileId;

        /// <summary>
        /// The database from which to read and write tags.
        /// </summary>
        IFileProcessingDB _database;

        /// <summary>
        /// A list of possible tags.
        /// </summary>
        string[] _tags;

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static readonly LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

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
                _licenseCache.Validate("ELI28712");

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

            Array.Sort(tags, StringComparer.OrdinalIgnoreCase);

            return tags;
        }

        /// <summary>
        /// Gets the file tags associated with the current file id.
        /// </summary>
        /// <returns>The file tags associated with the current file id.</returns>
        string[] GetFileTags()
        {
            // If there is no database, return null
            if (_database == null)
            {
                return null;
            }

            // Get the file tags from the database
            VariantVector vector = _database.GetTagNames();
            string[] tags = GetVectorAsArray(vector);

            // Sort the tags alphabetically
            Array.Sort(tags, StringComparer.OrdinalIgnoreCase);

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
        /// <param name="tags">The tags to display in the menu.</param>
        /// <param name="checkedTags">The names of the tags that should be checked.</param>
        /// <param name="applyNewTags"><see langword="true"/> if the user can apply new tags;
        /// <see langword="false"/> if the user cannot apply new tags.</param>
        /// <returns>A drop down context menu strip to display to the user.</returns>
        ContextMenuStrip CreateDropDown(ICollection<string> tags, string[] checkedTags, 
            bool applyNewTags)
        {
            ContextMenuStrip dropDown = null;
            try
            {
                // Create a context menu for the drop down
                dropDown = new ContextMenuStrip();
                dropDown.ShowCheckMargin = true;
                dropDown.ShowImageMargin = false;
                dropDown.ItemClicked += HandleItemClicked;

                // Get the drop down items
                ToolStripItemCollection items = dropDown.Items;

                // Add the file tags
                if (tags.Count > 0)
                {
                    foreach (string tag in tags)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem(tag);
                        item.Checked = IsTagContainedInArray(tag, checkedTags);
                        items.Add(item);
                    }
                }
                else
                {
                    ToolStripMenuItem item = new ToolStripMenuItem("No tags available");
                    item.Enabled = false;
                    items.Add(item);
                }

                // Add the apply new tags button if necessary
                if (applyNewTags)
                {
                    items.Add(new ToolStripSeparator());
                    
                    items.Add(new ToolStripMenuItem("Apply new tag..."));
                }

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
        /// Determines if the specified tag is contained in the specified array.
        /// </summary>
        /// <param name="tag">The tag to check for containment.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns><see langword="true"/> if <paramref name="tag"/> is in 
        /// <paramref name="tags"/>; <see langword="false"/> if <paramref name="tag"/> is not in 
        /// <paramref name="tags"/>.</returns>
        static bool IsTagContainedInArray(string tag, string[] tags)
        {
            return Array.BinarySearch(tags, tag, StringComparer.OrdinalIgnoreCase) >= 0;
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

        /// <summary>
        /// Displays a prompt to the user allowing them to apply a new tag. Applies the new tag if 
        /// they select OK.
        /// </summary>
        void PromptToApplyNewTag()
        {
            // Display the dialog.
            using (FileTagDialog dialog = new FileTagDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Add and tag the file
                    FileTag tag = dialog.FileTag;
                    _database.AddTag(tag.Name, tag.Description);
                    _database.TagFile(_fileId, tag.Name);
                }
            }
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
            base.OnClick(e);

            try
            {
                // If there is no database, do nothing
                if (_database == null)
                {
                    return;
                }

                // Create the drop down
                _tags = GetFileTags();
                string[] checkedTags = GetTagsForFileId(_fileId);
                bool applyNewTags = _database.AllowDynamicTagCreation();

                if (_dropDown != null)
                {
                    _dropDown.Dispose();
                }
                _dropDown = CreateDropDown(_tags, checkedTags, applyNewTags);

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
        /// Handles the <see cref="ToolStrip.ItemClicked"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStrip.ItemClicked"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStrip.ItemClicked"/> event.</param>
        void HandleItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                // Get the index of the clicked item
                ToolStripItem item = e.ClickedItem;
                int index = item.Owner.Items.IndexOf(item);

                if (index < _tags.Length)
                {
                    // A tag was clicked. Toggle it in the database.
                    _database.ToggleTagOnFile(_fileId, item.Text);
                }
                else if (index == item.Owner.Items.Count - 1)
                {
                    // Apply new tag was clicked. Allow the user to apply a new tag.
                    PromptToApplyNewTag();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28726", ex);
            }
        }

        #endregion Event Handlers
    }
}
