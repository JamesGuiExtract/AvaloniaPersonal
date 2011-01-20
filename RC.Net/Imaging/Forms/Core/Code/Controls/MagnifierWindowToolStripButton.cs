using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TD.SandDock;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// A derived <see cref="ToolStripButton"/> that contains the image for the magnifier window.
    /// </summary>
    [ToolboxBitmap(typeof(MagnifierWindowToolStripButton),
        ToolStripButtonConstants._MAGNIFIER_WINDOW_BUTTON_IMAGE)]
    public partial class MagnifierWindowToolStripButton : ToolStripButtonBase
    {
        #region Fields

        /// <summary>
        /// The <see cref="DockableWindow"/> associated with this magnifier control.
        /// </summary>
        DockableWindow _dockableWindow;

        /// <summary>
        /// Indicates whether the dockable window was collapsed before hiding it or not.
        /// </summary>
        bool _collapsed;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MagnifierWindowToolStripButton"/>
        /// class.
        /// </summary>
        public MagnifierWindowToolStripButton()
            : base(typeof(MagnifierWindowToolStripButton),
            ToolStripButtonConstants._MAGNIFIER_WINDOW_BUTTON_IMAGE)
        {
            base.Text = ToolStripButtonConstants._MAGNIFIER_WINDOW_BUTTON_TEXT;
        }

        #endregion Constructors
        
        #region Properties

        /// <summary>
        /// Gets/sets the text associated with this button.
        /// </summary>
        [DefaultValue(ToolStripButtonConstants._MAGNIFIER_WINDOW_BUTTON_TEXT)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

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
                _dockableWindow = value;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event. If
        /// <see cref="MagnifierWindowToolStripButton.DockableWindow"/> is not
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
    }
}
