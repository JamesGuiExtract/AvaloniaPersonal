// RuleSetPropertiesPage.cpp : implementation file
//

#include "stdafx.h"
#include "afcore.h"
#include "RuleSetPropertiesPage.h"

#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>
#include <VectorOperations.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giENABLE_LIST_COLUMN = 0;
const int giNAME_LIST_COLUMN = 1;
const int giINDEXING_ITEM = 0;
const int giPAGINATION_ITEM = 1;
const int giREDACTION_PAGES_ITEM = 2;
const int giREDACTION_DOCS_ITEM = 3;

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
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI34021");
}//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_COUNTER_LIST, m_CounterList);
	DDX_Control(pDX, IDC_CHECK_INTERNAL_USE_ONLY, m_checkboxForInternalUseOnly);
	DDX_Control(pDX, IDC_SERIAL_NUMBERS, m_editKeySerialNumbers);
	DDX_Control(pDX, IDC_CHECK_SWIPING_RULE, m_checkSwipingRule);
	DDX_Control(pDX, IDC_FKB_VERSION, m_editFKBVersion);
}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CRuleSetPropertiesPage, CPropertyPage)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_COUNTER_LIST, &CRuleSetPropertiesPage::OnCounterListItemChanged)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CRuleSetPropertiesPage message handlers
//-------------------------------------------------------------------------------------------------
BOOL CRuleSetPropertiesPage::OnInitDialog() 
{
	try
	{
		CPropertyPage::OnInitDialog();
		
		// setup UI controls
		setupCounterList();

		// Next, initialize the UI with data from the RuleSet object's counters

		// Update the Indexing counter [FlexIDSCore #3059]
		if (asCppBool( m_ipRuleSet->UseIndexingCounter ))
		{
			// Rule set requires this counter, confirm license state.  If counter 
			// is licensed, item value >= 0
			if (m_iIndexingCounterItem != -1)
			{
				// Check the checkbox
				m_CounterList.SetCheck( m_iIndexingCounterItem, TRUE );
			}
			else
			{
				// Error
				UCLIDException ue( "ELI21445", "Unable to set required indexing counter!" );
				throw ue;
			}
		}
		else if (m_iIndexingCounterItem != -1)
		{
			// Uncheck the checkbox
			m_CounterList.SetCheck( m_iIndexingCounterItem, FALSE );
		}

		// Update the Pagination counter
		if (asCppBool( m_ipRuleSet->UsePaginationCounter ))
		{
			// Rule set requires this counter, confirm license state.  If counter 
			// is licensed, item value >= 0
			if (m_iPaginationCounterItem != -1)
			{
				// Check the checkbox
				m_CounterList.SetCheck( m_iPaginationCounterItem, TRUE );
			}
			else
			{
				// Error
				UCLIDException ue( "ELI21446", "Unable to set required pagination counter!" );
				throw ue;
			}
		}
		else if (m_iPaginationCounterItem != -1)
		{
			// Uncheck the checkbox
			m_CounterList.SetCheck( m_iPaginationCounterItem, FALSE );
		}

		// Update the Redaction By Pages counter
		if (asCppBool( m_ipRuleSet->UsePagesRedactionCounter ))
		{
			// Rule set requires this counter, confirm license state.  If counter 
			// is licensed, item value >= 0
			if (m_iRedactPagesCounterItem != -1)
			{
				// Check the checkbox
				m_CounterList.SetCheck( m_iRedactPagesCounterItem, TRUE );
			}
			else
			{
				// Error
				UCLIDException ue( "ELI21447", 
					"Unable to set required redaction by pages counter!" );
				throw ue;
			}
		}
		else if (m_iRedactPagesCounterItem != -1)
		{
			// Uncheck the checkbox
			m_CounterList.SetCheck( m_iRedactPagesCounterItem, FALSE );
		}

		// Update the Redaction By Documents counter
		if (asCppBool( m_ipRuleSet->UseDocsRedactionCounter ))
		{
			// Rule set requires this counter, confirm license state.  If counter 
			// is licensed, item value >= 0
			if (m_iRedactDocsCounterItem != -1)
			{
				// Check the checkbox
				m_CounterList.SetCheck( m_iRedactDocsCounterItem, TRUE );
			}
			else
			{
				// Error
				UCLIDException ue( "ELI21448", 
					"Unable to set required redaction by documents counter!" );
				throw ue;
			}
		}
		else if (m_iRedactDocsCounterItem != -1)
		{
			// Uncheck the checkbox
			m_CounterList.SetCheck( m_iRedactDocsCounterItem, FALSE );
		}

		// Update the USB counter serial number edit box
		m_editKeySerialNumbers.SetWindowText(m_ipRuleSet->KeySerialList);

		// Update the FKB version.
		m_editFKBVersion.SetWindowText(m_ipRuleSet->FKBVersion);

		// Update the checkboxes
		m_checkboxForInternalUseOnly.SetCheck( asBSTChecked(m_ipRuleSet->ForInternalUseOnly) );
		m_checkSwipingRule.SetCheck( asBSTChecked(m_ipRuleSet->IsSwipingRule) );

		// Hide checkboxes without full RDT license [FIDSC #3062, #3594]
		if (!isRdtLicensed())
		{
			hideCheckboxes();
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
		m_ipRuleSet->FKBVersion = _bstr_t(zFKBVersion);

		bool bChecked;

		// Require the FKB verson to be set if 
		if (asCppBool(zFKBVersion.IsEmpty()) &&
				((isCounterAvailable(m_iIndexingCounterItem, bChecked) && bChecked) ||
				 (isCounterAvailable(m_iPaginationCounterItem, bChecked) && bChecked) ||
				 (isCounterAvailable(m_iRedactPagesCounterItem, bChecked) && bChecked) ||
				 (isCounterAvailable(m_iRedactDocsCounterItem, bChecked) && bChecked) ||
				 m_CounterList.GetCheck( m_iRedactDocsCounterItem ) == BST_CHECKED))
		{
			UCLIDException ue("ELI32485", "An FKB version must be specified for a swiping rule or "
				"a ruleset that decrements counters.");
			ue.display();
			return;
		}

		// update the state of the Indexing and Pagination counters
		if (isCounterAvailable(m_iIndexingCounterItem, bChecked))
		{
			m_ipRuleSet->UseIndexingCounter = asVariantBool(bChecked);
		}

		if (isCounterAvailable(m_iPaginationCounterItem, bChecked))
		{
			m_ipRuleSet->UsePaginationCounter = asVariantBool(bChecked);
		}

		// Validate one or the other of the redaction checkboxes (P16 #2198)
		if ((isCounterAvailable(m_iRedactPagesCounterItem, bChecked) && bChecked) &&
		    (isCounterAvailable(m_iRedactDocsCounterItem, bChecked) && bChecked))
		{
			UCLIDException ue("ELI14499", "Cannot select redaction by pages and documents!");
			ue.display();
			return;
		}

		// Update state of the appropriate Redaction counter
		if (isCounterAvailable(m_iRedactPagesCounterItem, bChecked))
		{
			m_ipRuleSet->UsePagesRedactionCounter = asVariantBool(bChecked);
		}

		if (isCounterAvailable(m_iRedactDocsCounterItem, bChecked))
		{
			m_ipRuleSet->UseDocsRedactionCounter = asVariantBool(bChecked);
		}

		// Get the serial number list
		CString zSerialText;
		m_editKeySerialNumbers.GetWindowText(zSerialText);

		// Set focus to serial number list so if list not valid it will have the focus
		m_editKeySerialNumbers.SetFocus();
		validateSerialList(LPCTSTR(zSerialText));

		// Serial number list is valid so save in the ruleset
		m_ipRuleSet->KeySerialList = _bstr_t(zSerialText);
		
		// update the value related to the checkbox for internal use
		bChecked = m_checkboxForInternalUseOnly.GetCheck() == BST_CHECKED;
		m_ipRuleSet->ForInternalUseOnly = asVariantBool(bChecked);

		// Store whether this is a swiping rule
		bChecked = m_checkSwipingRule.GetCheck() == BST_CHECKED;
		m_ipRuleSet->IsSwipingRule = asVariantBool(bChecked);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11553")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::OnCounterListItemChanged(NMHDR* pNMHDR, LRESULT* pResult)
{
	try
	{
		// Get the list view item structure from the message
		LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);

		// Check if the item state has changed and it is changing to checked
		// AND it is one of the redaction check boxes
		if (pNMLV->uChanged & LVIF_STATE
			&& (pNMLV->uNewState & LVIS_STATEIMAGEMASK) == INDEXTOSTATEIMAGEMASK(giTARGET_STATE_UNCHECKED))
		{
			// Get the index of the checked/unchecked item and the item data for the item
			int iIndex = pNMLV->iItem;
			int iItemData = m_CounterList.GetItemData(iIndex);

			// Check if this is one of the redaction check boxes
			if (iItemData == giREDACTION_PAGES_ITEM)
			{
				// Need to uncheck the redaction count by doc item
				if (m_iRedactDocsCounterItem != -1)
				{
					m_CounterList.SetCheck(m_iRedactDocsCounterItem, FALSE);
				}
			}
			else if (iItemData == giREDACTION_DOCS_ITEM)
			{
				// Need to uncheck the redaction count by page item
				if (m_iRedactPagesCounterItem != -1)
				{
					m_CounterList.SetCheck(m_iRedactPagesCounterItem, FALSE);
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14496")

	*pResult = 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::hideCheckboxes()
{
	// Hide the check boxes
	m_checkboxForInternalUseOnly.ShowWindow(FALSE);
	m_checkSwipingRule.ShowWindow(FALSE);

	// Get the dimensions of the swiping rule checkbox (ie. the last checkbox)
	RECT checkRect = {0};
	m_checkSwipingRule.GetWindowRect(&checkRect);
	ScreenToClient(&checkRect);

	// Get the dimensions of the key serial numbers edit box 
	// (ie. the control immediately above the check boxes)
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
	long lEnabledWidth = 60;
	long lNameWidth = rect.Width() - lEnabledWidth;

	// Add column headers
	m_CounterList.InsertColumn( giENABLE_LIST_COLUMN, "Enabled", 
		LVCFMT_CENTER, lEnabledWidth, giENABLE_LIST_COLUMN );

	m_CounterList.InsertColumn( giNAME_LIST_COLUMN, "Name", 
		LVCFMT_LEFT, lNameWidth, giNAME_LIST_COLUMN );

	// Default each counter item to unused
	m_iIndexingCounterItem = -1;
	m_iPaginationCounterItem = -1;
	m_iRedactPagesCounterItem = -1;
	m_iRedactDocsCounterItem = -1;

	// Determine if FLEX Index rules are licensed - requires FLEX Index Rule Writing license
	// (But if in read-only mode, always show)
	// [FlexIDSCore #3059]
	int nCounterItemPosition = giINDEXING_ITEM;
	if (m_bReadOnly || LicenseManagement::isLicensed( gnFLEXINDEX_RULE_WRITING_OBJECTS ))
	{
		// Add this item at the top of the list
		nCounterItemPosition = m_CounterList.InsertItem( giINDEXING_ITEM, "" );
		m_CounterList.SetItemText( nCounterItemPosition, giNAME_LIST_COLUMN, 
			"FLEX Index - Indexing" );
		m_CounterList.SetItemData( nCounterItemPosition, giINDEXING_ITEM );

		// Save position of this item
		m_iIndexingCounterItem = nCounterItemPosition;
	}

	// Determine if Pagination rules are licensed - requires full RDT license
	// (But if in read-only mode, always show)
	if (m_bReadOnly || isRdtLicensed())
	{
		// Add this item next
		nCounterItemPosition = m_CounterList.InsertItem( nCounterItemPosition + 1, "" );
		m_CounterList.SetItemText( nCounterItemPosition, giNAME_LIST_COLUMN, 
			"FLEX Index - Pagination" );
		m_CounterList.SetItemData( nCounterItemPosition, giPAGINATION_ITEM );

		// Save position of this item
		m_iPaginationCounterItem = nCounterItemPosition;
	}

	// Determine if Redaction By Pages rules are licensed - requires ID Shield Rule Writing license
	// (But if in read-only mode, always show)
	if (m_bReadOnly || LicenseManagement::isLicensed( gnIDSHIELD_RULE_WRITING_OBJECTS ))
	{
		// Add this item next
		nCounterItemPosition = m_CounterList.InsertItem( nCounterItemPosition + 1, "" );
		m_CounterList.SetItemText( nCounterItemPosition, giNAME_LIST_COLUMN, 
			"ID Shield - Redaction (By Page)" );
		m_CounterList.SetItemData( nCounterItemPosition, giREDACTION_PAGES_ITEM );

		// Save position of this item
		m_iRedactPagesCounterItem = nCounterItemPosition;
	}

	// Determine if Redaction By Documents rules are licensed - requires full RDT license
	// (But if in read-only mode, always show)
	if (m_bReadOnly || isRdtLicensed())
	{
		// Add this item next
		nCounterItemPosition = m_CounterList.InsertItem( nCounterItemPosition + 1, "" );
		m_CounterList.SetItemText( nCounterItemPosition, giNAME_LIST_COLUMN, 
			"ID Shield - Redaction (By Document)" );
		m_CounterList.SetItemData( nCounterItemPosition, giREDACTION_DOCS_ITEM );

		// Save position of this item
		m_iRedactDocsCounterItem = nCounterItemPosition;
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesPage::validateSerialList( const string &strSerialList )
{
	try	
	{
		// Attempt to convert strSerialList to a range on numbers in the same way CRuleSet will to
		// ensure a valid list.
		vector<DWORD> vecSerialNumbers;
		addRangeToVector(vecSerialNumbers, strSerialList);
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uex2("ELI12020", "Invalid Serial Number list.", ue);
		throw uex2;
		// if any exceptions are thrown it is because a serial number list is not valid.
	}
	// no exceptions means serial numbers are valid
}
//-------------------------------------------------------------------------------------------------
bool CRuleSetPropertiesPage::isCounterAvailable(int nCounterItem, bool &rbIsCounterChecked)
{
	if (nCounterItem != -1)
	{
		rbIsCounterChecked = (m_CounterList.GetCheck(nCounterItem) == BST_CHECKED);
		return true;
	}
	
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CRuleSetPropertiesPage::isRdtLicensed()
{
	return LicenseManagement::isLicensed(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS);
}
//-------------------------------------------------------------------------------------------------