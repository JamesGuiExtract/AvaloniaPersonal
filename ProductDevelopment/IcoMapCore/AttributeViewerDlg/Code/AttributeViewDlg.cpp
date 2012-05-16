//===========================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AttributeViewDlg.cpp
//
// PURPOSE:	To provide edit and view capabilities to collected Feature 
//				attributes.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//===========================================================================

#include "stdafx.h"
#include "resource.h"
#include "AttributeViewDlg.h"

#include "PartDlg.h"
#include "LineDlg.h"
#include "TransferDlg.h"
#include "CfgAttributeViewer.h"

#include <CurveCalculationEngineImpl.h>
#include <CCEHelper.h>
#include <CurveCalculatorDlg.h>
#include <TemporaryResourceOverride.h>
#include <IcoMapOptions.h>
#include <cpputil.h>
#include <StringTokenizer.h>
#include <ValueRestorer.h>
#include <TPPoint.h>
#include <TemporaryFileName.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

// Define item types for the attribute lists
typedef	enum EItemType
{
	kInvalidType = 0,
	kStartPointType,
	kLineType,
	kCurveType
};

// Data structure for each item in a List
typedef	struct tagITEMINFO
{
	EItemType	eType;
	bool		bMarkForDelete;
	PTTVM		mapData;
} ITEMINFO;

// Global variable
extern HINSTANCE gAVDllResource;

// Column Heading IDs
#define	ID_LIST_COLUMN					0
#define	PART_LIST_COLUMN				1
#define	TYPE_LIST_COLUMN				2
#define	LINEBEARING_LIST_COLUMN			3
#define	LINEDISTANCE_LIST_COLUMN		4
#define	CHORDBEARING_LIST_COLUMN		5
#define	CHORDLENGTH_LIST_COLUMN			6
#define	RADIUS_LIST_COLUMN				7
#define	STARTPOINT_X_LIST_COLUMN		8			// Hidden column
#define	STARTPOINT_Y_LIST_COLUMN		9			// Hidden column

// Dialog size bounds
const int giAVDLG_MIN_WIDTH	= 350;
const int giAVDLG_MIN_TALL_HEIGHT = 300;
const int giAVDLG_MIN_SHORT_HEIGHT = 200;

const int SEGMENT_NUMBER_LEN = 9;
const int SEGMENT_TYPE_LEN = 9;
const string SPACE_IN_BETWEEN = "     ";

