using Extract.Utilities.Forms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using TD.SandDock;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// A derived <see cref="ToolStripButton"/> that manages the visibility of a dockable window.
    /// </summary>
    public partial class DockableWindowToolStripButton : ToolStripButtonBase
    {
        #region Fields

        /// <summary>
        /// The <see cref="DockableWindow"/> associated with this button.
        /// </summary>
        DockableWindow _dockableWindow;

        /// <summary>
        /// Indicates whether the dockable window was collapsed before hiding it or not.
        /// </summary>
        bool _collapsed;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DockableWindowToolStripButton"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public DockableWindowToolStripButton(Type resourceType, string resourceName)
            : base(resourceType, resourceName)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the <see cref="TD.SandDock.DockableWindow"/> that this control is associated with.
        /// <para><b>Note:</b></para>
        /// This should not be set until the <see cref="Form"/> containing the dockable window has
        /// been displayed and its state has been restored (if the state has been saved). If this
        /// property is set earlier then the toggled state of the button may appear wrong.
        /// </summary>
        public DockableWindow DockableWindow
        {
            get
            {
                return _dockableWindow;
            }
            set
            {
                try
                {
                    if (value != _dockableWindow)
                    {
                        if (_dockableWindow != null)
                        {
                            _dockableWindow.DockSituationChanged -=
                                HandleDockableWindow_DockSituationChanged;
                        }

                        _dockableWindow = value;

                        if (_dockableWindow != null)
                        {
                            if (_dockableWindow.IsOpen || _dockableWindow.Collapsed)
                            {
                                CheckState = CheckState.Checked;
                            }
                            else
                            {
                                CheckState = CheckState.Unchecked;
                            }

                            _dockableWindow.DockSituationChanged +=
                                HandleDockableWindow_DockSituationChanged;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI34988");
                }
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event. If
        /// <see cref="DockableWindowToolStripButton.DockableWindow"/> is not
        /// <see langword="null"/> then will toggle the display of the window.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                // Check if a dockable window has been assigned
                if (_dockableWindow != null)
                {
                    // If the window is opened or collapsed, close it
                    if (_dockableWindow.IsOpen || _dockableWindow.Collapsed)
                    {
                        _collapsed = _dockableWindow.Collapsed;
                        _dockableWindow.Close();
                        CheckState = CheckState.Unchecked;
                    }
                    else
                    {
                        var form = _collapsed ? Parent.FindForm() : null;
                        try
                        {
                            // Suspend layout so that there is no flicker from the collapsing
                            if (form != null)
                            {
                                form.SuspendLayout();
                            }

                            // Window is not open, open it
                            _dockableWindow.Open();
                            if (_collapsed)
                            {
                                _dockableWindow.Collapsed = true;
                            }

                            CheckState = CheckState.Checked;
                        }
                        finally
                        {
                            // Reset the collapsed state
                            _collapsed = false;
                            if (form != null)
                            {
                                form.ResumeLayout();
                            }
                        }
                    }
                }

                base.OnClick(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31382", ex);
            }
        }

        #endregion Overrides

        #region EventHandlers

        /// <summary>
        /// Handles the <see cref="DockControl.DockSituationChanged"/> event of the
        /// <see cref="DockableWindowToolStripButton.DockableWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDockableWindow_DockSituationChanged(object sender, EventArgs e)
        {
            try
            {
                if (_dockableWindow.IsOpen || _dockableWindow.Collapsed)
                {
                    CheckState = CheckState.Checked;
                }
                else
                {
                    CheckState = CheckState.Unchecked;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34989");
            }
        }

        #endregion EventHandlers
    }
}
