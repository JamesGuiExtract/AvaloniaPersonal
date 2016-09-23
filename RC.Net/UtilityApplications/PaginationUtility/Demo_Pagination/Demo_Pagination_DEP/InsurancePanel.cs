using System;
using System.Windows.Forms;

namespace Extract.Demo_Pagination
{
    /// <summary>
    /// The doc-type specific panel for the "Insurance" doc type.
    /// </summary>
    internal partial class InsurancePanel : SectionPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InsurancePanel"/> class.
        /// </summary>
        public InsurancePanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
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
                    _insuranceProviderTextBox.Text = data.InsuranceProvider;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41388");
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
                    data.InsuranceProvider = _insuranceProviderTextBox.Text;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41389");
            }
        }
    }
}
