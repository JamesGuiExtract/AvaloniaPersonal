// FindFromRSDPP.cpp : Implementation of CFindFromRSDPP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "FindFromRSDPP.h"


#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <EditorLicenseID.h>
#include "..\..\AFCore\Code\Common.h"
#include <AFTagManager.h>
//-------------------------------------------------------------------------------------------------
// CFindFromRSDPP
//-------------------------------------------------------------------------------------------------
CFindFromRSDPP::CFindFromRSDPP() 
{
	m_dwTitleID = IDS_TITLEFindFromRSDPP;
	m_dwHelpFileID = IDS_HELPFILEFindFromRSDPP;
	m_dwDocStringID = IDS_DOCSTRINGFindFromRSDPP;
}
//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSDPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CFindFromRSDPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the SPM finder
			UCLID_AFVALUEFINDERSLib::IFindFromRSDPtr ipFinder = m_ppUnk[i];

			CComBSTR bstrName;
			GetDlgItemText(IDC_EDIT_ATTRIBUTE_NAME, bstrName.m_str);
			// Test if this is a valid attribute name
			IAttributePtr ipTmpAttr(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI10248", ipTmpAttr != NULL);

			ipTmpAttr->Name = _bstr_t(bstrName);
			ipFinder->AttributeName = _bstr_t(bstrName);
			
			CComBSTR bstrRSDFileName;
			GetDlgItemText(IDC_EDIT_RSD_FILE, bstrRSDFileName.m_str);
			ipFinder->RSDFileName = _bstr_t(bstrRSDFileName);
		}
			
		// if we reached here, then the data was successfully transfered
		// from the UI to the object.
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10221")

	// if we reached here, it's because of an exception
	// An error message has already been displayed to the user.
	// Return S_FALSE to indicate to the outer scope that the
	// Apply was not successful.
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CFindFromRSDPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEFINDERSLib::IFindFromRSDPtr ipFinder = m_ppUnk[0];

		if (ipFinder)
		{
			m_editAttributeName = GetDlgItem(IDC_EDIT_ATTRIBUTE_NAME);
			m_editRSDFileName = GetDlgItem(IDC_EDIT_RSD_FILE);
			m_btnSelectDocTag = GetDlgItem(IDC_BTN_SELECT_DOC_TAG);
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			string strAttributeName = ipFinder->AttributeName;
			m_editAttributeName.SetWindowText( strAttributeName.c_str());

			string strRSDFileName = ipFinder->RSDFileName;
			m_editRSDFileName.SetWindowText( strRSDFileName.c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10222");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFindFromRSDPP::OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10247");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFindFromRSDPP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
			m_editRSDFileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12007");

	return 0;
}