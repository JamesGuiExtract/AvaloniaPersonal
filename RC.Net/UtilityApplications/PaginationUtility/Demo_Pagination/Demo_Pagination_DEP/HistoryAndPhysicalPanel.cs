using System;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Demo_Pagination
{
    /// <summary>
    /// The doc-type specific panel for the "History and Physical" doc type.
    /// </summary>
    internal partial class HistoryAndPhysicalPanel : SectionPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryAndPhysicalPanel"/> class.
        /// </summary>
        public HistoryAndPhysicalPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the <see cref="ErrorProvider" /> to display error glyph for fields with invalid data.
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
                    _dataGridView.Rows.Clear();

                    if (!string.IsNullOrWhiteSpace(data.HistoryHighlight1))
                    {
                        _dataGridView.Rows.Add(new[] { data.HistoryHighlight1 });
                    }
                    if (!string.IsNullOrWhiteSpace(data.HistoryHighlight2))
                    {
                        _dataGridView.Rows.Add(new[] { data.HistoryHighlight2 });
                    }
                    if (!string.IsNullOrWhiteSpace(data.HistoryHighlight3))
                    {
                        _dataGridView.Rows.Add(new[] { data.HistoryHighlight3 });
                    }
                    if (!string.IsNullOrWhiteSpace(data.HistoryHighlight4))
                    {
                        _dataGridView.Rows.Add(new[] { data.HistoryHighlight4 });
                    }
                    if (!string.IsNullOrWhiteSpace(data.HistoryHighlight5))
                    {
                        _dataGridView.Rows.Add(new[] { data.HistoryHighlight5 });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41384");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="validateData"><see langword="true"/> if the <see paramref="data"/> should
        /// be validated for errors when saving; otherwise, <see langwor="false"/>.</param>
        /// <returns></returns>
        public override bool SaveData(Demo_PaginationDocumentData data, bool validateData)
        {
            try
            {
                if (data != null)
                {
                    var rowData = _dataGridView.Rows
                        .OfType<DataGridViewRow>()
                        .Where(row => !row.IsNewRow)
                        .Select(row => row.Cells[0].Value.ToString())
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Take(5)
                        .ToList();

                    for (int i = 0; i < rowData.Count; i++)
                    {
                        switch (i)
                        {
                            case 0: data.HistoryHighlight1 = rowData[0]; break;
                            case 1: data.HistoryHighlight2 = rowData[1]; break;
                            case 2: data.HistoryHighlight3 = rowData[2]; break;
                            case 3: data.HistoryHighlight4 = rowData[3]; break;
                            case 4: data.HistoryHighlight5 = rowData[4]; break;
                        }
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41387");
            }
        }
    }
}
