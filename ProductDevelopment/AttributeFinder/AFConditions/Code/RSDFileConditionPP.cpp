// RSDFileConditionPP.cpp : Implementation of CRSDFileConditionPP
#include "stdafx.h"
#include "AFConditions.h"
#include "RSDFileConditionPP.h"
#include "..\..\AFCore\Code\EditorLicenseID.h"
#include "..\..\AFCore\Code\Common.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <AFTagManager.h>

#include <io.h>

using std::string;

//-------------------------------------------------------------------------------------------------
// CRSDFileConditionPP
//-------------------------------------------------------------------------------------------------
CRSDFileConditionPP::CRSDFileConditionPP()
{
	try
	{
		m_dwTitleID = IDS_TITLERSDFileConditionPP;
		m_dwHelpFileID = IDS_HELPFILERSDFileConditionPP;
		m_dwDocStringID = IDS_DOCSTRINGRSDFileConditionPP;

		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI10927", m_ipAFUtility != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10911");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileConditionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CRSDFileConditionPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFCONDITIONSLib::IRSDFileConditionPtr ipCondition = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI10912", ipCondition != NULL);

			// Set the .rsd file name
			m_editRSDFileName = GetDlgItem(IDC_EDIT_RSD_FILE);
			CComBSTR bstrName;
			m_editRSDFileName.GetWindowText(bstrName.m_str);
			_bstr_t _bstrName(bstrName);

			if(m_ipAFUtility->StringContainsTags(_bstrName) == VARIANT_FALSE)
			{
				// ensure that the file exists
				if (!isValidFile(asString(_bstrName)))
				{
					UCLIDException ue("ELI10910", "Specified .rsd file does not exist!");
					ue.addDebugInfo("RSD File", asString(_bstrName));
					ue.addWin32ErrorInfo();
					throw ue;
				}
			}
			ipCondition->RSDFileName = _bstrName;
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10913")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CRSDFileConditionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFCONDITIONSLib::IRSDFileConditionPtr ipCondition = m_ppUnk[0];
		if (ipCondition != NULL)
		{
			// Get controls
			m_editRSDFileName = GetDlgItem(IDC_EDIT_RSD_FILE);
			m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE);

			// Set ICON for tag button
			m_btnSelectDocTag = GetDlgItem( IDC_BTN_SELECT_DOC_TAG );
			m_btnSelectDocTag.SetIcon( ::LoadIcon( _Module.m_hInstResource, 
				MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG) ) );

			// Provide name of RSD file in edit box
			CString cstrName = (LPCSTR)_bstr_t(ipCondition->RSDFileName);
			m_editRSDFileName.SetWindowText(cstrName);
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10914");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRSDFileConditionPP::OnClickedBtnBrowse(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// bring open file dialog
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrRSD_FILE_OPEN_FILTER.c_str(), NULL);
		
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			m_editRSDFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10915");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRSDFileConditionPP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
													   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get position of doc tag button
		RECT rect;
		m_btnSelectDocTag.GetWindowRect(&rect);
		CRect rc(rect);
		
		// Display appropriate drop-down menu
		AFTagManager tagMgr;
		string strChoice = tagMgr.displayTagsForSelection(CWnd::FromHandle(m_hWnd), rc.right, rc.top);
		if (strChoice != "")
		{
			// Replace selected text with tag text
			m_editRSDFileName.ReplaceSel( strChoice.c_str(), TRUE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15715");

	return 0;
}
//-------------------------------------------------------------------------------------------------
