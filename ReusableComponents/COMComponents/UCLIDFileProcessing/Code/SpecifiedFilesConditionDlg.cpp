#include "stdafx.h"
#include "SpecifiedFilesConditionDlg.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// SpecifiedFilesConditionDlg
//-------------------------------------------------------------------------------------------------
SpecifiedFilesConditionDlg::SpecifiedFilesConditionDlg(
									const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB)
: CDialog(SpecifiedFilesConditionDlg::IDD)
, m_ipFAMDB(ipFAMDB)
, m_nLastSelectedFile(-1)
, m_bIgnoreNextFocus(false)
{
}
//-------------------------------------------------------------------------------------------------
SpecifiedFilesConditionDlg::SpecifiedFilesConditionDlg(
									const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
									const SpecifiedFilesCondition& settings)
: CDialog(SpecifiedFilesConditionDlg::IDD)
, m_ipFAMDB(ipFAMDB)
, m_settings(settings)
, m_nLastSelectedFile(-1)
, m_bIgnoreNextFocus(false)
{
}
//-------------------------------------------------------------------------------------------------
SpecifiedFilesConditionDlg::~SpecifiedFilesConditionDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37301");
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_FILENAMES, m_listFileNames);
	DDX_Control(pDX, IDC_RADIO_STATIC_LIST, m_btnStaticList);
	DDX_Control(pDX, IDC_RADIO_LIST_FILE, m_btnListFile);
	DDX_Control(pDX, IDC_EDIT_LIST_FILE_NAME, m_editListFileName);
	DDX_Control(pDX, IDC_BTN_ADD_FILE_NAME, m_btnAddFileName);
	DDX_Control(pDX, IDC_BTN_MODIFY_FILE_NAME, m_btnModifyFileName);
	DDX_Control(pDX, IDC_BTN_DELETE_FILE_NAME, m_btnDeleteFileName);
	DDX_Control(pDX, IDC_BTN_BROWSE_FILE_NAME, m_btnBrowseFileName);
	DDX_Control(pDX, IDC_BTN_BROWSE_LIST_FILE, m_btnListFileBrowse);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SpecifiedFilesConditionDlg, CDialog)
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &SpecifiedFilesConditionDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &SpecifiedFilesConditionDlg::OnClickedCancel)
	ON_BN_CLICKED(IDC_RADIO_STATIC_LIST, &SpecifiedFilesConditionDlg::OnClickedRadio)
	ON_BN_CLICKED(IDC_RADIO_LIST_FILE, &SpecifiedFilesConditionDlg::OnClickedRadio)
	ON_NOTIFY(NM_SETFOCUS, IDC_LIST_FILENAMES, &SpecifiedFilesConditionDlg::OnSetFocusSpecifiedFileNames)
	ON_NOTIFY(NM_CLICK, IDC_LIST_FILENAMES, &SpecifiedFilesConditionDlg::OnClickListFileNames)
	ON_NOTIFY(LVN_KEYDOWN, IDC_LIST_FILENAMES, &SpecifiedFilesConditionDlg::OnKeyDownListFileNames)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_FILENAMES, &SpecifiedFilesConditionDlg::OnItemChangedListFileNames)
	ON_NOTIFY(LVN_BEGINLABELEDIT, IDC_LIST_FILENAMES, &SpecifiedFilesConditionDlg::OnBeginLabelEditListFileNames)
	ON_NOTIFY(LVN_ENDLABELEDIT, IDC_LIST_FILENAMES, &SpecifiedFilesConditionDlg::OnEndLabelEditListFileNames)
	ON_BN_CLICKED(IDC_BTN_ADD_FILE_NAME, &SpecifiedFilesConditionDlg::OnClickedBtnAddFileName)
	ON_BN_CLICKED(IDC_BTN_MODIFY_FILE_NAME, &SpecifiedFilesConditionDlg::OnClickedBtnModifyFileName)
	ON_BN_CLICKED(IDC_BTN_DELETE_FILE_NAME, &SpecifiedFilesConditionDlg::OnClickedBtnDeleteFileName)
	ON_BN_CLICKED(IDC_BTN_BROWSE_FILE_NAME, &SpecifiedFilesConditionDlg::OnClickedBtnBrowseFileName)
	ON_BN_CLICKED(IDC_BTN_BROWSE_LIST_FILE, &SpecifiedFilesConditionDlg::OnClickedBtnBrowseListFile)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// SpecifiedFilesConditionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL SpecifiedFilesConditionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		ListView_SetExtendedListViewStyle(m_listFileNames.m_hWnd, LVS_EX_GRIDLINES);
		m_listFileNames.InsertColumn(0, "");

		// Read the settings object and set the dialog based on the settings
		setControlsFromSettings();

		updateControlStates();
		updateGridWidth();

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37302")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnOK()
{
	// Allow the user to use the enter key to close the dialog
	OnClickedOK();
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37303");
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickedCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37304");
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickedOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Save the settings
		if (saveSettings())
		{
			// If settings saved successfully, close the dialog
			CDialog::OnOK();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37305");
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickedRadio()
{
	try
	{
		updateControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37306");
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnSetFocusSpecifiedFileNames(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		// In order to prevent having to click on the file list control multiple times, enter edit
		// mode as soon as it gains focus via a click except in special cases where this value is
		// set to true.
		if (!m_bIgnoreNextFocus && (GetKeyState(VK_LBUTTON) & 0x80) != 0)
		{
			// Get the item corresponding to the location of the click.
			CPoint point;
			GetCursorPos(&point);
			m_listFileNames.ScreenToClient(&point);
			UINT uiFlags;
			int	nItemIndex = m_listFileNames.HitTest(point, &uiFlags);
	
			if (nItemIndex == -1)
			{
				// If the user clicked outside of an existing row, create a new row.
				nItemIndex = m_listFileNames.GetItemCount();
				m_listFileNames.InsertItem(nItemIndex, "");

				updateGridWidth();
			}

			m_listFileNames.EditLabel(nItemIndex);
		}

		m_bIgnoreNextFocus = false;

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37307")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickListFileNames(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{	
		LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
		int	nItemIndex = pNMItemActivate->iItem;

		if (nItemIndex == -1)
		{
			// If the user clicked outside of an existing row, create a new row.
			nItemIndex = m_listFileNames.GetItemCount();
			m_listFileNames.InsertItem(nItemIndex, "");

			updateGridWidth();
		}

		m_listFileNames.EditLabel(nItemIndex);

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37308")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnKeyDownListFileNames(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		LPNMLVKEYDOWN pLVKeyDown = reinterpret_cast<LPNMLVKEYDOWN>(pNMHDR);
		if (pLVKeyDown->wVKey == VK_DELETE)
		{
			int nItem = getSelectedItem();
			if (nItem != -1)
			{
				m_listFileNames.DeleteItem(nItem);
			}
		}
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37309")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnItemChangedListFileNames(NMHDR *pNMHDR, LRESULT *pResult)// If the user clicked outside of an existing row, create a new row.
{
	try
	{
		LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);

		if (pNMLV->iItem != m_nLastSelectedFile)
		{
			// If the last selected item is no longer seleted, disable the edit/modify buttons until
			// edit mode is activated. The last selected item may be deleted if empty so we don't
			// yet know if the new selection will remain and, therefore, whether the modify and
			// delete buttons should be enabled.
			m_btnModifyFileName.EnableWindow(FALSE);
			m_btnDeleteFileName.EnableWindow(FALSE);

			m_nLastSelectedFile = pNMLV->iItem;
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37310")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnBeginLabelEditListFileNames(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		// If entering edit mode, this is not a transient item selection and the modify/delete
		// buttons should be active.
		m_btnModifyFileName.EnableWindow(TRUE);
		m_btnDeleteFileName.EnableWindow(TRUE);

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37311")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnEndLabelEditListFileNames(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{	
		NMLVDISPINFO *pItemInfo = reinterpret_cast<NMLVDISPINFO*>(pNMHDR);

		if (getSelectedItem() == -1)
		{
			// If the last selected item is no longer seleted, disable the edit/modify buttons until
			// edit mode is activated. The last selected item may be deleted if empty so we don't
			// yet know if the new selection will remain and, therefore, whether the modify and
			// delete buttons should be enabled.
			m_btnModifyFileName.EnableWindow(FALSE);
			m_btnDeleteFileName.EnableWindow(FALSE);
		}


		// If edit mode is ending because the user has clicked outside of the current label, the
		// list control will be receiving a focus event as a result. Do not allow this click to
		// cause another edit mode to be activated.
		if ((GetKeyState(VK_LBUTTON) & 0x80) != 0)
		{
			m_bIgnoreNextFocus = true;
		}

		// If no edit took place, pItemInfo->item.pszText will be null despite the fact that
		// the label may have text. Set strItemText from the current item if necessary.
		bool bUpdated = (pItemInfo->item.pszText != __nullptr);
		string strItemText = bUpdated
			? pItemInfo->item.pszText
			: m_listFileNames.GetItemText(pItemInfo->item.iItem, 0);

		// Delete any labels whose text has been cleared or that never were given text.
		if (!containsNonWhitespaceChars(strItemText))
		{
			m_listFileNames.DeleteItem(pItemInfo->item.iItem);
			
			updateGridWidth();
		}
		// Otherwise, apply the edited text back to the list control if it was updated.
		else if (bUpdated)
		{
			m_listFileNames.SetItemText(pItemInfo->item.iItem, 0, pItemInfo->item.pszText);
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37312")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickedBtnAddFileName()
{
	try
	{
		int nItemIndex = m_listFileNames.GetItemCount();
		m_listFileNames.InsertItem(nItemIndex, "");

		updateGridWidth();

		// Focus needs to be returned to m_listFileNames for EditLabel to work.
		m_listFileNames.SetFocus();
		m_listFileNames.EditLabel(nItemIndex);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37313")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickedBtnModifyFileName()
{
	try
	{
		int nItem = getSelectedItem();
		if (nItem != -1)
		{
			// Focus needs to be returned to m_listFileNames for EditLabel to work.
			m_listFileNames.SetFocus();
			m_listFileNames.EditLabel(nItem);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37314")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickedBtnDeleteFileName()
{
	try
	{
		int nItem = getSelectedItem();
		if (nItem != -1)
		{
			m_listFileNames.DeleteItem(nItem);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37315")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickedBtnBrowseFileName()
{
	try
	{
		// If there is no item currently seleted allow the browse button to create a new one.
		int nItem = getSelectedItem();
		if (nItem == -1)
		{
			nItem = m_listFileNames.GetItemCount();
			m_listFileNames.InsertItem(nItem, "");

			updateGridWidth();
		}

		CString zFileType("Image files|*.bmp;*.rle;*.dib;*.rst;*.gp4;*.mil;*.cal;*.cg4;*.flc;"
			"*.fli;*.gif;*.jpg;*.pcx;*.pct;*.png;*.tga;*.tif;*.tiff;*.pdf|All files (*.*)|*.*||");

		CFileDialog dlg(TRUE, "", NULL, OFN_FILEMUSTEXIST | OFN_NOCHANGEDIR, zFileType, this);
		if (dlg.DoModal() == IDOK)
		{
			m_listFileNames.SetItemText(nItem, 0, dlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37316")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::OnClickedBtnBrowseListFile()
{
	try
	{
		CString zFileType("Text files (*.txt)|*.txt|Dat Files (*.dat)|*.dat|All files (*.*)|*.*||");

		CFileDialog dlg(TRUE, ".txt", NULL, OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			zFileType, this);
		if (dlg.DoModal() == IDOK)
		{
			m_editListFileName.SetWindowText(dlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37317")
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::setControlsFromSettings()
{
	try
	{
		if (m_settings.getUseSpecifiedFiles())
		{
			m_btnStaticList.SetCheck(BST_CHECKED);
		}
		else if (m_settings.getUseListFile())
		{
			m_btnListFile.SetCheck(BST_CHECKED);
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI37318");
		}

		vector<string>& vecSpecifiedFiles = m_settings.getSpecifiedFiles();
		for (size_t i = 0; i < vecSpecifiedFiles.size(); i++)
		{
			m_listFileNames.InsertItem(i, vecSpecifiedFiles[i].c_str());
		}

		m_editListFileName.SetWindowText(m_settings.getListFileName().c_str());
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37319");
}
//-------------------------------------------------------------------------------------------------
bool SpecifiedFilesConditionDlg::saveSettings()
{
	try
	{
		if (m_btnStaticList.GetCheck() == BST_CHECKED)
		{
			m_settings.setUseSpecifiedFiles();
		}
		else if (m_btnListFile.GetCheck() == BST_CHECKED)
		{
			m_settings.setUseListFile();
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI37320");
		}

		vector<string> vecSpecifiedFiles;
		int nCount = m_listFileNames.GetItemCount();
		for (int i = 0; i < nCount; i++)
		{
			vecSpecifiedFiles.push_back((LPCTSTR)m_listFileNames.GetItemText(i, 0));
		}
		m_settings.setSpecifiedFiles(vecSpecifiedFiles);

		if (m_settings.getUseSpecifiedFiles() && vecSpecifiedFiles.empty())
		{
			MessageBox("You must specify at least one file.", "No File Specified",
				MB_OK | MB_ICONERROR);

			m_listFileNames.SetFocus();

			return false;
		}

		CString zListFileName;
		m_editListFileName.GetWindowText(zListFileName);
		m_settings.setListFileName((LPCTSTR)zListFileName);

		if (m_settings.getUseListFile() && !isValidFile(m_settings.getListFileName()))
		{
			MessageBox("You must specify a valid text file.", "File Not Found",
				MB_OK | MB_ICONERROR);

			m_editListFileName.SetFocus();

			return false;
		}

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37321")
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::updateControlStates()
{
	try
	{
		if (m_btnStaticList.GetCheck() == BST_CHECKED)
		{
			m_listFileNames.EnableWindow(TRUE);
			m_btnAddFileName.EnableWindow(TRUE);
			// m_btnModifyFileName and m_btnDeleteFileName will get enabled based on selection in
			// m_listFileNames
			m_btnBrowseFileName.EnableWindow(TRUE);
			m_editListFileName.EnableWindow(FALSE);
			m_btnListFileBrowse.EnableWindow(FALSE);
		}
		else if (m_btnListFile.GetCheck() == BST_CHECKED)
		{
			m_listFileNames.EnableWindow(FALSE);
			m_btnAddFileName.EnableWindow(FALSE);
			m_btnModifyFileName.EnableWindow(FALSE);
			m_btnDeleteFileName.EnableWindow(FALSE);
			m_btnBrowseFileName.EnableWindow(FALSE);
			m_editListFileName.EnableWindow(TRUE);
			m_btnListFileBrowse.EnableWindow(TRUE);
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI37322");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37323");
}
//-------------------------------------------------------------------------------------------------
void SpecifiedFilesConditionDlg::updateGridWidth()
{
	try
	{
		CRect rect;
		m_listFileNames.GetClientRect(&rect);
		m_listFileNames.SetColumnWidth(0, rect.Width());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37324");
}
//-------------------------------------------------------------------------------------------------
int SpecifiedFilesConditionDlg::getSelectedItem()
{
	POSITION pos = m_listFileNames.GetFirstSelectedItemPosition();
	if (pos != NULL)
	{
		return m_listFileNames.GetNextSelectedItem(pos);
	}

	return -1;
}
//-------------------------------------------------------------------------------------------------
