// SelectFilesDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SelectFilesDlg.h"
#include "ActionStatusCondition.h"
#include "ActionStatusConditionDlg.h"
#include "FilePriorityCondition.h"
#include "FilePriorityConditionDlg.h"
#include "QueryCondition.h"
#include "QueryConditionDlg.h"
#include "FileTagCondition.h"
#include "FileTagConditionDlg.h"
#include "SpecifiedFilesCondition.h"
#include "SpecifiedFilesConditionDlg.h"
#include "FileSetCondition.h"
#include "FileSetConditionDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ADOUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giACTION_CONJUNCTION_COL_WIDTH = 50;

//-------------------------------------------------------------------------------------------------
// CSelectFilesDlg dialog
//-------------------------------------------------------------------------------------------------
CSelectFilesDlg::CSelectFilesDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
								 const string& strSectionHeader, const string& strQueryHeader,
								 const SelectFileSettings& settings)
: CDialog(CSelectFilesDlg::IDD)
, m_ipFAMDB(ipFAMDB)
, m_strSectionHeader(strSectionHeader)
, m_strQueryHeader(strQueryHeader)
, m_settings(settings)
, m_nActionStatusConditionOption(-1)
, m_nFileSetConditionOption(-1)
, m_nPriorityConditionOption(-1)
, m_nQueryConditionOption(-1)
, m_nSpecifiedFilesConditionOption(-1)
, m_nTagConditionOption(-1)
{
}
//-------------------------------------------------------------------------------------------------
CSelectFilesDlg::~CSelectFilesDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27325");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CSelectFilesDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_GROUP_SELECT, m_grpSelectFor);
	DDX_Control(pDX, IDC_LIST_CONDITIONS, m_listConditions);
	DDX_Control(pDX, IDC_BTN_MODIFY_CONDITION, m_btnModifyCondition);
	DDX_Control(pDX, IDC_BTN_DELETE_CONDITION, m_btnDeleteCondition);
	DDX_Control(pDX, IDC_RADIO_AND, m_cmbAnd);
	DDX_Control(pDX, IDC_RADIO_OR, m_cmbOr);
	DDX_Control(pDX, IDC_CHECK_LIMIT_SCOPE, m_checkSubset);
	DDX_Control(pDX, IDC_EDIT_LIMIT_SCOPE, m_editSubsetSize);
	DDX_Control(pDX, IDC_CMB_LIMIT_SCOPE_UNITS, m_comboSubsetUnits);
	DDX_Control(pDX, IDC_CMB_LIMIT_SCOPE_METHOD, m_comboSubsetMethod);
	DDX_Control(pDX, IDC_CMB_CONDITION_TYPE, m_cmbConditionType);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSelectFilesDlg, CDialog)
	//{{AFX_MSG_MAP(CSelectFilesDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &CSelectFilesDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &CSelectFilesDlg::OnClickedCancel)
	ON_BN_CLICKED(IDC_CHECK_LIMIT_SCOPE, &CSelectFilesDlg::OnClickedCheckSubset)
	ON_BN_CLICKED(IDC_BTN_ADD_CONDITION, &CSelectFilesDlg::OnBnClickedBtnAddCondition)
	ON_BN_CLICKED(IDC_BTN_MODIFY_CONDITION, &CSelectFilesDlg::OnBnClickedBtnModifyCondition)
	ON_BN_CLICKED(IDC_BTN_DELETE_CONDITION, &CSelectFilesDlg::OnBnClickedBtnDeleteCondition)
	ON_BN_CLICKED(IDC_RADIO_AND, &CSelectFilesDlg::OnBnClickedConjunction)
	ON_BN_CLICKED(IDC_RADIO_OR, &CSelectFilesDlg::OnBnClickedConjunction)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_CONDITIONS, &CSelectFilesDlg::OnNMDblclkListConditions)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_CONDITIONS, &CSelectFilesDlg::OnLvnItemChangedListConditions)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CSelectFilesDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CSelectFilesDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Set the group box caption
		m_grpSelectFor.SetWindowText(m_strSectionHeader.c_str());

		IVariantVectorPtr ipFileSets = m_ipFAMDB->GetFileSets();
		ASSERT_RESOURCE_ALLOCATION("ELI37353", ipFileSets != __nullptr);

		// Populate the available conditions and set the action status condition as the default.
		m_nActionStatusConditionOption = m_cmbConditionType.AddString("Action status condition");
		if (ipFileSets->Size > 0)
		{
			// Only show the file set condition if at least one file set is available.
			m_nFileSetConditionOption = m_cmbConditionType.AddString("File set condition");
		}
		m_nPriorityConditionOption = m_cmbConditionType.AddString("Priority condition");
		m_nQueryConditionOption = m_cmbConditionType.AddString("Query condition");
		m_nSpecifiedFilesConditionOption = m_cmbConditionType.AddString("Specified file(s) condition");
		m_nTagConditionOption = m_cmbConditionType.AddString("Tag condition");
		m_cmbConditionType.SetCurSel(0);

		// Intialize the list control that displays the configured conditions.
		CRect rect;
		m_listConditions.GetClientRect(&rect);

		m_listConditions.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		m_listConditions.InsertColumn(0, "Select all files", LVCFMT_LEFT,
			rect.Width() - giACTION_CONJUNCTION_COL_WIDTH);
		m_listConditions.InsertColumn(1, "", LVCFMT_LEFT, giACTION_CONJUNCTION_COL_WIDTH); 

		// Default subset to random and as a percentage.
		m_comboSubsetMethod.SetCurSel(0);
		m_comboSubsetUnits.SetCurSel(0);

		// Update the controls
		updateControls();

		// Read the settings object and set the dialog based on the settings
		setControlsFromSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26986")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnOK()
{
	// Allow the user to use the enter key to close the dialog
	OnClickedOK();
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26990");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClickedCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call cancel
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26991");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClickedOK()
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26992");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClickedCheckSubset()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27706");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnBnClickedBtnAddCondition()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		int nCurSel = m_cmbConditionType.GetCurSel();

		if (nCurSel == m_nActionStatusConditionOption)
		{
			addCondition(new ActionStatusCondition());
		}
		else if (nCurSel == m_nFileSetConditionOption)
		{
			addCondition(new FileSetCondition());
		}
		else if (nCurSel == m_nPriorityConditionOption)
		{
			addCondition(new FilePriorityCondition());
		}
		else if (nCurSel == m_nQueryConditionOption)
		{
			addCondition(new QueryCondition());
		}
		else if (nCurSel == m_nSpecifiedFilesConditionOption)
		{
			addCondition(new SpecifiedFilesCondition());
		}
		else if (nCurSel == m_nTagConditionOption)
		{
			addCondition(new FileTagCondition());
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI33787");
		}

		setControlsFromSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33786");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnBnClickedBtnModifyCondition()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (!m_settings.getConditions().empty())
		{
			POSITION pos = m_listConditions.GetFirstSelectedItemPosition();
			if (pos != __nullptr)
			{
				int nIndex = m_listConditions.GetNextSelectedItem(pos);

				SelectFileCondition* pCondition = m_settings.getConditions()[nIndex];
				ASSERT_RESOURCE_ALLOCATION("ELI33791", pCondition != __nullptr);

				pCondition->configure(m_ipFAMDB, m_strQueryHeader);

				setControlsFromSettings();
			}
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33789");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnBnClickedBtnDeleteCondition()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (!m_settings.getConditions().empty())
		{
			POSITION pos = m_listConditions.GetFirstSelectedItemPosition();
			if (pos != __nullptr)
			{
				int nIndex = m_listConditions.GetNextSelectedItem(pos);

				m_settings.deleteCondition(nIndex);

				setControlsFromSettings();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33790");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnBnClickedConjunction()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		m_settings.setConjunction(m_cmbAnd.GetCheck() == BST_CHECKED);

		setControlsFromSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33792");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnNMDblclkListConditions(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
		
		int nIndex = pNMItemActivate->iItem;

		if (nIndex >= 0 && nIndex < (int)m_settings.getConditions().size())
		{
			SelectFileCondition* pCondition = m_settings.getConditions()[nIndex];
			ASSERT_RESOURCE_ALLOCATION("ELI33794", pCondition != __nullptr);

			pCondition->configure(m_ipFAMDB, m_strQueryHeader);

			setControlsFromSettings();
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33793");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnLvnItemChangedListConditions(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
		ASSERT_RESOURCE_ALLOCATION("ELI33825", pNMLV != __nullptr);
		
		BOOL bEnable = asMFCBool((pNMLV->uNewState & LVIS_SELECTED) != 0);

		m_btnModifyCondition.EnableWindow(bEnable);
		m_btnDeleteCondition.EnableWindow(bEnable);

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33824");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
template <class T>
void CSelectFilesDlg::addCondition(T* pCondition)
{
	try
	{
		if (pCondition->configure(m_ipFAMDB, m_strQueryHeader))
		{
			m_settings.addCondition(pCondition);
		}
		else
		{
			delete pCondition;
		}
	}
	catch (...)
	{
		delete pCondition;
	}
}
//-------------------------------------------------------------------------------------------------
bool CSelectFilesDlg::saveSettings()
{
	try
	{
		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Check for narrowing scope
		long nSubsetSize = -1;
		bool bUseRandom = true;
		bool bUseTop = false;
		bool bUsePercentage = true;
		if (m_checkSubset.GetCheck() == BST_CHECKED)
		{
			bUseRandom = (m_comboSubsetMethod.GetCurSel() == 0);
			bUseTop = (m_comboSubsetMethod.GetCurSel() == 1);

			CString zMessageUnits;

			// Get the units to be used to limit the subset.
			if (m_comboSubsetUnits.GetCurSel() == 0)
			{
				bUsePercentage = true;
				zMessageUnits = "percentage";
			}
			else
			{
				bUsePercentage = false;
				zMessageUnits = "count";
			}

			// Get the amount from the control
			CString zTemp;
			m_editSubsetSize.GetWindowText(zTemp);

			if (zTemp.IsEmpty())
			{
				MessageBox("Must not leave " + zMessageUnits + " blank!", "Empty " + zMessageUnits,
					MB_OK | MB_ICONERROR);
				m_editSubsetSize.SetFocus();
				return false;
			}

			// Convert string to long
			nSubsetSize = asLong((LPCTSTR) zTemp);

			if (bUsePercentage)
			{
				if (nSubsetSize < 1 || nSubsetSize > 99)
				{
					MessageBox("Percentage must be between 1 and 99 inclusive!",
						"Invalid Percentage", MB_OK | MB_ICONERROR);
					m_editSubsetSize.SetFocus();
					return false;
				}
			}
			else
			{
				if (nSubsetSize < 1)
				{
					MessageBox("Random subset size must be at least 1!",
						"Invalid Subset Size", MB_OK | MB_ICONERROR);
					m_editSubsetSize.SetFocus();
					return false;
				}
			}
		}

		// Set the scope narrowing values
		m_settings.setLimitToSubset(nSubsetSize != -1);
		if (nSubsetSize != -1)
		{
			m_settings.setSubsetIsRandom(bUseRandom);
			m_settings.setSubsetIsTop(bUseTop);
			m_settings.setSubsetUsePercentage(bUsePercentage);
			m_settings.setSubsetSize(nSubsetSize);
		}

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26996")
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::updateControls() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		BOOL bSubset = asMFCBool(m_checkSubset.GetCheck() == BST_CHECKED);

		// Enable/disable the subset configuration controls depending on m_checkSubset.
		m_comboSubsetMethod.EnableWindow(bSubset);
		m_editSubsetSize.EnableWindow(bSubset);
		m_comboSubsetUnits.EnableWindow(bSubset);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26997");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::setControlsFromSettings()
{
	try
	{
		bool bAnd = m_settings.getConjunction();
		m_cmbAnd.SetCheck(asMFCBool(bAnd));
		m_cmbOr.SetCheck(asMFCBool(!bAnd));

		// Populate list box
		m_listConditions.DeleteAllItems();
		vector<SelectFileCondition*> conditions = m_settings.getConditions();

		if (conditions.empty())
		{
			m_listConditions.GetHeaderCtrl()->ShowWindow(FALSE);
			m_listConditions.EnableWindow(FALSE);
		}
		else
		{
			m_listConditions.GetHeaderCtrl()->ShowWindow(TRUE);
			m_listConditions.EnableWindow(TRUE);

			for (size_t i = 0; i < conditions.size(); i++)
			{
				bool bFirst = (i == 0);
				if (!bFirst)
				{
					m_listConditions.SetItemText(i - 1, 1, m_settings.getConjunction() ? "and" : "or");
				}

				m_listConditions.InsertItem(i, conditions[i]->getSummaryString(bFirst).c_str());
			}
		}

		// There is no more selection in m_listConditions, so the delete and modify buttons should
		// be disabled.
		m_btnModifyCondition.EnableWindow(FALSE);
		m_btnDeleteCondition.EnableWindow(FALSE);

		// Check for limiting by random condition
		if (m_settings.getLimitToSubset())
		{
			// Set the check box and update the text in the edit control
			m_checkSubset.SetCheck(BST_CHECKED);
			m_comboSubsetMethod.SetCurSel(m_settings.getSubsetIsRandom() ? 0 
				: m_settings.getSubsetIsTop() ? 1 : 2);
			m_comboSubsetUnits.SetCurSel(m_settings.getSubsetUsePercentage() ? 0 : 1);
			m_editSubsetSize.SetWindowText(asString(m_settings.getSubsetSize()).c_str());
		}

		// Since changes have been made, re-update the controls
		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27000");
}
//-------------------------------------------------------------------------------------------------

