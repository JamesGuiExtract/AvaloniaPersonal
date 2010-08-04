// RSDSplitterPP.cpp : Implementation of CRSDSplitterPP
#include "stdafx.h"
#include "AFSplitters.h"
#include "RSDSplitterPP.h"
#include "..\..\AFCore\Code\Common.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// CRSDSplitterPP
//-------------------------------------------------------------------------------------------------
CRSDSplitterPP::CRSDSplitterPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLERSDSplitterPP;
		m_dwHelpFileID = IDS_HELPFILERSDSplitterPP;
		m_dwDocStringID = IDS_DOCSTRINGRSDSplitterPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07708")
}
//-------------------------------------------------------------------------------------------------
CRSDSplitterPP::~CRSDSplitterPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16328");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitterPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CRSDSplitterPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CRSDSplitterPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFSPLITTERSLib::IRSDSplitterPtr ipRSDSplitter = m_ppUnk[i];
			if (ipRSDSplitter)
			{
				try
				{
					try
					{
						CComBSTR bstrFileName;
						GetDlgItemText(IDC_EDIT_RSD_FILE, bstrFileName.m_str);
						
						ipRSDSplitter->RSDFileName = _bstr_t(bstrFileName);
					}
					CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05814");
				}
				catch (...)
				{
					ATLControls::CEdit editRESFileName = GetDlgItem(IDC_EDIT_RSD_FILE);
					editRESFileName.SetSel(0, -1);
					editRESFileName.SetFocus();
					return S_FALSE;
				}
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05770");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CRSDSplitterPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFSPLITTERSLib::IRSDSplitterPtr ipRSDSplitter(m_ppUnk[0]);
		if (ipRSDSplitter)
		{
			m_editRSDFileName = GetDlgItem(IDC_EDIT_RSD_FILE);
			m_btnSelectDocTag = GetDlgItem(IDC_BTN_SELECT_DOC_TAG);
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));


			// get rsd file
			string strRSDFile = ipRSDSplitter->RSDFileName;
			if (!strRSDFile.empty())
			{
				SetDlgItemText(IDC_EDIT_RSD_FILE, strRSDFile.c_str());
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05771");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRSDSplitterPP::OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		static CString zFileName("");

		// bring the file open dialog
		CFileDialog openDlg(TRUE, ".rsd", zFileName,
			OFN_HIDEREADONLY|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST|OFN_NOCHANGEDIR,
			gstrRSD_FILE_OPEN_FILTER.c_str());

		if (openDlg.DoModal() == IDOK)
		{
			zFileName = openDlg.GetPathName();

			// update the edit box
			SetDlgItemText(IDC_EDIT_RSD_FILE, zFileName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05772");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRSDSplitterPP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12006");

	return 0;
}
//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRSDSplitterPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07677", "RSDSplitter PP" );
}
//-------------------------------------------------------------------------------------------------
