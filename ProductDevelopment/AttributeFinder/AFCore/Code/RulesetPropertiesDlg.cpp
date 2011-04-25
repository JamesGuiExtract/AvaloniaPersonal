// RuleSetPropertiesDlg.cpp : implementation file
//

#include "stdafx.h"
#include "afcore.h"
#include "RuleSetPropertiesDlg.h"

#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

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
// CRuleSetPropertiesDlg dialog
//-------------------------------------------------------------------------------------------------
CRuleSetPropertiesDlg::CRuleSetPropertiesDlg(UCLID_AFCORELib::IRuleSetPtr ipRuleSet,
											 CWnd* pParent /*=NULL*/)
	: CDialog(CRuleSetPropertiesDlg::IDD, pParent), m_ipRuleSet(ipRuleSet)
{
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_COUNTER_LIST, m_CounterList);
	DDX_Control(pDX, IDC_CHECK_INTERNAL_USE_ONLY, m_checkboxForInternalUseOnly);
	DDX_Control(pDX, IDC_SERIAL_NUMBERS, m_editKeySerialNumbers);
	DDX_Control(pDX, IDC_CHECK_SWIPING_RULE, m_checkSwipingRule);
	DDX_Control(pDX, IDOK, m_buttonOk);
	DDX_Control(pDX, IDCANCEL, m_buttonCancel);
}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CRuleSetPropertiesDlg, CDialog)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_COUNTER_LIST, &CRuleSetPropertiesDlg::OnCounterListItemChanged)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CRuleSetPropertiesDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CRuleSetPropertiesDlg::OnInitDialog() 
{
	try
	{
		CDialog::OnInitDialog();
		
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
void CRuleSetPropertiesDlg::OnOK() 
{
	try
	{
		int iCheckBoxState;

		// update the state of the Indexing and Pagination counters
		if (m_iIndexingCounterItem != -1)
		{
			iCheckBoxState = m_CounterList.GetCheck( m_iIndexingCounterItem );
			m_ipRuleSet->UseIndexingCounter = (iCheckBoxState == BST_CHECKED) 
				? VARIANT_TRUE : VARIANT_FALSE;
		}

		if (m_iPaginationCounterItem != -1)
		{
			iCheckBoxState = m_CounterList.GetCheck( m_iPaginationCounterItem );
			m_ipRuleSet->UsePaginationCounter = (iCheckBoxState == BST_CHECKED) 
				? VARIANT_TRUE : VARIANT_FALSE;
		}

		// Validate one or the other of the redaction checkboxes (P16 #2198)
		if( m_iRedactPagesCounterItem != -1 && 
			m_iRedactDocsCounterItem != -1 && 
			m_CounterList.GetCheck( m_iRedactPagesCounterItem ) == BST_CHECKED && 
			m_CounterList.GetCheck( m_iRedactDocsCounterItem ) == BST_CHECKED )
		{
			UCLIDException ue("ELI14499", "Cannot select redaction by pages and documents!");
			ue.display();
			return;
		}

		// Update state of the appropriate Redaction counter
		if (m_iRedactPagesCounterItem != -1)
		{
			iCheckBoxState = m_CounterList.GetCheck( m_iRedactPagesCounterItem );
			m_ipRuleSet->UsePagesRedactionCounter = (iCheckBoxState == BST_CHECKED) 
				? VARIANT_TRUE : VARIANT_FALSE;
		}

		if (m_iRedactDocsCounterItem != -1)
		{
			iCheckBoxState = m_CounterList.GetCheck( m_iRedactDocsCounterItem );
			m_ipRuleSet->UseDocsRedactionCounter = (iCheckBoxState == BST_CHECKED) 
				? VARIANT_TRUE : VARIANT_FALSE;
		}
		
		// Get the serial number list
		CString czSerialText;
		m_editKeySerialNumbers.GetWindowText(czSerialText);

		// Set focus to serial number list so if list not valid it will have the focus
		m_editKeySerialNumbers.SetFocus();
		validateSerialList( LPCTSTR(czSerialText) );

		// Serial number list is valid so save in the ruleset
		m_ipRuleSet->KeySerialList = _bstr_t(czSerialText);
		
		// update the value related to the checkbox for internal use
		bool bChecked = m_checkboxForInternalUseOnly.GetCheck() == BST_CHECKED;
		m_ipRuleSet->ForInternalUseOnly = asVariantBool(bChecked);

		// Store whether this is a swiping rule
		bChecked = m_checkSwipingRule.GetCheck() == BST_CHECKED;
		m_ipRuleSet->IsSwipingRule = asVariantBool(bChecked);

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11553")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesDlg::OnCounterListItemChanged(NMHDR* pNMHDR, LRESULT* pResult)
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
void CRuleSetPropertiesDlg::hideCheckboxes()
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
	m_editKeySerialNumbers.GetWindowRect(&editRect);
	ScreenToClient(&editRect);

	// Calculate the distance of empty space that should be removed
	long lDelta = checkRect.bottom - editRect.bottom;

	// Move the OK button up to fill the empty space
	RECT okRect = {0};
	m_buttonOk.GetWindowRect(&okRect);
	ScreenToClient(&okRect);
	okRect.top -= lDelta;
	okRect.bottom -= lDelta;
	m_buttonOk.MoveWindow(&okRect);

	// Move the cancel button up to fill the empty space
	RECT cancelRect = {0};
	m_buttonCancel.GetWindowRect(&cancelRect);
	ScreenToClient(&cancelRect);
	cancelRect.top -= lDelta;
	cancelRect.bottom -= lDelta;
	m_buttonCancel.MoveWindow(&cancelRect);

	// Shrink the dialog by the amount of empty space
	RECT mainRect = {0};
	GetWindowRect(&mainRect);
	mainRect.bottom -= lDelta;
	MoveWindow(&mainRect);
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesDlg::setupCounterList()
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
	// [FlexIDSCore #3059]
	int nCounterItemPosition = giINDEXING_ITEM;
	if (LicenseManagement::isLicensed( gnFLEXINDEX_RULE_WRITING_OBJECTS ))
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
	if (isRdtLicensed())
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
	if (LicenseManagement::isLicensed( gnIDSHIELD_RULE_WRITING_OBJECTS ))
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
	if (isRdtLicensed())
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
void CRuleSetPropertiesDlg::validateSerialList( const string &strSerialList )
{
	
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strSerialList, ',', vecTokens);
	
	long i;
	try	
	{
		long nNumSerials = vecTokens.size();
		for ( i = 0; i < nNumSerials; i++ )
		{
			DWORD dwSN;
			dwSN = asUnsignedLong(trim(vecTokens[i], " ", " "));
		}
	}
	catch (...)
	{
		UCLIDException ue("ELI12020", "Invalid Serial Number in list.");
		ue.addDebugInfo( "Serial Number List", strSerialList );
		ue.addDebugInfo( "Invalid Item", vecTokens[i] );
		throw ue;
		// if any exceptions are thrown it is because a serial number is not valid
	}
	// no exceptions means serial numbers are valid
}
//-------------------------------------------------------------------------------------------------
bool CRuleSetPropertiesDlg::isRdtLicensed()
{
	return LicenseManagement::isLicensed(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS);
}
//-------------------------------------------------------------------------------------------------