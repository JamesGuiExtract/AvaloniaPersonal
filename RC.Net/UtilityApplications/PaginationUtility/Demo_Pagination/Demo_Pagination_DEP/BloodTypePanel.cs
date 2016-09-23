using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extract.Demo_Pagination
{
    /// <summary>
    /// The doc-type specific panel for the "Type and Screen" doc type.
    /// </summary>
    internal partial class BloodTypePanel : SectionPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BloodTypePanel"/> class.
        /// </summary>
        public BloodTypePanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the <see cref="ErrorProvider"/> to display error glyph for fields with invalid data.
        /// </summary>
        public override ErrorProvider ErrorProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the <paramref name="data" /> into the controls.
        /// </summary>
        /// <param name="data">The <see cref="Demo_PaginationDocumentData" /> to load.</param>
        public override void LoadData(Demo_PaginationDocumentData data)
        {
            try
            {
                if (data != null)
                {
                    _bloodTypeTextBox.Text = data.BloodType;
                    _rhTextBox.Text = data.BloodRh;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41385");
            }
        }

        /// <summary>
        /// Loads the controls values to <paramref name="data" />.
        /// </summary>
        /// <param name="data">The <see cref="Demo_PaginationDocumentData" /> to save.</param>
        /// <param name="validateData"><see langword="true" /> if the <see paramref="data" /> should
        /// be validated for errors when saving; otherwise, <see langwor="false" />.</param>
        /// <returns></returns>
        public override bool SaveData(Demo_PaginationDocumentData data, bool validateData)
        {
            try
            {
                if (data != null)
                {
                    data.BloodType = _bloodTypeTextBox.Text;
                    data.BloodRh = _rhTextBox.Text;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41386");
            }
        }
    }
}
