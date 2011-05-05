// DynamicFileListFSPP.cpp : Implementation of CDynamicFileListFSPP
#include "stdafx.h"
#include "ESFileSuppliers.h"
#include "DynamicFileListFSPP.h"
#include "FileSupplierUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>
#include <DocTagUtils.h>

//-------------------------------------------------------------------------------------------------
// CDynamicFileListFSPP
//-------------------------------------------------------------------------------------------------
CDynamicFileListFSPP::CDynamicFileListFSPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEDynamicFileListFSPP;
		m_dwHelpFileID = IDS_HELPFILEDynamicFileListFSPP;
		m_dwDocStringID = IDS_DOCSTRINGDynamicFileListFSPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13967")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFSPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CDynamicFileListFSPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CDynamicFileListFSPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			EXTRACT_FILESUPPLIERSLib::IDynamicFileListFSPtr ipDynamicFileListFS(m_ppUnk[i]);
			if (ipDynamicFileListFS)
			{
				// save dynamic list file
				if (!saveDynamicFileListFS(ipDynamicFileListFS))
				{
					return S_FALSE;
				}
			}
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13968")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CDynamicFileListFSPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FILESUPPLIERSLib::IDynamicFileListFSPtr ipDynamicFileListFS = m_ppUnk[0];
		if (ipDynamicFileListFS)
		{
			// Set the DocTag button and the browse button
			m_editFileName = GetDlgItem(IDC_EDT_FILENAME);
			m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_FILE);
			m_btnSelectDocTag.SubclassDlgItem(IDC_BTN_SELECT_DOC_TAG, CWnd::FromHandle(m_hWnd));
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			// Insert File name
			string strFileName = ipDynamicFileListFS->FileName;
			m_editFileName.SetWindowText(strFileName.c_str());

			// Set focus to the editbox
			m_editFileName.SetSel(0, -1);
			m_editFileName.SetFocus();
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13969");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDynamicFileListFSPP::OnBnClickedBtnSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ChooseDocTagForEditBox(IFAMTagManagerPtr(CLSID_FAMTagManager), m_btnSelectDocTag,
			m_editFileName, false);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13970");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDynamicFileListFSPP::OnBnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strTypes = "Text files (*.txt)|*.txt|All Files (*.*)|*.*||";

		// Bring open file dialog
		string strFileExtension( s_strTypes );

		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY, strFileExtension.c_str(), CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// If the user clicked on OK and the return file name is not empty, 
		// then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			// Get the file name
			m_editFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13971");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CDynamicFileListFSPP::saveDynamicFileListFS(EXTRACT_FILESUPPLIERSLib::IDynamicFileListFSPtr ipDynamicFileListFS)
{
	CComBSTR bstrFileName;

	try
	{
		// Get the file name
		GetDlgItemText(IDC_EDT_FILENAME, bstrFileName.m_str);
		_bstr_t _bstrFileName(bstrFileName);
		if (_bstrFileName.length() == 0)
		{
			// Set focus to the editbox
			m_editFileName.SetSel(0, -1);
			m_editFileName.SetFocus();

			throw UCLIDException("ELI13972", "The dynamic file supplier list file name has not been specified!");
		}

		// Store the filename
		ipDynamicFileListFS->FileName = _bstrFileName;
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13973");

	return false;
}
//-------------------------------------------------------------------------------------------------
void CDynamicFileListFSPP::validateLicense()
{
	static const unsigned long FE_PP_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( FE_PP_COMPONENT_ID, "ELI13974", "Dynamic List File Supplier PP" );
}
//-------------------------------------------------------------------------------------------------
