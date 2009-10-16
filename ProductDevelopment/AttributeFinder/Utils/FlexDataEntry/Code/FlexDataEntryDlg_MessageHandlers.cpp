// CFlexDataEntryDlg_MessageHandlers.cpp : implementation file of CFlexDataEntryDlg message handlers

#include "stdafx.h"
#include "FlexDataEntry.h"
#include "FlexDataEntryDlg.h"
#include "..\\..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <ComUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// String constant for product version with Patch
const std::string	gstrFDE_VERSION = "FlexDataEntry";

// String constant for Automatic opening of VOA file
const std::string	gstrAUTO_OPEN_VOA = "AutomaticOpenVOAFile";

// String constant for Automatic Prompt to Save
const std::string	gstrAUTO_PROMPT_TO_SAVE = "AutoPromptToSave";

// String constant for Warn User If Find Before Save
const std::string	gstrWARN_IF_FIND_BEFORE_SAVE = "WarnIfFindBeforeSave";

//-------------------------------------------------------------------------------------------------
// CFlexDataEntryDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CFlexDataEntryDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		///////////////////////////////////////////////////////////
		// Add "About Attribute Finder..." menu item to system menu
		///////////////////////////////////////////////////////////

		// IDM_ABOUTBOX must be in the system command range.
		ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
		ASSERT(IDM_ABOUTBOX < 0xF000);

		CMenu* pSysMenu = GetSystemMenu(FALSE);
		if (pSysMenu != NULL)
		{
			CString strAboutMenu;
			strAboutMenu.LoadString( IDC_BTN_HELP );
			if (!strAboutMenu.IsEmpty())
			{
				pSysMenu->AppendMenu( MF_SEPARATOR );
				pSysMenu->AppendMenu( MF_STRING, IDM_ABOUTBOX, strAboutMenu );
			}
		}

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon

		// Get screen resolution
		RECT rectScreen;
		SystemParametersInfo( SPI_GETWORKAREA, 0, &rectScreen, 0 );
		int iWidth = rectScreen.right - rectScreen.left + 1;
		int iHeight = rectScreen.bottom - rectScreen.top + 1;

		// Reposition dialog to left half of screen with minimum border
		::SetWindowPos( m_hWnd, NULL, 0, 1, iWidth / 2, iHeight - 2, SWP_NOZORDER );
		GetClientRect( m_rect );

		// Parse output format from INI file
		// Do this before creating toolbar so that Save button can be shown or hidden
		readOutputFormatTemplate( getINIPath() );

		// Read INI file for AutoOpenVOA setting - not required
		string strText = getSetting( gstrAUTO_OPEN_VOA, false );
		long lValue = 0;
		if (!strText.empty())
		{
			lValue = asLong( strText );
		}
		m_bOpenVOAFile = (lValue == 1);

		// Create the dialog toolbar
