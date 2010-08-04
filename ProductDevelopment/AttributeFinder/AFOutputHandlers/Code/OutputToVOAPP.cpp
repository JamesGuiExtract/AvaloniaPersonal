// OutputToVOAPP.cpp : Implementation of COutputToVOAPP
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "OutputToVOAPP.h"
#include "..\..\AFCore\Code\EditorLicenseID.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// COutputToVOAPP
//-------------------------------------------------------------------------------------------------
COutputToVOAPP::COutputToVOAPP()
{
	m_dwTitleID = IDS_TITLEOutputToVOAPP;
	m_dwHelpFileID = IDS_HELPFILEOutputToVOAPP;
	m_dwDocStringID = IDS_DOCSTRINGOutputToVOAPP;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOAPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("COutputToVOAPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the output handler object
			UCLID_AFOUTPUTHANDLERSLib::IOutputToVOAPtr ipOutputToVOA = m_ppUnk[i];

			// if the edit box does not have text, then it's an error conditition
			CComBSTR bstrFileName;
			GetDlgItemText(IDC_EDIT_FILENAME, bstrFileName.m_str);
			_bstr_t _bstrFileName(bstrFileName);
			if (_bstrFileName.length() == 0)
			{
				throw UCLIDException("ELI08876", "Please specify an output filename!");
			}

			// store the filename
			ipOutputToVOA->FileName = _bstrFileName;
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08877")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOAPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT COutputToVOAPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IOutputToVOAPtr ipOutputToVOA = m_ppUnk[0];
		if (ipOutputToVOA)
		{
			// initialize controls
			m_editFileName = GetDlgItem(IDC_EDIT_FILENAME);
			m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_FILE);
			m_btnSelectDocTag = GetDlgItem(IDC_BTN_SELECT_DOC_TAG);
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			string strFileName = ipOutputToVOA->FileName;
			m_editFileName.SetWindowText(strFileName.c_str());

			// set focus to the editbox
			m_editFileName.SetSel(0, -1);
			m_editFileName.SetFocus();
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08878");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COutputToVOAPP::OnClickedBtnBrowseFile(WORD wNotifyCode, 
											   WORD wID, HWND hWndCtl, 
											   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "VOA Files (*.voa)|*.voa"
											"|All Files (*.*)|*.*||";

		// bring open file dialog
		string strFileExtension(s_strAllFiles);
		CFileDialog fileDlg(FALSE, ".rsd", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_OVERWRITEPROMPT,
			strFileExtension.c_str(), NULL);
		
		// if the user clicked on OK, then update the filename in the editbox
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			m_editFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08879");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COutputToVOAPP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
			m_editFileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12009");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void COutputToVOAPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI08884", "OutputToVOA PP" );
}
//-------------------------------------------------------------------------------------------------
