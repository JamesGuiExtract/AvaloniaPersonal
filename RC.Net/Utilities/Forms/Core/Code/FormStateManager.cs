using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using TD.SandDock;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Manages the state of a <see cref="Form"/>'s user interface properties.
    /// <para><b>Note</b></para>
    /// Though the methods <see cref="SaveState"/> and <see cref="RestoreSavedState"/> allow for
    /// explicitly loading and saving the UI state information, this class will automatically save
    /// the form's state when the managed <see cref="Form"/>'s <see cref="Form.Closing"/> event is
    /// raised and restore the saved state when the managed <see cref="Form"/>'s
    /// <see cref="Form.Load"/> event is raised.
    /// </summary>
    public class FormStateManager : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FormStateManager).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="Form"/> for which state information is being managed.
        /// </summary>
        Form _form;

        /// <summary>
        /// The name of the file to which form properties will be maintained.
        /// </summary>
        string _persistenceFileName;

        /// <summary>
        /// The original value of <see cref="Form.FormBorderStyle"/> for the managed
        /// <see cref="Form"/>.
        /// </summary>
        FormBorderStyle _originalBorderStyle;

        /// <summary>
        /// The original value of <see cref="Form.SizeGripStyle"/> for the managed
        /// <see cref="Form"/>.
        /// </summary>
        SizeGripStyle _originalSizeGripStyle;

        /// <summary>
        /// The name of the <see cref="SandDockManager"/> for which state information should be
        /// maintained.
        /// </summary>
        SandDockManager _sandDockManager;

        /// <summary>
        /// Indicates whether state info for toolstrips should be maintained for the form.
        /// </summary>
        bool _manageToolStrips;

        /// <summary>
        /// The size and location of the form in screen pixels when it is in its normal 
        /// (unmaximized) state.
        /// </summary>
        Rectangle _bounds;

        /// <summary>
        /// The form's <see cref="FormWindowState"/>.
        /// </summary>
        FormWindowState _state;

        /// <summary>
        /// Indicates whether the form should be displayed in full screen mode.
        /// </summary>
        bool _fullScreen;

        /// <summary>
        /// Indicates whether full screen mode is supported by the managed <see cref="Form"/>.
        /// </summary>
        bool _supportsFullScreenMode = true;

        /// <summary>
        /// A reference count of the number of methods currently updating the <see cref="Form"/>'s
        /// state.
        /// </summary>
        int _updatingStateReferenceCount;

        /// <summary>
        /// A tab to indicate to users how to exit full screen mode.
        /// </summary>
        AutoHideScreenTab _fullScreenTab;

        /// <summary>
        /// Mutex used to serialize persistence of control and form layout.
        /// </summary>
        Mutex _layoutMutex;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="FormStateManager"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="FormStateManager"/> class.
        /// <para><b>Note</b></para>
        /// <see cref="FormStateManager"/> should not be created or used in design time.
        /// </summary>
        /// <param name="form">The <see cref="Form"/> whose state is to be managed.</param>
        /// <param name="persistenceFileName">The name of the file to which form properties will be
        /// maintained.</param>
        /// <param name="mutexName">Name for the mutex used to serialize persistence of the
        /// control and form layout.</param>
        /// <param name="manageToolStrips">If <see langword="true"/>, the form's
        /// <see cref="ToolStrip"/> will be persisted.</param>
        /// <param name="fullScreenTabText">If not <see langword="null"/> or empty, an
        /// <see cref="AutoHideScreenTab"/> will be displayed with the provided text that, if
        /// clicked, will exit full screen mode. If <see langword="null"/> full screen mode will not
        /// be supported and the persistence xml file will not contain the FullScreen attribute.
        /// </param>
        /// <throws><see cref="ExtractException"/> if instantiated at design-time.</throws>
        public FormStateManager(Form form, string persistenceFileName, string mutexName,
            bool manageToolStrips, string fullScreenTabText)
            : this(form, persistenceFileName,
                mutexName, null, manageToolStrips, fullScreenTabText)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormStateManager"/> class.
        /// <para><b>Note</b></para>
        /// <see cref="FormStateManager"/> should not be created or used in design time.
        /// </summary>
        /// <param name="form">The <see cref="Form"/> whose state is to be managed.</param>
        /// <param name="persistenceFileName">The name of the file to which form properties will be
        /// maintained.</param>
        /// <param name="mutexName">Name for the mutex used to serialize persistence of the
        /// control and form layout.</param>
        /// <param name="sandDockManager">If specified, this <see cref="SandDockManager"/>'s state
        /// info will be persisted.</param>
        /// <param name="manageToolStrips">If <see langword="true"/>, the form's
        /// <see cref="ToolStrip"/> will be persisted.</param>
        /// <throws><see cref="ExtractException"/> if instantiated at design-time.</throws>
        public FormStateManager(Form form, string persistenceFileName, string mutexName,
            SandDockManager sandDockManager, bool manageToolStrips)
            : this(form, persistenceFileName, mutexName, sandDockManager, manageToolStrips, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormStateManager"/> class.
        /// <para><b>Note</b></para>
        /// <see cref="FormStateManager"/> should not be created or used in design time.
        /// </summary>
        /// <param name="form">The <see cref="Form"/> whose state is to be managed.</param>
        /// <param name="persistenceFileName">The name of the file to which form properties will be
        /// maintained.</param>
        /// <param name="mutexName">Name for the mutex used to serialize persistence of the
        /// control and form layout.</param>
        /// <param name="sandDockManager">If specified, this <see cref="SandDockManager"/>'s state
        /// info will be persisted.</param>
        /// <param name="manageToolStrips">If <see langword="true"/>, the form's
        /// <see cref="ToolStrip"/> will be persisted.</param>
        /// <param name="fullScreenTabText">If not <see langword="null"/> or empty, an
        /// <see cref="AutoHideScreenTab"/> will be displayed with the provided text that, if
        /// clicked, will exit full screen mode. If <see langword="null"/> full screen mode will not
        /// be supported and the persistence xml file will not contain the FullScreen attribute.
        /// </param>
        /// <throws><see cref="ExtractException"/> if instantiated at design-time.</throws>
        public FormStateManager(Form form, string persistenceFileName, string mutexName,
            SandDockManager sandDockManager, bool manageToolStrips, string fullScreenTabText)
        {
            try
            {
                ExtractException.Assert("ELI30836", "FormStateManager should not be used at design time.",
                    LicenseManager.UsageMode != LicenseUsageMode.Designtime);

                try
                {
                    // Validate the license
                    LicenseUtilities.ValidateLicense(
                        LicenseIdName.ExtractCoreObjects, "ELI30997", _OBJECT_NAME);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30998", ex);
                }

                _form = form;
                _originalBorderStyle = _form.FormBorderStyle;
                _originalSizeGripStyle = _form.SizeGripStyle;
                _persistenceFileName = persistenceFileName;
                _layoutMutex = new Mutex(false, mutexName);
                _sandDockManager = sandDockManager;
                _manageToolStrips = manageToolStrips;

                // Register to receive events relating to the state of the form.
                _form.Load += HandleFormLoad;
                _form.FormClosing += HandleFormClosing;
                _form.LocationChanged += HandleFormStateChanged;
                _form.SizeChanged += HandleFormStateChanged;

                if (!string.IsNullOrEmpty(fullScreenTabText))
                {
                    _fullScreenTab = new AutoHideScreenTab(fullScreenTabText);
                    _fullScreenTab.LabelClicked += HandleFullScreenTabLabelClicked;
                }
                else if (fullScreenTabText == null)
                {
                    _supportsFullScreenMode = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30755", ex);
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the <see cref="FullScreen"/> property is changed.
        /// </summary>
        public event EventHandler<EventArgs> FullScreenChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the position of toolstrips should be persisted.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the position of toolstrips should be persisted; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        public bool ManageToolStrips
        {
            get
            {
                return _manageToolStrips;
            }

            set
            {
                _manageToolStrips = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the managed <see cref="Form"/> is in full screen
        /// mode.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if [full screen]; otherwise, <see langword="false"/>.
        /// </value>
        public bool FullScreen
        {
            get
            {
                return _fullScreen;
            }

            set
            {
                try
                {
                    if (value != _fullScreen)
                    {
                        SetFullScreen(value);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30756", ex);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="FormStateManager"/> instance is
        /// currently updating the <see cref="Form"/>'s UI state.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this <see cref="FormStateManager"/> instance is
        /// currently updating the <see cref="Form"/>'s UI state; otherwise,
        /// <see langword="false"/>.
        /// </value>
        protected bool UpdatingState
        {
            get
            {
                return _updatingStateReferenceCount > 0;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Saves the <see cref="Form"/>'s current UI state to disk.
        /// </summary>
        public virtual void SaveState()
        {
            bool gotMutex = false;

            try
            {
                // Check user.config file for corruption and create backup when this gets destroyed
				// https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker.EnsureValidUserConfigFile();

                // Synchronize access to persistence data [DNRCAU #???]
                gotMutex = _layoutMutex.WaitOne();

                // Convert the manage UI properties to XML
                XmlDocument document = new XmlDocument();
                document.AppendChild(ToXmlNode(document));

                // Create the directory for the file if necessary
                string directory = Path.GetDirectoryName(_persistenceFileName);
                Directory.CreateDirectory(directory);

                // Save the XML
                document.Save(_persistenceFileName);

                if (_manageToolStrips)
                {
                    ToolStripManager.SaveSettings(_form);
                }

                if (_sandDockManager != null)
                {
                    _sandDockManager.SaveLayout();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30757",
                    "Unable to save form state.", ex);
                ee.AddDebugData("XML file", _persistenceFileName, false);
                throw ee;
            }
            finally
            {
                if (gotMutex)
                {
                    _layoutMutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Restores the <see cref="Form"/>'s UI state from disk.
        /// </summary>
        /// <retuns><see langword="true"/> if the UI state was restored from disk,
        /// <see langword="false"/> otherwise.</retuns>
        public virtual bool RestoreSavedState()
        {
            try
            {
                // Synchronize access to persistence data [DNRCAU #???]
                _layoutMutex.WaitOne();

                // Load form position info from file if it exists
                if (File.Exists(_persistenceFileName))
                {
                    // Load the XML
                    XmlDocument document = new XmlDocument();
                    document.Load(_persistenceFileName);

                    // Load the managed UI properties from the xml.
                    LoadFromXml(document);

                    // Use the properties to return the UI to the saved state.
                    RestoreState();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30758",
                    "Unable to load previous form state.", ex);
                ee.AddDebugData("XML file", _persistenceFileName, false);
                throw ee;
            }
            finally
            {
                _layoutMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Restores the state of the user interface of the <see cref="Form"/> using this
        /// <see cref="FormStateManager"/> instance's current properties.
        /// </summary>
        public virtual void RestoreState()
        {
            try
            {
                // Before restoring the state make sure the config is still valid
				// https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker.EnsureValidUserConfigFile();
                
                _updatingStateReferenceCount++;

                _form.StartPosition = FormStartPosition.Manual;

                // Check if the form is on-screen
                if (IntersectsWithScreen(_bounds))
                {
                    _form.Bounds = _bounds;
                }
                else
                {
                    // The form is off-screen, move it on screen
                    _form.Location = Screen.PrimaryScreen.Bounds.Location;
                    _form.Size = _bounds.Size;
                    _bounds = _form.Bounds;
                }

                _form.WindowState = _state;

                if (_sandDockManager != null)
                {
                    _sandDockManager.LoadLayout();
                }

                // So that the toolstrips are properly positioned, it is important to load the
                // SandDock layout before restoring the toolstrips.
                if (_manageToolStrips)
                {
                    foreach (ToolStripContainer toolStripContainer in
                        _form.Controls.OfType<ToolStripContainer>())
                    {
                        FormsMethods.ToolStripManagerLoadHelper(toolStripContainer);
                    }
                }

                SetFullScreen(_fullScreen);
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI32026", ex);
            }
            finally
            {
                _updatingStateReferenceCount--;
            }
        }

        /// <summary>
        /// Creates an <see cref="XmlNode"/> that represents the managed UI properties of the
        /// <see cref="Form"/>.
        /// </summary>
        /// <param name="document">The <see cref="XmlDocument"/> to use when creating the
        /// <see cref="XmlNode"/>.</param>
        /// <returns>An  <see cref="XmlNode"/>that represents the managed UI properties of the
        /// <see cref="Form"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes",
            MessageId = "System.Xml.XmlNode")]
        public virtual XmlNode ToXmlNode(XmlDocument document)
        {
            try
            {
                XmlElement element = document.CreateElement("Form");
                SetAttribute(element, "X", _bounds.X);
                SetAttribute(element, "Y", _bounds.Y);
                SetAttribute(element, "Width", _bounds.Width);
                SetAttribute(element, "Height", _bounds.Height);
                SetAttribute(element, "State", _state);
                if (_supportsFullScreenMode)
                {
                    SetAttribute(element, "FullScreen", _fullScreen);
                }

                return element;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28964", ex);
            }
        }

        /// <summary>
        /// Loads the managed UI properties from the specified XML.
        /// </summary>
        /// <param name="xmlSource">The XML from which the managed UI properties are loaded.</param>
        /// <returns>An <see cref="IXPathNavigable"/> for the XML relating to the
        /// <see cref="FormStateManager"/> class's managed properties.</returns>
        public virtual IXPathNavigable LoadFromXml(IXPathNavigable xmlSource)
        {
            try
            {
                // Get an IXPathNavigable for the XML relating to this class.
                XPathNavigator xmlNavigator = xmlSource.CreateNavigator();

                XPathNavigator element = xmlNavigator.SelectSingleNode("Form");
                ExtractException.Assert("ELI30760", "Failed to load form state data.",
                    element != null);

                // Load the UI state properties.
                _bounds = GetBounds(element);
                _state = GetAttribute<FormWindowState>(element, "State", _form.WindowState);
                if (_supportsFullScreenMode)
                {
                    _fullScreen = GetAttribute<bool>(element, "FullScreen", false);
                }

                return element as IXPathNavigable;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28961", ex);
            }
        }

        /// <summary>
        /// Assigns an attribute with the specified name and value to the specified element.
        /// </summary>
        /// <param name="element">The XML element to which the attribute is assigned.</param>
        /// <param name="name">The name of the attribute to assign.</param>
        /// <param name="value">The value of the attribute to assign.</param>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes",
            MessageId = "System.Xml.XmlNode")]
        protected static void SetAttribute(XmlElement element, string name, object value)
        {
            try
            {
                element.SetAttribute(name, value.ToString());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30761", ex);
            }
        }

        /// <summary>
        /// Gets an attribute from the specified XML.
        /// </summary>
        /// <param name="xmlSource">An <see cref="XPathNavigator"/> for the XML from which to
        /// retrieve the attribute.</param>
        /// <param name="attributeName">The name of the attribute to retrieve</param>
        /// <param name="defaultValue">The value that should be assumed if the specified attribute
        /// value is blank or missing.</param>
        /// <returns>The value of the specified attribute.</returns>
        protected static T GetAttribute<T>(XPathNavigator xmlSource, string attributeName,
            T defaultValue)
        {
            try
            {
                string value = xmlSource.GetAttribute(attributeName, string.Empty);

                if (string.IsNullOrEmpty(value))
                {
                    return defaultValue;
                }
                else
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30762", ex);
            }
        }

        /// <summary>
        /// Displays the managed <see cref="Form"/> in full screen mode or regular mode according to
        /// <see paramref="showFullScreen."/>
        /// </summary>
        /// <param name="showFullScreen">If set to <see langword="true"/> the <see cref="Form"/> is
        /// displayed in full screen mode, if <see langword="false"/> it is returned to non-full
        /// screen mode (which may include being maximized).</param>
        protected void SetFullScreen(bool showFullScreen)
        {
            try
            {
                _updatingStateReferenceCount++;

                if (showFullScreen)
                {
                    ExtractException.Assert("ELI30975",
                        "Full screen mode is not supported for the current form.",
                        _supportsFullScreenMode);

                    // [FlexIDSCore:4457]
                    // Setting the grip style to Hide doesn't actually seem to hide the grip. But
                    // I'll keep the code here regardless since it should be here and in case
                    // someone later discovers what was preventing the grip from being hidden.
                    _form.SizeGripStyle = SizeGripStyle.Hide;
                    _form.WindowState = FormWindowState.Normal;
                    _form.FormBorderStyle = FormBorderStyle.None;
                    _form.Bounds = Screen.FromControl(_form).Bounds;
                    _form.Activated += HandleFormActivated;

                    if (_fullScreenTab != null)
                    {
                        _fullScreenTab.Show(_form);
                    }
                }
                else
                {
                    if (_fullScreenTab != null)
                    {
                        _fullScreenTab.Hide();
                    }

                    _form.Activated -= HandleFormActivated;
                    _form.SizeGripStyle = _originalSizeGripStyle;
                    _form.FormBorderStyle = _originalBorderStyle;
                    _form.Bounds = _bounds;
                    _form.WindowState = _state;
                }

                _fullScreen = showFullScreen;

                OnFullScreenChanged();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI30770", ex);
            }
            finally
            {
                _updatingStateReferenceCount--;
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="FormStateManager"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="FormStateManager"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="FormStateManager"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (_layoutMutex != null)
                {
                    _layoutMutex.Dispose();
                    _layoutMutex = null;
                }

                if (_fullScreenTab != null)
                {
                    _fullScreenTab.Dispose();
                    _fullScreenTab = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Event handlers

        /// <summary>
        /// Handles the case that the managed <see cref="Form"/> is activated
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleFormActivated(object sender, EventArgs e)
        {
            try
            {
                // To force the form to display on top of the taskbar, make it TopMost. But
                // immediately remove the TopMost property so that the user is able to alt-tab to
                // other applications and have the managed form fall to the background.
                _form.TopMost = true;
                _form.TopMost = false;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30771", ex);
            }
        }

        /// <summary>
        /// Handles the form closing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Sometimes when verification is stopped via an unexpected sequence
                // it can lead to the form already having been disposed by the time we got here.
                // Avoid displaying an exception in that case... this can even lead to a crash at this point
                // (one example: UI close, then while dirty prompt is up, save from FAM)
                if (!_form.IsDisposed)
                {
                    SaveState();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31519", ex);
            }
        }

        /// <summary>
        /// Handles the form load.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFormLoad(object sender, EventArgs e)
        {
            try
            {
                RestoreSavedState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30772", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.LocationChanged"/> and <see cref="Control.SizeChanged"/>
        /// events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFormStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (!UpdatingState && !FullScreen)
                {
                    _state = (_form.WindowState == FormWindowState.Minimized)
                        ? FormWindowState.Normal : _form.WindowState;

                    if (_state != FormWindowState.Maximized)
                    {
                        _bounds =
                            (_state == FormWindowState.Normal) ? _form.Bounds : _form.RestoreBounds;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30763", ex);
            }
        }

        /// <summary>
        /// Handles <see cref="AutoHideScreenTab.LabelClicked"/> for the full screen tab.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFullScreenTabLabelClicked(object sender, EventArgs e)
        {
            try
            {
                SetFullScreen(false);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30819", ex);
            }
        }

        #endregion Event handlers

        #region Private members

        /// <summary>
        /// Gets the <see cref="Form"/> bounds from the specified XML.
        /// </summary>
        /// <param name="xmlSource">The element from which bounds should be retrieved.</param>
        /// <returns>The bounds from the specified XML <paramref name="xmlSource"/>.</returns>
        Rectangle GetBounds(XPathNavigator xmlSource)
        {
            int x = GetAttribute<int>(xmlSource, "X", _form.Bounds.X);
            int y = GetAttribute<int>(xmlSource, "Y", _form.Bounds.Y);
            int width = GetAttribute<int>(xmlSource, "Width", _form.Bounds.Width);
            int height = GetAttribute<int>(xmlSource, "Height", _form.Bounds.Height);

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects with the screen.
        /// </summary>
        /// <param name="rectangle">The rectangle to test in screen coordinates.</param>
        /// <returns><see langword="true"/> if any part of the <paramref name="rectangle"/> would 
        /// appear on the screen; <see langword="false"/> if the <paramref name="rectangle"/> is 
        /// completely off-screen.</returns>
        static bool IntersectsWithScreen(Rectangle rectangle)
        {
            try
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.Bounds.IntersectsWith(rectangle))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28962", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="FullScreenChanged"/> event.
        /// </summary>
        void OnFullScreenChanged()
        {
            if (FullScreenChanged != null)
            {
                FullScreenChanged(this, new EventArgs());
            }
        }

        #endregion Private members
    }
}
