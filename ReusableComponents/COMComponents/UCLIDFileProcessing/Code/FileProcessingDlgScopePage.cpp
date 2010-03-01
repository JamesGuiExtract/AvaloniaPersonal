// FileProcessingDlgScopePage.cpp : implementation file
//
#include "stdafx.h"
#include "resource.h"
#include "UCLIDFileProcessing.h"
#include "FilePriorityHelper.h"
#include "FileProcessingDlgScopePage.h"
#include "FileProcessingManager.h"
#include "FPCategories.h"

#include <FileProcessingConfigMgr.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <FileDialogEx.h>
#include <XBrowseForFolder.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <string>
#include <vector>
#include <TextFunctionExpander.h>
#include <misc.h>
#include <COMUtils.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string	gstrINACTIVE = "Inactive";
const string	gstrACTIVE = "Active";
const string	gstrPAUSED = "Paused";
const string	gstrSTOPPED = "Stopped";
const string	gstrDONE = "Done";

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgScopePage property page
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(FileProcessingDlgScopePage, CPropertyPage)
//-------------------------------------------------------------------------------------------------
FileProcessingDlgScopePage::FileProcessingDlgScopePage() 
: CPropertyPage(FileProcessingDlgScopePage::IDD),
  m_ipClipboardMgr(NULL),
  m_cfgMgr(NULL), 
  m_ipMiscUtils(NULL),
  m_bEnabled(true),
  m_bInitialized(false)
{
	try
	{
		m_zConditionDescription = _T("");
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27600");
}
//-------------------------------------------------------------------------------------------------
FileProcessingDlgScopePage::~FileProcessingDlgScopePage()
{
	try
	{
		// Ensure COM pointers are released
		m_ipClipboardMgr = NULL;
		m_ipMiscUtils = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16528");
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::refresh()
{
	// Clear listed File Suppliers and the FAM Condition
	m_wndGrid.Clear();
	m_zConditionDescription = "";

	// Add each File Supplier to the list
	IIUnknownVectorPtr ipFileSuppliersData = getFSMgmtRole()->FileSuppliers;
	ASSERT_RESOURCE_ALLOCATION("ELI14259", ipFileSuppliersData != NULL);

	int iCount = ipFileSuppliersData->Size();
	for (int i = 1; i <= iCount; i++)
	{
		// Retrieve this File Supplier Data object
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSD = ipFileSuppliersData->At(i - 1);
		ASSERT_RESOURCE_ALLOCATION("ELI13733", ipFSD != NULL);

		// Update this row in the grid
		updateList( i, ipFSD );
	}

	// Update the FAM Condition
	IObjectWithDescriptionPtr ipConditionObjWithDesc = getFSMgmtRole()->FAMCondition;
	if (ipConditionObjWithDesc->Object != NULL)
	{
		// get the FAM condition's description
		_bstr_t _bstrText = ipConditionObjWithDesc->GetDescription();
		m_zConditionDescription = (const char*)_bstrText;
	}

	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::ResetInitialized()
{
	m_bInitialized = false;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::setConfigMgr(FileProcessingConfigMgr* cfgMgr)
{
	m_cfgMgr = cfgMgr;
	ASSERT_RESOURCE_ALLOCATION("ELI08915", m_cfgMgr != NULL);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::setEnabled(bool bEnabled)
{
	m_bEnabled = bEnabled;

	// Call setButtonStates() to set the status of the controls
	setButtonStates();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::setFPMgr(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPMgr)
{
	m_pFPM = pFPMgr;
	ASSERT_RESOURCE_ALLOCATION("ELI14078", m_pFPM != NULL);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::updateSupplierStatus(WPARAM wParam, LPARAM lParam)
{
	// Create local new status
	UCLID_FILEPROCESSINGLib::EFileSupplierStatus eNewStatus = 
		(UCLID_FILEPROCESSINGLib::EFileSupplierStatus)wParam;

	if (lParam == NULL)
	{
		// Refresh entire grid
		refresh();
	}
	else
	{
		// Step through each file supplier
		long lCount = m_wndGrid.GetRowCount();
		for (int i = 1; i <= lCount; i++)
		{
			// Retrieve this File Supplier
			UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipThisFS = getFileSupplier( i );
			ASSERT_RESOURCE_ALLOCATION("ELI14043", ipThisFS != NULL);

			// Compare File Suppliers
			if ((LPARAM)ipThisFS.GetInterfacePtr() == lParam)
			{
				// Update the status
				setStatus( i, eNewStatus );
				break;
			}
		}
	}

	// Update action buttons
	setButtonStates();
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgScopePage::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			// translate accelerators
			static HACCEL hAccel = LoadAccelerators(AfxGetApp()->m_hInstance, 
				MAKEINTRESOURCE(IDR_ACCELERATORS));
			if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
			{
				// since the message has been handled, no further dispatch is needed
				return TRUE;
			}
		}

		// Display the context menu for skip condition
		if (pMsg->message == WM_RBUTTONDOWN)
		{
			// If the user right click on the FAM condition text box
			if(m_bInitialized && m_editSelectCondition.GetSafeHwnd() == pMsg->hwnd)
			{   
				// Display the context menu
				displayContextMenu();   
			}   
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15270")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BTN_REMOVE, m_btnRemove);
	DDX_Control(pDX, IDC_BTN_CONFIGURE, m_btnConfigure);
	DDX_Control(pDX, IDC_BTN_ADD, m_btnAdd);
	DDX_Text(pDX, IDC_EDIT_CONDITION, m_zConditionDescription);
	DDX_Control(pDX, IDC_BTN_FAMCONDITION, m_btnSelectCondition);
	DDX_Control(pDX, IDC_EDIT_CONDITION, m_editSelectCondition);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlgScopePage, CPropertyPage)
	ON_BN_CLICKED(IDC_BTN_ADD, OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_REMOVE, OnBtnRemove)
	ON_BN_CLICKED(IDC_BTN_CONFIGURE, OnBtnConfigure)
	ON_WM_SIZE()
	ON_WM_CREATE()
	ON_COMMAND(ID_CONTEXT_CUT, &FileProcessingDlgScopePage::OnContextCut)
	ON_COMMAND(ID_CONTEXT_COPY, &FileProcessingDlgScopePage::OnContextCopy)
	ON_COMMAND(ID_CONTEXT_PASTE, &FileProcessingDlgScopePage::OnContextPaste)
	ON_COMMAND(ID_CONTEXT_DELETE, &FileProcessingDlgScopePage::OnContextDelete)
	ON_BN_CLICKED(IDC_BTN_FAMCONDITION, OnBtnSelectCondition)
	ON_MESSAGE(WM_NOTIFY_GRID_LCLICK, OnLButtonClkRowCol)
	ON_MESSAGE(WM_NOTIFY_CELL_DBLCLK, OnLButtonDblClkRowCol)
	ON_MESSAGE(WM_NOTIFY_CELL_MODIFIED, OnModifyCell)
	ON_STN_DBLCLK(IDC_EDIT_CONDITION, &FileProcessingDlgScopePage::OnDoubleClickCondition)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgScopePage message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgScopePage::OnInitDialog() 
{
	CPropertyPage::OnInitDialog();
	
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Please refer to the MFC documentation on 
		// SubclassDlgItem for information on this 
		// call. This makes sure that our C++ grid 
		// window class subclasses the window that 
		// is created with the User Control.
		m_wndGrid.SubclassDlgItem( IDC_GRID, this );

		// Prepare Header labels
		vector<string>	vecHeader;
		vecHeader.push_back("Enabled");
		vecHeader.push_back("Force Processing");
		vecHeader.push_back("Priority");
		vecHeader.push_back("Description");
		vecHeader.push_back("Status");

		// Prepare Column widths
		vector<int>	vecWidths;
		vecWidths.push_back( 70 );
		vecWidths.push_back( 120 );
		vecWidths.push_back( 100 );
		vecWidths.push_back( 0 );		// Will be resized by DoResize()
		vecWidths.push_back( 80 );

		// Get the priority strings
		vector<string> vecPriorities;
		getPrioritiesVector(vecPriorities);

		// Setup the grid control
		//    5 columns of header labels
		//    5 columns of column widths
		//	  A list of priorities for the drop down column
		m_wndGrid.PrepareGrid( vecHeader, vecWidths, vecPriorities );
		m_wndGrid.SetControlID( IDC_GRID );
		m_wndGrid.GetParam()->EnableUndo(TRUE);

		// Resize the Picture control around the Grid
		CWnd*	pPicture = GetDlgItem( IDC_PICTURE );
		if (pPicture != NULL)
		{
			// Get the Grid dimensions
			CRect	rectGrid;
			m_wndGrid.GetWindowRect( rectGrid );
			
			// Adjust rect size
			ScreenToClient( rectGrid );
			rectGrid.left -= 1;
			rectGrid.top -= 1;
			rectGrid.right += 1;
			rectGrid.bottom += 1;
			
			// Set Picture size
			pPicture->MoveWindow( &rectGrid );
		}

		// Disable File Supplier buttons
		setButtonStates();

		// Set m_bInitialized to true so that 
		// next call to OnSize() will not be skipped
		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08907")
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnBtnAdd() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// create a new ObjectWithDescription for the user to select or configure
		IObjectWithDescriptionPtr ipObject(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI13683", ipObject != NULL);

		// allow the user to select and configure ipObject
		VARIANT_BOOL vbResult = getMiscUtils()->AllowUserToSelectAndConfigureObject(ipObject,
			"File Supplier", get_bstr_t(FP_FILE_SUPP_CATEGORYNAME), VARIANT_FALSE, 0, NULL);

		// Check result
		if (vbResult == VARIANT_TRUE)
		{
			// Retrieve the description
			_bstr_t	bstrText = ipObject->GetDescription();
			CString	zText((const char*)bstrText);

			// Get list count and index of previously selected File Supplier
			int iCount = m_wndGrid.GetRowCount();
			int iSelectedRow = m_wndGrid.GetFirstSelectedRow();

			// Populate vector of strings for text cells
			vector<string>	vecText;
			vecText.push_back( (LPCTSTR)zText );
			vecText.push_back( gstrINACTIVE.c_str() );

			// Determine index of new row
			// Insert position is after the first selected item (P13 #4732)
			int iNewIndex = (iSelectedRow > 0) ? iSelectedRow + 1 : iCount + 1;

			// Insert or append a new row
			m_wndGrid.InsertRows( iNewIndex, 1 );
			
			// Add info: Enabled, not Forced, Priority, Description, Inactive
			m_wndGrid.SetRowInfo( iNewIndex, true, false,
				getPriorityString((UCLID_FILEPROCESSINGLib::EFilePriority)kPriorityDefault),
				vecText );

			// Create a new FileSupplierData object with Not Forced
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSD( CLSID_FileSupplierData );
			ASSERT_RESOURCE_ALLOCATION( "ELI13730", ipFSD != NULL );
			ipFSD->ForceProcessing = VARIANT_FALSE;

			// Insert the object-with-description into the File Supplier Data object and
			// Insert the File Supplier Data object into the vector
			ipFSD->FileSupplier = ipObject;
			getFileSuppliersData()->Insert( iNewIndex - 1, ipFSD );

			// Select the new entry
			m_wndGrid.SetSelection( 0 );
			CGXRangeList* pList = m_wndGrid.GetParam()->GetRangeList();
			ASSERT_RESOURCE_ALLOCATION("ELI15639", pList != NULL);
			POSITION area = pList->AddTail(new CGXRange);
			m_wndGrid.SetSelection( area, iNewIndex, 1, iNewIndex, 5 );

			// Update the display
			UpdateData( FALSE );

			// Update button states
			setButtonStates();

			// Update the UI, menu and toolbar items
			updateUI();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08908")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnBtnRemove() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Handle single-selection case, 
		// get index of selected File Supplier
		int iSelectedRow = m_wndGrid.GetFirstSelectedRow();
		if (iSelectedRow > 0)
		{
			// Retrieve current file processor description
			int		iResult;
			CString	zDescription = m_wndGrid.GetValueRowCol( iSelectedRow, 3 );

			// Create prompt for confirmation
			CString	zPrompt;
			zPrompt.Format( "Are you sure that file supplier '%s' should be deleted?", 
				zDescription );

			// Present MessageBox
			iResult = MessageBox( (LPCTSTR)zPrompt, "Confirm Delete", 
				MB_YESNO | MB_ICONQUESTION );

			// Act on response
			if (iResult == IDYES)
			{
				// Remove this FSD object from the collection
				getFileSuppliersData()->Remove( iSelectedRow - 1 );

				// Refresh the display
				refresh();

				// Select next or last File Supplier
				int iSize = m_wndGrid.GetRowCount();
				if (iSelectedRow > iSize)
				{
					iSelectedRow = iSize;
				}
				if (iSelectedRow > 0)
				{
					m_wndGrid.SelectRow( iSelectedRow );
				}

				// Update button states
				setButtonStates();

				// Update the UI, menu and toolbar items
				updateUI();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08910")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnBtnConfigure() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Handle single-selection case, 
		// get index of selected File Supplier
		int iSelectedRow = m_wndGrid.GetFirstSelectedRow();
		if (iSelectedRow > 0)
		{
			// Just return if this row is Locked
			if (m_wndGrid.GetRowLock( iSelectedRow ))
			{
				return;
			}

			// Retrieve selected File Supplier Data
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr	ipFSD = getFileSuppliersData()->At( iSelectedRow - 1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI13936", ipFSD != NULL );

			// get the position and dimensions of the command button
			RECT rectCommandButton;
			getDlgItemWindowRect(IDC_BTN_CONFIGURE, rectCommandButton);

			// allow the user to modify the file supplier
			VARIANT_BOOL vbResult = getMiscUtils()->HandlePlugInObjectCommandButtonClick(
				ipFSD->FileSupplier, "File Supplier", get_bstr_t(FP_FILE_SUPP_CATEGORYNAME),
				VARIANT_FALSE, 0, NULL, rectCommandButton.right, rectCommandButton.top);

			// Check result
			if (vbResult == VARIANT_TRUE)
			{
				// remove and re-insert the associated FileSupplierData object in the
				// vector so that the vector becomes dirty (and therefore subsequent attempts to
				// discard the changes will cause a user confirmation)
				getFileSuppliersData()->Remove(iSelectedRow - 1 );
				getFileSuppliersData()->Insert(iSelectedRow - 1, ipFSD);

				// Refresh the list
				refresh();

				// Retain selection
				m_wndGrid.SelectRow( iSelectedRow );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13941")
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlgScopePage::OnLButtonClkRowCol(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// Extract Row and Column
		int nRow = LOWORD( lParam );
		int nCol = HIWORD( lParam );

		// Simply return if click on the header of the grid
		// Fix [P13: 3930] L. L Song
		// OR if click is not in one of the check box columns
		if (nRow <= 0 || (nCol != 1 && nCol != 2))
		{
			// Call setButtonStates() to disable 
			// remove and configure butttons
			setButtonStates();
			return 0;
		}

		// Retrieve the File Supplier Data
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSD = getFileSuppliersData()->At( nRow - 1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI14085", ipFSD != NULL );

		// Get the new setting
		CString zTemp = m_wndGrid.GetCellValue( nRow, nCol );
		long nCheck = asLong( (LPCTSTR)zTemp );

		// Update Enabled flag
		if (nCol == 1)
		{
			// Retrieve the Object With Description
			IObjectWithDescriptionPtr ipObjWD = ipFSD->FileSupplier;
			ASSERT_RESOURCE_ALLOCATION( "ELI14086", ipObjWD != NULL );

			// Update the Enabled flag
			ipObjWD->Enabled = asVariantBool(nCheck == 1);
			// Update the UI
			updateUI();
		}
		// Update Force Processing flag
		else if (nCol == 2)
		{
			// Update the Force Processing flag
			ipFSD->ForceProcessing = asVariantBool(nCheck == 1);
		}

		// Update button states based on selected File Supplier
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13696")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlgScopePage::OnLButtonDblClkRowCol(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// get the index of the selected row
		int iSelectedRow = m_wndGrid.GetFirstSelectedRow();
		if (iSelectedRow > 0)
		{
			// exit if the row is locked
			if (m_wndGrid.GetRowLock( iSelectedRow ))
			{
				return 0;
			}

			// retrieve the selected file supplier data
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr 
				ipFileSuppliersData = getFileSuppliersData()->At( iSelectedRow - 1 );
			ASSERT_RESOURCE_ALLOCATION("ELI16085", ipFileSuppliersData != NULL );

			// allow the user to configure the selected file supplier
			VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectDoubleClick(ipFileSuppliersData->FileSupplier,
				"File Supplier", get_bstr_t(FP_FILE_SUPP_CATEGORYNAME), VARIANT_FALSE, 0, NULL);

			if (vbDirty == VARIANT_TRUE)
			{
				// remove and re-insert the associated FileSupplierData object in the
				// vector so that the vector becomes dirty (and therefore subsequent attempts to
				// discard the changes will cause a user confirmation)
				getFileSuppliersData()->Remove(iSelectedRow - 1 );
				getFileSuppliersData()->Insert(iSelectedRow - 1, ipFileSuppliersData);

				// Refresh the list
				refresh();

				// Retain selection
				m_wndGrid.SelectRow( iSelectedRow );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19417");

	return 0;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPropertyPage::OnSize(nType, cx, cy);

		// first call to this function shall be ignored
		if (!m_bInitialized) 
		{
			return;
		}

		// Declare CRect variables
		static bool bInit = false;
		static int nLen1, nLen2, nAddButtonWidth;
		CRect rectDlg;
		CRect rectLabelGrid, rectGrid;
		CRect rectAddButton, rectRemoveButton, rectConfigureButton;
		CRect rectLabelCondition, rectConditionDescription, rectSelectCondition;

		// Get original sizes and positions
		getDlgItemWindowRect(IDC_BTN_ADD, rectAddButton);
		ScreenToClient(&rectAddButton);
		getDlgItemWindowRect(IDC_BTN_REMOVE, rectRemoveButton);
		ScreenToClient(&rectRemoveButton);
		getDlgItemWindowRect(IDC_BTN_CONFIGURE, rectConfigureButton);
		ScreenToClient(&rectConfigureButton);
		getDlgItemWindowRect(IDC_STATIC_SUPPLIER, rectLabelGrid);
		ScreenToClient(&rectLabelGrid);
		getDlgItemWindowRect(IDC_GRID, rectGrid);
		ScreenToClient(&rectGrid);
		
		getDlgItemWindowRect(IDC_STATIC_CONDITION, rectLabelCondition);
		ScreenToClient(&rectLabelCondition);
		getDlgItemWindowRect(IDC_EDIT_CONDITION, rectConditionDescription);
		ScreenToClient(&rectConditionDescription);
		getDlgItemWindowRect(IDC_BTN_FAMCONDITION, rectSelectCondition);
		ScreenToClient(&rectSelectCondition);

		if (!bInit)
		{
			GetClientRect(&rectDlg);

			// Save distance between Grid and buttons
			nLen1 = rectAddButton.left - rectGrid.right;
			// Save distance between Grid and FAM Condition label
			nLen2 = rectLabelCondition.top - rectGrid.bottom;
			// Save width of Add button
			nAddButtonWidth = rectAddButton.Width();
			
			bInit = true;
		}
		
		// get dialog rect
		GetClientRect(&rectDlg);

		// Compute delta height
		long tmp = rectGrid.bottom;
		rectGrid.bottom = rectDlg.bottom - nLen1;
		long dh = rectGrid.bottom - tmp;

		// move buttons
		rectAddButton.right = rectDlg.right - nLen1;
		rectAddButton.left = rectAddButton.right - nAddButtonWidth;
		rectRemoveButton.right = rectAddButton.right;
		rectRemoveButton.left = rectAddButton.left;
		rectConfigureButton.right = rectAddButton.right;
		rectConfigureButton.left = rectAddButton.left;

		// Adjust position of FAM Condition description
		long height = rectConditionDescription.Height();
		rectConditionDescription.bottom = rectDlg.bottom - nLen1;
		rectConditionDescription.top = rectConditionDescription.bottom - height;
		rectConditionDescription.right = rectDlg.right - 2*nLen1 - nAddButtonWidth;

		// Adjust vertical position of FAM Condition label
		height = rectLabelCondition.Height();
		rectLabelCondition.bottom = rectConditionDescription.top - nLen1;
		rectLabelCondition.top = rectLabelCondition.bottom - height;

		// Adjust position of FAM Condition Select button
		height = rectSelectCondition.Height();
		rectSelectCondition.top = rectConditionDescription.top;
		rectSelectCondition.bottom = rectSelectCondition.top + height;
		rectSelectCondition.right = rectAddButton.right;
		rectSelectCondition.left = rectAddButton.left;

		// Resize grid
		rectGrid.right = rectDlg.right - 2 * nLen1 - nAddButtonWidth;
		rectGrid.bottom = rectLabelCondition.top - nLen1;

		// Move the buttons to their new positions
		m_btnAdd.MoveWindow( &rectAddButton );
		m_btnRemove.MoveWindow( &rectRemoveButton );
		m_btnConfigure.MoveWindow( &rectConfigureButton );

		// Move the FAM Condition controls to their new positions
		m_btnSelectCondition.MoveWindow(&rectSelectCondition);
		GetDlgItem(IDC_STATIC_CONDITION)->MoveWindow(&rectLabelCondition);
		m_editSelectCondition.MoveWindow(&rectConditionDescription);

		// Update grid position and internal sizing
		GetDlgItem(IDC_GRID)->MoveWindow(&rectGrid);
		m_wndGrid.DoResize();

		// Update position of Picture control that provides a border for the grid
		rectGrid.left -= 1;
		rectGrid.top -= 1;
		rectGrid.right += 1;
		rectGrid.bottom += 1;
		GetDlgItem(IDC_PICTURE)->MoveWindow(&rectGrid);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08924")	
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnDoubleClickCondition()
{
	try
	{
		// get the current FAM condition
		IObjectWithDescriptionPtr ipFAMCondition = getFSMgmtRole()->FAMCondition;
		ASSERT_RESOURCE_ALLOCATION("ELI16087", ipFAMCondition != NULL);

		// allow the user to select and/or configure ipFAMCondition
		VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectDoubleClick(ipFAMCondition,
			"Condition", get_bstr_t(FP_FAM_CONDITIONS_CATEGORYNAME), VARIANT_TRUE, 0, NULL);

		// check if the FAM condition has been modified
		if (vbDirty == VARIANT_TRUE)
		{
			// display the updated description
			m_zConditionDescription = static_cast<const char*> (ipFAMCondition->Description);
			UpdateData( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16086");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnBtnSelectCondition() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// get the FAMCondition object-with-description
		IObjectWithDescriptionPtr ipCondition = getFSMgmtRole()->FAMCondition;
		ASSERT_RESOURCE_ALLOCATION( "ELI13558", ipCondition != NULL );
		
		// get the dimensions of the FAM condition command button
		RECT rectConditionCommandButton;
		m_btnSelectCondition.GetWindowRect(&rectConditionCommandButton);

		// allow the user to modify the FAM condition based on menu selection
		VARIANT_BOOL vbResult = getMiscUtils()->HandlePlugInObjectCommandButtonClick(ipCondition,
			"Condition", get_bstr_t(FP_FAM_CONDITIONS_CATEGORYNAME), VARIANT_TRUE, 0, NULL,
			rectConditionCommandButton.right, rectConditionCommandButton.top);

		// If the user clicks the OK button
		if (vbResult == VARIANT_TRUE)
		{
			// Display the description
			m_zConditionDescription = static_cast<const char*> (ipCondition->Description);
			UpdateData( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13561")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnContextCut()
{
	try
	{
		//put the obj on the clipboard
		OnContextCopy();

		//remove the obj
		OnContextDelete();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15814");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnContextCopy()
{
	try
	{
		// Retrieve existing FAM condition
		IObjectWithDescriptionPtr	ipObject = getFSMgmtRole()->FAMCondition;
		ASSERT_RESOURCE_ALLOCATION("ELI15815", ipObject != NULL);

		// ClipboardManager will handle the Copy
		getClipboardManager()->CopyObjectToClipboard( ipObject );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15816");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnContextPaste()
{
	try
	{
		// Test ClipboardManager object to see if it is an FAM condition
		IUnknownPtr	ipObject(NULL);
		if (getClipboardManager()->ObjectIsTypeWithDescription(IID_IFAMCondition))
		{
			// Retrieve object from ClipboardManager
			ipObject = getClipboardManager()->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION("ELI15817", ipObject != NULL);
		}
		else
		{
			// Throw exception, object is not an FAM condition
			throw UCLIDException("ELI15818", "Clipboard object is not a FAM Condition.");
		}

		// Set the FAM condition
		IObjectWithDescriptionPtr ipSK = ipObject;
		if (ipSK != NULL)
		{
			getFSMgmtRole()->FAMCondition = ipSK;

			// Display the FAM condition description
			m_zConditionDescription = (char *) ipSK->GetDescription();
			UpdateData( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15823");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::OnContextDelete()
{
	try
	{
		// delete selected items via context menu
		// Retrieve existing FAM condition description
		CString	zDesc;
		IObjectWithDescriptionPtr ipSK = getFSMgmtRole()->FAMCondition;
		ASSERT_RESOURCE_ALLOCATION("ELI15819", ipSK != NULL );
		string strDescription(ipSK->Description);
		zDesc = strDescription.c_str();

		// Request confirmation
		CString	zPrompt;
		int		iResult;
		zPrompt.Format( "Are you sure that FAM Condition '%s' should be deleted?", 
			zDesc );
		iResult = MessageBox( (LPCTSTR)zPrompt, "Confirm Delete", 
			MB_YESNO | MB_ICONQUESTION );

		// Act on response
		if (iResult == IDYES)
		{
			// Clear the FAM condition object
			getFSMgmtRole()->FAMCondition = NULL;
				
			// Display the empty FAM condition description
			m_zConditionDescription = "";
			UpdateData( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15820");
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlgScopePage::OnModifyCell(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Extract Row and Column
		int nRow = LOWORD( lParam );
		int nCol = HIWORD( lParam );

		// Check ID - the message is only sent by the drop down column so no need to
		// check the column ID, just get the cell value
		if (wParam == IDC_GRID)
		{
			// Get the new priority value from the grid
			string strPriority = (LPCTSTR) m_wndGrid.GetCellValue(nRow, nCol);
			
			// Get the priority value
			UCLID_FILEPROCESSINGLib::EFilePriority ePriority = getPriorityFromString(strPriority);

			// Get the file supplier for this row
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFD =
				getFileSuppliersData()->At(nRow-1);

			// Set the priority
			ipFD->Priority = ePriority;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27601");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::displayContextMenu()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MNU_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		bool bEnable = !m_zConditionDescription.IsEmpty();

		// FAM condition is not defined
		pContextMenu->EnableMenuItem( ID_CONTEXT_CUT, bEnable ? nEnable : nDisable );
		pContextMenu->EnableMenuItem( ID_CONTEXT_COPY, bEnable ? nEnable : nDisable );
		pContextMenu->EnableMenuItem( ID_CONTEXT_DELETE, bEnable ? nEnable : nDisable );

		// Check Clipboard object type to enable/disable Paste menu item
		bEnable = asCppBool(
			getClipboardManager()->ObjectIsTypeWithDescription(IID_IFAMCondition) == VARIANT_TRUE);
		pContextMenu->EnableMenuItem( ID_CONTEXT_PASTE, bEnable ? nEnable : nDisable );

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos( &point );

		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15822")
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileSupplierPtr FileProcessingDlgScopePage::getFileSupplier(int iRow)
{
	// get the file suppliers data
	IIUnknownVectorPtr ipFileSuppliersData = getFSMgmtRole()->FileSuppliers;
	if (ipFileSuppliersData == NULL)
	{
		return NULL;
	}

	// Validate row number
	if (iRow < 1 || iRow > ipFileSuppliersData->Size())
	{
		return NULL;
	}

	// Retrieve desired FileSupplierData item 
	UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipData = ipFileSuppliersData->At( iRow - 1 );
	ASSERT_RESOURCE_ALLOCATION("ELI13731", ipData != NULL);

	// get the file supplier obj-with-desc
	IObjectWithDescriptionPtr ipFSObjWithDesc = ipData->FileSupplier;
	ASSERT_RESOURCE_ALLOCATION("ELI14263", ipFSObjWithDesc != NULL);

	// Return FileSupplier object from Data item
	return ipFSObjWithDesc->Object;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::EFileSupplierStatus FileProcessingDlgScopePage::getStatus(int iRow)
{
	// Validate row number
	int iCount = m_wndGrid.GetRowCount();
	if ((iRow <= 0) || (iRow > iCount))
	{
		UCLIDException ue( "ELI13685", "Invalid row number." );
		ue.addDebugInfo( "Row Number", iRow );
		ue.addDebugInfo( "Row Count", iCount );
		throw ue;
	}

	// Get desired File Supplier Data item
	UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSD = getFileSuppliersData()->At( iRow - 1 );
	ASSERT_RESOURCE_ALLOCATION("ELI13732", ipFSD != NULL);

	// Return status
	return ipFSD->FileSupplierStatus;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::setButtonStates()
{
	// The Grid, Add button and Select FAM condition button will be disabled while FAM
	//  is running and enabled when FAM is stopped manually or finished.
	BOOL bEnabled = asMFCBool(m_bEnabled);
	m_wndGrid.EnableWindow(bEnabled);
	m_btnAdd.EnableWindow(bEnabled);
	m_btnSelectCondition.EnableWindow(bEnabled);
	m_editSelectCondition.EnableWindow(bEnabled);

	// Check for selection of supplier item in the Grid
	int iSelIndex = m_wndGrid.GetFirstSelectedRow();
	if (iSelIndex > 0)
	{
		// Enable the remove and configure buttons if FAM is stopped or finished
		m_btnRemove.EnableWindow(bEnabled);
		m_btnConfigure.EnableWindow(bEnabled);
	}
	else
	{
		// Disable the remove and configure buttons if nothing is selected
		m_btnRemove.EnableWindow(FALSE);
		m_btnConfigure.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgScopePage::OnSetActive()
{
	return CPropertyPage::OnSetActive();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::setStatus(int iRow, 
										   UCLID_FILEPROCESSINGLib::EFileSupplierStatus eNewStatus)
{
	// Validate status change

	// Get status text
	string strNewStatus;
	switch (eNewStatus)
	{
		case UCLID_FILEPROCESSINGLib::kInactiveStatus:
			strNewStatus = gstrINACTIVE.c_str();
			break;

		case UCLID_FILEPROCESSINGLib::kActiveStatus:
			strNewStatus = gstrACTIVE.c_str();
			break;

		case UCLID_FILEPROCESSINGLib::kPausedStatus:
			strNewStatus = gstrPAUSED.c_str();
			break;

		case UCLID_FILEPROCESSINGLib::kStoppedStatus:
			strNewStatus = gstrSTOPPED.c_str();
			break;

		case UCLID_FILEPROCESSINGLib::kDoneStatus:
			strNewStatus = gstrDONE.c_str();
			break;
	}

	// Lock the row unless in INACTIVE, STOP OR DONE status
	if (eNewStatus == UCLID_FILEPROCESSINGLib::kInactiveStatus
		|| eNewStatus == UCLID_FILEPROCESSINGLib::kStoppedStatus
		|| eNewStatus == UCLID_FILEPROCESSINGLib::kDoneStatus)
	{
		m_wndGrid.SetRowLock( iRow, false );
	}
	else
	{
		m_wndGrid.SetRowLock( iRow, true );
	}

	// Set the new status value
	m_wndGrid.SetStyleRange(CGXRange( iRow, 5 ),
							CGXStyle()
								.SetValue( strNewStatus.c_str() )
					);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::updateList(int nRow, 
											UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSD)
{
	// Object must be defined
	ASSERT_RESOURCE_ALLOCATION("ELI13948", ipFSD != NULL);

	// Retrieve associated ObjectWithDescription
	IObjectWithDescriptionPtr ipObj = ipFSD->FileSupplier;
	ASSERT_RESOURCE_ALLOCATION("ELI13734", ipObj != NULL);

	// Retrieve Enabled state
	bool bEnabled = (ipObj->Enabled == VARIANT_TRUE);

	// Retrieve Force Processing state
	bool bForce = (ipFSD->ForceProcessing == VARIANT_TRUE);

	// Retrieve Description text
	_bstr_t bstrDescription = ipObj->GetDescription();
	string strDescription = asString(bstrDescription);

	// Retrieve status
	UCLID_FILEPROCESSINGLib::EFileSupplierStatus eStatus = ipFSD->FileSupplierStatus;

	// Create text vector - defaulting to Inactive
	vector<string> vecText;
	vecText.push_back( strDescription.c_str() );
	vecText.push_back( gstrINACTIVE.c_str() );

	// Update the row
	m_wndGrid.SetRowInfo(nRow, bEnabled, bForce,
		getPriorityString(ipFSD->Priority), vecText);

	// Set the proper status in the row
	if (eStatus != UCLID_FILEPROCESSINGLib::kInactiveStatus)
	{
		// Update the status and Lock the row
		setStatus( nRow, eStatus );
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::updateUI()
{
	// Get the pointer to the Property sheet object
	ResizablePropertySheet* pFPDPropSheet = (ResizablePropertySheet*)GetParent();
	// Get the pointer to the current FileProcessingDlg object
	FileProcessingDlg* pFPDlg = (FileProcessingDlg*)pFPDPropSheet->GetParent();

	pFPDlg->updateUI();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgScopePage::getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow)
{
	// retrieve the dialog item using its resource ID
	CWnd* cwndDlgItem = GetDlgItem(uiDlgItemResourceID);
	ASSERT_RESOURCE_ALLOCATION("ELI19439", cwndDlgItem != NULL);

	// set the window rect to the appropriate position and dimensions
	cwndDlgItem->GetWindowRect(&rectWindow);
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileSupplyingMgmtRolePtr FileProcessingDlgScopePage::getFSMgmtRole()
{
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFPM( m_pFPM );
	ASSERT_RESOURCE_ALLOCATION("ELI14264", ipFPM != NULL);

	// get the file supplying mgmt role
	UCLID_FILEPROCESSINGLib::IFileSupplyingMgmtRolePtr ipFSMgmtRole = ipFPM->FileSupplyingMgmtRole;
	ASSERT_RESOURCE_ALLOCATION("ELI14258", ipFSMgmtRole != NULL);

	return ipFSMgmtRole;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr FileProcessingDlgScopePage::getFileSuppliersData()
{
	// get the file supplying mgmt role
	IIUnknownVectorPtr ipFileSuppliersData = getFSMgmtRole()->FileSuppliers;
	ASSERT_RESOURCE_ALLOCATION("ELI19429", ipFileSuppliersData != NULL);

	return ipFileSuppliersData;
}
//-------------------------------------------------------------------------------------------------
IClipboardObjectManagerPtr FileProcessingDlgScopePage::getClipboardManager()
{
	// check if a clipboard manager has all ready been created
	if (!m_ipClipboardMgr)
	{
		// create MiscUtils object
		m_ipClipboardMgr.CreateInstance(CLSID_ClipboardObjectManager);
		ASSERT_RESOURCE_ALLOCATION("ELI16088", m_ipClipboardMgr != NULL);
	}

	return m_ipClipboardMgr;
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr FileProcessingDlgScopePage::getMiscUtils()
{
	// check if a MiscUtils object has all ready been created
	if (!m_ipMiscUtils)
	{
		// create MiscUtils object
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI19438", m_ipMiscUtils != NULL);
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------