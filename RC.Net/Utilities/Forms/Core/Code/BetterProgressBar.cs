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
        Color _overlayColor = Color.White;

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
                _textFormat.Alignment = StringAlignment.Center;
                Style = ProgressBarStyle.Continuous;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30355", ex);
            }
        }

        #endregion Constructors

        /// <summary>
        /// Raises the <see cref="Control.CreateControl"/> event.
        /// </summary>
        protected override void OnCreateControl()
        {
            try
            {
                base.OnCreateControl();

                // In order to display the progress status bar as continuous, themes need to be diabled
                // (see MSDN documentation).
                if (Style == ProgressBarStyle.Continuous)
                {
                    int result = NativeMethods.SetWindowTheme(Handle, "", "");
                    if (result != 0)
                    {
                        ExtractException ee = new ExtractException("ELI30513",
                            "Failed to modify theme to allow for continuous progress bar style.");
                        ee.AddDebugData("HRESULT", result, false);
                        ee.Log();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30511", ex);
            }
        }

        /// <summary>
        /// Processes windows messages.
        /// </summary>
        /// <param name="m">The Windows <see cref="Message"/> to process.</param>
        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);

                // Handle the paint message in WndProc to draw the percentage text. Overriding
                // OnPaint is not a good option since either OnPaint is not called or
                // ControlStyles.UserPaint is used which does allow OnPaint to be called but
                // prevents base.OnPaint from drawing the progress bars.
                if (m.Msg == WindowsMessage.Paint)
                {
                    if (_displayPercentage && (Value > 0 || _displayZeroPercent))
                    {
                        using (var graphics = Graphics.FromHwnd(Handle))
                        {
                            int percentage = 100 * Value / Maximum;
                            string text = percentage.ToString("G", CultureInfo.CurrentCulture) + "%";

                            // Draw the text so that _overlayColor is used for the portion of the
                            // text over the progress bar and _textColor is used for the portion
                            // that is not.
                            if (Style == ProgressBarStyle.Continuous)
                            {
                                Rectangle bar = ClientRectangle;
                                // Calculate the width the progress bar should be. I have
                                // unscientifically found that about 2 pixels should be added to
                                // this value... I'm not clear on why.
                                bar.Width = 2 + (bar.Width * percentage / 100);
                            
                                using (Region regionLeft = new Region(bar),
                                    regionRight = new Region(ClientRectangle))
                                {
                                    graphics.Clip = regionRight;
                                        
                                    regionRight.Exclude(regionLeft);
                                    graphics.Clip = regionLeft;
                                    graphics.DrawString(text, base.Font,
                                        ExtractBrushes.GetSolidBrush(_overlayColor),
                                        ClientRectangle, _textFormat);
                                    graphics.Clip = regionRight;
                                    graphics.DrawString(text, base.Font,
                                        ExtractBrushes.GetSolidBrush(_textColor),
                                        ClientRectangle, _textFormat);
                                }
                            }
                            // Draw text in _textColor on top of a solid rectangle of _overlayColor
                            // since with the block progress bar style it is not very readable to
                            // just alternate the font color.
                            else
                            {
                                SizeF textSize = graphics.MeasureString(text, base.Font,
                                    ClientRectangle.Size, _textFormat);
                                Rectangle backgroundRect = new Rectangle(
                                    (ClientRectangle.Width / 2) - ((int)textSize.Width / 2),
                                    (ClientRectangle.Height / 2) - ((int)textSize.Height / 2),
                                    (int)textSize.Width, (int)textSize.Height);

                                graphics.FillRectangle(
                                    ExtractBrushes.GetSolidBrush(_overlayColor), backgroundRect);

                                graphics.DrawString(text, base.Font,
                                    ExtractBrushes.GetSolidBrush(_textColor),
                                    ClientRectangle, _textFormat);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30512", ex);
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
