using Extract.DataEntry;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Extract.LabDE.StandardLabDE
{
    /// <summary>
    /// A sample <see cref="DataEntryControlHost"/> intended to demonstrate functionality.
    /// </summary>
    public partial class StandardLabDEPanel : DataEntryControlHost
    {
        #region Constructors

        public StandardLabDEPanel() 
            : base()
        {
            try
            {
                InitializeComponent();

                _operatorComments.TextChanged += HandleOperatorCommentsChanged;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25408", ex);
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Ties the StandardDEP's comment property to the _operatorComments control.
        /// </summary>
        /// <value>The comment.</value>
        /// <returns>The comment.</returns>
        public override string Comment
        {
            get
            {
                return _operatorComments.Text;
            }

            set
            {
                _operatorComments.Text = value;
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the case that the operator comment text has changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleOperatorCommentsChanged(object sender, EventArgs e)
        {
            base.OnDataChanged();
        }

        #endregion Event Handlers
    }
}