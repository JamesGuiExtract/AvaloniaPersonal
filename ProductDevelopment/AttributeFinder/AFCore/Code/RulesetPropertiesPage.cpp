// RuleSetPropertiesPage.cpp : implementation file
//

#include "stdafx.h"
#include "afcore.h"
#include "RuleSetPropertiesPage.h"
#include "CounterEditDlg.h"

#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>
#include <VectorOperations.h>
#include <LoadFileDlgThread.h>
#include <COMUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giENABLE_LIST_COLUMN = 0;
const int giID_LIST_COLUMN = 1;
const int giNAME_LIST_COLUMN = 2;

const int giTARGET_STATE_UNCHECKED = 2;

//-------------------------------------------------------------------------------------------------
// CRuleSetPropertiesPage dialog
//-------------------------------------------------------------------------------------------------
CRuleSetPropertiesPage::CRuleSetPropertiesPage(UCLID_AFCORELib::IRuleSetPtr ipRuleSet,
	bool bReadOnly)
: CPropertyPage(CRuleSetPropertiesPage::IDD)
, m_ipRuleSet(ipRuleSet)
, m_bReadOnly(bReadOnly)
{
}
//--------------------------------------------------------------------------------------------------
CRuleSetPropertiesPage::~CRuleSetPropertiesPage()
{
	try
	{
		m_ipRuleSet = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI38264");
}//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BTN_ADD_COUNTER, m_btnAddCounter);
	DDX_Control(pDX, IDC_BTN_EDIT_COUNTER, m_btnEditCounter);
	DDX_Control(pDX, IDC_BTN_DELETE_COUNTER, m_btnDeleteCounter);
	DDX_Control(pDX, IDC_COUNTER_LIST, m_CounterList);
	DDX_Control(pDX, IDC_CHECK_INTERNAL_USE_ONLY, m_checkboxForInternalUseOnly);
	DDX_Control(pDX, IDC_CHECK_SWIPING_RULE, m_checkSwipingRule);
	DDX_Control(pDX, IDC_FKB_VERSION, m_editFKBVersion);
	DDX_Control(pDX, IDC_CHECK_SPECIFY_OCR_PARAMETERS, m_checkSpecifiedOCRParameters);
	DDX_Control(pDX, IDC_BTN_OCRPARAMETERS, m_btnEditOCRParameters);
	DDX_Control(pDX, IDC_BTN_IMPORT_OCR_PARAMETERS, m_btnImportOCRParameters);

}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CRuleSetPropertiesPage, CPropertyPage)
	ON_BN_CLICKED(IDC_BTN_ADD_COUNTER, &CRuleSetPropertiesPage::OnClickedBtnAddCounter)
	ON_BN_CLICKED(IDC_BTN_EDIT_COUNTER, &CRuleSetPropertiesPage::OnClickedBtnEditCounter)
	ON_BN_CLICKED(IDC_BTN_DELETE_COUNTER, &CRuleSetPropertiesPage::OnClickedBtnDeleteCounter)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_COUNTER_LIST, &CRuleSetPropertiesPage::OnCounterListItemChanged)
	ON_BN_CLICKED(IDC_CHECK_SPECIFY_OCR_PARAMETERS, &CRuleSetPropertiesPage::OnBnClickedCheckSpecifyOcrParameters)
	ON_BN_CLICKED(IDC_BTN_OCRPARAMETERS, &CRuleSetPropertiesPage::OnBnClickedBtnOcrparameters)
	ON_BN_CLICKED(IDC_BTN_IMPORT_OCR_PARAMETERS, &CRuleSetPropertiesPage::OnBnClickedBtnImportOcrParameters)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CRuleSetPropertiesPage message handlers
