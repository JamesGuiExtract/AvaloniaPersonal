// LaunchAppFileProcessorPP.cpp : Implementation of CLaunchAppFileProcessorPP
// LaunchAppFileProcessorPP.cpp : Implementation of CLaunchAppFileProcessorPP
#include "stdafx.h"
#include "FileProcessors.h"
#include "LaunchAppFileProcessorPP.h"
#include "XBrowseForFolder.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>

#include <vector>
#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CLaunchAppFileProcessorPP
//-------------------------------------------------------------------------------------------------
CLaunchAppFileProcessorPP::CLaunchAppFileProcessorPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLELaunchAppFileProcessorPP;
		m_dwHelpFileID = IDS_HELPFILELaunchAppFileProcessorPP;
		m_dwDocStringID = IDS_DOCSTRINGLaunchAppFileProcessorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI12206")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessorPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CLaunchAppFileProcessorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CLaunchAppFileProcessorPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_FILEPROCESSORSLib::ILaunchAppFileProcessorPtr ipFP(m_ppUnk[i]);
			if (ipFP)
			{
				CComBSTR bstrCmdLineFileName;
				m_editCmdLine.GetWindowText(&bstrCmdLineFileName);
				CComBSTR bstrWorkingDirFileName;
				m_editWorkingDir.GetWindowText(&bstrWorkingDirFileName);
				CComBSTR bstrParameters;
				m_editParameters.GetWindowText(&bstrParameters);

				ipFP->CommandLine = _bstr_t(bstrCmdLineFileName);
				ipFP->WorkingDirectory = _bstr_t(bstrWorkingDirFileName);
				ipFP->Parameters = _bstr_t(bstrParameters);

				ipFP->IsBlocking = asVariantBool(m_radioBlocking.GetCheck() == BST_CHECKED);

				ipFP->PropagateErrors = asVariantBool(m_checkPropErrors.GetCheck() == BST_CHECKED);
			}
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12207")

	// An Exception was caught
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CLaunchAppFileProcessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSORSLib::ILaunchAppFileProcessorPtr ipFP = m_ppUnk[0];
		if (ipFP)
		{
			m_editCmdLine = GetDlgItem(IDC_EDIT_CMD_LINE);
			m_btnCmdLineSelectTag.SubclassDlgItem(IDC_BTN_SELECT_CMD_LINE_DOC_TAG,
				CWnd::FromHandle(m_hWnd));
			m_btnCmdLineSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource,
				MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnCmdLineBrowse = GetDlgItem(IDC_BTN_BROWSE_CMD_LINE);

			m_editWorkingDir = GetDlgItem(IDC_EDIT_WORKING_DIR);
			m_btnWorkingDirSelectTag.SubclassDlgItem(IDC_BTN_SELECT_WORKING_DIR_DOC_TAG,
				CWnd::FromHandle(m_hWnd));
			m_btnWorkingDirSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource,
				MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnWorkingDirBrowse = GetDlgItem(IDC_BTN_BROWSE_WORKING_DIR);

			m_editParameters = GetDlgItem(IDC_EDIT_PARAMETERS);
			m_btnParametersSelectTag.SubclassDlgItem(IDC_BTN_PARAMETERS_DOC_TAG,
				CWnd::FromHandle(m_hWnd));
			m_btnParametersSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource,
				MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnParametersBrowse = GetDlgItem(IDC_BTN_PARAMETERS_BROWSE);

			m_radioBlocking = GetDlgItem(IDC_RADIO_BLOCKING);
			m_radioNonBlocking = GetDlgItem(IDC_RADIO_NON_BLOCKING);

			m_checkPropErrors = GetDlgItem(IDC_CHECK_PROP_ERRORS);

			// Set the propagate errors checkbox
			bool bPropErrors = asCppBool(ipFP->PropagateErrors);
			m_checkPropErrors.SetCheck(asBSTChecked(bPropErrors));
			if (bPropErrors)
			{
				m_radioBlocking.SetCheck(BST_CHECKED);
				m_radioBlocking.EnableWindow(FALSE);
				m_radioNonBlocking.SetCheck(BST_UNCHECKED);
				m_radioNonBlocking.EnableWindow(FALSE);
			}
			else
			{
				if (ipFP->IsBlocking == VARIANT_TRUE)
				{
					m_radioBlocking.SetCheck(BST_CHECKED);
				}
				else
				{
					m_radioNonBlocking.SetCheck(BST_CHECKED);
				}
			}

			m_editCmdLine.SetWindowText(ipFP->CommandLine);
			m_editWorkingDir.SetWindowText(ipFP->WorkingDirectory);
			m_editParameters.SetWindowText(ipFP->Parameters);
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12208");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLaunchAppFileProcessorPP::OnClickedBtnCmdLineSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		RECT rect;
		m_btnCmdLineSelectTag.GetWindowRect(&rect);
		string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			m_editCmdLine.ReplaceSel(strChoice.c_str(), TRUE);
		}
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12209");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLaunchAppFileProcessorPP::OnClickedBtnCmdLineBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string strFile = chooseFile();	
		if (strFile != "")
		{
			m_editCmdLine.SetWindowText(strFile.c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12210");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLaunchAppFileProcessorPP::OnClickedBtnWorkingDirSelectTag(WORD wNotifyCode, WORD wID,
																   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		RECT rect;
		m_btnWorkingDirSelectTag.GetWindowRect(&rect);
		string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			m_editWorkingDir.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12211");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLaunchAppFileProcessorPP::OnClickedBtnWorkingDirBrowse(WORD wNotifyCode, WORD wID,
																HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		char pszPath[MAX_PATH + 1] = {0};

		if(!XBrowseForFolder(m_hWnd, NULL, pszPath, sizeof(pszPath)))
		{
			// Check if there was an actual error (not user hitting cancel)
			// otherwise just return (do not clear the edit box) [LRCAU #5291 & FlexIDSCore #3001]
			DWORD dwError = GetLastError();
			if (dwError != 0)
			{
				UCLIDException ue("ELI13471", "Error getting working directory!");
				ue.addWin32ErrorInfo(dwError);
				ue.display();	
			}
			else
			{
				return 0;
			}
		}
		
		if (pszPath != "")
		{
			// Set the working directory path
			m_editWorkingDir.SetWindowText(pszPath);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12212");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLaunchAppFileProcessorPP::OnClickedBtnParametersSelectTag(WORD wNotifyCode, WORD wID,
																   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		RECT rect;
		m_btnParametersSelectTag.GetWindowRect(&rect);
		string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			m_editParameters.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25467");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLaunchAppFileProcessorPP::OnClickedBtnParametersBrowse(WORD wNotifyCode, WORD wID,
																HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string strFile = chooseFile();	
		if (strFile != "")
		{
			// Place quotes around the file name [LRCAU #4910]
			strFile = "\"" + strFile + "\"";
			m_editParameters.ReplaceSel(strFile.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25476");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLaunchAppFileProcessorPP::OnClickedCheckPropogateErrors(WORD wNotifyCode, WORD wID,
																 HWND hWndCtl, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// If propogating errors then the launch app processor must be set to blocking
		bool bChecked = m_checkPropErrors.GetCheck() == BST_CHECKED;
		if (bChecked)
		{
			m_radioBlocking.SetCheck(BST_CHECKED);
			m_radioBlocking.EnableWindow(FALSE);
			m_radioNonBlocking.SetCheck(BST_UNCHECKED);
			m_radioNonBlocking.EnableWindow(FALSE);
		}
		else
		{
			m_radioBlocking.EnableWindow(TRUE);
			m_radioNonBlocking.EnableWindow(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29825");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
const string CLaunchAppFileProcessorPP::chooseFile()
{
	const static string s_strAllFiles = "All Files (*.*)|*.*||";
	string strFileExtension(s_strAllFiles);

	// bring open file dialog
	CFileDialog fileDlg(TRUE, NULL, "", 
		OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
		s_strAllFiles.c_str(), CWnd::FromHandle(m_hWnd));
	
	// Pass the pointer of dialog to create ThreadFileDlg object
	ThreadFileDlg tfd(&fileDlg);

	// If cancel button is clicked
	if (tfd.doModal() != IDOK)
	{
		return "";
	}
	
	string strFile = (LPCTSTR)fileDlg.GetPathName();

	return strFile;
}
//-------------------------------------------------------------------------------------------------
void CLaunchAppFileProcessorPP::validateLicense()
{
	static const unsigned long LAUNCHAPPLICATION_PP_COMPONENT_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE( LAUNCHAPPLICATION_PP_COMPONENT_ID, "ELI12213", 
		"LaunchApplication File Processor PP" );
}
//-------------------------------------------------------------------------------------------------
