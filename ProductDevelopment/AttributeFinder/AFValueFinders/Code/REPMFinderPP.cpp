// REPMFinderPP.cpp : Implementation of CREPMFinderPP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "REPMFinderPP.h"
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
// CREPMFinderPP
//-------------------------------------------------------------------------------------------------
CREPMFinderPP::CREPMFinderPP()
: m_bStoreRuleWorked(false),
  m_ipDataScorer(NULL)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEREPMFinderPP;
		m_dwHelpFileID = IDS_HELPFILEREPMFinderPP;
		m_dwDocStringID = IDS_DOCSTRINGREPMFinderPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI33293")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinderPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CREPMFinderPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the REPM finder
			UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipREPMFinder = m_ppUnk[i];
			
			if (!storeRulesFile(ipREPMFinder))
			{
				return S_FALSE;
			}
			ipREPMFinder->IgnoreInvalidTags = m_chkIgnoreMissingTags.GetCheck() == 1 ? VARIANT_TRUE : VARIANT_FALSE;

			// store information related to capturing the ruleID(s) of the 
			// successful rule(s)
			storeRuleWorked(ipREPMFinder);

			// store information related to data scoring
			storeDataScorerInfo(ipREPMFinder);

			// store misc other boolean flags
			bool bCaseSensitive = IsDlgButtonChecked(IDC_CHK_CASE_REPM)==TRUE;
			ipREPMFinder->CaseSensitive = bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;

			// store the return match type
			if (IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_MATCH) == TRUE)
			{
				ipREPMFinder->ReturnMatchType = (UCLID_AFVALUEFINDERSLib::EPMReturnMatchType)
					kReturnFirstMatch;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_RETURN_BEST_MATCH) == TRUE)
			{
				ipREPMFinder->ReturnMatchType = (UCLID_AFVALUEFINDERSLib::EPMReturnMatchType)
					kReturnBestMatch;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_RETURN_ALL_MATCHES) == TRUE)
			{
				ipREPMFinder->ReturnMatchType = (UCLID_AFVALUEFINDERSLib::EPMReturnMatchType)
					kReturnAllMatches;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_OR_BEST) == TRUE)
			{
				ipREPMFinder->ReturnMatchType = (UCLID_AFVALUEFINDERSLib::EPMReturnMatchType)
					kReturnFirstOrBest;
			}
			else
			{
				// we should never reach here if we're handling all
				// the types of return matches
				THROW_LOGIC_ERROR_EXCEPTION("ELI33294")
			}
		}
		m_bDirty = FALSE;

		// if we reached here, then the data was successfully transfered
		// from the UI to the object.
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33295")

	// if we reached here, it's because of an exception
	// An error message has already been displayed to the user.
	// Return S_FALSE to indicate to the outer scope that the
	// Apply was not successful.
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinderPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CREPMFinderPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		// set no delay.
		m_infoTip.SetShowDelay(0);
		
		// connect ATL control templates to the various window controls
		m_editRuleFile = GetDlgItem(IDC_EDIT_RULE_FILE);
		m_editRuleWorkedName = GetDlgItem(IDC_EDIT_RULE_WORKED_NAME);
		m_chkStoreRuleWorked = GetDlgItem(IDC_CHK_STORE_RULE_WORKED);
		m_txtDefineRuleWorkedName = GetDlgItem(IDC_TEXT_DEFINE_NAME);
		m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_REPM);
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
		m_chkIgnoreMissingTags = GetDlgItem(IDC_CHK_IGNORE_NO_TAGS);
		m_btnSelectDocTag = GetDlgItem(IDC_BTN_SELECT_DOC_TAG);
		m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		
		UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipREPMFinder = m_ppUnk[0];
		if (ipREPMFinder)
		{
			// read patterns options
			string strFileName = ipREPMFinder->RulesFileName;
			m_editRuleFile.SetWindowText(strFileName.c_str());
			m_chkIgnoreMissingTags.SetCheck(ipREPMFinder->IgnoreInvalidTags == VARIANT_TRUE ? 1 : 0);
			
			// read the state of whether the rule id of the successful
			// rule should be stored as a document tag
			m_bStoreRuleWorked = ipREPMFinder->StoreRuleWorked==VARIANT_TRUE;
			CheckDlgButton(IDC_CHK_STORE_RULE_WORKED, m_bStoreRuleWorked?BST_CHECKED:BST_UNCHECKED);
			if (m_bStoreRuleWorked)
			{
				string strRuleWorkedName = ipREPMFinder->RuleWorkedName;			
				// set text in the edit box
				SetDlgItemText(IDC_EDIT_RULE_WORKED_NAME, strRuleWorkedName.c_str());
			}

			bool bCaseSensitive = ipREPMFinder->CaseSensitive==VARIANT_TRUE;
			CheckDlgButton(IDC_CHK_CASE_REPM, bCaseSensitive ? 
				BST_CHECKED : BST_UNCHECKED);

			// determine if a data scorer has been specified and if so
			// update the various controls
			m_ipDataScorer = ipREPMFinder->DataScorer;
			ASSERT_RESOURCE_ALLOCATION("ELI33296", m_ipDataScorer != __nullptr);

			// update the data scorer description and the min-match-score
			// set the description of the data scorer object
			string strDesc = m_ipDataScorer->Description;
			SetDlgItemText(IDC_STATIC_DATA_SCORER, strDesc.c_str());

			// set the min-match score
			long nMinScore = ipREPMFinder->MinScoreToConsiderAsMatch;
			string strMinScore = asString(nMinScore);
			SetDlgItemText(IDC_EDIT_MIN_SCORE, strMinScore.c_str());

			// set the min-First match score
			long nMinFirstScore = ipREPMFinder->MinFirstToConsiderAsMatch;
			string strMinFirstScore = asString(nMinFirstScore);
			SetDlgItemText(IDC_EDIT_MIN_FIRST_SCORE, strMinFirstScore.c_str());


			// update the return-match-type radio button
			EPMReturnMatchType eType = (EPMReturnMatchType) ipREPMFinder->ReturnMatchType;
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
					UCLIDException ue("ELI33297", "Invalid return-match type!");
					ue.addDebugInfo("eType", (unsigned long) eType);
					throw ue;
				}
			};

			// update the state of all controls that depend on the value
			// of other controls
			updateControls();
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33298");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33299");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedBtnOpenNotepad(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33300");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedPatternFileInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Provide an existing file name or an unspecified file name,\n"
					  "using <DocType> as a place holder for the actual file name.\n"
					  "For instance, \"C:\\GrantorGranteeFinder\\<DocType>.dat.etf\"\n\n"
					  "File Syntax:\n"
					  "#import \"xxx.dat\"\n"
					  "<patternid>===[regular expression]\n"
					  "<patternid>===[regular expression]\n"
					  "...\n"
					  "Note:\n"
					  "- Use #import statement to include a file. The syntax is similar to C++ syntax, where a file name must be enclosed in a pair of quotes. The file name must\n"
					  "  either be fully specified, or without specifying the full path. In the latter case, the file must exist in or related to the directory where this module is in.\n"
					  "- Imported files are evaluated before evaluating patterns so imports may specify a regesx to be inserted into the current pattern.\n"
					  "- Each pattern declaration must have a unique pattern ID, followed by an equal sign, followed by pattern text. No white space is allowed on the left side\n"
					  "  of the equal sign. Any white presented to the right of the equal sign will be considered as a part of the pattern text.\n"
					  "- Pattern ID can be composed of any characters except equal sign (=) or any white space characters. There's no limit on number of characters to form a\n"
					  "  pattern ID as long as it's unique.\n"
					  "- Duplicate pattern IDs will result in an exception.\n"
					  "- Patterns are read from top to bottom. Pattern ID will not affect this reading sequence.\n"
					  "- If the pattern matches, each named group from the regex will generate an attribute with a type matching the name of the group. (The overall match result\n"
					  "  will be ignored.)\n"
					  "- Named group results whose name begins with an underscore will be ignored.");   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33304");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedRuleIDTagInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Provide a string to use as a label for an object tag.\n"
					  "For instance, \"REPM_Rule\"");   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33306");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedCheckStoreRuleWorked(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bStoreRuleWorked = IsDlgButtonChecked(IDC_CHK_STORE_RULE_WORKED)==TRUE;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33307");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedMinScoreInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33308");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedMinFirstScoreInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33309");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedButtonSelectDataScorer(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create a copy of the DataScorer object-with-description
		ICopyableObjectPtr ipCopyableObj = m_ipDataScorer;
		ASSERT_RESOURCE_ALLOCATION( "ELI33310", ipCopyableObj != __nullptr );
		
		IObjectWithDescriptionPtr ipDataScorerCopy = ipCopyableObj->Clone();
		ASSERT_RESOURCE_ALLOCATION( "ELI33311", ipDataScorerCopy != __nullptr );

		// Create the IObjectSelectorUI object
		IObjectSelectorUIPtr ipObjSelect( CLSID_ObjectSelectorUI );
		ASSERT_RESOURCE_ALLOCATION( "ELI33312", ipObjSelect != __nullptr );

		// initialize private license for the object
		IPrivateLicensedComponentPtr ipPLComponent = ipObjSelect;
		ASSERT_RESOURCE_ALLOCATION("ELI33313", ipPLComponent != __nullptr);
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33314");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedRadioMatch(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33318");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CREPMFinderPP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ChooseDocTagForEditBox(IAFUtilityPtr(CLSID_AFUtility), m_btnSelectDocTag, m_editRuleFile);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33319");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CREPMFinderPP::isRDTLicensed()
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
bool CREPMFinderPP::storeRulesFile(UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipREPMFinder)
{
	try
	{
		CComBSTR bstrRulesFile;
		// if the edit box has text
		GetDlgItemText(IDC_EDIT_RULE_FILE, bstrRulesFile.m_str);
		_bstr_t _bstrFileName(bstrRulesFile);
		if (_bstrFileName.length() == 0)
		{
			throw UCLIDException("ELI33321", "File name shall not be empty.");
		}

		ipREPMFinder->RulesFileName = _bstr_t(bstrRulesFile);
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33322");
	
	m_editRuleFile.SetSel(0, -1);
	m_editRuleFile.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
void CREPMFinderPP::storeRuleWorked(UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipREPMFinder)
{
	try
	{
		ipREPMFinder->StoreRuleWorked = m_bStoreRuleWorked ? VARIANT_TRUE : VARIANT_FALSE;
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
			
			ipREPMFinder->RuleWorkedName = _bstrName;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33324");
}
//-------------------------------------------------------------------------------------------------
void CREPMFinderPP::storeDataScorerInfo(UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipREPMFinder)
{
	if (usingDataScorer())
	{
		// store the data scorer object in the REPM
		ipREPMFinder->DataScorer = m_ipDataScorer;

		// also store the min-match-score after validating it
		CComBSTR bstrMinMatchScore;
		GetDlgItemText(IDC_EDIT_MIN_SCORE, bstrMinMatchScore.m_str);
		string strMinMatchScore = asString(bstrMinMatchScore);
		long nMinMatchScore;
		try
		{
			// attempt converting the min score into a valid integer 
			// try updating the attribute in the REPM
			nMinMatchScore = asLong(strMinMatchScore);
			ipREPMFinder->MinScoreToConsiderAsMatch = nMinMatchScore;
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
			// try updating the attribute in the REPM
			long nMinFirstMatchScore = asLong(strMinFirstMatchScore);
			ipREPMFinder->MinFirstToConsiderAsMatch = nMinFirstMatchScore;
			// if the return first or best dialog button is selected make sure the 
			// Minimum first match score is > Minimum match score
			if ( (IsDlgButtonChecked(IDC_RADIO_RETURN_FIRST_OR_BEST) == TRUE) && 
					( nMinFirstMatchScore <= nMinMatchScore ) )
			{
				UCLIDException ue("ELI33325", "Return first >= Score is <= Minimum match score!");
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
		// delete the data scorer object in the REPM
		ipREPMFinder->DataScorer = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
void CREPMFinderPP::updateControls()
{
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
bool CREPMFinderPP::usingDataScorer()
{
	return (m_ipDataScorer == __nullptr || m_ipDataScorer->Object == NULL) ? false : true;
}
//-------------------------------------------------------------------------------------------------
void CREPMFinderPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI33326", "REPM Finder PP" );
}
//-------------------------------------------------------------------------------------------------
