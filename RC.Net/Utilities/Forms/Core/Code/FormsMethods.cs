using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a grouping of methods for performing operations on 
    /// <see cref="System.Windows.Forms"/> related objects.
    /// </summary>
    public static class FormsMethods
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(FormsMethods).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

        #region FormsMethods Methods

        /// <summary>
        /// Adds and docks a control in a container and sets the minimum size of the parent to 
        /// exactly fit the control.
        /// </summary>
        /// <param name="control">The control to add and dock into the 
        /// <paramref name="container"/>.</param>
        /// <param name="parent">The control that contains the <paramref name="container"/>.
        /// </param>
        /// <param name="container">The control to which the <paramref name="control"/> should be 
        /// added and docked. Must not contain any other controls.</param>
        public static void DockControlIntoContainer(Control control, Control parent, 
            Control container)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23143");

                // Add the control to the container
                container.Controls.Add(control);

                // Compute the size change needed to fit the control exactly on the parent
                Size sizeChange = control.Size;
                sizeChange += container.Padding.Size;
                sizeChange -= container.Size;

                // Increase only, do not shrink
                if (sizeChange.Height < 0)
                {
                    sizeChange.Height = 0;
                }
                if (sizeChange.Width < 0)
                {
                    sizeChange.Width = 0;
                }

                // Increase the size of the parent to exactly fit the control
                parent.MinimumSize = parent.Size + sizeChange;

                // Dock the control
                control.Dock = DockStyle.Fill;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22219", ex);
            }
        }

        /// <summary>
        /// Centers the <paramref name="formToCenter"/> <see cref="Form"/>
        /// in the <paramref name="formToCenterIn"/> <see cref="Form"/>.
        /// </summary>
        /// <param name="formToCenter">The <see cref="Form"/> to be centered.</param>
        /// <param name="formToCenterIn">The <see cref="Form"/> to be centered on.</param>
        public static void CenterFormInForm(Form formToCenter, Form formToCenterIn)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23144");

                // Get the location and size of the parent form
                Point formToCenterInLocation = formToCenterIn.Location;
                Size formToCenterInSize = formToCenterIn.Size;

                // Compute and set the location for this form
                formToCenter.Location = new Point(
                    formToCenterInLocation.X + ((formToCenterInSize.Width / 2) - (formToCenter.Width / 2)),
                    formToCenterInLocation.Y + ((formToCenterInSize.Height / 2) - (formToCenter.Height / 2)));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22220", ex);
            }
        }

        /// <summary>
        /// Prevents the specified <see cref="Control"/> from updating (redrawing) until the lock 
        /// is released.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> for which updating is to be locked/
        /// unlocked.</param>
        /// <param name="lockUpdate"><see langword="true"/> to lock the
        /// <see cref="Control"/>from updating or <see langword="false"/> to release the lock and allow
        /// updates again and refresh the control.</param>
        public static void LockControlUpdate(Control control, bool lockUpdate)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26080");

                NativeMethods.LockControlUpdate(control, lockUpdate);

                // If unlocking the updates, refresh the control now to show changes
                // since the lock was applied.
                if (!lockUpdate)
                {
                    control.Refresh();
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI25203", ex);
            }
        }

        #endregion FormsMethods Methods
    }
}
