// SPMFinderPP.cpp : Implementation of CSPMFinderPP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "SPMFinderPP.h"
#include "..\..\AFCore\Code\AFCategories.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <AFTagManager.h>
#include <DocTagUtils.h>

#include <string>

using namespace std;

const int NUM_OF_CHARS = 4096;

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CSPMFinderPP
//-------------------------------------------------------------------------------------------------
CSPMFinderPP::CSPMFinderPP()
: m_bIsPatternsFromFile(false),
  m_bStoreRuleWorked(false),
  m_ipDataScorer(NULL)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLESPMFinderPP;
		m_dwHelpFileID = IDS_HELPFILESPMFinderPP;
		m_dwDocStringID = IDS_DOCSTRINGSPMFinderPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07711")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinderPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CSPMFinderPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the SPM finder
			UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder = m_ppUnk[i];

			// update the IsPatternsFromFile attribute
			ipSPMFinder->IsPatternsFromFile = m_bIsPatternsFromFile ? VARIANT_TRUE : VARIANT_FALSE;
			
			// depending upon whether the patterns are loaded from
			// files or directly from typed text, update the SPM
			if (m_bIsPatternsFromFile)
			{
				if (!storeRulesFile(ipSPMFinder))
				{
					return S_FALSE;
				}
				ipSPMFinder->IgnoreInvalidTags = m_chkIgnoreMissingTags.GetCheck() == 1 ? VARIANT_TRUE : VARIANT_FALSE;
				
			}
			else
			{
				if (!storeRulesText(ipSPMFinder))
				{
					return S_FALSE;
				}
				ipSPMFinder->IgnoreInvalidTags = m_chkIgnoreMissingTags.GetCheck() == 1 ? VARIANT_TRUE : VARIANT_FALSE;
			}

			// store information related to capturing the ruleID(s) of the 
			// successful rule(s)
			storeRuleWorked(ipSPMFinder);

			storePreprocessors(ipSPMFinder);

			// store information related to data scoring
			storeDataScorerInfo(ipSPMFinder);

			// store misc other boolean flags
			bool bCaseSensitive = IsDlgButtonChecked(IDC_CHK_CASE_SPM)==TRUE;
			ipSPMFinder->CaseSensitive = bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
			bool bMultipleAsOne = IsDlgButtonChecked(IDC_CHK_MULTIPLE_WS)==TRUE;
			ipSPMFinder->TreatMultipleWSAsOne = bMultipleAsOne ? VARIANT_TRUE : VARIANT_FALSE;
			bool bGreedy = IsDlgButtonChecked(IDC_CHK_GREEDY)==TRUE;
			ipSPMFinder->GreedySearch = bGreedy ? VARIANT_TRUE : VARIANT_FALSE;

			// store the return match type
			if (IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_MATCH) == TRUE)
			{
				ipSPMFinder->ReturnMatchType = (UCLID_AFVALUEFINDERSLib::ESPMReturnMatchType)
					kReturnFirstMatch;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_RETURN_BEST_MATCH) == TRUE)
			{
				ipSPMFinder->ReturnMatchType = (UCLID_AFVALUEFINDERSLib::ESPMReturnMatchType)
					kReturnBestMatch;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_RETURN_ALL_MATCHES) == TRUE)
			{
				ipSPMFinder->ReturnMatchType = (UCLID_AFVALUEFINDERSLib::ESPMReturnMatchType)
					kReturnAllMatches;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_OR_BEST) == TRUE)
			{
				ipSPMFinder->ReturnMatchType = (UCLID_AFVALUEFINDERSLib::ESPMReturnMatchType)
					kReturnFirstOrBest;
			}
			else
			{
				// we should never reach here if we're handling all
				// the types of return matches
				THROW_LOGIC_ERROR_EXCEPTION("ELI08637")
			}
		}
		m_bDirty = FALSE;

		// if we reached here, then the data was successfully transfered
		// from the UI to the object.
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07018")

	// if we reached here, it's because of an exception
	// An error message has already been displayed to the user.
	// Return S_FALSE to indicate to the outer scope that the
	// Apply was not successful.
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinderPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		// set no delay.
		m_infoTip.SetShowDelay(0);
		
		// connect ATL control templates to the various window controls
		m_radioPatternText = GetDlgItem(IDC_RADIO_TEXT);
		m_editPatternText = GetDlgItem(IDC_EDIT_PATTERN_TEXT);
		m_editRuleFile = GetDlgItem(IDC_EDIT_RULE_FILE);
		m_editRuleWorkedName = GetDlgItem(IDC_EDIT_RULE_WORKED_NAME);
		m_chkStoreRuleWorked = GetDlgItem(IDC_CHK_STORE_RULE_WORKED);
		m_txtDefineRuleWorkedName = GetDlgItem(IDC_TEXT_DEFINE_NAME);
		m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_SPM);
		m_btnOpenNotepad = GetDlgItem(IDC_BTN_OPEN_NOTEPAD);
		m_btnOpenNotepad.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_NOTEPAD)));
		m_editMinMatchScore = GetDlgItem(IDC_EDIT_MIN_SCORE);
		m_editMinFirstMatchScore = GetDlgItem(IDC_EDIT_MIN_FIRST_SCORE);
		m_txtMinScoreLabel = GetDlgItem(IDC_STATIC_MIN_SCORE_LABEL);
		m_txtMinFirstScoreLabel = GetDlgItem(IDC_STATIC_MIN_FIRST_SCORE_LABEL);
		m_radioReturnFirstMatch = GetDlgItem(IDC_RADIO_RETURN_FIRST_MATCH);
		m_radioReturnBestMatch = GetDlgItem(IDC_RADIO_RETURN_BEST_MATCH);
		m_radioReturnAllMatches = GetDlgItem(IDC_RADIO_RETURN_ALL_MATCHES);
		m_radioReturnFirstOrBest = GetDlgItem(IDC_RADIO_RETURN_FIRST_OR_BEST);
		m_listPreprocessor = GetDlgItem(IDC_LIST_PREPROCESSORS);
		m_chkIgnoreMissingTags = GetDlgItem(IDC_CHK_IGNORE_NO_TAGS);
		m_btnSelectDocTag = GetDlgItem(IDC_BTN_SELECT_DOC_TAG);
		m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		
		m_listPreprocessor.SetExtendedListViewStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		CRect rect;
		m_listPreprocessor.GetClientRect(&rect);
		m_listPreprocessor.InsertColumn( 0, "Value", LVCFMT_LEFT, rect.Width(), 0 );
			
		UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder = m_ppUnk[0];
		if (ipSPMFinder)
		{
			// read patterns options
			m_bIsPatternsFromFile = ipSPMFinder->IsPatternsFromFile == VARIANT_TRUE;
			if (m_bIsPatternsFromFile)
			{
				string strFileName = ipSPMFinder->RulesFileName;
				m_editRuleFile.SetWindowText(strFileName.c_str());
				m_chkIgnoreMissingTags.SetCheck(ipSPMFinder->IgnoreInvalidTags == VARIANT_TRUE ? 1 : 0);
			}
			else
			{
				string strText = ipSPMFinder->RulesText;
				m_editPatternText.SetWindowText(strText.c_str());
				m_chkIgnoreMissingTags.SetCheck(ipSPMFinder->IgnoreInvalidTags == VARIANT_TRUE ? 1 : 0);
			}

			// read the state of whether the rule id of the successful
			// rule should be stored as a document tag
			m_bStoreRuleWorked = ipSPMFinder->StoreRuleWorked==VARIANT_TRUE;
			CheckDlgButton(IDC_CHK_STORE_RULE_WORKED, m_bStoreRuleWorked?BST_CHECKED:BST_UNCHECKED);
			if (m_bStoreRuleWorked)
			{
				string strRuleWorkedName = ipSPMFinder->RuleWorkedName;			
				// set text in the edit box
				SetDlgItemText(IDC_EDIT_RULE_WORKED_NAME, strRuleWorkedName.c_str());
			}

			BOOL bEnableReadPattern = FALSE;
			// enable/disable first radio button
			if (isRDTLicensed())
			{
				bEnableReadPattern = TRUE;
			}
			m_radioPatternText.EnableWindow(bEnableReadPattern);

			//updateControls();

			bool bCaseSensitive = ipSPMFinder->CaseSensitive==VARIANT_TRUE;
			CheckDlgButton(IDC_CHK_CASE_SPM, bCaseSensitive ? 
				BST_CHECKED : BST_UNCHECKED);

			bool bMultipleWSAsOne = ipSPMFinder->TreatMultipleWSAsOne == VARIANT_TRUE;
			CheckDlgButton(IDC_CHK_MULTIPLE_WS, bMultipleWSAsOne ? 
				BST_CHECKED : BST_UNCHECKED);
			
			bool bGreedy = ipSPMFinder->GreedySearch==VARIANT_TRUE;
			CheckDlgButton(IDC_CHK_GREEDY, bGreedy ? BST_CHECKED : BST_UNCHECKED);
			
			m_listPreprocessor.DeleteAllItems();
			// lst of values
			IVariantVectorPtr ipPreprocessors(ipSPMFinder->Preprocessors);
			if (ipPreprocessors)
			{
				CString zValue("");
				long nSize = ipPreprocessors->Size;
				for (long n=0; n<nSize; n++)
				{
					zValue = (char*)_bstr_t(ipPreprocessors->GetItem(n));
					m_listPreprocessor.InsertItem(n, zValue);
				}
				// set selection to the first item
				if (nSize > 0)
				{
					m_listPreprocessor.SetItemState(0, LVIS_SELECTED, LVIS_SELECTED);
				}
			}

			// determine if a data scorer has been specified and if so
			// update the various controls
			m_ipDataScorer = ipSPMFinder->DataScorer;
			ASSERT_RESOURCE_ALLOCATION("ELI08606", m_ipDataScorer != __nullptr);

			// update the data scorer description and the min-match-score
			// set the description of the data scorer object
			string strDesc = m_ipDataScorer->Description;
			SetDlgItemText(IDC_STATIC_DATA_SCORER, strDesc.c_str());

			// set the min-match score
			long nMinScore = ipSPMFinder->MinScoreToConsiderAsMatch;
			string strMinScore = asString(nMinScore);
			SetDlgItemText(IDC_EDIT_MIN_SCORE, strMinScore.c_str());

			// set the min-First match score
			long nMinFirstScore = ipSPMFinder->MinFirstToConsiderAsMatch;
			string strMinFirstScore = asString(nMinFirstScore);
			SetDlgItemText(IDC_EDIT_MIN_FIRST_SCORE, strMinFirstScore.c_str());


			// update the return-match-type radio button
			ESPMReturnMatchType eType = (ESPMReturnMatchType) ipSPMFinder->ReturnMatchType;
			switch (eType)
			{
			case kReturnFirstMatch:
				m_radioReturnFirstMatch.SetCheck(1);
				break;

			case kReturnBestMatch:
				m_radioReturnBestMatch.SetCheck(1);
				break;

			case kReturnAllMatches:
				m_radioReturnAllMatches.SetCheck(1);
				break;
			case kReturnFirstOrBest:
				m_radioReturnFirstOrBest.SetCheck(1);
				break;

			default:
				// we should never reach here if we're handling
				// all the types
				{
					UCLIDException ue("ELI08636", "Invalid return-match type!");
					ue.addDebugInfo("eType", (unsigned long) eType);
					throw ue;
				}
			};

			updateButtons();
			// update the state of all controls that depend on the value
			// of other controls
			updateControls();
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07019");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "DAT Files (*.dat)|*.dat"
											"|Encrypted Text Files (*.etf)|*.etf"
											"|All Files (*.*)|*.*||";
		string strFileExtension(s_strAllFiles);
		// check for license to enable selecting 
		// different types of file from file dialog
		if (!isRDTLicensed())
		{
			const static string s_strETFFile = "Encrypted Text Files (*.etf)|*.etf||";
			strFileExtension = s_strETFFile;
		}

		// bring open file dialog
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			strFileExtension.c_str(), NULL);
		
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			m_editRuleFile.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07033");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedBtnOpenNotepad(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CComBSTR bstrFile;
		// if the edit box has text
		GetDlgItemText(IDC_EDIT_RULE_FILE, bstrFile.m_str);
		string strFileName = asString(bstrFile);
		if (!strFileName.empty())
		{
			// get window system32 path
			char pszSystemDir[MAX_PATH];
			::GetSystemDirectory(pszSystemDir, MAX_PATH);
			
			string strCommand(pszSystemDir);
			strCommand += "\\Notepad.exe ";
			strCommand += strFileName;
			// run Notepad.exe with this file
			::runEXE(strCommand);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07037");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedRadioFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bIsPatternsFromFile = IsDlgButtonChecked(IDC_RADIO_FILE)==BST_CHECKED;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07035");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedRadioText(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bIsPatternsFromFile = IsDlgButtonChecked(IDC_RADIO_TEXT)==BST_UNCHECKED;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07036");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedPatternTextInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Syntax:\n"
					  "#import \"xxx.dat\"\n"
					  "[VARS_BEGIN]\n"
					  "VAR_NAME=variable definition\n"
					  "......\n"
					  "[VARS_END]\n"
					  "[PATTERNS_BEGIN]\n"
					  "PAT_ID_1=pattern text\n"
					  "......\n"
					  "[PATTERNS_END]\n\n"
					  "Note:\n"
					  "- Use #import statement to include a file. The syntax is similar to C++ syntax, where a file name must be enclosed in a pair of quotes. The file name must\n"
					  "  either be fully specified, or without specifying the full path. In the latter case, the file must exist in or related to the directory where this module is in.\n"
					  "- All variables must be declared inside VARS block, which has a start tag [VARS_BEGIN] and an end tag [VARS_END].\n"
					  "- Each variable name must be followed by an equal sign, and then immediately followed by the actual variable definition. No white space is allowed on left\n"
					  "  side of the equal sign. If one or more white space is presented on the right side of the equal sign, it will be considered as a part of the variable definition.\n"
					  "- Variable name can use any characters except equal sign (=) or any white space chars. It is recommended using alpha-characters in naming each variable.\n"
					  "- All patterns must be declared inside PATTERNS block, which starts with [PATTERNS_BEGIN] and ends with [PATTERNS_END].\n"
					  "- Each pattern declaration must have a unique pattern ID, followed by an equal sign, followed by pattern text. No white space is allowed on the left side\n"
					  "  of the equal sign. Any white presented to the right of the equal sign will be considered as a part of the pattern text.\n"
					  "- Pattern ID can be composed of any characters except equal sign (=) or any white space characters. There's no limit on number of characters to form a\n"
					  "  pattern ID as long as it's unique in the \"reading scope\". It is recommended to have a certain prefix for each document type. For instance, Assignment\n"
					  "  of Mortgage patterns can use \"AM_\" as each pattern ID's prefix.\n"
					  "- Patterns are read from top to bottom. Pattern ID will not affect this reading sequence.\n"
					  "- Use escape squences for special characters, ex. \\r\\n, \\t, etc.\n"
					  "- If duplicate entries are defined for variables or patterns, whichever comes last will be the one loaded into the rule definition set in the application.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07063");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedPatternFileInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Provide an existing file name or an unspecified file name,\n"
					  "using <DocType> as a place holder for the actual file name.\n"
					  "For instance, \"C:\\GrantorGranteeFinder\\<DocType>.dat.etf\"");   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07109");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedPreprocessorInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Use preprocessor to read in some pattern(s) but not some others.\n"
					  "For instance, in the patterns section, the following is defined:\n"
					  "......\n"
					  "#ifdef PICK_ORIGINAL_MORTGAGOR\n"
					  "SM_10=ABC^?OriginalMortgagor^DEF^?Mortgagee\n"
					  "#else\n"
					  "SM_11=ABC^DEF^?Mortgagee\n"
					  "#endif\n"
					  "......\n"
					  "If PICK_ORIGINAL_MORTGAGOR preprocessor is defined in SPM Finder,\n"
					  "pattern SM_10 will be read and SM_11 will be discarded. If it's not defined,\n"
					  "then SM_10 will be discarded and SM_11 will be read.\n\n"
					  "Note: Only #ifdef...#else...#endif and #ifdef...#endif are legitimate directives\n"
					  "to be used in the pattern text. All preprocessors are case-sensitive.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08768");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedRuleIDTagInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Provide a string to use as a label for an object tag.\n"
					  "For instance, \"SPM_Rule\"");   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15697");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedCheckStoreRuleWorked(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bStoreRuleWorked = IsDlgButtonChecked(IDC_CHK_STORE_RULE_WORKED)==TRUE;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07074");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedMinScoreInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("The 'Minimum match score' represents the minimum score that\n"
			"a candidate data must obtain in order to be considered as a successful\n"
			"match.  For the Return First or best options this value is used for the\n"
			"min best score.  Match scores are always in the range 0 to 100, with 100\n"
			"representing perfectly valid data.");   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08607");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedMinFirstScoreInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("The 'Return First match score' represents the minimum score that\n"
			"a candidate data must obtain in order to be considered as a successful\n"
			"first match when using the Return First or Best option. For all other \n"
			"options this value is ignored.  This value must be greater than the value\n"
			"of Minimum match score which is used for the best match.\n"
			"Match scores are always in the range 0 to 100, with 100\n"
			"representing perfectly valid data.");   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09034");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedButtonSelectDataScorer(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create a copy of the DataScorer object-with-description
		ICopyableObjectPtr ipCopyableObj = m_ipDataScorer;
		ASSERT_RESOURCE_ALLOCATION( "ELI08633", ipCopyableObj != __nullptr );
		
		IObjectWithDescriptionPtr ipDataScorerCopy = ipCopyableObj->Clone();
		ASSERT_RESOURCE_ALLOCATION( "ELI08634", ipDataScorerCopy != __nullptr );

		// Create the IObjectSelectorUI object
		IObjectSelectorUIPtr ipObjSelect( CLSID_ObjectSelectorUI );
		ASSERT_RESOURCE_ALLOCATION( "ELI08635", ipObjSelect != __nullptr );

		// initialize private license for the object
		IPrivateLicensedComponentPtr ipPLComponent = ipObjSelect;
		ASSERT_RESOURCE_ALLOCATION("ELI10316", ipPLComponent != __nullptr);
		_bstr_t _bstrKey = LICENSE_MGMT_PASSWORD.c_str();
		ipPLComponent->InitPrivateLicense(_bstrKey);

		// Prepare the prompts for object selector
		_bstr_t	bstrTitle("Data Scorer");
		_bstr_t	bstrDesc("Data Scorer description");
		_bstr_t	bstrSelect("Select Data Scorer");
		_bstr_t	bstrCategory( AFAPI_DATA_SCORERS_CATEGORYNAME.c_str() );

		// Show the UI
		VARIANT_BOOL vbResult = ipObjSelect->ShowUI1(bstrTitle, bstrDesc, 
			bstrSelect, bstrCategory, ipDataScorerCopy, VARIANT_TRUE);

		// If the user clicks the OK button
		if (vbResult == VARIANT_TRUE)
		{
			// Store the updated DataScorer
			m_ipDataScorer = ipDataScorerCopy;

			// Update the DataScorer description
			string strDesc = m_ipDataScorer->Description;
			SetDlgItemText(IDC_STATIC_DATA_SCORER, strDesc.c_str());

			// since the user may have specified or unspecified
			// the data scorer, update the state of dependent controls
			updateControls();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08632");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CString zEnt;
		bool bSuccess = promptForValue(zEnt, m_listPreprocessor);

		if (bSuccess)
		{
			int nTotal = m_listPreprocessor.GetItemCount();
			
			int nIndex = m_listPreprocessor.InsertItem(nTotal, zEnt);
			for (int i = 0; i <= nTotal; i++)
			{
				int nState = (i == nIndex) ? LVIS_SELECTED : 0;
				
				m_listPreprocessor.SetItemState(i, nState, LVIS_SELECTED);
			}
			
			CRect rect;
			
			m_listPreprocessor.GetClientRect(&rect);
			
			// adjust the column width in case there is a vertical scrollbar now
			m_listPreprocessor.SetColumnWidth(0, rect.Width());
			
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08763");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get currently selected item
		int nSelectedItemIndex = m_listPreprocessor.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex==LB_ERR)
		{
			return 0;
		}

		char pszValue[NUM_OF_CHARS];
		// get selected text
		m_listPreprocessor.GetItemText(nSelectedItemIndex, 0, pszValue, NUM_OF_CHARS);
		CString zEnt(pszValue);
		bool bSuccess = promptForValue(zEnt, m_listPreprocessor);
		if (bSuccess)
		{
			m_listPreprocessor.DeleteItem(nSelectedItemIndex);

			int nTotal = m_listPreprocessor.GetItemCount();
			
			int nIndex = m_listPreprocessor.InsertItem(nTotal, zEnt);
			
			for (int i = 0; i <= nTotal; i++)
			{
				int nState = (i == nIndex) ? LVIS_SELECTED : 0;
				
				m_listPreprocessor.SetItemState(i, nState, LVIS_SELECTED);
			}

			// Set Dirty flag
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08764");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get first selected item
		int nItem = m_listPreprocessor.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item?", "Confirm Delete", MB_YESNO);
			if (nRes == IDYES)
			{
				// remove selected items
				
				int nFirstItem = nItem;
				
				while(nItem != -1)
				{
					// remove from the UI listbox
					m_listPreprocessor.DeleteItem(nItem);
					
					nItem = m_listPreprocessor.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = m_listPreprocessor.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listPreprocessor.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					m_listPreprocessor.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
				
			}
		}
				
		CRect rect;
		
		m_listPreprocessor.GetClientRect(&rect);
		
		// adjust the column width in case there is a vertical scrollbar now
		m_listPreprocessor.SetColumnWidth(0, rect.Width());

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08765");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnDblclkListPreprocessor(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return OnClickedBtnModify(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	updateButtons();
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedRadioMatch(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_OR_BEST) == TRUE)
		{
			m_editMinFirstMatchScore.EnableWindow(TRUE);
		}
		else
		{
			m_editMinFirstMatchScore.EnableWindow(FALSE);
		}
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09096");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSPMFinderPP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ChooseDocTagForEditBox(IAFUtilityPtr(CLSID_AFUtility), m_btnSelectDocTag, m_editRuleFile);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12001");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CSPMFinderPP::isRDTLicensed()
{
	static const unsigned long COMP_RDT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	static bool bLicensed = false;
	if (!bLicensed)
	{
		bLicensed = LicenseManagement::isLicensed(COMP_RDT_ID);
	}

	return bLicensed;
}
//-------------------------------------------------------------------------------------------------
void CSPMFinderPP::storePreprocessors(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder)
{
	try
	{
		IVariantVectorPtr ipPreprocessors = ipSPMFinder->Preprocessors;
		if (ipPreprocessors)
		{
			ipPreprocessors->Clear();
			// get each item from the list
			int nSize = m_listPreprocessor.GetItemCount();			
			char pszValue[NUM_OF_CHARS];
			for (int n=0; n<nSize; n++)
			{
				m_listPreprocessor.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
				// store them in the vector
				ipPreprocessors->PushBack(_bstr_t(pszValue));
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08766");
}
//-------------------------------------------------------------------------------------------------
bool CSPMFinderPP::storeRulesFile(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder)
{
	try
	{
		CComBSTR bstrRulesFile;
		// if the edit box has text
		GetDlgItemText(IDC_EDIT_RULE_FILE, bstrRulesFile.m_str);
		_bstr_t _bstrFileName(bstrRulesFile);
		if (_bstrFileName.length() == 0)
		{
			throw UCLIDException("ELI07045", "File name shall not be empty.");
		}

		ipSPMFinder->RulesFileName = _bstr_t(bstrRulesFile);
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07044");
	
	m_editRuleFile.SetSel(0, -1);
	m_editRuleFile.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CSPMFinderPP::storeRulesText(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder)
{
	try
	{
		CComBSTR bstrRulesText;
		// if the edit box has text
		GetDlgItemText(IDC_EDIT_PATTERN_TEXT, bstrRulesText.m_str);
		ipSPMFinder->RulesText = _bstr_t(bstrRulesText);
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19336");
	
	m_editPatternText.SetSel(0, -1);
	m_editPatternText.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
void CSPMFinderPP::storeRuleWorked(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder)
{
	try
	{
		ipSPMFinder->StoreRuleWorked = m_bStoreRuleWorked ? VARIANT_TRUE : VARIANT_FALSE;
		if (m_bStoreRuleWorked)
		{
			CComBSTR bstrRuleWorkedName;
			// if the edit box has text
			GetDlgItemText(IDC_EDIT_RULE_WORKED_NAME, bstrRuleWorkedName.m_str);
			_bstr_t _bstrName(bstrRuleWorkedName);
			if (_bstrName.length() == 0)
			{
				::MessageBox(NULL, "Name for the rule that works is empty. It will be set to default value.", "Name", MB_OK);
				_bstrName = "RuleWorked";
			}
			
			ipSPMFinder->RuleWorkedName = _bstrName;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07046");
}
//-------------------------------------------------------------------------------------------------
void CSPMFinderPP::storeDataScorerInfo(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder)
{
	if (usingDataScorer())
	{
		// store the data scorer object in the SPM
		ipSPMFinder->DataScorer = m_ipDataScorer;

		// also store the min-match-score after validating it
		CComBSTR bstrMinMatchScore;
		GetDlgItemText(IDC_EDIT_MIN_SCORE, bstrMinMatchScore.m_str);
		string strMinMatchScore = asString(bstrMinMatchScore);
		long nMinMatchScore;
		try
		{
			// attempt converting the min score into a valid integer 
			// try updating the attribute in the SPM
			nMinMatchScore = asLong(strMinMatchScore);
			ipSPMFinder->MinScoreToConsiderAsMatch = nMinMatchScore;
		}
		catch (...)
		{
			// if an exception was caught while trying to 
			// update the min-match-score, set focus on it
			// and re-throw the exception
			m_editMinMatchScore.SetSel(0, -1);
			m_editMinMatchScore.SetFocus(); 
			throw;
		}
		// also store the min-First match-score after validating it
		CComBSTR bstrMinFirstMatchScore;
		GetDlgItemText(IDC_EDIT_MIN_FIRST_SCORE, bstrMinFirstMatchScore.m_str);
		string strMinFirstMatchScore = asString(bstrMinFirstMatchScore);
		try
		{
			// attempt converting the min first score into a valid integer 
			// try updating the attribute in the SPM
			long nMinFirstMatchScore = asLong(strMinFirstMatchScore);
			ipSPMFinder->MinFirstToConsiderAsMatch = nMinFirstMatchScore;
			// if the return first or best dialog button is selected make sure the 
			// Minimum first match score is > Minimum match score
			if ( (IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_OR_BEST) == TRUE) && 
					( nMinFirstMatchScore <= nMinMatchScore ) )
			{
				UCLIDException ue("ELI09095", "Return first >= Score is <= Minimum match score!");
				ue.addDebugInfo("First match Score", nMinFirstMatchScore);
				ue.addDebugInfo("Minimum match score", nMinMatchScore );
				throw ue;
			}
		}
		catch (...)
		{
			// if an exception was caught while trying to 
			// update the min-match-score, set focus on it
			// and re-throw the exception
			m_editMinFirstMatchScore.SetSel(0, -1);
			m_editMinFirstMatchScore.SetFocus();
			throw;
		}

	}
	else
	{
		// delete the data scorer object in the SPM
		ipSPMFinder->DataScorer = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
void CSPMFinderPP::updateButtons()
{
	int nSelCount = m_listPreprocessor.GetSelectedCount();
	int nCount = m_listPreprocessor.GetItemCount();
	
	if (nCount == 0)
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_SPM)).EnableWindow(FALSE);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_SPM)).EnableWindow(FALSE);
	}
	else
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_SPM)).EnableWindow(nSelCount == 1 ? true : false);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_SPM)).EnableWindow(nSelCount >= 1 ? true : false);
	}
}
//-------------------------------------------------------------------------------------------------
void CSPMFinderPP::updateControls()
{
	CheckDlgButton(IDC_RADIO_TEXT, m_bIsPatternsFromFile?BST_UNCHECKED:BST_CHECKED);
	m_editPatternText.EnableWindow(m_bIsPatternsFromFile ? FALSE : TRUE);
	CheckDlgButton(IDC_RADIO_FILE, m_bIsPatternsFromFile?BST_CHECKED:BST_UNCHECKED);
	m_editRuleFile.EnableWindow(m_bIsPatternsFromFile ? TRUE : FALSE);
	m_btnSelectDocTag.EnableWindow(m_bIsPatternsFromFile ? TRUE : FALSE);
	m_chkIgnoreMissingTags.EnableWindow(m_bIsPatternsFromFile ? TRUE : FALSE);
	m_btnBrowse.EnableWindow(m_bIsPatternsFromFile ? TRUE : FALSE);
	m_btnOpenNotepad.EnableWindow(m_bIsPatternsFromFile ? TRUE : FALSE);
	m_editRuleWorkedName.EnableWindow(m_bStoreRuleWorked ? TRUE : FALSE);
	m_editMinMatchScore.EnableWindow(usingDataScorer() ? TRUE : FALSE);
	m_txtMinScoreLabel.EnableWindow(usingDataScorer() ? TRUE : FALSE);
	
	// the return best match radio button should only be enabled
	// if a data scorer is available
	if (m_ipDataScorer != __nullptr && m_ipDataScorer->Object != __nullptr)
	{
		// enable the "return best matches" option
		m_radioReturnBestMatch.EnableWindow(TRUE);
		m_radioReturnFirstOrBest.EnableWindow(TRUE);
		if (IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_OR_BEST) == TRUE)
		{
			m_editMinFirstMatchScore.EnableWindow(TRUE);
		}
		else
		{
			m_editMinFirstMatchScore.EnableWindow(FALSE);
		}
		m_txtMinFirstScoreLabel.EnableWindow(TRUE);
	}
	else
	{
		// if the "return best matches" is the current selection,
		// then reset the selection to "Return first match"
		if (IsDlgButtonChecked(IDC_RADIO_RETURN_BEST_MATCH) == TRUE ||
			(IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_OR_BEST) == TRUE))
		{
			m_radioReturnFirstMatch.SetCheck(1);
		}
		
		m_radioReturnFirstOrBest.SetCheck(0);
		m_radioReturnBestMatch.SetCheck(0);

		// disable the "return best matches" option
		m_radioReturnBestMatch.EnableWindow(FALSE);
		m_radioReturnFirstOrBest.EnableWindow(FALSE);
		m_editMinFirstMatchScore.EnableWindow(FALSE);
		m_txtMinFirstScoreLabel.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
bool CSPMFinderPP::usingDataScorer()
{
	return (m_ipDataScorer == __nullptr || m_ipDataScorer->Object == NULL) ? false : true;
}
//-------------------------------------------------------------------------------------------------
void CSPMFinderPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07682", "SPM Finder PP" );
}
//-------------------------------------------------------------------------------------------------
