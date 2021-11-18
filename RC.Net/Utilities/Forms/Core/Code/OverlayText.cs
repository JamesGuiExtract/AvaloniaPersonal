using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Extract.Drawing;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Displays text on top of a control
    /// </summary>
    public class OverlayText : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="Control"/> on which the text should be displayed.
        /// </summary>
        Control _target;

        /// <summary>
        /// Function to provide text to be displayed.
        /// </summary>
        Func<string> _textProvider;

        /// <summary>
        /// The brush to be used to draw the text.
        /// </summary>
        SolidBrush _brush;

        /// <summary>
        /// The <see cref="Font"/> in which the text should be drawn.
        /// </summary>
        Font _font;

        /// <summary>
        /// A <see cref="StringFormat"/> value that determines where the text will be drawn.
        /// </summary>
        StringFormat _stringFormat;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayText"/> class.
        /// </summary>
        /// <param name="target">
        /// </param>
        /// <param name="text">A function that calculates what text should be displayed.</param>
        /// <param name="font">The <see cref="Font"/> to use.</param>
        /// <param name="color">The <see cref="Color"/> in which the text should be drawn. If the
        /// alpha value of the color is &lt; 255, the text will be transparent.</param>
        /// <param name="stringFormat">A <see cref="StringFormat"/> value that determines where the
        /// text will be drawn. If <see paramref="null"/>, the <see paramref="font"/> will be scaled
        /// such that the text is centered and fills the control as completelyl as possible.</param>
        OverlayText(Control target, Func<string> textProvider, Font font, Color color,
            StringFormat stringFormat)
        {
            try
            {
                _target = target;
                _textProvider = textProvider;
                _font = font;
                _brush = ExtractBrushes.GetSolidBrush(color);
                _stringFormat = stringFormat;

                _target.Paint += HandleTargetPaint;
                _target.Invalidate();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31124", ex);
            }
        }

        #endregion Constructors

        #region Static Members

        /// <summary>
        /// Displays text on the specified <see paramref="target"/> <see cref="Control"/>.
        /// </summary>
        /// <param name="target">The <see cref="Control"/> on which the text should be displayed.
        /// </param>
        /// <param name="textProvider">Function that returns the text that should be displayed.</param>
        /// <param name="font">The <see cref="Font"/> to use.</param>
        /// <param name="color">The <see cref="Color"/> in which the text should be drawn. If the
        /// alpha value of the color is &lt; 255, the text will be transparent.</param>
        /// <param name="stringFormat">A <see cref="StringFormat"/> value that determines where the
        /// text will be drawn. If <see paramref="null"/>, the <see paramref="font"/> will be scaled
        /// such that the text is centered and fills the control as completelyl as possible.</param>
        /// <param name="displayTime">The number of seconds the text should be displayed before
        /// automatically disappearing. If &lt;= 0, the text will be displayed until the Cancel is
        /// called on the <see cref="CancellationTokenSource"/> return value.</param>
        /// <returns>A <see cref="CancellationTokenSource"/> instance to allow the text to be
        /// removed by calling Cancel.</returns>
        public static CancellationTokenSource ShowText(Control target, Func<string> textProvider, Font font,
            Color color, StringFormat stringFormat, int displayTime)
        {
            try
            {
                CancellationTokenSource canceler = new CancellationTokenSource();

                Task.Factory.StartNew(() =>
                {
                    using OverlayText label = new(target, textProvider, font, color, stringFormat);
                    canceler.Token.WaitHandle.WaitOne(
                        displayTime > 0 ? displayTime * 1000 : Timeout.Infinite);
                }, canceler.Token);

                return canceler;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31111", ex);
            }
        }

        /// <summary>
        /// Displays text on the specified <see paramref="target"/> <see cref="Control"/>.
        /// </summary>
        /// <param name="target">The <see cref="Control"/> on which the text should be displayed.
        /// </param>
        /// <param name="text">The text that should be displayed.</param>
        /// <param name="font">The <see cref="Font"/> to use.</param>
        /// <param name="color">The <see cref="Color"/> in which the text should be drawn. If the
        /// alpha value of the color is &lt; 255, the text will be transparent.</param>
        /// <param name="stringFormat">A <see cref="StringFormat"/> value that determines where the
        /// text will be drawn. If <see paramref="null"/>, the <see paramref="font"/> will be scaled
        /// such that the text is centered and fills the control as completelyl as possible.</param>
        /// <param name="displayTime">The number of seconds the text should be displayed before
        /// automatically disappearing. If &lt;= 0, the text will be displayed until the Cancel is
        /// called on the <see cref="CancellationTokenSource"/> return value.</param>
        /// <returns>A <see cref="CancellationTokenSource"/> instance to allow the text to be
        /// removed by calling Cancel.</returns>
        public static CancellationTokenSource ShowText(Control target, string text, Font font,
            Color color, StringFormat stringFormat, int displayTime)
        {
            return ShowText(target, () => text, font, color, stringFormat, displayTime);
        }


        #endregion Static Members

        #region IDisposable Members

        /// <overloads>
        /// Releases resources used by the <see cref="OverlayText"/>
        /// </overloads>
        /// <summary>
        /// Releases resources used by the <see cref="OverlayText"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the <see cref="OverlayText"/>
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_target != null)
                {
                    _target.Paint -= HandleTargetPaint;
                    _target.Invalidate();
                    _target = null;
                }
            }
        }

        #endregion IDisposable Members

        #region Event Handlers

        /// <summary>
        /// Handles the Paint event of the target control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        void HandleTargetPaint(object sender, PaintEventArgs e)
        {
            // [FlexIDSCore:4556]
            // It seems in some cases e.Graphics must be null (or perhaps _target is disposed?).
            // In that case, nothing can be drawn.
            if (e.Graphics == null || _target.IsDisposed)
            {
                return;
            }

            Font font = _font;

            try 
	        {
                StringFormat stringFormat;
                string text = _textProvider();

                // If _stringFormat is null, center the text and scale the font so that it fills as
                // much of the control as possible.
                if (_stringFormat == null)
                {
                    stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;

                    font = FontMethods.GetFontThatFits(text, e.Graphics,
                        _target.ClientRectangle.Size, _font.FontFamily, _font.Style);
                }
                else
                {
                    stringFormat = _stringFormat;
                }

                e.Graphics.DrawString(text, font, _brush, _target.ClientRectangle, stringFormat);
	        }
	        catch (Exception ex)
	        {
                // [FlexIDSCore:4556]
                // Since we are getting an exception in rare cases, and since I have not had much
                // luck finding a way to resolve the issue, I am changing the exception handling to
                // log instead of display. In the cases I have seen (at least), the overlay text is
                // still seen, its just that a single fleeting call generates an exception. The
                // display of the exception is interfering with use of the software for a UI problem
                // that would otherwise be undetectable.
		        ExtractException.Log("ELI31110", ex);
	        }
            finally
            {
                // If a scaled font was created, dispose of it.
                if (font != _font)
                {
                    font.Dispose();
                }
            }
        }

        #endregion Event Handlers
    }
}
