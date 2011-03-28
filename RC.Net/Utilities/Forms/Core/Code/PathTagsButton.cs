using Extract.Licensing;
using Extract.Utilities.Forms.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

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
        /// The names of valid function tags.
        /// </summary>
        static readonly string[] _functionTags = GetFunctionTags();

        /// <summary>
        /// The context menu that is displayed when the tag button is clicked.
        /// </summary>
        ContextMenuStrip _dropDown;

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
        /// Whether or not function tags should be displayed in the drop down.
        /// </summary>
        bool _displayFunctionTags = true;

        #endregion PathTagsButton Fields

        #region PathTagsButton Events

        /// <summary>
        /// Occurs when a tag is selected.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a tag is selected.")]
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
        /// Gets/sets the text control associated with this <see cref="PathTagsButton"/>.
        /// </summary>
        /// <returns>The text control associated with this <see cref="PathTagsButton"/>.</returns>
        /// <value>The text control associated with this <see cref="PathTagsButton"/>.</value>
        [DefaultValue(null)]
        [Description("The text control to automatically update when a tag is selected.")]
        public TextBox TextControl { get; set; }

        /// <summary>
        /// Gets or sets the path tags that are available for selection.
        /// </summary>
        /// <value>The path tags that are available for selection.</value>
        /// <returns>The path tags that are available for selection.</returns>
        [Category("Behavior")]
        [Description("The path tags that are available for selection.")]
        public IPathTags PathTags
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
        /// Gets/sets whether function tags should be displayed in the drop down.
        /// </summary>
        [Category("Behavior")]
        [Description("The path tags that are available for selection.")]
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

        #region PathTagsButton Methods

        /// <summary>
        /// Retrieves an array of the names of function tags.
        /// </summary>
        /// <returns>A array of the names of function tags.</returns>
        static string[] GetFunctionTags()
        {
            List<string> functionTags;

            // Do not do this at design time
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                functionTags = new List<string>();
            }
            else
            {
                // Get the function tag names
                MiscUtils miscUtils = new MiscUtils();
                VariantVector functions = miscUtils.GetExpansionFunctionNames();

                // Construct an array of the function tags
                int size = functions.Size;
                functionTags = new List<string>(size);
                for (int i = 0; i < size; i++)
                {
                    string function = functions[i] as string;
                    if (function != null)
                    {
                        functionTags.Add(function);
                    }
                }
            }

            // Return the resulting array
            return functionTags.ToArray();
        }

        /// <summary>
        /// Determines whether the <see cref="PathTags"/> property should be serialized.
        /// </summary>
        /// <returns><see langword="true"/> if the property has changed; <see langword="false"/>
        /// if the property is its default value.</returns>
        bool ShouldSerializePathTags()
        {
            return _pathTags != null && _pathTags.GetType() != typeof(SourceDocumentPathTags);
        }

        /// <summary>
        /// Resets the <see cref="PathTags"/> property to its default value.
        /// </summary>
        void ResetPathTags()
        {
            _pathTags = null;
        }

        #endregion PathTagsButton Methods

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
                // Create the drop down if it is not already created
                if (_dropDown == null)
                {
                    // Create a context menu for the drop down
                    _dropDown = new ContextMenuStrip();
                    _dropDown.ShowImageMargin = false;
                    _dropDown.ItemClicked += HandleItemClicked;

                    // Add the menu items to the drop down
                    ToolStripItemCollection items = _dropDown.Items;

                    // Add the doc tags
                    foreach (string docTag in PathTags.Tags)
                    {
                        items.Add(new ToolStripMenuItem(docTag));
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
                        foreach (string function in _functionTags)
                        {
                            items.Add(new ToolStripMenuItem(function));
                        }
                    }
                }

                // Get the right-top coordinate of the button
                Rectangle clientRectangle = this.ClientRectangle;
                Point rightTop = new Point(clientRectangle.Right, clientRectangle.Top);

                // Show the context menu at the top right of the button
                _dropDown.Show(PointToScreen(rightTop));
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22725", ex);
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
                if (!string.IsNullOrEmpty(tagName))
                {
                    if (TextControl != null)
                    {
                        TextControl.SelectedText = tagName;
                    }

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
}
