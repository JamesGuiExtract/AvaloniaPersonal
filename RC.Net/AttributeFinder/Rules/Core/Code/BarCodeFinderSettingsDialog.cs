using Extract.Licensing;
using Extract.Utilities;
using Nuance.OmniPage.CSDK.ArgTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using static System.FormattableString;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows for configuration of an <see cref="BarcodeFinder"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class BarcodeFinderSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(BarcodeFinderSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeFinderSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="BarcodeFinder"/> instance to configure.</param>
        public BarcodeFinderSettingsDialog(BarcodeFinder settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI46974", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                // Populate a grid that shows all available barcode types and the pass in which the
                // rule would search for it.
                AddGridRow(BAR_TYPE.BAR_A2of5, "Airline 2 of 5");
                AddGridRow(BAR_TYPE.BAR_CB, "Codabar");
                AddGridRow(BAR_TYPE.BAR_CODE11, "Code 11");
                AddGridRow(BAR_TYPE.BAR_C39, "Code 39");
                AddGridRow(BAR_TYPE.BAR_C39_EXT, "Code 39 Extended");
                AddGridRow(BAR_TYPE.BAR_C93, "Code 93. The full ASCII character set is supported");
                AddGridRow(BAR_TYPE.BAR_C128, "Code 128");
                AddGridRow(BAR_TYPE.BAR_DMATRIX, "Data Matrix (2D barcode)");
                AddGridRow(BAR_TYPE.BAR_EAN, "EAN 8/13");
                AddGridRow(BAR_TYPE.BAR_EAN14, "EAN-14 Code");
                AddGridRow(BAR_TYPE.BAR_SSCC18, "SSCC18/EAN-18 Code");
                AddGridRow(BAR_TYPE.BAR_UCC128, "UCC/EAN Code 128. Includes SSCC-18 and EAN-14 as well");
                AddGridRow(BAR_TYPE.BAR_BOOKLAND, "Bookland EAN Code");
                AddGridRow(BAR_TYPE.BAR_PDF417, "PDF417 (2D barcode)");
                AddGridRow(BAR_TYPE.BAR_QR, "QR Code (Quick Response) (2D barcode)");
                AddGridRow(BAR_TYPE.BAR_UPC_A, "UPC-A");
                AddGridRow(BAR_TYPE.BAR_UPC_E, "UPC-E (6-digit)");
                AddGridRow(BAR_TYPE.BAR_POSTNET, "Postnet code (US postal code)");
                AddGridRow(BAR_TYPE.BAR_4STATE_USPS, "USPS 4-State Customer Barcode. (a.k.a. OneCode, Intelligent Mail)");
                AddGridRow(BAR_TYPE.BAR_4STATE_AUSPOST, "Australia Post 4-State Customer Barcode)");
                AddGridRow(BAR_TYPE.BAR_DATABAR_LTD, "GS1 Databar Limited Code");
                AddGridRow(BAR_TYPE.BAR_DATABAR_EXP, "GS1 Databar Expanded Code");
                AddGridRow(BAR_TYPE.BAR_C39_NSS, "Code 39 without start-stop characters");
                AddGridRow(BAR_TYPE.BAR_PATCH, "Patch Code");
                AddGridRow(BAR_TYPE.BAR_2of5, "Code 2 of 5 Standard");
                AddGridRow(BAR_TYPE.BAR_ITF, "ITF (Interleaved 2 of 5)");
                AddGridRow(BAR_TYPE.BAR_ITF14, "ITF 14 Code");
                AddGridRow(BAR_TYPE.BAR_PLANET, "Planet Code (US postal code)");
                AddGridRow(BAR_TYPE.BAR_ITAPOST25, "Italian Postal 2 of 5 Code");
                AddGridRow(BAR_TYPE.BAR_MAT25, "Matrix 2 of 5");
                AddGridRow(BAR_TYPE.BAR_MSI, "Modified Plessey Code");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46975");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="BarcodeFinder"/> to configure.
        /// </summary>
        /// <value>The <see cref="BarcodeFinder"/> to configure.</value>
        public BarcodeFinder Settings
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                var enabledTypes = new HashSet<BAR_TYPE>(
                    Settings.Types.ToIEnumerable<BAR_TYPE>());

                foreach (var row in _barcodeTypesDataGridView.Rows
                    .OfType<DataGridViewRow>()
                    .Where(row => enabledTypes.Contains((BAR_TYPE)row.Tag)))
                {
                    row.Cells[0].Value = true;
                }

                UpdatePassCount();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46976");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the CellMouseUp event of <see cref="_barcodeTypesDataGridView"/>.
        /// </summary>
        void HandleBarcodeTypesDataGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == _barcodeEnableColumn.Index)
                {
                    _barcodeTypesDataGridView.EndEdit();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46987");
            }
        }

        /// <summary>
        /// Handles the CellValueChanged event of <see cref="_barcodeTypesDataGridView"/>.
        /// </summary>
        void HandleBarcodeTypesDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == _barcodeEnableColumn.Index)
                {
                    UpdatePassCount();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46988");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for <see cref="_okButton"/>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.Types = BarTypes.ToVariantVector();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46977");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Adds the specified bar type to the grid as an option, with the specified visible name.
        /// </summary>
        void AddGridRow(BAR_TYPE type, string name)
        {
            var pass = Invariant($"Pass {Settings.PassMapping[type]}");
            var index = _barcodeTypesDataGridView.Rows.Add(false, name, null, pass);
            _barcodeTypesDataGridView.Rows[index].Tag = type;
        }

        /// <summary>
        /// The currently configured barcode types to find.
        /// </summary>
        BAR_TYPE[] BarTypes
        {
            get
            {
                return _barcodeTypesDataGridView.Rows
                    .OfType<DataGridViewRow>()
                    .Where(row => (bool)row.Cells[0].Value)
                    .Select(row => (BAR_TYPE)row.Tag)
                    .ToArray();
            }
        }

        /// <summary>
        /// Returns true if the current configuration is invalid
        /// </summary>
        /// <returns></returns>
        bool WarnIfInvalid()
        {
            if (BarTypes.Length == 0)
            {
                UtilityMethods.ShowMessageBox(
                    "At least one type of barcode must be selected.", "Empty selection", false);
                return true;
            }

            return false;
        }

        void UpdatePassCount()
        {
            var passCount = BarcodeFinder.GetPassSequence(BarTypes).Length;
            _passCountLabel.Text = 
                Invariant($"The current configuration will require {passCount} pass{(passCount == 1 ? "" : "es")} ");
        }

        #endregion Private Members
    }
}
