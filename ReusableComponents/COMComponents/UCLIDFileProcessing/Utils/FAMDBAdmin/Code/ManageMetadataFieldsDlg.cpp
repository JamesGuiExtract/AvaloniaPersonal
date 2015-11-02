// ManageMetadataFieldsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ManageMetadataFieldsDlg.h"

#include <SuspendWindowUpdates.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <PromptDlg.h>
#include <StringCSIS.h>

#include <vector>
#include <numeric>
#include <string>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giMETADATAFIELDNAME_COLUMN = 0;

//-------------------------------------------------------------------------------------------------
// CManageMetadataFieldsDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CManageMetadataFieldsDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CManageMetadataFieldsDlg::CManageMetadataFieldsDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr &ipFAMDB,
							   CWnd* pParent) :
CDialog(CManageMetadataFieldsDlg::IDD, pParent),
m_ipFAMDB(ipFAMDB)
{
	ASSERT_ARGUMENT("ELI37713", ipFAMDB != __nullptr);
}

CManageMetadataFieldsDlg::~CManageMetadataFieldsDlg()
{
}

void CManageMetadataFieldsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BTN_ADD_METADATA_FIELD, m_btnAdd);
	DDX_Control(pDX, IDC_BTN_REMOVE_METADATA_FIELD, m_btnRemove);
	DDX_Control(pDX, IDC_BTN_RENAME_METADATA_FIELD, m_btnRename);
	DDX_Control(pDX, IDC_BTN_REFRESH_METADATA_FIELDS, m_btnRefresh);
	DDX_Control(pDX, IDC_BTN_METADATA_FIELD_CLOSE, m_btnClose);
	DDX_Control(pDX, IDC_LIST_METADATA_FIELDS_TO_MANAGE, m_listMetadataFields);
}


BEGIN_MESSAGE_MAP(CManageMetadataFieldsDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_ADD_METADATA_FIELD, &CManageMetadataFieldsDlg::OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_REMOVE_METADATA_FIELD, &CManageMetadataFieldsDlg::OnBtnRemove)
	ON_BN_CLICKED(IDC_BTN_RENAME_METADATA_FIELD, &CManageMetadataFieldsDlg::OnBtnRename)
	ON_BN_CLICKED(IDC_BTN_REFRESH_METADATA_FIELDS, &CManageMetadataFieldsDlg::OnBtnRefresh)
	ON_BN_CLICKED(IDC_BTN_METADATA_FIELD_CLOSE, &CManageMetadataFieldsDlg::OnBtnClose)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_METADATA_FIELDS_TO_MANAGE, &CManageMetadataFieldsDlg::OnLvnItemchangedList)
	ON_NOTIFY(NM_RDBLCLK, IDC_LIST_METADATA_FIELDS_TO_MANAGE, &CManageMetadataFieldsDlg::OnNMRDblclkList)
END_MESSAGE_MAP()


// CManageMetadataFieldsDlg message handlers

