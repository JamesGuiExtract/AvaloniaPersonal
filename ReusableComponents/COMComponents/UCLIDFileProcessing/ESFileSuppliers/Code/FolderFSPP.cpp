// FolderFSPP.cpp : Implementation of CFolderFSPP
#include "stdafx.h"
#include "FolderFSPP.h"
#include "FileSupplierUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <misc.h>
#include <COMUtils.h>
#include <XBrowseForFolder.h>
#include <RegConstants.h>
#include <RegistryPersistenceMgr.h>
#include <ComponentLicenseIDs.h>

#include <Mmc.h>
#include <io.h>
#include <fstream>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int iMAX_FOLDER_HISTORY_SIZE = 8;

//-------------------------------------------------------------------------------------------------
// CFolderFSPP
//-------------------------------------------------------------------------------------------------
CFolderFSPP::CFolderFSPP()
:	ma_pUserCfgMgr(NULL),
	ma_pCfgMgr(NULL)
{
	try
	{
		// Check licensing
		validateLicense();
		ma_pUserCfgMgr = auto_ptr<RegistryPersistenceMgr>( new RegistryPersistenceMgr(HKEY_CURRENT_USER, 
			gstrREG_ROOT_KEY + "\\ReusableComponents\\COMComponents\\ESFileSuppliers"));

		ma_pCfgMgr = auto_ptr<FileSupplierConfigMgr> (new FileSupplierConfigMgr(ma_pUserCfgMgr.get(), "\\FolderFS"));

	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13754")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFSPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CFolderFSPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CFolderFSPP::Apply\n"));

		EXTRACT_FILESUPPLIERSLib::IFolderFSPtr ipFileSup = m_ppUnk[0];

		if (ipFileSup)
		{
			// save dynamic list file
			if (!saveFolderFS(ipFileSup))
			{
				return S_FALSE;
			}
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13755")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CFolderFSPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FILESUPPLIERSLib::IFolderFSPtr ipFileSup = m_ppUnk[0];
		if (ipFileSup)
		{
			// Attache the controls 
			m_cmbFolder.Attach(GetDlgItem(IDC_CMB_FOLDER));
			m_btnSelectFolderDocTag.SubclassDlgItem(IDC_BTN_SELECT_FOLDER_DOC_TAG, CWnd::FromHandle(m_hWnd));
			// Set icon for  the Select doc tag
			m_btnSelectFolderDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_cmbFileExtension.Attach(GetDlgItem(IDC_CMB_FILE_EXTENSION));
			m_chkRecursive.Attach(GetDlgItem(IDC_CHK_RECURSIVE));
			m_chkContinuousProcess.Attach(GetDlgItem(IDC_CHK_CONTINUOUS));
			m_chkAdded.Attach(GetDlgItem(IDC_CHK_ADDED));
			m_chkModified.Attach(GetDlgItem(IDC_CHK_MODIFIED));
			m_chkTargetForRenameOrMove.Attach(GetDlgItem(IDC_CHK_TARGET_FOR_RENAME_OR_MOVE));
			m_chkNoExistingFiles.Attach(GetDlgItem(IDC_CHK_NO_EXISTING));

			// Set RecurseFolder check 
			int iChkState = ipFileSup->RecurseFolders == VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED;
			m_chkRecursive.SetCheck(iChkState);

			// Set continuous default state ( it will be checked if any of Added, Modified or Target of Rename or move are checked
			int iContinuousState = BST_UNCHECKED;

			// Set Added check
			iChkState = ipFileSup->AddedFiles == VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED;
			m_chkAdded.SetCheck(iChkState);

			// Update Continous flag
			iContinuousState = iContinuousState == BST_CHECKED ? BST_CHECKED : iChkState;

			// Set Modified Check
			iChkState = ipFileSup->ModifiedFiles == VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED;
			m_chkModified.SetCheck(iChkState);
			
			// Update Continous flag
			iContinuousState = iContinuousState == BST_CHECKED ? BST_CHECKED : iChkState;

			// Set Target of Move or rename check
			iChkState = ipFileSup->TargetOfMoveOrRename == VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED;
			m_chkTargetForRenameOrMove.SetCheck(iChkState);

			// Update Continous flag
			iContinuousState = iContinuousState == BST_CHECKED ? BST_CHECKED : iChkState;
			
			// Set Continous Check
			m_chkContinuousProcess.SetCheck( iContinuousState );

			// Set No existing files check
			iChkState = ipFileSup->NoExistingFiles == VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED;
			m_chkNoExistingFiles.SetCheck( iChkState );

			// Set the File extension drop down list
			setFileExtensions();
			// set the Folder drop down list
			setFolderHistory();
			
			// Get the folder name from the file supplier
			string strFolder = asString(ipFileSup->FolderName);

			// Get the extensions from the file supplier
			string strExt = asString(ipFileSup->FileExtensions);

			// Set the folder if the string is not empty, if it is empty the default is the last used
			if ( !strFolder.empty() )
			{
				// Set value of Folder name controls
				m_cmbFolder.SetWindowTextA(strFolder.c_str());
			};
			
			// Set the extensions if the string is not empty
			if ( !strExt.empty() )
			{
				// Set value of File extensions control
				m_cmbFileExtension.SetWindowTextA(strExt.c_str());
			};
			
			// Update the state of the buttons
			updateButtons();
		}
		// Set dirty to false
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13756");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFolderFSPP::OnBnClickedBtnContinuous(WORD /*wNotifyCode*/, WORD /*wID*/, 
											 HWND hWndCtl, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	// update the state of the buttons
	updateButtons();
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFolderFSPP::OnBnClickedBtnSelectFolderTag(WORD /*wNotifyCode*/, WORD /*wID*/, 
											 HWND hWndCtl, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	RECT rect;
	
	// Display the Select Folder tag popup
	m_btnSelectFolderDocTag.GetWindowRect(&rect);
	string str = CFileSupplierUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);

	if (str != "")
	{
		// if a tag was selected insert at the current selection
		m_cmbFolder.Clear();
		CString zWindowText;
		m_cmbFolder.GetWindowText(zWindowText);
		zWindowText.Delete(LOWORD(m_dwSel), HIWORD(m_dwSel) - LOWORD(m_dwSel) );
		zWindowText.Insert(LOWORD(m_dwSel), str.c_str());
		m_cmbFolder.SetWindowText(zWindowText);
		m_bDirty = true;
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFolderFSPP::OnCbnSelendcancelCmbFolder(WORD /*wNotifyCode*/, WORD /*wID*/, 
											 HWND hWndCtl, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	// Save the location of the current edit selection
	m_dwSel = m_cmbFolder.GetEditSel ();
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFolderFSPP::OnBnClickedBtnBrowse(WORD /*wNotifyCode*/, WORD /*wID*/, 
											 HWND hWndCtl, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		string strFolder("");
		if (ma_pCfgMgr.get())
		{
			// Get the Last oppend folder from the registry
			strFolder = ma_pCfgMgr->getLastOpenedFolderNameFromScope();
		}

		CString zFolder = strFolder.c_str();
		char pszPath[MAX_PATH + 1];

		// bring up the folder dialog
		if (!XBrowseForFolder(m_hWnd, zFolder, pszPath, sizeof(pszPath)))
		{
			return 0;
		}

		// put in the edit box
		m_cmbFolder.SetWindowText(pszPath);

		// Set the dirty flag because the folder has changed
		m_bDirty = true;

		if (ma_pCfgMgr.get())
		{
			// Store folder in registry
			strFolder = (LPCTSTR)pszPath;
			ma_pCfgMgr->setLastOpenedFolderNameFromScope(strFolder);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13758");
	return 0;

}
//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CFolderFSPP::updateButtons()
{
	if ( m_chkContinuousProcess.GetCheck() == BST_CHECKED )
	{
		// Enable buttons that are valid if Continous proceesing is checked
		m_chkAdded.EnableWindow( TRUE );
		m_chkModified.EnableWindow( TRUE );
		m_chkTargetForRenameOrMove.EnableWindow( TRUE );
		m_chkNoExistingFiles.EnableWindow( TRUE );
	}
	else
	{
		// Set items to be disabled to Unchecked
		m_chkAdded.SetCheck( BST_UNCHECKED );
		m_chkModified.SetCheck( BST_UNCHECKED );
		m_chkTargetForRenameOrMove.SetCheck( BST_UNCHECKED );
		m_chkNoExistingFiles.SetCheck( BST_UNCHECKED );
	
		// Disable the check boxes that should not be enabled unless Continuous process is checked
		m_chkAdded.EnableWindow( FALSE );
		m_chkModified.EnableWindow( FALSE );
		m_chkTargetForRenameOrMove.EnableWindow( FALSE );
		m_chkNoExistingFiles.EnableWindow( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CFolderFSPP::validateLicense()
{
	static const unsigned long FOLDER_FILE_LIST_FSPP = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( FOLDER_FILE_LIST_FSPP, "ELI13757", "Folder File Supplier PP" );
}
//-------------------------------------------------------------------------------------------------
void CFolderFSPP::setFileExtensions()
{
	if (ma_pCfgMgr.get())
	{
		long nCurrSel = -1;
		// Get list of last used file extensions
		string strExt = ma_pCfgMgr->getLastUsedFileExtension();

		// Get get default extensions list with history
		vector<string> vecExtList = ma_pCfgMgr->getFileExtensionList();
		for (unsigned int n = 0; n < vecExtList.size(); n++)
		{
			// Add extensions to drop down list
			m_cmbFileExtension.AddString(vecExtList[n].c_str());
			if(strExt == vecExtList[n])
			{
				// Save the current selection
				nCurrSel = n;
			}
		}

		if (nCurrSel >= 0)
		{
			// select the first item
			m_cmbFileExtension.SetCurSel(nCurrSel);
		}
		else if (vecExtList.size() > 0)
		{
			// select the first item
			m_cmbFileExtension.SetCurSel(0);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFolderFSPP::setFolderHistory()
{
		if (ma_pCfgMgr.get())
	{
		vector<string> vecFolderList;
		// Get folder history from registry
		ma_pCfgMgr->getOpenedFolderHistoryFromScope(vecFolderList);
		for (unsigned int n = 0; n < vecFolderList.size(); n++)
		{
			// Add to history list
			m_cmbFolder.AddString(vecFolderList[n].c_str());
		}

		if (vecFolderList.size() > 0)
		{
			// select the first item
			m_cmbFolder.SetCurSel(0);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFolderFSPP::saveFolderHistory()
{
	vector<string> vecFolders;
	for(int i = 0; i < (m_cmbFolder.GetCount()) && (i < iMAX_FOLDER_HISTORY_SIZE); i++)
	{
		CString zFolder;
		// Get item from Combo box
		m_cmbFolder.GetLBText(i, zFolder);
		string strTmp = zFolder;
		// save on vector to save to registry
		vecFolders.push_back(strTmp);
	}
	ma_pCfgMgr->setOpenedFolderHistoryFromScope(vecFolders);
}
//-------------------------------------------------------------------------------------------------
void CFolderFSPP::saveFileExtensionHistory()
{
	vector<string> vecExt;
	for(int i = 0; i < m_cmbFileExtension.GetCount(); i++)
	{
		CString zExt;
		// Get item from Combo box
		m_cmbFileExtension.GetLBText(i, zExt);
		string strTmp = zExt;
		// save on vector to save to registry
		vecExt.push_back(strTmp);
	}
	ma_pCfgMgr->setFileExtensionList(vecExt);
}
//-------------------------------------------------------------------------------------------------
void CFolderFSPP::pushCurrentFolderToHistory()
{

	CString zFolder("");
	// Get the current edit text in combo box
	m_cmbFolder.GetWindowText(zFolder);
	if(zFolder == "")
	{
		return;
	}
	// Find current text in list
	long nAlreadyInList = m_cmbFolder.FindStringExact(-1, zFolder);
	if(nAlreadyInList >= 0)
	{
		// remove from list 
		m_cmbFolder.DeleteString(nAlreadyInList);
	}
	else
	{
		// Remove strings if history list exceeds max size
		while(m_cmbFolder.GetCount() >= iMAX_FOLDER_HISTORY_SIZE)
		{
			m_cmbFolder.DeleteString(m_cmbFolder.GetCount() - 1);
		}
	}
	// Add current text to position 0 of list
	m_cmbFolder.InsertString(0, zFolder);
	m_cmbFolder.SetCurSel(0);

	// Save to registry
	saveFolderHistory();
}
//-------------------------------------------------------------------------------------------------
void CFolderFSPP::pushCurrentFileExtensionToHistory()
{
	CString zExt("");
	// Get current edit text in combo box
	m_cmbFileExtension.GetWindowText(zExt);
	if(zExt == "")
	{
		return;
	}
	string strExt = zExt;
	// Save text to registry as last used
	ma_pCfgMgr->setLastUsedFileExtension(strExt);

	// Find text in combo box
	long nAlreadyInList = m_cmbFileExtension.FindStringExact(-1, zExt);
	if(nAlreadyInList >= 0)
	{
		// Make it the current selection
		m_cmbFileExtension.SetCurSel(nAlreadyInList);
		return;
	}
	// not in list so put it at position 0
	m_cmbFileExtension.InsertString(0, zExt);
	//  make it the current selection
	m_cmbFileExtension.SetCurSel(0);

	// Save to registry
	saveFileExtensionHistory();
}
//-------------------------------------------------------------------------------------------------
bool CFolderFSPP::saveFolderFS(EXTRACT_FILESUPPLIERSLib::IFolderFSPtr ipFileSup)
{
	try
	{
		// Get Folder name
		CString zFolder;
		m_cmbFolder.GetWindowTextA(zFolder);

		// Trim the end '\' or '/' if exist
		zFolder.TrimRight(_T("\\/"));

		// Set the folder name
		ipFileSup->FolderName = zFolder.operator LPCSTR();
		
		// Get file extenstions
		CString zFileExtensions;
		m_cmbFileExtension.GetWindowTextA(zFileExtensions);
		ipFileSup->FileExtensions = zFileExtensions.operator LPCSTR();

		// Get Recursive check state
		bool bChkState;
		bChkState = m_chkRecursive.GetCheck() == BST_CHECKED;
		ipFileSup->RecurseFolders = bChkState ? VARIANT_TRUE : VARIANT_FALSE;

		// Get Continuous Check state
		bool bContinuousState;
		bContinuousState = m_chkContinuousProcess.GetCheck() == BST_CHECKED;

		// Used to make sure at least one of Added, Modified or Target for rename or move are checked
		int nNumChecks = 0;

		// Get Added state
		bChkState = m_chkAdded.GetCheck() == BST_CHECKED;
		ipFileSup->AddedFiles = bChkState ? VARIANT_TRUE : VARIANT_FALSE;

		// if checked add to number of checked
		if ( bChkState )
		{
			nNumChecks++;
		}
		// get Modified state
		bChkState = m_chkModified.GetCheck() == BST_CHECKED;
		ipFileSup->ModifiedFiles = bChkState ? VARIANT_TRUE : VARIANT_FALSE;

		// if checked add to number of checked
		if ( bChkState )
		{
			nNumChecks++;
		}
		// Get Target for rename or move state
		bChkState = m_chkTargetForRenameOrMove.GetCheck() == BST_CHECKED;
		ipFileSup->TargetOfMoveOrRename = bChkState ? VARIANT_TRUE : VARIANT_FALSE;

		// if checked add to number of checked
		if ( bChkState )
		{
			nNumChecks++;
		}

		// if Continous is checked and none of Added, Modified or Target for rename or move are checked throw and exception
		if ( bContinuousState && nNumChecks < 1 )
		{
			m_chkContinuousProcess.SetFocus();
			UCLIDException ue("ELI14446", "Must select type of files to continuously process." );
			throw ue;
		}
		// Get No existing files state
		bChkState = m_chkNoExistingFiles.GetCheck() == BST_CHECKED;
		ipFileSup->NoExistingFiles = bChkState ? VARIANT_TRUE : VARIANT_FALSE;	

		// Save the current folder to history
		pushCurrentFolderToHistory();
		
		// Save the Current extensions to history
		pushCurrentFileExtensionToHistory();

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14445");

	return false;
}
//-------------------------------------------------------------------------------------------------
