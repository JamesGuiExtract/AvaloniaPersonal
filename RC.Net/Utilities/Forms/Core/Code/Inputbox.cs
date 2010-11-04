// InputBox - Based on implementation by: Andrew Ma
// URL: http://www.devhood.com/Tools/tool_details.aspx?tool_id=295

using Extract.Licensing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Displays a prompt in a dialog box, waits for the user to input text
    /// or click a button, and then returns a string containing the contents of the text box.
    /// </summary>
    public static class InputBox
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME =
           typeof(InputBox).ToString();

        #endregion Constants

        /// <summary>
        /// Displays a prompt in a dialog box, waits for the user to input text or
        /// click a button, and then returns a string containing the contents of the text box.
        /// </summary>
        /// <param name="prompt">String expression displayed as the message
        /// in the dialog box.</param>
        /// <param name="title">String expression displayed in the title bar
        /// of the dialog box.</param>
        /// <param name="defaultResponse">String expression displayed in the text box as
        /// the default response if no other input is provided. If you omit defaultResponse,
        /// the displayed text box is empty.  This parameter may be <see langword="null"/>
        /// <para><b>Note:</b></para>
        /// This is a reference parameter that after the function has returned will
        /// contain the value that the user entered in the input box.</param>
        /// <returns>The result from the dialog's closing action.</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        public static DialogResult Show(string prompt, string title, ref string defaultResponse)
        {
            return Show(prompt, title, ref defaultResponse, -1, -1);
        }

        /// <summary>
        /// Displays a prompt in a dialog box, waits for the user to input text or
        /// click a button, and then returns a string containing the contents of the text box.
        /// <para><b>Note:</b></para>
        /// The dialog will be centered within the <paramref name="owner"/> window.
        /// </summary>
        /// <param name="owner">The form which owns this dialog box.  The dialog box
        /// will be centered in the owner window. If parameter is <see langword="null"/>
        /// then the input box will be displayed in the default location.</param>
        /// <param name="prompt">String expression displayed as the message
        /// in the dialog box.</param>
        /// <param name="title">String expression displayed in the title bar
        /// of the dialog box.</param>
        /// <param name="defaultResponse">String expression displayed in the text box as
        /// the default response if no other input is provided. If you omit defaultResponse,
        /// the displayed text box is empty.  This parameter may be <see langword="null"/>
        /// <para><b>Note:</b></para>
        /// This is a reference parameter that after the function has returned will
        /// contain the value that the user entered in the input box.</param>
        /// <returns>The result from the dialog's closing action.</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        public static DialogResult Show(IWin32Window owner, string prompt, string title,
            ref string defaultResponse)
        {
            // Check if the owner is null
            if (owner == null)
            {
                // Owner is null, just show the input box at the default location
                return Show(prompt, title, ref defaultResponse, -1, -1);
            }

            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23171",
                    _OBJECT_NAME);

                // Get the screen rectangle of the owner window
                Rectangle ownerRectangle = NativeMethods.GetWindowScreenRectangle(owner);

                // Compute the center point of the owner form
                Point centerPoint = new Point((ownerRectangle.Left + (ownerRectangle.Width / 2)),
                    (ownerRectangle.Top + (ownerRectangle.Height / 2)));

                // Create a new input box dialog
                using (InputBoxForm inputBox = new InputBoxForm())
                {
                    // Compute the left of the input box based on the owner center point
                    centerPoint.X -= inputBox.Width / 2;

                    // Compute the top of the input box based on the owner center point
                    centerPoint.Y -= inputBox.Height / 2;

                    // Set the title, prompt and default response
                    inputBox.Title = title;
                    inputBox.Prompt = prompt;
                    inputBox.DefaultResponse = defaultResponse ?? "";

                    // Set the startup location
                    inputBox.StartLocation = centerPoint;

                    // Show the input box
                    DialogResult result = inputBox.ShowDialog();

                    // Set the ref value
                    defaultResponse = inputBox.ReturnValue;

                    // Return the dialog result
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21966", ex);
            }
        }

        /// <summary>
        /// Displays a prompt in a dialog box, waits for the user to input text or
        /// click a button, and then returns a string containing the contents of the text box.
        /// </summary>
        /// <param name="prompt">String expression displayed as the message
        /// in the dialog box.</param>
        /// <param name="title">String expression displayed in the title bar
        /// of the dialog box.</param>
        /// <param name="defaultResponse">String expression displayed in the text box as
        /// the default response if no other input is provided. If you omit defaultResponse,
        /// the displayed text box is empty.  This parameter may be <see langword="null"/>
        /// <para><b>Note:</b></para>
        /// This is a reference parameter that after the function has returned will
        /// contain the value that the user entered in the input box.</param>
        /// <param name="x">Integer expression that specifies, in pixels,
        /// the distance of the left edge of the dialog box from the
        /// left edge of the screen.</param>
        /// <param name="y">Integer expression that specifies, in pixels,
        /// the distance of the upper edge of the dialog box from the top of the screen.</param>
        /// <returns>The result from the dialog's closing action.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "x")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "y")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        public static DialogResult Show(string prompt, string title, ref string defaultResponse,
            int x, int y)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23256",
                    _OBJECT_NAME);

                // Create a new input box dialog
                using (InputBoxForm inputBox = new InputBoxForm())
                {
                    // Set the title, prompt and default response
                    inputBox.Title = title;
                    inputBox.Prompt = prompt;
                    inputBox.DefaultResponse = defaultResponse ?? "";

                    // If a start location has been defined, set it
                    if (x >= 0 && y >= 0)
                    {
                        inputBox.StartLocation = new Point(x, y);
                    }

                    // Show the input box
                    DialogResult result = inputBox.ShowDialog();

                    // Set the ref value
                    defaultResponse = inputBox.ReturnValue;

                    // Return the dialog result
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI21967", ex);
            }
        }
    }
}