void CManageMetadataFieldsDlg::OnBtnAdd()
{
AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Prompt for new metadata field
		PromptDlg dlgAddField("Add Metadata Field", "Metadata Field Name");
		if (dlgAddField.DoModal() == IDOK)
		{
			string strFieldToAdd = (LPCSTR)dlgAddField.m_zInput;

			// Add application trace about the database modification
			UCLIDException uex("ELI37647", "Application trace: Database change");
			uex.addDebugInfo("Change", "Add new metadata field");
			uex.addDebugInfo("Metadata Field Name", strFieldToAdd);
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));

			m_ipFAMDB->AddMetadataField(strFieldToAdd.c_str());

			// Log application trace exception
			uex.log();

			refreshMetadataFieldList();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37648");
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::OnBtnRemove()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// If there are no selected items or the user said no to delete then just return
		if (m_listMetadataFields.GetSelectedCount() == 0
			|| MessageBox("Remove the selected Metadata Field(s)?", "Remove Metadata Field",
			MB_YESNO | MB_ICONQUESTION) == IDNO)
		{
			// Nothing to do
			return;
		}

		// Display the wait cursor while the fields are deleted
		CWaitCursor cursor;

		// Build the list of fields to delete
		vector<string> vecFieldsToDelete;
		POSITION pos = m_listMetadataFields.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			int iIndex = m_listMetadataFields.GetNextSelectedItem( pos );

			// Loop through selected items and add each item to the vector
			while (iIndex != -1)
			{
				vecFieldsToDelete.push_back((LPCTSTR) m_listMetadataFields.GetItemText
					(iIndex, giMETADATAFIELDNAME_COLUMN));

				iIndex = m_listMetadataFields.GetNextSelectedItem(pos);
			}
		}

		// Add application trace about the database modification
		UCLIDException uex("ELI37714", "Application trace: Database change");
		uex.addDebugInfo("Change", "Metadata field(s) removed");
		uex.addDebugInfo("User Name", getCurrentUserName());
		uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
		uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
		string strFields = "";

		for (vector<string>::iterator it = vecFieldsToDelete.begin();
			it != vecFieldsToDelete.end(); ++it)
		{
			m_ipFAMDB->DeleteMetadataField(it->c_str());

			if (!strFields.empty())
			{
				strFields += ", ";
			}
			strFields += (*it);
		}

		uex.addDebugInfo("Metadata Field Name(s)", strFields);
		uex.log();

		refreshMetadataFieldList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37715");
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::OnBtnRename()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );  

	try
	{

		// Check if there is no field selected
		if ( m_listMetadataFields.GetSelectedCount() != 1 )
		{
			return;
		}

		// Get the selected field to rename
		POSITION pos = m_listMetadataFields.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			int iIndex = m_listMetadataFields.GetNextSelectedItem( pos );
			
			string strOldName = m_listMetadataFields.GetItemText(iIndex, giMETADATAFIELDNAME_COLUMN);

			// Prompt for new metadata field name
			PromptDlg dlgAddField("Rename Metadata Field", "Metadata Field Name", strOldName.c_str());

			if (dlgAddField.DoModal() == IDOK)
			{
				string strNewName = (LPCSTR)dlgAddField.m_zInput;
				
				if (strOldName != strNewName)
				{
					// Add application trace about the database modification
					UCLIDException uex("ELI37716", "Application trace: Database change");
					uex.addDebugInfo("Change", "Rename metadata field");
					uex.addDebugInfo("Old Metadata Field Name", strOldName);
					uex.addDebugInfo("New Metadata Field Name", strNewName);
					uex.addDebugInfo("User Name", getCurrentUserName());
					uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
					uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));

					m_ipFAMDB->RenameMetadataField(strOldName.c_str(), strNewName.c_str());

					// Log application trace exception
					uex.log();

					refreshMetadataFieldList();
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37717");
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::OnBtnRefresh()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		refreshMetadataFieldList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37662");
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::OnBtnClose()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call dialog OnOK
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37650");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::updateControls()
{
	try
	{
		// Get the selection count
		unsigned int iCount = m_listMetadataFields.GetSelectedCount();

		// Enable delete if at least 1 item selected
		// Enable modify iff 1 item selected
		BOOL bEnableRemove = asMFCBool(iCount > 0);
		BOOL bEnableRename = asMFCBool(iCount == 1);

		m_btnRemove.EnableWindow(bEnableRemove);
		m_btnRename.EnableWindow(bEnableRename);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37651");
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::configureMetadataFieldList()
{
	try
	{
		// Set list style
		m_listMetadataFields.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);

		// Build column information struct
		LVCOLUMN lvColumn;
		lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
		lvColumn.fmt = LVCFMT_LEFT;

		// Get dimensions of list control
		CRect recList;
		m_listMetadataFields.GetClientRect(recList);

		// Set information for metadata field name column
		lvColumn.pszText = "Metadata field name";
		lvColumn.cx = recList.Width();

		// Add the metadata field name column
		m_listMetadataFields.InsertColumn(giMETADATAFIELDNAME_COLUMN, &lvColumn);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37652");
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::refreshMetadataFieldList()
{
	try
	{
		SuspendWindowUpdates updater(*this);

		// Store the position of the first selected item
		int nSelectedItem = 0;
		POSITION pos = m_listMetadataFields.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			nSelectedItem = m_listMetadataFields.GetNextSelectedItem(pos);
		}
		
		clearListSelection();

		// Clear the current field list
		m_listMetadataFields.DeleteAllItems();

		IVariantVectorPtr ipMetadataFields = m_ipFAMDB->GetMetadataFieldNames();
		ASSERT_RESOURCE_ALLOCATION("ELI37653", ipMetadataFields != __nullptr);

		// Get the number of fields
		long lSize = ipMetadataFields->Size;

		// Populate the list
		for (long i=0; i < lSize; i++)
		{
			// Get the metadata field name
			_bstr_t bstrMetadataFieldName = ipMetadataFields->Item[i].bstrVal;

			// Add the info into the list
			m_listMetadataFields.InsertItem(i, (const char*)bstrMetadataFieldName);
		}

		// Select either the last selected item position or select the first item
		m_listMetadataFields.SetItemState(nSelectedItem < lSize ? nSelectedItem : 0,
			LVIS_SELECTED | LVIS_FOCUSED, LVIS_SELECTED | LVIS_FOCUSED);

		// Need to ensure the column is the correct width because there might be a scroll bar now
		CRect recList;
		m_listMetadataFields.GetClientRect(recList);

		// Set the width of the column
		m_listMetadataFields.SetColumnWidth(giMETADATAFIELDNAME_COLUMN, recList.Width());

		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37654");
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::clearListSelection()
{
	try
	{
		POSITION pos = m_listMetadataFields.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// no item selected, return
			return;
		}

		// iterate through all selections and set them as unselected.  pos will be
		// set to NULL by GetNextSelectedItem if there are no more selected items
		while (pos)
		{
			int nItemSelected = m_listMetadataFields.GetNextSelectedItem(pos);
			m_listMetadataFields.SetItemState(nItemSelected, 0, LVIS_SELECTED);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37655");
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::OnLvnItemchangedList(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update controls when selection changes
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37663");

	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void CManageMetadataFieldsDlg::OnNMRDblclkList(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Same as clicking rename
		OnBtnRename();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37664");

	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
BOOL CManageMetadataFieldsDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Configure the list
		configureMetadataFieldList();

		// Populate the list
		refreshMetadataFieldList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37665");

	return FALSE;
}
