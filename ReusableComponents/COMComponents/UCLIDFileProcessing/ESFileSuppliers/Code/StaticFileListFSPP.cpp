// StaticFileListFSPP.cpp : Implementation of CStaticFileListFSPP
#include "stdafx.h"
#include "StaticFileListFSPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <misc.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>

#include <Mmc.h>
#include <io.h>
#include <fstream>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int MAX_MULTI_SELECT_FILE_BUF_SIZE = 32768;

//-------------------------------------------------------------------------------------------------
// CStaticFileListFSPP
//-------------------------------------------------------------------------------------------------
CStaticFileListFSPP::CStaticFileListFSPP()
{
	try
	{
		// Check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13722")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFSPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CStaticFileListFSPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CStaticFileListFSPP::Apply\n"));
		SetDirty(FALSE);

		EXTRACT_FILESUPPLIERSLib::IStaticFileListFSPtr ipFileSup = m_ppUnk[0];
		if( ipFileSup )
		{	
			ipFileSup->FileList = m_vecFileList;
		}
		else
		{
			return S_FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13721")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CStaticFileListFSPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FILESUPPLIERSLib::IStaticFileListFSPtr ipFileSup = m_ppUnk[0];
		if (ipFileSup)
		{
			//Attach all the member variables to the DLG items they represent
			m_FileList = GetDlgItem(IDC_FILE_LIST);
			m_btnAddFile = GetDlgItem(IDC_BTN_ADD);
			m_btnRemoveFile = GetDlgItem(IDC_BTN_REMOVE);
			m_btnClearList = GetDlgItem(IDC_BTN_CLEAR);
			m_btnAddList = GetDlgItem(IDC_BTN_LOAD_LIST);

			//Set to enable full row selections and gridlines
			//TODO: CAUSES WARNING as is. Seemingly equivalent to other areas of code,
			//      should be verified asap.
			m_FileList.SetExtendedListViewStyle(LVS_EX_FULLROWSELECT);

			//Insert a column into the ListCtrl
			CRect rect;
			m_FileList.GetClientRect(&rect);
			m_FileList.InsertColumn( 0, "", LVCFMT_LEFT, rect.Width(), 0 );

			//Get the IVariantVector from the StaticFileListFS Object.
			IVariantVectorPtr ivVec = ipFileSup->FileList;
			m_vecFileList = ivVec;

			_variant_t varType;
			_bstr_t bstrType;
			string strTemp;

			//Then put each item into m_FileList
			long lSize = ivVec->GetSize();
			for( long i = 0; i < lSize; i++)
			{
				varType = ivVec->GetItem(i);
				bstrType = varType.bstrVal;
				strTemp = bstrType;

				m_FileList.AddItem(i, 0, strTemp.c_str());
			}
		}
		updateButtons();
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13723");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStaticFileListFSPP::OnBnClickedBtnAdd(WORD /*wNotifyCode*/, WORD /*wID*/, 
										   HWND hWndCtl, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strTypes = "All Files (*.*)|*.*||";

		// bring open file dialog
		string strFileExtension( s_strTypes );

		CFileDialog fileDlg(TRUE, NULL, NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_ALLOWMULTISELECT ,
			strFileExtension.c_str(), CWnd::FromHandle(m_hWnd));

		// We need to create a large buffer so that multiple files may be selected
		TCHAR buf[MAX_MULTI_SELECT_FILE_BUF_SIZE];
		memset(buf, 0, MAX_MULTI_SELECT_FILE_BUF_SIZE * sizeof(TCHAR));

		fileDlg.m_ofn.lpstrFile = &buf[0];
		fileDlg.m_ofn.nMaxFile = MAX_MULTI_SELECT_FILE_BUF_SIZE;

		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);
	
		// if the user clicked on OK and the return file name is not empty,
		// then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			// Get an iterator for the file(s) that were chosen
			POSITION pos = fileDlg.GetStartPosition();
			std::string strFile = "";
			while(pos != __nullptr)
			{
				//Get the pathname(s)
				strFile = fileDlg.GetNextPathName(pos);

				// Add the file to the listbox
				addFile(strFile);
			}
		}
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13725");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStaticFileListFSPP::OnBnClickedBtnClear(WORD /*wNotifyCode*/, WORD /*wID*/, 
											 HWND hWndCtl, BOOL& /*bHandled*/)
{
	if(clear())
	{
		updateButtons();
	}
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStaticFileListFSPP::OnBnClickedBtnLoadList(WORD /*wNotifyCode*/, WORD /*wID*/, 
												HWND hWndCtl, BOOL& /*bHandled*/)
{
	try
	{
		if(m_FileList.GetItemCount() )
		{
			//If there are items in the list, clear them
			if( clear() )
			{
				//if the user clicks ok to delete all items, add new ones
				addListOfFiles();
			}
		}
		else
		{
			//for an empty list, add the new files to the list
			addListOfFiles();
		}
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13726");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStaticFileListFSPP::OnBnClickedBtnRemove(WORD /*wNotifyCode*/, WORD /*wID*/, 
											  HWND hWndCtl, BOOL& /*bHandled*/)
{
	try
	{
		int nItem = m_FileList.GetNextItem(-1, LVNI_ALL|LVNI_SELECTED);
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item(s)?", "Confirm Delete", MB_YESNO);
			if (nRes == IDYES)
			{
				// remove selected items
				int nFirstItem = nItem;
				while(nItem != -1)
				{
					// remove from the UI listbox
					m_FileList.DeleteItem(nItem);
					m_vecFileList->Remove(nItem, 1);
					nItem = m_FileList.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = m_FileList.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_FileList.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					m_FileList.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
			}
		}
		CRect rect;
		m_FileList.GetClientRect(&rect);	

		// adjust the column width in case there is a vertical scrollbar now
		m_FileList.SetColumnWidth(0, MMCLV_AUTO);

		updateButtons();

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13716")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStaticFileListFSPP::OnNMClickFileList(int /*idCtrl*/, LPNMHDR pNMHDR, BOOL& /*bHandled*/)
{
	updateButtons();
	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CStaticFileListFSPP::clear()
{	
	try
	{
		int nRes = MessageBox("Delete all item(s)?", "Confirm Delete", MB_YESNO);

		if (nRes == IDYES)
		{
			m_FileList.DeleteAllItems();
			m_vecFileList->Clear();

			return true;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13703");
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CStaticFileListFSPP::addFile(std::string strFileName)
{
	//Make sure the filename is not null
	if(strFileName != "")
	{
		// P13 #4468 trim trailing spaces from file name
		// this is fair to do since the OS will not allow the creation of a file name
		// with trailing spaces, it will truncate them
		strFileName = trim(strFileName, "", " ");
		
		// Verify that the file exists
		if (!isFileOrFolderValid(strFileName))
		{
			// Log an exception
			UCLIDException ue("ELI15698", "Application trace: A File in the loading list does not exist!");
			ue.addDebugInfo("Missing File", strFileName);
			ue.log();

			// Notify the user with a message box that the file DNE
			// and let the users choose if they want to ignore this file
			// or stop loading the list.
			string strPrompt = "File \"" + strFileName + "\" can not be found. \nChoose \"Yes\" to skip this file " +
				"or \"No\" to stop loading the list.";
			int iResult = MessageBox(strPrompt.c_str(), "File Not Found - Continue Loading List?", 
				MB_ICONINFORMATION | MB_YESNO);

			if (iResult == IDNO)
			{
				// Return false if user choose "No" to 
				// stop loading
				return false;
			}
		}
		else
		{
			//Add the item to the list				
			m_FileList.AddItem(m_FileList.GetItemCount(), 0, strFileName.c_str());
			
			//put the filename in the vector
			m_vecFileList->PushBack( get_bstr_t( strFileName ) );
		}
	}
	else
	{
		UCLIDException ue("ELI13626", "Attempt to add a null string to the list!");
		throw ue;
	}
	updateButtons();

	// Return true to continue loading the files
	return true;
}
//-------------------------------------------------------------------------------------------------
void CStaticFileListFSPP::addListOfFiles()
{
	const static string s_strTypes = "Text files (*.txt)|*.txt|All Files (*.*)|*.*||";

	// bring open file dialog
	string strFileExtension( s_strTypes );

	CFileDialog fileDlg(TRUE, NULL, "", 
		OFN_HIDEREADONLY, strFileExtension.c_str(), CWnd::FromHandle(m_hWnd));
	
	// Pass the pointer of dialog to create ThreadDataStruct object
	ThreadFileDlg tfd(&fileDlg);

	// if the user clicked on OK and the return file name is not empty,
	// then pull all the filenames (1 per line) from the file.
	if (tfd.doModal() == IDOK)
	{
		// get the file name as a std::string
		std::string strFileName((LPCTSTR)fileDlg.GetPathName());

		//create a stream
		ifstream inFile;

		//open the file
		inFile.open(strFileName.c_str(), ios::in);

		//verify that it opened
		if(! inFile.is_open() )
		{
			//if it doesnt open, toss a UE.
			UCLIDException ue("ELI13644", "Error opening the specified file list!");
			ue.addDebugInfo("Filename: ", strFileName);
			throw ue;
		}
		else if(inFile.good() )
		{
			vector<string> vecFileList;
			getFileListFromFile( strFileName, vecFileList );

			for each ( string strFile in vecFileList )
			{
				// Add the file to the list and the vector
				// If there is invalid file in the list, the user can choose to
				// stop loading the list inside addFile(), and the return value will be set to false, 
				// which causes this to break from the for each loop
				if (!addFile(strFile))
				{
					break;
				}

				// This page is now dirty as a file has been added
				m_bDirty = true;
			}
		}
		else
		{
			UCLIDException ue("ELI13702", "Error reading from the specified file list!");
			ue.addDebugInfo("Filename: ", strFileName);
			throw ue;
		}
	}
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CStaticFileListFSPP::validateLicense()
{
	static const unsigned long STATIC_FILE_LIST_FSPP = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( STATIC_FILE_LIST_FSPP , "ELI13724", "Static File List FSPP" );
}
//-------------------------------------------------------------------------------------------------
void CStaticFileListFSPP::updateButtons()
{
	int iNumSel = m_FileList.GetSelectedCount();

	//If there is at least 1 thing selected enable delete button. 
	if( iNumSel > 0 )
	{
		m_btnRemoveFile.EnableWindow(TRUE);
	}
	else
	{
		m_btnRemoveFile.EnableWindow(FALSE);
	}
	
	//If there is at least 1 item in the list, enable clear button.
	if(m_vecFileList->GetSize() > 0 )
	{
		m_btnClearList.EnableWindow(TRUE);
	}
	else
	{
		m_btnClearList.EnableWindow(FALSE);
	}

	// Adjust the column width automatically to the biggest entry in the list
	m_FileList.SetColumnWidth(0, MMCLV_AUTO);
	
	
}
//-------------------------------------------------------------------------------------------------
