// RedactFileProcessorPP.cpp : Implementation of CRedactFileProcessorPP
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "RedactFileProcessorPP.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"
#include "..\..\..\..\AFCore\Code\Common.h"

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
// CRedactFileProcessorPP
//-------------------------------------------------------------------------------------------------
CRedactFileProcessorPP::CRedactFileProcessorPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLERedactFileProcessorPP;
		m_dwHelpFileID = IDS_HELPFILERedactFileProcessorPP;
		m_dwDocStringID = IDS_DOCSTRINGRedactFileProcessorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI11530")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessorPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessorPP::Apply(void)
{
	ATLTRACE(_T("CRedactFileProcessorPP::Apply\n"));
	int nResult = S_OK;

	try
	{
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Get Redaction File Processor object
			UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactFileProcessorPtr ipRedactFileProc = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI09868", ipRedactFileProc != NULL);

			// Retrieve and store RSD filename
			_bstr_t bstrFileName;
			m_editRuleFileName.GetWindowText(bstrFileName.GetAddress());

			// Assign the rsd file name and validate the tags
			try
			{
				ipRedactFileProc->RuleFileName = bstrFileName;
			}
			catch(...)
			{
				nResult = S_FALSE;
				m_editRuleFileName.SetSel(0, -1);
				m_editRuleFileName.SetFocus();

				throw;
			}

			// Retrieve and store output filename
			bstrFileName = "";
			m_editOutputFileName.GetWindowText(bstrFileName.GetAddress());

			// Assign the output file name and verify tags
			try
			{
				if ( asString(bstrFileName) == "<SourceDocName>" )
				{
					int nResult = MessageBox(
						"Source Document will be overwritten! Modify original?",
						"Overwrite Source Document?", MB_YESNO | MB_ICONWARNING);
					if ( nResult == IDYES )
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
			catch(...)
			{
				nResult = S_FALSE;
				m_editOutputFileName.SetSel(0, -1);
				m_editOutputFileName.SetFocus();

				throw;
			}

			// Retrieve whether to use the redacted image
			ipRedactFileProc->UseRedactedImage = 
				asVariantBool(m_radioUseRedactedImage.GetCheck() == BST_CHECKED);

			// Retrieve and store ReadFromUSS
			ipRedactFileProc->ReadFromUSS = asVariantBool( m_chkReadUSS.GetCheck() == BST_CHECKED );

			// Retrieve and store collected Attribute names
			ipRedactFileProc->AttributeNames = getAttributeNames();

			// Retrieve and store Annotation settings
			ipRedactFileProc->CarryForwardAnnotations = asVariantBool( 
				m_chkCarryAnnotation.GetCheck() == BST_CHECKED );

			ipRedactFileProc->ApplyRedactionsAsAnnotations = asVariantBool( 
				m_chkRedactionAsAnnotation.GetCheck() == BST_CHECKED );

			// Retrieve and store setting for Output File Creation
			bool bAlways = (m_radioOutputAlways.GetCheck() == BST_CHECKED);
			ipRedactFileProc->CreateOutputFile = bAlways ? 0 : 1;

			bool bUseVOA = asCppBool(m_chkUseVOA.GetCheck() == BST_CHECKED);
			ipRedactFileProc->UseVOA = asVariantBool(bUseVOA);
	
			// Assign the VOA file name and verify tags
			try
			{
				bstrFileName = "";
				if (bUseVOA)
				{
					m_editVOAFileName.GetWindowText(bstrFileName.GetAddress());
				}
				ipRedactFileProc->VOAFileName = bstrFileName;
			}
			catch(...)
			{
				nResult = S_FALSE;
				m_editVOAFileName.SetSel(0, -1);
				m_editVOAFileName.SetFocus();

				throw;
			}

			// Set the redaction text
			ipRedactFileProc->RedactionText = m_redactionAppearance.m_strText.c_str();

			// Set the redaction colors
			ipRedactFileProc->BorderColor = m_redactionAppearance.m_crBorderColor;
			ipRedactFileProc->FillColor = m_redactionAppearance.m_crFillColor;

			// Set redaction appearance options
			ipRedactFileProc->FontName = m_redactionAppearance.m_lgFont.lfFaceName;
			ipRedactFileProc->IsBold = asVariantBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);
			ipRedactFileProc->IsItalic = asVariantBool(m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);
			ipRedactFileProc->FontSize = m_redactionAppearance.m_iPointSize;
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09867");

	return nResult;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Prepare control members
		m_editRuleFileName = GetDlgItem(IDC_RULES_FILENAME);
		m_editOutputFileName = GetDlgItem(IDC_OUTPUT_FILENAME);
		m_radioUseRedactedImage = GetDlgItem(IDC_USE_REDACTED_IMAGE);
		m_radioUseOriginalImage = GetDlgItem(IDC_USE_ORIGINAL_IMAGE);
		m_chkReadUSS = GetDlgItem(IDC_CHK_FROM_USS);
		m_radioAllAttributes = GetDlgItem(IDC_ALL_ATTRIBUTES);
		m_radioSelectAttributes = GetDlgItem(IDC_SELECT_ATTRIBUTES);;
		m_editAttributes = GetDlgItem(IDC_ATTRIBUTES);
		m_radioOutputAlways = GetDlgItem(IDC_OUTPUT_ALWAYS);
		m_radioOutputOnly = GetDlgItem(IDC_OUTPUT_ONLY);
		m_btnSelectRulesFileTag.SubclassDlgItem(IDC_BTN_SELECT_RULES_FILE_TAG, CWnd::FromHandle(m_hWnd));
		m_btnSelectRulesFileTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_btnSelectImageFileTag.SubclassDlgItem(IDC_BTN_SELECT_IMAGE_FILE_TAG, CWnd::FromHandle(m_hWnd));
		m_btnSelectImageFileTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_btnSelectVOAFileTag.SubclassDlgItem(IDC_BTN_SELECT_VOA_FILE_TAG, CWnd::FromHandle(m_hWnd));
		m_btnSelectVOAFileTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_chkUseVOA = GetDlgItem(IDC_BTN_USEVOA);
		m_editVOAFileName = GetDlgItem(IDC_VOA_FILENAME);
		m_btnBrowseVOA = GetDlgItem(IDC_BTN_VOA_BROWSE_FILE);
		m_chkCarryAnnotation = GetDlgItem( IDC_CHECK_CARRY_ANNOTATIONS );
		m_chkRedactionAsAnnotation = GetDlgItem( IDC_CHECK_REDACTIONS_AS_ANNOTATIONS );
		m_chkHCData = GetDlgItem(IDC_CHK_HCDATA);
		m_chkMCData = GetDlgItem(IDC_CHK_MCDATA);
		m_chkLCData = GetDlgItem(IDC_CHK_LCDATA);
		m_chkOCData = GetDlgItem(IDC_CHK_OCDATA);
		m_btnRedactionAppearance = GetDlgItem(IDC_BUTTON_REDACTION_APPEARANCE);

		// Get Redaction File Processor object
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactFileProcessorPtr ipRedactFileProc = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI09875", ipRedactFileProc != NULL);

		//////////////////////////
		// Initialize data members
		//////////////////////////
		// RSD file
		string strRuleFileName = asString(ipRedactFileProc->RuleFileName);
		m_editRuleFileName.SetWindowText(strRuleFileName.c_str());

		// Output file
		string strOutputFileName = asString(ipRedactFileProc->OutputFileName);
		m_editOutputFileName.SetWindowText(strOutputFileName.c_str()); 

		// Use redacted image
		bool bUseRedactedImage = asCppBool(ipRedactFileProc->UseRedactedImage);
		m_radioUseRedactedImage.SetCheck( asBSTChecked(bUseRedactedImage) );
		m_radioUseOriginalImage.SetCheck( asBSTChecked(!bUseRedactedImage) );

		// Read from USS
		bool bCheck = ipRedactFileProc->ReadFromUSS == VARIANT_TRUE;
		m_chkReadUSS.SetCheck(bCheck);

		// default the attribute check boxes to unchecked
		m_chkHCData.SetCheck(BST_UNCHECKED);
		m_chkMCData.SetCheck(BST_UNCHECKED);
		m_chkLCData.SetCheck(BST_UNCHECKED);
		m_chkOCData.SetCheck(BST_UNCHECKED);

		// Collected Attribute names
		putAttributeNames(ipRedactFileProc->AttributeNames);

		// Output file creation
		long lValue = ipRedactFileProc->CreateOutputFile;
		if (lValue == 0)
		{
			// Always create output file
			m_radioOutputAlways.SetCheck( TRUE );
		}
		else
		{
			// Only create output file if redactable data was found
			m_radioOutputOnly.SetCheck( TRUE );
		}

		// VOA checkbox and filename
		m_chkUseVOA.SetCheck(asBSTChecked(ipRedactFileProc->UseVOA));

		// Default to <SourceDocName>.voa if no VOA File name provided
		// [FlexIDSCore #3382]
		string strVOAFileName = asString(ipRedactFileProc->VOAFileName);
		if (strVOAFileName.empty())
		{
			strVOAFileName = gstrDEFAULT_VOA_FILE;
		}
		m_editVOAFileName.SetWindowText(strVOAFileName.c_str());

		// Apply Annotation settings
		m_chkCarryAnnotation.SetCheck(asBSTChecked(ipRedactFileProc->CarryForwardAnnotations));
		m_chkRedactionAsAnnotation.SetCheck(
			asBSTChecked(ipRedactFileProc->ApplyRedactionsAsAnnotations));

		// Disable the checkboxes if not licensed
		if (!LicenseManagement::sGetInstance().isAnnotationLicensed() )
		{
			m_chkCarryAnnotation.EnableWindow( FALSE );
			m_chkRedactionAsAnnotation.EnableWindow( FALSE );
		}

		// Get the redaction text
		m_redactionAppearance.m_strText = asString(ipRedactFileProc->RedactionText);

		// Get the redaction colors
		m_redactionAppearance.m_crBorderColor = ipRedactFileProc->BorderColor;
		m_redactionAppearance.m_crFillColor = ipRedactFileProc->FillColor;

		// Get the font
		lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, 
			asString(ipRedactFileProc->FontName).c_str(), LF_FACESIZE);
		m_redactionAppearance.m_lgFont.lfWeight = 
			 ipRedactFileProc->IsBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;
		m_redactionAppearance.m_lgFont.lfItalic = 
			ipRedactFileProc->IsItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;
		m_redactionAppearance.m_iPointSize = ipRedactFileProc->FontSize;

		//////////////////////////
		// Enable/disable controls
		//////////////////////////
		updateAttributeGroup();
		updateVOAGroup();

		// Clear dirty flag
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09873");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedBtnBrowseRules(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// bring open file dialog
		CFileDialog fileDlg(TRUE, ".rsd", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST, gstrRSD_FILE_OPEN_FILTER.c_str(), CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// if the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			// get the file name
			m_editRuleFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09874");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedBtnBrowseOutput(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "TIF Files (*.tif)|*.tif||";

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09993");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedBtnAllAttributes(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateAttributeGroup();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11775");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedBtnSelectAttributes(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateAttributeGroup();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11776");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedBtnOCData(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19591");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedSelectRulesFileTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Display the tags and set the selected to the edit box
		RECT rect;
		m_btnSelectRulesFileTag.GetWindowRect(&rect);
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			m_editRuleFileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12010");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedSelectImageFileTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12011");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedSelectVOAFileTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Display the tags and set the selected to the edit box
		RECT rect;
		m_btnSelectVOAFileTag.GetWindowRect(&rect);
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			m_editVOAFileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12711");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedUseVOAFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		updateVOAGroup();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12712");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnClickedBtnBrowseVOAFile(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "VOA Files (*.voa)|*.voa||";

		// bring open file dialog
		CFileDialog fileDlg(TRUE, ".voa", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_HIDEREADONLY, s_strAllFiles.c_str(), CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// if the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			m_editVOAFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12713");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactFileProcessorPP::OnBnClickedButtonRedactAppearance(WORD wNotifyCode, WORD wID, 
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24749");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRedactFileProcessorPP::updateAttributeGroup()
{
	// check which radio button is selected
	if (m_radioSelectAttributes.GetCheck() == 1 )
	{
		// enable the check boxes
		m_chkHCData.EnableWindow(TRUE);
		m_chkMCData.EnableWindow(TRUE);
		m_chkLCData.EnableWindow(TRUE);
		m_chkOCData.EnableWindow(TRUE);

		// if the OCData checkbox is checked then need to enable
		// the attribute edit box
		if (m_chkOCData.GetCheck() == BST_CHECKED)
		{
			m_editAttributes.EnableWindow(TRUE);
		}
		else
		{
			m_editAttributes.EnableWindow(FALSE);
		}
	}
	else
	{
		// all selected, disable all controls under selected attribute
		m_chkHCData.EnableWindow(FALSE);
		m_chkMCData.EnableWindow(FALSE);
		m_chkLCData.EnableWindow(FALSE);
		m_chkOCData.EnableWindow(FALSE);
		m_editAttributes.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CRedactFileProcessorPP::getAttributeNames()
{
	IVariantVectorPtr ipAttributeNames = NULL;
	if ( m_radioSelectAttributes.GetCheck() == 1 )
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
			replaceVariable( strAttributeText, string(";").c_str(), string(",").c_str() );

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

		ipAttributeNames.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI11782", ipAttributeNames != NULL );

		int nNumOfTokens = vecTokens.size();
		for ( int i = 0; i < nNumOfTokens; i++ )
		{
			string strTrimmed = trim(vecTokens[i], " ", " " );
			ipAttributeNames->PushBack(strTrimmed.c_str());
		}
	}
	return ipAttributeNames;
}
//-------------------------------------------------------------------------------------------------
void CRedactFileProcessorPP::putAttributeNames(IVariantVectorPtr ipAttributeNames)
{
	string strAttributeNames = "";

	// as long as there is a valid attribute vector, set the check box
	// and attribute edit box data according to the vector
	if ( ipAttributeNames != NULL )
	{
		m_radioSelectAttributes.SetCheck(TRUE);

		int nNumOfAttributeNames = ipAttributeNames->Size;
		for ( int i = 0; i < nNumOfAttributeNames; i++ )
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
void CRedactFileProcessorPP::updateVOAGroup()
{
	if ( m_chkUseVOA.GetCheck() == 1 )
	{
		m_editVOAFileName.EnableWindow(TRUE);
		m_btnSelectVOAFileTag.EnableWindow(TRUE);
		m_btnBrowseVOA.EnableWindow(TRUE);
	}
	else
	{
		m_editVOAFileName.EnableWindow(FALSE);
		m_btnSelectVOAFileTag.EnableWindow(FALSE);
		m_btnBrowseVOA.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void CRedactFileProcessorPP::validateLicense()
{
	VALIDATE_LICENSE(gnIDSHIELD_AUTOREDACTION_OBJECT, "ELI11531", "Legacy Redaction File Processor PP");
}
//-------------------------------------------------------------------------------------------------