// ManageAttributeSets.cpp : implementation file
//

#include "stdafx.h"
#include "FAMDBAdmin.h"
#include "ManageAttributeSets.h"
#include "afxdialogex.h"
#include "cpputil.h"
#include <COMUtils.h>

#include <UCLIDException.h>
#include <SuspendWindowUpdates.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giATTRIBUTE_SETS_COLUMN = 0;

//-------------------------------------------------------------------------------------------------
// CManageAttributeSets dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CManageAttributeSets, CDialogEx)

CManageAttributeSets::CManageAttributeSets(const IFileProcessingDBPtr& ipFAMDB, CWnd* pParent /*=NULL*/)
	: CDialogEx(CManageAttributeSets::IDD, pParent)
{
	try
	{
		m_ipAttributeDBMgr.CreateInstance(CLSID_AttributeDBMgr);
		ASSERT_RESOURCE_ALLOCATION("ELI38675", m_ipAttributeDBMgr != __nullptr);

		m_ipAttributeDBMgr->FAMDB = ipFAMDB;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38635");
}
//-------------------------------------------------------------------------------------------------
CManageAttributeSets::~CManageAttributeSets()
{
	try
	{
		m_ipAttributeDBMgr = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI38636");
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_ATTRIBUTE_SETS_TO_MANAGE, m_listAttributeSets);
	DDX_Control(pDX, IDC_BTN_ADD_ATTRIBUTE_SET, m_btnAdd);
	DDX_Control(pDX, IDC_BTN_RENAME_ATTRIBUTE_SET, m_btnRename);
	DDX_Control(pDX, IDC_BTN_REMOVE_ATTRIBUTE_SET, m_btnRemove);
	DDX_Control(pDX, IDC_BTN_HISTORY_ATTRIBUTE_SETS, m_btnHistory);
	DDX_Control(pDX, IDC_BTN_REFRESH_ATTRIBUTE_SETS, m_btnRefresh);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageAttributeSets, CDialogEx)
	ON_BN_CLICKED(IDC_BTN_ATTRIBUTE_SET_CLOSE, &CManageAttributeSets::OnBtnClose)
	ON_BN_CLICKED(IDC_BTN_ADD_ATTRIBUTE_SET, &CManageAttributeSets::OnBnClickedBtnAddAttributeSet)
	ON_BN_CLICKED(IDC_BTN_RENAME_ATTRIBUTE_SET, &CManageAttributeSets::OnBnClickedBtnRenameAttributeSet)
	ON_BN_CLICKED(IDC_BTN_REFRESH_ATTRIBUTE_SETS, &CManageAttributeSets::OnBnClickedBtnRefreshAttributeSets)
	ON_BN_CLICKED(IDC_BTN_REMOVE_ATTRIBUTE_SET, &CManageAttributeSets::OnBnClickedBtnRemoveAttributeSet)
	ON_BN_CLICKED(IDC_BTN_HISTORY_ATTRIBUTE_SETS, &CManageAttributeSets::OnBnClickedBtnHistoryAttributeSets)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_ATTRIBUTE_SETS_TO_MANAGE, &CManageAttributeSets::OnLvnItemchangedListAttributeSetsToManage)
END_MESSAGE_MAP()


