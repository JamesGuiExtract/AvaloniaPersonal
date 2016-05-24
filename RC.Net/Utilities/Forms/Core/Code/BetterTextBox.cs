using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// An extension of the <see cref="TextBox"/> class that allows for indication of a required
    /// field with a missing value or an error glyph that can be applied for specific issues with
    /// the text box's value.
    /// </summary>
    public class BetterTextBox : TextBox
    {
        #region Fields

        /// <summary>
        /// The <see cref="ErrorProvider"/> to be used to display error glyphs.
        /// </summary>
        ErrorProvider _errorProvider;

        /// <summary>
        /// Indicates when the required marker is in the process of being updated so that text
        /// changes for this purpose are not interpreted as events that should trigger updating
        /// the required marker.
        /// </summary>
        bool _updatingMarker;

        /// <summary>
        /// Indicates when new text is being applied to the <see cref="Text"/> property.
        /// </summary>
        bool _settingText;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterTextBox"/> class.
        /// </summary>
        public BetterTextBox()
            : base()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether a value is required.
        /// </summary>
        /// <value><see langword="true"/> if required; otherwise, <see langword="false"/>.
        /// </value>
        [DefaultValue(false)]
        public bool Required
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="ErrorProvider"/> to be used to display error glyphs.
        /// </summary>
        /// <value>
        /// The error provider.
        /// </value>
        public ErrorProvider ErrorProvider
        {
            get
            {
                if (_errorProvider == null)
                {
                    _errorProvider = new ErrorProvider();
                    _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                    this.SetErrorGlyphPosition(_errorProvider);
                }

                return _errorProvider;
            }

            set
            {
                _errorProvider = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Sets the error provider glyph and tooltip, when errorText is non-empty. 
        /// Clears the error provider glyph and tooltip when errorText is String.Empty.
        /// </summary>
        /// <param name="errorText">The error text.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void SetError(string errorText)
        {
            try
            {
                ErrorProvider.SetError(this, errorText);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39881");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Gets or sets the current text in the <see cref="T:System.Windows.Forms.TextBox"/>.
        /// </summary>
        /// <returns>The text displayed in the control.</returns>
        public override string Text
        {
            get
            {
                try
                {
                    // If in the process of updating the Text value, literal underlying value needs
                    // to be returned.
                    if (_settingText)
                    {
                        return base.Text;
                    }

                    // If any TextBoxExtensionMethods method is calling, it is for the purposes of
                    // dealing with special text values; the literal underlying value needs to be
                    // returned.
                    var frame = new StackFrame(1);
                    var callingType = frame.GetMethod().DeclaringType;
                    if (callingType == typeof(TextBoxExtensionMethods))
                    {
                        return base.Text;
                    }

                    // For all other callers, treat the special required text as an empty string.
                    return this.IsRequiredMarkerSet()
                        ? ""
                        : base.Text;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39882");
                }
            }

            set
            {
                try
                {
                    _settingText = true;
                    base.Text = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39883");
                }
                finally
                {
                    _settingText = false;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Enter"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnEnter(EventArgs e)
        {
            try
            {
                base.OnEnter(e);

                UpdateRequiredMarker(hasFocus: true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39884");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Leave"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLeave(EventArgs e)
        {
            try
            {
                base.OnLeave(e);

                UpdateRequiredMarker(hasFocus: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39885");
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnTextChanged(EventArgs e)
        {
            try
            {
                base.OnTextChanged(e);

                UpdateRequiredMarker(hasFocus: Focused);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39886");
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Updates the presence or absence of the marker based on the current state of the control
        /// and the state of the <see paramref="hasFocus"/> parameter.
        /// </summary>
        /// <param name="hasFocus">This method needs to be called as part of both gaining and
        /// losing focus and the <see cref="Control.Focus"/> property may not yet be in the state
        /// that will result from the current event handler, allow the caller to specify the new
        /// focus state.</param>
        void UpdateRequiredMarker(bool hasFocus)
        {
            if (_updatingMarker)
            {
                return;
            }

            try
            {
                _updatingMarker = true;

                if (!hasFocus && Enabled && Required && string.IsNullOrWhiteSpace(base.Text))
                {
                    this.SetRequiredMarker();
                }
                else
                {
                    this.RemoveRequiredMarker();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39887");
            }
            finally
            {
                _updatingMarker = false;
            }
        }

        #endregion Private Members
    }
}
