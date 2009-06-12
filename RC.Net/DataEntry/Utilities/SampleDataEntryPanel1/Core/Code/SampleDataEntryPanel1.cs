using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    /// <summary>
    /// A sample <see cref="DataEntryControlHost"/> intended to demonstrate functionality.
    /// </summary>
    public partial class SampleDataEntryPanel1 : DataEntryControlHost
    {
        #region Constructors

        public SampleDataEntryPanel1() 
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23672", ex);
            }
        }

        #endregion Constructors
    }
}