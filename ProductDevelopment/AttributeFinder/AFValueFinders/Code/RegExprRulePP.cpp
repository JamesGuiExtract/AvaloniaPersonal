// RegExprRulePP.cpp : Implementation of CRegExprRulePP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "RegExprRulePP.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <AFTagManager.h>

//-------------------------------------------------------------------------------------------------
// CRegExprRulePP
//-------------------------------------------------------------------------------------------------
CRegExprRulePP::CRegExprRulePP() 
: m_bIsRegExpFromFile(false)
{
	try
	{
		// Check licensing
		validateLicense();

		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13043", ipMiscUtils != NULL );

		m_dwTitleID = IDS_TITLERegExprRulePP;
		m_dwHelpFileID = IDS_HELPFILERegExprRulePP;
		m_dwDocStringID = IDS_DOCSTRINGRegExprRulePP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI19344")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRulePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CRegExprRulePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEFINDERSLib::IRegExprRulePtr ipRegExprRule = m_ppUnk[i];
			ipRegExprRule->IsRegExpFromFile = m_bIsRegExpFromFile ? VARIANT_TRUE : VARIANT_FALSE;
			if ( m_bIsRegExpFromFile ) 
			{
				if ( !storePatternFile(ipRegExprRule))
				{
					return S_FALSE;
				}
			}
			else
			{
				// get the pattern string, and verify it is not empty
				if (!storePattern(ipRegExprRule))
				{
					return S_FALSE;
				}
			}		
			// check whether the case check box is checked
			ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_REG_EXP_CASE));
			int nChecked = checkBox.GetCheck();			
			ipRegExprRule->IsCaseSensitive = (nChecked == 1) ? VARIANT_TRUE : VARIANT_FALSE;

			// Check if sub attribute check box is checked
			nChecked = m_checkCreateSubAttributesFromMatches.GetCheck();
			ipRegExprRule->CreateSubAttributesFromNamedMatches = asVariantBool(nChecked == BST_CHECKED);
		}
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19303")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRulePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CRegExprRulePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_nObjects > 0)
		{
			UCLID_AFVALUEFINDERSLib::IRegExprRulePtr ipRegExprRule = m_ppUnk[0];
	
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			m_radioPatternText = GetDlgItem(IDC_RADIO_TEXT);
			m_editPatternText = GetDlgItem(IDC_EDIT_PATTERN);
			m_editRegExpFile = GetDlgItem(IDC_EDIT_REG_EXP_FILE);
			m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_REG_EXP);
			m_btnOpenNotepad = GetDlgItem(IDC_BTN_OPEN_NOTEPAD);
			m_btnOpenNotepad.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_NOTEPAD)));
			m_btnSelectDocTag = GetDlgItem(IDC_BTN_SELECT_DOC_TAG);
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			m_bIsRegExpFromFile = ipRegExprRule->IsRegExpFromFile == VARIANT_TRUE;
			if ( m_bIsRegExpFromFile )
			{
				string strFileName = ipRegExprRule->RegExpFileName;
				m_editRegExpFile.SetWindowText( strFileName.c_str());
			}
			else
			{
				string strPattern = ipRegExprRule->Pattern;
				m_editPatternText.SetWindowText( strPattern.c_str() );
			}

			// Setup the case check box value
			bool bCaseSensitive = ipRegExprRule->GetIsCaseSensitive() == VARIANT_TRUE;
			ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_REG_EXP_CASE));
			checkBox.SetCheck(bCaseSensitive ? 1 : 0);

			// Setup the Subattribute check box value
			m_checkCreateSubAttributesFromMatches = GetDlgItem(IDC_CHK_NAMED_MATCHES_AS_SUBATTRIBUTES);
			bool bCapturesAsSubAttributes = asCppBool(ipRegExprRule->CreateSubAttributesFromNamedMatches);
			m_checkCreateSubAttributesFromMatches.SetCheck(asBSTChecked( bCapturesAsSubAttributes ));
			
			updateControls();
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19302");

	return 1;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnChangeEditPattern(WORD wNotifyCode, WORD wID, 
											HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	SetDirty(TRUE);

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnClickedChkCaseRegExpr(WORD wNotifyCode, WORD wID, 
												HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	SetDirty(TRUE);

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "DAT Files (*.dat)|*.dat"
											"|Encrypted Text Files (*.etf)|*.etf"
											"|All Files (*.*)|*.*||";

		string strFileExtension(s_strAllFiles);
		// bring open file dialog
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			strFileExtension.c_str(), NULL);
		
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			m_editRegExpFile.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07527");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnClickedBtnOpenNotepad(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CComBSTR bstrFile;
		// if the edit box has text
		GetDlgItemText(IDC_EDIT_REG_EXP_FILE, bstrFile.m_str);
		string strFileName = asString(bstrFile);
		if (!strFileName.empty())
		{
			// get window system32 path
			char pszSystemDir[MAX_PATH];
			::GetSystemDirectory(pszSystemDir, MAX_PATH);
			
			string strCommand(pszSystemDir);
			strCommand += "\\Notepad.exe";
			
			// run Notepad.exe with this file
			runEXE( strCommand, strFileName );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07528");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnClickedRadioFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bIsRegExpFromFile = IsDlgButtonChecked(IDC_RADIO_FILE)==BST_CHECKED;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07530");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnClickedRadioText(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bIsRegExpFromFile = IsDlgButtonChecked(IDC_RADIO_TEXT)==BST_UNCHECKED;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07529");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnClickedRegExpFileInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// show tooltip info
		CString zText("Provide an existing file name or an unspecified file name,\n"
					  "using <DocType> as a place holder for the actual file name.\n"
					  "For instance, \"C:\\GrantorGranteeFinder\\<DocType>.dat.etf\"\n\n"
					  "The file directory can be specified with the <RSDFileDir> tag \n"
					  "to indicate the directory with the current RSD file");   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07536");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		RECT rect;
		m_btnSelectDocTag.GetWindowRect(&rect);
		CRect rc(rect);
		
		AFTagManager tagMgr;
		string strChoice = tagMgr.displayTagsForSelection(CWnd::FromHandle(m_hWnd), rc.right, rc.top);
		if (strChoice != "")
		{
			m_editRegExpFile.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12008");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRegExprRulePP::OnClickedNamedMatchesAsSubAttributes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23029");
	
	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CRegExprRulePP::storePattern(UCLID_AFVALUEFINDERSLib::IRegExprRulePtr ipRegExprRule)
{
	try
	{
		// get the pattern string, and verify it is not empty
		CComBSTR bstrPattern;
		GetDlgItemText(IDC_EDIT_PATTERN, bstrPattern.m_str);

		ipRegExprRule->Pattern = _bstr_t(bstrPattern);

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05837");

	ATLControls::CEdit editBox(GetDlgItem(IDC_EDIT_PATTERN));
	editBox.SetSel(0, -1);
	editBox.SetFocus();

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CRegExprRulePP::storePatternFile(UCLID_AFVALUEFINDERSLib::IRegExprRulePtr ipRegExprRule)
{
	try
	{
		CComBSTR bstrRegExpFile;
		// if the edit box has text
		GetDlgItemText(IDC_EDIT_REG_EXP_FILE, bstrRegExpFile.m_str);
		_bstr_t _bstrFileName(bstrRegExpFile);
		if (_bstrFileName.length() == 0)
		{
			throw UCLIDException("ELI07523", "File name shall not be empty.");
		}

		ipRegExprRule->RegExpFileName = _bstr_t(bstrRegExpFile);
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07524");
	
	m_editRegExpFile.SetSel(0, -1);
	m_editRegExpFile.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
void CRegExprRulePP::updateControls()
{
	CheckDlgButton(IDC_RADIO_TEXT, m_bIsRegExpFromFile?BST_UNCHECKED:BST_CHECKED);
	m_editPatternText.EnableWindow(m_bIsRegExpFromFile ? FALSE : TRUE);
	CheckDlgButton(IDC_RADIO_FILE, m_bIsRegExpFromFile?BST_CHECKED:BST_UNCHECKED);
	m_editRegExpFile.EnableWindow(m_bIsRegExpFromFile ? TRUE : FALSE);
	m_btnSelectDocTag.EnableWindow(m_bIsRegExpFromFile ? TRUE : FALSE);
	m_btnBrowse.EnableWindow(m_bIsRegExpFromFile ? TRUE : FALSE);
	m_btnOpenNotepad.EnableWindow(m_bIsRegExpFromFile ? TRUE : FALSE);
}
//-------------------------------------------------------------------------------------------------
void CRegExprRulePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07681", 
		"Regular Expression Finder PP" );
}
//-------------------------------------------------------------------------------------------------
