using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents a <see cref="ContextMenuStrip"/> that allows the user to select file tags.
    /// </summary>
    public sealed partial class FileTagDropDown : ContextMenuStrip
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FileTagDropDown).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// List of displayed tags.
        /// </summary>
        readonly FileTag[] _tags;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when a file tag is clicked.
        /// </summary>
        public event EventHandler<FileTagClickedEventArgs> FileTagClicked;

        /// <summary>
        /// Occurs when the user creates a new file tag.
        /// </summary>
        public event EventHandler<FileTagAddedEventArgs> FileTagAdded;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTagDropDown"/> class.
        /// </summary>
        /// <param name="tags">The tags to display in the menu.</param>
        /// <param name="checkedTags">The names of the tags that should be checked.</param>
        /// <param name="applyNewTags"><see langword="true"/> if the user can apply new tags;
        /// <see langword="false"/> if the user cannot apply new tags.</param>
        public FileTagDropDown(FileTag[] tags, string[] checkedTags, bool applyNewTags)
        {
            // Load licenses in design mode
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                // Load the license files from folder
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            }

            // Validate license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28778",
                _OBJECT_NAME);

            InitializeComponent();

            ShowCheckMargin = true;
            ShowImageMargin = false;

            // Add the file tags
            if (tags.Length > 0)
            {
                Array.Sort(checkedTags, StringComparer.OrdinalIgnoreCase);

                foreach (FileTag tag in tags)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(tag.Name);
                    item.Checked = IsTagContainedInArray(tag.Name, checkedTags);
                    item.ToolTipText = tag.Description;
                    Items.Add(item);
                }
            }
            else
            {
                ToolStripMenuItem item = new ToolStripMenuItem("No tags available");
                item.Enabled = false;
                Items.Add(item);
            }

            // Add the apply new tags button if necessary
            if (applyNewTags)
            {
                Items.Add(new ToolStripSeparator());

                Items.Add(new ToolStripMenuItem("Apply new tag..."));
            }

            _tags = tags;
        }

        #endregion Constructors

        #region Methods

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
        /// Toggles the state of the specified <paramref name="item"/> and raises the 
        /// <see cref="FileTagClicked"/> event.
        /// </summary>
        /// <param name="item">The tool strip item to mark as clicked.</param>
        void MarkItemAsClicked(ToolStripItem item)
        {
            // Toggle the state of the menu item
            ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
            menuItem.Checked = !menuItem.Checked;

            // Raise the FileTagClicked event
            OnFileTagClicked(new FileTagClickedEventArgs(item.Text));
        }

        /// <summary>
        /// Displays a prompt to the user allowing them to apply a new tag. Raises the 
        /// <see cref="FileTagAdded"/> if the user selects OK.
        /// </summary>
        void PromptToApplyNewTag()
        {
            // Display the dialog
            using (FileTagDialog dialog = new FileTagDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Raise the FileTagAdded event
                    OnFileTagAdded(new FileTagAddedEventArgs(dialog.FileTag));
                }
            }
        }

        #endregion Methods

        #region OnEvents

        /// <summary>
        /// Raises the <see cref="FileTagClicked"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileTagClicked"/> 
        /// event.</param>
        void OnFileTagClicked(FileTagClickedEventArgs e)
        {
            if (FileTagClicked != null)
            {
                FileTagClicked(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="FileTagAdded"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileTagAdded"/> 
        /// event.</param>
        void OnFileTagAdded(FileTagAddedEventArgs e)
        {
            if (FileTagAdded != null)
            {
                FileTagAdded(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ToolStrip.ItemClicked"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="ToolStrip.ItemClicked"/> 
        /// event.</param>
        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            try
            {
                base.OnItemClicked(e);

                // Get the index of the clicked item
                ToolStripItem item = e.ClickedItem;
                int index = item.Owner.Items.IndexOf(item);

                if (index < _tags.Length)
                {
                    // A tag was clicked
                    MarkItemAsClicked(item);
                }
                else if (index == item.Owner.Items.Count - 1)
                {
                    // Apply new tag was clicked
                    PromptToApplyNewTag();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28760", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="ToolStripDropDown.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ToolStripDropDownClosingEventArgs"/> that contains the 
        /// event data.</param>
        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            try
            {
                base.OnClosing(e);

                // Prevent the context menu from closing when an item is clicked [DNRCAU #361]
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28762", ex);
            }
        }

        #endregion OnEvents
    }
}
