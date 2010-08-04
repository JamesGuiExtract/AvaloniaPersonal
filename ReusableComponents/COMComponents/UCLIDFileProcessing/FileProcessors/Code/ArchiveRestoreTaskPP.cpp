//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ArchiveRestoreTaskPP.cpp
//
// PURPOSE:	Implementation of the ArchiveRestoreTask property page
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include "stdafx.h"
#include "FileProcessors.h"
#include "ArchiveRestoreTaskPP.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <LoadFileDlgThread.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <XBrowseForFolder.h>
#include <Misc.h>
#include <ComUtils.h>
#include <StringCSIS.h>

#include <algorithm>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const static string gstrALL_FILES_FILTER = "All Files (*.*)|*.*||";

const static string gstrNOTE =
	"NOTE: The root archive folder specified below should be unique to each database\r\n"
	"because it uses a database-based file identifier to implement archive/restore\r\n"
	"functionality. Using the same archive folder across multiple databases will cause\r\n"
	"data from previous archive operations to be overwritten or for the wrong data to\r\n"
	"be restored.";

const static string gstrARCHIVED_TAG_LABEL = "Tag to associate with archived file";
const static string gstrARCHIVED_FILE_LABEL = "File to archive";
const static string gstrARCHIVED_GROUP_OVERWRITE_LABEL = "If the file to archive exists";
const static string gstrRESTORE_TAG_LABEL = "Tag of file to restore";
const static string gstrRESTORE_FILE_LABEL = "Location to restore file to";
const static string gstrRESTORE_GROUP_OVERWRITE_LABEL = "If the file to restore exists";

