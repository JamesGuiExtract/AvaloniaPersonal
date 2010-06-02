using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// An enhanced version of the <see cref="RichTextBox"/> control that allows disabling
    /// text selection.
    /// </summary>
    public partial class BetterRichTextBox : RichTextBox
    {
        #region Fields

        /// <summary>
        /// Flag to indicate whether selection of text is allowed in the <see cref="Control"/>.
        /// </summary>
        bool _allowSelection;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterRichTextBox"/> class.
        /// </summary>
        public BetterRichTextBox()
            : base()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the WndProc message.
        /// </summary>
        /// <param name="m">The message being processed.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (!_allowSelection && m.Msg == WindowsMessage.SetFocus)
            {
                m.Msg = WindowsMessage.KillFocus;
            }

            base.WndProc(ref m);
        }

        #endregion Overrides

        #region Properties

        /// <summary>
        /// Gets/sets whether text selection is allowed in the <see cref="Control"/>.
        /// </summary>
        public bool AllowSelection
        {
            get
            {
                return _allowSelection;
            }
            set
            {
                _allowSelection = value;
            }
        }

        #endregion Properties
    }
}
