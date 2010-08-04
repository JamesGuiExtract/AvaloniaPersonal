// FileExistencePP.cpp : Implementation of CFileExistencePP
#include "stdafx.h"
#include "ESSkipConditions.h"
#include "FileExistencePP.h"
#include "SkipConditionUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// CFileExistencePP
//-------------------------------------------------------------------------------------------------
CFileExistencePP::CFileExistencePP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEFileExistencePP;
		m_dwHelpFileID = IDS_HELPFILEFileExistencePP;
		m_dwDocStringID = IDS_DOCSTRINGFileExistencePP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13579")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistencePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CFileExistencePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CFileExistencePP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			EXTRACT_FAMCONDITIONSLib::IFileExistenceFAMConditionPtr ipFileExistence(m_ppUnk[i]);
			if (ipFileExistence)
			{
				// save FAM condition
				if (!saveFileFAMCondition(ipFileExistence))
				{
					return S_FALSE;
				}
			}
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13580")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CFileExistencePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FAMCONDITIONSLib::IFileExistenceFAMConditionPtr ipFileExistence = m_ppUnk[0];
		if (ipFileExistence)
		{
			// Set the DocTag button and the browse button
			m_cmbDoes = GetDlgItem(IDC_CMB_DOESEXIST);
			m_FileName = GetDlgItem(IDC_EDT_FILENAME);
			m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_FILE);
			m_btnSelectDocTag.SubclassDlgItem(IDC_BTN_SELECT_DOC_TAG, CWnd::FromHandle(m_hWnd));
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			// Insert "does" and "does not" into combobox
			m_cmbDoes.InsertString(0, _bstr_t("does"));
			m_cmbDoes.InsertString(1, _bstr_t("does not"));
			m_cmbDoes.SetCurSel(ipFileExistence->FileExists == VARIANT_TRUE ? 0 : 1);

			// Insert File name
			string strFileName = ipFileExistence->FileString;
			m_FileName.SetWindowText(strFileName.c_str());

			// Set focus to the editbox
			m_FileName.SetSel(0, -1);
			m_FileName.SetFocus();
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13581");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileExistencePP::OnBnClickedBtnSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		RECT rect;
		m_btnSelectDocTag.GetWindowRect(&rect);
		
		// Choose the needed tags
		std::string strChoice = CFAMConditionUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);

		if (strChoice != "")
		{
			m_FileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13577");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileExistencePP::OnBnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "All Files (*.*)|*.*||";

		// bring open file dialog
		string strFileExtension(s_strAllFiles);
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY, strFileExtension.c_str(), CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// if the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			// get the file name
			m_FileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13582");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CFileExistencePP::saveFileFAMCondition(EXTRACT_FAMCONDITIONSLib::IFileExistenceFAMConditionPtr ipFileExistence)
{
	CComBSTR bstrFileName;

	try
	{
		// Store Does or Does not
		int nCurrentIndex = m_cmbDoes.GetCurSel();
		ipFileExistence->FileExists = asVariantBool(nCurrentIndex == 0);

		// Get the file name
		GetDlgItemText(IDC_EDT_FILENAME, bstrFileName.m_str);
		_bstr_t _bstrFileName(bstrFileName);
		if (_bstrFileName.length() == 0)
		{
			throw UCLIDException("ELI13578", "The FAM condition file name has not been specified!");
		}

		// Store the filename
		ipFileExistence->FileString = _bstrFileName;
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13583");

	return false;
}
//-------------------------------------------------------------------------------------------------
void CFileExistencePP::validateLicense()
{
	static const unsigned long FE_PP_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( FE_PP_COMPONENT_ID, "ELI13584", "File Existence PP" );
}
//-------------------------------------------------------------------------------------------------