//--------------------------------------------------------------------------------------------------
// CArchiveRestoreTaskPP
//--------------------------------------------------------------------------------------------------
CArchiveRestoreTaskPP::CArchiveRestoreTaskPP() 
{
	try
	{
		// check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI24601");
}
//--------------------------------------------------------------------------------------------------
CArchiveRestoreTaskPP::~CArchiveRestoreTaskPP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24602");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTaskPP::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check parameter
		ASSERT_ARGUMENT("ELI24603", pbValue != NULL);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then the license is valid
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24604");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTaskPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
		
	try
	{
		// check licensing
		validateLicense();

		// save the settings to the ArchiveRestoreTask
		for(UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_FILEPROCESSORSLib::IArchiveRestoreTaskPtr ipArchiveRestore(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI24605", ipArchiveRestore != NULL);

			// Store the operation type
			ipArchiveRestore->Operation = (UCLID_FILEPROCESSORSLib::EArchiveRestoreOperationType)
				((m_radioArchive.GetCheck() == BST_CHECKED) ?
				kCMDOperationArchiveFile : kCMDOperationRestoreFile);

			// Store the root folder
			_bstr_t bstrArchiveFolder;
			m_editArchiveFolder.GetWindowText(bstrArchiveFolder.GetAddress());
			ipArchiveRestore->ArchiveFolder = bstrArchiveFolder;

			// Store the tag
			_bstr_t bstrTag;
			m_cmbFileTag.GetWindowText(bstrTag.GetAddress());
			ipArchiveRestore->FileTag = bstrTag;

			// Store the overwrite value
			ipArchiveRestore->AllowOverwrite =
				asVariantBool(m_radioOverwriteFile.GetCheck() == BST_CHECKED);

			// Store the file to archive value
			_bstr_t bstrFile;
			m_editSourceFile.GetWindowText(bstrFile.GetAddress());
			ipArchiveRestore->FileToArchive = bstrFile;

			// Store the delete value
			ipArchiveRestore->DeleteFileAfterArchive =
				asVariantBool(m_checkDeleteFile.GetCheck() == BST_CHECKED);

			// Update the tags file if it does not contain a tag [LRCAU #5242]
			string strArchiveFolder = asString(bstrArchiveFolder);
			if (strArchiveFolder.find_first_of("<$") == string::npos)
			{
				saveTagToTagsFile(asString(bstrTag), getTagFile(strArchiveFolder));
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24606");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// Message Handlers
//--------------------------------------------------------------------------------------------------
LRESULT CArchiveRestoreTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// get the ArchiveRestoreTask associated with this property page
		// NOTE: this assumes only one coclass is associated with this property page
		UCLID_FILEPROCESSORSLib::IArchiveRestoreTaskPtr ipArchiveRestoreTask(m_ppUnk[0]);
		ASSERT_RESOURCE_ALLOCATION("ELI24607", ipArchiveRestoreTask != NULL);

		// Set the NOTE text
		GetDlgItem(IDC_EDIT_ARCHIVE_NOTE).SetWindowText(gstrNOTE.c_str());

		// get the controls from the property page
		m_radioArchive = GetDlgItem(IDC_RADIO_ARCHIVE);
		m_radioRestore = GetDlgItem(IDC_RADIO_RESTORE);
		m_editArchiveFolder = GetDlgItem(IDC_EDIT_ARCHIVE_FOLDER);
		m_btnArchiveFolderBrowse = GetDlgItem(IDC_BTN_ARCHIVE_FOLDER_BROWSE);
		m_labelFileTag = GetDlgItem(IDC_ARCHIVE_TAG_LABEL);
		m_cmbFileTag = GetDlgItem(IDC_CMB_ARCHIVE_TAG);
		m_labelSourceFile = GetDlgItem(IDC_ARCHIVE_FILE_LABEL);
		m_editSourceFile = GetDlgItem(IDC_EDIT_ARCHIVE_FILE);
		m_btnSourceFileBrowse = GetDlgItem(IDC_BTN_ARCHIVE_FILE_BROWSE);
		m_checkDeleteFile = GetDlgItem(IDC_CHECK_ARCHIVE_DELETE);
		m_groupOverwriteOrFail = GetDlgItem(IDC_ARCHIVE_GROUP_OVERWRITE);
		m_radioOverwriteFile = GetDlgItem(IDC_RADIO_ARCHIVE_OVERWRITE);
		m_radioFailFile = GetDlgItem(IDC_RADIO_ARCHIVE_FAIL);

		// get the doc tag buttons
		m_btnArchiveFolderDocTags.SubclassDlgItem(IDC_BTN_ARCHIVE_FOLDER_DOC_TAG,
			CWnd::FromHandle(m_hWnd));
		m_btnSourceFileDoctTags.SubclassDlgItem(IDC_BTN_ARCHIVE_FILE_DOC_TAG,
			CWnd::FromHandle(m_hWnd));

		// set the icon for the doc tag buttons
		m_btnArchiveFolderDocTags.SetIcon(::LoadIcon(_Module.m_hInstResource, 
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_btnSourceFileDoctTags.SetIcon(::LoadIcon(_Module.m_hInstResource, 
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		
		// load the property page with the data from the ArchiveRestoreTask
		m_editArchiveFolder.SetWindowText(ipArchiveRestoreTask->ArchiveFolder);
		m_cmbFileTag.SetWindowText(ipArchiveRestoreTask->FileTag);
		m_editSourceFile.SetWindowText(ipArchiveRestoreTask->FileToArchive);

		// Set the checked states
		m_checkDeleteFile.SetCheck(asBSTChecked(ipArchiveRestoreTask->DeleteFileAfterArchive));

		// Set the appropriate task type radio button
		if (ipArchiveRestoreTask->Operation == kCMDOperationArchiveFile)
		{
			m_radioArchive.SetCheck(BST_CHECKED);
			m_radioRestore.SetCheck(BST_UNCHECKED);
		}
		else
		{
			m_radioArchive.SetCheck(BST_UNCHECKED);
			m_radioRestore.SetCheck(BST_CHECKED);
		}

		// Set the appropriate overwrite radio button
		if (ipArchiveRestoreTask->AllowOverwrite == VARIANT_TRUE)
		{
			m_radioOverwriteFile.SetCheck(BST_CHECKED);
			m_radioFailFile.SetCheck(BST_UNCHECKED);
		}
		else
		{
			m_radioOverwriteFile.SetCheck(BST_UNCHECKED);
			m_radioFailFile.SetCheck(BST_CHECKED);
		}

		updateWindowState();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24608");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CArchiveRestoreTaskPP::OnClickedBtnRadioOperation(WORD wNotifyCode, WORD wID,
													   HWND hWndCtl, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		updateWindowState();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24609");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CArchiveRestoreTaskPP::OnClickedBtnArchiveDocTag(WORD wNotifyCode, WORD wID,
													  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the position of the doc tag button
		RECT rect;
		m_btnArchiveFolderDocTags.GetWindowRect(&rect);

		// Display the doc tag menu without the SourceDocName [LRCAU #5242]
		string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top, false);

		// If the user selected a tag, add it to the corresponding edit control
		if (!strChoice.empty())
		{
			m_editArchiveFolder.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24610");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CArchiveRestoreTaskPP::OnClickedBtnArchiveBrowse(WORD wNotifyCode, WORD wID,
													  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Display the folder browser
		char pszPath[MAX_PATH + 1] = {0};
		if(XBrowseForFolder(m_hWnd, NULL, pszPath, sizeof(pszPath)))
		{
			// Ensure there is a path
			if (pszPath != "")
			{
				// Set the path in the UI
				m_editArchiveFolder.SetWindowText(pszPath);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24612");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CArchiveRestoreTaskPP::OnClickedBtnFileDocTag(WORD wNotifyCode, WORD wID,
													  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// get the position of the input image doc tag button
		RECT rect;
		m_btnSourceFileDoctTags.GetWindowRect(&rect);

		// display the doc tag menu and get the user's selection
		string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);

		// if the user selected a tag, add it to the input image filename edit control
		if (strChoice != "")
		{
			m_editSourceFile.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24613");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CArchiveRestoreTaskPP::OnClickedBtnFileBrowse(WORD wNotifyCode, WORD wID,
													  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// bring open file dialog
		CFileDialog fileDlg(TRUE, NULL, "",
			OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrALL_FILES_FILTER.c_str(), CWnd::FromHandle(m_hWnd));

		// Pass the pointer of dialog to create ThreadFileDlg object
		ThreadFileDlg tfd(&fileDlg);

		// If OK was clicked
		if (tfd.doModal() == IDOK)
		{
			// Set the source file name
			m_editSourceFile.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24614");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CArchiveRestoreTaskPP::OnEnChangeEditArchiveFolder(WORD wNotifyCode, WORD wID,
														   HWND hWndCtl, BOOL& bHandled)
{
	// Get the current directory from the control
	CString zDir;
	m_editArchiveFolder.GetWindowText(zDir);

	// Ensure there are at least 3 characters in the string
	// (i.e. 'C:\')
	if (zDir.GetLength() > 2)
	{
		// Build the path to the tag file
		string strTagFile = getTagFile(string(zDir));

		// Attempt to get the tags document
		loadTagsFromFile(strTagFile);
	}

	return 0;
}

//--------------------------------------------------------------------------------------------------
// Private functions
//--------------------------------------------------------------------------------------------------
void CArchiveRestoreTaskPP::validateLicense()
{
	VALIDATE_LICENSE(gnFILE_ACTION_MANAGER_OBJECTS, "ELI24615", "ArchiveRestoreTask Property Page");
}
//-------------------------------------------------------------------------------------------------
void CArchiveRestoreTaskPP::updateWindowState()
{
	// Check whether archive or restore
	bool bArchive = m_radioArchive.GetCheck() == BST_CHECKED;

	// Set the labels appropriately
	m_labelFileTag.SetWindowText(
		bArchive ? gstrARCHIVED_TAG_LABEL.c_str() : gstrRESTORE_TAG_LABEL.c_str());
	m_labelSourceFile.SetWindowText(
		bArchive ? gstrARCHIVED_FILE_LABEL.c_str() : gstrRESTORE_FILE_LABEL.c_str());
	m_groupOverwriteOrFail.SetWindowText(bArchive ?
		gstrARCHIVED_GROUP_OVERWRITE_LABEL.c_str() : gstrRESTORE_GROUP_OVERWRITE_LABEL.c_str());

	// Enable/disable the check box appropriately
	m_checkDeleteFile.EnableWindow(asMFCBool(bArchive));

	// Clear the check box if disabling(restoring)
	if (!bArchive)
	{
		m_checkDeleteFile.SetCheck(BST_UNCHECKED);
	}
}
//-------------------------------------------------------------------------------------------------
void CArchiveRestoreTaskPP::loadTagsFromFile(const string& strTagsFile)
{
	// Store the current text from the combo box
	CString zTemp;
	m_cmbFileTag.GetWindowText(zTemp);

	// Clear the combo box and drop down list
	m_cmbFileTag.ResetContent();

	// Add the current text back to the combo box
	m_cmbFileTag.SetWindowText(zTemp);

	// Check if this is a valid file and if so, load the tags
	if (isValidFile(strTagsFile))
	{
		// Get the tags from the file
		vector<string> vecTags = convertFileToLines(strTagsFile);

		// Sort the tags
		sort(vecTags.begin(), vecTags.end());

		// Add the strings to the drop down
		for(vector<string>::iterator it = vecTags.begin(); it != vecTags.end(); it++)
		{
			m_cmbFileTag.AddString(it->c_str());
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CArchiveRestoreTaskPP::saveTagToTagsFile(const string& strTag, const string& strTagsFile)
{
	// Create a vector to hold the tags to write to the tags file
	vector<string> vecTags;

	// First check if the tag file exists
	if (isValidFile(strTagsFile))
	{
		// Get tag as case insensitive string
		stringCSIS strcisTag(strTag, false);

		// Read the lines from the file
		vecTags = convertFileToLines(strTagsFile);

		// Check if the vector contains the specified tag (need to replace the
		// existing tag with the new casing)
		bool bFound = false;
		for (size_t i=0; i < vecTags.size(); i++)
		{
			// Check if the strings are equal
			if (strcisTag == vecTags[i])
			{
				// Set string to new casing of tag
				vecTags[i] = strTag;
				bFound = true;
				break;
			}
		}

		// If the tag wasn't found then add it to the list
		if (!bFound)
		{
			vecTags.push_back(strTag);
		}
	}
	// Tags file does not exist
	else
	{
		// Ensure the directory exists (create it if it doesn't)
		string strDirectory = getDirectoryFromFullPath(strTagsFile);
		if (!isValidFolder(strDirectory))
		{
			// Directory did not exist, create it
			createDirectory(strDirectory);
		}

		// Since the file did not exist, just add the new tag to the vector of tags
		vecTags.push_back(strTag);
	}

	// Write the tags to the file (overwrite the existing file)
	writeLinesToFile(vecTags, strTagsFile);
}
//-------------------------------------------------------------------------------------------------
string CArchiveRestoreTaskPP::getTagFile(const string& strArchiveDirectory)
{
	// Build the path to the tag file
	string strTagFile = strArchiveDirectory;

	// Look for ending '\'
	if (strTagFile[strTagFile.length()-1] != '\\')
	{
		strTagFile += "\\";
	}

	strTagFile += "Tags.dat";

	return strTagFile;
}
//-------------------------------------------------------------------------------------------------
