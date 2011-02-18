// RedactionTaskPP.cpp : Implementation of CRedactionTaskPP
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "RedactionTaskPP.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"

#include <Common.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const string gstrDEFAULT_VOA_FILE = "<SourceDocName>.voa";

//-------------------------------------------------------------------------------------------------
// CRedactionTaskPP
//-------------------------------------------------------------------------------------------------
CRedactionTaskPP::CRedactionTaskPP() : m_ipPdfSettings(NULL)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLERedactFileProcessorPP;
		m_dwHelpFileID = IDS_HELPFILERedactFileProcessorPP;
		m_dwDocStringID = IDS_DOCSTRINGRedactFileProcessorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI28586")
}
//-------------------------------------------------------------------------------------------------
CRedactionTaskPP::~CRedactionTaskPP()
{
	try
	{
		m_ipPdfSettings = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29793");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTaskPP::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch (...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTaskPP::Apply()
{
	int nResult = S_OK;

	try
	{
		// TODO: Validate before applying changes
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Ensure at least one attribute type is selected for redaction
			IVariantVectorPtr ipAttributeNames = getAttributeNames();
			if (ipAttributeNames != NULL && ipAttributeNames->Size <= 0)
			{
				MessageBox("Please specify at least one data category to redact.",
						"Select a data category", MB_YESNO | MB_ICONWARNING);
				m_radioSelectAttributes.SetFocus();
				return S_FALSE;
			}

			// Get Redaction File Processor object
			UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactionTaskPtr ipRedactFileProc = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI28587", ipRedactFileProc != NULL);

			// Retrieve and store output filename
			_bstr_t bstrFileName;
			m_editOutputFileName.GetWindowText(bstrFileName.GetAddress());

			// Assign the output file name and verify tags
			try
			{
				if (asString(bstrFileName) == "<SourceDocName>")
				{
					int nResult = MessageBox(
						"Source document will be overwritten. Modify original?",
						"Overwrite source document?", MB_YESNO | MB_ICONWARNING);
					if (nResult == IDYES)
					{
						ipRedactFileProc->OutputFileName = bstrFileName;
					}
					else
					{
						m_editOutputFileName.SetSel(0, -1);
						m_editOutputFileName.SetFocus();
						return S_FALSE;
					}
				}
				else
				{
					ipRedactFileProc->OutputFileName = bstrFileName;
				}
			}
			catch (...)
			{
				nResult = S_FALSE;
				m_editOutputFileName.SetSel(0, -1);
				m_editOutputFileName.SetFocus();

				throw;
			}

			// Ensure the PDF password settings are configured properly
			if (m_chkPdfSecurity.GetCheck() == BST_CHECKED)
			{
				IMustBeConfiguredObjectPtr ipConfigure = m_ipPdfSettings;
				if (ipConfigure == NULL || ipConfigure->IsConfigured() == VARIANT_FALSE)
				{
					MessageBox("Pdf security settings are not configured properly.",
						"PDF Security Not Configured", MB_ICONERROR);
					m_btnPdfSettings.SetFocus();
					return S_FALSE;
				}
			}

			// Retrieve whether to use the redacted image
			ipRedactFileProc->UseRedactedImage = 
				asVariantBool(m_radioUseRedactedImage.GetCheck() == BST_CHECKED);

			// Retrieve and store collected Attribute names
			ipRedactFileProc->AttributeNames = ipAttributeNames;

			// Retrieve and store Annotation settings
			ipRedactFileProc->CarryForwardAnnotations = asVariantBool(
				m_chkCarryAnnotation.GetCheck() == BST_CHECKED);

			ipRedactFileProc->ApplyRedactionsAsAnnotations = asVariantBool(
				m_chkRedactionAsAnnotation.GetCheck() == BST_CHECKED);

			// Assign the VOA file name
			ipRedactFileProc->VOAFileName = m_strVoaFileName.c_str();

			// Set the redaction text and options
			ipRedactFileProc->RedactionText = m_redactionAppearance.m_strText.c_str();
			ipRedactFileProc->AutoAdjustTextCasing =
				asVariantBool(m_redactionAppearance.m_bAdjustTextCasing);
			ipRedactFileProc->ReplacementValues = m_redactionAppearance.getReplacements();
			ipRedactFileProc->PrefixText = m_redactionAppearance.m_strPrefixText.c_str();
			ipRedactFileProc->SuffixText = m_redactionAppearance.m_strSuffixText.c_str();

			// Set the redaction colors
			ipRedactFileProc->BorderColor = m_redactionAppearance.m_crBorderColor;
			ipRedactFileProc->FillColor = m_redactionAppearance.m_crFillColor;

			// Set redaction appearance options
			ipRedactFileProc->FontName = m_redactionAppearance.m_lgFont.lfFaceName;
			ipRedactFileProc->IsBold = asVariantBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);
			ipRedactFileProc->IsItalic = asVariantBool(m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);
			ipRedactFileProc->FontSize = m_redactionAppearance.m_iPointSize;

			// Set PDF password settings
			if (m_chkPdfSecurity.GetCheck() == BST_CHECKED)
			{
				ipRedactFileProc->PdfPasswordSettings = m_ipPdfSettings;
			}
			else
			{
				ipRedactFileProc->PdfPasswordSettings = NULL;
			}
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28588");

	return nResult;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Data categories to redact
		m_radioAllAttributes = GetDlgItem(IDC_ALL_ATTRIBUTES);
		m_radioSelectAttributes = GetDlgItem(IDC_SELECT_ATTRIBUTES);;
		m_chkHCData = GetDlgItem(IDC_CHK_HCDATA);
		m_chkMCData = GetDlgItem(IDC_CHK_MCDATA);
		m_chkLCData = GetDlgItem(IDC_CHK_LCDATA);
		m_chkManual = GetDlgItem(IDC_CHK_MANUAL);
		m_chkOCData = GetDlgItem(IDC_CHK_OCDATA);
		m_editAttributes = GetDlgItem(IDC_ATTRIBUTES);

		// Output file
		m_editOutputFileName = GetDlgItem(IDC_OUTPUT_FILENAME);
		m_btnSelectImageFileTag.SubclassDlgItem(IDC_BTN_SELECT_IMAGE_FILE_TAG, CWnd::FromHandle(m_hWnd));
		m_btnSelectImageFileTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_radioUseRedactedImage = GetDlgItem(IDC_USE_REDACTED_IMAGE);
		m_radioUseOriginalImage = GetDlgItem(IDC_USE_ORIGINAL_IMAGE);
		m_chkCarryAnnotation = GetDlgItem(IDC_CHECK_CARRY_ANNOTATIONS);
		m_chkRedactionAsAnnotation = GetDlgItem(IDC_CHECK_REDACTIONS_AS_ANNOTATIONS);
		m_chkPdfSecurity = GetDlgItem(IDC_CHECK_PDF_SECURITY);
		m_btnPdfSettings = GetDlgItem(IDC_BTN_PDF_SECURITY_SETTINGS);

		// Data file
		m_stcDataFile = GetDlgItem(IDC_STATIC_DATA_FILE_DESCRIPTION);

		// Redaction text and color settings
		m_btnRedactionAppearance = GetDlgItem(IDC_BUTTON_REDACTION_APPEARANCE);

		// Get Redaction File Processor object
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactionTaskPtr ipRedactFileProc = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI28589", ipRedactFileProc != NULL);

		//////////////////////////
		// Initialize data members
		//////////////////////////

		// Output file
		string strOutputFileName = asString(ipRedactFileProc->OutputFileName);
		m_editOutputFileName.SetWindowText(strOutputFileName.c_str()); 

		// Use redacted image
		bool bUseRedactedImage = asCppBool(ipRedactFileProc->UseRedactedImage);
		m_radioUseRedactedImage.SetCheck(asBSTChecked(bUseRedactedImage));
		m_radioUseOriginalImage.SetCheck(asBSTChecked(!bUseRedactedImage));

		// default the attribute check boxes to unchecked
		m_chkHCData.SetCheck(BST_UNCHECKED);
		m_chkMCData.SetCheck(BST_UNCHECKED);
		m_chkLCData.SetCheck(BST_UNCHECKED);
		m_chkManual.SetCheck(BST_UNCHECKED);
		m_chkOCData.SetCheck(BST_UNCHECKED);

		// Collected Attribute names
		putAttributeNames(ipRedactFileProc->AttributeNames);

		// Default to <SourceDocName>.voa if no VOA File name provided
		// [FlexIDSCore #3382]
		m_strVoaFileName = asString(ipRedactFileProc->VOAFileName);
		if (m_strVoaFileName.empty())
		{
			m_strVoaFileName = gstrDEFAULT_VOA_FILE;
		}

		// Apply Annotation settings
		m_chkCarryAnnotation.SetCheck(asBSTChecked(ipRedactFileProc->CarryForwardAnnotations));
		m_chkRedactionAsAnnotation.SetCheck(
			asBSTChecked(ipRedactFileProc->ApplyRedactionsAsAnnotations));

		// Disable the checkboxes if not licensed
		if (!LicenseManagement::sGetInstance().isAnnotationLicensed())
		{
			m_chkCarryAnnotation.EnableWindow(FALSE);
			m_chkRedactionAsAnnotation.EnableWindow(FALSE);
		}

		// Get the redaction text
		m_redactionAppearance.m_strText = asString(ipRedactFileProc->RedactionText);
		m_redactionAppearance.m_bAdjustTextCasing = asCppBool(ipRedactFileProc->AutoAdjustTextCasing);

		m_redactionAppearance.updateReplacementsFromVector(ipRedactFileProc->ReplacementValues);
		m_redactionAppearance.m_strPrefixText = asString(ipRedactFileProc->PrefixText);
		m_redactionAppearance.m_strSuffixText = asString(ipRedactFileProc->SuffixText);

		// Get the redaction colors
		m_redactionAppearance.m_crBorderColor = ipRedactFileProc->BorderColor;
		m_redactionAppearance.m_crFillColor = ipRedactFileProc->FillColor;

		// Get the font
		string strFontName = asString(ipRedactFileProc->FontName);
		LPTSTR result = lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, 
			strFontName.c_str(), LF_FACESIZE);
		if (result == NULL)
		{
			UCLIDException uex("ELI29782", "Unable to copy Font name.");
			uex.addDebugInfo("Font Name To Copy", strFontName);
			uex.addDebugInfo("Font Name Length", strFontName.length());
			uex.addDebugInfo("Max Length", LF_FACESIZE);
			throw uex;
		}
		m_redactionAppearance.m_lgFont.lfWeight = 
			 ipRedactFileProc->IsBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;
		m_redactionAppearance.m_lgFont.lfItalic = 
			ipRedactFileProc->IsItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;
		m_redactionAppearance.m_iPointSize = ipRedactFileProc->FontSize;

		ICopyableObjectPtr ipCopy = ipRedactFileProc->PdfPasswordSettings;
		bool bPdfSecurity = ipCopy != NULL;
		if (bPdfSecurity)
		{
			m_ipPdfSettings = ipCopy->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI29783", m_ipPdfSettings != NULL);
		}
		m_chkPdfSecurity.SetCheck(asBSTChecked(bPdfSecurity));
		m_btnPdfSettings.EnableWindow(asMFCBool(bPdfSecurity));

		//////////////////////////
		// Enable/disable controls
		//////////////////////////
		updateAttributeGroup();
		updateDataFileDescription();

		// Clear dirty flag
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28590");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnClickedBtnBrowseOutput(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "TIF Files (*.tif)|*.tif|PDF files (*.pdf)|*.pdf||";

		// bring open file dialog
		CFileDialog fileDlg(TRUE, ".tif", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_HIDEREADONLY, s_strAllFiles.c_str(), CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// if the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			m_editOutputFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28591");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnClickedBtnAllAttributes(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
													BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateAttributeGroup();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28592");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnClickedBtnSelectAttributes(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
													   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateAttributeGroup();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28593");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnClickedBtnOCData(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
											 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_chkOCData.GetCheck() == BST_CHECKED)
		{
			m_editAttributes.EnableWindow(TRUE);
		}
		else
		{
			m_editAttributes.EnableWindow(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28594");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnClickedSelectImageFileTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
													  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Display the tags and set the selected to the edit box
		RECT rect;
		m_btnSelectImageFileTag.GetWindowRect(&rect);
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			m_editOutputFileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28595");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnBnClickedButtonRedactAppearance(WORD wNotifyCode, WORD wID, 
															HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CRedactionAppearanceDlg dialog(m_redactionAppearance);
		if (dialog.DoModal() == IDOK)
		{
			dialog.getOptions(m_redactionAppearance);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28596");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnClickedButtonDataFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get SelectTargetFile object
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::ISelectTargetFileUIPtr ipFileSelector(CLSID_SelectTargetFileUI);
		ASSERT_RESOURCE_ALLOCATION("ELI28597", ipFileSelector != NULL);

		// Initialize parameters
		ipFileSelector->Title = "Specify ID Shield data file path";
		ipFileSelector->Instructions = "ID Shield data file";
		ipFileSelector->DefaultFileName = gstrDEFAULT_TARGET_FILENAME.c_str();
		ipFileSelector->DefaultExtension = ".voa";
		ipFileSelector->FileTypes = "VOA Files (*.voa)|*.voa||";
		ipFileSelector->FileName = m_strVoaFileName.c_str();
		
		// Prompt for new data file setting
		if (ipFileSelector->PromptForFile() == VARIANT_TRUE)
		{
			m_strVoaFileName = asString(ipFileSelector->FileName);
			updateDataFileDescription();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28598");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnClickedCheckPdfSecurity(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_btnPdfSettings.EnableWindow(asMFCBool(m_chkPdfSecurity.GetCheck() == BST_CHECKED));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29778");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionTaskPP::OnClickedBtnPdfSettings(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_ipPdfSettings == NULL)
		{
			m_ipPdfSettings.CreateInstance(CLSID_PdfPasswordSettings);
			ASSERT_RESOURCE_ALLOCATION("ELI29779", m_ipPdfSettings != NULL);

			// Enforce setting both passwords due to Leadtools bug [LRCAU #5749]
			m_ipPdfSettings->RequireUserAndOwnerPassword = VARIANT_TRUE;
		}

		// Run the configuration for the PDF security settings
		IConfigurableObjectPtr ipConfigure = m_ipPdfSettings;
		ASSERT_RESOURCE_ALLOCATION("ELI29780", ipConfigure != NULL);
		ipConfigure->RunConfiguration();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29781");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRedactionTaskPP::updateAttributeGroup()
{
	// check which radio button is selected
	if (m_radioSelectAttributes.GetCheck() == BST_CHECKED)
	{
		// enable the check boxes
		m_chkHCData.EnableWindow(TRUE);
		m_chkMCData.EnableWindow(TRUE);
		m_chkLCData.EnableWindow(TRUE);
		m_chkManual.EnableWindow(TRUE);
		m_chkOCData.EnableWindow(TRUE);

		// if the OCData checkbox is checked then need to enable the attribute edit box
		BOOL bEnabled = asMFCBool(m_chkOCData.GetCheck() == BST_CHECKED);
		m_editAttributes.EnableWindow(bEnabled);
	}
	else
	{
		// all selected, disable all controls under selected attribute
		m_chkHCData.EnableWindow(FALSE);
		m_chkMCData.EnableWindow(FALSE);
		m_chkLCData.EnableWindow(FALSE);
		m_chkManual.EnableWindow(FALSE);
		m_chkOCData.EnableWindow(FALSE);
		m_editAttributes.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void CRedactionTaskPP::updateDataFileDescription()
{
	if (m_strVoaFileName == gstrDEFAULT_TARGET_FILENAME)
	{
		m_stcDataFile.SetWindowText(gstrDEFAULT_TARGET_MESSAGE.c_str());
	}
	else
	{
		m_stcDataFile.SetWindowText(m_strVoaFileName.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CRedactionTaskPP::getAttributeNames()
{
	IVariantVectorPtr ipAttributeNames = NULL;
	if (m_radioSelectAttributes.GetCheck() == 1)
	{
		vector<string> vecTokens;
		// check the state of each check box
		// and add appropriate data to the list
		// NOTE: handle the OCData first since sGetTokens will clear the
		// vector that is passed into it
		if (m_chkOCData.GetCheck() == BST_CHECKED)
		{
			// get the text from the attribute edit box
			CString zAttributeText;
			m_editAttributes.GetWindowText(zAttributeText);
			string strAttributeText(zAttributeText);
			
			// Allow comma OR semicolon to be used as delimiter between names (P16 #2571)
			// So change all semicolon characters to commas before tokenization
			replaceVariable(strAttributeText, string(";").c_str(), string(",").c_str());

			// Separate into the various names
			// TODO: need validation of the Attributes
			StringTokenizer::sGetTokens(strAttributeText, ',', vecTokens);
		}
		if (m_chkHCData.GetCheck() == BST_CHECKED)
		{
			vecTokens.push_back("HCData");
		}
		if (m_chkMCData.GetCheck() == BST_CHECKED)
		{
			vecTokens.push_back("MCData");
		}
		if (m_chkLCData.GetCheck() == BST_CHECKED)
		{
			vecTokens.push_back("LCData");
		}
		if (m_chkManual.GetCheck() == BST_CHECKED)
		{
			vecTokens.push_back("Manual");
		}

		ipAttributeNames.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI28599", ipAttributeNames != NULL);

		int nNumOfTokens = vecTokens.size();
		for (int i = 0; i < nNumOfTokens; i++)
		{
			string strTrimmed = trim(vecTokens[i], " ", " ");
			ipAttributeNames->PushBack(strTrimmed.c_str());
		}
	}
	return ipAttributeNames;
}
//-------------------------------------------------------------------------------------------------
void CRedactionTaskPP::putAttributeNames(IVariantVectorPtr ipAttributeNames)
{
	string strAttributeNames = "";

	// as long as there is a valid attribute vector, set the check box
	// and attribute edit box data according to the vector
	if (ipAttributeNames != NULL)
	{
		m_radioSelectAttributes.SetCheck(TRUE);

		int nNumOfAttributeNames = ipAttributeNames->Size;
		for (int i = 0; i < nNumOfAttributeNames; i++)
		{
			string strCurrName = asString(_bstr_t(ipAttributeNames->GetItem(i)));
			
			// check in a case insensitive way [p16 #2680]
			string strCurrNameLC = strCurrName;
			makeLowerCase(strCurrNameLC);
			if (strCurrNameLC == "hcdata")
			{
				m_chkHCData.SetCheck(BST_CHECKED);
			}
			else if (strCurrNameLC == "mcdata")
			{
				m_chkMCData.SetCheck(BST_CHECKED);
			}
			else if (strCurrNameLC == "lcdata")
			{
				m_chkLCData.SetCheck(BST_CHECKED);
			}
			else if (strCurrNameLC == "manual")
			{
				m_chkManual.SetCheck(BST_CHECKED);
			}
			else
			{
				m_chkOCData.SetCheck(BST_CHECKED);
				if (!strAttributeNames.empty())
				{
					strAttributeNames.append(", ");
				}
				strAttributeNames.append(strCurrName);
			}
		}
	}
	else
	{
		m_radioAllAttributes.SetCheck(TRUE);
	}
	updateAttributeGroup();
	m_editAttributes.SetWindowText(strAttributeNames.c_str());
}
//-------------------------------------------------------------------------------------------------
void CRedactionTaskPP::validateLicense()
{
	VALIDATE_LICENSE(gnIDSHIELD_AUTOREDACTION_OBJECT, "ELI28600", "Redaction Task PP");
}
//-------------------------------------------------------------------------------------------------
