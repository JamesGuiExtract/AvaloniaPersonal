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

//-------------------------------------------------------------------------------------------------
// CRuleSetPropertiesDlg dialog
//-------------------------------------------------------------------------------------------------
CRuleSetPropertiesDlg::CRuleSetPropertiesDlg(UCLID_AFCORELib::IRuleSetPtr ipRuleSet,
											 CWnd* pParent /*=NULL*/)
	: CDialog(CRuleSetPropertiesDlg::IDD, pParent), m_ipRuleSet(ipRuleSet)
{
	//{{AFX_DATA_INIT(CRuleSetPropertiesDlg)
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CRuleSetPropertiesDlg)
	DDX_Control(pDX, IDC_COUNTER_LIST, m_CounterList);
	DDX_Control(pDX, IDC_CHECK_INTERNAL_USE_ONLY, m_checkboxForInternalUseOnly);
	DDX_Control(pDX, IDC_SERIAL_NUMBERS, m_editKeySerialNumbers);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CRuleSetPropertiesDlg, CDialog)
	//{{AFX_MSG_MAP(CRuleSetPropertiesDlg)
	ON_NOTIFY(NM_CLICK, IDC_COUNTER_LIST, &CRuleSetPropertiesDlg::OnNMClickCounterList)
	//}}AFX_MSG_MAP
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

		// Update the internal use checkbox + disable without full RDT license [FlexIDSCore #3062]
		m_checkboxForInternalUseOnly.SetCheck( asMFCBool( m_ipRuleSet->ForInternalUseOnly ) );
		m_checkboxForInternalUseOnly.EnableWindow( asMFCBool( 
			LicenseManagement::sGetInstance().isLicensed( gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS ) ) );
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
		validateSerialList( czSerialText.operator LPCTSTR() );

		// Serial number list is valid so save in the ruleset
		m_ipRuleSet->KeySerialList = _bstr_t(czSerialText);
		
		// update the value related to the checkbox for internal use
		iCheckBoxState = m_checkboxForInternalUseOnly.GetCheck();
		m_ipRuleSet->ForInternalUseOnly = (iCheckBoxState == TRUE) ? VARIANT_TRUE : VARIANT_FALSE;

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11553")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetPropertiesDlg::OnNMClickCounterList(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		////////////////////////////
		// Check for check box click
		////////////////////////////
		// Retrieve position of click
		CPoint	p;
		GetCursorPos( &p );
		m_CounterList.ScreenToClient( &p );

		UINT	uiFlags;
		int		iIndex = m_CounterList.HitTest( p, &uiFlags );

		// Was the click on the checkbox?
		// Note: if the item is clicked on anywhere but the check box,
		// the flag contains LVHT_ONITEMSTATEICON, LVHT_ONITEMICON and 
		// LVHT_ONITEMLABEL. If the item is clicked on only the check box
		// part, the flag will only contain LVHT_ONITEMSTATEICON 
		if (iIndex >= 0
			&& (uiFlags & LVHT_ONITEMSTATEICON)
			&& !(uiFlags & LVHT_ONITEMICON)
			&& !(uiFlags & LVHT_ONITEMLABEL))
		{
			// Retrieve the Item Data for the clicked item
			int iItemData = m_CounterList.GetItemData( iIndex );

			// If the click was either of the redaction checkboxes make sure the other one
			// is un-checked.
			if( iItemData == giREDACTION_PAGES_ITEM )
			{
				// User checked redaction by pages, uncheck redaction by docs
				if (m_iRedactDocsCounterItem != -1)
				{
					// Redaction by document exists, make sure it is unchecked
					if(! m_CounterList.SetCheck( m_iRedactDocsCounterItem, 0) )
					{
						// If there is an error, throw an exception
						UCLIDException ue("ELI14497", "Invalid redaction selection!");
						ue.addDebugInfo("Un-checking:", "Redaction Docs");
						ue.display();
					}
				}
			}
			
			if( iItemData == giREDACTION_DOCS_ITEM )
			{
				// User checked redaction by docs, uncheck redaction by pages
				if (m_iRedactPagesCounterItem != -1)
				{
					// Redaction by page exists, make sure it is unchecked
					if(! m_CounterList.SetCheck( m_iRedactPagesCounterItem, 0) )
					{
						// If there is an error, throw an exception
						UCLIDException ue("ELI14498", "Invalid redaction selection!");
						ue.addDebugInfo("Un-checking:", "Redaction Pages");
						ue.display();
					}
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
	if (LicenseManagement::sGetInstance().isLicensed( gnFLEXINDEX_RULE_WRITING_OBJECTS ))
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
	if (LicenseManagement::sGetInstance().isLicensed( gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS ))
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
	if (LicenseManagement::sGetInstance().isLicensed( gnIDSHIELD_RULE_WRITING_OBJECTS ))
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
	if (LicenseManagement::sGetInstance().isLicensed( gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS ))
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