//-------------------------------------------------------------------------------------------------
// CAttributeViewDlg dialog
//-------------------------------------------------------------------------------------------------
CAttributeViewDlg::CAttributeViewDlg(CfgAttributeViewer* pCfg, 
									 IUCLDFeaturePtr ptrCurrentFeature, 
									 IUCLDFeaturePtr ptrOriginalFeature, 
									 bool bMakeCurrReadOnly, 
									 bool bMakeOrigReadOnly,
									 bool bCanStoreOriginalAttributes,
									 CWnd* pParent /*=NULL*/)
	: CDialog(CAttributeViewDlg::IDD, pParent),
	m_pCfg(pCfg),
	m_ptrCurrFeature(ptrCurrentFeature),
	m_ptrOrigFeature(ptrOriginalFeature),
	m_bShowOriginalList(false),
	m_bCurrentIsReadOnly(bMakeCurrReadOnly),
	m_bOriginalIsReadOnly(bMakeOrigReadOnly),
	m_bCanStoreOriginalAttributes(bCanStoreOriginalAttributes),
	m_sizeSmall(0),
	m_sizeLarge(0),
	m_bInitialized(false),
	m_bOnlyViewItem(false),
	m_bOriginalDefined(false),
	m_bCurrentFeatureValid(false),
	m_bOriginalFeatureValid(false),
	m_bCurrentFeatureEmpty(true),
	m_bOriginalFeatureEmpty(true),
	m_iListWithFocus(0),
	m_iControlSpace(10),
	m_iCurrentLastSelected(-1),
	m_iOriginalLastSelected(-1)
{
	try
	{
		EnableAutomation();
		
		// Initialize curve calculation engine object
		m_pEngine = new CurveCalculationEngineImpl();
		ASSERT_RESOURCE_ALLOCATION( "ELI02827", (m_pEngine != NULL) );
		
		// Load the icon from the correct DLL resource
		TemporaryResourceOverride temporaryResourceOverride( gAVDllResource );
		m_hIcon = AfxGetApp()->LoadIcon( IDI_AVDLG_ICON );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12488")

	//{{AFX_DATA_INIT(CAttributeViewDlg)
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
CAttributeViewDlg::~CAttributeViewDlg()
{
	// Clean up engine object
	if (m_pEngine)
	{
		delete m_pEngine;
		m_pEngine = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CAttributeViewDlg::DestroyWindow() 
{ 
	try
	{
		// Release ITEMINFO data from Current Attributes List
		int iCount = m_listCurrent.GetItemCount();
		if (iCount > 0)
		{
			for (int i = 0; i < iCount; i++)
			{
				// Retrieve this data structure
				ITEMINFO*	pData = (ITEMINFO *)m_listCurrent.GetItemData( i );

				// Release the memory
				if (pData != NULL)
				{
					delete pData;
					pData = NULL;
				}
			}
		}

		// Release ITEMINFO data from Original Attributes List
		iCount = m_listOriginal.GetItemCount();
		if (iCount > 0)
		{
			for (int i = 0; i < iCount; i++)
			{
				// Retrieve this data structure
				ITEMINFO*	pData = (ITEMINFO *)m_listOriginal.GetItemData( i );

				// Release the memory
				if (pData != NULL)
				{
					delete pData;
					pData = NULL;
				}
			}
		}

		return CDialog::DestroyWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01994")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnFinalRelease()
{
	// When the last reference for an automation object is released
	// OnFinalRelease is called.  The base class will automatically
	// deletes the object.  Add additional cleanup required for your
	// object before calling the base class.

	CDialog::OnFinalRelease();
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAttributeViewDlg)
	DDX_Control(pDX, IDC_STATIC_CURR, m_staticCurr);
	DDX_Control(pDX, IDOK, m_ok);
	DDX_Control(pDX, IDCANCEL, m_cancel);
	DDX_Control(pDX, IDC_STATIC_ORIG, m_staticOrig);
	DDX_Control(pDX, IDC_STATIC_DISPLAY, m_staticDisplay);
	DDX_Control(pDX, IDC_LIST2, m_listOriginal);
	DDX_Control(pDX, IDC_LIST1, m_listCurrent);
	DDX_Control(pDX, IDC_SHOWHIDE, m_showHide);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttributeViewDlg, CDialog)
	//{{AFX_MSG_MAP(CAttributeViewDlg)
	ON_BN_CLICKED(IDC_SHOWHIDE, OnShowhide)
	ON_WM_SIZE()
	ON_COMMAND(ID_INSERT_CURVE, OnInsertCurve)
	ON_COMMAND(ID_INSERT_LINE, OnInsertLine)
	ON_COMMAND(ID_INSERT_PART, OnInsertPart)
	ON_COMMAND(ID_ITEM_EDIT, OnItemEdit)
	ON_COMMAND(ID_ITEM_TRANSFER, OnItemTransfer)
	ON_COMMAND(ID_ITEM_VIEW, OnItemView)
	ON_COMMAND(ID_ITEM_DELETE, OnItemDelete)
	ON_COMMAND(ID_ITEM_CLOSURE, OnItemClosure)
	ON_NOTIFY(NM_SETFOCUS, IDC_LIST1, OnSetfocusList1)
	ON_NOTIFY(NM_SETFOCUS, IDC_LIST2, OnSetfocusList2)
	ON_NOTIFY(NM_CLICK, IDC_LIST1, OnClickList1)
	ON_NOTIFY(NM_CLICK, IDC_LIST2, OnClickList2)
	ON_NOTIFY(NM_KILLFOCUS, IDC_LIST1, OnKillfocusList1)
	ON_NOTIFY(NM_KILLFOCUS, IDC_LIST2, OnKillfocusList2)
	ON_NOTIFY(LVN_KEYDOWN, IDC_LIST1, OnKeydownList1)
	ON_NOTIFY(LVN_KEYDOWN, IDC_LIST2, OnKeydownList2)
	ON_WM_CONTEXTMENU()
	ON_NOTIFY(NM_DBLCLK, IDC_LIST1, OnDblclkList1)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST2, OnDblclkList2)
	ON_WM_GETMINMAXINFO()
	//}}AFX_MSG_MAP
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTW, 0, 0xFFFF, OnToolTipText)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTA, 0, 0xFFFF, OnToolTipText)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BEGIN_DISPATCH_MAP(CAttributeViewDlg, CDialog)
	//{{AFX_DISPATCH_MAP(CAttributeViewDlg)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_DISPATCH_MAP
END_DISPATCH_MAP()
//-------------------------------------------------------------------------------------------------
// Note: we add support for IID_IAttributeViewDlg to support typesafe binding
//  from VBA.  This IID must match the GUID that is attached to the 
//  dispinterface in the .ODL file.

// {623972D4-94F3-417B-A98B-14D5CF72219A}
static const IID IID_IAttributeViewDlg =
{ 0x623972d4, 0x94f3, 0x417b, { 0xa9, 0x8b, 0x14, 0xd5, 0xcf, 0x72, 0x21, 0x9a } };

BEGIN_INTERFACE_MAP(CAttributeViewDlg, CDialog)
	INTERFACE_PART(CAttributeViewDlg, IID_IAttributeViewDlg, Dispatch)
END_INTERFACE_MAP()

//-------------------------------------------------------------------------------------------------
// CAttributeViewDlg message handlers
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnShowhide() 
{
	try
	{
		// Change the setting and store it
		m_bShowOriginalList = !m_bShowOriginalList;
		m_pCfg->setShowStored( m_bShowOriginalList );

		// Update the button text
		shiftView();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01997")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	try
	{
		// Minimum width to allow display of buttons
		lpMMI->ptMinTrackSize.x = giAVDLG_MIN_WIDTH;

		// Minimum height based on visibility of original attributes
		if (m_bShowOriginalList)
		{
			lpMMI->ptMinTrackSize.y = giAVDLG_MIN_TALL_HEIGHT;
		}
		else
		{
			lpMMI->ptMinTrackSize.y = giAVDLG_MIN_SHORT_HEIGHT;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01999")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnCancel() 
{
	try
	{
		// Retrieve size and position
		CRect	rect;
		GetWindowRect( &rect );

		// Store size and position
		m_pCfg->setWindowPos( rect.left, rect.top );
		m_pCfg->setWindowSize( rect.Width(), rect.Height() );

		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02001")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnOK() 
{
	try
	{
		CWaitCursor wait;

		int		iErrorID = 0;
		CString	zError;

		////////////////////////////////
		// Validate Current Feature data
		////////////////////////////////

		// Check if the Current Feature has parts
		if (m_listCurrent.GetItemCount() > 1)		// always has a Placeholder
		{
			// Validate the Current Feature data
			if (!validateFeature( &m_listCurrent, &iErrorID ))
			{
				// Load the error string from the string table
				if (!zError.LoadString( iErrorID ))
				{
					// Unable to load validation error string
					MessageBox( "Unable to load string describing error validating the current feature.", 
						"Validation Error", MB_ICONEXCLAMATION | MB_OK );
					return;
				}
				else
				{
					MessageBox( zError, "Validation Error", 
						MB_ICONEXCLAMATION | MB_OK );
					return;
				}
			}
			// Feature is valid!
			else
			{
				// See if Current Feature allowed changes
				if (m_bCurrentIsReadOnly)
				{
					// Check for NULL feature
					// NOTE: This is unexpected, but now safe
					if (m_ptrCurrFeature == NULL)
					{
						// Declare a new Feature
						IUCLDFeaturePtr	ptrNewFeature;

						// Default to polyline type
						ptrNewFeature->setFeatureType( kPolyline );

						// Point to the new Feature
						m_ptrFinalCurrFeature = ptrNewFeature;
					}
					else
					{
						// Just point to the feature provided in construction
						m_ptrFinalCurrFeature = m_ptrCurrFeature;
					}
				}
				else
				{
					// Calculate updated Current Feature data
					m_ptrFinalCurrFeature = getFeature( 1 );
				}

				// Set validity flag
				m_bCurrentFeatureValid = true;

				// Clear the Feature empty flag
				m_bCurrentFeatureEmpty = false;
			}
		}
		else
		{
			// Provide confirmation dialog to user about empty Feature
			CString	zText;
			zText.Format( "All attributes have been removed.  Please confirm acceptance of an empty feature." );
			int iResult = MessageBox( zText.operator LPCTSTR(), 
				"Confirm Empty Feature", 
				MB_ICONQUESTION | MB_DEFBUTTON2 | MB_YESNO );

			// Check return from Message Box
			if (iResult == IDNO)
			{
				// Just return to the dialog
				return;
			}

			// User accepted the empty feature
			// Current Feature is still valid
			m_bCurrentFeatureValid = true;
		}
		
		/////////////////////////////////
		// Validate Original Feature data
		/////////////////////////////////

		// Check if the Original Feature has parts
		if (m_listOriginal.GetItemCount() > 1)		// always has a Placeholder
		{
			// Validate the Original Feature data
			if (!validateFeature( &m_listOriginal, &iErrorID ))
			{
				// Load the error string from the string table
				if (!zError.LoadString( iErrorID ))
				{
					// Unable to load validation error string
					MessageBox( "Unable to load string describing error validating the stored feature.", 
						"Validation Error", MB_ICONEXCLAMATION | MB_OK );
					return;
				}
				else
				{
					MessageBox( zError, "Validation Error", 
						MB_ICONEXCLAMATION | MB_OK );
					return;
				}
			}
			// Feature is valid!
			else
			{
				// See if Original Feature allowed changes
				if (m_bOriginalIsReadOnly || !m_bCanStoreOriginalAttributes)
				{
					// Check for NULL feature
					if (m_ptrOrigFeature == NULL)
					{
						// Declare a new Feature
						IUCLDFeaturePtr	ptrNewFeature;

						// Check type of Current Feature
						if (m_ptrCurrFeature != NULL)
						{
							// Default to type in Current Feature
							ptrNewFeature->setFeatureType( 
								m_ptrCurrFeature->getFeatureType() );
						}
						else
						{
							// Default to polyline type
							ptrNewFeature->setFeatureType( kPolyline );
						}

						// Point to the new Feature
						m_ptrFinalCurrFeature = ptrNewFeature;
					}
					else
					{
						// Just point to the feature provided in construction
						m_ptrFinalOrigFeature = m_ptrOrigFeature;
					}
				}
				else
				{
					// Calculate updated Original Feature data
					m_ptrFinalOrigFeature = getFeature( 2 );
				}

				// Set validity flag
				m_bOriginalFeatureValid = true;

				// Clear the Feature empty flag
				m_bOriginalFeatureEmpty = false;
			}
		}
		else
		{
			// Original Feature is empty, but still valid
			m_bOriginalFeatureValid = true;
		}

		// Retrieve size and position
		CRect	rect;
		GetWindowRect( &rect );

		// Store size and position
		m_pCfg->setWindowPos( rect.left, rect.top );
		m_pCfg->setWindowSize( rect.Width(), rect.Height() );

		// Call base class method
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02003")
}
//-------------------------------------------------------------------------------------------------
BOOL CAttributeViewDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride( gAVDllResource );

	try
	{
		CDialog::OnInitDialog();

		// Setup icons
		SetIcon(m_hIcon, TRUE);
		SetIcon(m_hIcon, FALSE);

		// Set flag
		m_bInitialized = true;

		m_bShowOriginalList = m_pCfg->getShowStored();

		// Retrieve persistent size and position of dialog
		long	lLeft = 0;
		long	lTop = 0;
		long	lWidth = 0;
		long	lHeight = 0;
		m_pCfg->getWindowPos( lLeft, lTop );
		m_pCfg->getWindowSize( lWidth, lHeight );

		////////////////////
		// Check size limits
		////////////////////
		// Minimum height based on visibility of original attributes
		if (m_bShowOriginalList)
		{
			if (lHeight < giAVDLG_MIN_TALL_HEIGHT)
			{
				lHeight = giAVDLG_MIN_TALL_HEIGHT;
			}
		}
		else
		{
			if (lHeight < giAVDLG_MIN_SHORT_HEIGHT)
			{
				lHeight = giAVDLG_MIN_SHORT_HEIGHT;
			}
		}

		// Minimum width to allow display of buttons
		if (lWidth < giAVDLG_MIN_WIDTH)
		{
			lWidth = giAVDLG_MIN_WIDTH;
		}

		// Adjust window position based on retrieved settings
		MoveWindow( lLeft, lTop, lWidth, lHeight, TRUE );

		// Enable full row selection and grid lines
		m_listCurrent.SetExtendedStyle( LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );
		if (m_bCanStoreOriginalAttributes)
		{
			m_listOriginal.SetExtendedStyle( LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );
		}
		else
		{
			// No gridlines if data not allowed
			m_listOriginal.SetExtendedStyle( LVS_EX_FULLROWSELECT );
		}

		// Create the toolbar
		createToolBar();

		// Store sizes for large and small versions of dialog
		getDialogSizes( m_bShowOriginalList );

		// Prepare header controls for the lists
		prepareHeaders();

		// Parse the Attributes
		parseFeature( m_ptrCurrFeature, &m_listCurrent );
		m_bOriginalDefined = parseFeature( m_ptrOrigFeature, &m_listOriginal );

		// Add text message to Original attributes if it can't be stored
		if (!m_bCanStoreOriginalAttributes)
		{
			m_listOriginal.SetItemText( 0, 0, 
				"IcoMap will not be able to store original attributes for the selected feature." );
		}

		// Add item IDs and Part numbers
		updateView( &m_listCurrent );
		updateView( &m_listOriginal );

		// Set button text
		shiftView( true );

		// Display initial toolbar settings
		doToolBarUpdates();

		return TRUE;  // return TRUE unless you set the focus to a control
					  // EXCEPTION: OCX Property Pages should return FALSE
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02006")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CAttributeViewDlg::OnToolTipText(UINT id, NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride( gAVDllResource );

	try
	{
		// Check Notification code
		ASSERT(pNMHDR->code == TTN_NEEDTEXTA || pNMHDR->code == TTN_NEEDTEXTW);

		// Let top level routing frame handle the message
		if (GetRoutingFrame() != NULL) 
		{
			return FALSE;
		}

		// Also handle UNICODE versions of the message
		TOOLTIPTEXTA* pTTTA = (TOOLTIPTEXTA*)pNMHDR;
		TOOLTIPTEXTW* pTTTW = (TOOLTIPTEXTW*)pNMHDR;
		CString strTipText;
		UINT nID = pNMHDR->idFrom;

		if (pNMHDR->code == TTN_NEEDTEXTA && (pTTTA->uFlags & TTF_IDISHWND) ||
			pNMHDR->code == TTN_NEEDTEXTW && (pTTTW->uFlags & TTF_IDISHWND))
		{
			// idFrom is actually the HWND of the tool 
			nID = ::GetDlgCtrlID((HWND)nID);
		}

		// Skip separator items
		if (nID != 0)
		{
			// Retrieve text from stringtable
			strTipText.LoadString( nID );

			if (pNMHDR->code == TTN_NEEDTEXTA)
			{
				lstrcpyn( pTTTA->szText, strTipText, sizeof(pTTTA->szText) );
			}
			else
			{
				_mbstowcsz( pTTTW->szText, strTipText, sizeof(pTTTW->szText) );
			}

			*pResult = 0;

			// Raise the tooltip window above other popup windows
			::SetWindowPos( pNMHDR->hwndFrom, HWND_TOP, 0, 0, 0, 0, SWP_NOACTIVATE |
				SWP_NOSIZE | SWP_NOMOVE | SWP_NOOWNERZORDER ); 
			  
			return TRUE;
		}

		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02009")

	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::createToolBar()
{
	// Create the toolbar object with desired options
	if (m_toolBar.CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | 
		CBRS_TOP | CBRS_FLYBY | CBRS_SIZE_DYNAMIC | CBRS_BORDER_BOTTOM) )
	{
		// Load the bitmap resource
		m_toolBar.LoadToolBar( IDR_AVTOOLBAR );
	}
	
	// Add tooltips to style
	m_toolBar.SetBarStyle( m_toolBar.GetBarStyle() | CBRS_TOOLTIPS | 
		CBRS_FLYBY | CBRS_SIZE_DYNAMIC | CBRS_BORDER_BOTTOM );
	
	// Position the toolbar in the dialog
	RepositionBars( AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0 );
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::canEditSelection(CListCtrl* pList)
{
	bool	bCan = false;
	int		iNumSelected = -1;
	
	// Check selection count
	if (pList != NULL)
	{
		iNumSelected = pList->GetSelectedCount();
	}
	
	// Permission is based on number of items selected
	switch( iNumSelected )
	{
		// No list has focus
	case -1:
		bCan = false;
		m_bOnlyViewItem = false;
		break;
		
		// No items are selected
	case 0:
		bCan = false;
		m_bOnlyViewItem = false;
		break;
		
		// One item is selected
	case 1:
		{
			// Determine index of the selected item
			POSITION	pos = pList->GetFirstSelectedItemPosition();
			int			iItem = 0;
			if (pos == NULL)
			{
				// Throw exception - Cannot Edit without a selected item
				UCLIDException uclidException( "ELI01804", 
					"Cannot edit without a selected item." );
				throw uclidException;
			}
			else
			{
				// Get index of selected item
				iItem = pList->GetNextSelectedItem( pos );
			}
			
			// Enable of Edit means Edit
			m_bOnlyViewItem = false;
			
			// Edit enabled except for Placeholder item
			if (((ITEMINFO *)pList->GetItemData( iItem ))->eType != kInvalidType)
			{
				bCan = true;
			}
			
			// Check read-only status of this list
			if ((!m_bCurrentIsReadOnly && (m_iListWithFocus == 1)) ||
				(!m_bOriginalIsReadOnly && (m_iListWithFocus == 2)))
			{
				// Not read-only, enable of Edit means Edit
				m_bOnlyViewItem = false;
			}
			else
			{
				// Is read-only, enable of Edit means View
				m_bOnlyViewItem = true;
			}
		}
		break;
		
		// Two or more items are selected
	default:
		bCan = false;
		m_bOnlyViewItem = false;
		break;
	}
	
	// Return permission
	return bCan;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::canDeleteSelection(CListCtrl* pList)
{
	bool	bCan = false;
	bool	bPartsSelected = false;
	bool	bSegmentsSelected = false;
	int		iNumSelected = -1;
	
	// Check selection count
	if (pList != NULL)
	{
		iNumSelected = pList->GetSelectedCount();
	}
	
	// Permission is based on number of items selected
	switch( iNumSelected )
	{
		// No list has focus
	case -1:
		bCan = false;
		break;
		
		// No items are selected
	case 0:
		bCan = false;
		
		// Reset appropriate flag
		if (m_iListWithFocus == 1)
		{
			m_iCurrentLastSelected = -1;
		}
		else if (m_iListWithFocus == 2)
		{
			m_iOriginalLastSelected = -1;
		}
		break;
		
		// One or more items are selected
	default:
		// Only enable changes if appropriate input flag is clear
		if ((!m_bCurrentIsReadOnly && (m_iListWithFocus == 1)) ||
			(!m_bOriginalIsReadOnly && (m_iListWithFocus == 2)))
		{
			// Check to see if the placeholder item is selected
			int iLast = pList->GetItemCount();
			if (iLast > 0)
			{
				int iState = pList->GetItemState( iLast - 1, LVIS_SELECTED );
				if (iState && LVIS_SELECTED)
				{
					// Disable Delete if placeholder is selected
					bCan = false;
				}
				else
				{
					bCan = true;
				}
				
				// Step through selected items and determine last selected index
				POSITION	pos = pList->GetFirstSelectedItemPosition();
				int			iItem = 0;
				while (pos != NULL)
				{
					// Get index of selected item
					iItem = pList->GetNextSelectedItem( pos );
				}
				
				// Set appropriate flag
				if (m_iListWithFocus == 1)
				{
					m_iCurrentLastSelected = iItem;
				}
				else if (m_iListWithFocus == 2)
				{
					m_iOriginalLastSelected = iItem;
				}
			}
		}
		break;
	}
	
	// Return permission
	return bCan;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::canAppendPart(CListCtrl* pList, bool bContextMenu)
{
	bool	bCan = false;
	int		iNumSelected = -1;
	
	// Check selection count
	if (pList != NULL)
	{
		iNumSelected = pList->GetSelectedCount();
	}
	
	// Permission is based on number of items selected
	switch( iNumSelected )
	{
		// No list has focus
	case -1:
		bCan = false;
		break;
		
		// No items are selected
	case 0:
		// Only enable changes if appropriate input flag is clear
		if (!m_bCanStoreOriginalAttributes && (m_iListWithFocus == 2))
		{
			// Is read-only, disable Append
			bCan = false;
		}
		else
		{
			bCan = true;
		}
		break;
		
		// One item is selected
	case 1:
		// Only enable changes if appropriate input flag is clear
		if (!m_bCanStoreOriginalAttributes && (m_iListWithFocus == 2))
		{
			// Is read-only, disable Append
			bCan = false;
		}
		else if ((!m_bCurrentIsReadOnly && (m_iListWithFocus == 1)) ||
			(!m_bOriginalIsReadOnly && (m_iListWithFocus == 2)))
		{
			// Enable toolbar button anytime
			if( !bContextMenu )
			{
				bCan = true;
			}
			else
			{
				// Determine index of the selected item
				POSITION	pos = pList->GetFirstSelectedItemPosition();
				int			iItem = 0;
				if (pos == NULL)
				{
					// Throw exception - Cannot Append Part without a selected item
					UCLIDException uclidException( "ELI01815", 
						"Cannot append part without a selected item." );
					throw uclidException;
				}
				else
				{
					// Get index of selected item
					iItem = pList->GetNextSelectedItem( pos );
					
					// Get record type
					DWORD dwType = ((ITEMINFO *)pList->GetItemData( iItem ))->eType;
					
					// Can only append part via context menu 
					// when placeholder item is selected
					switch (dwType)
					{
					case kInvalidType:
						bCan = true;
						break;
						
					case kLineType:
					case kCurveType:
						bCan = false;
						break;
						
					default:
						bCan = false;
						break;
					}
				}
			}
		}
		break;
		
		// Two or more items are selected
	default:
		// Only enable changes if appropriate input flag is clear
		if ((!m_bCurrentIsReadOnly && (m_iListWithFocus == 1)) ||
			(!m_bOriginalIsReadOnly && (m_iListWithFocus == 2)))
		{
			if( !bContextMenu )
			{
				// Enable toolbar button anytime
				bCan = true;
			}
			else
			{
				// Disable context menu item
				bCan = false;
			}
		}
		break;
	}
	
	// Return permission
	return bCan;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::canInsertLine(CListCtrl* pList)
{
	bool	bCan = false;
	int		iNumSelected = -1;
	
	// Check selection count
	if (pList != NULL)
	{
		iNumSelected = pList->GetSelectedCount();
	}
	
	// Permission is based on number of items selected
	switch( iNumSelected )
	{
		// No list has focus
	case -1:
		bCan = false;
		break;
		
		// No items are selected
	case 0:
		bCan = false;
		break;
		
		// One item is selected
	case 1:
		// Only enable changes if appropriate input flag is clear
		if (!m_bCanStoreOriginalAttributes && (m_iListWithFocus == 2))
		{
			// Is read-only, disable Insert
			bCan = false;
		}
		else if ((!m_bCurrentIsReadOnly && (m_iListWithFocus == 1)) ||
			(!m_bOriginalIsReadOnly && (m_iListWithFocus == 2)))
		{
			// Determine index of the selected item
			POSITION	pos = pList->GetFirstSelectedItemPosition();
			int			iItem = 0;
			if (pos == NULL)
			{
				// Throw exception - Cannot Insert without a selected item
				UCLIDException uclidException( "ELI01805", 
					"Cannot insert line without a selected item." );
				throw uclidException;
			}
			else
			{
				// Get index of selected item
				iItem = pList->GetNextSelectedItem( pos );
			}
			
			// Insert enabled except at top of list
			if (iItem != 0)
			{
				bCan = true;
			}
		}
		break;
		
		// Two or more items are selected
	default:
		bCan = false;
		break;
	}
	
	// Return permission
	return bCan;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::canInsertCurve(CListCtrl* pList)
{
	bool	bCan = false;
	int		iNumSelected = -1;
	
	// Check selection count
	if (pList != NULL)
	{
		iNumSelected = pList->GetSelectedCount();
	}
	
	// Permission is based on number of items selected
	switch( iNumSelected )
	{
		// No list has focus
	case -1:
		bCan = false;
		break;
		
		// No items are selected
	case 0:
		bCan = false;
		break;
		
		// One item is selected
	case 1:
		// Only enable changes if appropriate input flag is clear
		if (!m_bCanStoreOriginalAttributes && (m_iListWithFocus == 2))
		{
			// Is read-only, disable Insert
			bCan = false;
		}
		else if ((!m_bCurrentIsReadOnly && (m_iListWithFocus == 1)) ||
			(!m_bOriginalIsReadOnly && (m_iListWithFocus == 2)))
		{
			// Determine index of the selected item
			POSITION	pos = pList->GetFirstSelectedItemPosition();
			int			iItem = 0;
			if (pos == NULL)
			{
				// Throw exception - Cannot Insert without a selected item
				UCLIDException uclidException( "ELI01806", 
					"Cannot insert curve without a selected item." );
				throw uclidException;
			}
			else
			{
				// Get index of selected item
				iItem = pList->GetNextSelectedItem( pos );
			}
			
			// Insert enabled except at top of list
			if (iItem != 0)
			{
				bCan = true;
			}
		}
		break;
		
		// Two or more items are selected
	default:
		bCan = false;
		break;
	}
	
	// Return permission
	return bCan;
}
//-------------------------------------------------------------------------------------------------
// NOTE: ID_ITEM_VIEW button has been removed from the toolbar.  11-29-2001
//
// View versus Edit functionality is now handled inside OnItemEdit() with 
// the help of m_bOnlyViewItem.  If this member is true, then dialog is 
// constructed in View mode.
void CAttributeViewDlg::doToolBarUpdates() 
{
	CListCtrl*	pList = NULL;
	
	/////////////////////////////////////
	// Check which list control has focus
	/////////////////////////////////////
	
	// Current Attributes
	if (m_iListWithFocus == 1)
	{
		// Determine number of items selected in current list
		pList = &m_listCurrent;
	}
	// Original Attributes
	else if (m_iListWithFocus == 2)
	{
		// Determine number of items selected in original list
		pList = &m_listOriginal;
	}
	
	// Enable or disable buttons
	m_toolBar.GetToolBarCtrl().EnableButton( ID_ITEM_EDIT, 
		canEditSelection( pList ) );
	m_toolBar.GetToolBarCtrl().EnableButton( ID_ITEM_DELETE, 
		canDeleteSelection( pList ) );
	m_toolBar.GetToolBarCtrl().EnableButton( ID_INSERT_PART, 
		canAppendPart( pList ) );
	m_toolBar.GetToolBarCtrl().EnableButton( ID_INSERT_LINE, 
		canInsertLine( pList ) );
	m_toolBar.GetToolBarCtrl().EnableButton( ID_INSERT_CURVE, 
		canInsertCurve( pList ) );
	
	BOOL bEnableTransfer = (m_bCurrentIsReadOnly && m_bOriginalIsReadOnly) 
		|| !m_bCanStoreOriginalAttributes ? FALSE : TRUE;
	// Transfer button is always enabled except when both
	// attribute sets are read-only OR if original attributes cannot be stored
	m_toolBar.GetToolBarCtrl().EnableButton(ID_ITEM_TRANSFER, bEnableTransfer);

	m_toolBar.GetToolBarCtrl().EnableButton(ID_ITEM_CLOSURE, m_bOriginalDefined ? TRUE : FALSE);
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::shiftView(bool bFirst) 
{
	// Get present size and position of dialog
	CRect	rect;
	GetWindowRect( &rect );
	
	// Only display Current Attributes
	if (!m_bShowOriginalList)
	{
		// Set appropriate button text
		m_showHide.SetWindowText( "Show &Stored Attributes >>" );
		
		// Hide the second static text object
		m_staticOrig.ShowWindow( SW_HIDE );
		
		// Hide the second list control
		m_listOriginal.ShowWindow( SW_HIDE );
		
		// Exclude resize if called from OnInitDialog()
		if (!bFirst)
		{
			// Compute height change
			int iChange = m_sizeLarge.cy - m_sizeSmall.cy;
			
			// Shrink dialog rectangle
			MoveWindow( rect.left, rect.top, rect.Width(), rect.Height() - iChange );
		}
	}
	// Also display Original Attributes
	else
	{
		// Set appropriate button text
		m_showHide.SetWindowText( "<< Hide &Stored Attributes" );
		
		// Show the second static text object
		m_staticOrig.ShowWindow( SW_SHOW );
		
		// Show the second list control
		m_listOriginal.ShowWindow( SW_SHOW );
		
		// Exclude resize if called from OnInitDialog()
		if (!bFirst)
		{
			// Compute height change
			int iChange = m_sizeLarge.cy - m_sizeSmall.cy;
			
			// Expand dialog rectangle
			MoveWindow( rect.left, rect.top, rect.Width(), rect.Height() + iChange );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::prepareHeaders() 
{
	// Get dimensions of control
	CRect	rect;
	m_listCurrent.GetClientRect( &rect );
	
	/////////////////////////////////////
	// Add 10 column headings to 1st list
	/////////////////////////////////////
	// General columns
	m_listCurrent.InsertColumn( ID_LIST_COLUMN, "ID", 
		LVCFMT_LEFT, 35, ID_LIST_COLUMN );
	m_listCurrent.InsertColumn( PART_LIST_COLUMN, "Part", 
		LVCFMT_LEFT, 35, PART_LIST_COLUMN );
	m_listCurrent.InsertColumn( TYPE_LIST_COLUMN, "Type", 
		LVCFMT_LEFT, 45, TYPE_LIST_COLUMN );
	
	// Line columns
	m_listCurrent.InsertColumn( LINEBEARING_LIST_COLUMN, "Direction", 
		LVCFMT_LEFT, 85, LINEBEARING_LIST_COLUMN );
	m_listCurrent.InsertColumn( LINEDISTANCE_LIST_COLUMN, "Distance", 
		LVCFMT_LEFT, 90, LINEDISTANCE_LIST_COLUMN );
	
	// Curve columns
	m_listCurrent.InsertColumn( CHORDBEARING_LIST_COLUMN, "Chord Direction", 
		LVCFMT_LEFT, 85, CHORDBEARING_LIST_COLUMN );
	m_listCurrent.InsertColumn( CHORDLENGTH_LIST_COLUMN, "Chord Length", 
		LVCFMT_LEFT, 90, CHORDLENGTH_LIST_COLUMN );
	m_listCurrent.InsertColumn( RADIUS_LIST_COLUMN, "Radius", 
		LVCFMT_LEFT, 90, RADIUS_LIST_COLUMN );
	
	// Start Point columns (Hidden)
	//		m_listCurrent.InsertColumn( STARTPOINT_X_LIST_COLUMN, "Start X", 
	//			LVCFMT_LEFT, 0, STARTPOINT_X_LIST_COLUMN );
	//		m_listCurrent.InsertColumn( STARTPOINT_Y_LIST_COLUMN, "Start Y", 
	//			LVCFMT_LEFT, 0, STARTPOINT_Y_LIST_COLUMN );
	
	/////////////////////////////////////
	// Add 10 column headings to 2nd list
	/////////////////////////////////////
	if (m_bCanStoreOriginalAttributes)
	{
		// General columns
		m_listOriginal.InsertColumn( ID_LIST_COLUMN, "ID", 
			LVCFMT_LEFT, 35, ID_LIST_COLUMN );
		m_listOriginal.InsertColumn( PART_LIST_COLUMN, "Part", 
			LVCFMT_LEFT, 35, PART_LIST_COLUMN );
		m_listOriginal.InsertColumn( TYPE_LIST_COLUMN, "Type", 
			LVCFMT_LEFT, 45, TYPE_LIST_COLUMN );
		
		// Line columns
		m_listOriginal.InsertColumn( LINEBEARING_LIST_COLUMN, "Direction", 
			LVCFMT_LEFT, 85, LINEBEARING_LIST_COLUMN );
		m_listOriginal.InsertColumn( LINEDISTANCE_LIST_COLUMN, "Distance", 
			LVCFMT_LEFT, 90, LINEDISTANCE_LIST_COLUMN );
		
		// Curve columns
		m_listOriginal.InsertColumn( CHORDBEARING_LIST_COLUMN, "Chord Direction", 
			LVCFMT_LEFT, 85, CHORDBEARING_LIST_COLUMN );
		m_listOriginal.InsertColumn( CHORDLENGTH_LIST_COLUMN, "Chord Length", 
			LVCFMT_LEFT, 90, CHORDLENGTH_LIST_COLUMN );
		m_listOriginal.InsertColumn( RADIUS_LIST_COLUMN, "Radius", 
			LVCFMT_LEFT, 90, RADIUS_LIST_COLUMN );
		
		// Start Point columns (hidden)
		//			m_listOriginal.InsertColumn( STARTPOINT_X_LIST_COLUMN, "Start X", 
		//				LVCFMT_LEFT, 0, STARTPOINT_X_LIST_COLUMN );
		//			m_listOriginal.InsertColumn( STARTPOINT_Y_LIST_COLUMN, "Start Y", 
		//				LVCFMT_LEFT, 0, STARTPOINT_Y_LIST_COLUMN );
	}
	else
	{
		// Just one column
		m_listOriginal.InsertColumn( ID_LIST_COLUMN, "Warning", 
			LVCFMT_LEFT, rect.Width(), ID_LIST_COLUMN );
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::updateView(CListCtrl* pList) 
{
		// Determine number of items in this list
	int	iCount = pList->GetItemCount();
	
	// Loop through each item in the list
	int iPartNumber = 0;
	DWORD	dwType = 0;
	CString	zID;
	CString	zType;
	for (int i = 0; i < iCount; i++)
	{
		// Retrieve the ITEMINFO data
		ITEMINFO*	pData = (ITEMINFO *)pList->GetItemData( i );
		
		// Determine Type of this item 
		dwType = pData->eType;
		
		// Update Part Number if this is a Start Point
		if (dwType == kStartPointType)
		{
			iPartNumber++;
			zType.Format( "%d", iPartNumber );
		}
		
		if (dwType != kInvalidType)
		{
			// Set item ID for this item
			zID.Format( "%d", i+1 );
			pList->SetItemText( i, ID_LIST_COLUMN, zID.operator LPCTSTR() );
			
			// Set Part Number text
			pList->SetItemText( i, PART_LIST_COLUMN, zType.operator LPCTSTR() );
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::parseFeature(IUCLDFeaturePtr ptrFeature, CListCtrl* pList)
{
	// Release any previously allocated memory
	int iCount = pList->GetItemCount();
	if (iCount > 0)
	{
		for (int i = 0; i < iCount; i++)
		{
			// Retrieve the ITEMINFO data
			ITEMINFO*	pData = (ITEMINFO *)pList->GetItemData( i );
			
			// Release the memory
			delete pData;
		}
	}
	
	// Clear the list
	pList->DeleteAllItems();
	
	// Add the (blank) placeholder item
	int iNewIndex = pList->InsertItem( 0, "" );
	
	// Set the Item Data
	ITEMINFO*	pData = new ITEMINFO;
	pData->eType = kInvalidType;
	pData->bMarkForDelete = false;
	pList->SetItemData( iNewIndex, (DWORD)pData );
	
	// Check for NULL Feature
	if (ptrFeature == NULL)
	{
		// No Parts available to parse
		return false;
	}
	
	// Determine number of Parts included in this Feature
	long lNumParts = 0;
	lNumParts = ptrFeature->getNumParts();
	if (lNumParts == 0)
	{
		// No Parts available to parse
		return false;
	}
	
	// Retrieve the collection of Parts
	IEnumPartPtr ptrEnumPart( ptrFeature->getParts() );
	
	// Loop through each Part and parse
	IPartPtr	ptrPart;
	for (int i = 0; i < lNumParts; i++)
	{
		// Get the Part
		ptrPart = ptrEnumPart->next();
		
		// Parse the Part
		parsePart( ptrPart, pList );
	}
	
	// Successful parse
	return true;
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::parsePart(IPartPtr ptrPart, CListCtrl* pList)
{
	// Determine number of Segments included in this Part
	long lNumSegments = ptrPart->getNumSegments();
	if (lNumSegments == 0)
	{
		// No Segments available to parse
		return;
	}
	
	// Retrieve the Start Point
	ICartographicPointPtr	ptrPoint;
	ptrPoint = ptrPart->getStartingPoint();
	
	// Add the starting point to the list
	int iIndex = -1;		// add point to end of list
	setPoint( iIndex, ptrPoint, pList );
	
	// Retrieve the collection of Segments
	IEnumSegmentPtr ipEnumSegment = ptrPart->getSegments();
	
	// current segment and next segment
	IESSegmentPtr ipSegmentCurrent = ipEnumSegment->next();
	IESSegmentPtr ipSegmentNext(NULL);
	while (ipSegmentCurrent != NULL)
	{
		// get next segment
		ipSegmentNext = ipEnumSegment->next();
		if (ipSegmentNext != NULL)
		{
			// if the next segment requires tangent-in
			if (ipSegmentNext->requireTangentInDirection() == VARIANT_TRUE)
			{
				// get tangent-out from current segment
				_bstr_t _bstrTangentIn = ipSegmentCurrent->getTangentOutDirection();
				ipSegmentNext->setTangentInDirection(_bstrTangentIn);
			}
		}
		
		// Parse current segment
		parseSegment(ipSegmentCurrent, pList);

		// set current segment
		ipSegmentCurrent = ipSegmentNext;
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::parseSegment(IESSegmentPtr ptrSegment, CListCtrl* pList)
{
	// Get current count of list items
	int iCount = pList->GetItemCount();
	
	// Determine segment type
	if (ptrSegment->getSegmentType() == kLine)
	{
		// This is a line segment
		ILineSegmentPtr	ptrLine(ptrSegment);
		IIUnknownVectorPtr ipParams = ptrSegment->getParameters();
		// get the line bearing
		string strBearing = ptrSegment->getTangentOutDirection();
		string strDistance("");
		long nSize = ipParams->Size();
		for (long n=0; n<nSize; n++)
		{
			IParameterTypeValuePairPtr ipParam = ipParams->At(n);
			// find the line distance
			if (ipParam->eParamType == kLineDistance)
			{
				strDistance = ipParam->strValue;
				strDistance = convertInCurrentDistanceUnit(strDistance);
				break;
			}
		}

		if (strDistance.empty())
		{
			throw UCLIDException("ELI12485", "Failed to get line distance.");
		}
		
		// Add a new item at the end but before the Placeholder item
		CString	zText;
		zText.Format( "%d", iCount );
		int iNewIndex = pList->InsertItem( iCount-1, zText );
		
		// Create an ITEMINFO structure
		ITEMINFO*	pData = new ITEMINFO;
		pData->eType = kLineType;
		pData->bMarkForDelete = false;
		
		// The bearing string from line segment is in terms of 
		// quadrant bearig (eg. N45d34m12sW). For display purpose, it needs to be 
		// converted to the current distance type (for example, Azimuth) format.
		// First, evaluate the bearing string using Bearing class, then convert
		// it into current direction type format
		m_bearing.evaluate(strBearing.c_str());
		// We can skip validation here since it's been validated in line segment.
		// Use direction helper to convert the bearing as polar angle in
		// radians to direction in string.
		string	strDirectionText(m_directionHelper.polarAngleInRadiansToDirectionInString(m_bearing.getRadians() ) );
		// The string stored in the List Ctrl is in the form of current direction type
		// i.e. if the current direction type is azimuth, then the string shall look
		// like the angle format, for instance, 23d34m23s
		pData->mapData[kLineBearing] = strDirectionText;
		
		// Add Distance item to map
		pData->mapData[kLineDistance] = strDistance;
		
		// Provide the ITEMDATA
		pList->SetItemData( iNewIndex, (DWORD)pData );
		
		// Set Type text
		pList->SetItemText( iNewIndex, TYPE_LIST_COLUMN, "Line" );
				
		// Set Bearing and Distance cells
		pList->SetItemText( iNewIndex, LINEBEARING_LIST_COLUMN, 
			strDirectionText.c_str() );
		pList->SetItemText( iNewIndex, LINEDISTANCE_LIST_COLUMN, 
			strDistance.c_str() );
		// Other subitems remain clear
	}
	else if (ptrSegment->getSegmentType() == kArc)
	{
		// This is a curve segment
		IArcSegmentPtr	ptrArc(ptrSegment);
		
		// Add a new item at the end but before the Placeholder item
		CString	zText;
		zText.Format( "%d", iCount );
		int iNewIndex = pList->InsertItem( iCount-1, zText );
		
		// Create an ITEMINFO structure
		ITEMINFO*	pData = new ITEMINFO;
		pData->eType = kCurveType;
		pData->bMarkForDelete = false;
		
		// Set Type text
		pList->SetItemText( iNewIndex, TYPE_LIST_COLUMN, "Curve" );
		
		// Retrieve vector of curve parameters
		IIUnknownVectorPtr ptrVector = ptrSegment->getParameters();
		long lNumParams = ptrVector->Size();
		
		// Create CCE Helper 
		bool		bNeedsMore = false;
		m_pEngine->reset();
		CCEHelper	helper( m_pEngine );
		
		// Retrieve individual parameters
		string	strTemp;
		for (long lIndex = 0; lIndex < lNumParams; lIndex++)
		{
			// Retrieve this parameter from the vector
			IParameterTypeValuePairPtr	ptrParam = ptrVector->At( lIndex );
			
			// Retrieve value string
			strTemp = ptrParam->strValue;

			// Provide this parameter to the Helper
			ECurveParameterType	eType = ptrParam->eParamType;
			helper.setCurveParameter(eType, strTemp);
			
			// if the parameter is distance, convert it to be in
			// the current unit
			strTemp = convertDistanceOrDirection(eType, strTemp);
			
			// Add this item to the map if not a point
			switch( eType )
			{
				// Just skip over these parameters, they may need to be 
				// recalculated if other segments change
			case kArcStartingPoint:
			case kArcMidPoint:
			case kArcEndingPoint:
			case kArcCenter:
			case kArcExternalPoint:
			case kArcChordMidPoint:
				bNeedsMore = true;
				break;
				
				// bearing parameter types
			case kArcTangentInBearing:
			case kArcTangentOutBearing:
			case kArcChordBearing:
			case kArcRadialInBearing:
			case kArcRadialOutBearing:
				{
					// Bearing strings shall be converted to the format of
					// current direction type
					string strDirection(m_directionHelper.polarAngleInRadiansToDirectionInString(
						helper.getCurveBearingAngleOrDistanceInDouble(eType)));
					pData->mapData[eType] = strDirection;
				}
				break;
				
				// These parameters are useful and should be saved
			default:
				pData->mapData[eType] = strTemp;
				break;
			}
		}
		
		// Make sure curve is valid
		if (bNeedsMore)
		{
			// Concavity and Angle Size
			pData->mapData[kArcConcaveLeft] = 
				helper.getCurveParameter( kArcConcaveLeft );
			pData->mapData[kArcDeltaGreaterThan180Degrees] = 
				helper.getCurveParameter( kArcDeltaGreaterThan180Degrees );
			
			// Chord Bearing, Chord Length, and Radius
			pData->mapData[kArcChordBearing] = m_directionHelper.polarAngleInRadiansToDirectionInString(
				helper.getCurveBearingAngleOrDistanceInDouble( kArcChordBearing ) );
			pData->mapData[kArcChordLength] = 
				helper.getCurveParameter( kArcChordLength );
			pData->mapData[kArcRadius] = 
				helper.getCurveParameter( kArcRadius );
		}
		
		// Provide the ITEMDATA
		pList->SetItemData( iNewIndex, (DWORD)pData );
		
		// Retrieve Chord Bearing string for display
		strTemp = m_directionHelper.polarAngleInRadiansToDirectionInString(
			helper.getCurveBearingAngleOrDistanceInDouble( kArcChordBearing ) );
		pList->SetItemText( iNewIndex, CHORDBEARING_LIST_COLUMN, strTemp.c_str());
		
		// Retrieve Chord Length string for display
		char	pszText[100];
		double	dValue = 0.0;
		dValue = helper.getCurveBearingAngleOrDistanceInDouble( kArcChordLength );
		
		// Retrieve and apply precision value
		char	pszFormat[30];
		int		iOptionValue = IcoMapOptions::sGetInstance().getPrecisionDigits();
		
		// use current distance unit for displaying any distance value
		string strUnit( m_distance.getStringFromUnit( 
			m_distance.getCurrentDistanceUnit() ) );
		
		sprintf_s( pszFormat, sizeof(pszFormat), "%%.%df %s", iOptionValue, strUnit.c_str() );
		sprintf_s( pszText, sizeof(pszText), pszFormat, dValue );
		pList->SetItemText( iNewIndex, CHORDLENGTH_LIST_COLUMN, 
			pszText);
		
		// Retrieve Radius string for display
		dValue = helper.getCurveBearingAngleOrDistanceInDouble( kArcRadius );
		sprintf_s( pszText, sizeof(pszText), pszFormat, dValue );
		pList->SetItemText( iNewIndex, RADIUS_LIST_COLUMN, 
			pszText);
	}
	else
	{
		// Throw exception - Unsupported segment type
		UCLIDException uclidException( "ELI01781", 
			"Unsupported segment type." );
		uclidException.addDebugInfo( "Segment Type: ", 
			ptrSegment->getSegmentType() );
		throw uclidException;
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::setPoint(int iIndex, ICartographicPointPtr ptrPoint, 
								 CListCtrl* pList)
{
	// Set the list item
	int iNewIndex = -1;
	int iCount = pList->GetItemCount();
	CString	zText;
	if (iIndex == -1)
	{
		// Add a new item at the end but before the Placeholder
		zText.Format( "%d", iCount );
		iNewIndex = pList->InsertItem( iCount-1, zText );
		
		// Create an ITEMINFO structure
		ITEMINFO*	pData = new ITEMINFO();
		pData->eType = kStartPointType;
		pData->bMarkForDelete = false;
		
		double dX, dY;
		ptrPoint->GetPointInXY(&dX, &dY);
		// Combine X and Y
		zText.Format( "%f,%f", dX, dY);
		
		// Add Start Point item to map
		string	strText( zText.operator LPCTSTR() );
		pData->mapData[kArcStartingPoint] = strText;
		
		// Provide the ITEMDATA
		pList->SetItemData( iNewIndex, (DWORD)pData );
		
		// Set Type text
		pList->SetItemText( iNewIndex, 2, "S.P." );
		
// Set Start X text
//			zText.Format( "%f", ptrPoint->GetdX() );
//			pList->SetItemText( iNewIndex, STARTPOINT_X_LIST_COLUMN, zText );

// Set Start Y text
//			zText.Format( "%f", ptrPoint->GetdY() );
//			pList->SetItemText( iNewIndex, STARTPOINT_Y_LIST_COLUMN, zText );
// Other subitems remain clear
	}
	// Modify an existing Point item
	else
	{
		// Check item index
		if (iIndex < iCount-1)
		{
			// Retrieve Item Data
			ITEMINFO*	pData;
			pData = (ITEMINFO *)pList->GetItemData( iIndex );
			
			// Replace Point data
			CString	zText;
			double dX, dY;
			ptrPoint->GetPointInXY(&dX, &dY);
			zText.Format( "%f,%f", dX, dY);
			string	strText( zText.operator LPCTSTR() );
			pData->mapData[kArcStartingPoint] = strText;
//				ptrPoint->get_dX( &dNewX );
//				zText.Format( "%f", dNewValue );
//				pList->SetItemText( iIndex, STARTPOINT_X_LIST_COLUMN, zText );

//				ptrPoint->get_dY( &dNewValue );
//				zText.Format( "%f", dNewValue );
//				pList->SetItemText( iIndex, STARTPOINT_Y_LIST_COLUMN, zText );
		}
		else
		{
			// Throw exception - Invalid index for Point item 
			UCLIDException uclidException( "ELI01782", 
				"Invalid index for Start Point item." );
			uclidException.addDebugInfo( "Index: ", iIndex );
			throw uclidException;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::getDialogSizes(bool bSmallFromLarge) 
{
	// Determine present size of dialog
	int		iHeightChange = 0;
	CRect	rect;
	CRect	rectLow;
	GetClientRect( &rect );
	
	// Check which size is now known
	if (bSmallFromLarge)
	{
		// Large size is now known, so save it
		m_sizeLarge.cx = rect.Width();
		m_sizeLarge.cy = rect.Height();
		
		// Compute the small size as:
		// Large height - height difference between 2nd static and OK button
		m_staticOrig.GetWindowRect( &rect );
		m_ok.GetWindowRect( &rectLow );
		iHeightChange = rectLow.top - rect.top;
		
		// Store small size
		m_sizeSmall.cx = m_sizeLarge.cx;
		m_sizeSmall.cy = m_sizeLarge.cy - iHeightChange;
	}
	else
	{
		// Small size is now known, so save it
		m_sizeSmall.cx = rect.Width();
		m_sizeSmall.cy = rect.Height();
		
		// Compute the large size as:
		// Small height + height difference between 1st static and OK button
		m_staticCurr.GetWindowRect( &rect );
		m_ok.GetWindowRect( &rectLow );
		iHeightChange = rectLow.top - rect.top;
		
		// Store large size
		m_sizeLarge.cx = m_sizeSmall.cx;
		m_sizeLarge.cy = m_sizeSmall.cy + iHeightChange;
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnSetfocusList1(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// Remember which list has focus
		m_iListWithFocus = 1;

		// Check selection status and update toolbar buttons
		doToolBarUpdates();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02046")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnSetfocusList2(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// Remember which list has focus
		m_iListWithFocus = 2;

		// Check selection status and update toolbar buttons
		doToolBarUpdates();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02048")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride( gAVDllResource );

	try
	{
		CDialog::OnSize(nType, cx, cy);

		if (m_bInitialized)
		{
			////////////////////////////
			// Prepare controls for move
			////////////////////////////
			CRect	rectDlg;
			CRect	rectCurr;
			CRect	rectOrig;
			CRect	rectStatic;
			CRect	rectDisplay;
			CRect	rectShowHide;

			// Get total dialog size
			GetWindowRect( &rectDlg );

			// Get sizes and positions before move
			m_listCurrent.GetWindowRect( &rectCurr );
			m_listOriginal.GetWindowRect( &rectOrig );
			m_showHide.GetWindowRect( &rectShowHide );

			// Get position differences since top left of current list 
			// should not change relative to total dialog
			int iDiffX = rectCurr.left - rectDlg.left;
			int iDiffY = rectCurr.top - rectDlg.top;

			// Determine (unchanged) position of current static text
			m_staticCurr.GetWindowRect( &rectStatic );
			ScreenToClient( &rectStatic );

			// Determine new height for the lists
			int iNewHeight = 0;
			iNewHeight = rectDlg.Height();
			int iFudge = 0;
			if (!m_bShowOriginalList)
			{
				// Account for everything but list
				// Note extra half control space and extra button height so that 
				// list does not cover up buttons
				iNewHeight -= (rectStatic.top + rectStatic.Height() + 
					((int)(2.5*m_iControlSpace)) + 2*rectShowHide.Height());
			}
			else
			{
				// Account for everything but lists
				// Note extra half control space and extra button height so that 
				// list does not cover up buttons
				iNewHeight -= (rectStatic.top + 2*rectStatic.Height() + 
					2*rectShowHide.Height() + ((int)(3.5*m_iControlSpace)));

				// Same size for each
				iNewHeight /= 2;		
			}

			// Convert list rects to client coordinates before Move
			ScreenToClient( rectCurr );
			ScreenToClient( rectOrig );

			///////////////
			// Do the moves
			///////////////
			// Current attributes
			m_listCurrent.MoveWindow( rectCurr.left, rectCurr.top, 
				cx - iDiffX - iDiffX/2, iNewHeight, TRUE );

			// Save new list position to help position buttons
			m_listCurrent.GetWindowRect( &rectCurr );
			ScreenToClient( rectCurr );

			// Original attributes label
			m_staticOrig.GetWindowRect( &rectStatic );
			ScreenToClient( &rectStatic );
			m_staticOrig.MoveWindow( rectCurr.left, 
				rectCurr.bottom + m_iControlSpace, 
				rectStatic.Width(), rectStatic.Height(), TRUE );
			m_staticOrig.GetWindowRect( &rectStatic );
			ScreenToClient( &rectStatic );

			// Original attributes
			m_listOriginal.MoveWindow( rectOrig.left, rectStatic.bottom + 1, 
				cx - iDiffX - iDiffX/2, iNewHeight, TRUE );

			// Save new list position to help position buttons
			m_listOriginal.GetWindowRect( &rectOrig );
			ScreenToClient( rectOrig );

			// Display precision label
			// Right justified but not overlapping Current Attributes label
			m_staticDisplay.GetWindowRect( &rectDisplay );
			ScreenToClient( &rectDisplay );
			m_staticDisplay.MoveWindow( rectStatic.right + 7, 
				rectDisplay.top, 
				rectCurr.right - (rectStatic.right + 7), 
				rectDisplay.Height(), TRUE );

			///////////////
			// Move buttons
			///////////////
			CRect	rectOK;
			CRect	rectCancel;

			// Get button sizes
			m_showHide.GetWindowRect( &rectShowHide );
			m_cancel.GetWindowRect( &rectCancel );
			m_ok.GetWindowRect( &rectOK );

			// Convert button rects to client coordinates before Move
			ScreenToClient( rectShowHide );
			ScreenToClient( rectCancel );
			ScreenToClient( rectOK );

			ScreenToClient( rectDlg );

			// Move the Show/Hide Attributes button
			m_showHide.MoveWindow( rectShowHide.left, 
				rectDlg.bottom - m_iControlSpace - rectShowHide.Height(), 
				rectShowHide.Width(), rectShowHide.Height() );
			m_showHide.GetWindowRect( &rectShowHide );
			ScreenToClient( rectShowHide );

			// Move the Cancel button
			m_cancel.MoveWindow( rectCurr.right - rectCancel.Width(), 
				rectShowHide.top, rectCancel.Width(), rectCancel.Height() );

			// Retrieve adjusted rectangle for new Cancel position
			m_cancel.GetWindowRect( &rectCancel );
			ScreenToClient( rectCancel );

			// Move the OK button
			m_ok.MoveWindow( rectCancel.left - 7 - rectOK.Width(), 
				rectShowHide.top, rectOK.Width(), rectOK.Height() );

			// Position the toolbar in the dialog
			RepositionBars( AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0 );

			// Force a complete repaint
			Invalidate();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02050")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnInsertCurve() 
{
	try
	{
		CListCtrl*	pList = NULL;
		if (m_iListWithFocus == 1)
		{
			// Use the Current Attributes list
			pList = &m_listCurrent;
		}
		else
		{
			// Use the Original Attributes list
			pList = &m_listOriginal;
		}

		// Retrieve record at insert location
		POSITION pos = pList->GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// Throw exception - Cannot Insert without a selected item
			UCLIDException uclidException( "ELI01783", 
				"Cannot insert curve without selecting an item." );
			throw uclidException;
		}
		else
		{
			while (pos)
			{
				// Get index of selected item
				int iItem = pList->GetNextSelectedItem( pos );

				// Create and run the Curve Calculator dialog in Edit mode
				CCurveCalculatorDlg	dlg( true, false, NULL );
				if (dlg.DoModal() == IDOK)
				{
					// Create an ITEMINFO structure
					ITEMINFO*	pData = new ITEMINFO;
					pData->eType = kCurveType;
					pData->bMarkForDelete = false;

					// Create the CCE Helper
					m_pEngine->reset();
					CCEHelper	cceHelper( m_pEngine );

					// Retrieve updated parameters and values
					for (int i = 1; i <= 3; i++)
					{
						// Get parameter type
						ECurveParameterType	eType = dlg.getParameter( i );
						// Get parameter value in string
						// Note: if it's bearing type, then the bearing is 
						// not in the current direction type
						string	strValue ( dlg.getParameterValue( i ) );
						
						// if the curve parameter is of some bearing type, the 
						// string retrieved from curve calculation dlg is in the
						// format of current direction type, therefore, it needs
						// to be converted into polar angle value to calculate other
						// parameter values via CCE since CCE only deals with polar
						// angle value
						switch (eType)
						{
							// bearing parameter types
						case kArcTangentInBearing:
						case kArcTangentOutBearing:
						case kArcChordBearing:
						case kArcRadialInBearing:
						case kArcRadialOutBearing:
							{
								// use direction helper to evaluate the string first
								m_directionHelper.evaluateDirection(strValue);
								// set curve parameter value
								m_pEngine->setCurveAngleOrBearingParameter(
									eType, m_directionHelper.getPolarAngleRadians() );
							}
							break;
						default:
							{
								// Provide data to CCE Helper
								cceHelper.setCurveParameter( eType, strValue );
							}
							break;
						}			
						
						pData->mapData[eType] = strValue;
					}

					// Retrieve concavity
					int iConcavity = dlg.getConcavity();
					if (iConcavity != -1)
					{
						// Value needs to be provided to CCE Helper and map
						if (iConcavity == 0)
						{
							cceHelper.setCurveParameter( kArcConcaveLeft, "0" );
							pData->mapData[kArcConcaveLeft] = "0";
						}
						else
						{
							cceHelper.setCurveParameter( kArcConcaveLeft, "1" );
							pData->mapData[kArcConcaveLeft] = "1";
						}
					}

					// Retrieve angle size
					int iAngle = dlg.getAngleSize();
					if (iAngle != -1)
					{
						// Value needs to be provided to CCE Helper and map
						if (iAngle == 0)
						{
							cceHelper.setCurveParameter( 
								kArcDeltaGreaterThan180Degrees, "0" );
							pData->mapData[kArcDeltaGreaterThan180Degrees] = "0";
						}
						else
						{
							cceHelper.setCurveParameter( 
								kArcDeltaGreaterThan180Degrees, "1" );
							pData->mapData[kArcDeltaGreaterThan180Degrees] = "1";
						}
					}

					// Add a new Line item at the insert position
					// but before the Placeholder item
					CString	zID;
					zID.Format( "%d", iItem+1 );
					int iNewItem = pList->InsertItem( iItem, zID );

					// Set the Type string
					pList->SetItemText( iNewItem, TYPE_LIST_COLUMN, "Curve" );

					// Set the ItemData
					pList->SetItemData( iNewItem, (DWORD)pData );

					// Retrieve Chord Bearing string for display
					string	strTemp ( m_directionHelper.polarAngleInRadiansToDirectionInString(
						cceHelper.getCurveBearingAngleOrDistanceInDouble( kArcChordBearing ) ) );

					pList->SetItemText( iNewItem, CHORDBEARING_LIST_COLUMN, strTemp.c_str() );

					// Retrieve Chord Length string for display
					double	dValue = 0.0;
					dValue = cceHelper.getCurveBearingAngleOrDistanceInDouble( kArcChordLength );

					// Retrieve and apply precision value
					char	pszFormat[30];
					char	pszString[100];
					int		iOptionValue = IcoMapOptions::sGetInstance().getPrecisionDigits();

					string strUnit( m_distance.getStringFromUnit( 
						m_distance.getCurrentDistanceUnit() ) );
					sprintf_s( pszFormat, sizeof(pszFormat), "%%.%df %s", iOptionValue, strUnit.c_str() );
					sprintf_s( pszString, sizeof(pszString), pszFormat, dValue );
					pList->SetItemText( iItem, CHORDLENGTH_LIST_COLUMN, 
						pszString);

					// Retrieve Radius string for display
					dValue = cceHelper.getCurveBearingAngleOrDistanceInDouble( kArcRadius );
					sprintf_s( pszString, sizeof(pszString), pszFormat, dValue );
					pList->SetItemText( iNewItem, RADIUS_LIST_COLUMN, 
						pszString);

					// Do any required item renumbering
					updateView( pList );
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02052")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnInsertLine() 
{
	try
	{
		CListCtrl*	pList = NULL;
		if (m_iListWithFocus == 1)
		{
			// Use the Current Attributes list
			pList = &m_listCurrent;
		}
		else
		{
			// Use the Original Attributes list
			pList = &m_listOriginal;
		}

		// Retrieve record at insert location
		POSITION pos = pList->GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// Throw exception - Cannot Insert without a selected item
			UCLIDException uclidException( "ELI01784", 
				"Cannot insert line without selecting an item." );
			throw uclidException;
		}
		else
		{
			// Get index of selected item
			int iItem = pList->GetNextSelectedItem( pos );

			// Create default Bearing and Distance strings
			CString	zBearing( "" );
			CString	zDistance( "" );

			// Create and run the Line dialog in Edit mode
			CLineDlg	dlg( zBearing, zDistance, false, true, NULL );
			if (dlg.DoModal() == IDOK)
			{
				// Retrieve updated strings
				zBearing = dlg.getBearing();
				zDistance = dlg.getDistance();

				// Create an ITEMINFO structure
				ITEMINFO*	pData = new ITEMINFO;
				pData->eType = kLineType;
				pData->bMarkForDelete = false;

				// Add map items
				pData->mapData[kLineBearing] = zBearing.operator LPCTSTR();
				pData->mapData[kLineDistance] = zDistance.operator LPCTSTR();

				// Add a new Line item at the insert position
				// but before the Placeholder item
				CString	zID;
				zID.Format( "%d", iItem+1 );
				int iNewItem = pList->InsertItem( iItem, zID );

				// Set the Type string
				pList->SetItemText( iNewItem, TYPE_LIST_COLUMN, "Line" );

				// Set the ItemData
				pList->SetItemData( iNewItem, (DWORD)pData );

				// Create distance string with non-standard precision
				string strDistance = convertInCurrentDistanceUnit((LPCTSTR)zDistance);

				// Update cells in list
				pList->SetItemText( iNewItem, LINEBEARING_LIST_COLUMN, 
					zBearing.operator LPCTSTR() );
				pList->SetItemText( iNewItem, LINEDISTANCE_LIST_COLUMN, 
					strDistance.c_str());

				// Do any required item renumbering
				updateView( pList );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02054")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnInsertPart() 
{
	try
	{
		CListCtrl*	pList = NULL;
		if (m_iListWithFocus == 1)
		{
			// Use the Current Attributes list
			pList = &m_listCurrent;
		}
		else
		{
			// Use the Original Attributes list
			pList = &m_listOriginal;
		}

		// Create default X and Y strings
		CString	zX( "0" );
		CString	zY( "0" );

		// Create and run the Part dialog in Edit mode
		CPartDlg	dlg( zX, zY, false, true );
		if (dlg.DoModal() == IDOK)
		{
			// Retrieve updated strings
			zX = dlg.getX();
			zY = dlg.getY();

			// Create an ITEMINFO structure
			ITEMINFO*	pData = new ITEMINFO;
			pData->eType = kStartPointType;
			pData->bMarkForDelete = false;

			// Add item to map
			CString	zTemp;
			zTemp.Format( "%s,%s", zX, zY );
			pData->mapData[kArcStartingPoint] = zTemp.operator LPCTSTR();

			// Add a new Start Point item at the end of the list
			// but before the Placeholder item
			int iCount = pList->GetItemCount();
			CString	zID;
			zID.Format( "%d", iCount );
			int iItem = pList->InsertItem( iCount-1, zID );

			// Set the Type string
			pList->SetItemText( iItem, TYPE_LIST_COLUMN, "S.P." );

			// Set the ItemData
			pList->SetItemData( iItem, (DWORD)pData );

			// Update cells in list
//			pList->SetItemText( iItem, STARTPOINT_X_LIST_COLUMN, 
//				zX.operator LPCTSTR() );
//			pList->SetItemText( iItem, STARTPOINT_Y_LIST_COLUMN, 
//				zY.operator LPCTSTR() );

			// Do any required item renumbering
			updateView( pList );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02056")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnItemDelete() 
{
	try
	{
		// Get pointer to active List
		CListCtrl*	pList = NULL;
		if (m_iListWithFocus == 1)
		{
			// Use the Current Attributes list
			pList = &m_listCurrent;
		}
		else
		{
			// Use the Original Attributes list
			pList = &m_listOriginal;
		}

		// Retrieve record(s) to be Deleted
		POSITION pos = pList->GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// Throw exception - Cannot Delete unselected items
			UCLIDException uclidException( "ELI01785", 
				"Cannot delete unselected items." );
			throw uclidException;
		}
		else
		{
			while (pos)
			{
				// Get index of selected item
				int iItem = pList->GetNextSelectedItem( pos );

				// Retrieve Item Info
				ITEMINFO*	pData;
				pData = (ITEMINFO *)pList->GetItemData( iItem );

				// Mark for delete as appropriate
				switch( pData->eType )
				{
				case kStartPointType:
					{
						// Retrieve Part number
						CString	zPart = pList->GetItemText( iItem, PART_LIST_COLUMN );
						int		iPart = 0;
						iPart = atoi( zPart.operator LPCTSTR() );

						if (iPart > 0)
						{
							// Provide a confirmation dialog
							CString	zText;
							zText.Format( "Delete starting point and all associated line and curve segments from part #%d.", iPart );
							int iResult = MessageBox( zText.operator LPCTSTR(), 
								"Confirm Delete", MB_ICONQUESTION | MB_DEFBUTTON2 | MB_YESNOCANCEL );

							// Check return from Message Box
							if (iResult == IDCANCEL)
							{
								// Clear all bMarkedForDelete flags 
								for (int i = 0; i < pList->GetItemCount(); i++)
								{
									// Retrieve the Item Data
									pData = (ITEMINFO *)pList->GetItemData( i );

									// Remove any delete marking on this item
									pData->bMarkForDelete = false;
								}

								// Just return - user cancelled the entire operation
								return;
							}
							else if (iResult == IDYES)
							{
								// Mark this item for deletion
								pData->bMarkForDelete = true;

								// Delete remaining Segments from this Part
								for (int i = 0; i < pList->GetItemCount(); i++)
								{
									// Determine Part number of this item
									zPart = pList->GetItemText( i, PART_LIST_COLUMN );
									int		iThisPart = 0;
									iThisPart = atoi( zPart.operator LPCTSTR() );

									// Check for match
									if (iThisPart == iPart)
									{
										// Retrieve the Item Data
										pData = (ITEMINFO *)pList->GetItemData( i );

										// Mark this item for delete
										pData->bMarkForDelete = true;

									}		// end if this item is associated with 
											//     the part being deleted
								}			// end for each item in list
							}				// end if user confirmed delete
						}					// end if valid Part number
					}						// end case kStartPointType
					break;

				case kLineType:
				case kCurveType:
					{
						// Mark this item for delete
						pData->bMarkForDelete = true;
					}
					break;

				default:
					// Throw exception - Unknown item type
					UCLIDException uclidException( "ELI01788", 
						"Cannot delete unknown item type." );
					uclidException.addDebugInfo( "Item Type: ", pData->eType );
					throw uclidException;
					break;
				}
			}

			// Delete all marked items
			deleteMarked( pList );

			// Do any required item renumbering
			updateView( pList );

			// Update toolbar button states
			doToolBarUpdates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02058")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnItemClosure() 
{
	try
	{
		// create a temp file to hold info
		TemporaryFileName tempFile("ClosureReport_", ".tmp", true);
		ofstream ofs(tempFile.getName().c_str(), ios::out | ios::trunc);

		// closure report displayed in notepad
		getClosureReport(ofs);
		// get window system32 path
		char pszSystemDir[MAX_PATH];
		::GetSystemDirectory(pszSystemDir, MAX_PATH);
		
		string strCommand(pszSystemDir);
		strCommand += "\\Notepad.exe ";
		strCommand += tempFile.getName();
		// run Notepad.exe with this file
		::runEXE(strCommand);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12487")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::deleteMarked(CListCtrl* pList)
{
	try
	{
		// Loop through listed items
		for (int i = 0; i < pList->GetItemCount(); i++)
		{
			// Retrieve Item Data
			ITEMINFO*	pData = (ITEMINFO *)pList->GetItemData( i );

			// Check flag for deletion
			if (pData->bMarkForDelete)
			{
				// Release the allocated memory
				delete pData;

				// Delete the list item
				pList->DeleteItem( i );

				// Decrement index to avoid skipping an item
				i--;
			}
		}

		// Update selection to item underneath last selected (deleted)
		int iCount = pList->GetItemCount();
		int iSelected = -1;
		if (m_iListWithFocus == 1)
		{
			iSelected = m_iCurrentLastSelected;
		}
		else if (m_iListWithFocus == 2)
		{
			iSelected = m_iOriginalLastSelected;
		}

		// Make sure that too many items weren't deleted
		if (iSelected >= iCount)
		{
			// Just default to last (placeholder) item
			iSelected = iCount - 1;
		}

		// Don't allow placeholder to be selected
		// unless it's the only item left
		if ((iSelected == (iCount - 1)) && (iCount != 1))
		{
			iSelected--;
		}

		// Set the selection
		pList->SetItemState( iSelected, LVIS_SELECTED, LVIS_SELECTED );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02060")
}
//-------------------------------------------------------------------------------------------------
string CAttributeViewDlg::convertDistanceOrDirection(ECurveParameterType eCurveType, 
													 const string& strInValue)
{
	string strConvertedValue(strInValue);

	if (isDistance(eCurveType))
	{
		strConvertedValue = convertInCurrentDistanceUnit(strInValue);
	}
	else if (isDirection(eCurveType))
	{
		// always work in normal mode here
		ReverseModeValueRestorer rmvr;
		AbstractMeasurement::workInReverseMode(false);
		// all directions are in the format of quadrant bearing
		m_directionHelper.evaluateDirection(strInValue);
		strConvertedValue = m_directionHelper.directionAsString();
	}

	return strConvertedValue;
}
//-------------------------------------------------------------------------------------------------
string CAttributeViewDlg::convertInCurrentDistanceUnit(const string& strDistance)
{
	m_distance.reset();
	// Create distance string with non-standard precision
	m_distance.evaluate(strDistance.c_str());
	double dTemp = m_distance.getDistanceInCurrentUnit();

	string strRet = distanceValueToString(dTemp);

	return strRet;
}
//-------------------------------------------------------------------------------------------------
string CAttributeViewDlg::distanceValueToString(double dDistanceInCurrentUnit)
{
	string strUnit(m_distance.getStringFromUnit( 
								m_distance.getCurrentDistanceUnit()));
	
	CString pszFormat = getDistanceFormatString();
	char pszConvertedDistance[100];
	// each distance value must have a unit with it
	sprintf_s( pszConvertedDistance, sizeof(pszConvertedDistance), pszFormat, 
		dDistanceInCurrentUnit);

	string strRet = (LPCTSTR)pszConvertedDistance;
	strRet += " " + strUnit;
	return strRet;
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::getClosureReport(ofstream& ofs)
{
	if (m_ptrOrigFeature == NULL)
	{
		return;
	}

	// we only provide closure report for the feature that has one part only
	if (m_ptrOrigFeature->getNumParts() > 1)
	{
		ofs << "Failed to get closure report on feature that has more than one part." << endl;
		return;
	}

	/////////////////////////////
	// add time stamp, notes, etc
	// get current date/time
	__time64_t currTime;
	tm pTime;
	time( &currTime );
	int iError = _localtime64_s( &pTime, &currTime );

	// Convert time to string
	char szTime[32];
	iError = asctime_s( szTime, sizeof(szTime), &pTime );
	string strTimeNote = trim( szTime, "", "\r\n" );

	ofs << "Closure report generated on " << strTimeNote << endl;
	string strCurrentDistanceUnit = m_distance.getStringFromUnit(m_distance.getCurrentDistanceUnit());
	ofs << "Drawing unit for this parcel is set to : " << strCurrentDistanceUnit << endl << endl;

	/////////////////////////////////
	// header for display segment info
	string strDottedLine("-");
	strDottedLine = ::padCharacter(strDottedLine, false, '-', 100);
	ofs << strDottedLine << endl;

	string strTemp = ::padCharacter("Segment", false, ' ', SEGMENT_NUMBER_LEN);
	ofs << strTemp;

	strTemp = ::padCharacter("Segment", false, ' ', SEGMENT_TYPE_LEN);
	ofs << strTemp << "Segment" << endl;

	strTemp = ::padCharacter("Number", false, ' ', SEGMENT_NUMBER_LEN);
	ofs << strTemp;

	strTemp = ::padCharacter("Type", false, ' ', SEGMENT_TYPE_LEN);
	ofs << strTemp << "Parameters" << endl;

	ofs << strDottedLine << endl;

	///////////////////////////////
	// all segments in the feature
	IEnumPartPtr ipEnumPart = m_ptrOrigFeature->getParts();
	IPartPtr ipPart = ipEnumPart->next();
	IEnumSegmentPtr ipEnumSegment = ipPart->getSegments();
	IESSegmentPtr ipSegment = ipEnumSegment->next();
	// what's the perimeter (in current unit)
	double dPerimeter = 0.0;
	int nSegmentNumber = 1;
	while (ipSegment != NULL)
	{
		getSegmentReport(nSegmentNumber, ipSegment, ofs);

		// get current segment length string (usually in feet)
		strTemp = ipSegment->getSegmentLengthString();
		m_distance.reset();
		m_distance.evaluate(strTemp.c_str());
		// distance in current unit
		double dTemp = m_distance.getDistanceInCurrentUnit();
		dPerimeter += dTemp;

		ipSegment = ipEnumSegment->next();
		nSegmentNumber++;
	}

	// extra blank line here
	ofs << endl;

	/////////////////////////////
	// Error segment
	string strTotalError("0");
	bool bHasClosureError = getErrorSegmentReport(ipPart, strTotalError, ofs);

	/////////////////////////
	// Perimeter
	string strPerimeter = distanceValueToString(dPerimeter);
	ofs << "Feature Perimeter ";
	if (bHasClosureError)
	{
		ofs << "(excluding error segment) ";
	}

	ofs << ": " << strPerimeter << endl;

	if (!bHasClosureError)
	{
		// no closure error
		return;
	}

	///////////////
	// error ratio
	m_distance.reset();
	m_distance.evaluate(strTotalError.c_str());
	double dTotalError = m_distance.getDistanceInCurrentUnit();
	double dDenominator = dPerimeter / dTotalError;
	CString pszFormat = getDistanceFormatString();
	char pszDenominator[100];
	sprintf_s( pszDenominator, sizeof(pszDenominator), pszFormat, dDenominator );

	ofs << "Closure error ratio : " << strTotalError << "/" << strPerimeter 
		<< " (Approximately 1 in " << (LPCTSTR)pszDenominator << ")" << endl;
}
//-------------------------------------------------------------------------------------------------
string CAttributeViewDlg::getCurveTypeName(ECurveParameterType eCurveParamType)
{
	string strCurveTypeName("");

	switch (eCurveParamType)
	{
	case kLineDeflectionAngle:
		strCurveTypeName = "Line Deflection Angle";
		break;
	case kLineInternalAngle:
		strCurveTypeName = "Line Internal Angle";
		break;
	case kLineBearing:
		strCurveTypeName = "Line Direction";
		break;
	case kLineDistance:
		strCurveTypeName = "Line Distance";
		break;
	case kArcDelta:
		strCurveTypeName = "Delta Angle";
		break;
	case kArcStartAngle:
		strCurveTypeName = "Start Angle";
		break;
	case kArcEndAngle:
		strCurveTypeName = "End Angle";
		break;
	case kArcDegreeOfCurveChordDef:
		strCurveTypeName = "Degree Of Curve";
		break;
	case kArcDegreeOfCurveArcDef:
		strCurveTypeName = "Degrees Of Curve (Arc Definition)";
		break;
	case kArcTangentInBearing:
		strCurveTypeName = "Tangent-in Direction";
		break;
	case kArcTangentOutBearing:
		strCurveTypeName = "Tangent-out Direction";
		break;
	case kArcChordBearing:
		strCurveTypeName = "Chord Direction";
		break;
	case kArcRadialInBearing:
		strCurveTypeName = "Radial-in Direction";
		break;
	case kArcRadialOutBearing:
		strCurveTypeName = "Radial-out Direction";
		break;
	case kArcRadius:
		strCurveTypeName = "Radius";
		break;
	case kArcLength:
		strCurveTypeName = "Arc Length";
		break;
	case kArcChordLength:
		strCurveTypeName = "Chord Length";
		break;
	case kArcExternalDistance:
		strCurveTypeName = "External Distance";
		break;
	case kArcMiddleOrdinate:
		strCurveTypeName = "Middle Ordinate";
		break;
	case kArcTangentDistance:
		strCurveTypeName = "Tangent Distance";
		break;
	}

	return strCurveTypeName;
}
//-------------------------------------------------------------------------------------------------
CString CAttributeViewDlg::getDistanceFormatString()
{
	// Retrieve and apply precision value
	char pszFormat[30];
	int	iOptionValue = IcoMapOptions::sGetInstance().getPrecisionDigits();
	sprintf_s( pszFormat, sizeof(pszFormat), "%%.%df", iOptionValue );

	return pszFormat;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::getErrorSegmentReport(IPartPtr ipPart, 
											  string& strTotalError,
											  ofstream& ofs)
{
	// set start point of the part to (0,0)
	ICartographicPointPtr ipStartPoint(CLSID_CartographicPoint);
	ASSERT_RESOURCE_ALLOCATION("ELI12500", ipStartPoint != NULL);
	ipStartPoint->InitPointInXY(0.0, 0.0);

	ipPart->setStartingPoint(ipStartPoint);
	ICartographicPointPtr ipEndPoint = ipPart->getEndingPoint();
	ASSERT_RESOURCE_ALLOCATION("ELI12501", ipEndPoint != NULL);

	// if start and end point coincide at the same spot, 
	// there's no closing error
	if (ipStartPoint->IsEqual(ipEndPoint) == VARIANT_TRUE)
	{
		ofs << "This feature is perfectly closed." << endl; 
		// no error segment
		return false;
	}

	// get the error segment, which starts from the end point
	// of the part and ends at the start point of the part
	double dX, dY;
	ipEndPoint->GetPointInXY(&dX, &dY);
	TPPoint tpPointStart(dX, dY);
	ipStartPoint->GetPointInXY(&dX, &dY);
	TPPoint tpPointEnd(dX, dY);

	// always work in normal mode here
	ReverseModeValueRestorer rmvr;
	AbstractMeasurement::workInReverseMode(false);

	// the error bearing
	m_bearing.resetVariables();
	m_bearing.evaluate(tpPointStart, tpPointEnd);
	string strErrorDirection = 
		m_directionHelper.polarAngleInRadiansToDirectionInString(m_bearing.getRadians());
	ofs << "Error Direction : " << strErrorDirection << endl;

	// the error distance (in current unit)
	double dErrorDistance = tpPointStart.distanceTo(tpPointEnd);
	string strDistance = distanceValueToString(dErrorDistance);
	strTotalError = strDistance;
	ofs << "Error Distance : " << strDistance << endl;

	return true;
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::getSegmentReport(int nSegmentNumber, 
										 IESSegmentPtr ipSegment,
										 ofstream& ofs)
{
	// segment number
	string strTemp = ::asString(nSegmentNumber);
	ofs << ::padCharacter(strTemp, false, ' ', SEGMENT_NUMBER_LEN);

	// segment type
	ESegmentType eSegmentType = ipSegment->getSegmentType();
	if (eSegmentType == kLine)
	{
		ofs << ::padCharacter("Line", false, ' ', SEGMENT_TYPE_LEN);
	}
	else if (eSegmentType == kArc)
	{
		ofs << ::padCharacter("Arc", false, ' ', SEGMENT_TYPE_LEN);
	}
	else
	{
		throw UCLIDException("ELI12499", "Invalid segment type");
	}

	// segment parameters
	IIUnknownVectorPtr ipParams = ipSegment->getParameters();
	long nSize = ipParams->Size();
	for (long n=0; n<nSize; n++)
	{
		IParameterTypeValuePairPtr ipParam = ipParams->At(n);
		// get the parameter name
		ECurveParameterType eCurveType = ipParam->eParamType;
		// skip any concavity or delta angle
		if (eCurveType == kArcConcaveLeft 
			|| eCurveType == kArcDeltaGreaterThan180Degrees)
		{
			continue;
		}
		// do not show any tangent in info if it's a line
		if (eSegmentType == kLine && eCurveType == kArcTangentInBearing)
		{
			continue;
		}

		strTemp = getCurveTypeName(eCurveType) + ": ";
		ofs << strTemp;
		// get the parameter value
		strTemp = ipParam->strValue;

		// direction is always stored as quadrant bearing
		if (isDirection(eCurveType))
		{
			// always work in normal mode here
			ReverseModeValueRestorer rmvr;
			AbstractMeasurement::workInReverseMode(false);

			m_bearing.resetVariables();
			m_bearing.evaluate(strTemp.c_str());
			if (!m_bearing.isValid())
			{
				UCLIDException ue("ELI12498", "Invalid direction input");
				ue.addDebugInfo("Input", strTemp);
				throw ue;
			}
			
			// get the output in whatever is the current format, i.e. quadrant
			// bearing, polar angle or azimuth
			strTemp = m_directionHelper.polarAngleInRadiansToDirectionInString(
				m_bearing.getRadians());
		}
		else
		{
			// get the distance
			strTemp = convertDistanceOrDirection(eCurveType, strTemp);
		}

		ofs << strTemp << SPACE_IN_BETWEEN;
	}

	// at last, add a line feed
	ofs << endl;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::isDirection(ECurveParameterType eType)
{
	bool bRet = false;

	switch (eType)
	{		
	// bearing parameter types
	case kLineBearing:
	case kArcTangentInBearing:
	case kArcTangentOutBearing:
	case kArcChordBearing:
	case kArcRadialInBearing:
	case kArcRadialOutBearing:
		bRet = true;
		break;
	}

	return bRet;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::isDistance(ECurveParameterType eType)
{
	bool bRet = false;

	switch (eType)
	{
	// distance parameter types
	case kLineDistance:
	case kArcRadius:
	case kArcLength:
	case kArcChordLength:
	case kArcExternalDistance:
	case kArcMiddleOrdinate:
	case kArcTangentDistance:
		bRet = true;
		break;
	}

	return bRet;
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnItemEdit() 
{
	try
	{
		// since this might take for a while to open up especially curve calculator
		// put an hourglass here
		CWaitCursor wait;

		CListCtrl*	pList = NULL;
		if (m_iListWithFocus == 1)
		{
			// Use the Current Attributes list
			pList = &m_listCurrent;
		}
		else
		{
			// Use the Original Attributes list
			pList = &m_listOriginal;
		}

		// Retrieve record to be Edited
		POSITION pos = pList->GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// Throw exception - Cannot Edit unselected items
			UCLIDException uclidException( "ELI01786", 
				"Cannot edit unselected items." );
			throw uclidException;
		}
		else
		{
			while (pos)
			{
				// Get index of selected item
				int iItem = pList->GetNextSelectedItem( pos );

				// Retrieve Item Info
				ITEMINFO*	pData;
				pData = (ITEMINFO *)pList->GetItemData( iItem );

				// Call appropriate dialog
				switch( pData->eType )
				{
				case kStartPointType:
					{
						// Retrieve X and Y strings
						string strCombined;
						string strX;
						string strY;
						bool bResult = getItemDataString( pData->mapData, 
							kArcStartingPoint, strCombined );
						if (bResult)
						{
							// Parse X and Y
							vector<string> vecTokens;
							StringTokenizer	st;
							st.parse( strCombined.c_str(), vecTokens );
							if (vecTokens.size() != 2)
							{
								// Throw exception
								UCLIDException ue( "ELI02226", 
									"Unable to parse starting point." );
								ue.addDebugInfo( "Input string", strCombined );
								throw ue;
							}

							// Retrieve parsed strings
							strX = vecTokens[0];
							strY = vecTokens[1];
						}
						else
						{
							// Throw exception - Unable to retrieve X and Y
							UCLIDException uclidException( "ELI02227", 
								"Unable to retrieve Start Point." );
							throw uclidException;
						}

						// Create and run the Part dialog in View or Edit mode
						bool	bReadOnly = m_bOnlyViewItem;
						CPartDlg	dlg( strX.c_str(), strY.c_str(), bReadOnly, false );

						if ((dlg.DoModal() == IDOK) && !bReadOnly)
						{
							// Retrieve updated strings
							strX = dlg.getX();
							strY = dlg.getY();

							// Update Item Data
							strCombined = strX + "," + strY;
							pData->mapData[kArcStartingPoint] = strCombined;

							// Update cells in list
//							pList->SetItemText( iItem, STARTPOINT_X_LIST_COLUMN, 
//								zX.operator LPCTSTR() );
//							pList->SetItemText( iItem, STARTPOINT_Y_LIST_COLUMN, 
//								zY.operator LPCTSTR() );
						}
					}
					break;

				case kLineType:
					{
						// Retrieve Bearing and Distance strings
						string strBearing;
						string strDistance;
						bool bResult1 = getItemDataString( pData->mapData, 
							kLineBearing, strBearing );
						bool bResult2 = getItemDataString( pData->mapData, 
							kLineDistance, strDistance );

						// Check success of retrievals
						if (!bResult1 || !bResult2)
						{
							// Throw exception
							UCLIDException uclidException( "ELI02160", 
								"Unable to retrieve Line Bearing and Distance." );
							throw uclidException;
						}

						// convert distance to current unit
						strDistance = convertInCurrentDistanceUnit(strDistance);

						// Create and run the Line dialog in View or Edit mode
						bool	bReadOnly = m_bOnlyViewItem;
						CLineDlg	dlg( strBearing.c_str(), strDistance.c_str(), 
							bReadOnly, false, NULL );

						if ((dlg.DoModal() == IDOK) && !bReadOnly)
						{
							// Retrieve updated strings
							strBearing = dlg.getBearing();
							strDistance = dlg.getDistance();

							// Update Item Data
							pData->mapData[kLineBearing] = strBearing;
							pData->mapData[kLineDistance] = strDistance;

							strDistance = convertInCurrentDistanceUnit(strDistance);

							// Update cells in list
							pList->SetItemText( iItem, LINEBEARING_LIST_COLUMN, 
								strBearing.c_str() );
							pList->SetItemText( iItem, LINEDISTANCE_LIST_COLUMN, 
								strDistance.c_str() );
						}
					}
					break;

				case kCurveType:
					{
						//////////////////////////////////////////
						// Determine which parameters are original
						//////////////////////////////////////////
						ECurveParameterType	eP1;
						ECurveParameterType	eP2;
						ECurveParameterType	eP3;
						bool bResult = getCurveParameters( pData->mapData, &eP1, 
							&eP2, &eP3 );
						if (!bResult)
						{
							// Throw exception
							UCLIDException uclidException( "ELI02161", 
								"Unable to determine curve parameters." );
							throw uclidException;
						}

						///////////////////////////////////////////
						// Retrieve three original parameter values
						///////////////////////////////////////////
						string	strP1;
						string	strP2;
						string	strP3;
						if (!getItemDataString( pData->mapData, eP1, strP1 ))
						{
							// Throw exception
							UCLIDException uclidException( "ELI02162", 
								"Unable to determine first curve parameter." );
							uclidException.addDebugInfo( "Parameter Type: ", eP1 );
							throw uclidException;
						}
						// convert the string if it's of distance type
						strP1 = convertDistanceOrDirection(eP1, strP1);

						if (!getItemDataString( pData->mapData, eP2, strP2 ))
						{
							// Throw exception
							UCLIDException uclidException( "ELI02163", 
								"Unable to determine second curve parameter." );
							uclidException.addDebugInfo( "Parameter Type: ", eP2 );
							throw uclidException;
						}
						// convert the string if it's of distance type
						strP2 = convertDistanceOrDirection(eP2, strP2);

						if (!getItemDataString( pData->mapData, eP3, strP3 ))
						{
							// Throw exception
							UCLIDException uclidException( "ELI02164", 
								"Unable to determine third curve parameter." );
							uclidException.addDebugInfo( "Parameter Type: ", eP3 );
							throw uclidException;
						}
						// convert the string if it's of distance type
						strP3 = convertDistanceOrDirection(eP3, strP3);

						////////////////////////////////////
						// Retrieve Concavity and Angle Size
						////////////////////////////////////
						string	strTemp;
						int		iConcavity = 0;		// default, if not required
						int		iAngle = 0;			// default, if not required
						if (getItemDataString( pData->mapData, kArcConcaveLeft, 
							strTemp ))
						{
							// Parse the string
							iConcavity = atoi( strTemp.c_str() );
						}
						if (getItemDataString( pData->mapData, 
							kArcDeltaGreaterThan180Degrees, strTemp ))
						{
							// Parse the string
							iAngle = atoi( strTemp.c_str() );
						}

						// Create and run the Curve Calculator dialog in 
						// View or Edit mode
						bool	bReadOnly = m_bOnlyViewItem;
						CCurveCalculatorDlg	dlg(
							eP1, strP1.c_str(),		// 1st combo and edit
							eP2, strP2.c_str(),		// 2nd combo and edit
							eP3, strP3.c_str(),		// 3rd combo and edit
							iConcavity,				// Concave left/right
							iAngle,					// Angle > 180 or < 180
							true,					// Hide the Units radios
							bReadOnly ? true : false	// Hide the OK button ?
							);

						if ((dlg.DoModal() == IDOK) && !bReadOnly)
						{
							// Clear the Map
							pData->mapData.clear();

							// Create the CCE Helper
							m_pEngine->reset();
							CCEHelper	cceHelper( m_pEngine);

							// Retrieve updated parameters and values
							for (int i = 1; i <= 3; i++)
							{
								// Retrieve info
								ECurveParameterType	eType = dlg.getParameter( i );
								string	strValue = dlg.getParameterValue( i );

								// if the curve parameter is of some bearing type, the 
								// string retrieved from curve calculation dlg is in the
								// format of current direction type, therefore, it needs
								// to be converted into polar angle value to calculate other
								// parameter values via CCE
								switch (eType)
								{
									// bearing parameter types
								case kArcTangentInBearing:
								case kArcTangentOutBearing:
								case kArcChordBearing:
								case kArcRadialInBearing:
								case kArcRadialOutBearing:
									{
										// use direction helper to evaluate the string first
										m_directionHelper.evaluateDirection(strValue);
										// set curve parameter value
										m_pEngine->setCurveAngleOrBearingParameter(
											eType, m_directionHelper.getPolarAngleRadians() );
									}
									break;
								default:
									{
										// Provide data to CCE Helper
										cceHelper.setCurveParameter( eType, strValue );
									}
									break;
								}								
								
								// Add this item to the map
								pData->mapData[eType] = strValue;
							}

							// Retrieve concavity
							int iConcavity = dlg.getConcavity();
							if (iConcavity != -1)
							{
								// Value needs to be provided to CCE Helper and Map
								if (iConcavity == 0)
								{
									cceHelper.setCurveParameter( kArcConcaveLeft, "0" );
									pData->mapData[kArcConcaveLeft] = "0";
								}
								else
								{
									cceHelper.setCurveParameter( kArcConcaveLeft, "1" );
									pData->mapData[kArcConcaveLeft] = "1";
								}
							}

							// Retrieve angle size
							int iAngle = dlg.getAngleSize();
							if (iAngle != -1)
							{
								// Value needs to be provided to CCE Helper and Map
								if (iAngle == 0)
								{
									cceHelper.setCurveParameter( 
										kArcDeltaGreaterThan180Degrees, "0" );
									pData->mapData[kArcDeltaGreaterThan180Degrees] = "0";
								}
								else
								{
									cceHelper.setCurveParameter( 
										kArcDeltaGreaterThan180Degrees, "1" );
									pData->mapData[kArcDeltaGreaterThan180Degrees] = "1";
								}
							}

							// Retrieve Chord Direction string for display
							// The direction string shall be in the form of current direciton type
							string	strTemp ( m_directionHelper.polarAngleInRadiansToDirectionInString(
								cceHelper.getCurveBearingAngleOrDistanceInDouble( kArcChordBearing ) ) );

							pList->SetItemText( iItem, CHORDBEARING_LIST_COLUMN, 
								strTemp.c_str());

							// Retrieve Chord Length string for display
							double	dValue = 0.0;
							dValue = cceHelper.getCurveBearingAngleOrDistanceInDouble( kArcChordLength );

							// Retrieve and apply precision value
							char	pszFormat[30];
							char	pszString[100];
							int		iOptionValue = IcoMapOptions::sGetInstance().getPrecisionDigits();

							string strUnit( m_distance.getStringFromUnit( 
								m_distance.getCurrentDistanceUnit() ) );
							sprintf_s( pszFormat, sizeof(pszFormat), "%%.%df %s", 
								iOptionValue, strUnit.c_str() );
							sprintf_s( pszString, sizeof(pszString), pszFormat, dValue );
							pList->SetItemText( iItem, CHORDLENGTH_LIST_COLUMN, 
								pszString);

							// Retrieve Radius string for display
							dValue = cceHelper.getCurveBearingAngleOrDistanceInDouble( kArcRadius );
							sprintf_s( pszString, sizeof(pszString), pszFormat, dValue );
							pList->SetItemText( iItem, RADIUS_LIST_COLUMN, 
								pszString);
						}
					}
					break;

				case kInvalidType:
					// Just ignore this item type, there is no data to edit
					// This is an error condition
					break; 

				default:
					// Throw exception - Unknown item type
					UCLIDException uclidException( "ELI01789", 
						"Cannot edit unknown item type." );
					uclidException.addDebugInfo( "Item Type: ", pData->eType );
					throw uclidException;
					break;
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02062")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnItemTransfer() 
{
	try
	{
		/////////////////////////////
		// Sanity check on conditions
		/////////////////////////////

		// Check for both sets read-only
		if (m_bOriginalIsReadOnly && m_bCurrentIsReadOnly)
		{
			MessageBox( "Transfer of Attributes is not possible because both attribute sets are defined as read-only.",
				"Error", MB_ICONINFORMATION | MB_OK );

			return;
		}

		/////////////////////////////////////
		// Create and run the Transfer dialog
		/////////////////////////////////////
		CTransferDlg	dlg( m_bOriginalDefined, !m_bOriginalIsReadOnly, 
			!m_bCurrentIsReadOnly );

		if (dlg.DoModal() == IDOK)
		{
			CListCtrl*	pSource = NULL;
			CListCtrl*	pTarget = NULL;

			// Determine direction of Transfer
			if (dlg.isTransferToOriginal())
			{
				//////////////////////////////////////////////
				// Copy Current Feature Attributes to Original
				//////////////////////////////////////////////
				pSource = &m_listCurrent;
				pTarget = &m_listOriginal;

				// Set flag
				m_bOriginalDefined = true;
			}
			else
			{
				//////////////////////////////////////////////
				// Copy Original Feature Attributes to Current
				//////////////////////////////////////////////
				pSource = &m_listOriginal;
				pTarget = &m_listCurrent;
			}

			////////////////////////
			// Clear the target list
			////////////////////////
			int iCount = pTarget->GetItemCount();
			int i;
			for (i = 0; i < iCount; i++)
			{
				// Retrieve the Item Data (Type and Map)
				ITEMINFO*	pData = (ITEMINFO *)pTarget->GetItemData( i );

				// Free the allocated memory
				if (pData != NULL)
				{
					delete pData;
					pData = NULL;
				}
			}

			// Clear list items
			pTarget->DeleteAllItems();

			///////////////////////////
			// Step through Source list
			///////////////////////////
			CString		zText;
			DWORD		dwType = 0;
			int			iNewItem = -1;
			ITEMINFO*	pData = NULL;
			ITEMINFO*	pOldData = NULL;
			iCount = pSource->GetItemCount();
			for (i = 0; i < iCount; i++)
			{
				////////////////////////
				// Copy item information
				////////////////////////
				// Get the item text
				zText = pSource->GetItemText( i, ID_LIST_COLUMN );

				// Allocate new ITEMINFO structure
				pData = new ITEMINFO;

				// Get the Item Data (Type and Map) from the Source 
				pOldData = (ITEMINFO *)pSource->GetItemData( i );
				pData->eType = pOldData->eType;
				pData->bMarkForDelete = false;
				for (PTTVM::iterator itData = pOldData->mapData.begin(); itData != pOldData->mapData.end(); itData++)
				{
					// Store Type/Value pair in new Map
					pData->mapData[itData->first] = itData->second;
				}

				// Add this item to the Original list
				iNewItem = pTarget->InsertItem( i, zText.operator LPCTSTR(), NULL );

				// Set the Item Data (Type and Map)
				pTarget->SetItemData( iNewItem, (DWORD)pData );

				//////////////////////////////////
				// Add subitems based on item type
				//////////////////////////////////
				switch( pData->eType )
				{
				case kStartPointType:
					// Add Start Point label to the new item
					pTarget->SetItemText( iNewItem, TYPE_LIST_COLUMN, "S.P." );

					// Copy the Part number
					zText = pSource->GetItemText( i, PART_LIST_COLUMN );
					pTarget->SetItemText( iNewItem, PART_LIST_COLUMN, 
						zText.operator LPCTSTR() );

					// Copy the X
//					zText = pSource->GetItemText( i, STARTPOINT_X_LIST_COLUMN );
//					pTarget->SetItemText( iNewItem, STARTPOINT_X_LIST_COLUMN, 
//						zText.operator LPCTSTR() );

					// Copy the Y
//					zText = pSource->GetItemText( i, STARTPOINT_Y_LIST_COLUMN );
//					pTarget->SetItemText( iNewItem, STARTPOINT_Y_LIST_COLUMN, 
//						zText.operator LPCTSTR() );
					break;

				case kLineType:
					// Add Line label to the new item
					pTarget->SetItemText( iNewItem, TYPE_LIST_COLUMN, "Line" );

					// Copy the Part number
					zText = pSource->GetItemText( i, PART_LIST_COLUMN );
					pTarget->SetItemText( iNewItem, PART_LIST_COLUMN, 
						zText.operator LPCTSTR() );

					// Copy the Bearing
					zText = pSource->GetItemText( i, LINEBEARING_LIST_COLUMN );
					pTarget->SetItemText( iNewItem, LINEBEARING_LIST_COLUMN, 
						zText.operator LPCTSTR() );

					// Copy the Distance
					zText = pSource->GetItemText( i, LINEDISTANCE_LIST_COLUMN );
					pTarget->SetItemText( iNewItem, LINEDISTANCE_LIST_COLUMN, 
						zText.operator LPCTSTR() );
					break;

				case kCurveType:
					// Add Curve label to the new item
					pTarget->SetItemText( iNewItem, TYPE_LIST_COLUMN, "Curve" );

					// Copy the Part number
					zText = pSource->GetItemText( i, PART_LIST_COLUMN );
					pTarget->SetItemText( iNewItem, PART_LIST_COLUMN, 
						zText.operator LPCTSTR() );

					// Copy the Chord Bearing
					zText = pSource->GetItemText( i, CHORDBEARING_LIST_COLUMN );
					pTarget->SetItemText( iNewItem, CHORDBEARING_LIST_COLUMN, 
						zText.operator LPCTSTR() );

					// Copy the Chord Length
					zText = pSource->GetItemText( i, CHORDLENGTH_LIST_COLUMN );
					pTarget->SetItemText( iNewItem, CHORDLENGTH_LIST_COLUMN, 
						zText.operator LPCTSTR() );

					// Copy the Radius
					zText = pSource->GetItemText( i, RADIUS_LIST_COLUMN );
					pTarget->SetItemText( iNewItem, RADIUS_LIST_COLUMN, 
						zText.operator LPCTSTR() );
					break;

				case kInvalidType:
					// Clear the item label
					pTarget->SetItemText( iNewItem, ID_LIST_COLUMN, "" );

					// No subitems, because this is the Placeholder
					break;

				default:
					// Throw exception - Unsupported item type
					UCLIDException uclidException( "ELI01790", 
						"Cannot transfer unknown item type." );
					uclidException.addDebugInfo( "Item Type: ", pData->eType );
					throw uclidException;
					break;

				}		// end switch dwType
			}			// end for each item in Source List

			// Update display in Target List
			updateView( pTarget );

		}				// end if IDOK
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02064")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnItemView() 
{
	try
	{
		CListCtrl*	pList = NULL;
		if (m_iListWithFocus == 1)
		{
			// Use the Current Attributes list
			pList = &m_listCurrent;
		}
		else
		{
			// Use the Original Attributes list
			pList = &m_listOriginal;
		}

		// Retrieve record to be Viewed
		POSITION pos = pList->GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// Throw exception - Cannot View unselected items
			UCLIDException uclidException( "ELI01787", 
				"Cannot view unselected items." );
			throw uclidException;
		}
		else
		{
			while (pos)
			{
				// Get index of selected item
				int iItem = pList->GetNextSelectedItem( pos );

				// Get Item Data
				ITEMINFO*	pData = (ITEMINFO *)pList->GetItemData( iItem );

				// Call appropriate dialog
				switch( pData->eType )
				{
				case kStartPointType:
					{
						// Extract X and Y coordinates from Map
						string strCombined;
						string strX;
						string strY;
						bool bResult = getItemDataString( pData->mapData, 
							kArcStartingPoint, strCombined );
						if (bResult)
						{
							// Parse X and Y
							vector<string> vecTokens;
							StringTokenizer	st;
							st.parse( strCombined.c_str(), vecTokens );
							if (vecTokens.size() != 2)
							{
								// Throw exception
								UCLIDException ue( "ELI02228", 
									"Unable to parse starting point." );
								ue.addDebugInfo( "Input string", strCombined );
								throw ue;
							}

							// Retrieve parsed strings
							strX = vecTokens[0];
							strY = vecTokens[1];
						}
						else
						{
							// Throw exception - Unable to retrieve X and Y
							UCLIDException uclidException( "ELI02165", 
								"Unable to retrieve Start Point." );
							throw uclidException;
						}

						// Create and run the Part dialog in View mode
						CPartDlg	dlg( strX.c_str(), strY.c_str(), true, false );
						dlg.DoModal();
					}
					break;

				case kLineType:
					{
						// Retrieve Bearing and Distance strings
						string strBearing;
						string strDistance;
						bool bResult = getItemDataString( pData->mapData, 
							kLineBearing, strBearing );
						if (!bResult)
						{
							// Throw exception - Unable to retrieve Line Bearing
							UCLIDException uclidException( "ELI02166", 
								"Unable to retrieve Line Bearing." );
							throw uclidException;
						}
						bResult = getItemDataString( pData->mapData, 
							kLineDistance, strDistance );
						if (!bResult)
						{
							// Throw exception - Unable to retrieve Line Distance
							UCLIDException uclidException( "ELI02167", 
								"Unable to retrieve Line Distance." );
							throw uclidException;
						}

						// Create and run the Line dialog in View mode
						CLineDlg	dlg( strBearing.c_str(), strDistance.c_str(), 
							true, false, NULL );
						dlg.DoModal();
					}
					break;

				case kCurveType:
					{
						//////////////////////////////////////////
						// Determine which parameters are original
						//////////////////////////////////////////
						ECurveParameterType	eP1;
						ECurveParameterType	eP2;
						ECurveParameterType	eP3;
						bool bResult = getCurveParameters( pData->mapData, &eP1, 
							&eP2, &eP3 );
						if (!bResult)
						{
							// Throw exception - Unable to determine curve parameters
							UCLIDException uclidException( "ELI02168", 
								"Unable to determine curve parameters." );
							throw uclidException;
						}

						///////////////////////////////////////////
						// Retrieve three original parameter values
						///////////////////////////////////////////
						string	strP1;
						string	strP2;
						string	strP3;
						if (!getItemDataString( pData->mapData, eP1, strP1 ))
						{
							// Throw exception
							UCLIDException uclidException( "ELI02169", 
								"Unable to determine first curve parameter." );
							uclidException.addDebugInfo( "Parameter Type: ", eP1 );
							throw uclidException;
						}
						if (!getItemDataString( pData->mapData, eP2, strP2 ))
						{
							// Throw exception
							UCLIDException uclidException( "ELI02170", 
								"Unable to determine second curve parameter." );
							uclidException.addDebugInfo( "Parameter Type: ", eP2 );
							throw uclidException;
						}
						if (!getItemDataString( pData->mapData, eP3, strP3 ))
						{
							// Throw exception
							UCLIDException uclidException( "ELI02171", 
								"Unable to determine third curve parameter." );
							uclidException.addDebugInfo( "Parameter Type: ", eP3 );
							throw uclidException;
						}

						////////////////////////////////////
						// Retrieve Concavity and Angle Size
						////////////////////////////////////
						string	strTemp;
						int		iConcavity = 0;		// default, if not required
						int		iAngle = 0;			// default, if not required
						if (getItemDataString( pData->mapData, kArcConcaveLeft, 
							strTemp ))
						{
							// Parse the string
							iConcavity = atoi( strTemp.c_str() );
						}
						if (getItemDataString( pData->mapData, 
							kArcDeltaGreaterThan180Degrees, strTemp ))
						{
							// Parse the string
							iAngle = atoi( strTemp.c_str() );
						}

						// Create and run the Curve Calculator dialog in Edit mode
						CCurveCalculatorDlg	dlg(
							eP1, strP1.c_str(),		// 1st combo and edit
							eP2, strP2.c_str(),		// 2nd combo and edit
							eP3, strP3.c_str(),		// 3rd combo and edit
							iConcavity,				// Concave left/right
							iAngle,					// Angle > 180 or < 180
							true,					// Hide the Units radios
							true					// Hide the OK button
							);

						dlg.DoModal();
					}
					break;

				case kInvalidType:
					// Just ignore this item type, there is no data to view
					// This is an error condition
					break; 

				default:
					// Throw exception - Unknown item type
					UCLIDException uclidException( "ELI01791", 
						"Cannot view unknown item type." );
					uclidException.addDebugInfo( "Item Type: ", pData->eType );
					throw uclidException;
					break;
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02066")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnClickList1(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// Check selection status and update toolbar buttons
		doToolBarUpdates();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02068")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnClickList2(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// Check selection status and update toolbar buttons
		doToolBarUpdates();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02070")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnKillfocusList1(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// This list no longer has the focus
		m_iListWithFocus = 0;
		
		// Check selection status and update toolbar buttons
		doToolBarUpdates();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02072")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnKillfocusList2(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// This list no longer has the focus
		m_iListWithFocus = 0;
		
		// Check selection status and update toolbar buttons
		doToolBarUpdates();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02074")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnKeydownList1(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		LV_KEYDOWN* pLVKeyDow = (LV_KEYDOWN*)pNMHDR;
		
		// Check for Delete key
		if (pLVKeyDow->wVKey == VK_DELETE)
		{
			// Must first check to see if Delete is allowed
			if (canDeleteSelection( &m_listCurrent ))
			{
				OnItemDelete();
			}
		}
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02076")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnKeydownList2(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		LV_KEYDOWN* pLVKeyDow = (LV_KEYDOWN*)pNMHDR;
		
		// Check for Delete key
		if (pLVKeyDow->wVKey == VK_DELETE)
		{
			// Must first check to see if Delete is allowed
			if (canDeleteSelection( &m_listOriginal ))
			{
				OnItemDelete();
			}
		}
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02078")
}
//-------------------------------------------------------------------------------------------------
int CAttributeViewDlg::getNumParts(CListCtrl* pList)
{
	try
	{
		int		iPartNumber = 0;
		int		iCount = pList->GetItemCount();
		for (int i = 0; i < iCount; i++)
		{
			// Retrieve Item Data
			ITEMINFO*	pData = (ITEMINFO *)pList->GetItemData( i );

			// Determine Type of this item
			switch( pData->eType )
			{
			// Update Part count if this is a Start Point
			case kStartPointType:
				iPartNumber++;
				break;

			// Don't care about other types
			default:
				break;
			}
		}

		// Return count
		return iPartNumber;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02080")

	return 0;
}
//-------------------------------------------------------------------------------------------------
int CAttributeViewDlg::getNumSegments(CListCtrl* pList, int iPart)
{
	try
	{
		int		iPartNumber = 0;
		int		iSegmentNumber = 0;
		int		iCount = pList->GetItemCount();

		// Loop through items
		// Stop after finished with desired part
		for (int i = 0; ((i < iCount) && (iPartNumber <= iPart)); i++)
		{
			// Retrieve Item Data
			ITEMINFO*	pData = (ITEMINFO *)pList->GetItemData( i );

			// Determine Type of this item
			switch( pData->eType )
			{
			// Update Part count if this is a Start Point
			case kStartPointType:
				iPartNumber++;
				break;

			// Update Segment counter if Part numbers match
			case kLineType:
			case kCurveType:
				if (iPartNumber == iPart)
				{
					iSegmentNumber++;
				}
				break;

			// Don't care about other types
			default:
				break;
			}
		}

		// Return count
		return iSegmentNumber;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02082")

	return 0;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::hasDistinctBearings(CListCtrl* pList, int iPart)
{
	bool	bDistinct = false;
	bool	bFoundOne = false;
	double	dRadians = 0.0;
	double	dTest = 0.0;
	string	strTest;
	int		iPartNumber = 0;
	int		iCount = pList->GetItemCount();
	
	// Loop through items
	// Stop after finished with desired part
	// Stop after two different Bearings have been found
	for (int i = 0; ((i < iCount) && (iPartNumber <= iPart) && (!bDistinct)); i++)
	{
		// Retrieve Item Data
		ITEMINFO*	pData = (ITEMINFO *)pList->GetItemData( i );
		
		// Determine Type of this item
		switch( pData->eType )
		{
			// Update Part count if this is a Start Point
		case kStartPointType:
			iPartNumber++;
			break;
			
			// Evaluate Line Bearing in Radians
		case kLineType:
			{
				// Retrieve Bearing
				if (!getItemDataString( pData->mapData, kLineBearing, strTest ))
				{
					// Throw exception - Unable to retrieve Line Bearing
					UCLIDException uclidException( "ELI02172", 
						"Unable to retrieve Line Bearing." );
					throw uclidException;
				}
				
				// Convert Bearing to radians
				m_directionHelper.evaluateDirection( strTest );
				if (m_directionHelper.isDirectionValid())
				{
					dTest = m_directionHelper.getPolarAngleRadians();
				}
				else
				{
					// Throw exception - Unable to determine Line Bearing
					UCLIDException uclidException( "ELI02173", 
						"Unable to determine Line Bearing." );
					uclidException.addDebugInfo( "Line Bearing: ", strTest.c_str() );
					throw uclidException;
				}
				
				// Compare Bearings
				if (!bFoundOne)
				{
					// Just save this one and start comparisons later
					dRadians = dTest;
					bFoundOne = true;
				}
				else
				{
					// Check absolute difference
					if (abs( dTest - dRadians ) < 0.000001)
					{
						// Bearing effectively matches previous value,
						// Keep looking for another different one
					}
					else
					{
						// Bearing is different
						bDistinct = true;
					}
				}
			}
			break;
			
			// Evaluate Chord Bearing in Radians
		case kCurveType:
			{
				// Retrieve Chord Bearing from cell since it may not 
				// be available in map
				CString	zTest;
				zTest = pList->GetItemText( i, CHORDBEARING_LIST_COLUMN );
				if (zTest.IsEmpty())
				{
					// Throw exception - Unable to retrieve Chord Bearing
					UCLIDException uclidException( "ELI02174", 
						"Unable to retrieve Chord Bearing." );
					throw uclidException;
				}
				
				// Convert Chord Bearing to radians
				m_directionHelper.evaluateDirection( (LPCTSTR)zTest );
				if (m_directionHelper.isDirectionValid())
				{
					dTest = m_directionHelper.getPolarAngleRadians();
				}
				else
				{
					// Throw exception - Unable to determine Chord Bearing
					UCLIDException uclidException( "ELI02175", 
						"Unable to determine Chord Bearing." );
					uclidException.addDebugInfo( "Chord Bearing: ", 
						zTest.operator LPCTSTR() );
					throw uclidException;
				}
				
				// Compare Bearings
				if (!bFoundOne)
				{
					// Just save this one and start comparisons later
					dRadians = dTest;
					bFoundOne = true;
				}
				else
				{
					// Check absolute difference
					if (abs( dTest - dRadians ) < 0.000001)
					{
						// Bearing effectively matches previous value,
						// Keep looking for another different one
					}
					else
					{
						// Bearing is different
						bDistinct = true;
					}
				}
			}
			break;
			
			// Don't care about other types
		default:
			break;
			}
		}
		
		return bDistinct;
}
//-------------------------------------------------------------------------------------------------
IUCLDFeaturePtr CAttributeViewDlg::getFeature(int iList)
{
	int				iCount = 0;
	EFeatureType	eType;
	CListCtrl*		pList = NULL;
	
	// Getting Original Feature
	if (iList == 2)
	{
		// Use the Original Attributes list
		pList = &m_listOriginal;
		iCount = m_listOriginal.GetItemCount();
		
		// Check to see if Feature was defined
		if (m_ptrOrigFeature != NULL)
		{
			eType = m_ptrOrigFeature->getFeatureType();
		}
		else
		{
			// Try to use Current Feature type
			if (m_ptrCurrFeature != NULL)
			{
				eType = m_ptrCurrFeature->getFeatureType();
			}
			else
			{
				// Just default to polyline
				eType = kPolyline;
			}
		}
	}
	// Getting Current Feature
	else
	{
		// Use the Current Attributes list
		pList = &m_listCurrent;
		iCount = m_listCurrent.GetItemCount();
		
		// Check to see if Feature was defined
		if (m_ptrCurrFeature != NULL)
		{
			eType = m_ptrCurrFeature->getFeatureType();
		}
		else
		{
			// Try to use Original Feature type
			if (m_ptrOrigFeature != NULL)
			{
				eType = m_ptrOrigFeature->getFeatureType();
			}
			else
			{
				// Just default to polyline
				eType = kPolyline;
			}
		}
	}
	
	// Create a new Feature
	IUCLDFeaturePtr	ptrFeature(CLSID_Feature);
	ASSERT_RESOURCE_ALLOCATION("ELI01802", ptrFeature != NULL);
	
	// Define feature type
	ptrFeature->setFeatureType( eType );
	
	// Check to see if parts are still defined
	if (iCount > 0)
	{
		int			iPartNumber = 0;
		bool		bPartFound = false;
		IPartPtr	ptrPart;
		for (int i = 0; i < iCount; i++)
		{
			// Retrieve this data structure
			ITEMINFO*	pData = (ITEMINFO *)pList->GetItemData( i );
			
			// Behavior based on item type
			switch( pData->eType )
			{
			case kStartPointType:
				{
					//////////////////////////////////////
					// Close the existing Part, if present
					//////////////////////////////////////
					if (bPartFound)
					{
						// Add the part to the feature
						ptrFeature->addPart( ptrPart );
					}
					
					///////////////////////
					// Create a new PartPtr
					///////////////////////
					ptrPart.CreateInstance(CLSID_Part);
					ASSERT_RESOURCE_ALLOCATION("ELI01795", ptrPart != NULL);
					// Create new CartographicPoint object
					ICartographicPointPtr ptrPoint(CLSID_CartographicPoint);
					ASSERT_RESOURCE_ALLOCATION("ELI01794", ptrPoint != NULL);
					// Extract X and Y coordinates from Map
					string strCombined;
					string strX;
					string strY;
					bool bResult = getItemDataString( pData->mapData, 
						kArcStartingPoint, strCombined );
					if (bResult)
					{
						// Parse X and Y
						vector<string> vecTokens;
						StringTokenizer	st;
						st.parse( strCombined.c_str(), vecTokens );
						if (vecTokens.size() != 2)
						{
							// Throw exception
							UCLIDException ue( "ELI02230", 
								"Unable to parse starting point." );
							ue.addDebugInfo( "Input string", strCombined );
							throw ue;
						}
						
						// Retrieve parsed strings
						strX = vecTokens[0];
						strY = vecTokens[1];
					}
					else
					{
						// Throw exception - Unable to retrieve X and Y
						UCLIDException uclidException( "ELI02229", 
							"Unable to retrieve Start Point." );
						throw uclidException;
					}
					
					double	dX = 0.0;
					double	dY = 0.0;
					if (strX.length() > 0)
					{
						dX = asDouble( strX );
					}
					else
					{
						// Throw exception - Unable to retrieve start point X
						UCLIDException uclidException( "ELI01792", 
							"Failed to retrieve start point X value." );
						throw uclidException;
					}

					if (strY.length() > 0)
					{
						dY = asDouble( strY );
					}
					else
					{
						// Throw exception - Unable to retrieve start point Y
						UCLIDException uclidException( "ELI01793", 
							"Failed to retrieve start point Y value." );
						throw uclidException;
					}
					
					// Set X and Y data elements
					ptrPoint->InitPointInXY( dX, dY );
					
					// Attach this point to Part as the starting point
					ptrPart->setStartingPoint( ptrPoint );
					
					// Set flag
					bPartFound = true;
				}
				break;
				
			case kLineType:
				{
					// Make sure a Part has been created already
					if (bPartFound)
					{
						// Create a Line Segment
						ILineSegmentPtr	ptrLine(CLSID_LineSegment);
						ASSERT_RESOURCE_ALLOCATION("ELI01798", ptrLine != NULL);
						
						// Retrieve Bearing and Distance values from Map
						string	strBearing;
						string	strDistance;
						if (!getItemDataString( pData->mapData, 
							kLineBearing, strBearing ))
						{
							// Throw exception - Unable to retrieve Line Bearing
							UCLIDException uclidException( "ELI01796", 
								"Failed to retrieve line bearing value." );
							throw uclidException;
						}
						if (!getItemDataString( pData->mapData, 
							kLineDistance, strDistance ))
						{
							// Throw exception - Unable to retrieve Line Distance
							UCLIDException uclidException( "ELI01797", 
								"Failed to retrieve line distance value." );
							throw uclidException;
						}
						
						// The string from ListCtrl is in the form of current direction type
						// it needs to be converted into bearing format
						if (m_directionHelper.sGetDirectionType() != kBearingDir)
						{
							m_directionHelper.evaluateDirection(strBearing);
							double dDirectionValue = m_directionHelper.getPolarAngleRadians();
							// use the Bearing class to convert the double value of radians
							// into a Bearing format
							m_bearing.resetVariables();
							m_bearing.evaluateRadians(dDirectionValue);
							strBearing = m_bearing.interpretedValueAsString();
						}
						
						// Set parameters this line
						IIUnknownVectorPtr ipNewParams(CLSID_IUnknownVector);
						IParameterTypeValuePairPtr ipNewBearing(CLSID_ParameterTypeValuePair);
						ipNewBearing->eParamType = kLineBearing;
						ipNewBearing->strValue = _bstr_t(strBearing.c_str());
						ipNewParams->PushBack(ipNewBearing);
						IParameterTypeValuePairPtr ipNewDistance(CLSID_ParameterTypeValuePair);
						ipNewDistance->eParamType = kLineDistance;
						ipNewDistance->strValue = _bstr_t(strDistance.c_str());
						ipNewParams->PushBack(ipNewDistance);
						
						// Add the segment to the part
						IESSegmentPtr	ptrSegment(ptrLine);
						ptrSegment->setParameters(ipNewParams);
						ptrPart->addSegment(ptrSegment);
					}
					else
					{
						// Throw exception - Unexpected Line item
						UCLIDException uclidException( "ELI01799", 
							"Unexpected line object." );
						throw uclidException;
					}
				}
				break;
					
			case kCurveType:
				// Make sure a Part has been created already
				if (bPartFound)
				{
					// Add the curve information
					addCurveToPart( pData->mapData, ptrPart );
				}
				else
				{
					// Throw exception - Unexpected Curve item
					UCLIDException uclidException( "ELI01800", 
						"Unexpected curve object." );
					throw uclidException;
				}
				break;
				
			case kInvalidType:
				//////////////////////////////////////
				// Close the existing Part, if present
				//////////////////////////////////////
				if (bPartFound)
				{
					// Add the part to the feature
					ptrFeature->addPart( ptrPart );
				}
				break;
				
			default:
				// Throw exception - Unknown item type
				UCLIDException uclidException( "ELI01801", 
					"Unknown item object." );
				uclidException.addDebugInfo( "Item Type: ", pData->eType );
				throw uclidException;
				break;
			}		// end switch Item type
		}			// end for each item in list
	}				// end if List has items
	
	// Return Feature
	return ptrFeature;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::isCurrentFeatureEmpty()
{
	return m_bCurrentFeatureEmpty;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::isOriginalFeatureEmpty()
{
	return m_bOriginalFeatureEmpty;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::isCurrentFeatureValid()
{
	return m_bCurrentFeatureValid;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::isOriginalFeatureValid()
{
	return m_bOriginalFeatureValid;
}
//-------------------------------------------------------------------------------------------------
IUCLDFeaturePtr CAttributeViewDlg::getCurrentFeature()
{
	int iError = 0;
	
	// Check to see if the Feature was validated and calculated
	if (m_bCurrentFeatureValid)
	{
		// Call internal method
		return m_ptrFinalCurrFeature;
	}
	// Current Feature is not valid
	else if (!validateFeature( &m_listCurrent, &iError ))
	{
		return NULL;
	}
	// Current Feature is valid
	else
	{
		// Return the calculated Feature 
		return getFeature( 1 );
	}

	return NULL;
}
//-------------------------------------------------------------------------------------------------
IUCLDFeaturePtr CAttributeViewDlg::getOriginalFeature()
{
	int iError = 0;
	
	// Check to see if the Feature was validated and calculated
	if (m_bOriginalFeatureValid)
	{
		// Call internal method
		return m_ptrFinalOrigFeature;
	}
	// Original Feature is not valid
	else if (!validateFeature( &m_listOriginal, &iError ))
	{
		return NULL;
	}
	// Original Feature is valid
	else
	{
		// Return the calculated Feature 
		return getFeature( 2 );
	}

	return NULL;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::validateFeature(CListCtrl* pList, int* piErrorStringID)
{
	bool		bValid = false;
	IUCLDFeaturePtr	ptrFeature;
	IUCLDFeaturePtr	ptrOtherFeature;
	int			iNumParts = getNumParts( pList );
	int			i = 0;
	
	// Quick sanity check
	if ((pList == NULL) || (pList->m_hWnd == NULL)) 
	{
		// No data available
		return false;
	}
	
	////////////////////////
	// Which Feature is this
	////////////////////////
	if (pList == &m_listCurrent)
	{
		ptrFeature = m_ptrCurrFeature;
		ptrOtherFeature = m_ptrOrigFeature;
	}
	else if (pList == &m_listOriginal)
	{
		ptrFeature = m_ptrOrigFeature;
		ptrOtherFeature = m_ptrCurrFeature;
	}
	else
	{
		// Error
		return false;
	}
	
	////////////////////
	// Test this Feature
	////////////////////
	
	///////////////////////////
	// Test for zombie Segments
	///////////////////////////
	if (getNumSegments( pList, 0 ) > 0)
	{
		// Provide error string ID
		*piErrorStringID = IDS_ERR_ZOMBIE;
		// Quit checking, already found error
		return bValid;
	}
	
	// Determine Feature type
	EFeatureType	eFeature;
	if (ptrFeature == NULL)
	{
		// Check the other Feature
		if (ptrOtherFeature != NULL)
		{
			// Just default to this type
			eFeature = ptrOtherFeature->getFeatureType();
		}
		else
		{
			// Just default to polyline
			eFeature = kPolyline;
		}
	}
	else
	{
		// Just default to this type
		eFeature = ptrFeature->getFeatureType();
	}
	
	// Is the Feature a Polyline?
	if (eFeature == kPolyline)
	{
		/////////////////////////////
		// Check for too few Segments
		/////////////////////////////
		for (i = 1; i <= iNumParts; i++)
		{
			if (getNumSegments( pList, i ) < 1)
			{
				// Provide error string ID
				*piErrorStringID = IDS_ERRPOLYLINE_TOOFEW;
				// Quit checking, already found error
				return bValid;
			}
		}
		
		//////////////////////
		// Validation Success!
		//////////////////////
		*piErrorStringID = 0;
		bValid = true;
	}
	// Is the Feature a Polygon?
	else if (eFeature == kPolygon)
	{
		/////////////////////////////
		// Check for too few Segments
		/////////////////////////////
		for (i = 1; i <= iNumParts; i++)
		{
			if (getNumSegments( pList, i ) < 2)
			{
				// Provide error string ID
				*piErrorStringID = IDS_ERRPOLYGON_TOOFEW;
				// Quit checking, already found error
				return bValid;
			}
		}
		
		//////////////////////////////
		// Check for distinct Bearings
		//////////////////////////////
		for (i = 1; i <= iNumParts; i++)
		{
			if (!hasDistinctBearings( pList, i ))
			{
				// Provide error string ID
				*piErrorStringID = IDS_ERRPOLYGON_BEARING;
				// Quit checking, already found error
				return bValid;
			}
		}
		
		//////////////////////
		// Validation Success!
		//////////////////////
		*piErrorStringID = 0;
		bValid = true;
	}
	else
	{
		// Throw exception - Unknown Feature Type
		UCLIDException uclidException( "ELI02176", 
			"Unknown feature type." );
		uclidException.addDebugInfo( "Feature Type: ", eFeature );
		throw uclidException;
	}
	
	return bValid;
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::addCurveToPart(PTTVM mapData, IPartPtr ptrPart)
{
	// Create ArcSegment object
	IArcSegmentPtr	ptrArc(CLSID_ArcSegment);
	ASSERT_RESOURCE_ALLOCATION("ELI12486", ptrArc != NULL);
	
	/////////////////////////////////////////////
	// Prepare the Parameters vector for this arc
	/////////////////////////////////////////////
	IIUnknownVectorPtr	ptrVector(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI02177", ptrVector != NULL);
	
	///////////////////////////////////////////
	// Loop through Curve Parameters in the Map
	///////////////////////////////////////////
	ECurveParameterType eType;
	string				strValue;
	
	for (PTTVM::const_iterator itData = mapData.begin(); itData != mapData.end(); itData++)
	{
		// Retrieve this key
		eType = itData->first;
		
		// Retrieve this value
		strValue = itData->second;
		// If the parameter is some bearing type, then the
		// string for that parameter stored in mapData is in
		// the form of current direction type. It needs to be
		// converted into Bearing format in order to be stored
		// in Segment
		switch (eType)
		{
			// bearing parameter types
		case kArcTangentInBearing:
		case kArcTangentOutBearing:
		case kArcChordBearing:
		case kArcRadialInBearing:
		case kArcRadialOutBearing:
			{
				// only do the conversion if the current direction type is
				// polar angle or azimuth
				if (m_directionHelper.sGetDirectionType() != kBearingDir)
				{
					m_directionHelper.evaluateDirection(strValue);
					double dDirectionValue = m_directionHelper.getPolarAngleRadians();
					// use the Bearing class to convert the double value of radians
					// into a Bearing format
					m_bearing.resetVariables();
					m_bearing.evaluateRadians(dDirectionValue);
					// get the string form for the bearing (ex. N23d34m23sW)
					strValue = m_bearing.interpretedValueAsString();
				}
			}
			break;
		}
		
		// Prepare a Type/Value pair for this parameter
		IParameterTypeValuePairPtr	ptrParam(CLSID_ParameterTypeValuePair);
		ASSERT_RESOURCE_ALLOCATION("ELI02178", ptrParam != NULL);
		// Set the parameter type and value		
		ptrParam->eParamType = eType;
		ptrParam->strValue = _bstr_t(strValue.c_str());
		
		// Add parameter to vector
		ptrVector->PushBack(ptrParam);
		
	}			// end for each curve parameter in the Map
	
	/////////////////////
	// Set the parameters
	/////////////////////
	IESSegmentPtr	ptrSegment(ptrArc);
	ptrSegment->setParameters(ptrVector);

	//////////////////////////////////////
	// Add the segment to the desired part
	//////////////////////////////////////
	ptrPart->addSegment( ptrSegment );
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::getItemDataString(PTTVM mapData, 
										  ECurveParameterType eType, 
										  std::string& rstrText)
{
	bool bFound = false;
	
	// Look for an entry of the specified type
	PTTVM::iterator	iter = mapData.end();
	iter = mapData.find( eType );
	
	// Retrieve string if found
	if (iter != mapData.end())
	{
		rstrText = iter->second;
		bFound = true;
	}
	else
	{
		rstrText = "";
	}
	
	// Return
	return bFound;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeViewDlg::getCurveParameters(PTTVM &mapData, 
										   ECurveParameterType* peP1, 
										   ECurveParameterType* peP2, 
										   ECurveParameterType* peP3)
{
	bool				bFound = false;
	bool				bDone = false;
	int					iNextParam = 1;
	string				strValue;
	ECurveParameterType eType;
	
	// Create a new CCE Helper object
	m_pEngine->reset();
	CCEHelper cceHelper( m_pEngine );
	
	// Loop through map items
	for (PTTVM::const_iterator itData = mapData.begin(); (itData != mapData.end() && !bDone); itData++)
	{
		// Retrieve this key
		eType = itData->first;
		
		// Retrieve this value
		strValue = itData->second;
		
		// if the curve parameter is of some bearing type, the 
		// string retrieved from ListCtrl is in the
		// format of current direction type, therefore, it needs
		// to be converted into polar angle value to calculate other
		// parameter values via CCE since CCE only deals with polar
		// angle value
		switch (eType)
		{
			// bearing parameter types
		case kArcTangentInBearing:
		case kArcTangentOutBearing:
		case kArcChordBearing:
		case kArcRadialInBearing:
		case kArcRadialOutBearing:
			{
				// use direction helper to evaluate the string first
				m_directionHelper.evaluateDirection(strValue);
				// set curve parameter value
				m_pEngine->setCurveAngleOrBearingParameter(
					eType, m_directionHelper.getPolarAngleRadians() );
			}
			break;
		default:
			{
				// Provide data to CCE Helper
				cceHelper.setCurveParameter( eType, strValue );
			}
			break;
		}			
		
		// Check type
		switch( eType )
		{
			// Just skip over these parameters, they don't fit 
			// into the Curve Calculator dialog combo boxes
		case kArcConcaveLeft:
		case kArcDeltaGreaterThan180Degrees:
		case kArcStartingPoint:
		case kArcMidPoint:
		case kArcEndingPoint:
		case kArcCenter:
		case kArcExternalPoint:
		case kArcChordMidPoint:
			break;
			
			// Each of these parameters can be used in the 
			// Curve Calculator dialog combo boxes
		case kArcDelta:
		case kArcStartAngle:
		case kArcEndAngle:
		case kArcDegreeOfCurveChordDef:
		case kArcDegreeOfCurveArcDef:
		case kArcTangentInBearing:
		case kArcTangentOutBearing:
		case kArcChordBearing:
		case kArcRadialInBearing:
		case kArcRadialOutBearing:
		case kArcRadius:
		case kArcLength:
		case kArcChordLength:
		case kArcExternalDistance:
		case kArcMiddleOrdinate:
		case kArcTangentDistance:
			{
				switch( iNextParam )
				{
				case 1:
					// This parameter will be in the first combo box
					*peP1 = eType;
					// Increment counter
					++iNextParam;
					break;
					
				case 2:
					// This parameter will be in the second combo box
					*peP2 = eType;
					// Increment counter
					++iNextParam;
					break;
					
				case 3:
					// This parameter will be in the third combo box
					*peP3 = eType;
					// Increment counter
					++iNextParam;
					// Done looking
					bDone = true;
					bFound = true;
					break;
					
				default:
					// Error condition
					break;
				}
			}
			break;
			
			// Unexpected parameter types for a curve
		case kLineBearing:
		case kLineDistance:
		case kInvalidParameterType:
		default:
			break;
		}
	}					// end for each element in Map
	
	/////////////////////////////////////////
	// Done looping, enough parameters found?
	/////////////////////////////////////////
	if (!bFound)
	{
		// Not enough found, use CCE Helper
		
		// Add Concavity in case it's needed
		if (m_pEngine->canCalculateParameter( kArcConcaveLeft ))
		{
			mapData[kArcConcaveLeft] = cceHelper.getCurveParameter( 
				kArcConcaveLeft );
		}
		
		// Add Angle Size in case it's needed
		if (m_pEngine->canCalculateParameter( kArcDeltaGreaterThan180Degrees ))
		{
			mapData[kArcDeltaGreaterThan180Degrees] = 
				cceHelper.getCurveParameter( kArcDeltaGreaterThan180Degrees );
		}
		
		/////////////////////////////////////////////
		// Default collection of parameters includes:
		//   Chord Bearing,
		//   Chord Length,
		//   Radius
		/////////////////////////////////////////////
		
		// Add Chord Bearing as a default parameter
		if (m_pEngine->canCalculateParameter( kArcChordBearing ))
		{
			// Make sure that Chord Bearing is not already in the map
			if (!getItemDataString( mapData, kArcChordBearing, strValue ))
			{
				// convert the bearing string into the current direction string
				mapData[kArcChordBearing] = m_directionHelper.polarAngleInRadiansToDirectionInString(
					cceHelper.getCurveBearingAngleOrDistanceInDouble( kArcChordBearing ) );
				
				// No parameters found yet
				if (iNextParam == 1)
				{
					// This is the first parameter
					*peP1 = kArcChordBearing;
					++iNextParam;
				}
				// Found only one parameter so far
				else if (iNextParam == 2)
				{
					// This is the second parameter
					*peP2 = kArcChordBearing;
					++iNextParam;
				}
				// Found only two parameters so far
				else if (iNextParam == 3)
				{
					// This is the third and last parameter
					*peP3 = kArcChordBearing;
					++iNextParam;
					return true;
				}
			}			// end if Chord Bearing not already in Map
		}				// end if Chord Bearing available in Helper
		
		// Add Chord Length as a default parameter
		if (m_pEngine->canCalculateParameter( kArcChordLength ))
		{
			// Make sure that Chord Bearing is not already in the map
			if (!getItemDataString( mapData, kArcChordLength, strValue ))
			{
				string strChordLength = cceHelper.getCurveParameter(kArcChordLength);
				// Add it to the map
				mapData[kArcChordLength] = convertInCurrentDistanceUnit(strChordLength);
					
				
				// No parameters found yet
				if (iNextParam == 1)
				{
					// This is the first parameter
					*peP1 = kArcChordLength;
					++iNextParam;
				}
				// Found only one parameter so far
				else if (iNextParam == 2)
				{
					// This is the second parameter
					*peP2 = kArcChordLength;
					++iNextParam;
				}
				// Found only two parameters so far
				else if (iNextParam == 3)
				{
					// This is the third and last parameter
					*peP3 = kArcChordLength;
					++iNextParam;
					return true;
				}
			}			// end if Chord Length not already in Map
		}				// end if Chord Length available in Helper
		
		// Add Radius as a default parameter
		if (m_pEngine->canCalculateParameter( kArcRadius ))
		{
			// Make sure that Radius is not already in the map
			if (!getItemDataString( mapData, kArcRadius, strValue ))
			{
				string strRadius = cceHelper.getCurveParameter(kArcRadius);
				// Add it to the map
				mapData[kArcRadius] = convertInCurrentDistanceUnit(strRadius);
				
				// No parameters found yet
				if (iNextParam == 1)
				{
					// This is the first parameter
					*peP1 = kArcRadius;
					++iNextParam;
				}
				// Found only one parameter so far
				else if (iNextParam == 2)
				{
					// This is the second parameter
					*peP2 = kArcRadius;
					++iNextParam;
				}
				// Found only two parameters so far
				else if (iNextParam == 3)
				{
					// This is the third and last parameter
					*peP3 = kArcRadius;
					++iNextParam;
					return true;
				}
			}			// end if Radius not already in Map
		}				// end if Radius available in Helper
	}					// end if not enough parameters found
		
	return bFound;
}
//-------------------------------------------------------------------------------------------------
int CAttributeViewDlg::DoModal() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride( gAVDllResource );

	try
	{
		return CDialog::DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02105")

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnContextMenu(CWnd* pWnd, CPoint point) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride( gAVDllResource );

	try
	{
		// Create and load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_AVCONTEXTMENU );

		////////////////////////////////
		// Enable and disable menu items
		////////////////////////////////
		CListCtrl*	pList = NULL;

		// Current List has focus
		if (m_iListWithFocus == 1)
		{
			pList = &m_listCurrent;
		}
		// Original List has focus
		else if (m_iListWithFocus == 2)
		{
			pList = &m_listOriginal;
		}

		// No List has focus
		if (pList == NULL)
		{
			// All items are disabled
			menu.EnableMenuItem( ID_ITEM_EDIT, 
				MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			menu.EnableMenuItem( ID_ITEM_DELETE, 
				MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			menu.EnableMenuItem( ID_INSERT_PART, 
				MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			menu.EnableMenuItem( ID_INSERT_LINE, 
				MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			menu.EnableMenuItem( ID_INSERT_CURVE, 
				MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
		}
		// A List has focus
		else
		{
			// Check Edit
			if (canEditSelection( pList ))
			{
				menu.EnableMenuItem( ID_ITEM_EDIT, 
					MF_BYCOMMAND | MF_ENABLED );
			}
			else
			{
				menu.EnableMenuItem( ID_ITEM_EDIT, 
					MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			}

			// Check Delete
			if (canDeleteSelection( pList ))
			{
				menu.EnableMenuItem( ID_ITEM_DELETE, 
					MF_BYCOMMAND | MF_ENABLED );
			}
			else
			{
				menu.EnableMenuItem( ID_ITEM_DELETE, 
					MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			}

			// Check Append Part
			if (canAppendPart( pList, true ))
			{
				menu.EnableMenuItem( ID_INSERT_PART, 
					MF_BYCOMMAND | MF_ENABLED );
			}
			else
			{
				menu.EnableMenuItem( ID_INSERT_PART, 
					MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			}

			// Check Insert Line
			if (canInsertLine( pList ))
			{
				menu.EnableMenuItem( ID_INSERT_LINE, 
					MF_BYCOMMAND | MF_ENABLED );
			}
			else
			{
				menu.EnableMenuItem( ID_INSERT_LINE, 
					MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			}

			// Check Insert Curve
			if (canInsertCurve( pList ))
			{
				menu.EnableMenuItem( ID_INSERT_CURVE, 
					MF_BYCOMMAND | MF_ENABLED );
			}
			else
			{
				menu.EnableMenuItem( ID_INSERT_CURVE, 
					MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
			}
		}

		//////////////////////////////////
		// Position the menu on the screen
		//////////////////////////////////
		menu.GetSubMenu(0)->TrackPopupMenu( TPM_LEFTALIGN | TPM_RIGHTBUTTON, 
			point.x, point.y, this, NULL );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02108")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnDblclkList1(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		CListCtrl*	pList = &m_listCurrent;

		// Check to see if Edit is enabled for this selection
		if (canEditSelection( pList ))
		{
			// Just call the Edit handler
			OnItemEdit();
		}
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02110")
}
//-------------------------------------------------------------------------------------------------
void CAttributeViewDlg::OnDblclkList2(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		CListCtrl*	pList = &m_listOriginal;

		// Check to see if Edit is enabled for this selection
		if (canEditSelection( pList ))
		{
			// Just call the Edit handler
			OnItemEdit();
		}
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02112")
}
//-------------------------------------------------------------------------------------------------
