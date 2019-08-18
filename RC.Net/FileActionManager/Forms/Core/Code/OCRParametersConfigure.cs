using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCR2Lib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Form used to configure RunMode
    /// </summary>
    [ComVisible(true)]
    [Guid("13D0AD14-1C43-490D-8F41-22195AFB1C96")]
    [CLSCompliant(false)]
    public partial class OCRParametersConfigure : Form, IOCRParametersConfigure
    {
        VariantVector _parameterMap;
        BindingList<Language> _languages = new BindingList<Language>();
        BindingList<KeyValueClass<int, string>> _enumSettings = new BindingList<KeyValueClass<int, string>>();
        BindingList<KeyValueClass<string, string>> _stringSettings = new BindingList<KeyValueClass<string, string>>();
        bool _readOnly;
        bool _settingValues;
        
        #region Constructors 

        /// <summary>
        /// Initializes readable names for the <see cref="DESPECKLE_METHOD"/> enum.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static OCRParametersConfigure()
        {
            try
            {
                DESPECKLE_METHOD.DESPECKLE_AUTO.SetReadableValue("Auto");
                DESPECKLE_METHOD.DESPECKLE_HALFTONE.SetReadableValue("Halftone");
                DESPECKLE_METHOD.DESPECKLE_INVERSE.SetReadableValue("Inverse");
                DESPECKLE_METHOD.DESPECKLE_MEDIAN.SetReadableValue("Median");
                DESPECKLE_METHOD.DESPECKLE_NORMAL.SetReadableValue("Normal");
                DESPECKLE_METHOD.DESPECKLE_PEPPER.SetReadableValue("Pepper");
                DESPECKLE_METHOD.DESPECKLE_PEPPERANDSALT.SetReadableValue("Pepper and salt");
                DESPECKLE_METHOD.DESPECKLE_SALT.SetReadableValue("Salt");
                DESPECKLE_METHOD.DESPECKLE_SMOOTHEDGE.SetReadableValue("Smooth edge");

                EPageDecompositionMethod.kAutoDecomposition.SetReadableValue("Auto");
                EPageDecompositionMethod.kLegacyDecomposition.SetReadableValue("Legacy");
                EPageDecompositionMethod.kStandardDecomposition.SetReadableValue("Standard");

                EOcrTradeOff.kAccurate.SetReadableValue("Accurate");
                EOcrTradeOff.kBalanced.SetReadableValue("Balanced");
                EOcrTradeOff.kFast.SetReadableValue("Fast");
                EOcrTradeOff.kRegistry.SetReadableValue("Registry");

                FILLINGMETHOD.FM_DEFAULT.SetReadableValue("DEFAULT");
                FILLINGMETHOD.FM_OMNIFONT.SetReadableValue("OMNIFONT");
                FILLINGMETHOD.FM_DRAFTDOT9.SetReadableValue("DRAFTDOT9");
                FILLINGMETHOD.FM_BARCODE.SetReadableValue("BARCODE");
                FILLINGMETHOD.FM_OMR.SetReadableValue("OMR");
                FILLINGMETHOD.FM_HANDPRINT.SetReadableValue("HANDPRINT");
                FILLINGMETHOD.FM_BRAILLE.SetReadableValue("BRAILLE");
                FILLINGMETHOD.FM_DRAFTDOT24.SetReadableValue("DRAFTDOT24");
                FILLINGMETHOD.FM_OCRA.SetReadableValue("OCRA");
                FILLINGMETHOD.FM_OCRB.SetReadableValue("OCRB");
                FILLINGMETHOD.FM_MICR.SetReadableValue("MICR");
                FILLINGMETHOD.FM_BARCODE2D.SetReadableValue("BARCODE2D");
                FILLINGMETHOD.FM_DOTDIGIT.SetReadableValue("DOTDIGIT");
                FILLINGMETHOD.FM_DASHDIGIT.SetReadableValue("DASHDIGIT");
                FILLINGMETHOD.FM_ASIAN.SetReadableValue("ASIAN");
                FILLINGMETHOD.FM_CMC7.SetReadableValue("CMC7");
                FILLINGMETHOD.FM_NO_OCR.SetReadableValue("NO_OCR");

                LANGUAGES.LANG_NO.SetReadableValue("");
                LANGUAGES.LANG_ALL.SetReadableValue("All");
                LANGUAGES.LANG_ALL_LATIN.SetReadableValue("All Latin");
                LANGUAGES.LANG_ALL_ASIAN.SetReadableValue("All Asian");
                LANGUAGES.LANG_UD.SetReadableValue("User dictionary");

                LANGUAGES.LANG_ENG.SetReadableValue("English");
                LANGUAGES.LANG_GER.SetReadableValue("German");
                LANGUAGES.LANG_FRE.SetReadableValue("French");
                LANGUAGES.LANG_DUT.SetReadableValue("Dutch");
                LANGUAGES.LANG_NOR.SetReadableValue("Norwegian");
                LANGUAGES.LANG_SWE.SetReadableValue("Swedish");
                LANGUAGES.LANG_FIN.SetReadableValue("Finnish");
                LANGUAGES.LANG_DAN.SetReadableValue("Danish");
                LANGUAGES.LANG_ICE.SetReadableValue("Icelandic");
                LANGUAGES.LANG_POR.SetReadableValue("Portuguese");
                LANGUAGES.LANG_SPA.SetReadableValue("Spanish");
                LANGUAGES.LANG_CAT.SetReadableValue("Catalan");
                LANGUAGES.LANG_GAL.SetReadableValue("Galician");
                LANGUAGES.LANG_ITA.SetReadableValue("Italian");
                LANGUAGES.LANG_MAL.SetReadableValue("Maltese");
                LANGUAGES.LANG_GRE.SetReadableValue("Greek");
                LANGUAGES.LANG_POL.SetReadableValue("Polish");
                LANGUAGES.LANG_CZH.SetReadableValue("Czech");
                LANGUAGES.LANG_SLK.SetReadableValue("Slovak");
                LANGUAGES.LANG_HUN.SetReadableValue("Hungarian");
                LANGUAGES.LANG_SLN.SetReadableValue("Slovenian");
                LANGUAGES.LANG_CRO.SetReadableValue("Croatian");
                LANGUAGES.LANG_ROM.SetReadableValue("Romanian");
                LANGUAGES.LANG_ALB.SetReadableValue("Albanian");
                LANGUAGES.LANG_TUR.SetReadableValue("Turkish");
                LANGUAGES.LANG_EST.SetReadableValue("Estonian");
                LANGUAGES.LANG_LAT.SetReadableValue("Latvian");
                LANGUAGES.LANG_LIT.SetReadableValue("Lithuanian");
                LANGUAGES.LANG_ESP.SetReadableValue("Esperanto");
                LANGUAGES.LANG_SRL.SetReadableValue("Serbian (Latin)");
                LANGUAGES.LANG_SRB.SetReadableValue("Serbian (Cyrillic)");
                LANGUAGES.LANG_MAC.SetReadableValue("Macedonian (Cyrillic)");
                LANGUAGES.LANG_MOL.SetReadableValue("Moldavian (Cyrillic)");
                LANGUAGES.LANG_BUL.SetReadableValue("Bulgarian (Cyrillic)");
                LANGUAGES.LANG_BEL.SetReadableValue("Byelorussian (Cyrillic)");
                LANGUAGES.LANG_UKR.SetReadableValue("Ukrainian (Cyrillic)");
                LANGUAGES.LANG_RUS.SetReadableValue("Russian (Cyrillic)");
                LANGUAGES.LANG_CHE.SetReadableValue("Chechen");
                LANGUAGES.LANG_KAB.SetReadableValue("Kabardian");
                LANGUAGES.LANG_AFR.SetReadableValue("Afrikaans");
                LANGUAGES.LANG_AYM.SetReadableValue("Aymara");
                LANGUAGES.LANG_BAS.SetReadableValue("Basque");
                LANGUAGES.LANG_BEM.SetReadableValue("Bemba");
                LANGUAGES.LANG_BLA.SetReadableValue("Blackfoot");
                LANGUAGES.LANG_BRE.SetReadableValue("Breton");
                LANGUAGES.LANG_BRA.SetReadableValue("Portuguese (Brazilian)");
                LANGUAGES.LANG_BUG.SetReadableValue("Bugotu");
                LANGUAGES.LANG_CHA.SetReadableValue("Chamorro");
                LANGUAGES.LANG_CHU.SetReadableValue("Chuana or Tswana");
                LANGUAGES.LANG_COR.SetReadableValue("Corsican");
                LANGUAGES.LANG_CRW.SetReadableValue("Crow");
                LANGUAGES.LANG_ESK.SetReadableValue("Eskimo");
                LANGUAGES.LANG_FAR.SetReadableValue("Faroese");
                LANGUAGES.LANG_FIJ.SetReadableValue("Fijian");
                LANGUAGES.LANG_FRI.SetReadableValue("Frisian");
                LANGUAGES.LANG_FRU.SetReadableValue("Friulian");
                LANGUAGES.LANG_GLI.SetReadableValue("Gaelic Irish");
                LANGUAGES.LANG_GLS.SetReadableValue("Gaelic Scottish");
                LANGUAGES.LANG_GAN.SetReadableValue("Ganda or Luganda");
                LANGUAGES.LANG_GUA.SetReadableValue("Guarani");
                LANGUAGES.LANG_HAN.SetReadableValue("Hani");
                LANGUAGES.LANG_HAW.SetReadableValue("Hawaiian");
                LANGUAGES.LANG_IDO.SetReadableValue("Ido");
                LANGUAGES.LANG_IND.SetReadableValue("Indonesian");
                LANGUAGES.LANG_INT.SetReadableValue("Interlingua");
                LANGUAGES.LANG_KAS.SetReadableValue("Kashubian");
                LANGUAGES.LANG_KAW.SetReadableValue("Kawa");
                LANGUAGES.LANG_KIK.SetReadableValue("Kikuyu");
                LANGUAGES.LANG_KON.SetReadableValue("Kongo");
                LANGUAGES.LANG_KPE.SetReadableValue("Kpelle");
                LANGUAGES.LANG_KUR.SetReadableValue("Kurdish");
                LANGUAGES.LANG_LTN.SetReadableValue("Latin");
                LANGUAGES.LANG_LUB.SetReadableValue("Luba");
                LANGUAGES.LANG_LUX.SetReadableValue("Luxembourgian");
                LANGUAGES.LANG_MLG.SetReadableValue("Malagasy");
                LANGUAGES.LANG_MLY.SetReadableValue("Malay");
                LANGUAGES.LANG_MLN.SetReadableValue("Malinke");
                LANGUAGES.LANG_MAO.SetReadableValue("Maori");
                LANGUAGES.LANG_MAY.SetReadableValue("Mayan");
                LANGUAGES.LANG_MIA.SetReadableValue("Miao");
                LANGUAGES.LANG_MIN.SetReadableValue("Minankabaw");
                LANGUAGES.LANG_MOH.SetReadableValue("Mohawk");
                LANGUAGES.LANG_NAH.SetReadableValue("Nahuatl");
                LANGUAGES.LANG_NYA.SetReadableValue("Nyanja");
                LANGUAGES.LANG_OCC.SetReadableValue("Occidental");
                LANGUAGES.LANG_OJI.SetReadableValue("Ojibway");
                LANGUAGES.LANG_PAP.SetReadableValue("Papiamento");
                LANGUAGES.LANG_PID.SetReadableValue("Pidgin English");
                LANGUAGES.LANG_PRO.SetReadableValue("Provencal");
                LANGUAGES.LANG_QUE.SetReadableValue("Quechua");
                LANGUAGES.LANG_RHA.SetReadableValue("Rhaetic");
                LANGUAGES.LANG_ROY.SetReadableValue("Romany");
                LANGUAGES.LANG_RUA.SetReadableValue("Ruanda");
                LANGUAGES.LANG_RUN.SetReadableValue("Rundi");
                LANGUAGES.LANG_SAM.SetReadableValue("Samoan");
                LANGUAGES.LANG_SAR.SetReadableValue("Sardinian");
                LANGUAGES.LANG_SHO.SetReadableValue("Shona");
                LANGUAGES.LANG_SIO.SetReadableValue("Sioux");
                LANGUAGES.LANG_SMI.SetReadableValue("Sami");
                LANGUAGES.LANG_SML.SetReadableValue("Lule Sami");
                LANGUAGES.LANG_SMN.SetReadableValue("Northern Sami");
                LANGUAGES.LANG_SMS.SetReadableValue("Southern Sami");
                LANGUAGES.LANG_SOM.SetReadableValue("Somali");
                LANGUAGES.LANG_SOT.SetReadableValue("Sotho, Suto or Sesuto");
                LANGUAGES.LANG_SUN.SetReadableValue("Sundanese");
                LANGUAGES.LANG_SWA.SetReadableValue("Swahili");
                LANGUAGES.LANG_SWZ.SetReadableValue("Swazi");
                LANGUAGES.LANG_TAG.SetReadableValue("Tagalog");
                LANGUAGES.LANG_TAH.SetReadableValue("Tahitian");
                LANGUAGES.LANG_TIN.SetReadableValue("Tinpo");
                LANGUAGES.LANG_TON.SetReadableValue("Tongan");
                LANGUAGES.LANG_TUN.SetReadableValue("Tun");
                LANGUAGES.LANG_VIS.SetReadableValue("Visayan");
                LANGUAGES.LANG_WEL.SetReadableValue("Welsh");
                LANGUAGES.LANG_WEN.SetReadableValue("Wend or Sorbian");
                LANGUAGES.LANG_WOL.SetReadableValue("Wolof");
                LANGUAGES.LANG_XHO.SetReadableValue("Xhosa");
                LANGUAGES.LANG_ZAP.SetReadableValue("Zapotec");
                LANGUAGES.LANG_ZUL.SetReadableValue("Zulu");
                LANGUAGES.LANG_JPN.SetReadableValue("Japanese");
                LANGUAGES.LANG_CHS.SetReadableValue("Simplified Chinese");
                LANGUAGES.LANG_CHT.SetReadableValue("Traditional Chinese");
                LANGUAGES.LANG_KRN.SetReadableValue("Korean");
                LANGUAGES.LANG_THA.SetReadableValue("Thai");
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI45964", ex);
            }
        }

        /// <summary>
        /// Constructor for form
        /// </summary>
        public OCRParametersConfigure()
        {
            try
            {
                InitializeComponent();

                // Initialize the languages DataGridView
                _languagesDataGridView.AutoGenerateColumns = false;
                _languagesDataGridView.ColumnHeadersVisible = false;
                _languagesDataGridView.DataSource = _languages;

                // Add language column
                var combo = new DataGridViewComboBoxColumn();
                combo.ValueType = typeof(LANGUAGES);
                combo.ValueMember = "Value";
                combo.DisplayMember = "DisplayName";
                combo.DataSource = Enum.GetValues(typeof(LANGUAGES))
                    .OfType<LANGUAGES>()
                    .Select(lang =>
                        lang.TryGetReadableValue(out string readableValue)
                        ? new Language { Value = lang, DisplayName = readableValue }
                        : null
                    )
                    .Where(pair => pair != null)
                    .OrderBy(pair => pair.DisplayName)
                    .ToList();
                combo.DataPropertyName = "Value";
                combo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _languagesDataGridView.Columns.Add(combo);


                // Initialize the enum settings DataGridView
                _enumSettingsDataGridView.AutoGenerateColumns = false;
                _enumSettingsDataGridView.DataSource = _enumSettings;

                // Add setting key column
                var col = new DataGridViewTextBoxColumn();
                col.ValueType = typeof(int);
                col.DataPropertyName = "Key";
                col.Name = "EOCRParameter";
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                _enumSettingsDataGridView.Columns.Add(col);

                // Add setting value column
                col = new DataGridViewTextBoxColumn();
                col.ValueType = typeof(double);
                col.DataPropertyName = "Value";
                col.Name = "Value";
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _enumSettingsDataGridView.Columns.Add(col);


                // Initialize the string settings DataGridView
                _stringSettingsDataGridView.AutoGenerateColumns = false;
                _stringSettingsDataGridView.DataSource = _stringSettings;

                // Add setting key column
                col = new DataGridViewTextBoxColumn();
                col.ValueType = typeof(string);
                col.DataPropertyName = "Key";
                col.Name = "Nuance setting path";
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                _stringSettingsDataGridView.Columns.Add(col);

                // Add setting value column
                col = new DataGridViewTextBoxColumn();
                col.ValueType = typeof(string);
                col.DataPropertyName = "Value";
                col.Name = "Value";
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _stringSettingsDataGridView.Columns.Add(col);


                _defaultDecompositionMethodComboBox.InitializeWithReadableEnum<EPageDecompositionMethod>(true);
                _accuracyTradeoffComboBox.InitializeWithReadableEnum<EOcrTradeOff>(true);
                _defaultFillingMethodComboBox.InitializeWithReadableEnum<FILLINGMETHOD>(true);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI45873", ex);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _forceDespeckleMethodComboBox.InitializeWithReadableEnum<DESPECKLE_METHOD>(true);
                SetControlValues();
                SetControlStates();

                if (_readOnly)
                {
                    foreach(Control tabPage in _tabControl.TabPages)
                    {
                        tabPage.Enabled = false;
                    }
                    Controls.Remove(_okButton);
                    _cancelButton.Text = "Close";
                }
                else
                {
                    // Switch to the page with the most options to set
                    _tabControl.SelectedTab = _recognitionOptionsTabPage;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45879");
            }
        }

        #endregion Overrides

        #region IOCRParametersConfigure members

        /// <summary>
        /// Method used to configure OCR parameters
        /// </summary>
        /// <param name="pParams">The <see cref="IHasOCRParameters"/> object that is being configured</param>
        /// <param name="vbReadOnly">Whether to allow editing of the parameters</param>
        /// <param name="nHandle">The handle to the parent window</param>
        public void ConfigureOCRParameters(IHasOCRParameters pParams, bool vbReadOnly, int nHandle)
        {
            try
            {
                _parameterMap = (VariantVector)pParams.OCRParameters;
                _readOnly = vbReadOnly;

                // Display the dialog centered on the parent
                NativeWindow parentWindow = new NativeWindow();
                parentWindow.AssignHandle((IntPtr)nHandle);
                if (ShowDialog(parentWindow) == DialogResult.OK)
                {
                    ApplyControlValuesToSettings();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI45874", ex);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles events by setting dependant control states
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void Handle_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_settingValues)
                {
                    return;
                }

                SetControlStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI45877", ex);
            }
        }

        /// <summary>
        /// Handles the EnabledChanged event of a data grid view to make it actually look disabled
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void Handle_DataGridView_EnabledChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = sender as DataGridView;
                if (!dgv.Enabled)
                {
                    dgv.DefaultCellStyle.BackColor = SystemColors.Control;
                    dgv.DefaultCellStyle.ForeColor = SystemColors.GrayText;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.GrayText;
                    dgv.CurrentCell = null;
                    dgv.ReadOnly = true;
                    dgv.EnableHeadersVisualStyles = false;
                }
                else
                {
                    dgv.DefaultCellStyle.BackColor = SystemColors.Window;
                    dgv.DefaultCellStyle.ForeColor = SystemColors.ControlText;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Window;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
                    dgv.ReadOnly = false;
                    dgv.EnableHeadersVisualStyles = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46005");
            }
        } 

        /// <summary>
        /// Handles the DataError event of a data grid view in order to display the exception
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void Handle_DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                e.Exception.ExtractDisplay("ELI46006");
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46007");
            }
        }

        /// <summary>
        /// Handles the click event of the _setDefaultsButton
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void Handle_SetDefaultsButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetDefaultRecognitionOptions();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46036");
            }
        }

        /// <summary>
        /// Handles the click event of the _setClassicButton
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void Handle_SetClassicButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetClassicRecognitionOptions();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46037");
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Method to enable or disable the controls based on the values.
        /// </summary>
        void SetControlStates()
        {
            if (_neverForceDespeckleRadioButton.Checked)
            {
                _forceDespeckleMethodComboBox.Enabled = false;
                _forceDespeckleLevelNumericUpDown.Enabled = false;
            }
            else
            {
                _forceDespeckleMethodComboBox.Enabled = true;
                var method = _forceDespeckleMethodComboBox.ToEnumValue<DESPECKLE_METHOD>();
                switch (method)
                {
                    case DESPECKLE_METHOD.DESPECKLE_NORMAL:
                    case DESPECKLE_METHOD.DESPECKLE_INVERSE:
                    case DESPECKLE_METHOD.DESPECKLE_HALFTONE:
                        _forceDespeckleLevelNumericUpDown.Enabled = true;
                        _forceDespeckleLevelNumericUpDown.Minimum = 1;
                        _forceDespeckleLevelNumericUpDown.Maximum = 4;
                        break;
                    case DESPECKLE_METHOD.DESPECKLE_PEPPER:
                    case DESPECKLE_METHOD.DESPECKLE_SALT:
                    case DESPECKLE_METHOD.DESPECKLE_PEPPERANDSALT:
                        _forceDespeckleLevelNumericUpDown.Enabled = true;
                        _forceDespeckleLevelNumericUpDown.Minimum = 1;
                        _forceDespeckleLevelNumericUpDown.Maximum = 256;
                        break;
                    default:
                        _forceDespeckleLevelNumericUpDown.Enabled = false;
                        _forceDespeckleLevelNumericUpDown.Minimum = 0;
                        break;
                }
            }

            if (_skipPageOnFailureCheckBox.Checked)
            {
                _requireOnePageSuccessCheckBox.Enabled =
                    _maxPageFailureNumberNumericUpDown.Enabled =
                    _maxPageFailurePercentNumericUpDown.Enabled = true;
            }
            else
            {
                _requireOnePageSuccessCheckBox.Enabled =
                    _maxPageFailureNumberNumericUpDown.Enabled =
                    _maxPageFailurePercentNumericUpDown.Enabled = false;
            }

            if (_specifyRecognitionLanguagesCheckBox.Checked)
            {
                _recognitionLanguagesGroupBox.Enabled = true;
            }
            else
            {
                _recognitionLanguagesGroupBox.Enabled = false;
            }

            if (_ignoreParagraphFlagCheckBox.Checked)
            {
                _treatZonesAsParagraphsCheckBox.Enabled = true;
            }
            else
            {
                _treatZonesAsParagraphsCheckBox.Enabled = false;
            }
        }

        private void SetControlValues()
        {
            _settingValues = true;

            try
            {
                // Clear values for the data grid views
                _languages.Clear();
                _enumSettings.Clear();
                _stringSettings.Clear();

                // Set defaults
                _forceDespeckleMethodComboBox.SelectEnumValue(DESPECKLE_METHOD.DESPECKLE_AUTO);

                if (_parameterMap.Size > 0)
                {
                    SetClassicRecognitionOptions();
                }
                else
                {
                    SetDefaultRecognitionOptions();
                }

                foreach(var ocrParam in ((IOCRParameters)_parameterMap).ToIEnumerable())
                {
                    // int * int
                    ocrParam.Match(pair =>
                    {
                        (int key, int value) = pair;
                        switch ((EOCRParameter)key)
                        {
                            case EOCRParameter.kForceDespeckleMode:
                                switch ((EForceDespeckleMode)value)
                                {
                                    case EForceDespeckleMode.kNeverForce:
                                        _neverForceDespeckleRadioButton.Checked = true;
                                        break;
                                    case EForceDespeckleMode.kForceWhenBitonal:
                                        _forceDespeckleWhenBitonalRadioButton.Checked = true;
                                        break;
                                    case EForceDespeckleMode.kAlwaysForce:
                                        _alwaysForceDespeckleRadioButton.Checked = true;
                                        break;
                                }
                                break;
                            case EOCRParameter.kForceDespeckleMethod:
                                _forceDespeckleMethodComboBox.SelectEnumValue((DESPECKLE_METHOD)value);
                                break;
                            case EOCRParameter.kForceDespeckleLevel:
                                _forceDespeckleLevelNumericUpDown.Minimum = 0;
                                _forceDespeckleLevelNumericUpDown.Maximum = int.MaxValue;
                                _forceDespeckleLevelNumericUpDown.Value = value;
                                break;
                            case EOCRParameter.kAutoDespeckleMode:
                                _autoDespeckleCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kZoneOrdering:
                                _zoneOrderingCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kIgnoreAreaOutsideSpecifiedZone:
                                _ignoreAreaOutsideSpecifiedZoneCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kLocateZonesInSpecifiedZone:
                                _locateZonesInsideSpecifiedZoneCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kLimitToBasicLatinCharacters:
                                _limitToBasicLatinCharactersCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kReturnUnrecognizedCharacters:
                                _returnUnrecognizedCharactersCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kSkipPageOnFailure:
                                _skipPageOnFailureCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kRequireOnePageSuccess:
                                _requireOnePageSuccessCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kMaxPageFailureNumber:
                                _maxPageFailureNumberNumericUpDown.Value = (uint)value;
                                break;
                            case EOCRParameter.kMaxPageFailurePercent:
                                _maxPageFailurePercentNumericUpDown.Value = value;
                                break;
                            case EOCRParameter.kDefaultDecompositionMethod:
                                _defaultDecompositionMethodComboBox.SelectEnumValue((EPageDecompositionMethod)value);
                                break;
                            case EOCRParameter.kTradeoff:
                                _accuracyTradeoffComboBox.SelectEnumValue((EOcrTradeOff)value);
                                break;
                            case EOCRParameter.kDefaultFillingMethod:
                                _defaultFillingMethodComboBox.SelectEnumValue((FILLINGMETHOD)value);
                                break;
                            case EOCRParameter.kTimeout:
                                _timeoutNumericUpDown.Value = value;
                                break;
                            case EOCRParameter.kOutputMultipleSpaceCharacterSequences:
                                _outputMultipleSpaceCharacterSequencesCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kOutputOneSpaceCharacterPerCount:
                                _outputOneSpaceCharPerCountCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kOutputTabCharactersForTabSpaceType:
                                _outputTabCharactersCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kAssignSpatialInfoToSpaceCharacters:
                                _assignSpatialInfoToSpaceCharsCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kIgnoreParagraphFlag:
                                _ignoreParagraphFlagCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kTreatZonesAsParagraphs:
                                _treatZonesAsParagraphsCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kSpecifyLanguage:
                                _specifyRecognitionLanguagesCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kSingleLanguageDetection:
                                _singleLanguageDetectionCheckBox.Checked = value != 0;
                                break;
                            case EOCRParameter.kLanguage:
                                var lang = (LANGUAGES)value;
                                if (lang.TryGetReadableValue(out var description))
                                {
                                    _languages.Add(new Language { Value = lang, DisplayName = description });
                                }
                                break;
                            default:
                                _enumSettings.Add(new KeyValueClass<int, string>
                                { Key = key, Value = value.ToString(CultureInfo.InvariantCulture) });
                                break;
                        }
                    },
                    // int * double
                    pair =>
                    {
                        (int key, double value) = pair;
                        _enumSettings.Add(new KeyValueClass<int, string>
                        { Key = key, Value = value.ToString("F1", CultureInfo.InvariantCulture) });
                    },
                    // string * int
                    pair =>
                    {
                        (string key, int value) = pair;
                        switch (key)
                        {
                            case "Kernel.Img.Max.Pix.X":
                                _maxXNumericUpDown.Value = (uint)value;
                                break;
                            case "Kernel.Img.Max.Pix.Y":
                                _maxYNumericUpDown.Value = (uint)value;
                                break;
                            case "Kernel.OcrMgr.PreferAccurateEngine":
                                _preferAccuracteEngineCheckBox.Checked = value != 0;
                                break;
                            default:
                                _stringSettings.Add(new KeyValueClass<string, string>
                                {
                                    Key = key,
                                    Value = value.ToString(CultureInfo.InvariantCulture)
                                });
                                break;
                        }
                    },
                    // string * double
                    pair =>
                    {
                        (string key, double value) = pair;
                        _stringSettings.Add(new KeyValueClass<string, string>
                        {
                            Key = key,
                            Value = value.ToString("F1", CultureInfo.InvariantCulture)
                        });
                    },
                    // string * string
                    pair =>
                    {
                        (string key, string value) = pair;
                        _stringSettings.Add(new KeyValueClass<string, string>
                        {
                            Key = key,
                            Value = value
                        });
                    });
                }
            }
            finally
            {
                _settingValues = false;
            }
        }

        private void ApplyControlValuesToSettings()
        {
            // Clear parameter vector so that only appropriate settings remain
            _parameterMap.Clear();

            // Image options
            if (_neverForceDespeckleRadioButton.Checked)
            {
                _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kForceDespeckleMode, VariantValue = EForceDespeckleMode.kNeverForce });
            }
            else
            {
                if (_alwaysForceDespeckleRadioButton.Checked)
                {
                    _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kForceDespeckleMode, VariantValue = EForceDespeckleMode.kAlwaysForce });
                }
                else
                {
                    _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kForceDespeckleMode, VariantValue = EForceDespeckleMode.kForceWhenBitonal });
                }

                var method = _forceDespeckleMethodComboBox.ToEnumValue<DESPECKLE_METHOD>();
                _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kForceDespeckleMethod, VariantValue = method });
                switch (method)
                {
                    case DESPECKLE_METHOD.DESPECKLE_NORMAL:
                    case DESPECKLE_METHOD.DESPECKLE_INVERSE:
                    case DESPECKLE_METHOD.DESPECKLE_HALFTONE:
                    case DESPECKLE_METHOD.DESPECKLE_PEPPER:
                    case DESPECKLE_METHOD.DESPECKLE_SALT:
                    case DESPECKLE_METHOD.DESPECKLE_PEPPERANDSALT:
                        _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kForceDespeckleLevel, VariantValue = (int)_forceDespeckleLevelNumericUpDown.Value });
                        break;
                    default:
                        break;
                }
            }

            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kAutoDespeckleMode, VariantValue = _autoDespeckleCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = "Kernel.Img.Max.Pix.X", VariantValue = (int)_maxXNumericUpDown.Value });
            _parameterMap.PushBack(new VariantPair { VariantKey = "Kernel.Img.Max.Pix.Y", VariantValue = (int)_maxYNumericUpDown.Value });

            // Recognition settings
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kZoneOrdering, VariantValue = _zoneOrderingCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kIgnoreAreaOutsideSpecifiedZone, VariantValue = _ignoreAreaOutsideSpecifiedZoneCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kLocateZonesInSpecifiedZone, VariantValue = _locateZonesInsideSpecifiedZoneCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kLimitToBasicLatinCharacters, VariantValue = _limitToBasicLatinCharactersCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kReturnUnrecognizedCharacters, VariantValue = _returnUnrecognizedCharactersCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kSkipPageOnFailure, VariantValue = _skipPageOnFailureCheckBox.Checked ? 1 : 0 });
            if (_skipPageOnFailureCheckBox.Checked)
            {
                _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kRequireOnePageSuccess, VariantValue = _requireOnePageSuccessCheckBox.Checked ? 1 : 0 });
                // Use int value even though these will be translated to uint by the OCR engine.
                // This is to be an example of how this ought to be done to allow for simpler future settings
                // (all enum settings should be able to be translated to and from the _enumSettings list without a change in type. Having a separate list for unsigned ints seems unnecessary since the list
                // isn't going to be very readable anyway)
                int maxPageFailureNumber = unchecked((int)Decimal.ToUInt32(_maxPageFailureNumberNumericUpDown.Value));
                _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kMaxPageFailureNumber, VariantValue = maxPageFailureNumber });
                _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kMaxPageFailurePercent, VariantValue = (int)_maxPageFailurePercentNumericUpDown.Value });
            }
            _parameterMap.PushBack(new VariantPair {
                VariantKey = EOCRParameter.kDefaultDecompositionMethod,
                VariantValue = _defaultDecompositionMethodComboBox.ToEnumValue<EPageDecompositionMethod>() });
            _parameterMap.PushBack(new VariantPair {
                VariantKey = EOCRParameter.kTradeoff,
                VariantValue = _accuracyTradeoffComboBox.ToEnumValue<EOcrTradeOff>() });
            _parameterMap.PushBack(new VariantPair {
                VariantKey = EOCRParameter.kDefaultFillingMethod,
                VariantValue = _defaultFillingMethodComboBox.ToEnumValue<FILLINGMETHOD>() });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kTimeout, VariantValue = (int)_timeoutNumericUpDown.Value });
            _parameterMap.PushBack(new VariantPair { VariantKey = "Kernel.OcrMgr.PreferAccurateEngine", VariantValue = _preferAccuracteEngineCheckBox.Checked ? 1 : 0 });

            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kOutputMultipleSpaceCharacterSequences, VariantValue = _outputMultipleSpaceCharacterSequencesCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kOutputOneSpaceCharacterPerCount, VariantValue = _outputOneSpaceCharPerCountCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kOutputTabCharactersForTabSpaceType, VariantValue = _outputTabCharactersCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kAssignSpatialInfoToSpaceCharacters, VariantValue = _assignSpatialInfoToSpaceCharsCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kIgnoreParagraphFlag, VariantValue = _ignoreParagraphFlagCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kTreatZonesAsParagraphs, VariantValue = _treatZonesAsParagraphsCheckBox.Checked ? 1 : 0 });

            // Language settings
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kSpecifyLanguage, VariantValue = _specifyRecognitionLanguagesCheckBox.Checked ? 1 : 0 });
            _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kSingleLanguageDetection, VariantValue = _singleLanguageDetectionCheckBox.Checked ? 1 : 0 });

            if (_specifyRecognitionLanguagesCheckBox.Checked)
            {
                foreach (var lang in _languages)
                {
                    _parameterMap.PushBack(new VariantPair { VariantKey = EOCRParameter.kLanguage, VariantValue = lang.Value });
                }
            }

            // Set advanced/unrecognized settings (ignored by this version of the software but may be used by a newer version)
            foreach (var kv in _enumSettings)
            {
                if (Int32.TryParse(kv.Value, out int iValue))
                {
                    _parameterMap.PushBack(new VariantPair { VariantKey = kv.Key, VariantValue = iValue });
                }
                else if (Double.TryParse(kv.Value, out double dValue))
                {
                    _parameterMap.PushBack(new VariantPair { VariantKey = kv.Key, VariantValue = dValue });
                }
            }

            // Set advanced string settings (directly applied to OCR engine)
            foreach (var kv in _stringSettings)
            {
                if (Int32.TryParse(kv.Value, out int iValue))
                {
                    _parameterMap.PushBack(new VariantPair { VariantKey = kv.Key, VariantValue = iValue });
                }
                else if (Double.TryParse(kv.Value, out double dValue))
                {
                    _parameterMap.PushBack(new VariantPair { VariantKey = kv.Key, VariantValue = dValue });
                }
                else
                {
                    _parameterMap.PushBack(new VariantPair { VariantKey = kv.Key, VariantValue = kv.Value });
                }
            }
        }

        private void SetDefaultRecognitionOptions()
        {
            _defaultDecompositionMethodComboBox.SelectEnumValue(EPageDecompositionMethod.kAutoDecomposition);
            _accuracyTradeoffComboBox.SelectEnumValue(EOcrTradeOff.kAccurate);
            _defaultFillingMethodComboBox.SelectEnumValue(FILLINGMETHOD.FM_OMNIFONT);
            _timeoutNumericUpDown.Value = 240000;
            _preferAccuracteEngineCheckBox.Checked = true;
            _zoneOrderingCheckBox.Checked = false;

            _skipPageOnFailureCheckBox.Checked = true;
            _requireOnePageSuccessCheckBox.Checked = true;
            _maxPageFailureNumberNumericUpDown.Value = uint.MaxValue;
            _maxPageFailurePercentNumericUpDown.Value = 100;

            _limitToBasicLatinCharactersCheckBox.Checked = false;
            _returnUnrecognizedCharactersCheckBox.Checked = false;
            _outputMultipleSpaceCharacterSequencesCheckBox.Checked = true;
            _outputOneSpaceCharPerCountCheckBox.Checked = true;
            _outputTabCharactersCheckBox.Checked = false;
            _assignSpatialInfoToSpaceCharsCheckBox.Checked = true;
            _ignoreParagraphFlagCheckBox.Checked = true;
            _treatZonesAsParagraphsCheckBox.Checked = true;
            _ignoreAreaOutsideSpecifiedZoneCheckBox.Checked = true;
            _locateZonesInsideSpecifiedZoneCheckBox.Checked = true;
        }

        private void SetClassicRecognitionOptions()
        {
            _defaultDecompositionMethodComboBox.SelectEnumValue(EPageDecompositionMethod.kAutoDecomposition);
            _accuracyTradeoffComboBox.SelectEnumValue(EOcrTradeOff.kAccurate);
            _defaultFillingMethodComboBox.SelectEnumValue(FILLINGMETHOD.FM_OMNIFONT);
            _timeoutNumericUpDown.Value = 120000;
            _preferAccuracteEngineCheckBox.Checked = true;
            _zoneOrderingCheckBox.Checked = true;

            _skipPageOnFailureCheckBox.Checked = false;
            _requireOnePageSuccessCheckBox.Checked = false;
            _maxPageFailureNumberNumericUpDown.Value = 10;
            _maxPageFailurePercentNumericUpDown.Value = 25;

            _limitToBasicLatinCharactersCheckBox.Checked = true;
            _returnUnrecognizedCharactersCheckBox.Checked = false;
            _outputMultipleSpaceCharacterSequencesCheckBox.Checked = false;
            _outputOneSpaceCharPerCountCheckBox.Checked = false;
            _outputTabCharactersCheckBox.Checked = false;
            _assignSpatialInfoToSpaceCharsCheckBox.Checked = false;
            _ignoreParagraphFlagCheckBox.Checked = false;
            _treatZonesAsParagraphsCheckBox.Checked = false;
            _ignoreAreaOutsideSpecifiedZoneCheckBox.Checked = false;
            _locateZonesInsideSpecifiedZoneCheckBox.Checked = false;
        }

        #endregion

        #region Private Classes

        [Obfuscation(Feature = "renaming", Exclude = true)]
        private class Language
        {
            public LANGUAGES Value { get; set; } = LANGUAGES.LANG_NO;

            public string DisplayName { get; set; }
        }

        [Obfuscation(Feature = "renaming", Exclude = true)]
        private class KeyValueClass<TKey, TValue>
        {
            public TKey Key { get; set; }

            public TValue Value { get; set; }
        }

        #endregion Private Classes

    }
}
