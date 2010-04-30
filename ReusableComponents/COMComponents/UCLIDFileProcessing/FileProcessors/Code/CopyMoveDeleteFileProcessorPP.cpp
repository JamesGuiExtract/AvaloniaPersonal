// CopyMoveDeleteFileProcessorPP.cpp : Implementation of CCopyMoveDeleteFileProcessorPP
#include "stdafx.h"
#include "FileProcessors.h"
#include "CopyMoveDeleteFileProcessorPP.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <FileDialogEx.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>

#include <vector>
#include <string>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giMAX_FILE_HISTORY_SIZE = 10;

//-------------------------------------------------------------------------------------------------
// CCopyMoveDeleteFileProcessorPP
//-------------------------------------------------------------------------------------------------
CCopyMoveDeleteFileProcessorPP::CCopyMoveDeleteFileProcessorPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLECopyMoveDeleteFileProcessorPP;
		m_dwHelpFileID = IDS_HELPFILECopyMoveDeleteFileProcessorPP;
		m_dwDocStringID = IDS_DOCSTRINGCopyMoveDeleteFileProcessorPP;

		// Create User confgure manager to get or set list history in the combo boxes
		ma_pUserCfgMgr = auto_ptr<RegistryPersistenceMgr>( new RegistryPersistenceMgr(HKEY_CURRENT_USER, 
			FileProcessorsConfigMgr::FP_REGISTRY_PATH));

		ma_pCfgMgr = auto_ptr<FileProcessorsConfigMgr> (new FileProcessorsConfigMgr(ma_pUserCfgMgr.get(), "\\CopyMoveDeleteFile"));
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI12181")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessorPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CCopyMoveDeleteFileProcessorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CCopyMoveDeleteFileProcessorPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_FILEPROCESSORSLib::ICopyMoveDeleteFileProcessorPtr ipFP(m_ppUnk[i]);
			if (ipFP)
			{
				// Retrieve Source and Destination
				CComBSTR bstrSrcFileName;
				m_cmbSrc.GetWindowText(&bstrSrcFileName);
				CComBSTR bstrDstFileName;
				m_cmbDst.GetWindowText(&bstrDstFileName);

				// Set Operation
				if (m_radioMove.GetCheck() == 1)
				{
					ipFP->SetMoveFiles(_bstr_t(bstrSrcFileName), _bstr_t(bstrDstFileName));
				}
				else if (m_radioCopy.GetCheck() == 1)
				{
					ipFP->SetCopyFiles(_bstr_t(bstrSrcFileName), _bstr_t(bstrDstFileName));
				}
				else if (m_radioDelete.GetCheck() == 1)
				{
					ipFP->SetDeleteFiles(_bstr_t(bstrSrcFileName));
				}
				else
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI12182");
				}

				// Set allow readonly
				ipFP->AllowReadonly = asVariantBool(m_btnAllowReadonly.GetCheck() == 1);

				// Handle Source radio buttons
				if (m_radioSrcErr.GetCheck() == 1)
				{
					ipFP->SourceMissingType = UCLID_FILEPROCESSORSLib::kCMDSourceMissingError;
				}
				else if (m_radioSrcSkip.GetCheck() == 1)
				{
					ipFP->SourceMissingType = UCLID_FILEPROCESSORSLib::kCMDSourceMissingSkip;
				}
				else
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI19458");
				}

				// Handle Destination folder creation
				ipFP->CreateFolder = (m_btnCreateFolder.GetCheck() == 1) 
					? VARIANT_TRUE : VARIANT_FALSE;

				// Handle Destination radio buttons
				if (m_radioDstErr.GetCheck() == 1)
				{
					ipFP->DestinationPresentType = 
						UCLID_FILEPROCESSORSLib::kCMDDestinationPresentError;
				}
				else if (m_radioDstSkip.GetCheck() == 1)
				{
					ipFP->DestinationPresentType = 
						UCLID_FILEPROCESSORSLib::kCMDDestinationPresentSkip;
				}
				else if (m_radioDstOver.GetCheck() == 1)
				{
					ipFP->DestinationPresentType = 
						UCLID_FILEPROCESSORSLib::kCMDDestinationPresentOverwrite;
				}
				else
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI19423");
				}

				// Save the current source and destination files to history
				pushCurrentFilesToHistory(true);
				pushCurrentFilesToHistory(false);
			}
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12183")

	// An Exception was caught
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSORSLib::ICopyMoveDeleteFileProcessorPtr ipFP = m_ppUnk[0];
		if (ipFP)
		{
			// Get main radio-button controls
			m_radioMove = GetDlgItem(IDC_RADIO_MOVE);
			m_radioCopy = GetDlgItem(IDC_RADIO_COPY);
			m_radioDelete = GetDlgItem(IDC_RADIO_DELETE);

			// Get the allow readonly checkbox
			m_btnAllowReadonly = GetDlgItem(IDC_CHECK_ALLOW_READONLY);

			// Get Source items
			m_cmbSrc = GetDlgItem(IDC_CMB_SRC_FILE);
			m_btnSrcSelectTag.SubclassDlgItem(IDC_BTN_SELECT_SRC_DOC_TAG, CWnd::FromHandle(m_hWnd));
			m_btnSrcSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnSrcBrowse = GetDlgItem(IDC_BTN_BROWSE_SRC);
			m_radioSrcErr = GetDlgItem( IDC_RADIO_NOSRC_ERR );
			m_radioSrcSkip = GetDlgItem( IDC_RADIO_NOSRC_NOERR );

			// Get Destination items
			m_cmbDst = GetDlgItem(IDC_CMB_DST_FILE);
			m_btnDstSelectTag.SubclassDlgItem(IDC_BTN_SELECT_DST_DOC_TAG, CWnd::FromHandle(m_hWnd));
			m_btnDstSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnDstBrowse = GetDlgItem(IDC_BTN_BROWSE_DST);
			m_radioDstErr = GetDlgItem( IDC_RADIO_DEST_ERR );
			m_radioDstSkip = GetDlgItem( IDC_RADIO_DEST_NOERR );
			m_radioDstOver = GetDlgItem( IDC_RADIO_DEST_OVERWRITE );
			m_btnCreateFolder = GetDlgItem( IDC_CHECK_DIRECTORY );

			// Set allow readonly
			m_btnAllowReadonly.SetCheck(asCppBool(ipFP->AllowReadonly) ? 1 : 0);

			// Set operation radio button
			BOOL bTmp;
			ECopyMoveDeleteOperationType eOperation = (ECopyMoveDeleteOperationType)ipFP->Operation;
			switch(eOperation)
			{
			case kCMDOperationMoveFile:
				m_radioMove.SetCheck(1);
				OnClickedRadioMove(0, 0, 0, bTmp);
				break;
			case kCMDOperationCopyFile:
				m_radioCopy.SetCheck(1);
				OnClickedRadioCopy(0, 0, 0, bTmp);
				break;
			case kCMDOperationDeleteFile:
				m_radioDelete.SetCheck(1);
				OnClickedRadioDelete(0, 0, 0, bTmp);
				break;
			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI12185");
			}

			// set the files drop down list for source and destination 
			// combo boxes
			setHistory(true);
			setHistory(false);

			// Populate combo boxes
			m_cmbSrc.SetWindowText(ipFP->SourceFileName);
			m_cmbDst.SetWindowText(ipFP->DestinationFileName);

			// Set source radio button
			if (ipFP->SourceMissingType == kCMDSourceMissingError)
			{
				m_radioSrcErr.SetCheck( 1 );
			}
			else if (ipFP->SourceMissingType == kCMDSourceMissingSkip)
			{
				m_radioSrcSkip.SetCheck( 1 );
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI19424");
			}

			// Set folder creation
			m_btnCreateFolder.SetCheck( (ipFP->CreateFolder == VARIANT_TRUE) ? 1 : 0 );

			// Set destination radio button
			if (ipFP->DestinationPresentType == kCMDDestinationPresentError)
			{
				m_radioDstErr.SetCheck( 1 );
			}
			else if (ipFP->DestinationPresentType == kCMDDestinationPresentSkip)
			{
				m_radioDstSkip.SetCheck( 1 );
			}
			else if (ipFP->DestinationPresentType == kCMDDestinationPresentOverwrite)
			{
				m_radioDstOver.SetCheck( 1 );
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI19425");
			}
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12184");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnClickedRadioMove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Enable Destination controls
		m_cmbDst.EnableWindow(TRUE);
		m_btnDstSelectTag.EnableWindow(TRUE);
		m_btnDstBrowse.EnableWindow(TRUE);

		m_btnCreateFolder.EnableWindow( TRUE );
		m_radioDstErr.EnableWindow( TRUE );
		m_radioDstSkip.EnableWindow( TRUE );
		m_radioDstOver.EnableWindow( TRUE );

		// [FlexIDSCore #3176] Disable readonly checkbox
		m_btnAllowReadonly.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12186");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnClickedRadioCopy(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Enable Destination controls
		m_cmbDst.EnableWindow(TRUE);
		m_btnDstSelectTag.EnableWindow(TRUE);
		m_btnDstBrowse.EnableWindow(TRUE);

		m_btnCreateFolder.EnableWindow( TRUE );
		m_radioDstErr.EnableWindow( TRUE );
		m_radioDstSkip.EnableWindow( TRUE );
		m_radioDstOver.EnableWindow( TRUE );

		// Disable readonly checkbox
		m_btnAllowReadonly.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12187");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnClickedRadioDelete(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Disable Destination controls
		m_cmbDst.EnableWindow(FALSE);
		m_btnDstSelectTag.EnableWindow(FALSE);
		m_btnDstBrowse.EnableWindow(FALSE);

		m_btnCreateFolder.EnableWindow( FALSE );
		m_radioDstErr.EnableWindow( FALSE );
		m_radioDstSkip.EnableWindow( FALSE );
		m_radioDstOver.EnableWindow( FALSE );

		// Enable readonly checkbox
		m_btnAllowReadonly.EnableWindow(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12188");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnClickedBtnSrcSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		RECT rect;
		m_btnSrcSelectTag.GetWindowRect(&rect);
		std::string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			// if a tag was selected insert at the current selection
			m_cmbSrc.Clear();
			CString zWindowText;
			m_cmbSrc.GetWindowText(zWindowText);
			zWindowText.Delete(LOWORD(m_dwSelSrc), HIWORD(m_dwSelSrc) - LOWORD(m_dwSelSrc) );
			zWindowText.Insert(LOWORD(m_dwSelSrc), strChoice.c_str());
			m_cmbSrc.SetWindowText(zWindowText);
		}
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12189");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnClickedBtnSrcBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Call chooseFile() to display file dialog box and get the
		// selected file
		std::string strFile = chooseFile(true);	
		if (strFile != "")
		{
			m_cmbSrc.SetWindowText(strFile.c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12190");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnClickedBtnDstSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		RECT rect;
		m_btnDstSelectTag.GetWindowRect(&rect);
		std::string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);
		if (strChoice != "")
		{
			// if a tag was selected insert at the current selection
			m_cmbDst.Clear();
			CString zWindowText;
			m_cmbDst.GetWindowText(zWindowText);
			zWindowText.Delete(LOWORD(m_dwSelDst), HIWORD(m_dwSelDst) - LOWORD(m_dwSelDst) );
			zWindowText.Insert(LOWORD(m_dwSelDst), strChoice.c_str());
			m_cmbDst.SetWindowText(zWindowText);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12191");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnClickedBtnDstBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Call chooseFile() to display file dialog box and get the
		// selected file
		std::string strFile = chooseFile(false);
		if (strFile != "")
		{
			m_cmbDst.SetWindowText(strFile.c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12192");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnCbnSelEndCancelCmbSrcFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	// Save the location of the current edit selection
	// It includes the starting and end position of the selection
	m_dwSelSrc = m_cmbSrc.GetEditSel();

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCopyMoveDeleteFileProcessorPP::OnCbnSelEndCancelCmbDstFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	// Save the location of the current edit selection
	// It includes the starting and end position of the selection
	m_dwSelDst = m_cmbDst.GetEditSel();

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
const std::string CCopyMoveDeleteFileProcessorPP::chooseFile(bool bIsSource)
{
	// Get the default file from the registry
	string strDefFile;
	if (ma_pCfgMgr.get())
	{
		if (bIsSource)
		{
			// Get the Last oppend source file from the registry
			strDefFile = ma_pCfgMgr->getLastOpenedSourceNameFromScope();
		}
		else
		{
			// Get the Last oppend folder from the registry
			strDefFile = ma_pCfgMgr->getLastOpenedDestNameFromScope();
		}
	}

	const static string s_strAllFiles = "All Files (*.*)|*.*||";
	string strFileExtension(s_strAllFiles);

	// bring open file dialog (if browse for source then require file to exist) [FlexIDSCore #4254]
	CFileDialogEx fileDlg(TRUE, NULL, strDefFile.c_str(), 
		OFN_HIDEREADONLY | (bIsSource ? OFN_FILEMUSTEXIST : 0)
		| OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
		s_strAllFiles.c_str(), CWnd::FromHandle(m_hWnd));
	
	// Pass the pointer of dialog to create ThreadFileDlg object
	ThreadFileDlg tfd(&fileDlg);

	// If cancel button is clicked
	if (tfd.doModal() != IDOK)
	{
		return "";
	}
	
	// Get the selected file name
	std::string strFile = fileDlg.GetPathName().operator LPCTSTR();

	if (ma_pCfgMgr.get())
	{
		// Store the selected file name in registry
		if (bIsSource)
		{
			ma_pCfgMgr->setLastOpenedSourceNameFromScope(strFile);
		}
		else
		{
			ma_pCfgMgr->setLastOpenedDestNameFromScope(strFile);
		}
	}

	return strFile;
}
//-------------------------------------------------------------------------------------------------
void CCopyMoveDeleteFileProcessorPP::setHistory(bool bIsSource)
{
	ATLControls::CComboBox * pCurComboBox;
	pCurComboBox = bIsSource ? &m_cmbSrc : &m_cmbDst;
	ASSERT_ARGUMENT("ELI15703", pCurComboBox != NULL);

	if (ma_pCfgMgr.get())
	{
		vector<string> vecFileList;
		// Get file history from registry
		if (bIsSource)
		{
			ma_pCfgMgr->getOpenedSourceHistoryFromScope(vecFileList);
		}
		else
		{
			ma_pCfgMgr->getOpenedDestHistoryFromScope(vecFileList);
		}

		for (unsigned int n = 0; n < vecFileList.size(); n++)
		{
			if (!vecFileList[n].empty())
			{
				// Add to history list
				pCurComboBox->AddString(vecFileList[n].c_str());
			}
		}

		if (vecFileList.size() > 0)
		{
			// select the first item
			pCurComboBox->SetCurSel(0);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CCopyMoveDeleteFileProcessorPP::saveHistory(bool bIsSource)
{
	ATLControls::CComboBox * pCurComboBox;
	pCurComboBox = bIsSource ? &m_cmbSrc : &m_cmbDst;
	ASSERT_ARGUMENT("ELI15702", pCurComboBox != NULL);

	vector<string> vecFiless;
	for(int i = 0; i < (pCurComboBox->GetCount()) && (i < giMAX_FILE_HISTORY_SIZE); i++)
	{
		CString zFile;
		// Get item from Combo box
		int n = pCurComboBox->GetLBTextLen(i);
		pCurComboBox->GetLBText(i, zFile.GetBuffer(n));
		string strTmp = zFile;
		// save on vector to save to registry
		vecFiless.push_back(strTmp);
	}

	if (bIsSource)
	{
		ma_pCfgMgr->setOpenedSourceHistoryFromScope(vecFiless);
	}
	else
	{
		ma_pCfgMgr->setOpenedDestHistoryFromScope(vecFiless);
	}
}
//-------------------------------------------------------------------------------------------------
void CCopyMoveDeleteFileProcessorPP::pushCurrentFilesToHistory(bool bIsSource)
{
	ATLControls::CComboBox * pCurComboBox;
	pCurComboBox = bIsSource ? &m_cmbSrc : &m_cmbDst;
	ASSERT_ARGUMENT("ELI15700", pCurComboBox != NULL);

	CString zFile("");
	// Get the current edit text in combo box
	pCurComboBox->GetWindowText(zFile);

	if(zFile == "")
	{
		return;
	}

	// Find current text in list
	long nAlreadyInList = pCurComboBox->FindStringExact(-1, zFile);
	if(nAlreadyInList >= 0)
	{
		// remove from list 
		pCurComboBox->DeleteString(nAlreadyInList);
	}
	else
	{
		// Remove strings if history list exceeds max size
		while(pCurComboBox->GetCount() >= giMAX_FILE_HISTORY_SIZE)
		{
			pCurComboBox->DeleteString(pCurComboBox->GetCount() - 1);
		}
	}

	// Add current text to position 0 of list
	pCurComboBox->InsertString(0, zFile);
	pCurComboBox->SetCurSel(0);

	// Save to registry
	saveHistory(bIsSource);
}
//-------------------------------------------------------------------------------------------------
void CCopyMoveDeleteFileProcessorPP::validateLicense()
{
	static const unsigned long COPYMOVEDELETEFP_PP_COMPONENT_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE( COPYMOVEDELETEFP_PP_COMPONENT_ID, "ELI12193", 
		"CopyMoveDelete File Processor PP" );
}
//-------------------------------------------------------------------------------------------------