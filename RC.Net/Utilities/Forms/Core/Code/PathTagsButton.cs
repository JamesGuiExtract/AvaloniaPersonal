using Extract.Licensing;
using Extract.Utilities.Forms.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
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
    /// Represents a button with a drop down that allows the user to select expandable path tags.
    /// </summary>
    public partial class PathTagsButton : Button
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(PathTagsButton).ToString();

        /// <summary>
        /// The text to display in the button if set to display text.
        /// </summary>
        private static readonly string _BUTTON_TEXT = "Tags >";

        #endregion Constants

        #region PathTagsButton Fields

        /// <summary>
        /// The names of valid function tags.
        /// </summary>
        private static string[] _functionTags = GetFunctionTags();

        /// <summary>
        /// The context menu that is displayed when the tag button is clicked.
        /// </summary>
        private ContextMenuStrip _dropDown;

        /// <summary>
        /// The list of document tags to be displayed in the context menu drop down.
        /// </summary>
        private List<string> _docTags = new List<string>();

        /// <summary>
        /// The display style for this <see cref="PathTagsButton"/>. Default style is
        /// image only.
        /// </summary>
        private PathTagsButtonDisplayStyle _displayStyle = PathTagsButtonDisplayStyle.ImageOnly;

        #endregion PathTagsButton Fields

        #region PathTagsButton Events

        /// <summary>
        /// Occurs when a tag is selected.
        /// </summary>
        public event EventHandler<TagSelectedEventArgs> TagSelected;

        #endregion PathTagsButton Events

        #region PathTagsButton Constructors

        /// <overload>Initializes a new <see cref="PathTagsButton"/> class.</overload>
        /// <summary>
        /// Initializes a new <see cref="PathTagsButton"/> class.
        /// </summary>
        public PathTagsButton()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="PathTagsButton"/> class.
        /// </summary>
        /// <param name="addSourceDoc">If <see langword="true"/> then
        /// &lt;SourceDocName&gt; tag will be added to the document tags.</param>
        public PathTagsButton(bool addSourceDoc)
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

                if (addSourceDoc)
                {
                    _docTags.Add("<SourceDocName>");
                }

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23151", ex);
            }
        }

        #endregion PathTagsButton Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the <see cref="PathTagsButtonDisplayStyle"/> for this button.
        /// </summary>
        /// <returns>The <see cref="PathTagsButtonDisplayStyle"/> for this button.</returns>
        /// <value>The <see cref="PathTagsButtonDisplayStyle"/> for this button.</value>
        [DefaultValue(PathTagsButtonDisplayStyle.ImageOnly)]
        public PathTagsButtonDisplayStyle PathTagsButtonDisplayStyle
        {
            get
            {
                return _displayStyle;
            }
            set
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
        }

        #endregion Properties

        #region PathTagsButton Methods

        /// <summary>
        /// Resets the document tags list with the specified list. If <paramref name="tags"/>
        /// is <see langword="null"/> then the document tags list will be cleared. If
        /// <paramref name="tags"/> is not <see langword="null"/> then the document tag list
        /// be reset to the specified tags.
        /// </summary>
        /// <param name="tags">The collection of tags to reset the document tags list
        /// to. If <see langword="null"/> then the list will be cleared.</param>
        public void ResetDocTags(IEnumerable<string> tags)
        {
            try
            {
                // First clear the list of doc tags
                _docTags.Clear();

                // If the new tags collection is not null, fill the doc tags list
                // with the specified tags
                if (tags != null)
                {
                    _docTags.AddRange(tags);
                }

                // Ensure the doc tags are sorted
                _docTags.Sort();

                // If the context menu has been created, clear it so that it will be reset
                // the next time the button is clicked
                if (_dropDown != null)
                {
                    _dropDown.Dispose();
                    _dropDown = null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26212", ex);
            }
        }

        /// <summary>
        /// Adds the specified collection of strings to the document tags collection.
        /// </summary>
        /// <param name="tags">The tags to add to the collection of document tags. Must
        /// not be <see langword="null"/>.</param>
        /// <exception cref="ExtractException">If <paramref name="tags"/> is
        /// <see langword="null"/>.</exception>
        public void AddDocTags(IEnumerable<string> tags)
        {
            try
            {
                ExtractException.Assert("ELI26213", "Tags collection cannot be null!",
                    tags != null);

                _docTags.AddRange(tags);

                // Ensure the doc tags are sorted
                _docTags.Sort();

                // If the context menu has been created, clear it so that it will be reset
                // the next time the button is clicked
                if (_dropDown != null)
                {
                    _dropDown.Dispose();
                    _dropDown = null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26214", ex);
            }
        }

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
                    foreach (string docTag in _docTags)
                    {
                        items.Add(new ToolStripMenuItem(docTag));
                    }

                    // Only add the separator if there was at least one doc tag
                    if (_docTags.Count > 0)
                    {
                        items.Add(new ToolStripSeparator());
                    }

                    // Add the function tags
                    foreach (string function in _functionTags)
                    {
                        items.Add(new ToolStripMenuItem(function));
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
        private void HandleItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                // Check if a tag was selected (the separator's Text will be the empty string)
                string tagName = e.ClickedItem.Text;
                if (!string.IsNullOrEmpty(tagName))
                {
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
}
