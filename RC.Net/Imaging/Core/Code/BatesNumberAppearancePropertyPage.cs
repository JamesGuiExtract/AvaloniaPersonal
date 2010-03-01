using Extract;
using Extract.Drawing;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents the property page for the appearance of Bates numbers.
    /// </summary>
    public partial class BatesNumberAppearancePropertyPage : UserControl, IPropertyPage
    {
        #region BatesNumberManagerAppearancePropertyPage Constants

        /// <summary>
        /// The text prepended before the selected alignment.
        /// </summary>
        readonly static string _ANCHOR_ALIGNMENT_BASE_TEXT = "Selected alignment: ";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(BatesNumberAppearancePropertyPage).ToString();

        #endregion BatesNumberManagerAppearancePropertyPage Constants

        #region BatesNumberManagerAppearancePropertyPage Fields

        /// <summary>
        /// Whether or not the settings on the property page have been modified.
        /// </summary>
        private bool _dirty;

        /// <summary>
        /// The font to use for Bates numbers.
        /// </summary>
        Font _font;

        /// <summary>
        /// A dialog that allows the user to select a font for the Bates number.
        /// </summary>
        FontDialog _fontDialog;

        /// <summary>
        /// The format object that will be modified by this dialog
        /// </summary>
        BatesNumberFormat _format;

        #endregion BatesNumberManagerAppearancePropertyPage Fields

        #region BatesNumberManagerAppearancePropertyPage Events

        /// <summary>
        /// Event raised when the property page changes.
        /// </summary>
        public event EventHandler PropertyPageModified;

        #endregion BatesNumberManagerAppearancePropertyPage Events

        #region BatesNumberManagerAppearancePropertyPage Constructors

        /// <summary>
        /// Initializes a new <see cref="BatesNumberAppearancePropertyPage"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public BatesNumberAppearancePropertyPage(BatesNumberFormat format)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23184",
					_OBJECT_NAME);

                InitializeComponent();

                _format = format;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23185", ex);
            }
        }

        #endregion BatesNumberManagerAppearancePropertyPage Constructors

        #region BatesNumberManagerAppearancePropertyPage Methods

        /// <summary>
        /// Resets all the values to the values stored in <see cref="_format"/> and 
        /// resets the dirty flag to <see langword="false"/>.
        /// </summary>
        private void RefreshSettings()
        {
            // Set the UI elements
            _font = _format.Font;
            _horizontalInchesTextBox.Text = 
                _format.HorizontalInches.ToString(CultureInfo.CurrentCulture);
            _verticalInchesTextBox.Text =
                _format.VerticalInches.ToString(CultureInfo.CurrentCulture);
            _anchorAlignmentControl.AnchorAlignment = _format.AnchorAlignment;
            UpdateFontText();

            switch (_format.PageAnchorAlignment)
            {
                case AnchorAlignment.LeftBottom:
                    _horizontalPositionComboBox.Text = "left";
                    _verticalPositionComboBox.Text = "bottom";
                    break;

                case AnchorAlignment.RightBottom:
                    _horizontalPositionComboBox.Text = "right";
                    _verticalPositionComboBox.Text = "bottom";
                    break;

                case AnchorAlignment.LeftTop:
                    _horizontalPositionComboBox.Text = "left";
                    _verticalPositionComboBox.Text = "top";
                    break;

                case AnchorAlignment.RightTop:
                    _horizontalPositionComboBox.Text = "right";
                    _verticalPositionComboBox.Text = "top";
                    break;

                default:
                    ExtractException ee = new ExtractException("ELI22242",
                        "Unexpected page anchor alignment.");
                    ee.AddDebugData("Alignment", _format.PageAnchorAlignment, false);
                    throw ee;
            }

            // Reset the dirty flag
            _dirty = false;
        }

        /// <summary>
        /// Updates the font text box with information stored in <see cref="_font"/>.
        /// </summary>
        private void UpdateFontText()
        {
            _fontTextBox.Text = FontMethods.GetUserFriendlyFontString(_font);
        }

        #endregion BatesNumberManagerAppearancePropertyPage Methods

        #region BatesNumberManagerAppearancePropertyPage OnEvents

        /// <summary>
        /// Raises the <see cref="UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="UserControl.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Refresh the UI elements
            RefreshSettings();
        }

        /// <summary>
        /// Raises the <see cref="PropertyPageModified"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="PropertyPageModified"/> 
        /// event.</param>
        protected virtual void OnPropertyPageModified(EventArgs e)
        {
            _dirty = true;

            if (PropertyPageModified != null)
            {
                PropertyPageModified(this, e);
            }
        }

        #endregion BatesNumberManagerAppearancePropertyPage OnEvents

        #region BatesNumberManagerAppearancePropertyPage Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        private void HandleChangeFontButtonClick(object sender, EventArgs e)
        {
            // Create the font dialog if not already created
            if (_fontDialog == null)
            {
                _fontDialog = new FontDialog();
                _fontDialog.AllowScriptChange = false;
                _fontDialog.ShowEffects = false;
            }
            _fontDialog.Font = _font;

            // Show the font dialog and bail if the user selected cancel
            if (_fontDialog.ShowDialog() == DialogResult.OK)
            {
                // Store the font chosen
                _font = _fontDialog.Font;

                // Display the font
                UpdateFontText();

                // Raise the PropertyPageModified event
                OnPropertyPageModified(new EventArgs());
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandleHorizontalInchesTextBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandleVerticalInchesTextBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandleHorizontalPositionComboBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandleVerticalPositionComboBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="AnchorAlignmentControl.AnchorAlignmentChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="AnchorAlignmentControl.AnchorAlignmentChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="AnchorAlignmentControl.AnchorAlignmentChanged"/> event.</param>
        private void HandleAnchorAlignmentControlAnchorAlignmentChanged(object sender,
            AnchorAlignmentChangedEventArgs e)
        {
            // Set the base text for the label
            _selectedAlignmentLabel.Text = _ANCHOR_ALIGNMENT_BASE_TEXT;

            // Append the selected alignment
            _selectedAlignmentLabel.Text +=
                AnchorAlignmentHelper.GetAlignmentAsString(e.AnchorAlignment);

            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        #endregion BatesNumberManagerAppearancePropertyPage Event Handlers

        #region IPropertyPage Members

        /// <summary>
        /// Applies the changes to the <see cref="BatesNumberFormat"/>.
        /// </summary>
        public void Apply()
        {
            try
            {
                // Ensure the settings are valid
                if (!this.IsValid)
                {
                    MessageBox.Show("Cannot apply changes. Settings are invalid.", "Invalid settings",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    return;
                }

                float horizontalInches =
                    Convert.ToSingle(_horizontalInchesTextBox.Text, CultureInfo.CurrentCulture);
                float verticalInches =
                    Convert.ToSingle(_verticalInchesTextBox.Text, CultureInfo.CurrentCulture);

                AnchorAlignment pageAnchorAlignment = 0;
                if (_horizontalPositionComboBox.Text == "right")
                {
                    pageAnchorAlignment += 2;
                }
                if (_verticalPositionComboBox.Text == "top")
                {
                    pageAnchorAlignment += 6;
                }

                // Store the settings
                _format.Font = _font;
                _format.HorizontalInches = horizontalInches;
                _format.VerticalInches = verticalInches;
                _format.AnchorAlignment = _anchorAlignmentControl.AnchorAlignment;
                _format.PageAnchorAlignment = pageAnchorAlignment;

                // Reset the dirty flag
                _dirty = false;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27839", ex);
            }
        }

        /// <summary>
        /// Gets whether the settings on the property page have been modified.
        /// </summary>
        /// <return><see langword="true"/> if the settings on the property page have been modified;
        /// <see langword="false"/> if they have not been modified.</return>
        public bool IsDirty
        {
            get
            {
                return _dirty;
            }
        }

        /// <summary>
        /// Gets whether the user-specified settings on the property page are valid.
        /// </summary>
        /// <value><see langword="true"/> if the user-specified settings are valid; 
        /// <see langword="false"/> if the settings are not valid.</value>
        public bool IsValid
        {
            get
            {
                // Ensure inches are non-negative integers
                float inchesX, inchesY;
                if (!float.TryParse(_horizontalInchesTextBox.Text, out inchesX) || inchesX < 0 ||
                    !float.TryParse(_verticalInchesTextBox.Text, out inchesY) || inchesY < 0)
                {
                    return false;
                }

                // Handle special case of zero horizontal inches
                if (inchesX == 0)
                {
                    switch (_anchorAlignmentControl.AnchorAlignment)
                    {
                        // Can't go zero inches from right, if anchor point is on left side
                        case AnchorAlignment.LeftBottom:
                        case AnchorAlignment.Left:
                        case AnchorAlignment.LeftTop:
                            if (_horizontalPositionComboBox.Text == "right")
                            {
                                return false;
                            }
                            break;

                        // Can't go zero inches from left, if anchor point is on right side
                        case AnchorAlignment.RightBottom:
                        case AnchorAlignment.Right:
                        case AnchorAlignment.RightTop:
                            if (_horizontalPositionComboBox.Text == "left")
                            {
                                return false;
                            }
                            break;
                        default:
                            break;
                    }
                }

                // Handle special case of zero vertical inches
                if (inchesY == 0)
                {
                    switch (_anchorAlignmentControl.AnchorAlignment)
                    {
                        // Can't go zero inches from top, if anchor point is on bottom
                        case AnchorAlignment.LeftBottom:
                        case AnchorAlignment.Bottom:
                        case AnchorAlignment.RightBottom:
                            if (_verticalPositionComboBox.Text == "top")
                            {
                                return false;
                            } 
                            break;

                        // Can't go zero inches from bottom, if anchor point is on top
                        case AnchorAlignment.LeftTop:
                        case AnchorAlignment.Top:
                        case AnchorAlignment.RightTop:
                            if (_verticalPositionComboBox.Text == "bottom")
                            {
                                return false;
                            }
                            break;
                    }
                }

                // If this point is reached, settings are valid
                return true;
            }
        }

        /// <summary>
        /// Sets the focus to the first control in the property page.
        /// </summary>
        public void SetFocusToFirstControl()
        {
            // Do nothing
        }

        #endregion
    }
}
