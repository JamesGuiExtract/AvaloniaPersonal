using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

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
        private LongToLongMap _parameterMap;
        private bool _readOnly;
        private bool _settingValues;

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

                LANGUAGES.LANG_NO.SetReadableValue("[None]");
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
                LANGUAGES.LANG_ESK.SetReadableValue("Eskimo language selection. This");
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
                _language1ComboBox.InitializeWithReadableEnum<LANGUAGES>(true);
                _language2ComboBox.InitializeWithReadableEnum<LANGUAGES>(true);
                _language3ComboBox.InitializeWithReadableEnum<LANGUAGES>(true);
                _language4ComboBox.InitializeWithReadableEnum<LANGUAGES>(true);
                _language5ComboBox.InitializeWithReadableEnum<LANGUAGES>(true);
                SetValues();
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
                _parameterMap = pParams.OCRParameters;
                _readOnly = vbReadOnly;

                // Display the dialog centered on the parent
                NativeWindow parentWindow = new NativeWindow();
                parentWindow.AssignHandle((IntPtr)nHandle);
                if (ShowDialog(parentWindow) == DialogResult.OK)
                {
                    ApplyValues();
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
        
        #endregion

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

            if (_specifyRecognitionLanguagesCheckBox.Checked)
            {
                _recognitionLanguagesGroupBox.Enabled = true;
            }
            else
            {
                _recognitionLanguagesGroupBox.Enabled = false;
            }
        }

        private void SetValues()
        {
            _settingValues = true;
            try
            {
                _forceDespeckleMethodComboBox.SelectEnumValue(DESPECKLE_METHOD.DESPECKLE_AUTO);
                _language1ComboBox.SelectEnumValue(LANGUAGES.LANG_NO);
                _language2ComboBox.SelectEnumValue(LANGUAGES.LANG_NO);
                _language3ComboBox.SelectEnumValue(LANGUAGES.LANG_NO);
                _language4ComboBox.SelectEnumValue(LANGUAGES.LANG_NO);
                _language5ComboBox.SelectEnumValue(LANGUAGES.LANG_NO);

                for (int i = 0; i < _parameterMap.Size; i++)
                {
                    _parameterMap.GetKeyValue(i, out int key, out int value);
                    EOCRParameter parameter = (EOCRParameter)key;

                    switch (parameter)
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
                        case EOCRParameter.kKernel_Img_Max_Pix_X:
                            _maxXNumericUpDown.Value = value;
                            break;
                        case EOCRParameter.kKernel_Img_Max_Pix_Y:
                            _maxYNumericUpDown.Value = value;
                            break;
                        case EOCRParameter.kZoneOrdering:
                            _zoneOrderingCheckBox.Checked = value != 0;
                            break;
                        case EOCRParameter.kLimitToBasicLatinCharacters:
                            _limitToBasicLatinCharactersCheckBox.Checked = value != 0;
                            break;
                        case EOCRParameter.kSpecifyLanguage:
                            _specifyRecognitionLanguagesCheckBox.Checked = value != 0;
                            break;
                        case EOCRParameter.kSingleLanguageDetection:
                            _singleLanguageDetectionCheckBox.Checked = value != 0;
                            break;
                        case EOCRParameter.kLanguage1:
                            _language1ComboBox.SelectEnumValue((LANGUAGES)value);
                            break;
                        case EOCRParameter.kLanguage2:
                            _language2ComboBox.SelectEnumValue((LANGUAGES)value);
                            break;
                        case EOCRParameter.kLanguage3:
                            _language3ComboBox.SelectEnumValue((LANGUAGES)value);
                            break;
                        case EOCRParameter.kLanguage4:
                            _language4ComboBox.SelectEnumValue((LANGUAGES)value);
                            break;
                        case EOCRParameter.kLanguage5:
                            _language5ComboBox.SelectEnumValue((LANGUAGES)value);
                            break;
                    }

                    //RecAPI.kRecSetLicense(null, "9d478fe171d5");
                    //RecAPI.kRecInit(null, null);
                    //RecAPI.kRecLoadSettings(0, @"D:\OCRSettings.txt");

                    //RecAPI.kRecSettingGetHandle(IntPtr.Zero, "Kernel.Img.Max.Pix.X", out var hSetting);
                    //RecAPI.kRecSettingGetInt(0, hSetting, out value);
                    //_maxXNumericUpDown.Value = value;

                    //RecAPI.kRecSettingGetHandle(IntPtr.Zero, "Kernel.Img.Max.Pix.Y", out hSetting);
                    //RecAPI.kRecSettingGetInt(0, hSetting, out value);
                    //_maxYNumericUpDown.Value = value;

                    //RecAPI.kRecQuit();
                }
            }
            finally
            {
                _settingValues = false;
            }
        }

        private void ApplyValues()
        {
            _parameterMap.Clear();

            if (_neverForceDespeckleRadioButton.Checked)
            {
                _parameterMap.Set((int)EOCRParameter.kForceDespeckleMode, (int)EForceDespeckleMode.kNeverForce);

                // Remove N/A values from the map
                _parameterMap.RemoveItem((int)EOCRParameter.kForceDespeckleMethod);
                _parameterMap.RemoveItem((int)EOCRParameter.kForceDespeckleLevel);
            }
            else
            {
                if (_alwaysForceDespeckleRadioButton.Checked)
                {
                    _parameterMap.Set((int)EOCRParameter.kForceDespeckleMode, (int)EForceDespeckleMode.kAlwaysForce);
                }
                else
                {
                    _parameterMap.Set((int)EOCRParameter.kForceDespeckleMode, (int)EForceDespeckleMode.kForceWhenBitonal);
                }

                var method = _forceDespeckleMethodComboBox.ToEnumValue<DESPECKLE_METHOD>();
                _parameterMap.Set((int)EOCRParameter.kForceDespeckleMethod, (int)method);
                switch (method)
                {
                    case DESPECKLE_METHOD.DESPECKLE_NORMAL:
                    case DESPECKLE_METHOD.DESPECKLE_INVERSE:
                    case DESPECKLE_METHOD.DESPECKLE_HALFTONE:
                    case DESPECKLE_METHOD.DESPECKLE_PEPPER:
                    case DESPECKLE_METHOD.DESPECKLE_SALT:
                    case DESPECKLE_METHOD.DESPECKLE_PEPPERANDSALT:
                        _parameterMap.Set((int)EOCRParameter.kForceDespeckleLevel, (int)_forceDespeckleLevelNumericUpDown.Value);
                        break;
                    default:
                        _parameterMap.RemoveItem((int)EOCRParameter.kForceDespeckleLevel);
                        break;
                }
            }

            _parameterMap.Set((int)EOCRParameter.kAutoDespeckleMode, _autoDespeckleCheckBox.Checked ? 1 : 0);
            _parameterMap.Set((int)EOCRParameter.kKernel_Img_Max_Pix_X, (int)_maxXNumericUpDown.Value);
            _parameterMap.Set((int)EOCRParameter.kKernel_Img_Max_Pix_Y, (int)_maxYNumericUpDown.Value);
            _parameterMap.Set((int)EOCRParameter.kZoneOrdering, _zoneOrderingCheckBox.Checked ? 1 : 0);
            _parameterMap.Set((int)EOCRParameter.kLimitToBasicLatinCharacters, _limitToBasicLatinCharactersCheckBox.Checked ? 1 : 0);
            _parameterMap.Set((int)EOCRParameter.kSpecifyLanguage, _specifyRecognitionLanguagesCheckBox.Checked ? 1 : 0);
            _parameterMap.Set((int)EOCRParameter.kSingleLanguageDetection, _singleLanguageDetectionCheckBox.Checked ? 1 : 0);

            if (_specifyRecognitionLanguagesCheckBox.Checked)
            {
                var languages = new[]
                {
                _language1ComboBox.ToEnumValue<LANGUAGES>(),
                _language2ComboBox.ToEnumValue<LANGUAGES>(),
                _language3ComboBox.ToEnumValue<LANGUAGES>(),
                _language4ComboBox.ToEnumValue<LANGUAGES>(),
                _language5ComboBox.ToEnumValue<LANGUAGES>()
            }
                .Distinct()
                .OrderBy(l => l)
                .Where(l => l != LANGUAGES.LANG_NO)
                .Select((l, i) => ((int)l, i));
                foreach (var (language, index) in languages)
                {
                    var key = (int)EOCRParameter.kLanguage1 + index;
                    _parameterMap.Set(key, language);
                }
            }
        }

        #endregion
    }
}
