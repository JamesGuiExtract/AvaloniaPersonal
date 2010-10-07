using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Simplifies the managment of the various components that need to be managed in conjunction
    /// with an application command.  This includes establishing the keyboard shortcut mapping and
    /// simplifies enabling/disabling or showing/hiding the option by enabling/disabling/showing or 
    /// hiding the associated <see cref="ToolStripItem"/>s and enabling or disabling the associated
    /// keyboard shortcut as appropriate.
    /// </summary>
    public class ApplicationCommand
    {
        #region Fields

        /// <summary>
        /// The keyboard shortcut manager.
        /// </summary>
        private ShortcutsManager _shortcutsManager;
        
        /// <summary>
        /// The keyboard shortcut(s) associated with this command
        /// </summary>
        private Keys[] _shortcutKeys;

        /// <summary>
        /// The handler that is to be called when the shortcut is used.
        /// </summary>
        private ShortcutHandler _shortcutHandler;

        /// <summary>
        /// Specifies whether the shortcut command should work even if the associated ToolStripItems
        /// are disabled
        /// </summary>
        private bool _shortcutsAlwaysEnabled;

        /// <summary>
        /// The controls avaible to execute the application command (ToolStripMenuItem, 
        /// ToolStripButton, etc.)
        /// </summary>
        private ToolStripItem[] _toolStripItems;

        /// <summary>
        /// Specifies whether the command is currently enabled or disabled.
        /// </summary>
        private bool _enabled;

        /// <summary>
        /// Specifies whether the command is currently visible or hidden.
        /// </summary>
        private bool _visible;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="ApplicationCommand"/> instance.
        /// </summary>
        /// <param name="shortcutsManager">The <see cref="ShortcutsManager"/> used to execute
        /// keyboard shortcuts. Can be <see langword="null"/> if keyboard shortcuts are not
        /// used for this command.</param>
        /// <param name="shortcutKeys">A vector of <see cref="Keys"/> specifying all keyboard
        /// combinations that should trigger this command.</param>
        /// <param name="shortcutHandler">The <see cref="ShortcutHandler"/> to be called when
        /// one of the specified keyboard combinations is used.</param>
        /// <param name="toolStripItems">The controls avaible to execute the application command
        /// (<see cref="ToolStripMenuItem"/>, <see cref="ToolStripButton"/>, etc.)</param>
        /// <param name="shortcutsAlwaysEnabled"><see langword="true"/> if the shortcut keys should
        /// work whether or not the toolStripItems are enabled or visible; <see langword="false"/>
        /// if the keyboard shortcuts should work only when the toolStripItems are enabled
        /// and visible.</param>
        /// <param name="visible"><see langword="true"/> if the command should be initialized as
        /// visible; <see langword="false"/> otherwise.</param>
        /// <param name="enabled"><see langword="true"/> if the command should be initialized as
        /// enabled; <see langword="false"/> otherwise.</param>
        public ApplicationCommand(ShortcutsManager shortcutsManager, Keys[] shortcutKeys,
            ShortcutHandler shortcutHandler, ToolStripItem[] toolStripItems,  
            bool shortcutsAlwaysEnabled, bool visible, bool enabled)
        {
            _shortcutsManager = shortcutsManager;
            _shortcutKeys = shortcutKeys;
            _shortcutHandler = shortcutHandler;
            _shortcutsAlwaysEnabled = shortcutsAlwaysEnabled;
            _toolStripItems = toolStripItems;

            // Enable the shortcuts right away if _shortcutsAlwaysEnabled; otherwise the Enabled 
            // property will enable the shortcuts when appropriate.
            if (_shortcutsAlwaysEnabled)
            {
                EnableShortcuts(true);
            }

            this.Visible = visible;
            this.Enabled = enabled;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets whether the <see cref="ApplicationCommand"/> is enabled or disabled.
        /// </summary>
        /// <value><see langword="true"/> to enable the command; <see langword="false"/> to
        /// disable it.</value>
        /// <returns><see langword="true"/> if the command is presently enabled; 
        /// <see langword="false"/> if it is presently disabled.</returns>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                try
                {
                    // Unless the shortcut keys are always to be enabled regardless of the state
                    // of the ToolStipItems, enable or disable the shortcuts here.
                    // Also, don't enable/disable shortcuts if not necessary to avoid a performance
                    // hit.
                    if (!_shortcutsAlwaysEnabled && _enabled != value)
                    {
                        EnableShortcuts(value);
                    }
                    
                    // [DataEntry:365, 659] Always enable/disable the tool strip items since the
                    // ImageViewer will sometimes change the state of the items independent of the
                    // ApplicationCommand.
                    if (_toolStripItems != null)
                    {
                        foreach (ToolStripItem toolStripItem in _toolStripItems)
                        {
                             toolStripItem.Enabled = value;
                        }
                    }

                    _enabled = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25112", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="ApplicationCommand"/> is visible or hidden.
        /// </summary>
        /// <value><see langword="true"/> to make the command visible; <see langword="false"/> to
        /// hide it.</value>
        /// <returns><see langword="true"/> if the command is presently visible; 
        /// <see langword="false"/> if it is presently hidden.</returns>
        public bool Visible
        {
            get
            {
                return _visible;
            }

            set
            {
                try
                {
                    if (_toolStripItems != null)
                    {
                        foreach (ToolStripItem toolStripItem in _toolStripItems)
                        {
                            toolStripItem.Visible = value;
                        }
                    }

                    _visible = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25115", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ShortcutHandler"/> to be called when one of the specified
        /// keyboard combinations is used.
        /// </summary>
        public ShortcutHandler ShortcutHandler
        {
            get
            {
                return _shortcutHandler;
            }

            set
            {
                try
                {
                    if (_shortcutHandler != value)
                    {
                        _shortcutHandler = value;

                        if (_enabled || _shortcutsAlwaysEnabled)
                        {
                            EnableShortcuts(true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30679", ex);
                }
            }
        }

        #endregion Properties

        #region Private Members

        /// <summary>
        /// Enables or disables the keyboard shortcuts associated with this command.
        /// </summary>
        /// <param name="enable"><see langword="true"/> to enable the command; 
        /// <see langword="false"/> to disable it.</param>
        private void EnableShortcuts(bool enable)
        {
            if (_shortcutsManager != null && _shortcutKeys != null)
            {
                foreach (Keys shortcutCombo in _shortcutKeys)
                {
                    _shortcutsManager[shortcutCombo] = (enable ? _shortcutHandler : null);
                }
            }
        }

        #endregion Private Members
    }
}
