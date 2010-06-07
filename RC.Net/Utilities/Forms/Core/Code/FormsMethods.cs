using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        static readonly string _OBJECT_NAME = typeof(FormsMethods).ToString();

        #endregion Constants

        #region Methods

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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23143",
                    _OBJECT_NAME);

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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI26080",
                    _OBJECT_NAME);

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
                throw ExtractException.AsExtractException("ELI25203", ex);
            }
        }

        /// <summary>
        /// Checks whether a .Net auto-complete list is displayed.
        /// </summary>
        /// <returns><see langword="true"/> if an auto-complete list is displayed;
        /// <see langword="false"/> otherwise.</returns>
        public static bool IsAutoCompleteDisplayed()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28877",
                    _OBJECT_NAME);

                return NativeMethods.IsAutoCompleteDisplayed();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28868", ex);
            }
        }

        /// <summary>
        /// Searches the specified collections for <see cref="ToolStrip"/> objects containing
        /// <see cref="ToolStripSeparator"/> objects and hides the separators if they
        /// are the last item in the list or proceeded by another separator. If no visible
        /// items are left in the <see cref="ToolStrip"/> then the <see cref="ToolStrip"/>
        /// will be hidden.
        /// </summary>
        /// <param name="controls">The control collection to search for <see cref="ToolStrip"/> and
        /// <see cref="ToolStripSeparator"/>.</param>
        public static void HideUnnecessaryToolStripSeparators(
            System.Windows.Forms.Control.ControlCollection controls)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI30186",
                    _OBJECT_NAME);

                // Iterate each control looking for a toolstrip
                foreach (Control control in controls)
                {
                    ToolStrip toolStrip = control as ToolStrip;
                    if (toolStrip != null)
                    {
                        // Add each visible item to a temporary collection
                        List<ToolStripItem> items = new List<ToolStripItem>();
                        foreach (ToolStripItem item in toolStrip.Items)
                        {
                            if (item.Visible)
                            {
                                items.Add(item);
                            }
                        }

                        // Iterate each item in the toolstrip looking for seperators
                        for (int i = 0; i < items.Count; i++)
                        {
                            ToolStripItem item = items[i];
                            if (item is ToolStripSeparator)
                            {
                                // Check if either first item, last item, or
                                // followed by seperator
                                if (i == 0 || i + 1 == items.Count
                                    || items[i + 1] is ToolStripSeparator)
                                {
                                    // No item to the right or the next item is a separator
                                    // no need to see this separator so hide it
                                    item.Visible = false;
                                }
                            }
                        }

                        // Check for any remaining visible items
                        bool visibleItem = false;
                        foreach (ToolStripItem item in items)
                        {
                            if (item.Visible)
                            {
                                visibleItem = true;
                                break;
                            }
                        }

                        // No visible items left in the toolstrip, hide it
                        if (!visibleItem)
                        {
                            // Hide the toolstrip
                            toolStrip.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30187", ex);
            }
        }

        /// <summary>
        /// Helper method to correctly load persisted <see cref="ToolStrip"/> objects.
        /// <para><b>Note:</b></para>
        /// Do not call this method before <see cref="Form.Load"/>. You can call
        /// it from <see cref="Form.Load"/>.
        /// This code is based on:
        /// http://social.msdn.microsoft.com/forums/en-US/winforms/thread/656f5332-610d-42c3-ae2d-0ffb84a74b34/
        /// For related bug in ToolStripManager see:
        /// http://connect.microsoft.com/VisualStudio/feedback/details/128042/toolstripmanager-loadsettings-does-not-restore-toolstrip-locations
        /// </summary>
        /// <param name="container">The <see cref="ToolStripContainer"/> to
        /// load <see cref="ToolStrip"/> items into.</param>
        public static void ToolStripManagerLoadHelper(ToolStripContainer container)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI30199",
                    _OBJECT_NAME);

                // Load the toolstrip settings
                ToolStripManager.LoadSettings(container.ParentForm);

                // Cache references to the top and bottom panel
                ToolStripPanel top = container.TopToolStripPanel;
                using (ToolStripPanel temp = new ToolStripPanel())
                {
                    container.ParentForm.Controls.Add(temp);

                    // Iterate the top toolstrip panel. For any toolstrip on the top panel
                    // move it to the bottom panel.
                    for (int i = 0; i < top.Controls.Count; i++)
                    {
                        ToolStrip item = top.Controls[i] as ToolStrip;
                        if (item != null)
                        {
                            // Move control to the bottom toolstrip panel
                            // decrement i since moving the control modifies
                            // the top control collection
                            item.Parent = temp;
                            i--;
                        }
                    }

                    // Now reload the toolstrips, this should move them to the correct location
                    ToolStripManager.LoadSettings(container.ParentForm);

                    container.ParentForm.Controls.Remove(temp);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30200", ex);
            }
        }

        #endregion Methods
    }
}
