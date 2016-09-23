using System.Windows.Forms;

namespace Extract.Demo_Pagination
{
    /// <summary>
    /// A panel containing doc-type specific controls for <see cref="PaginationDocumentDataPanel"/>.
    /// </summary>
    internal class SectionPanel : UserControl
    {
        /// <summary>
        /// Gets or sets the <see cref="ErrorProvider"/> to display error glyph for fields with invalid data.
        /// </summary>
        public virtual ErrorProvider ErrorProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the <paramref name="data"/> into the controls.
        /// </summary>
        /// <param name="data">The <see cref="Demo_PaginationDocumentData"/> to load.</param>
        public virtual void LoadData(Demo_PaginationDocumentData data)
        {
        }

        /// <summary>
        /// Loads the controls values to <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The <see cref="Demo_PaginationDocumentData"/> to save.</param>
        /// <param name="validateData"><see langword="true"/> if the <see paramref="data"/> should
        /// be validated for errors when saving; otherwise, <see langwor="false"/>.</param>
        /// <returns></returns>
        public virtual bool SaveData(Demo_PaginationDocumentData data, bool validateData)
        {
            return true;
        }
    }
}
