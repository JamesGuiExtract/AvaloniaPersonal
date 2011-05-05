// FileNamePatternPP.cpp : Implementation of CFileNamePatternPP
#include "stdafx.h"
#include "ESSkipConditions.h"
#include "FileNamePatternPP.h"
#include "SkipConditionUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>
#include <cpputil.h>
#include <DocTagUtils.h>

//-------------------------------------------------------------------------------------------------
// CFileNamePatternPP
//-------------------------------------------------------------------------------------------------
CFileNamePatternPP::CFileNamePatternPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEFileNamePatternPP;
		m_dwHelpFileID = IDS_HELPFILEFileNamePatternPP;
		m_dwDocStringID = IDS_DOCSTRINGFileNamePatternPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13656")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePatternPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CFileNamePatternPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CFileNamePatternPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			EXTRACT_FAMCONDITIONSLib::IFileNamePatternFAMConditionPtr ipFileNamePattern(m_ppUnk[i]);
			if (ipFileNamePattern)
			{
				// save FAM condition
				if (!saveFileFAMCondition(ipFileNamePattern))
				{
					return S_FALSE;
				}
			}
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13657")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CFileNamePatternPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FAMCONDITIONSLib::IFileNamePatternFAMConditionPtr ipFileNamePattern = m_ppUnk[0];
		if (ipFileNamePattern)
		{
			// Get the variables from dialog
			m_FileName = GetDlgItem(IDC_EDT_FAMCONDITION_FILENAME);
			m_btnSelectDocTag.SubclassDlgItem(IDC_BTN_SELECT_DOC_TAG, CWnd::FromHandle(m_hWnd));
			m_cmbDoes = GetDlgItem(IDC_CMB_DOES);
			m_cmbContain = GetDlgItem(IDC_CMB_CONTAIN);
			m_RegFileName = GetDlgItem(IDC_EDIT_REG_EXP_FILE);
			m_btnSelectDocTag2.SubclassDlgItem(IDC_BTN_SELECT_DOC_TAG2, CWnd::FromHandle(m_hWnd));
			m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_FILE);
			m_radioPatternText = GetDlgItem(IDC_RADIO_TEXT);
			m_RegPattern = GetDlgItem(IDC_EDIT_PATTERN);
			m_btnCaseSensitive = GetDlgItem(IDC_CHK_REG_EXP_CASE);

			// Set the DocTag button and the browse button
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnSelectDocTag2.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			// Insert "does" and "does not" into combobox
			m_cmbDoes.InsertString(0, _bstr_t("does"));
			m_cmbDoes.InsertString(1, _bstr_t("does not"));
			// Insert "contain" and "exactly match" into combobox
			m_cmbContain.InsertString(0, _bstr_t("contain"));
			m_cmbContain.InsertString(1, _bstr_t("exactly match"));

			m_cmbDoes.SetCurSel(ipFileNamePattern->DoesContainOrMatch == VARIANT_TRUE ? 0 : 1);
			m_cmbContain.SetCurSel(ipFileNamePattern->ContainMatch == VARIANT_TRUE ? 0 : 1);

			// Set the FAM condition file name
			string strFileName = ipFileNamePattern->FileString;
			m_FileName.SetWindowText(strFileName.c_str());

			// Set case sensitive
			bool bCaseSensitive = ipFileNamePattern->IsCaseSensitive == VARIANT_TRUE;
			m_btnCaseSensitive.SetCheck(bCaseSensitive ? 1 : 0);

			// Set the choice and text for regular expression source
			m_bIsRegExpFromFile = ipFileNamePattern->IsRegFromFile == VARIANT_TRUE;
			if ( m_bIsRegExpFromFile )
			{
				string strFileName = ipFileNamePattern->RegExpFileName;
				m_RegFileName.SetWindowText( strFileName.c_str());
			}
			else
			{
				string strPattern = ipFileNamePattern->RegPattern;
				m_RegPattern.SetWindowText( strPattern.c_str() );
			}

			// set focus to the editbox
			m_FileName.SetSel(0, -1);
			m_FileName.SetFocus();
			updateControls();
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13658");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileNamePatternPP::OnBnClickedBtnSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ChooseDocTagForEditBox(IFAMTagManagerPtr(CLSID_FAMTagManager), m_btnSelectDocTag,
			m_FileName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13659");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileNamePatternPP::OnBnClickedBtnSelectDocTag2(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ChooseDocTagForEditBox(IFAMTagManagerPtr(CLSID_FAMTagManager), m_btnSelectDocTag2,
			m_RegFileName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13660");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileNamePatternPP::OnBnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "DAT Files (*.dat)|*.dat"
											"|Encrypted Text Files (*.etf)|*.etf"
											"|All Files (*.*)|*.*||";

		// Bring open file dialog
		string strFileExtension(s_strAllFiles);
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			strFileExtension.c_str(), CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// If the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			// get the file name
			m_RegFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13661");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileNamePatternPP::OnBnClickedChkRegExpCase(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	SetDirty(TRUE);

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileNamePatternPP::OnBnClickedRadioFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Select the regular expression from file radio box and update the controls
		m_bIsRegExpFromFile = IsDlgButtonChecked(IDC_RADIO_FILE)==BST_CHECKED;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13666");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileNamePatternPP::OnBnClickedRadioText(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Select the regular expression from text radio box and update the controls
		m_bIsRegExpFromFile = IsDlgButtonChecked(IDC_RADIO_TEXT)==BST_UNCHECKED;
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13667");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFileNamePatternPP::OnEnChangeEditPattern(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	SetDirty(TRUE);

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CFileNamePatternPP::saveFileFAMCondition(EXTRACT_FAMCONDITIONSLib::IFileNamePatternFAMConditionPtr ipFileNamePattern)
{
	CComBSTR bstrName;

	try
	{
		// Store the variables
		int nCurrentIndex = m_cmbDoes.GetCurSel();
		ipFileNamePattern->DoesContainOrMatch = asVariantBool(nCurrentIndex == 0);

		nCurrentIndex = m_cmbContain.GetCurSel();
		ipFileNamePattern->ContainMatch = asVariantBool(nCurrentIndex == 0);

		bool isCaseSensitive = m_btnCaseSensitive.GetCheck() == BST_CHECKED;
 		ipFileNamePattern->IsCaseSensitive = asVariantBool(isCaseSensitive);

		bool isRegFromText = m_radioPatternText.GetCheck() == BST_CHECKED;
		ipFileNamePattern->IsRegFromFile = asVariantBool(!isRegFromText);

		GetDlgItemText(IDC_EDT_FAMCONDITION_FILENAME, bstrName.m_str);
		_bstr_t _bstrFileName(bstrName);
		if (_bstrFileName.length() == 0)
		{
			throw UCLIDException("ELI13675", "The FAM condition file name has not been specified!");
		}
		// store the filename
		ipFileNamePattern->FileString = _bstrFileName;

		// Consider regular expression from file or from text separately
		if (isRegFromText)
		{
			GetDlgItemText(IDC_EDIT_PATTERN, bstrName.m_str);
			_bstr_t _bstrPattern(bstrName);
			if (_bstrPattern.length() == 0)
			{
				throw UCLIDException("ELI13676", "The regular expression pattern has not been specified!");
			}
			// store the regular expression pattern
			ipFileNamePattern->RegPattern = _bstrPattern;
		}
		else
		{
			GetDlgItemText(IDC_EDIT_REG_EXP_FILE, bstrName.m_str);
			_bstr_t _bstrFileName(bstrName);
			if (_bstrFileName.length() == 0)
			{
				throw UCLIDException("ELI13677", "The regular expression file name has not been specified!");
			}
			// store the regular expression filename
			ipFileNamePattern->RegExpFileName = _bstrFileName;
		}

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13662");

	return false;
}
//-------------------------------------------------------------------------------------------------
void CFileNamePatternPP::updateControls()
{
	// Update controls based upon the settings
	CheckDlgButton(IDC_RADIO_TEXT, m_bIsRegExpFromFile?BST_UNCHECKED:BST_CHECKED);
	m_RegPattern.EnableWindow( asMFCBool(!m_bIsRegExpFromFile) );
	CheckDlgButton(IDC_RADIO_FILE, m_bIsRegExpFromFile?BST_CHECKED:BST_UNCHECKED);
	m_RegFileName.EnableWindow( asMFCBool(m_bIsRegExpFromFile) );
	m_btnSelectDocTag2.EnableWindow( asMFCBool(m_bIsRegExpFromFile) );
	m_btnBrowse.EnableWindow( asMFCBool(m_bIsRegExpFromFile) );
}
//-------------------------------------------------------------------------------------------------
void CFileNamePatternPP::validateLicense()
{
	static const unsigned long FNP_PP_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( FNP_PP_COMPONENT_ID, "ELI13663", "File Name Pattern FAM Condition PP" );
}
//-------------------------------------------------------------------------------------------------
