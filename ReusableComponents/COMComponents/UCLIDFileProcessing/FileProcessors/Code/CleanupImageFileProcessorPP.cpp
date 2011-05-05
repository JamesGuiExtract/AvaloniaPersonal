#include "stdafx.h"
#include "CleanupImageFileProcessorPP.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <LoadFileDlgThread.h>
#include <ComUtils.h>
#include <DocTagUtils.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrFILE_FILTER = "Image Cleanup Settings Files (*.ics.etf)|*.ics.etf||";

//-------------------------------------------------------------------------------------------------
// CCleanupImageFileProcessorPP
//-------------------------------------------------------------------------------------------------
CCleanupImageFileProcessorPP::CCleanupImageFileProcessorPP()
{
	try
	{
		m_dwTitleID = IDS_TITLECleanupImageFileProcessorPP;
		m_dwHelpFileID = IDS_HELPFILECleanupImageFileProcessorPP;
		m_dwDocStringID = IDS_DOCSTRINGCleanupImageFileProcessorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17307");
}
//-------------------------------------------------------------------------------------------------
CCleanupImageFileProcessorPP::~CCleanupImageFileProcessorPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17308");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanupImageFileProcessorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CCleanupImageFileProcessorPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_FILEPROCESSORSLib::ICleanupImageFileProcessorPtr ipFP(m_ppUnk[i]);
			if (ipFP)
			{
				CString zSettingsFile;
				m_edtSettingsFileName.GetWindowText(zSettingsFile);

				CString zExtension = zSettingsFile.Right(4);
				if (zExtension.MakeLower() == ".etf")
				{
					ipFP->ImageCleanupSettingsFileName = get_bstr_t(zSettingsFile);
				}
				else
				{
					UCLIDException ue("ELI17383", "File type must be .etf!");
					ue.addDebugInfo("File Name", string(zSettingsFile));
					throw ue;
				}
			}
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17351");

	// an exceptions was caught

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT CCleanupImageFileProcessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, 
												   LPARAM lParam, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		UCLID_FILEPROCESSORSLib::ICleanupImageFileProcessorPtr ipFP = m_ppUnk[0];
		if (ipFP)
		{
			// get the edit box
			m_edtSettingsFileName = GetDlgItem(IDC_EDIT_ICS_FILENAME);

			// set the settings file name
			m_edtSettingsFileName.SetWindowText(ipFP->ImageCleanupSettingsFileName);

			// get the tag button
			m_btnSelectTag.SubclassDlgItem(IDC_BTN_SELECT_ICS_FILENAME_DOC_TAG, CWnd::FromHandle(m_hWnd));

			// set the icon for the tag button
			m_btnSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
				MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17352");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCleanupImageFileProcessorPP::OnClickedBtnFileSelectTag(WORD wNotifyCode, WORD wID, 
																HWND hWndCtl, BOOL &bHandled)
{

	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ChooseDocTagForEditBox(IFAMTagManagerPtr(CLSID_FAMTagManager), m_btnSelectTag,
			m_edtSettingsFileName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17353");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCleanupImageFileProcessorPP::OnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, 
															 HWND hWndCtl, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// create open file dialog
		CFileDialog fileDlg(TRUE, NULL, NULL, OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrFILE_FILTER.c_str(), CWnd::FromHandle(m_hWnd));

		// pass the dialog to ThreadFileDlg object
		ThreadFileDlg tfd(&fileDlg);

		// display dialog box and if the user selects a file, set the edit box with the selected text
		if (tfd.doModal() == IDOK)
		{
			m_edtSettingsFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17355");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CCleanupImageFileProcessorPP::getEditBoxSelection(int& rnStartChar, int& rnEndChar)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// get the start and end position of the current selection.
		// if nothing is selected this just returns the cursor position as both the
		// start and end value.
		m_edtSettingsFileName.GetSel(rnStartChar, rnEndChar);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17354");
}
//-------------------------------------------------------------------------------------------------