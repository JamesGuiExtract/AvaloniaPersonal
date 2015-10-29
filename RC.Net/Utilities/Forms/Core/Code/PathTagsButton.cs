using Extract.Licensing;
using Extract.Utilities.Forms.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// The display style for this path tags button.
    /// </summary>
    public enum PathTagsButtonDisplayStyle
    {
        /// <summary>
        /// Set the button to display just the image.
        /// </summary>
        ImageOnly,

        /// <summary>
        /// Sets the button to display both the image and text (it is a good idea
        /// to set the <see cref="TextImageRelation"/> for the button). The default
        /// <see cref="TextImageRelation"/> will be image on the left, text on the right.
        /// </summary>
        ImageAndText,

        /// <summary>
        /// Displays just the text in the button.
        /// </summary>
        TextOnly
    }

    /// <summary>
    /// Represents a button with a drop down that allows the user to select expandable path tags.
    /// </summary>
    [DefaultEvent("TagSelected")]
    [DefaultProperty("PathTags")]
    [CLSCompliant(false)]
    public partial class PathTagsButton : Button
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PathTagsButton).ToString();

        /// <summary>
        /// The text to display in the button if set to display text.
        /// </summary>
        static readonly string _BUTTON_TEXT = "Tags >";

        #endregion Constants

        #region PathTagsButton Fields

        /// <summary>
        /// The context menu that is displayed when the tag button is clicked.
        /// </summary>
        ContextMenuStrip _dropDown;

        /// <summary>
        /// The menu items added and handled by this class. Any menu items not in this list will
        /// have been added in the <see cref="MenuOpening"/> event by another class.
        /// </summary>
        List<ToolStripItem> _ownedItems = new List<ToolStripItem>();

        /// <summary>
        /// The list of document tags to be displayed in the context menu drop down.
        /// </summary>
        IPathTags _pathTags;

        /// <summary>
        /// The display style for this <see cref="PathTagsButton"/>. Default style is
        /// image only.
        /// </summary>
        PathTagsButtonDisplayStyle _displayStyle = PathTagsButtonDisplayStyle.ImageOnly;

        /// <summary>
        /// A text control associated with this button.
        /// </summary>
        TextBoxBase _textControl;
        
        ///// <summary>
        ///// A combo box associated with this button.
        ///// </summary>
        //ComboBox _comboBox;
        // see: https://extract.atlassian.net/browse/ISSUE-13280

        /// <summary>
        /// Whether or not path tags should be displayed in the drop down.
        /// </summary>
        bool _displayPathTags = true;

        /// <summary>
        /// Whether or not function tags should be displayed in the drop down.
        /// </summary>
        bool _displayFunctionTags = true;

        #endregion PathTagsButton Fields

        #region PathTagsButton Events

        /// <summary>
        /// Raised as the menu is opening; allows for custom menu items to be added or for the menu
        /// to be cancelled.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when the menu is opening.")]
        public event EventHandler<PathTagsMenuOpeningEventArgs> MenuOpening;

        /// <summary>
        /// Occurs when a tag is being selected.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a tag is being selected.")]
        public event EventHandler<TagSelectingEventArgs> TagSelecting;

        /// <summary>
        /// Occurs when a tag has been selected.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a tag has been selected.")]
        public event EventHandler<TagSelectedEventArgs> TagSelected;

        #endregion PathTagsButton Events

        #region PathTagsButton Constructors

        /// <summary>
        /// Initializes a new <see cref="PathTagsButton"/> class.
        /// </summary>
        public PathTagsButton()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel()); 
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23150",
                    _OBJECT_NAME);

                InitializeComponent();

                this.Size = new Size(18, 20);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23151", ex);
            }
        }

        #endregion PathTagsButton Constructors

        #region Properties

        /// <summary>
        /// Gets/sets a text control associated with this <see cref="PathTagsButton"/>.
        /// <para><b>Note</b></para>
        /// Only one of <see cref="TextControl"/> and <see cref="ComboBox"/> should be set (not both).
        /// </summary>
        /// <returns>A text control associated with this <see cref="PathTagsButton"/>.</returns>
        /// <value>A text control associated with this <see cref="PathTagsButton"/>.</value>
        [DefaultValue(null)]
        [Description("A text control to automatically update when a tag is selected.")]
        public TextBoxBase TextControl
        {
            get
            {
                return _textControl;
            }

            set
            {
                //ExtractException.Assert("ELI38662", "A ComboBox cannot be set if a TextControl is.",
                //    value == null);
                // || _comboBox == null);

                _textControl = value;
            }
        }

        ///// <summary>
        ///// Gets/sets a <see cref="ComboBox"/> associated with this <see cref="PathTagsButton"/>.
        ///// <para><b>Note</b></para>
        ///// Only one of <see cref="TextControl"/> and <see cref="ComboBox"/> should be set (not both).
        ///// </summary>
        ///// <returns>The <see cref="ComboBox"/> associated with this <see cref="PathTagsButton"/>.</returns>
        ///// <value>The <see cref="ComboBox"/> associated with this <see cref="PathTagsButton"/>.</value>
        //[DefaultValue(null)]
        //[Description("A combo box to automatically update when a tag is selected.")]
        //public ComboBox ComboBox
        //{
        //    get
        //    {
        //        return _comboBox;
        //    }

        //    set
        //    {
        //        ExtractException.Assert("ELI38663", "A TextControl cannot be set if a ComboBox is.",
        //            value == null || _textControl == null);

        //        _comboBox = value;
        //    }
        //}
        // see: https://extract.atlassian.net/browse/ISSUE-13280
        //

        /// <summary>
        /// Gets or sets the path tags that are available for selection.
        /// </summary>
        /// <value>The path tags that are available for selection.</value>
        /// <returns>The path tags that are available for selection.</returns>
        [Category("Behavior")]
        [Description("The path tags that are available for selection.")]
        public virtual IPathTags PathTags
        {
            get
            {
                if (_pathTags == null)
                {
                    _pathTags = new SourceDocumentPathTags();
                }

                return _pathTags;
            }
            set
            {
                _pathTags = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PathTagsButtonDisplayStyle"/> for this button.
        /// </summary>
        /// <returns>The <see cref="PathTagsButtonDisplayStyle"/> for this button.</returns>
        /// <value>The <see cref="PathTagsButtonDisplayStyle"/> for this button.</value>
        [Category("Appearance")]
        [DefaultValue(PathTagsButtonDisplayStyle.ImageOnly)]
        [Description("Whether to display an image or text.")]
        public PathTagsButtonDisplayStyle DisplayStyle
        {
            get
            {
                return _displayStyle;
            }
            set
            {
                try
                {
                    // If setting to a new value then need to change the text/image
                    // of the button based on the new display style
                    if (_displayStyle != value)
                    {
                        _displayStyle = value;
                        switch (_displayStyle)
                        {
                            case PathTagsButtonDisplayStyle.ImageOnly:
                                this.Text = "";
                                this.Image = Resources.SelectDocTagArrow.ToBitmap();
                                break;

                            case PathTagsButtonDisplayStyle.TextOnly:
                                this.Text = _BUTTON_TEXT;
                                this.Image = null;
                                break;

                            case PathTagsButtonDisplayStyle.ImageAndText:
                                this.Text = _BUTTON_TEXT;
                                this.Image = Resources.SelectDocTagArrow.ToBitmap();
                                break;

                            default:
                                ExtractException.ThrowLogicException("ELI26221");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26498", ex);
                }
            }
        }

        /// <summary>
        /// Gets/sets whether path tags should be displayed in the drop down.
        /// </summary>
        [Category("Behavior")]
        [Description("The path tags that are available for selection.")]
        [DefaultValue(true)]
        public bool DisplayPathTags
        {
            get
            {
                return _displayPathTags;
            }
            set
            {
                _displayPathTags = value;
            }
        }

        /// <summary>
        /// Gets/sets whether function tags should be displayed in the drop down.
        /// </summary>
        [Category("Behavior")]
        [Description("The function tags that are available for selection.")]
        [DefaultValue(true)]
        public bool DisplayFunctionTags
        {
            get
            {
                return _displayFunctionTags;
            }
            set
            {
                _displayFunctionTags = value;
            }
        }

        #endregion Properties

        #region PathTagsButton OnEvents

        /// <summary>
        /// Raises the <see cref="Control.CreateControl"/> event.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            try
            {
                switch (_displayStyle)
                {
                    case PathTagsButtonDisplayStyle.ImageOnly:
                        this.Text = "";
                        this.Image = Resources.SelectDocTagArrow.ToBitmap();
                        break;

                    case PathTagsButtonDisplayStyle.TextOnly:
                        this.Text = _BUTTON_TEXT;
                        this.Image = null;
                        break;

                    case PathTagsButtonDisplayStyle.ImageAndText:
                        this.Text = _BUTTON_TEXT;
                        this.Image = Resources.SelectDocTagArrow.ToBitmap();
                        break;

                    default:
                        ExtractException.ThrowLogicException("ELI26222");
                        break;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26211", ex);
            }
        }

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
                // Create a new menu item each time as _dropDown may have been modified by a
                // MenuOpening handler last time.
                if (_dropDown != null)
                {
                    _dropDown.Dispose();
                }

                // Create a context menu for the drop down
                _dropDown = new ContextMenuStrip();
                _dropDown.ShowImageMargin = false;
                _dropDown.ItemClicked += HandleItemClicked;

                // Add the menu items to the drop down
                ToolStripItemCollection items = _dropDown.Items;

                if (_displayPathTags)
                {
                    foreach (string tag in PathTags.BuiltInTags)
                    {
                        items.Add(new ToolStripMenuItem(tag));
                    }

                    if (PathTags.CustomTags.Any())
                    {
                        if (items.Count > 0)
                        {
                            items.Add(new ToolStripSeparator());
                        }

                        foreach (string tag in PathTags.CustomTags)
                        {
                            items.Add(new ToolStripMenuItem(tag));
                        }
                    }
                }

                // Check if displaying function tags
                if (_displayFunctionTags)
                {
                    // Only add the separator if there was at least one doc tag
                    if (items.Count > 0)
                    {
                        items.Add(new ToolStripSeparator());
                    }

                    // Add the function tags
                    foreach (string function in PathTags.FormattedFunctionNames)
                    {
                        items.Add(new ToolStripMenuItem(function));
                    }
                }

                // Keep track of which items were added by this class (as opposed to MenuOpening
                // handlers).
                _ownedItems.AddRange(items.Cast<ToolStripItem>());

                // Allow MenuOpening handlers to add their own custom options or cancel the menu.
                var eventArgs = new PathTagsMenuOpeningEventArgs(_dropDown);
                OnMenuOpening(eventArgs);
                if (!eventArgs.Cancel)
                {
                    // Get the right-top coordinate of the button
                    Rectangle clientRectangle = this.ClientRectangle;
                    Point rightTop = new Point(clientRectangle.Right, clientRectangle.Top);

                    // Show the context menu at the top right of the button
                    _dropDown.Show(PointToScreen(rightTop));
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22725", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="TagSelecting"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="TagSelecting"/> 
        /// event.</param>
        protected virtual void OnTagSelecting(TagSelectingEventArgs e)
        {
            if (TagSelecting != null)
            {
                TagSelecting(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="TagSelected"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="TagSelected"/> 
        /// event.</param>
        protected virtual void OnTagSelected(TagSelectedEventArgs e)
        {
            if (TagSelected != null)
            {
                TagSelected(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:MenuOpening"/> event.
        /// </summary>
        /// <param name="e">The <see cref="PathTagsMenuOpeningEventArgs"/> instance containing the
        /// event data.</param>
        protected virtual void OnMenuOpening(PathTagsMenuOpeningEventArgs e)
        {
            if (MenuOpening != null)
            {
                MenuOpening(this, e);
            }
        }

        #endregion PathTagsButton OnEvents

        #region PathTagsButton Event Handlers

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
                // Check if a tag was selected (the separator's Text will be the empty string)
                string tagName = e.ClickedItem.Text;
                if (_ownedItems.Contains(e.ClickedItem) && !string.IsNullOrEmpty(tagName))
                {
                    if (tagName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase))
                    {
                        this.SafeBeginInvoke("ELI38069", () => 
                            PathTags.EditCustomTags((int)TopLevelControl.Handle));
                        return;
                    }

                    var eventArgs = new TagSelectingEventArgs(tagName);
                    OnTagSelecting(eventArgs);

                    if (eventArgs.Cancel)
                    {
                        return;
                    }

                    if (TextControl != null)
                    {
                        int originalSelectionStart = TextControl.SelectionStart;
                        string originalSelectedText = TextControl.SelectedText;

                        // If a tag function has been selected, automatically position the cursor
                        // between the parentheses
                        if (tagName.StartsWith("$", StringComparison.OrdinalIgnoreCase) &&
                            tagName.EndsWith(")", StringComparison.OrdinalIgnoreCase))
                        {
                            int parameterInsertIndex = tagName.IndexOf('(') + 1;
                            tagName = tagName.Substring(0, parameterInsertIndex);

                            tagName += originalSelectedText + ")";
                            TextControl.SelectionLength = TextControl.SelectedText.Length;

                            TextControl.SelectedText = tagName;
                            TextControl.SelectionStart = originalSelectionStart + parameterInsertIndex;
                            TextControl.SelectionLength = originalSelectedText.Length;
                        }
                        else
                        {
                            TextControl.SelectedText = tagName;
                            TextControl.SelectionLength = 0;
                        }

                        TextControl.Focus();
                    }
                    //else if (ComboBox != null)
                    //{
                    //    // Same logic as above must be repeated since ComboBox and TextBoxBase don't
                    //    // share a common base class or interface.
                    //    int originalSelectionStart = ComboBox.SelectionStart;
                    //    string originalSelectedText = ComboBox.SelectedText;

                    //    // If a tag function has been selected, automatically position the cursor
                    //    // between the parentheses
                    //    if (tagName.StartsWith("$", StringComparison.OrdinalIgnoreCase) &&
                    //        tagName.EndsWith(")", StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        int parameterInsertIndex = tagName.IndexOf('(') + 1;
                    //        tagName = tagName.Substring(0, parameterInsertIndex);

                    //        tagName += originalSelectedText + ")";
                    //        ComboBox.SelectionLength = ComboBox.SelectedText.Length;

                    //        ComboBox.SelectedText = tagName;
                    //        ComboBox.SelectionStart = originalSelectionStart + parameterInsertIndex;
                    //        ComboBox.SelectionLength = originalSelectedText.Length;
                    //    }
                    //    else
                    //    {
                    //        ComboBox.SelectedText = tagName;
                    //        ComboBox.SelectionLength = 0;
                    //    }

                    //    ComboBox.Focus();
                    //}

                    // Raise the TagSelected event
                    OnTagSelected(new TagSelectedEventArgs(tagName));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22718", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion PathTagsButton Event Handlers
    }

    /// <summary>
    /// Provides data for the <see cref="PathTagsButton.TagSelecting"/> event.
    /// </summary>
    public class TagSelectingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The tag that is being selected.
        /// </summary>
        readonly string _tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagSelectingEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The tag that was selected.</param>
        public TagSelectingEventArgs(string tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Gets the tag that is being selected.
        /// </summary>
        /// <returns>The tag that is being selected.</returns>
        public string Tag
        {
            get
            {
                return _tag;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PathTagsButton.TagSelected"/> event.
    /// </summary>
    public class TagSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// The tag that was selected.
        /// </summary>
        readonly string _tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagSelectedEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The tag that was selected.</param>
        public TagSelectedEventArgs(string tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Gets the tag that was selected.
        /// </summary>
        /// <returns>The tag that was selected.</returns>
        public string Tag
        {
            get
            {
                return _tag;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PathTagsButton.MenuOpening"/> event.
    /// </summary>
    public class PathTagsMenuOpeningEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The <see cref="ContextMenuStrip"/>.
        /// </summary>
        readonly ContextMenuStrip _contextMenuStrip;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathTagsMenuOpeningEventArgs"/> class.
        /// </summary>
        /// <param name="contextMenuStrip"></param>
        public PathTagsMenuOpeningEventArgs(ContextMenuStrip contextMenuStrip)
        {
            _contextMenuStrip = contextMenuStrip;
        }

        /// <summary>
        /// Gets the <see cref="ContextMenuStrip"/>.
        /// </summary>
        /// <returns></returns>
        public ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return _contextMenuStrip;
            }
        }
    }
}
