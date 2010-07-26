using Extract.Drawing;
using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A class that enhances the standard <see cref="ProgressBar"/> by adding the ability
    /// to display the current percentage in the progress bar.
    /// </summary>
    public partial class BetterProgressBar : ProgressBar
    {
        #region Fields

        /// <summary>
        /// Object name string used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(BetterProgressBar).ToString();

        /// <summary>
        /// The <see cref="Color"/> to draw the percentage text in.
        /// </summary>
        Color _textColor = Color.Black;

        /// <summary>
        /// The <see cref="Color"/> to draw the percentage text in when it overlays the
        /// progress bar.
        /// </summary>
        Color _overlayColor = Color.Black;

        /// <summary>
        /// The format for the percentage string.
        /// </summary>
        StringFormat _textFormat = new StringFormat();

        /// <summary>
        /// Whether the percentage text should be displayed or not.
        /// </summary>
        bool _displayPercentage = true;

        /// <summary>
        /// Whether 0% should be displayed in the progress bar or not.
        /// </summary>
        bool _displayZeroPercent = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterProgressBar"/> class.
        /// </summary>
        public BetterProgressBar()
        {
            try
            {
                // Only validate the license at run time
                if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                {
                    LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                        "ELI30354", _OBJECT_NAME);
                }

                InitializeComponent();
                _textFormat.LineAlignment = StringAlignment.Center;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30355", ex);
            }
        }

        #endregion Constructors

        /// <summary>
        /// Raises the <see cref="Control.Paint"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_displayPercentage && (Value > 0 || _displayZeroPercent))
            {
                Region clipRegion = e.Graphics.Clip;
                try
                {
                    int percentage = Value / Maximum;
                    string text = percentage.ToString("G%", CultureInfo.CurrentCulture);
                    Rectangle bar = ClientRectangle;
                    bar.Width = (int)(bar.Width * (Value / Maximum));
                    using (Region regionLeft = new Region(bar),
                        regionRight = new Region(ClientRectangle))
                    {
                        regionRight.Exclude(regionLeft);
                        e.Graphics.Clip = regionLeft;
                        e.Graphics.DrawString(text, base.Font,
                            ExtractBrushes.GetSolidBrush(_overlayColor),
                            ClientRectangle, _textFormat);
                        e.Graphics.Clip = regionRight;
                        e.Graphics.DrawString(text, base.Font,
                            ExtractBrushes.GetSolidBrush(_textColor),
                            ClientRectangle, _textFormat);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30353", ex);
                }
                finally
                {
                    e.Graphics.Clip = clipRegion;
                }
            }
        }

        #region Properties

        /// <summary>
        /// Gets/sets the color of the text for displaying the percentage.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public Color TextColor
        {
            get
            {
                return _textColor;
            }
            set
            {
                _textColor = value;
            }
        }

        /// <summary>
        /// Gets/sets the color of the text when it is overlayed over the progress bar.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public Color OverlayText
        {
            get
            {
                return _overlayColor;
            }
            set
            {
                _overlayColor = value;
            }
        }

        /// <summary>
        /// Gets/sets the alignment with which the percentage is displayed.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        [DefaultValue(StringAlignment.Center)]
        public StringAlignment PercentageAlignment
        {
            get
            {
                return _textFormat.Alignment;
            }
            set
            {
                _textFormat.Alignment = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the percentage should be displayed in the control.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        [DefaultValue(true)]
        public bool DisplayPercentage
        {
            get
            {
                return _displayPercentage;
            }
            set
            {
                _displayPercentage = value;
            }
        }

        /// <summary>
        /// Whether '0%' should be displayed or not when the value is 0.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        [DefaultValue(true)]
        public bool DisplayZeroPercent
        {
            get
            {
                return _displayZeroPercent;
            }
            set
            {
                _displayZeroPercent = value;
            }
        }

        #endregion Properties
    }
}