//		createToolBar();

		// Load and update the menu
		loadMenu();
		enableButton( IDC_BTN_FIND, false );
		enableButton( IDC_BTN_SAVE, false );

		// Create the grids
		m_lTotalGridHeight = createGrids();

		////////////////////////
		// Set scrollbar details
		////////////////////////
		m_nScrollPos = 0;
		SCROLLINFO si;
		si.cbSize = sizeof( SCROLLINFO );
		si.fMask = SIF_ALL; // SIF_ALL = SIF_PAGE | SIF_RANGE | SIF_POS;
		// nMin and nMax are Y-positions within the dialog that define
		// top and bottom of the RECT to be scrolled.  This allows 
		// the toolbar to remain visible
		si.nMin = 0;
		si.nMax = m_lTotalGridHeight - m_lMinScrollPos;
		// PageUp or PageDown move by one entire block less a single step
		si.nPage = m_rect.Height() - m_lMinScrollPos / 2 - m_lScrollStep;
		// Scroll box starts at the top
		si.nPos = 0;
		SetScrollInfo( SB_VERT, &si, TRUE ); 

		// Create the Attribute Finder Engine
		m_ipEngine.CreateInstance( CLSID_AttributeFinderEngine );
		ASSERT_RESOURCE_ALLOCATION("ELI10799", m_ipEngine != NULL);

		// Create new SRIR
		IInputReceiverPtr ipSpotRecIR( CLSID_SpotRecognitionWindow );
		ASSERT_RESOURCE_ALLOCATION("ELI10811", ipSpotRecIR != NULL);

		// Move SRIR to right half of screen with minimum border
		HWND	hwndSRIR = (HWND)(long)ipSpotRecIR->GetWindowHandle();
		::SetWindowPos( hwndSRIR, NULL, iWidth / 2, 1, iWidth / 2, 
			iHeight - 1, SWP_NOZORDER );

		// Set self as the paragraph handler
		// so that the recognized text can go into the Value edit box
		m_ipParagraphTextHandlers.CreateInstance( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION("ELI10945", m_ipParagraphTextHandlers != NULL);

		IParagraphTextHandlerPtr ipPTH(this);
		ASSERT_RESOURCE_ALLOCATION("ELI11009", ipPTH != NULL);

		// Add this PTH to collection
		m_ipParagraphTextHandlers->PushBack( ipPTH );

		// Set self as the SRW event handler
		m_ipSRIR = ipSpotRecIR;
		ISRWEventHandlerPtr ipSRWEH(this);
		ASSERT_RESOURCE_ALLOCATION("ELI12999", ipSRWEH != NULL);
		m_ipSRIR->SetSRWEventHandler( ipSRWEH );

		// Turn off auto fitting
		m_ipSRIR->FittingMode = 0;

		// Hide certain SRIR toolbar buttons
		updateSRIRToolbar();

		// Update the grid buttons
		updateButtons();

		// Connect SRIR to Input Manager
		getInputManager()->ConnectInputReceiver( ipSpotRecIR );

		// Create tooltip control
		m_ToolTipCtrl.Create(this, TTS_ALWAYSTIP);

		m_bInitialized = true;

		// Open image file provided on command-line
		if (!m_strCommandLine.empty())
		{
			m_ipSRIR->OpenImageFile(get_bstr_t( m_strCommandLine ));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10626")

	// Exit gracefully if initialization failed
	if (!m_bInitialized)
	{
		PostQuitMessage( 0 );
	}

	// return TRUE  unless you set the focus to a control
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CFlexDataEntryDlg::OnPaint() 
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CPaintDC dc( this ); // device context for painting

		// Get the toolbar height and the dialog width
//		CRect rectDlg;
//		GetWindowRect( &rectDlg );
//		CRect rectToolBar;
//		if (m_apToolBar)
//		{
//			m_apToolBar->GetWindowRect( &rectToolBar );
//			int iToolBarHeight = rectToolBar.Height();
//			int iDialogWidth = rectDlg.Width();

			// With gray and white pens, draw horizontal lines that span the entire width
			// of the dialog, and that are just below the toolbar buttons
//			CPen penGray;
//			CPen penWhite;
//			penGray.CreatePen(  PS_SOLID, 0, RGB( 128, 128, 128 ) );
//			penWhite.CreatePen( PS_SOLID, 0, RGB( 255, 255, 255 ) );

			// First the gray line
//			dc.SelectObject( &penGray );
//			dc.MoveTo( 0, iToolBarHeight );
//			dc.LineTo( iDialogWidth, iToolBarHeight );

			// Next the white line, one pixel below the gray
//			dc.SelectObject( &penWhite );
//			dc.MoveTo( 0, iToolBarHeight + 1 );
//			dc.LineTo( iDialogWidth, iToolBarHeight + 1 );
//		}
	}
}
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CFlexDataEntryDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		// Display the About box with version information
		m_ipEngine->ShowHelpAboutBox( kFlexIndexHelpAbout, get_bstr_t( gstrFDE_VERSION ) );
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnClose() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Get number of grids to be examined and/or deleted
		unsigned long	ulSize = m_vecGrids.size();
		unsigned int	ui;

		// Check if results saved before Close
		if (!isSaveHidden())
		{
			// Check Modified and Empty states for each grid
			bool bIsModified = false;
			bool bGridsEmpty = true;
			for (ui = 0; ui < ulSize; ui++)
			{
				// Retrieve this grid
				CDataEntryGrid*	pGrid = m_vecGrids[ui];

				// Check Modified state for grid contents
				if (pGrid->IsModified())
				{
					// Can stop checking if a modification is unsaved
					bIsModified = true;
					break;
				}

				// Check Empty state
				if (!pGrid->IsEmpty())
				{
					bGridsEmpty = false;
				}
			}		// end check if each grid Modified or Empty

			// Check for existence of output file
			string strOutput = getOutputFile();
			bool bOutput = isFileOrFolderValid( strOutput );

			// Prompt user if Save is needed
			// - modifications have not been saved OR
			// - unmodified results have not been saved
			if (bIsModified || (!bOutput && !bGridsEmpty))
			{
				// Provide prompt to user
				int iResult = MessageBox( "Save results to PXT file before close?", 
					"Confirm Save", MB_ICONEXCLAMATION | MB_YESNOCANCEL );
				if (iResult == IDYES)
				{
					// Save the changes before continuing with Close
					if (validateSave())
					{
						// No forced close here since we are already closing
						doSave( false );
					}
				}
				else if (iResult == IDCANCEL)
				{
					// Do not Close, just return
					return;
				}
				// else do not Save and allow application to Close
			}		// end if Save is needed
		}			// end if Save button visible

		// Release created Grids
		for (ui = 0; ui < ulSize; ui++)
		{
			delete m_vecGrids[ui];
		}

		CDialog::OnClose();

		// this function is usually called by clicking the X on the
		// top-right corner of the dialog to close the dialog.
		// Since we leave OnCancel() implementation empty, this is
		// the place to call CDialog::OnCancel() to close the window.
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11092")
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar) 
{
	int nDelta;
	int nMaxPos = m_lTotalGridHeight - m_rect.Height();

	switch (nSBCode)
	{
	case SB_LINEDOWN:
		// No change if at or past the bottom
		if (m_nScrollPos >= nMaxPos)
		{
			return;
		}
		// Change by lesser of one step or the rest of the way to the bottom
		nDelta = min( m_lScrollStep, nMaxPos - m_nScrollPos );
		break;

	case SB_LINEUP:
		// No change if at or past the top
		if (m_nScrollPos <= 0)
		{
			return;
		}
		// Change by lesser of one step or the rest of the way to the top
		nDelta = -min( m_lScrollStep, m_nScrollPos );
		break;

	case SB_PAGEDOWN:
		// No change if at or past the bottom
		if (m_nScrollPos >= nMaxPos)
		{
			return;
		}
		// Change by lesser of almost one page or the rest of the way to the bottom
		nDelta = min( m_rect.Height() - m_lScrollStep, nMaxPos-m_nScrollPos );
		break;

	case SB_PAGEUP:
		// No change if at or past the top
		if (m_nScrollPos <= 0)
		{
			return;
		}
		// Change by lesser of almost one page or the rest of the way to the top
		nDelta = -min( m_rect.Height() - m_lScrollStep, m_nScrollPos );
		break;
	
	case SB_THUMBPOSITION:
		// Change to new position
		nDelta = (int)nPos - m_nScrollPos;
		break;

	default:
		return;
	}

	// Update scroll position
	m_nScrollPos += nDelta;
	SetScrollPos( SB_VERT, m_nScrollPos, TRUE );

	// Scroll portion of window to new position
	RECT	rectGrids;
	GetClientRect( &rectGrids );
	// Do not clip entire Client area, exclude the toolbar region
	rectGrids.top = m_lMinScrollPos / 2;
	UpdateWindow();
//	ScrollWindow( 0, -nDelta, NULL, &rectGrids );	// toolbar scroll problem
	ScrollWindow( 0, -nDelta );		// toolbar scrolls out of sight
	CDialog::OnVScroll( nSBCode, nPos, pScrollBar );
}
//-------------------------------------------------------------------------------------------------
LRESULT CFlexDataEntryDlg::OnLClickGrid(WPARAM wParam, LPARAM lParam) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Loop through collection of Grids
		CDataEntryGrid* pActiveGrid = NULL;
		long lCount = m_vecGrids.size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this Grid
			CDataEntryGrid* pGrid = m_vecGrids[i];

			// Compare control ID
			if (pGrid->GetControlID() == wParam)
			{
				pActiveGrid = pGrid;
				setActiveGrid( pActiveGrid );
				break;
			}
		}

		// Highlight appropriate text in SRIR
		if (pActiveGrid != NULL)
		{
			// Get current cell
			ROWCOL	nRow = 0;
			ROWCOL	nCol = 0;
			BOOL bResult = pActiveGrid->GetCurrentCell( nRow, nCol );
			if (bResult == TRUE)
			{
				// Remove any highlight if user clicked a column header
				if (nRow == 0)
				{
					m_ipSRIR->DeleteTemporaryHighlight();
				}
				else
				{
					// Get the Attribute on this row
					IAttributePtr ipSel = pActiveGrid->GetAttributeFromRow( nRow );
					if (ipSel)
					{
						highlightAttribute( ipSel );
					}

					// Select entire text in this cell
					pActiveGrid->SetCurrentCell( nRow, nCol );
				}
			}
			// Else no cell is active in this grid, use cell specified in lParam or first cell
			else
			{
				// Retrieve row and column from LPARAM
				nRow = LOWORD( lParam );
				nCol = HIWORD( lParam );

				// Validate row and column
				if (nRow == 0)
				{
					nRow = 1;
				}
				if (nCol == 0)
				{
					nCol = 1;
				}

				pActiveGrid->SetCurrentCell( nRow, nCol );
			}
		}

		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11093")

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnAddButton(UINT nID)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Determine which grid
		int iIndex = nID % IDC_GROUP_MAX;
		CDataEntryGrid* pGrid = m_vecGrids[iIndex];

		// Select this grid and add a new record
		setActiveGrid( pGrid );
		int iNewRow = pGrid->AddNewRecord();

		// Select the new row and scroll the new row into view
		pGrid->SelectRow( iNewRow );
		pGrid->ScrollCellInView( iNewRow, 1 );

		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11097")
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnDeleteButton(UINT nID)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Determine which grid
		int iIndex = nID % IDC_GROUP_MAX;

		// Delete selected record
		CDataEntryGrid* pGrid = m_vecGrids[iIndex];
		pGrid->DeleteSelectedRecord();

		// Update the highlight
		if (m_ipSRIR)
		{
			// Remove the old highlight
			m_ipSRIR->DeleteTemporaryHighlight();

			// Add the new highlight, if any
			IAttributePtr ipNew = pGrid->GetSelectedAttribute();
			if (ipNew)
			{
				highlightAttribute( ipNew );
			}
		}

		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11098")
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnPreviousButton(UINT nID)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Determine which grid
		int iIndex = nID % IDC_GROUP_MAX;

		// Get grid pointer
		CDataEntryGrid* pGrid = m_vecGrids[iIndex];
		ASSERT_RESOURCE_ALLOCATION( "ELI13098", pGrid != NULL );
		setActiveGrid( pGrid );

		// Update number of current Attribute
		long lItem = pGrid->GetActiveRecord();
		if (lItem > 0)
		{
			lItem--;
		}

		// Set the current Attribute and update control status
		pGrid->SetActiveRecord( lItem );
		updateNavigationControls( pGrid );

		// Select the new item (1-relative index)
		pGrid->SelectRow( 1 );

		// Select the Attribute
		IAttributePtr ipSel = pGrid->GetSelectedAttribute();
		if (ipSel)
		{
			highlightAttribute( ipSel );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13099")
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnNextButton(UINT nID)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Determine which grid
		int iIndex = nID % IDC_GROUP_MAX;

		// Get grid pointer
		CDataEntryGrid* pGrid = m_vecGrids[iIndex];
		ASSERT_RESOURCE_ALLOCATION( "ELI13102", pGrid != NULL );
		setActiveGrid( pGrid );

		// Update number of current Attribute
		long lItem = pGrid->GetActiveRecord();
		if (lItem < pGrid->GetRecordCount() - 1)
		{
			lItem++;
		}

		// Set the current Attribute and update control status
		pGrid->SetActiveRecord( lItem );
		updateNavigationControls( pGrid );

		// Select the new item (1-relative index)
		pGrid->SelectRow( 1 );

		// Select the Attribute
		IAttributePtr ipSel = pGrid->GetSelectedAttribute();
		if (ipSel)
		{
			highlightAttribute( ipSel );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13100")
}
//-------------------------------------------------------------------------------------------------
BOOL CFlexDataEntryDlg::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	if (m_bInitialized)
	{
		// Handle shortcut keys
		if (pMsg->message == WM_KEYDOWN)
		{
			if (handleShortcutKey( pMsg->wParam ))
			{
				// Eat any already processed characters
				return TRUE;
			}

			// Also check for Delete key to operate on selected row (P16 #1891)
			if (pMsg->wParam == VK_DELETE)
			{
				CDataEntryGrid* pGrid = getActiveGrid();
				IAttributePtr ipSel = pGrid->GetSelectedAttribute();
				if (ipSel != NULL)
				{
					// A record is selected, so delete it
					pGrid->DeleteSelectedRecord();
					pGrid->SetCurrentCell( GX_INVALID, GX_INVALID );

					// Update the highlight
					if (m_ipSRIR)
					{
						// Remove the old highlight
						m_ipSRIR->DeleteTemporaryHighlight();

						// Add the new highlight, if any
						IAttributePtr ipNew = pGrid->GetSelectedAttribute();
						if (ipNew)
						{
							highlightAttribute( ipNew );
						}
					}

					// Update button state and eat the character
					updateButtons();
					return TRUE;
				}
			}
			// Also check for Tab key to continue navigation
			else if (pMsg->wParam == VK_TAB)
			{
				CDataEntryGrid* pGrid = getActiveGrid();
				int nID = pGrid->GetControlID();

				// Check for Shift key
				if (isVirtKeyCurrentlyPressed( VK_SHIFT ))
				{
					//    lParam == 1 to indicate that this is a Tab and should 
					//    navigate out of this grid regardless of IsArrowNavigationDisabled()
					OnCellLeft( nID, 1 );
				}
				else
				{
					//    lParam == 1 to indicate that this is a Tab and should 
					//    navigate out of this grid regardless of IsArrowNavigationDisabled()
					OnCellRight( nID, 1 );
				}
				return TRUE;
			}
		}

		// make sure the tool tip control is a valid window before passing messages to it
		if (asCppBool(::IsWindow(m_ToolTipCtrl.m_hWnd)))
		{
			// Show tooltips
			m_ToolTipCtrl.RelayEvent( pMsg );
		}
	}

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
BOOL CFlexDataEntryDlg::OnToolTipNotify(UINT id, NMHDR * pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	BOOL retCode = FALSE;
	
    TOOLTIPTEXT* pTTT = (TOOLTIPTEXT*)pNMHDR;
    UINT nID = pNMHDR->idFrom;
    if (pNMHDR->code == TTN_NEEDTEXT && (pTTT->uFlags & TTF_IDISHWND))
    {
        // idFrom is actually the HWND of the tool, ex. button control, edit control, etc.
        nID = ::GetDlgCtrlID((HWND)nID);
	}

	if (nID)
	{
		retCode = TRUE;
		pTTT->hinst = AfxGetResourceHandle();
		pTTT->lpszText = MAKEINTRESOURCE(nID);
	}

	return retCode;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnCancel()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Escape key
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnOK()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Enter key
}
//-------------------------------------------------------------------------------------------------
BOOL CFlexDataEntryDlg::DestroyWindow() 
{
	try
	{
		// Disconnect self as a listener of the input manager events
		SetInputManager(NULL);

		// Call base class functionality
		return CDialog::DestroyWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11094")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnBtnClear() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Clear highlights
		if (m_ipSRIR)
		{
			m_ipSRIR->DeleteTemporaryHighlight();
		}

		// Clear each grid and clear associated Attributes
		long lCount = m_vecGrids.size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this grid
			CDataEntryGrid*	pGrid = m_vecGrids[i];
			pGrid->Clear();
                      
            // Clear the IAttribute objects associated with this Grid
            pGrid->ClearAllAttributes();

			// Disable navigation controls
			updateNavigationControls( pGrid );
		}

		// Disable Save button if present
		if (!isSaveHidden())
		{
//			m_apToolBar->GetToolBarCtrl().EnableButton( IDC_BTN_SAVE, FALSE );
			enableButton( IDC_BTN_SAVE, false );
		}

		// Enable & disable buttons
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11090")
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnBtnFind() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Just return if Find button is hidden
		if (isFindHidden())
		{
			return;
		}

		try
		{
			// Check for open file with SRIR
			string strFile = m_ipSRIR->GetImageFileName();

			// Get the current directory and add a dummy file name to 
			// create a fully specified file name in the current directory
			string strFileInCurDir = getCurrentDirectory();
			strFileInCurDir = strFileInCurDir + "\\file_name_not_used.txt";

			// Get the absolute file name of the image file
			strFile = getAbsoluteFileName(strFileInCurDir, strFile, true);

			if (strFile.length() > 0)
			{
				// Maybe check for unsaved changes - if Save button visible
				if (!isSaveHidden())
				{
					// Check for warning flag - not required
					string strWarn = getSetting( gstrWARN_IF_FIND_BEFORE_SAVE, false );
					if (!strWarn.empty() && (asLong( strWarn ) == 1))
					{
						// Check for pending changes in grids
						bool bUnsavedEdits = false;
						long lCount = m_vecGrids.size();
						for (int i = 0; i < lCount; i++)
						{
							// Retrieve this grid
							CDataEntryGrid*	pGrid = m_vecGrids[i];
				                      
							// Check for unsaved edits
							if (pGrid->IsModified())
							{
								bUnsavedEdits = true;
								break;
							}
						}

						// Prompt user about Save
						if (bUnsavedEdits)
						{
							int iResult = MessageBox( "Finding attributes will overwrite unsaved changes in the Grids.\r\n\r\nContinue with Find?", 
								"Confirm Find", MB_ICONEXCLAMATION | MB_YESNO );
							if (iResult == IDNO)
							{
								// Just return without Finding attributes
								return;
							}
							// else go ahead with Find
						}	// end if unsaved edits were found
					}		// end if warning flag enabled in INI file
				}			// end if Save button visible

				// Disable the Find button and create a wait cursor [FlexIDSCore #3019]
				CWaitCursor wait;
				enableButton( IDC_BTN_FIND, false );

				// Create placeholder AFDocument
				IAFDocumentPtr	ipDoc( CLSID_AFDocument );
				ASSERT_RESOURCE_ALLOCATION( "ELI10813", ipDoc != NULL );

				// Create vector object for found Attributes
				IIUnknownVectorPtr	ipAttributes( CLSID_IUnknownVector );
				ASSERT_RESOURCE_ALLOCATION( "ELI10814", ipAttributes != NULL );

				// Retrieve name of RSD file
				string strRSD = getRSDFile();

				// Perform any appropriate auto-encrypt actions on the RSD file
				getMiscUtils()->AutoEncryptFile( get_bstr_t( strRSD.c_str() ), 
					get_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()) );

				// Find the RSD file
				if (isFileOrFolderValid( strRSD ))
				{
					// Construct name of USS file
					string strUSS = strFile + ".uss";

					// Search for associated USS file
					if (isFileOrFolderValid( strUSS ))
					{
						// Find Attributes from the USS file
						ipAttributes = m_ipEngine->FindAttributes( ipDoc, strUSS.c_str(), -1, 
							strRSD.c_str(), NULL, VARIANT_FALSE, NULL );
					}
					else
					{
						// Find Attributes from the image file
						ipAttributes = m_ipEngine->FindAttributes( ipDoc, strFile.c_str(), -1, 
							strRSD.c_str(), NULL, VARIANT_FALSE, NULL );
					}

					// Pass Attributes to grids
					populateGrids( ipAttributes );

					// Enable the Find button and enable the Save button, if present
					if (!isSaveHidden())
					{
						enableButton( IDC_BTN_SAVE, true );
					}
					enableButton( IDC_BTN_FIND, true );

					// Select first cell
					activateFirstCell();
				}
				else
				{
					UCLIDException ue("ELI13113", "RSD file not found." );
					ue.addDebugInfo("RSD File", strRSD );
					throw ue;
				}
			}
			else
			{
				UCLIDException ue("ELI13114", 
					"A file must be open in the Image Viewer before finding Attributes." );
				throw ue;
			}
		}
		catch(...)
		{
			// Enable the Find button if Find attribute throw an exception
//			m_apToolBar->GetToolBarCtrl().EnableButton( IDC_BTN_FIND, TRUE );
			enableButton( IDC_BTN_FIND, true );
			throw;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11091")
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnBtnSave() 
{
	// Just return if Save button is hidden
	if (isSaveHidden())
	{
		return;
	}

	if (validateSave())
	{
		// Write data to output file
		doSave();
	}
	// else warnings and prompts have already been seen
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnFileExit() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		OnClose();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13753")
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::OnBtnHelpAbout()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Display the About box with version information
		m_ipEngine->ShowHelpAboutBox( kFlexIndexHelpAbout, get_bstr_t( gstrFDE_VERSION ) );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11650")
}
//-------------------------------------------------------------------------------------------------
LRESULT CFlexDataEntryDlg::OnCellLeft(WPARAM wParam, LPARAM lParam) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Loop through collection of Grids
		long lCount = m_vecGrids.size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this Grid
			CDataEntryGrid* pGrid = m_vecGrids[i];
			ASSERT_RESOURCE_ALLOCATION( "ELI13142", pGrid != NULL );

			// Compare control ID
			if (pGrid->GetControlID() == wParam)
			{
				// Do nothing if Arrow navigation is disabled (P16 #1947)
				// Message is from a Arrow key iff lParam == 0
				if (pGrid->IsArrowNavigationDisabled() && (lParam == 0))
				{
					return 0;
				}

				// Get current cell - before move
				ROWCOL	nRow = 0;
				ROWCOL	nCol = 0;
				BOOL bResult2 = pGrid->GetCurrentCell( nRow, nCol );
				if (bResult2 == FALSE)
				{
					// Check for selected row
					int nCount = pGrid->GetRowCount();
					int nActive = pGrid->GetActiveRecord();
					if ((nActive >= 0) && (nActive < nCount))
					{
						// Treat row header as the current cell
						nRow = nActive + 1;
						nCol = 0;
					}
					else
					{
						// No current cell, just return
						return 0;
					}
				}

				// Clear whole-row selection if present
				pGrid->SetSelection( 0 );

				// Look for previous cell in this row
				if (nCol > 1)
				{
					// Move to the left - current Attribute will stay highlighted
					BOOL bResult = pGrid->SetCurrentCell( nRow, nCol - 1 );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13122", "Failed to move left in same row.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "Current Row", nRow );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}
					return 0;
				}

				// Look for previous row in this grid
				if (nRow > 1)
				{
					// Make sure that grid is wholly visible
					RECT rectGrid;
					pGrid->GetWindowRect( &rectGrid );
					while (2 * m_lMinScrollPos + m_lScrollStep > rectGrid.top)
					{
						OnVScroll( SB_LINEUP, 0, NULL );
						pGrid->GetWindowRect( &rectGrid );
					}

					// Move up one row
					BOOL bResult = pGrid->SetCurrentCell( nRow - 1, nCol );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13123", "Failed to move up one row.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "Current Row", nRow );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}

					// Move to the rightmost cell
					bResult = pGrid->SetCurrentCell( nRow - 1, pGrid->GetColCount() );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13128", "Failed to move to rightmost cell.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "New Row", nRow - 1 );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}

					// Attribute in new row will become highlighted
					IAttributePtr	ipAttr = pGrid->GetAttributeFromRow( nRow - 1 );
					ASSERT_RESOURCE_ALLOCATION( "ELI13124", ipAttr != NULL );
					highlightAttribute( ipAttr );
					return 0;
				}

				/////////////////////////
				// Look for previous grid
				/////////////////////////
				int j = i - 1;
				while (j >= 0)
				{
					// Get the "previous" grid
					CDataEntryGrid* pPreviousGrid = m_vecGrids[j];
					ASSERT_RESOURCE_ALLOCATION( "ELI13130", pPreviousGrid != NULL );

					// Make sure that row and column exist
					int nNumRows = pPreviousGrid->GetRowCount();
					int nNumCols = pPreviousGrid->GetColCount();
					if ((nNumRows > 0) & (nNumCols > 0))
					{
						// Make sure that grid is wholly visible
						RECT rectGrid;
						pPreviousGrid->GetWindowRect( &rectGrid );
						while (2 * m_lMinScrollPos + m_lScrollStep > rectGrid.top)
						{
							OnVScroll( SB_LINEUP, 0, NULL );
							pPreviousGrid->GetWindowRect( &rectGrid );
						}

						// Fake a left mouse click into last cell
						pPreviousGrid->SetFocus();
						CPoint pt;
						pPreviousGrid->OnStartSelection( nNumRows, nNumCols, MK_LBUTTON, pt );

						// Set last cell in the new grid
						setActiveGrid( pPreviousGrid );
						if (afxCurrentInstanceHandle != NULL)
						{
							pPreviousGrid->SetCurrentCell( nNumRows, nNumCols );
						}

						// Highlight entire attribute in this row
						IAttributePtr ipAttr = pPreviousGrid->GetAttributeFromRow( nNumRows );
						highlightAttribute( ipAttr );
						break;
					}
					else
					{
						// Decrement index to check the previous-previous grid
						j--;
					}
				}	// end while looking for previous grid
				break;
			}		// end if found active grid
		}			// end for each grid
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13105")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFlexDataEntryDlg::OnCellRight(WPARAM wParam, LPARAM lParam) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Loop through collection of Grids]
		bool bFoundCell = false;
		long lCount = m_vecGrids.size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this Grid
			CDataEntryGrid* pGrid = m_vecGrids[i];
			ASSERT_RESOURCE_ALLOCATION( "ELI13143", pGrid != NULL );

			// Compare control ID
			if (pGrid->GetControlID() == wParam)
			{
				// Do nothing if Arrow navigation is disabled (P16 #1947)
				// Message is from an Arrow key iff lParam == 0
				if (pGrid->IsArrowNavigationDisabled() && (lParam == 0))
				{
					return 0;
				}

				// Get current cell - before move
				ROWCOL	nRow = 0;
				ROWCOL	nCol = 0;
				BOOL bResult2 = pGrid->GetCurrentCell( nRow, nCol );
				if (bResult2 == FALSE)
				{
					// Check for selected row
					int nCount = pGrid->GetRowCount();
					int nActive = pGrid->GetActiveRecord();
					if ((nActive >= 0) && (nActive < nCount))
					{
						// Treat row header as the current cell
						nRow = nActive + 1;
						nCol = 0;
					}
					else
					{
						// No current cell, just return
						return 0;
					}
				}

				// Clear whole-row selection if present
				pGrid->SetSelection( 0 );

				// Look for next cell in this row
				if (nCol < pGrid->GetColCount())
				{
					// Move to the right - current Attribute will stay highlighted
					bFoundCell = true;
					BOOL bResult = pGrid->SetCurrentCell( nRow, nCol + 1 );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13125", "Failed to move right in same row.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "Current Row", nRow );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}
					return 0;
				}

				// Look for next row in this grid
				if (nRow < pGrid->GetRowCount())
				{
					// Make sure that grid is wholly visible
					RECT rectGrid;
					pGrid->GetWindowRect( &rectGrid );
					while (m_rect.bottom + m_lMinScrollPos / 2 < rectGrid.bottom)
					{
						OnVScroll( SB_LINEDOWN, 0, NULL );
						pGrid->GetWindowRect( &rectGrid );
					}

					// Move down one row
					BOOL bResult = pGrid->SetCurrentCell( nRow + 1, nCol );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13126", "Failed to move down one row.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "Current Row", nRow );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}

					// Move to the leftmost cell
					bFoundCell = true;
					bResult = pGrid->SetCurrentCell( nRow + 1, 1 );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13127", "Failed to move to leftmost cell.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "New Row", nRow - 1 );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}

					// Attribute in new row will become highlighted
					IAttributePtr	ipAttr = pGrid->GetAttributeFromRow( nRow + 1 );
					ASSERT_RESOURCE_ALLOCATION( "ELI13129", ipAttr != NULL );
					highlightAttribute( ipAttr );
					return 0;
				}

				/////////////////////
				// Look for next grid
				/////////////////////
				int j = i + 1;
				while (j < lCount)
				{
					// Get the "next" grid
					CDataEntryGrid* pNextGrid = m_vecGrids[j];
					ASSERT_RESOURCE_ALLOCATION( "ELI13131", pNextGrid != NULL );

					// Make sure that row and column exist
					int nNumRows = pNextGrid->GetRowCount();
					int nNumCols = pNextGrid->GetColCount();
					if ((nNumRows > 0) & (nNumCols > 0))
					{
						// Make sure that grid is wholly visible
						RECT rectGrid;
						pNextGrid->GetWindowRect( &rectGrid );
						while (m_rect.bottom + m_lMinScrollPos / 2 < rectGrid.bottom)
						{
							OnVScroll( SB_LINEDOWN, 0, NULL );
							pNextGrid->GetWindowRect( &rectGrid );
						}

						// Fake a left mouse click into first cell
						pNextGrid->SetFocus();
						CPoint pt;
						pNextGrid->OnStartSelection( 1, 1, MK_LBUTTON, pt );

						// Set first cell in the new grid
						setActiveGrid( pNextGrid );
						bFoundCell = true;
						if (afxCurrentInstanceHandle != NULL)
						{
							pNextGrid->SetCurrentCell( 1, 1 );
						}

						// Highlight entire attribute in this row
						IAttributePtr ipAttr = pNextGrid->GetAttributeFromRow( 1 );
						highlightAttribute( ipAttr );
						break;
					}
					else
					{
						// Increment index to check the next-next grid
						j++;
					}
				}
				break;
			}
		}

		// Check for auto-prompt to save
		string strSave = getSetting( gstrAUTO_PROMPT_TO_SAVE, false );
		if (!strSave.empty() && (asLong( strSave ) == 1) && !isSaveHidden() && !bFoundCell)
		{
			// Display prompt
			string strOutputFile = getOutputFile();
			string strMessage = string("Navigating past the last cell, save \"") + strOutputFile.c_str() + string("\"?");
			int iResult = MessageBox( strMessage.c_str(), "Save", MB_ICONQUESTION | MB_YESNOCANCEL );
			if (iResult == IDYES)
			{
				doSave( true );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13106")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFlexDataEntryDlg::OnCellUp(WPARAM wParam, LPARAM lParam) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Loop through collection of Grids
		long lCount = m_vecGrids.size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this Grid
			CDataEntryGrid* pGrid = m_vecGrids[i];
			ASSERT_RESOURCE_ALLOCATION( "ELI13144", pGrid != NULL );

			// Compare control ID
			if (pGrid->GetControlID() == wParam)
			{
				// Do nothing if Arrow navigation is disabled (P16 #1947)
				if (pGrid->IsArrowNavigationDisabled())
				{
					return 0;
				}

				// Get current cell - before move
				ROWCOL	nRow = 0;
				ROWCOL	nCol = 0;
				BOOL bResult2 = pGrid->GetCurrentCell( nRow, nCol );
				if (bResult2 == FALSE)
				{
					// No current cell, just return
					return 0;
				}

				// Clear whole-row selection if present
				pGrid->SetSelection( 0 );

				// Look for previous row in this grid
				if (nRow > 1)
				{
					// Make sure that grid is wholly visible
					RECT rectGrid;
					pGrid->GetWindowRect( &rectGrid );
					while (2 * m_lMinScrollPos + m_lScrollStep > rectGrid.top)
					{
						OnVScroll( SB_LINEUP, 0, NULL );
						pGrid->GetWindowRect( &rectGrid );
					}

					// Move up one row
					BOOL bResult = pGrid->SetCurrentCell( nRow - 1, nCol );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13133", "Failed to move up one row.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "Current Row", nRow );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}

					// Attribute in new row will become highlighted
					IAttributePtr	ipAttr = pGrid->GetAttributeFromRow( nRow - 1 );
					ASSERT_RESOURCE_ALLOCATION( "ELI13134", ipAttr != NULL );
					highlightAttribute( ipAttr );
					return 0;
				}

				// Look for previous cell in this (topmost) row
				if (nCol > 1)
				{
					// Move to the left - current Attribute will stay highlighted
					BOOL bResult = pGrid->SetCurrentCell( nRow, nCol - 1 );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13135", "Failed to move left in same row.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "Current Row", nRow );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}
					return 0;
				}

				/////////////////////////
				// Look for previous grid
				/////////////////////////
				int j = i - 1;
				while (j >= 0)
				{
					// Get the "previous" grid
					CDataEntryGrid* pPreviousGrid = m_vecGrids[j];
					ASSERT_RESOURCE_ALLOCATION( "ELI13136", pPreviousGrid != NULL );

					// Make sure that row and column exist
					int nNumRows = pPreviousGrid->GetRowCount();
					int nNumCols = pPreviousGrid->GetColCount();
					if ((nNumRows > 0) & (nNumCols > 0))
					{
						// Make sure that grid is wholly visible
						RECT rectGrid;
						pPreviousGrid->GetWindowRect( &rectGrid );
						while (2 * m_lMinScrollPos + m_lScrollStep > rectGrid.top)
						{
							OnVScroll( SB_LINEUP, 0, NULL );
							pPreviousGrid->GetWindowRect( &rectGrid );
						}

						// Fake a left mouse click into last cell
						CPoint pt;
						pPreviousGrid->OnStartSelection( nNumRows, nNumCols, MK_LBUTTON, pt );
						pPreviousGrid->SetFocus();

						// Set last cell in the new grid
						setActiveGrid( pPreviousGrid );
						pPreviousGrid->SetCurrentCell( nNumRows, nNumCols );

						// Highlight entire attribute in this row
						IAttributePtr ipAttr = pPreviousGrid->GetAttributeFromRow( nNumRows );
						highlightAttribute( ipAttr );
						break;
					}
					else
					{
						// Decrement index to check the previous-previous grid
						j--;
					}
				}	// end while looking for previous grid
				break;
			}		// end if found active grid
		}			// end for each grid
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13132")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFlexDataEntryDlg::OnCellDown(WPARAM wParam, LPARAM lParam) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Loop through collection of Grids
		bool bFoundCell = false;
		long lCount = m_vecGrids.size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this Grid
			CDataEntryGrid* pGrid = m_vecGrids[i];
			ASSERT_RESOURCE_ALLOCATION( "ELI13145", pGrid != NULL );

			// Compare control ID
			if (pGrid->GetControlID() == wParam)
			{
				// Do nothing if Arrow navigation is disabled (P16 #1947)
				if (pGrid->IsArrowNavigationDisabled())
				{
					return 0;
				}

				// Get current cell - before move
				ROWCOL	nRow = 0;
				ROWCOL	nCol = 0;
				BOOL bResult2 = pGrid->GetCurrentCell( nRow, nCol );
				if (bResult2 == FALSE)
				{
					// No current cell, just return
					return 0;
				}

				// Clear whole-row selection if present
				pGrid->SetSelection( 0 );

				// Look for next row in this grid
				if (nRow < pGrid->GetRowCount())
				{
					// Make sure that grid is wholly visible
					RECT rectGrid;
					pGrid->GetWindowRect( &rectGrid );
					while (m_rect.bottom + m_lMinScrollPos / 2 < rectGrid.bottom)
					{
						OnVScroll( SB_LINEDOWN, 0, NULL );
						pGrid->GetWindowRect( &rectGrid );
					}

					// Move down one row
					bFoundCell = true;
					BOOL bResult = pGrid->SetCurrentCell( nRow + 1, nCol );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13138", "Failed to move down one row.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "Current Row", nRow );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}

					// Attribute in new row will become highlighted
					IAttributePtr	ipAttr = pGrid->GetAttributeFromRow( nRow + 1 );
					ASSERT_RESOURCE_ALLOCATION( "ELI13139", ipAttr != NULL );
					highlightAttribute( ipAttr );
					return 0;
				}

				// Look for next cell in this (last) row
				if (nCol < pGrid->GetColCount())
				{
					// Move to the right - current Attribute will stay highlighted
					bFoundCell = true;
					BOOL bResult = pGrid->SetCurrentCell( nRow, nCol + 1 );
					if (bResult == FALSE)
					{
						UCLIDException ue("ELI13140", "Failed to move right in same row.");
						ue.addDebugInfo( "Grid Number", pGrid->GetID() );
						ue.addDebugInfo( "Current Row", nRow );
						ue.addDebugInfo( "Current Column", nCol );
						throw ue;
					}
					return 0;
				}

				/////////////////////
				// Look for next grid
				/////////////////////
				int j = i + 1;
				while (j < lCount)
				{
					// Get the "next" grid
					CDataEntryGrid* pNextGrid = m_vecGrids[j];
					ASSERT_RESOURCE_ALLOCATION( "ELI13141", pNextGrid != NULL );

					// Make sure that row and column exist
					int nNumRows = pNextGrid->GetRowCount();
					int nNumCols = pNextGrid->GetColCount();
					if ((nNumRows > 0) & (nNumCols > 0))
					{
						// Make sure that grid is wholly visible
						RECT rectGrid;
						pNextGrid->GetWindowRect( &rectGrid );
						while (m_rect.bottom + m_lMinScrollPos / 2 < rectGrid.bottom)
						{
							OnVScroll( SB_LINEDOWN, 0, NULL );
							pNextGrid->GetWindowRect( &rectGrid );
						}

						// Fake a left mouse click into first cell
						CPoint pt;
						pNextGrid->OnStartSelection( 1, 1, MK_LBUTTON, pt );
						pNextGrid->SetFocus();

						// Set first cell in the new grid
						bFoundCell = true;
						setActiveGrid( pNextGrid );
						pNextGrid->SetCurrentCell( 1, 1 );

						// Highlight entire attribute in this row
						IAttributePtr ipAttr = pNextGrid->GetAttributeFromRow( 1 );
						highlightAttribute( ipAttr );
						break;
					}
					else
					{
						// Increment index to check the next-next grid
						j++;
					}
				}
				break;
			}
		}

		// Check for auto-prompt to save
		string strSave = getSetting( gstrAUTO_PROMPT_TO_SAVE, false );
		if (!strSave.empty() && (asLong( strSave ) == 1) && !isSaveHidden() && !bFoundCell)
		{
			// Display prompt
			string strOutputFile = getOutputFile();
			string strMessage = string("Navigating past the last cell, save \"") + strOutputFile.c_str() + string("\"?");
			int iResult = MessageBox( strMessage.c_str(), "Save", MB_ICONQUESTION | MB_YESNOCANCEL );
			if (iResult == IDYES)
			{
				doSave( true );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13137")

	return 0;
}