//-------------------------------------------------------------------------------------------------
// CManageAttributeSets message handlers
//-------------------------------------------------------------------------------------------------
BOOL CManageAttributeSets::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		configureAttributeSetsList();

		refreshAttributeSetsList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38637");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::OnBtnClose()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call dialog OnOK
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38634");
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::OnBnClickedBtnRefreshAttributeSets()
{
	try
	{
		refreshAttributeSetsList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38691");
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::OnBnClickedBtnRenameAttributeSet()
{
	try
	{
		// Check if there is no Attribute Set selected
		if ( m_listAttributeSets.GetSelectedCount() != 1 )
		{
			return;
		}

		// Get the selected Attribute Set to rename
		POSITION pos = m_listAttributeSets.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			int iIndex = m_listAttributeSets.GetNextSelectedItem( pos );

			string strOldName = m_listAttributeSets.GetItemText(iIndex, giATTRIBUTE_SETS_COLUMN);

			// Make sure the old name exists (could have been deleted or modified in different instance)
			if (!attributeSetExists(strOldName))
			{
				UCLIDException ue("ELI38692", "Attribute set does not exist!");
				ue.addDebugInfo("AttributeSet", strOldName);
				throw ue;
			}

			PromptDlg addDlg("Rename Attribute Set", "Attribute Set Name", strOldName.c_str());
			if ( addDlg.DoModal() == IDOK)
			{
				string strNewName = (LPCSTR)addDlg.m_zInput;
				if ( strNewName == strOldName)
				{
					return;
				}

				// check if new name exists
				if (attributeSetExists(strNewName))
				{
					UCLIDException ue ("ELI38689", "Attribute Set already exists");
					ue.addDebugInfo("AttributeSetName", strNewName);
					ue.addDebugInfo("Old name", strOldName);
					throw ue;
				}
				m_ipAttributeDBMgr->RenameAttributeSetName(strOldName.c_str(), strNewName.c_str());
			}
			refreshAttributeSetsList();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38690");
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::OnBnClickedBtnAddAttributeSet()
{
	try
	{
		PromptDlg addDlg("Add Attribute Set", "Attribute Set Name");
		if ( addDlg.DoModal() == IDOK)
		{
			// Need to make sure that the name doesn't already exist
			if (attributeSetExists((LPCSTR)addDlg.m_zInput))
			{
				UCLIDException ue ("ELI38687", "Attribute Set already exists");
				ue.addDebugInfo("AttributeSetName", (LPCSTR)addDlg.m_zInput);
				throw ue;
			}
			m_ipAttributeDBMgr->CreateNewAttributeSetName((LPCSTR)addDlg.m_zInput);
		}
		refreshAttributeSetsList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38685");
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::OnBnClickedBtnRemoveAttributeSet()
{
	// TODO: Add ability to only delete spatial info after giving user a choice.
	try
	{
		// Check if there is no Attribute Set selected
		if ( m_listAttributeSets.GetSelectedCount() == 0 )
		{
			return;
		}

		// Display the wait cursor while attribute set is deleted
		CWaitCursor cursor;

		// Get the selected Attribute Set(s) to remove
		POSITION pos = m_listAttributeSets.GetFirstSelectedItemPosition();

		if (pos != __nullptr)
		{
			// Prompt to verify
			if (MessageBox("Remove the selected attribute set(s)?", "Confirmation", MB_YESNOCANCEL) != IDYES)
			{
				return;
			}

			string strAttributeSetsDeleted = "";

			// Display the wait cursor while attribute sets are deleted
			CWaitCursor cursor;

			// Get index of first selection
			int iIndex = m_listAttributeSets.GetNextSelectedItem( pos );

			// Remove the attribute sets
			while (iIndex != -1)
			{
				// Get the attribute sets to remove
				string strCurrSelection = m_listAttributeSets.GetItemText(iIndex, giATTRIBUTE_SETS_COLUMN);

				// Catch any exceptions for each attribute set being deleted.
				try
				{
					// Delete the attribute set
					m_ipAttributeDBMgr->DeleteAttributeSetName(strCurrSelection.c_str());

					// Add the attribute set to string for debug
					if (!strAttributeSetsDeleted.empty())
					{
						strAttributeSetsDeleted += ", ";
					}
					strAttributeSetsDeleted += strCurrSelection;
				}
				CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI41633");

				// Get index of next selection
				iIndex = m_listAttributeSets.GetNextSelectedItem( pos );
			}

			// Add application trace whenever a database modification is made
			// [LRCAU #5052 - JDS - 12/18/2008]
			// https://extract.atlassian.net/browse/ISSUE-10496
			UCLIDException uex("ELI41634", "Application trace: Database change");
			uex.addDebugInfo("Change", "Remove attribute set(s)");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipAttributeDBMgr->FAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipAttributeDBMgr->FAMDB->DatabaseName));
			uex.addDebugInfo("Attribute set(s) removed", strAttributeSetsDeleted);
			uex.log();
		}
		refreshAttributeSetsList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38695");
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::OnBnClickedBtnHistoryAttributeSets()
{
	try
	{
		MessageBox("Not Implemented", "History", MB_OK | MB_ICONINFORMATION);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38696");
	
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::OnLvnItemchangedListAttributeSetsToManage(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	*pResult = 0;

	// Update the controls to reflect the current number of selected items
	updateControls();
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::configureAttributeSetsList()
{
	m_listAttributeSets.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);

	// Build column information struct
	LVCOLUMN lvColumn;
	lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
	lvColumn.fmt = LVCFMT_CENTER;

	// Get dimensions of list control
	CRect recList;
	m_listAttributeSets.GetClientRect(&recList);

	// Set information for Attribute Sets column
	lvColumn.pszText = "Attribute Sets";
	lvColumn.cx = recList.Width() ;

	m_listAttributeSets.InsertColumn(giATTRIBUTE_SETS_COLUMN, &lvColumn);
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::updateControls()
{
	try
	{
		// Get the selection count
		unsigned int iCount = m_listAttributeSets.GetSelectedCount();

		// Allow removing multiple attribute sets
		m_btnRemove.EnableWindow(asMFCBool(iCount > 0));

		BOOL bEnableOneSelected = asMFCBool(iCount == 1);

		// Enable only if one item is selected
		m_btnRename.EnableWindow(bEnableOneSelected);
		m_btnHistory.EnableWindow(bEnableOneSelected);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38671");
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::refreshAttributeSetsList()
{
	try
	{
		SuspendWindowUpdates updater(*this);

		// Store the position of the first selected item
		int nSelectedItem = 0;
		POSITION pos = m_listAttributeSets.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			nSelectedItem = m_listAttributeSets.GetNextSelectedItem(pos);
		}
		
		clearListSelection();

		// Clear the current Attribute sets list
		m_listAttributeSets.DeleteAllItems();

		IStrToStrMapPtr ipAttributeSets = m_ipAttributeDBMgr->GetAllAttributeSetNames();
		ASSERT_RESOURCE_ALLOCATION("ELI38673", ipAttributeSets != __nullptr);

		// Get the number of Attribute Sets
		long lSize = ipAttributeSets->Size;

		// Populate the list
		for (long i=0; i < lSize; i++)
		{
			// Get the AttributeSet ID and name
			_bstr_t bstrAttributeSetName;
			_bstr_t bstrAttributeSetID;
			ipAttributeSets->GetKeyValue(i, bstrAttributeSetName.GetAddress(), bstrAttributeSetID.GetAddress());

			// Add the info into the list
			m_listAttributeSets.InsertItem(i, (const char*)bstrAttributeSetName);
			DWORD dwID = asLong((const char*)bstrAttributeSetID);
			m_listAttributeSets.SetItemData(i, dwID);
		}

		// Select either the last selected item position or select the first item
		m_listAttributeSets.SetItemState(nSelectedItem < lSize ? nSelectedItem : 0,
			LVIS_SELECTED | LVIS_FOCUSED, LVIS_SELECTED | LVIS_FOCUSED);

		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38674");
}
//-------------------------------------------------------------------------------------------------
void CManageAttributeSets::clearListSelection()
{
	try
	{
		// Get the position of the currently selected Attribute Set
		POSITION pos = m_listAttributeSets.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// no item selected, return
			return;
		}

		// iterate through all selections and set them as unselected.  pos will be
		// set to NULL by GetNextSelectedItem if there are no more selected items
		while (pos)
		{
			int nItemSelected = m_listAttributeSets.GetNextSelectedItem(pos);
			m_listAttributeSets.SetItemState(nItemSelected, 0, LVIS_SELECTED);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38672");
}
//-------------------------------------------------------------------------------------------------
bool CManageAttributeSets::attributeSetExists(const string &attributeName)
{
	// Need to make sure that the name doesn't already exist
	IStrToStrMapPtr ipAttributeSets = m_ipAttributeDBMgr->GetAllAttributeSetNames();
	ASSERT_RESOURCE_ALLOCATION("ELI38688", ipAttributeSets != __nullptr);

	return asCppBool(ipAttributeSets->Contains(attributeName.c_str()));
}
//-------------------------------------------------------------------------------------------------