//-------------------------------------------------------------------------------------------------
BOOL CRuleSetPropertiesPage::OnInitDialog() 
{
	try
	{
		CPropertyPage::OnInitDialog();
		
		setupCounterList();

		// Update the FKB version.
		m_editFKBVersion.SetWindowText(m_ipRuleSet->FKBVersion);

		// Update the checkboxes
		m_checkboxForInternalUseOnly.SetCheck( asBSTChecked(m_ipRuleSet->ForInternalUseOnly) );
		m_checkSwipingRule.SetCheck( asBSTChecked(m_ipRuleSet->IsSwipingRule) );

		IHasOCRParametersPtr ipHasParams(m_ipRuleSet);
		if (ipHasParams->OCRParameters->Size == 0)
		{
			m_checkSpecifiedOCRParameters.SetCheck(BST_UNCHECKED);
			m_btnEditOCRParameters.EnableWindow(FALSE);
		}
		else
		{
			m_checkSpecifiedOCRParameters.SetCheck(BST_CHECKED);
			m_btnEditOCRParameters.EnableWindow(TRUE);
		}

		// Hide checkboxes without full RDT license [FIDSC #3062, #3594]
		if (!isRdtLicensed())
		{
			hideCheckboxes();
		}

		if (m_bReadOnly)
		{
			m_CounterList.EnableWindow(FALSE);
			m_editFKBVersion.SetReadOnly(TRUE);
			m_checkboxForInternalUseOnly.EnableWindow(FALSE);
			m_checkSpecifiedOCRParameters.EnableWindow(FALSE);
			m_checkSwipingRule.EnableWindow(FALSE);
			m_btnEditOCRParameters.SetWindowText("View...");
			m_btnImportOCRParameters.EnableWindow(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11552")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::Apply()
{
	try
	{
		CString zFKBVersion;
		m_editFKBVersion.GetWindowText(zFKBVersion);
		int nEnabledCount = 0;

		// Update the enabled status of all counters in m_mapCounters based upon the check status.
		int nCount = m_CounterList.GetItemCount();
		for (int i = 0; i < nCount; i++)
		{
			long nCounterID = m_CounterList.GetItemData(i);
			bool bEnabled = m_CounterList.GetCheck(i) == BST_CHECKED;
			m_mapCounters[nCounterID].m_bEnabled = bEnabled;
			if (bEnabled)
			{
				nEnabledCount++;
			}
		}

		// Require the FKB version to be set if any counter has been selected to decrement.
		if (nEnabledCount > 0 && asCppBool(zFKBVersion.IsEmpty()))
		{
			UCLIDException ue("ELI32485", "An FKB version must be specified for a swiping rule or "
				"a ruleset that decrements counters.");
			throw ue;
		}

		// Apply m_mapCounters to m_ipRuleSet
		CounterInfo::ApplyCounterInfo(m_mapCounters, m_ipRuleSet);

		m_ipRuleSet->FKBVersion = _bstr_t(zFKBVersion);

		// update the value related to the checkbox for internal use
		bool bChecked = m_checkboxForInternalUseOnly.GetCheck() == BST_CHECKED;
		m_ipRuleSet->ForInternalUseOnly = asVariantBool(bChecked);

		// Store whether this is a swiping rule
		bChecked = m_checkSwipingRule.GetCheck() == BST_CHECKED;
		m_ipRuleSet->IsSwipingRule = asVariantBool(bChecked);

		bChecked = m_checkSpecifiedOCRParameters.GetCheck() == BST_CHECKED;

		if (!bChecked)
		{
			IHasOCRParametersPtr ipHasParams(m_ipRuleSet);
			ipHasParams->OCRParameters = __nullptr;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11553")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::OnClickedBtnAddCounter()
{
	try
	{
		addEditCounter(-1);

		// Added counter may require m_CounterList column sizes to account for scroll bar.
		updateGridWidth();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38989")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::OnClickedBtnEditCounter()
{
	try
	{
		int nItem = getSelectedItem();

		ASSERT_RUNTIME_CONDITION("ELI38992", nItem != -1, "No counter is selected.");

		addEditCounter(nItem);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38990")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::OnClickedBtnDeleteCounter()
{
	try
	{
		int nItem = getSelectedItem();

		ASSERT_RUNTIME_CONDITION("ELI40364", nItem != -1, "No counter is selected.");

		long nID = m_CounterList.GetItemData(nItem);
		m_mapCounters.erase(nID);
		m_CounterList.DeleteItem(nItem);

		// Deleted counter may require m_CounterList column sizes to account for removed scroll bar.
		updateGridWidth();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38991")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::OnCounterListItemChanged(NMHDR* pNMHDR, LRESULT* pResult)
{
	try
	{
		// Get the list view item structure from the message
		LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);

		// Get the index of the item and the ID for the associated counter.
		int iIndex = pNMLV->iItem;
		int iItemData = m_CounterList.GetItemData(iIndex);

		// Check if the item state has changed and it is changing to checked
		// AND it is one of the redaction check boxes
		if (pNMLV->uChanged & LVIF_STATE
			&& (pNMLV->uNewState & LVIS_STATEIMAGEMASK) == INDEXTOSTATEIMAGEMASK(giTARGET_STATE_UNCHECKED))
		{
			// Check dis-sallow either both of the indexing or both of the redaction counters from
			// being simultaneously checked.
			if (iItemData == giREDACTION_PAGES_COUNTERID)
			{
				// Need to uncheck the redaction count by doc item
				if (m_mapCounters[giREDACTION_DOCS_COUNTERID].m_nIndex != -1)
				{
					m_CounterList.SetCheck(m_mapCounters[giREDACTION_DOCS_COUNTERID].m_nIndex, FALSE);
				}
			}
			else if (iItemData == giREDACTION_DOCS_COUNTERID)
			{
				// Need to uncheck the redaction count by page item
				if (m_mapCounters[giREDACTION_PAGES_COUNTERID].m_nIndex != -1)
				{
					m_CounterList.SetCheck(m_mapCounters[giREDACTION_PAGES_COUNTERID].m_nIndex, FALSE);
				}
			}
			else if (iItemData == giINDEXING_PAGES_COUNTERID)
			{
				// Need to uncheck the redaction count by doc item
				if (m_mapCounters[giINDEXING_DOCS_COUNTERID].m_nIndex != -1)
				{
					m_CounterList.SetCheck(m_mapCounters[giINDEXING_DOCS_COUNTERID].m_nIndex, FALSE);
				}
			}
			else if (iItemData == giINDEXING_DOCS_COUNTERID)
			{
				// Need to uncheck the redaction count by page item
				if (m_mapCounters[giINDEXING_PAGES_COUNTERID].m_nIndex != -1)
				{
					m_CounterList.SetCheck(m_mapCounters[giINDEXING_PAGES_COUNTERID].m_nIndex, FALSE);
				}
			}
		}
		else if (m_CounterList.GetFirstSelectedItemPosition() != NULL)
		{
			if (iItemData < 100)
			{
				// Standard counters may not be edited.
				m_btnEditCounter.EnableWindow(FALSE);
				m_btnDeleteCounter.EnableWindow(FALSE);
			}
			else
			{
				m_btnEditCounter.EnableWindow(TRUE);
				m_btnDeleteCounter.EnableWindow(TRUE);
			}
		}
		else
		{
			// If nothing's selected, nothing to be edited/deleted.
			m_btnEditCounter.EnableWindow(FALSE);
			m_btnDeleteCounter.EnableWindow(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14496")

	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::OnBnClickedCheckSpecifyOcrParameters()
{
	try
	{
		bool bChecked = m_checkSpecifiedOCRParameters.GetCheck() == BST_CHECKED;
		m_btnEditOCRParameters.EnableWindow(bChecked);

		if (bChecked)
		{
			IHasOCRParametersPtr ipHasOCRParameters(m_ipRuleSet);

			// Open edit dialog if there are no parameters set
			if (ipHasOCRParameters->OCRParameters->Size == 0)
			{
				OnBnClickedBtnOcrparameters();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45953");
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::OnBnClickedBtnOcrparameters()
{
	try
	{
		// Create instance of the configure form using the Prog ID - to avoid circular dependency
		UCLID_RASTERANDOCRMGMTLib::IOCRParametersConfigurePtr ipConfigure;
		ipConfigure.CreateInstance("Extract.FileActionManager.Forms.OCRParametersConfigure");
		ASSERT_RESOURCE_ALLOCATION("ELI45954", ipConfigure != __nullptr);
		
		IHasOCRParametersPtr ipHasParams(m_ipRuleSet);

		// Configure the parameteres
		ipConfigure->ConfigureOCRParameters(ipHasParams, m_bReadOnly, (long)this->m_hWnd);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45955");
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::OnBnClickedBtnImportOcrParameters()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string strFile = chooseFile();	
		if (!strFile.empty())
		{
			// Load parameters from other file
			ILoadOCRParametersPtr ipLoadOCRParameters;
			ipLoadOCRParameters.CreateInstance(CLSID_RuleSet);
			ipLoadOCRParameters->LoadOCRParameters(get_bstr_t(strFile));

			// Copy them to this ruleset
			IHasOCRParametersPtr ipOtherHasOCRParameters(ipLoadOCRParameters);
			IHasOCRParametersPtr ipThisHasOCRParameters(m_ipRuleSet);

			// Check for existence of any params
			if (ipOtherHasOCRParameters->OCRParameters->Size == 0)
			{
				// Display Message Box about it
				MessageBox("No OCR parameters found", "Failure!", MB_OK | MB_ICONWARNING);
			}
			else
			{
				ipThisHasOCRParameters->OCRParameters = ipOtherHasOCRParameters->OCRParameters;

				// Enable controls
				m_checkSpecifiedOCRParameters.SetCheck(BST_CHECKED);
				m_btnEditOCRParameters.EnableWindow();

				MessageBox("OCR parameters imported", "Success!", MB_OK | MB_ICONINFORMATION);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI50079");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::hideCheckboxes()
{
	// Hide the check boxes
	m_checkboxForInternalUseOnly.ShowWindow(FALSE);
	m_checkSwipingRule.ShowWindow(FALSE);

	// Get the dimensions of the swiping rule checkbox (i.e. the last checkbox)
	RECT checkRect = {0};
	m_checkSwipingRule.GetWindowRect(&checkRect);
	ScreenToClient(&checkRect);

	// Get the dimensions of the key FKB edit box 
	// (i.e. the control immediately above the check boxes)
	RECT editRect = {0};
	m_editFKBVersion.GetWindowRect(&editRect);
	ScreenToClient(&editRect);

	// Calculate the distance of empty space that should be removed
	long lDelta = checkRect.bottom - editRect.bottom;

	// Shrink the dialog by the amount of empty space
	RECT mainRect = {0};
	GetWindowRect(&mainRect);
	mainRect.bottom -= lDelta;
	MoveWindow(&mainRect);
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::setupCounterList()
{
	m_CounterList.SetExtendedStyle( 
		LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES );
		
	// Get dimensions of control
	CRect	rect;
	m_CounterList.GetClientRect( &rect );

	// Define width of columns
	long lEnabledWidth = 55;
	long lIdWidth = 35;
	long lNameWidth = rect.Width() - lEnabledWidth - lIdWidth;

	// Add column headers
	m_CounterList.InsertColumn( giENABLE_LIST_COLUMN, "Enabled", 
		LVCFMT_CENTER, lEnabledWidth, giENABLE_LIST_COLUMN );

	m_CounterList.InsertColumn( giID_LIST_COLUMN, "ID", 
		LVCFMT_LEFT, lIdWidth, giID_LIST_COLUMN );

	m_CounterList.InsertColumn( giNAME_LIST_COLUMN, "Name", 
		LVCFMT_LEFT, lNameWidth, giNAME_LIST_COLUMN );

	// Retrieve the counter configuration from m_ipRuleSet into m_mapCounters.
	m_mapCounters = CounterInfo::GetCounterInfo(m_ipRuleSet);

	// Iterate the counter configuration to initialize m_CounterList.
	for (auto entry = m_mapCounters.begin(); entry != m_mapCounters.end(); entry++)
	{
		long nID = entry->first;
		CounterInfo& counterInfo = entry->second;

		// Because the counter grid will be disabled when read-only, its scroll bar won't work. To
		// Ensure all enabled counters can be seen, display only the enabled ones.
		if (m_bReadOnly && !counterInfo.m_bEnabled)
		{
			continue;
		}
		
		// Populate counter if:
		// - The grid is read-only
		// - or RDT is licensed
		// - or The counter is a custom counter
		// - or We have a gnFLEXINDEX_RULE_WRITING_OBJECTS license and the counter is indexing by doc.
		// - or We have a gnIDSHIELD_RULE_WRITING_OBJECTS license and the counter is redaction by page.
		if (m_bReadOnly || isRdtLicensed() || nID >= 100
			|| (nID == giINDEXING_DOCS_COUNTERID &&
				LicenseManagement::isLicensed(gnFLEXINDEX_RULE_WRITING_OBJECTS))
			|| (nID == giREDACTION_PAGES_COUNTERID &&
				LicenseManagement::isLicensed(gnIDSHIELD_RULE_WRITING_OBJECTS)))
		{
			counterInfo.m_nIndex = m_CounterList.InsertItem(counterInfo.m_nID, "");
			m_CounterList.SetItemText(counterInfo.m_nIndex, giID_LIST_COLUMN, asString(counterInfo.m_nID).c_str());
			m_CounterList.SetItemText(counterInfo.m_nIndex, giNAME_LIST_COLUMN, counterInfo.m_strName.c_str());
			m_CounterList.SetItemData(counterInfo.m_nIndex, counterInfo.m_nID);
			m_CounterList.SetCheck(counterInfo.m_nIndex, asMFCBool(counterInfo.m_bEnabled));
		}
		// If a counter that couldn't be displayed is enabled, we have a problem.
		else if (counterInfo.m_bEnabled)
		{
			UCLIDException ue("ELI38993", "Unable to set required counter!");
			ue.addDebugInfo("CounterID", nID);
			throw ue;
		}
	}

	if (m_bReadOnly)
	{
		m_btnAddCounter.ShowWindow(SW_HIDE);
		m_btnEditCounter.ShowWindow(SW_HIDE);
		m_btnDeleteCounter.ShowWindow(SW_HIDE);

		CRect rectCountersList;
		m_CounterList.GetWindowRect(&rectCountersList);
		ScreenToClient(&rectCountersList);

		CRect rectAddButton;
		m_btnAddCounter.GetWindowRect(&rectAddButton);
		ScreenToClient(&rectAddButton);

		rectCountersList.right = rectAddButton.right;
		m_CounterList.MoveWindow(&rectCountersList);
	}

	updateGridWidth();
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::addEditCounter(int nListIndex)
{
	CounterInfo* pCounterInfo = nullptr;
	CCounterEditDlg counterEditor(this);
	counterEditor.m_zCaption = (nListIndex == -1)
		? "Add Custom Counter"
		: "Edit Custom Counter";
	if (nListIndex != -1)
	{
		// If editing, load the existing counter ID and name into the counterEditor.
		counterEditor.m_zCounterID = m_CounterList.GetItemText(nListIndex, giID_LIST_COLUMN);
		counterEditor.m_zCounterName = m_CounterList.GetItemText(nListIndex, giNAME_LIST_COLUMN);
		pCounterInfo = &getCounterFromList(nListIndex);
		ASSERT_RUNTIME_CONDITION("ELI38994", pCounterInfo->m_nID >= 100,
			"Internal logic error");
	}

	// Display counterEditor in a loop until they either cancel or enter valid data.
	long nNewID = -1;
	while (counterEditor.DoModal() == IDOK)
	{
		long nNewID = asLong((LPCTSTR)counterEditor.m_zCounterID);

		// If adding a new counter or the counter ID has been changed, check to ensure the ID isn't
		// already being used for another counter.
		if ((pCounterInfo == nullptr || pCounterInfo->m_nID != nNewID) &&
			m_mapCounters.find(nNewID) != m_mapCounters.end())
		{
			MessageBox("Counter ID is already in use.", "Duplicate counter ID", MB_OK);
			continue;
		}	

		// If the ID is being changed, remove we will replace the old CounterInfo instance with a
		// completely new one.
		if (pCounterInfo != nullptr && pCounterInfo->m_nID != nNewID)
		{
			m_mapCounters.erase(pCounterInfo->m_nID);
			pCounterInfo = nullptr;
		}

		if (isCounterNameUsed((LPCTSTR)counterEditor.m_zCounterName))
		{
			MessageBox("Counter name is already in use.", "Duplicate counter name", MB_OK);
			continue;
		}

		if (pCounterInfo == nullptr)
		{
			// New counter or ID has been changed
			m_mapCounters.emplace(make_pair(nNewID, CounterInfo(nNewID, string(counterEditor.m_zCounterName))));
		}
		else
		{
			// Existing counter whose ID remains the same.
			m_mapCounters[nNewID].m_strName = counterEditor.m_zCounterName.Trim();
		}

		if (nListIndex == -1)
		{
			// A new counter is being added.
			nListIndex = m_CounterList.GetItemCount();
			m_CounterList.InsertItem(nListIndex, "");
			m_CounterList.SetCheck(nListIndex, TRUE); // Default any added counter to enabled.
			m_CounterList.SetItemState(nListIndex, LVIS_SELECTED, LVIS_SELECTED);
			m_CounterList.EnsureVisible(nListIndex, FALSE);
		}
		
		// Ensure the CounterInfo instance is linked with the correct row in m_CounterList
		m_mapCounters[nNewID].m_nIndex = nListIndex;

		// Set the specified data for the m_CounterList row.
		m_CounterList.SetItemText(nListIndex, giID_LIST_COLUMN, counterEditor.m_zCounterID);
		m_CounterList.SetItemText(nListIndex, giNAME_LIST_COLUMN, counterEditor.m_zCounterName.Trim());
		m_CounterList.SetItemData(nListIndex, nNewID);

		// Ensure grid has focus to show current selection.
		m_CounterList.SetFocus();

		break;
	}
}
//-------------------------------------------------------------------------------------------------
bool CRuleSetPropertiesPage::isCounterAvailable(int nCounterID, bool &rbIsCounterChecked)
{
	long nIndex = m_mapCounters[nCounterID].m_nIndex;

	if (nIndex != -1)
	{
		rbIsCounterChecked = (m_CounterList.GetCheck(nIndex) == BST_CHECKED);
		return true;
	}
	
	return false;
}
//-------------------------------------------------------------------------------------------------
CounterInfo& CRuleSetPropertiesPage::getCounterFromList(long nIndex)
{
	long nID = m_CounterList.GetItemData(nIndex);
	return m_mapCounters[nID];
}
//-------------------------------------------------------------------------------------------------
bool CRuleSetPropertiesPage::isCounterNameUsed(const char* szName)
{
	string strName = trim(szName, " ", " ");

	for (auto entry = m_mapCounters.begin(); entry != m_mapCounters.end(); entry++)
	{
		CounterInfo& counterInfo = entry->second;
		if (_strcmpi(strName.c_str(), counterInfo.m_strName.c_str()) == 0)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
int CRuleSetPropertiesPage::getSelectedItem()
{
	POSITION pos = m_CounterList.GetFirstSelectedItemPosition();
	if (pos != NULL)
	{
		return m_CounterList.GetNextSelectedItem(pos);
	}

	return -1;
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::updateGridWidth()
{
	try
	{
		// giNAME_LIST_COLUMN should fill all client width not used by giENABLE_LIST_COLUMN or
		// giID_LIST_COLUMN.
		int nUsedWidth = m_CounterList.GetColumnWidth(giENABLE_LIST_COLUMN);
		nUsedWidth += m_CounterList.GetColumnWidth(giID_LIST_COLUMN);

		CRect rect;
		m_CounterList.GetClientRect(&rect);
		m_CounterList.SetColumnWidth(giNAME_LIST_COLUMN, rect.Width() - nUsedWidth);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38995");
}
//-------------------------------------------------------------------------------------------------
bool CRuleSetPropertiesPage::isRdtLicensed()
{
	return LicenseManagement::isLicensed(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS);
}
//-------------------------------------------------------------------------------------------------
const std::string CRuleSetPropertiesPage::chooseFile()
{
	const static string s_strFiles = "Ruleset definition files (*.rsd;*.etf)|*.rsd;*.etf|All Files (*.*)|*.*||";

	// bring open file dialog
	CFileDialog fileDlg(TRUE, NULL, "", 
		OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
		s_strFiles.c_str(), CWnd::FromHandle(m_hWnd));
	
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
