using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TD.SandDock;

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
        public static void BeginInvoke(Control control, string eliCode, Action action,
            bool displayExceptions = true)
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
                using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
                {
                    // Set the initial folder if necessary
                    if (!string.IsNullOrEmpty(initialFolder))
                    {
                        folderBrowser.SelectedPath = initialFolder;
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        // Set the folder browser description
                        folderBrowser.Description = description;
                    }

                    // Show the dialog
                    DialogResult result = folderBrowser.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // Return the selected folder path.
                        return folderBrowser.SelectedPath;
                    }

                    return null;
                }
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
                using (OpenFileDialog openFile = new OpenFileDialog())
                {
                    // Set the initial folder if necessary
                    if (!string.IsNullOrEmpty(initialFolder))
                    {
                        openFile.InitialDirectory = initialFolder;
                    }

                    // Set the filter text and the initial filter
                    if (!string.IsNullOrEmpty(fileFilter))
                    {
                        openFile.Filter = fileFilter;
                        openFile.FilterIndex = 0;
                        openFile.AddExtension = true;
                    }
                    else
                    {
                        openFile.AddExtension = false;
                    }

                    // Set multi-select to false
                    openFile.Multiselect = false;

                    // Require that both the path and file exist
                    openFile.CheckFileExists = true;
                    openFile.CheckPathExists = true;

                    // Show the dialog
                    DialogResult result = openFile.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // Return the selected file path.
                        return openFile.FileName;
                    }

                    return null;
                }
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
        public static void SafeBeginInvoke(this Control control, string eliCode, Action action,
            bool displayExceptions = true)
        {
            FormsMethods.BeginInvoke(control, eliCode, action, displayExceptions);
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
