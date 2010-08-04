// AdvancedReplaceStringPP.cpp : Implementation of CAdvancedReplaceStringPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "AdvancedReplaceStringPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <AFTagManager.h>

//-------------------------------------------------------------------------------------------------
// CAdvancedReplaceStringPP
//-------------------------------------------------------------------------------------------------
CAdvancedReplaceStringPP::CAdvancedReplaceStringPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEAdvancedReplaceStringPP;
		m_dwHelpFileID = IDS_HELPFILEAdvancedReplaceStringPP;
		m_dwDocStringID = IDS_DOCSTRINGAdvancedReplaceStringPP;

		// Create an IMiscUtilsPtr object
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI19434", ipMiscUtils != NULL );

		// Get the file header string and its length from IMiscUtilsPtr object
		m_strFileHeader = ipMiscUtils->GetFileHeader();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07712")
}
//-------------------------------------------------------------------------------------------------
CAdvancedReplaceStringPP::~CAdvancedReplaceStringPP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16354");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceStringPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CAdvancedReplaceStringPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::IAdvancedReplaceStringPtr ipAdvancedRS = m_ppUnk[i];
			// get string to be replaced
			if (!storeToBeReplaced(ipAdvancedRS))
			{
				return S_FALSE;
			}
			
			// get replacement
			CComBSTR bstrReplacement;
			GetDlgItemText(IDC_EDIT_REPLACEMENT, bstrReplacement.m_str);
			ipAdvancedRS->Replacement = bstrReplacement.Detach();
			
			// occurrence type
			EReplacementOccurrenceType eOccurrenceType = kAllOccurrences;
			long nSpecifiedOccurrence = 0;
			if (IsDlgButtonChecked(IDC_RADIO_FIRST))
			{
				eOccurrenceType = kFirstOccurrence;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_LAST))
			{
				eOccurrenceType = kLastOccurrence;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_SPECIFIED))
			{
				eOccurrenceType = kSpecifiedOccurrence;
			}
			ipAdvancedRS->ReplacementOccurrenceType 
				= (UCLID_AFVALUEMODIFIERSLib::EReplacementOccurrenceType)eOccurrenceType;
			if (eOccurrenceType == kSpecifiedOccurrence)
			{
				if (!storeOccurrence(ipAdvancedRS))
				{
					return S_FALSE;
				}
			}
			
			// case sensitivity
			ipAdvancedRS->IsCaseSensitive 
				= IsDlgButtonChecked(IDC_CHK_CASE_ARS)==BST_CHECKED?VARIANT_TRUE:VARIANT_FALSE;
			// regular expression?
			ipAdvancedRS->AsRegularExpression 
				= IsDlgButtonChecked(IDC_CHK_AS_REG_EXPR_ARS)==BST_CHECKED?VARIANT_TRUE:VARIANT_FALSE;
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05004");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceStringPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Windows Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEMODIFIERSLib::IAdvancedReplaceStringPtr ipAdvancedRS(m_ppUnk[0]);
		if (ipAdvancedRS)
		{
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			string strToBeReplaced = ipAdvancedRS->StrToBeReplaced;
			string strReplacement = ipAdvancedRS->Replacement;

			// Set the strings on two edit boxes
			m_editFindString = GetDlgItem(IDC_EDIT_TO_BE_REPLACED);
			m_editReplaceString = GetDlgItem(IDC_EDIT_REPLACEMENT);
			m_editFindString.SetWindowText(strToBeReplaced.c_str());
			m_editReplaceString.SetWindowText(strReplacement.c_str());

			// Set ICON for two tag buttons
			m_btnSelectFindDocTag = GetDlgItem(IDC_BTN_SELECT_FIND_DOC_TAG);
			m_btnSelectFindDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnSelectReplaceDocTag = GetDlgItem(IDC_BTN_SELECT_REPLACE_DOC_TAG);
			m_btnSelectReplaceDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			EReplacementOccurrenceType eOccurrenceType = (EReplacementOccurrenceType)ipAdvancedRS->ReplacementOccurrenceType;
			switch (eOccurrenceType)
			{
			case kAllOccurrences:
				CheckDlgButton(IDC_RADIO_ALL, BST_CHECKED);
				break;
			case kFirstOccurrence:
				CheckDlgButton(IDC_RADIO_FIRST, BST_CHECKED);
				break;
			case kLastOccurrence:
				CheckDlgButton(IDC_RADIO_LAST, BST_CHECKED);
				break;
			case kSpecifiedOccurrence:
				{
					CheckDlgButton(IDC_RADIO_SPECIFIED, BST_CHECKED);
					SetDlgItemInt(IDC_EDIT_SPECIFIED_OCC, ipAdvancedRS->SpecifiedOccurrence, FALSE);
				}
				break;
			}

			// update the state of the specified occurrence edit box
			updateSpecifiedOccEdit();

			// case sensitivity
			if (ipAdvancedRS->IsCaseSensitive)
			{
				CheckDlgButton(IDC_CHK_CASE_ARS, BST_CHECKED);
			}

			if (ipAdvancedRS->AsRegularExpression)
			{
				CheckDlgButton(IDC_CHK_AS_REG_EXPR_ARS, BST_CHECKED);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05005");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedRadioAll(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// update the state of the specified occurrence edit box
		updateSpecifiedOccEdit();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05006");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedRadioFirst(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// update the state of the specified occurrence edit box
		updateSpecifiedOccEdit();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05007");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedRadioLast(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// update the state of the specified occurrence edit box
		updateSpecifiedOccEdit();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05008");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedRadioSpecified(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// update the state of the specified occurrence edit box
		updateSpecifiedOccEdit();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05009");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedSelectFindDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		RECT rect;
		m_btnSelectFindDocTag.GetWindowRect(&rect);
		CRect rc(rect);
		
		AFTagManager tagMgr;
		string strChoice = tagMgr.displayTagsForSelection(CWnd::FromHandle(m_hWnd), rc.right, rc.top);
		if (strChoice != "")
		{
			m_editFindString.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14511");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedSelectReplaceDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		RECT rect;
		m_btnSelectFindDocTag.GetWindowRect(&rect);
		CRect rc(rect);
		
		AFTagManager tagMgr;
		string strChoice = tagMgr.displayTagsForSelection(CWnd::FromHandle(m_hWnd), rc.right, rc.top);
		if (strChoice != "")
		{
			m_editReplaceString.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14512");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedBrowseFindFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "Text Files (*.txt)|*.txt"
											"|DAT Files (*.dat)|*.dat"
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
			CString zFullFileName = CString(m_strFileHeader.c_str()) + fileDlg.GetPathName();
			m_editFindString.SetWindowText(zFullFileName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14513");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedBrowseReplaceFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "Text Files (*.txt)|*.txt"
											"|DAT Files (*.dat)|*.dat"
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
			CString zFullFileName = CString(m_strFileHeader.c_str()) + fileDlg.GetPathName();
			m_editReplaceString.SetWindowText(zFullFileName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14514");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedDynamicFindHelp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Dynamically loading a string from a file is supported\n"
					  "- For example, if the String to find edit box contains\n"
					  "  file://D:\\find.dat, the contents of the file will be\n"
					  "  loaded dynamically at run time for finding.\n\n"
					  "- The string should begin with \"file://\" and users can\n"
					  "  browse or use tags to define the file name.\n");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14603");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAdvancedReplaceStringPP::OnClickedDynamicReplaceHelp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Dynamically loading a string from a file is supported\n"
					  "- For example, if the Replace with edit box contains \n"
					  "  file://D:\\replace.dat, the contents of the file will be\n"
					  "  loaded dynamically at run time for replacement.\n\n"
					  "- The string should begin with \"file://\" and users can browse\n"
					  "  or use tags to define the file name.\n");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14637");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
bool CAdvancedReplaceStringPP::storeOccurrence(
		UCLID_AFVALUEMODIFIERSLib::IAdvancedReplaceStringPtr ipARS)
{
	try
	{
		int nSpecifiedOccurrence = GetDlgItemInt(IDC_EDIT_SPECIFIED_OCC, NULL, FALSE);
		ipARS->SpecifiedOccurrence = nSpecifiedOccurrence;

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05817");

	ATLControls::CEdit editSpecifiedOcc(GetDlgItem(IDC_EDIT_SPECIFIED_OCC));
	editSpecifiedOcc.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CAdvancedReplaceStringPP::storeToBeReplaced(
	   UCLID_AFVALUEMODIFIERSLib::IAdvancedReplaceStringPtr ipARS)
{
	try
	{
		CComBSTR bstrToBeReplaced;
		m_editFindString.GetWindowText(bstrToBeReplaced.m_str);
		ipARS->StrToBeReplaced = _bstr_t(bstrToBeReplaced);

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05816");

	m_editFindString.SetFocus();
	m_editFindString.SetSel(-1);
	
	return false;
}
//-------------------------------------------------------------------------------------------------
void CAdvancedReplaceStringPP::updateSpecifiedOccEdit()
{
	BOOL bEnableWindow = IsDlgButtonChecked(IDC_RADIO_SPECIFIED) == BST_CHECKED;
	ATLControls::CEdit editSpecified(GetDlgItem(IDC_EDIT_SPECIFIED_OCC));
	editSpecified.EnableWindow(bEnableWindow);
}
//-------------------------------------------------------------------------------------------------
void CAdvancedReplaceStringPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07686", 
		"AdvancedReplaceString Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
