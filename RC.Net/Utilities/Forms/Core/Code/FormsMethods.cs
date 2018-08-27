using Extract.Licensing;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TD.SandDock;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// The Extract color scheme.
    /// </summary>
    public static class ExtractColors
    {
        public static readonly Color Blue = Color.FromArgb(0, 175, 249);

        public static readonly Color LightBlue = ControlPaint.LightLight(ExtractColors.Blue);

        public static readonly Color LightLightBlue = ControlPaint.LightLight(ExtractColors.LightBlue);

        public static readonly Color Orange = Color.FromArgb(249, 133, 0);

        public static readonly Color LightOrange = ControlPaint.LightLight(ExtractColors.Orange);

        public static readonly Color LightLightOrange = ControlPaint.LightLight(ExtractColors.LightOrange);

        public static readonly Color Gray = Color.FromArgb(216, 216, 216);

        public static readonly Color White = Color.White;
    }

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

        #region Delegates

        /// <summary>
        /// Indicates whether the <see paramref="control"/> qualifies for a particular need.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to check for qualification.</param>
        /// <returns><see langword="true"/> if the control qualifies, otherwise,
        /// <see langword="false"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public delegate bool ControlQualifier(Control control);

        #endregion Delegates

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
        /// <para><b>Note</b></para>
        /// In most situations it is advisable to invalidate or redraw controls after unlocking
        /// updates otherwise changes to the control that occured while it was locked will not be
        /// displayed.
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
        /// Gets which scrollbars are visible on a control
        /// </summary>
        /// <param name="control">The control to examine</param>
        public static ScrollBars GetVisibleScrollbars(Control control)
        {
            try
            {
                return NativeMethods.GetVisibleScrollbars(control);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46221");
            }
        }

        /// <summary>
        /// Searches the specified collections for <see cref="ToolStrip"/> objects containing
        /// <see cref="ToolStripSeparator"/> objects and removes the separators if they
        /// are the last item in the list or proceeded by another separator. If no visible
        /// items are left in the <see cref="ToolStrip"/> then the <see cref="ToolStrip"/>
        /// will be hidden.
        /// </summary>
        /// <param name="controls">The control collection to search for <see cref="ToolStrip"/> and
        /// <see cref="ToolStripSeparator"/>.</param>
        public static void RemoveUnnecessaryToolStripSeparators(Control.ControlCollection controls)
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
                        if (!RemoveUnnecessaryToolStripSeparators(toolStrip.Items))
                        {
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
        /// Searches the specified <see cref="ToolStripDropDown"/> and hides the separators if they
        /// are the last item in the list or proceeded by another separator. If no visible
        /// items are left in the <see cref="ToolStripDropDown"/> then this method will
        /// return <see langword="false"/>.
        /// </summary>
        /// <param name="toolStrip">The <see cref="ToolStrip"/> to search for <see cref="ToolStripSeparator"/>.
        /// </param>
        public static bool RemoveUnnecessaryToolStripSeparators(ToolStrip toolStrip)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32573",
                    _OBJECT_NAME);

                return RemoveUnnecessaryToolStripSeparators(toolStrip.Items);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32574");
            }
        }

        /// <summary>
        /// Removes any unnecessary toolstrip items and returns <see langword="true"/>
        /// if there are still visible items left in the collection.
        /// </summary>
        /// <param name="items">The collection to check for separators to hide.</param>
        /// <returns><see langword="true"/> if there are still visible items in the
        /// collection; <see langword="false"/> otherwise.</returns>
        static bool RemoveUnnecessaryToolStripSeparators(ToolStripItemCollection items)
        {
            // Add each visible item to a temporary collection
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var toolStripMenu = item as ToolStripMenuItem;
                if (toolStripMenu != null)
                {
                    if (toolStripMenu.DropDownItems.Count > 0)
                    {
                        // Hide empty separators in the underlying menu
                        if (!RemoveUnnecessaryToolStripSeparators(toolStripMenu.DropDownItems))
                        {
                            // Remove this item and start over (we may be able to remove a
                            // separator to the left of this item now, so need to start
                            // at the beginning)
                            items.RemoveAt(i);
                            i = -1;
                        }
                    }
                }
                else if (item is ToolStripSeparator)
                {
                    // If this is the first item, the last item, or the next item is
                    // a separator then this separator is unnecessary so remove it
                    if (i == 0 || (i + 1 == items.Count) || items[i + 1] is ToolStripSeparator)
                    {
                        // After removing this item, just need to check to the left again
                        items.RemoveAt(i);
                        i--;
                    }
                }
            }

            return items.Count > 0;
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
                    // move it to a temporary panel.
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

                    // [DotNetRCAndUtils:673]
                    // If any toolstrips remain in the temporary ToolStripPanel (such as when the
                    // application's user.config file doesn't exist) move them back to the top
                    // toolstrip container manually.
                    if (temp.Controls.Count > 0)
                    {
                        while (temp.Controls.Count > 0)
                        {
                            ((ToolStrip)temp.Controls[0]).Parent = top;
                        }

                        container.PerformLayout();
                    }

                    container.ParentForm.Controls.Remove(temp);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30200", ex);
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
                CenterFormInRectangle(formToCenter, formToCenterIn.DesktopBounds);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22220", ex);
            }
        }

        /// <summary>
        /// Centers the <paramref name="formToCenter"/> <see cref="Form"/>
        /// in the <paramref name="rectangleToCenterIn"/> <see cref="Form"/>.
        /// </summary>
        /// <param name="formToCenter">The <see cref="Form"/> to be centered.</param>
        /// <param name="rectangleToCenterIn">The <see cref="Rectangle"/> to be centered on.</param>
        public static void CenterFormInRectangle(Form formToCenter, Rectangle rectangleToCenterIn)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23144",
                    _OBJECT_NAME);

                // Get the location and size of the parent form
                Point formToCenterInLocation = rectangleToCenterIn.Location;
                Size formToCenterInSize = rectangleToCenterIn.Size;

                // Compute and set the location for this form
                formToCenter.Location = new Point(
                    formToCenterInLocation.X + ((formToCenterInSize.Width / 2) - (formToCenter.Width / 2)),
                    formToCenterInLocation.Y + ((formToCenterInSize.Height / 2) - (formToCenter.Height / 2)));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31383", ex);
            }
        }
        
        /// <summary>
        /// Removes the specified collection of <see cref="ToolStripItem"/> from their
        /// parent <see cref="ToolStrip"/> containers and disposes of them.
        /// </summary>
        /// <param name="items">One or more <see cref="ToolStripItem"/> to remove
        /// from their parent controls and dispose.</param>
        public static void RemoveAndDisposeToolStripItems(params ToolStripItem[] items)
        {
            try
            {
                RemoveAndDisposeToolStripItems((IEnumerable<ToolStripItem>)items);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32605");
            }
        }

        /// <summary>
        /// Removes the specified collection of <see cref="ToolStripItem"/> from their
        /// parent <see cref="ToolStrip"/> containers and disposes of them.
        /// </summary>
        /// <param name="items">The collection of <see cref="ToolStripItem"/> to remove
        /// from their parent controls and dispose.</param>
        public static void RemoveAndDisposeToolStripItems(IEnumerable<ToolStripItem> items)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32575",
                    _OBJECT_NAME);

                foreach (var item in items)
                {
                    RemoveAndDisposeToolStripItemInternal(item);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32576");
            }
        }

        /// <summary>
        /// Removes the specified <see cref="ToolStripItem"/> item from its parent
        /// <see cref="ToolStrip"/>, disposes the item.
        /// </summary>
        /// <param name="item">The item to remove and dispose.</param>
        static void RemoveAndDisposeToolStripItemInternal(ToolStripItem item)
        {
            ToolStrip owner = item.Owner;
            if (owner == null)
            {
                var temp = item.OwnerItem;
                if (temp == null || temp.Owner == null)
                {
                    return;
                }
                owner = temp.Owner;
            }

            owner.Items.Remove(item);
            item.Dispose();
        }

        /// <summary>
        /// Executes the <see paramref="action"/> in the UI thread of the provided
        /// <see paramref="control"/> via the message queue and blocks until the call is complete.
        /// If an exception is thrown, it will be thrown out on the calling thread even if the
        /// calling thread is different than the UI thread.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> that should execute the
        /// <see paramref="action"/>.</param>
        /// <param name="action">The <see cref="Action"/></param>
        public static void ExecuteInUIThread(Control control, Action action)
        {
            try
            {
                ExtractException thrownException = null;

                // Invoke to avoid modifying the imageViewer from outside the UI thread. Use begin
                // invoke so the operation isn't executed in the middle of another UI event.
                IAsyncResult result = control.BeginInvoke((MethodInvoker)(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        thrownException = ex.AsExtract("ELI32612");
                    }
                }));

                // If the control is running on a different thread, we can simply wait for the
                // invocation to complete.
                if (control.InvokeRequired)
                {
                    result.AsyncWaitHandle.WaitOne();
                }
                // If the control is running on this thread, we need to run the event loop while we wait
                // in order for the method to be called.
                else
                {
                    while (!result.AsyncWaitHandle.WaitOne(0))
                    {
                        Application.DoEvents();
                    }
                }

                if (thrownException != null)
                {
                    throw thrownException;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32613");
            }
        }

        /// <summary>
        /// Invokes the specified <see paramref="action"/> asynchronously within a try/catch handler
        /// that will display any exceptions.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> on which the <see paramref="action"/>
        /// should be invoked.</param>
        /// <param name="eliCode">The ELI code to associate with any exception.</param>
        /// <param name="action">The <see cref="Action"/> to be invoked.</param>
        /// <param name="displayExceptions"><see langword="true"/> to display any exception caught;
        /// <see langword="false"/> to log instead.</param>
        /// <param name="exceptionAction">A second action that should be executed in the case of an
        /// exception an exception in <see paramref="action"/>.</param>
        public static void BeginInvoke(Control control, string eliCode, Action action,
            bool displayExceptions = true, Action<Exception> exceptionAction = null)
        {
            try
            {
                control.BeginInvoke((MethodInvoker)(() =>
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            if (exceptionAction != null)
                            {
                                try
                                {
                                    exceptionAction(ex);
                                }
                                catch (Exception errorAction)
                                {
                                    errorAction.ExtractLog("ELI39994");
                                }
                            }

                            if (displayExceptions)
                            {
                                ex.ExtractDisplay(eliCode);
                            }
                            else
                            {
                                ex.ExtractLog(eliCode);
                            }
                        }
                    }));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35350");
            }
        }

        /// <summary>
        /// Starts or stops the specified <see paramref="window"/>'s title bar and taskbar button from
        /// flashing.
        /// </summary>
        /// <param name="start"><see langword="true"/> to start flashing; <see langword="false"/> to
        /// stop flashing.</param>
        /// <param name="window">The <see cref="IWin32Window"/> that is to flash.</param>
        /// <param name="stopOnActivate"><see langword="true"/> to stop flashing when the window is
        /// activated (brought to the foreground).</param>
        public static void FlashWindow(IWin32Window window, bool start, bool stopOnActivate)
        {
            try
            {
                NativeMethods.FlashWindow(window, start, stopOnActivate);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33220");
            }
        }

        /// <summary>
        /// Restores the specified <see paramref="Form"/> if it is currently minimized.
        /// </summary>
        /// <param name="form">The <see cref="Form"/> to restore.</param>
        public static void Restore(Form form)
        {
            try
            {
                NativeMethods.Restore(form);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35795");
            }
        }

        /// <summary>
        /// Allows the user to select a folder using the folder browser.
        /// </summary>
        /// <param name="description">The text to display over the selection control.</param>
        /// <param name="initialFolder">The initial folder for the folder browser.</param>
        /// <param name="fileFilter">The file type filter.</param>
        /// <param name="pickFolder">Whether to allow folders to be selected rather than files.</param>
        /// <param name="ensurePathExists">Whether to validate that the picked path exists.</param>
        /// <param name="ensureFileExists">Whether to validate that the picked file exists.</param>
        /// <returns>The result of the user's selection or <see langword="null"/> if the user
        /// canceled the dialog.</returns>
        public static string BrowseForFileOrFolder(string description, string initialFolder,
            bool pickFolder, string fileFilter, bool multipleSelect, bool ensurePathExists, bool ensureFileExists)
        {
            try
            {
                using (CommonOpenFileDialog browser = new CommonOpenFileDialog
                {
                    IsFolderPicker = pickFolder,
                    Multiselect = multipleSelect,
                    EnsurePathExists = ensurePathExists,
                    EnsureFileExists = ensureFileExists
                })
                {
                    // Set the initial folder if necessary
                    if (!string.IsNullOrEmpty(initialFolder))
                    {
                        browser.InitialDirectory = initialFolder;
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        // Set the description
                        browser.Title = description;
                    }
                    else if (pickFolder)
                    {
                        browser.Title = "Please select a folder";
                    }
                    else
                    {
                        browser.Title = "Please select a file";
                    }

                    // Set the filter text and the initial filter
                    if (!string.IsNullOrEmpty(fileFilter))
                    {
                        var filters = fileFilter.Split(new[] { '|' });
                        for (int i = 1; i < filters.Length; i += 2)
                        {
                            var filter = new CommonFileDialogFilter(filters[i - 1], filters[i]) { ShowExtensions = false };
                            browser.Filters.Add(filter);
                        }
                    }
                    if (browser.Filters.Any())
                    {
                        browser.DefaultExtension = browser.Filters[0].Extensions.FirstOrDefault();
                    }

                    // Show the dialog
                    var result = browser.ShowDialog();
                    if (result == CommonFileDialogResult.Ok)
                    {
                        // Return the selected path.
                        return browser.FileName;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44899");
            }
        }

        /// <summary>
        /// Allows the user to select a folder using the folder browser.
        /// </summary>
        /// <param name="description">The text to display over the folder selection contro.</param>
        /// <param name="initialFolder">The initial folder for the folder browser.</param>
        /// <returns>The result of the user's selection or <see langword="null"/> if the user
        /// canceled the dialog.</returns>
        public static string BrowseForFolder(string description, string initialFolder)
        {
            try
            {
                return BrowseForFileOrFolder(description, initialFolder, true, null, false, true, false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34029");
            }
        }

        /// <summary>
        /// Allows the user to select a single, exsiting file using the file dialog.
        /// </summary>
        /// <param name="fileFilter">The text to display over the file selection control.</param>
        /// <param name="initialFolder">The initial folder for the file browser.</param>
        /// <returns>The result of the user's selection or <see langword="null"/> if the user
        /// canceled the dialog.</returns>
        public static string BrowseForFile(string fileFilter, string initialFolder)
        {
            try
            {
                return BrowseForFileOrFolder(null, initialFolder, false, fileFilter, false, true, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34028");
            }
        }

        /// <summary>
        /// Moves the specified child controls from specified source to the specified destination.
        /// Controls will be added in a single row from left to right.
        /// </summary>
        /// <param name="sourceControl">The <see cref="Control"/> that currently contains the
        /// controls to be moved.</param>
        /// <param name="destinationControl">The <see cref="Control"/> that is to contain the
        /// controls to be moved.</param>
        /// <param name="controlsToMove">The <see cref="Control"/>s that are to be moved.</param>
        public static void MoveControls(Control sourceControl, Control destinationControl,
            params Control[] controlsToMove)
        {
            try
            {
                // Keep track of the position to try to add the next control.
                Point locationToAdd = new Point(0, 0);

                // If the destination already has child controls use the right side of the last control
                // as the initial location to add.
                foreach (Control control in destinationControl.Controls)
                {
                    Point location = control.Location;
                    location.Offset(control.Width, 0);

                    if ((location.Y > locationToAdd.Y) ||
                        (location.Y == locationToAdd.Y && location.X > locationToAdd.X))
                    {
                        locationToAdd = location;
                    }
                }

                // Add each control, updating the location to add as we go.
                foreach (Control control in controlsToMove)
                {
                    sourceControl.Controls.Remove(control);

                    control.Location = locationToAdd;
                    destinationControl.Controls.Add(control);

                    locationToAdd = control.Location;
                    locationToAdd.Offset(control.Width, 0);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34986");
            }
        }

        /// <summary>
        /// Moves the specified <see paramref="sandDockManager"/> and all of its
        /// <see cref="DockContainer"/>s and <see cref="DockControl"/>s to the specified 
        /// <see paramref="destinationControl"/> in a new form.
        /// <para><b>Note</b></para>
        /// This method will dispose of the supplied <see paramref="sandDockManager"/> and its
        /// <see cref="DockControl"/>(s) in the process.
        /// This method does not support moving individual dock containers to separate destination
        /// controls.
        /// </summary>
        /// <param name="sandDockManager">The <see cref="SandDockManager"/> to move.</param>
        /// <param name="destinationControl">The destination <see cref="Control"/> into which the
        /// <see cref="DockContainer"/>(s) should be added.</param>
        /// <returns>The new <see cref="SandDockManager"/>. The original
        /// <see paramref="sandDockManager"/> will have been disposed of and should no longer be
        /// used.</returns>
        public static SandDockManager MoveSandDockToNewForm(SandDockManager sandDockManager,
            Control destinationControl)
        {
            try
            {
                // Retrieve the current layout so it can be restored once all dockable windows have
                // been moved to the new form.
                string layout = sandDockManager.GetLayout();

                // The only way I can get controls to show up in the new form is if I re-create a
                // new SandDockManager and DockContainers in the new form. If the dock controls are
                // moved into an existing SandDockManager and DockContainers, for whatever reason
                // they don't show up.
                SandDockManager newSandDockManager = new SandDockManager();
                newSandDockManager.OwnerForm = (Form)destinationControl.TopLevelControl;

                var controlsToMove = sandDockManager.GetDockControls();

                // sandDockManager.GetDockContainers() will not return containers for which there
                // are no open windows. Therefore, open all windows prior to iterating the dock
                // containers. newSandDockManager.SetLayout() will restore the proper control state.
                foreach (DockControl control in controlsToMove.Where(control => !control.IsOpen))
                {
                    control.Open();
                }

                // Iterate each dock container to re-create a copy for the new form. Call order
                // seems to be very important in this loop:
                // 1) Create the new container
                // 2) Move the controls into it.
                // 3) Set the new layout system and manager.
                // 4) Add the new dock container into destinationControl.
                foreach (DockContainer dockContainer in sandDockManager.GetDockContainers())
                {
                    Control oldParentContainer = dockContainer.Parent;
                    DockContainer newDockContainer = new DockContainer();
                    DockControl[] dockControls = controlsToMove
                        .Where(control => control.Parent == dockContainer)
                        .ToArray();

                    MoveControls(dockContainer, newDockContainer, dockControls);

                    newDockContainer.LayoutSystem = dockContainer.LayoutSystem;
                    newDockContainer.Manager = newSandDockManager;
                    destinationControl.Controls.Add(newDockContainer);

                    oldParentContainer.Controls.Remove(dockContainer);
                    dockContainer.Dispose();
                }

                // Restore the same layout that existed in the last form.
                newSandDockManager.SetLayout(layout);

                sandDockManager.Dispose();

                return newSandDockManager;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34987");
            }
        }

        /// <summary>
        /// Makes the form invisible.
        /// </summary>
        /// <param name="form">The form to make invisible.</param>
        public static void MakeFormInvisible(Form form)
        {
            try
            {
                // Makes form invisible.
                form.Opacity = 0;

                // ...and we don't want it to appear in the task bar
                form.ShowInTaskbar = false;

                // ...and we don't want it to have a close button
                // (for sanity, not sure if it would be possible to use it with opacity = 0)
                form.ControlBox = false;

                // ...and we don't want the user to be able to alt-tab to it
                form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                
                // ...and we don't want the user to be able to do anything in it.
                form.Enabled = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35078");
            }
        }

        /// <summary>
        /// Gets the focused control by traversing all nested ActiveControls.
        /// </summary>
        /// <param name="containerControl">The <see cref="ContainerControl"/> for which the focused
        /// control is needed.</param>
        /// <returns>The <see cref="Control"/> that has input focus.</returns>
        public static Control GetFocusedControl(ContainerControl containerControl)
        {
            try
            {
                ContainerControl childContainer = containerControl.ActiveControl as ContainerControl;
                if (childContainer == null)
                {
                    return containerControl.ActiveControl;
                }
                else
                {
                    return GetFocusedControl(childContainer);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35848");
            }
        }

        /// <summary>
        /// Gets an enumeration of <see paramref="control"/> and all child controls in order of
        /// <see cref="Control.TabIndex"/>.
        /// </summary>
        /// <param name="control">The root of all <see cref="Control"/>s to be returned.</param>
        /// <returns>The enumeration of all <see cref="Control"/>s.</returns>
        public static IEnumerable<Control> GetAllControls(Control control)
        {
            yield return control;

            foreach (Control descendant in control.Controls
                .OfType<Control>().OrderBy(child => child.TabIndex)
                .SelectMany(child => GetAllControls(child)))
            {
                yield return descendant;
            }
        }

        /// <summary>
        /// Finds the next <see cref="Control"/> that is either <see paramref="root"/> or one of its
        /// descendants that matches the specified <see paramref="qualifier"/>.
        /// </summary>
        /// <param name="root">The resulting control must be this <see cref="Control"/> or one of
        /// its descendants.</param>
        /// <param name="current">If not <see paramref="null"/>, the next match after this one in
        /// the squence will be the one returned.</param>
        /// <param name="forward"><see langword="true"/> to search in order of
        /// <see cref="Control.TabIndex"/><see langword="false"/> to search in reverse order.
        /// </param>
        /// <param name="wrap"><see langword="true"/> wrap when the end of the sequence is reached;
        /// otherwise, <see paramref="false"/>. This parameter is ignored if
        /// <see paramref="current"/> is <see langword="null"/>.
        /// </param>
        /// <param name="qualifier">If not <see langword="null"/>, this
        /// <see cref="ControlQualifier"/> must be <see langword="true"/> for any return value.
        /// </param>
        /// <returns>The next <see cref="Control"/>.</returns>
        public static Control FindNextControl(Control root, Control current, bool forward,
            bool wrap, ControlQualifier qualifier)
        {
            try
            {
                // If not specified, use the top-level control as the root control.
                if (root == null)
                {
                    root = current.TopLevelControl;
                }

                // Get an enumeration of all controls in tab order (reversed if !forward).
                var allControls = GetAllControls(root);
                if (!forward)
                {
                    allControls = allControls.Reverse();
                }

                // Find the next qualifying control after current.
                Control nextQualifyingControl = allControls
                    .SkipWhile(c => current != null && c != current)
                    .Skip(1)
                    .Where(c => (qualifier == null) || qualifier(c))
                    .FirstOrDefault();

                // If a control was found, or not wrapping, return the result.
                if (nextQualifyingControl != null || !wrap || current == null)
                {
                    return nextQualifyingControl;
                }
                else
                {
                    // Otherwise, return the first from the beginning of the enumeration.
                    return allControls
                        .Where(c => (qualifier == null) || qualifier(c))
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35850");
            }
        }

        /// <summary>
        /// Gets a <see cref="IWin32Window"/> from the <see paramref="handle"/> which can be for
        /// either a managed WinForms control or a unmanaged window.
        /// </summary>
        /// <param name="handle">The window handle</param>
        /// <returns>A <see cref="IWin32Window"/> representing the <see paramref="handle"/>.
        /// </returns>
        public static IWin32Window WindowFromHandle(IntPtr handle)
        {
            try
            {
                IWin32Window window = Control.FromHandle(handle);
                if (window == null)
                {
                    window = new WindowWrapper(handle);
                }

                return window;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38059");
            }
        }

        /// <summary>
        /// Retrieves a handle to the specified window's parent or owner.
        /// </summary>
        /// <param name="windowHandle">Handle to the window whose parent window handle is to be
        /// retrieved.
        /// </param>
        /// <returns>If the window is a child window, the return value is a handle to the parent
        /// window. If the window is a top-level window, the return value is a handle to the owner
        /// window. If the window is a top-level unowned window or if the function fails, the return
        /// value is <see cref="IntPtr.Zero"/>.</returns>
        public static IntPtr GetParentWindowHandle(IntPtr windowHandle)
        {
            try
            {
                return NativeMethods.GetParent(windowHandle);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI39372", ex);
            }
        }

        /// <summary>
        /// Returns the <see cref="Control"/> associated with a mouse click event.
        /// (<see cref="WindowsMessage.LeftButtonDown"/>, 
        ///  <see cref="WindowsMessage.MiddleButtonDown"/>,
        ///  <see cref="WindowsMessage.RightButtonDown"/>)
        /// </summary>
        /// <param name="message">The <see cref="Message"/> generated by the mouse event.</param>
        /// <returns>The <see cref="Control"/> associated with the click or <see langword="null"/>
        /// if no control is found at the click location.</returns>
        public static Control GetClickedControl(Message message)
        {
            try
            {
                ExtractException.Assert("ELI39373", "Unexpected message!",
                    (message.Msg == WindowsMessage.LeftButtonDown ||
                     message.Msg == WindowsMessage.MiddleButtonDown ||
                     message.Msg == WindowsMessage.RightButtonDown));

                // Obtain the control that was clicked (may be a container rather than the specific
                // control.
                Control clickedControl = Control.FromHandle(message.HWnd);

                // [DataEntry:354]
                // Sometimes the window handle may be a child of a .Net control (such as the edit
                // box of a combo box). In this case, a Control will not be created from the handle.
                // Use the Win32 API to find the first ancestor that is a .Net control.
                while (clickedControl == null && message.HWnd != IntPtr.Zero)
                {
                    message.HWnd = GetParentWindowHandle(message.HWnd);

                    clickedControl = Control.FromHandle(message.HWnd);
                }

                if (clickedControl != null)
                {
                    // Get the position of the mouse in screen coordinates.
                    Point mousePosition = new Point((int)((uint)message.LParam & 0x0000FFFF),
                                                    (int)((uint)message.LParam & 0xFFFF0000) >> 16);
                    mousePosition = clickedControl.PointToScreen(mousePosition);

                    Control childControl = clickedControl;

                    // Loop down through all the control's descendants at the mouse position to try
                    // to find a control at the clicked location. 
                    while (true)
                    {
                        childControl = clickedControl.GetChildAtPoint(
                            clickedControl.PointToClient(mousePosition));

                        if (childControl == null)
                        {
                            return clickedControl;
                        }
                        else
                        {
                            clickedControl = childControl;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39374");
            }
        }

        /// <summary>
        /// Gets a recursive string listing of <see paramref="control"/>'s browsable properties
        /// including all descendant <see cref="Control"/>s or <see cref="DataGridViewBand"/>s.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> whose property values are to be
        /// listed.</param>
        /// <param name="output">The <see cref="StringBuilder"/> to which the property values
        /// should be written.</param>
        /// <param name="levelsDeep">The number of levels deep from the initially specified control
        /// this <see paramref="control"/> is. (used to drive indentation).</param>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "levelsDeep+1")]
        public static void GetControlPropertyListing(Control control, StringBuilder output,
            int levelsDeep)
        {
            try
            {
                if (output.Length > 0)
                {
                    output.AppendLine();
                }

                string margin = new string('\t', levelsDeep);

                output.Append(margin);
                output.Append("<<< ");
                output.Append(control.Name);
                output.AppendLine(" >>>");

                GetObjectPropertyValues(control, output, margin);

                // Iterate the rows/columns of a DataGridView as if they were child controls.
                var dataGridView = control as DataGridView;
                if (dataGridView != null)
                {
                    foreach (var column in dataGridView.Columns.Cast<DataGridViewColumn>()
                        .Where(column => !string.IsNullOrEmpty(column.Name)))
                    {
                        GetDataGridViewBandPropertyListing(column, output, levelsDeep + 1);
                    }

                    foreach (var row in dataGridView.Rows.Cast<DataGridViewRow>()
                        .Where(row => !string.IsNullOrEmpty(
                            (TypeDescriptor.GetProperties(row)["Name"].GetValue(row) ?? "").ToString())))
                    {
                        GetDataGridViewBandPropertyListing(row, output, levelsDeep + 1);
                    }
                }

                // Recursively dump the properties of child controls.
                foreach (var child in control.Controls.Cast<Control>()
                    .Where(child => !string.IsNullOrWhiteSpace(child.Name))
                    .OrderBy(child => child.TabIndex))
                {
                    GetControlPropertyListing(child, output, levelsDeep + 1);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39375");
            }
        }

        /// <summary>
        /// Gets a string listing of a <see cref="DataGridViewBand"/>.
        /// </summary>
        /// <param name="band">The <see cref="DataGridViewBand"/> whose browsable property values
        /// are to be listed.</param>
        /// <param name="output">The <see cref="StringBuilder"/> to which the property values
        /// should be written.</param>
        /// <param name="levelsDeep">The number of levels deep from the initially specified control
        /// this <see paramref="control"/> is (used to drive indentation).</param>
        public static void GetDataGridViewBandPropertyListing(DataGridViewBand band,
            StringBuilder output, int levelsDeep)
        {
            try
            {
                output.AppendLine();

                string margin = new string('\t', levelsDeep);

                output.Append(margin);
                output.Append("<<< ");
                output.Append(TypeDescriptor.GetProperties(band)["Name"].GetValue(band));
                output.AppendLine(" >>>");

                GetObjectPropertyValues(band, output, margin);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39376");
            }
        }

        /// <summary>
        /// Gets a string listing of the browsable property values of <see paramref="targetObject"/>.
        /// </summary>
        /// <param name="targetObject">The <see cref="object"/> whose browsable property values are
        /// to be listed.</param>
        /// <param name="output">The <see cref="StringBuilder"/> to which the property values
        /// should be written.</param>
        /// <param name="margin">The whitespace to be prepended out every line of text.</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
        public static void GetObjectPropertyValues(Object targetObject, StringBuilder output,
            string margin)
        {
            try
            {
                foreach (var property in TypeDescriptor.GetProperties(targetObject)
                        .OfType<PropertyDescriptor>()
                        .Where(property => property.IsBrowsable)
                        .OrderBy(property => property.Name))
                {
                    output.Append(margin);
                    output.Append(property.Name);
                    output.Append(": ");
                    object value = property.GetValue(targetObject);
                    string valueString = (value ?? "null").ToString();
                    // For any multi-line property value, indent another level.
                    if (valueString.Contains(Environment.NewLine))
                    {
                        output.AppendLine();
                        foreach (string line in valueString.Split(new[] { "\r\n" },
                            StringSplitOptions.None))
                        {
                            output.Append(margin + "\t");
                            output.AppendLine(line);
                        }
                    }
                    else
                    {
                        output.AppendLine(valueString);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39377");
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Provides extension methods for the <see cref="FormsMethods"/> class.
    /// </summary>
    public static class FormsExtensionMethods
    {
        /// <summary>
        /// Invokes the specified <see paramref="action"/> asynchronously within a try/catch handler
        /// that will display any exceptions.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> on which the <see paramref="action"/>
        /// should be invoked.</param>
        /// <param name="eliCode">The ELI code to associate with any exception.</param>
        /// <param name="action">The <see cref="Action"/> to be invoked.</param>
        /// <param name="displayExceptions"><see langword="true"/> to display any exception caught;
        /// <see langword="false"/> to log instead.</param>
        /// <param name="exceptionAction">A second action that should be executed in the case of an
        /// exception an exception in <see paramref="action"/>.</param>
        public static void SafeBeginInvoke(this Control control, string eliCode, Action action,
            bool displayExceptions = true, Action<Exception> exceptionAction = null)
        {
            FormsMethods.BeginInvoke(control, eliCode, action, displayExceptions, exceptionAction);
        }

        /// <summary>
        /// Moves the specified child controls from specified source to the specified destination.
        /// Controls will be added in a single row from left to right.
        /// </summary>
        /// <param name="sourceControl">The <see cref="Control"/> that currently contains the
        /// controls to be moved.</param>
        /// <param name="destinationControl">The <see cref="Control"/> that is to contain the
        /// controls to be moved.</param>
        /// <param name="controlsToMove">The <see cref="Control"/>s that are to be moved.</param>
        public static void MoveControls(this Control sourceControl, Control destinationControl,
            params Control[] controlsToMove)
        {
            FormsMethods.MoveControls(sourceControl, destinationControl, controlsToMove);
        }

        /// <summary>
        /// Moves the specified <see paramref="sandDockManager"/> and all of its
        /// <see cref="DockContainer"/>s and <see cref="DockControl"/>s to the specified 
        /// <see paramref="destinationControl"/> in a new form.
        /// <para><b>Note</b></para>
        /// This method will dispose of the supplied <see paramref="sandDockManager"/> and its
        /// <see cref="DockControl"/>(s) in the process.
        /// This method does not support moving individual dock containers to separate destination
        /// controls.
        /// </summary>
        /// <param name="sandDockManager">The <see cref="SandDockManager"/> to move.</param>
        /// <param name="destinationControl">The destination <see cref="Control"/> into which the
        /// <see cref="DockContainer"/>(s) should be added.</param>
        /// <returns>The new <see cref="SandDockManager"/>. The original
        /// <see paramref="sandDockManager"/> will have been disposed of and should no longer be
        /// used.</returns>
        public static SandDockManager MoveSandDockToNewForm(this SandDockManager sandDockManager,
            Control destinationControl)
        {
            return FormsMethods.MoveSandDockToNewForm(sandDockManager, destinationControl);
        }

        /// <summary>
        /// Makes the form invisible.
        /// </summary>
        /// <param name="form">The form to make invisible.</param>
        public static void MakeFormInvisible(this Form form)
        {
            FormsMethods.MakeFormInvisible(form);
        }

        /// <summary>
        /// Restores the specified <see paramref="Form"/> if it is currently minimized.
        /// </summary>
        /// <param name="form">The <see cref="Form"/> to restore.</param>
        public static void Restore(this Form form)
        {
            FormsMethods.Restore(form);
        }

        /// <summary>
        /// Gets the focused control by traversing all nested ActiveControls.
        /// </summary>
        /// <param name="containerControl">The <see cref="ContainerControl"/> for which the focused
        /// control is needed.</param>
        /// <returns>The <see cref="Control"/> that has input focus.</returns>
        public static Control GetFocusedControl(this ContainerControl containerControl)
        {
            return FormsMethods.GetFocusedControl(containerControl);
        }

        /// <summary>
        /// Gets an enumeration of <see paramref="control"/> and all child controls in order of
        /// <see cref="Control.TabIndex"/>.
        /// </summary>
        /// <param name="form">The <see cref="Form"/> for which all controls are needed.</param>
        /// <returns>The enumeration of all <see cref="Control"/>s.</returns>
        public static IEnumerable<Control> GetAllControls(this Control form)
        {
            return FormsMethods.GetAllControls(form);
        }

        /// <summary>
        /// Safely closes the form - if the call is from a thread that didn't create the form,
        /// uses Invoke.
        /// NOTE: This method can throw an Exception.
        /// </summary>
        /// <param name="control">The control to operate on</param>
        /// <param name="action">The action to perform on the control</param>
        public static void SafeInvoke(this Control control, Action action)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39240");
            }
        }

        /// <summary>
        /// Gets a recursive string listing of <see paramref="control"/>'s browsable properties
        /// including all descendant <see cref="Control"/>s or <see cref="DataGridViewBand"/>s.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> whose property values are to be
        /// listed.</param>
        /// <returns>A recursive string listing of the control's browsable properties.</returns>
        public static string GetPropertyListing(this Control control)
        {
            try
            {
                var sb = new StringBuilder();
                FormsMethods.GetControlPropertyListing(control, sb, 0);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39378");
            }
        }

        /// <summary>
        /// Gets Parent <see cref="Control"/>s, recursively
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to get ancestors of</param>
        /// <returns>An enumeration of ancestor <see cref="Control"/>s</returns>
        public static IEnumerable<Control> GetAncestors(this Control control)
        {
            var parent = control.Parent as Control;
            if (parent != null)
            {
                foreach (Control ancestor in GetSelfAndAncestors(parent))
                {
                    yield return ancestor;
                }
            }
        }

        /// <summary>
        /// Sets input focus to <see paramref="targetControl"/> which is a descendant of
        /// <see paramref="ancestorControl"/> activating any intermediate controls in the process.
        /// </summary>
        /// <param name="ancestorControl">The ancestor control to <see paramref="targetControl"/></param>
        /// <param name="targetControl">The control to be focused.</param>
        /// <returns><c>true</c> if the input focus request was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool FocusNestedControl(this Control ancestorControl, Control targetControl)
        {
            try
            {
                if (targetControl == null || !targetControl.CanFocus)
                {
                    return false;
                }

                var ancestors = targetControl.GetAncestors()
                    .TakeWhile(control => control != ancestorControl);

                Control parent = ancestorControl;
                foreach (Control next in ancestors.Reverse())
                {
                    var parentContainer = parent as ContainerControl;
                    if (parentContainer != null)
                    {
                        parentContainer.ActiveControl = next;
                    }

                    parent = next;
                }

                return targetControl.Focus();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41663");
            }
        }


        /// <summary>
        /// Helper method for <see cref="GetAncestors"/>
        /// </summary>
        /// <param name="control">The control to get ancestors of</param>
        /// <returns>This control and all ancestors</returns>
        private static IEnumerable<Control> GetSelfAndAncestors(Control control)
        {
            yield return control;
            var parent = control.Parent as Control;
            if (parent != null)
            {
                foreach (Control ancestor in GetSelfAndAncestors(parent))
                {
                    yield return ancestor;
                }
            }
        }

        /// <summary>
        /// Prompts for save
        /// </summary>
        /// <param name="owner">An <see cref="Win32Window"/> that will own the modal dialog box</param>
        /// <param name="dirty">Whether there are any changes to prompt about</param>
        /// <returns>Returns <see cref="DialogResult.Yes"/> if changes are to be saved,
        /// <see cref="DialogResult.No"/> if changes are not to be saved,
        /// and <see cref="DialogResult.Cancel"/> to abort and remain in the configuration dialog</returns>
        public static DialogResult PromptForSaveChanges(this IWin32Window owner, bool dirty)
        {
            try
            {
                if (dirty)
                {
                    return MessageBox.Show(owner,
                        "Changes have not been saved, would you like to save now?",
                        "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button3, 0);
                }

                return DialogResult.No;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45998");
            }
        }
    }

    /// <summary>
    /// Helper class that can be used in a using statement to lock control updating
    /// </summary>
    public class LockControlUpdates : IDisposable
    {
        #region Fields

        /// <summary>
        /// The control that has its updating locked
        /// </summary>
        Control _control;

        /// <summary>
        /// Indicates whether the control is locked or not
        /// </summary>
        bool _locked;

        /// <summary>
        /// Indicates whether the control should be invalidated when it is unlocked
        /// </summary>
        bool _invalidateOnUnlock;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LockControlUpdates"/> class.
        /// </summary>
        /// <param name="control">The control to lock.</param>
        public LockControlUpdates(Control control) :
            this(control, true, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockControlUpdates"/> class.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="initiallyLock">if set to <see langword="true"/> will intially lock
        /// the control.</param>
        public LockControlUpdates(Control control, bool initiallyLock)
            : this(control, initiallyLock, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockControlUpdates"/> class.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="initiallyLock">if set to <see langword="true"/> will intially lock
        /// the control.</param>
        /// <param name="invalidateOnUnlock">If <see langword="true"/> then
        /// <paramref name="control"/> will be invalidated after unlocking.</param>
        public LockControlUpdates(Control control, bool initiallyLock,
            bool invalidateOnUnlock)
        {
            try
            {
                if (control == null)
                {
                    throw new ArgumentNullException("control");
                }

                _control = control;
                _invalidateOnUnlock = invalidateOnUnlock;

                if (initiallyLock)
                {
                    Lock();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32012");
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Locks the control.
        /// </summary>
        public void Lock()
        {
            try
            {
                if (!_locked)
                {
                    FormsMethods.LockControlUpdate(_control, true);
                    _locked = !_locked;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32013");
            }
        }

        /// <summary>
        /// Unlocks the control
        /// </summary>
        public void Unlock()
        {
            try
            {
                if (_locked)
                {
                    FormsMethods.LockControlUpdate(_control, false);
                    _locked = !_locked;
                    if (_invalidateOnUnlock)
                    {
                        _control.Invalidate(true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32014");
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                if (_control != null)
                {
                    Unlock();
                    _control = null;
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Call the private Dispose(bool) helper and indicate 
            // that we are explicitly disposing
            this.Dispose(true);

            // Tell the garbage collector that the object doesn't require any
            // cleanup when collected since Dispose was called explicitly.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
