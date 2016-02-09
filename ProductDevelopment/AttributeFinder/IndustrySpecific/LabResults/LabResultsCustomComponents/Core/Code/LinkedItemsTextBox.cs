using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using Extract.Licensing;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// A text box that displays a list of hyperlink-styled values that can be clicked to raise
    /// events targeting the item associated with a link.
    /// </summary>
    public class LinkedItemsTextBox : Scintilla
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(LinkedItemsTextBox).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// An ID for the style used to format links in the text box.
        /// </summary>
        int _linkStyleId;

        /// <summary>
        /// Keeps track of the text <see cref="Range"/> for each item displayed in this box.
        /// </summary>
        List<Tuple<object, Range>> _items = new List<Tuple<object, Range>>();

        /// <summary>
        /// The item that been clicked (pending subsequent MouseUp).
        /// </summary>
        object _clickedItem;

        /// <summary>
        /// Indicates when if the listed items are in the process of being reset.
        /// </summary>
        bool _settingItems;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedItemsTextBox"/> class.
        /// </summary>
        public LinkedItemsTextBox()
        {
            try
            {
                _settingItems = true;

                IsReadOnly = true;

                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI39333",
                    _OBJECT_NAME);

                _linkStyleId = Styles.LastPredefined.Index;
                var linkStyle = Styles[_linkStyleId];
                linkStyle.Font = Font;
                linkStyle.IsHotspot = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39334");
            }
            finally
            {
                _settingItems = false;
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the link for one of the items provided via <see cref="SetItems"/> is
        /// clicked.
        /// </summary>
        public event EventHandler<ItemClickedEventArgs> ItemClicked;

        #endregion Events

        #region Methods

        /// <summary>
        /// Populates the text box with a comma separated list of links where the key of each
        /// dictionary item is the object being selected and the value is the text that should be
        /// used for the link.
        /// </summary>
        public void SetItems(Dictionary<object, string> items)
        {
            try
            {
                _settingItems = true;

                _items.Clear();

                // Compile the new text for the box using the values of items.
                var sb = new StringBuilder();
                foreach (var item in items)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    int itemPos = sb.Length;
                    sb.Append(item.Value);

                    // Create a text range to represent the added item and maintain its association
                    // with the item key.
                    var range = GetRange(itemPos, itemPos + item.Value.Length);
                    _items.Add(new Tuple<object, Range>(item.Key, range));
                }

                // Scintilla box will not accept new text (even programmatically) if IsReadOnly is true
                IsReadOnly = false;
                Text = sb.ToString();
                IsReadOnly = true;

                // Apply link styling to all added items.
                foreach (var range in _items.Select(item => item.Item2))
                {
                    range.SetStyle(_linkStyleId);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39326");
            }
            finally
            {
                _settingItems = false;
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Hide the base class <see cref="Scintilla.IsReadOnly"/> property which should always be
        /// <see langword="true"/> for this class.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        new public bool IsReadOnly
        {
            get
            {
                return true;
            }

            set
            {
                if (!_settingItems && !value)
                {
                    var ee = new ExtractException("ELI39332",
                        "IsReadOnly may not be modified for LinkedItemsTextBox");
                    ee.Display();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                base.OnMouseUp(e);

                // If the mouse down corresponded with an item being clicked, as long as that same
                // item is associated with the current mouse position, raise ItemClicked.
                if (_clickedItem != null &&
                    _clickedItem == GetItemFromPosition(PositionFromPoint(e.X, e.Y)))
                {
                    OnItemClicked(_clickedItem);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39327");
            }
            finally
            {
                _clickedItem = null;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:HotspotClick"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ScintillaNET.ScintillaMouseEventArgs"/> instance
        /// containing the event data.</param>
        protected override void OnHotspotClick(ScintillaMouseEventArgs e)
        {
            try 
	        {	        
		        base.OnHotspotClick(e);

                // If the handler of ItemClicked raised right here removes focus from this control
                // (such as by displaying a dialog), it causes the text box to behave as if the
                // mouse button is being held down once focus is returned to this control. This
                // behavior is avoided by waiting for the subsequent MouseUp event before raising
                // ItemClicked.
                _clickedItem = GetItemFromPosition(e.Position);

                ExtractException.Assert("ELI39328", "Item not found.", _clickedItem != null);
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI39329");
	        }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Gets the item associated with the specified character <see paramref="position"/> in the
        /// text box.
        /// </summary>
        /// <param name="position">The character index for which the associated item is needed.
        /// <see langword="null"/> if there is no item link at the specified position.</param>
        object GetItemFromPosition(int position)
        {
            foreach (var item in _items)
            {
                if (item.Item2.PositionInRange(position))
                {
                    return item.Item1;
                }
            }

            return null;
        }

        /// <summary>
        /// Raised the <see cref="ItemClicked"/> event.
        /// </summary>
        /// <param name="item">The item for which the event is being raised.</param>
        void OnItemClicked(object item)
        {
            var eventHandler = ItemClicked;
            if (eventHandler != null)
            {
                eventHandler(this, new ItemClickedEventArgs(item));
            }
        }

        #endregion Private Members
    }

    /// <summary>
    /// <see cref="EventArgs"/> extension used for <see cref="LinkedItemsTextBox.ItemClicked"/>
    /// </summary>
    public class ItemClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemClickedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The item whose link has been clicked.</param>
        public ItemClickedEventArgs(object item)
        {
            try
            {
                Item = item;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39330");
            }
        }

        /// <summary>
        /// Gets the item whose link has been clicked.
        /// </summary>
        /// <returns>The  <see cref="object"/> representing the item whose link has been clicked.
        /// </returns>
        public object Item
        {
            get;
            private set;
        }
    }
}
