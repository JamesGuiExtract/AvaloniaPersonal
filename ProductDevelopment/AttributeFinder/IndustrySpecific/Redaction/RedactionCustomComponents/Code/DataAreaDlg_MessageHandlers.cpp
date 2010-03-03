// DataAreaDlg_MessageHandlers.cpp : implementation of the DataAreaDlg message handlers

#include "stdafx.h"
#include "DataAreaDlg.h"
#include "ExemptionCodesDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <Win32Util.h>
#include <ZoneEntity.h>

#ifdef _VERIFICATION_LOGGING
#include <ThreadSafeLogFile.h>
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Dialog size bounds
const int	giDATAAREADLG_MIN_WIDTH			= 300;
const int	giDATAAREADLG_MIN_HEIGHT		= 500;

// Number of pages per row in Summary grid
const int	giPAGES_PER_ROW					= 8;

// Timer tick
const int giNUM_SECONDS_TO_REFRESH = 2000;

// Caption and Prompt to user to save changes to output file and history
const std::string gstrSAVE_CHANGES_CAPTION = "Save Changes";
const std::string gstrSAVE_CHANGES_PROMPT = 
	"Changes have been made to the document settings.\r\nApply the changes to the output file?";

// constants to refer to the columns in the data grid
const int giDATAGRID_TYPE_COLUMN = 3;
const int giDATAGRID_EXEMPTION_COLUMN = 6;

//-------------------------------------------------------------------------------------------------
// CDataAreaDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CDataAreaDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	try
	{
		// Create tooltip control
		m_ToolTipCtrl.Create( this, TTS_ALWAYSTIP );

		// Get screen resolution
		RECT rectScreen;
		SystemParametersInfo( SPI_GETWORKAREA, 0, &rectScreen, 0 );
		int iWidth = rectScreen.right - rectScreen.left + 1;
		int iHeight = rectScreen.bottom - rectScreen.top + 1;

		// Reposition dialog to left third of screen with minimum border
		// [p16 #2627] - set the x starting coordinate to be WORKAREA.left as opposed to 0
		// and the y starting coordinate to be WORKAREA.top + 1 as opposed to 1
		// this will take into account a Start Menu on the top or left hand side of the screen
		::SetWindowPos( m_hWnd, NULL, rectScreen.left, 1 + rectScreen.top, iWidth / 3, 
			iHeight - 26, SWP_NOZORDER );

		// Create the dialog toolbar - disabling the buttons
		createToolBar();
		updateButtons();

		// Setup the grids
		prepareGrids();

		// Read and process the INI file
		processINIFile();

		// Do initial resize and reposition of controls
		m_bInitialized = true;
		doResize( iWidth / 3 - 8, iHeight - 26 );

		// Display the dialog
		ShowWindow( SW_SHOW );
		SetFocus();

		// Prepare and Show Spot Recognition Window
		// [p16 #2627] - offset the x,y position rectScreen.left and rectScreen.top pixels 
		prepareAndShowSRIR( iWidth, iHeight, rectScreen.left, rectScreen.top);

		// Retrieve "other" settings from registry
		getRegistrySettings();

		// Get pointer to Generic Display OCX
		IDispatchPtr ipDispatch = m_ipSRIR->GetGenericDisplayOCX();
		ASSERT_RESOURCE_ALLOCATION( "ELI11370", ipDispatch != NULL );
		m_ipOCX = (_DUCLIDGenericDisplay *) ipDispatch.GetInterfacePtr();
		ASSERT_RESOURCE_ALLOCATION( "ELI11371", m_ipOCX != NULL );

		// Set the selection color
		m_ipOCX->setSelectionColor(m_crSelection);

		// Create the Attribute Finder Engine
		m_ipEngine.CreateInstance( CLSID_AttributeFinderEngine );
		ASSERT_RESOURCE_ALLOCATION("ELI13158", m_ipEngine != NULL);

		// Create the AFUtility object
		m_ipAFUtility.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION("ELI11254", m_ipAFUtility != NULL);

		// Set the stopwatch and start it
		m_nTimerID = SetTimer(1, giNUM_SECONDS_TO_REFRESH, NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11185")

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CDataAreaDlg::OnPaint() 
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
		CRect rectDlg;
		GetWindowRect( &rectDlg );
		CRect rectToolBar;
		if (m_apToolBar.get())
		{
			m_apToolBar->GetWindowRect( &rectToolBar );
			int iToolBarHeight = rectToolBar.Height();
			int iDialogWidth = rectDlg.Width();

			// With gray and white pens, draw horizontal lines that span the entire width
			// of the dialog, and that are just below the toolbar buttons
			CPen penGray;
			CPen penWhite;
			penGray.CreatePen(  PS_SOLID, 0, RGB( 128, 128, 128 ) );
			penWhite.CreatePen( PS_SOLID, 0, RGB( 255, 255, 255 ) );

			// First the gray line
			dc.SelectObject( &penGray );
			dc.MoveTo( 0, iToolBarHeight );
			dc.LineTo( iDialogWidth, iToolBarHeight );

			// Next the white line, one pixel below the gray
			dc.SelectObject( &penWhite );
			dc.MoveTo( 0, iToolBarHeight + 1 );
			dc.LineTo( iDialogWidth, iToolBarHeight + 1 );
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	// Minimum width to allow display of buttons, text, list
	lpMMI->ptMinTrackSize.x = giDATAAREADLG_MIN_WIDTH;

	// Minimum height to allow display of list, edit boxes, buttons
	lpMMI->ptMinTrackSize.y = giDATAAREADLG_MIN_HEIGHT;
}
//--------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnSize(UINT nType, int cx, int cy) 
{
	try
	{
		doResize( cx, cy );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11162");

	CDialog::OnSize(nType, cx, cy);
}
//-------------------------------------------------------------------------------------------------
BOOL CDataAreaDlg::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			handleCharacter( pMsg->wParam );

			m_ipSRIR->NotifyKeyPressed( pMsg->wParam );

			// Eat any characters so that 
			// they do not cause grid scrolling
			return TRUE;
		}

		// need to make sure the tool tip control has been created before 
		// sending messages to it
		if (m_ToolTipCtrl.m_hWnd != NULL)
		{
			// Show tooltips
			m_ToolTipCtrl.RelayEvent( pMsg );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18526");

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
BOOL CDataAreaDlg::OnToolTipNotify(UINT id, NMHDR * pNMHDR, LRESULT *pResult)
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
BOOL CDataAreaDlg::DestroyWindow() 
{
	try
	{
		// Call base class functionality
		return CDialog::DestroyWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11207")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnClose() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		::DestroyWindow( getSRIRWindowHandle() );

		// Call base class functionality
		CDialog::OnClose();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11389")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnTimer(UINT nIDEvent)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// If there is no current file, then update the current file editbox with "Waiting"
		// Dividing by 1000 converts to seconds from milliseconds 
		// because time(NULL) operates in seconds
		if ( m_strSourceDocName == "" && 
			(time(NULL) - m_timeCurrentFileLastUpdated) > (giNUM_SECONDS_TO_REFRESH / 1000) )
		{
			setCurrentFileName("Waiting...");
			SetDlgItemText(IDC_EDIT_DOC_NAME, "");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14414")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonSave()
{
	try
	{
		try
		{
			// Check enabled/disabled state
			if (!m_bEnableGeneral || m_bActiveConfirmationDlg)
			{
				return;
			}

			// Ensure required data is specified before saving
			if (promptBeforeNewDocument("save file"))
			{
				return;
			}

			CWaitCursor	wait;

			// Stop the current timer since we are saving the file
			m_swCurrTimeViewed.stop();

			// Save the output files
			createOutputFiles();

			// Save the IDShield Data(P16 #2901)
			saveIDShieldData();

			// Add information for this document to the queue, if desired
			if (m_nNumPreviousDocsToQueue > 0)
			{
				// Advance to next item in history, if re-reviewing documents OR
				// Move to current "new" document
				if (isInHistoryQueue())
				{
					// Update history items to catch any changes
					updateCurrentHistoryItems();

					// Update index
					m_nPositionInQueue++;

					// Load the "next" document in history
					navigateToHistoryItem( m_nPositionInQueue );

					// Do not post FILE_COMPLETE message
					return;
				}
				// Saving new document, update the history
				else
				{
					// Increment number stored, up to the maximum
					m_nNumPreviousDocsQueued++;

					// History is full, remove oldest
					if (m_nNumPreviousDocsQueued > m_nNumPreviousDocsToQueue)
					{
						m_vecDocumentHistory.erase(m_vecDocumentHistory.begin());

						// Delete the data from the first item data vector, 
						// then remove it from the history vector
						clearDataItemVector(m_vecDataItemHistory[0]);
						m_vecDataItemHistory.erase(m_vecDataItemHistory.begin());

						m_vecReviewedPagesHistory.erase(m_vecReviewedPagesHistory.begin());

						// Erase the first item in duration list(P16 2897)
						m_vecDurationsHistory.erase(m_vecDurationsHistory.begin());

						// Erase the first item in the IDShieldData list(P16 2901) 
						m_vecIDShieldDataHistory.erase(m_vecIDShieldDataHistory.begin());

						// Adjust history count
						m_nNumPreviousDocsQueued--;
					}

					// Set position index to count because we are not reviewing old docs
					m_nPositionInQueue = m_nNumPreviousDocsQueued;

					// Add new items to end of history collections
					updateCurrentHistoryItems();

					// Update toolbar buttons
					// and continue to post the FILE_COMPLETE message
					updateButtons();
				}
			}

#ifdef _VERIFICATION_LOGGING
			// Add entries to default log file
			ThreadSafeLogFile tslf;
			tslf.writeLine( "Ready to reset() and Post WM_FILE_COMPLETE from Save()" );
#endif

			// Reset SRIR and Dialog while waiting for another file
			reset();

			// Post WM_FILE_COMPLETE message to thread
			PostMessage( WM_FILE_COMPLETE, 0, 0 );
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11333");
	}
	catch(UCLIDException &ue)
	{
		// Display and log the exception if in the history queue.  If not in history queue then
		// the exception will be logged downstream
		ue.display(isInHistoryQueue());

		// If not in the history queue then prompt the user to fail the file.
		if (!isInHistoryQueue())
		{
			int iResult = 0;
			{
				WindowDisabler wd(getSRIRWindowHandle());
				m_bActiveConfirmationDlg = true;

				// Prompt the user to see if they want to fail the document and move on or not.
				iResult = MessageBox(
					"Would you like to fail this document and move to the next document?",
					"Unable To Save", MB_ICONQUESTION | MB_YESNO);
				m_bActiveConfirmationDlg = false;
			}

			// Check if the user wants to fail the file
			if (iResult == IDYES)
			{
				// Reset the SRIR viewer
				reset();

				// Clear the data item vector
				clearDataItemVector(m_vecDataItems);

				char* pszException = NULL;
				unsigned long ulLength = 0;
				try
				{
					string strException = ue.asStringizedByteStream();
					ulLength = strException.length() + 1;
					pszException = new char[ulLength];
					memset(pszException, 0, ulLength);
					memcpy(pszException, strException.c_str(), ulLength-1);
				}
				catch(...)
				{
					if (pszException != NULL)
					{
						delete [] pszException;
						pszException = NULL;
						ulLength = 0;
					}
				}

				// Post the file failed message
				PostMessage(WM_FILE_FAILED, (WPARAM)pszException, ulLength);
			}
			else
			{
				// Since the file has not been failed it will still be in the UI, restart
				// the stop watch
				m_swCurrTimeViewed.start();

				// Log the exception since it has not been logged yet
				ue.log();
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonToggleRedact() 
{
	try
	{
		try
		{
			// Check enabled/disabled state
			if (!m_bEnableToggle || m_bActiveConfirmationDlg)
			{
				return;
			}

			// Check active Settings object
			if (m_pActiveDDS != NULL)
			{
				// Disable the SRIR window
				WindowDisabler wd(getSRIRWindowHandle());

				// Update redaction and protect against multiple confirmations
				m_bActiveConfirmationDlg = true;
				m_pActiveDDS->toggleRedactChoice(m_hWnd);
				m_bActiveConfirmationDlg = false;

				// Set dirty flag
				m_bChangesMadeForHistory = true;

				// Update image window
				updateDataItemHighlights(m_pActiveDDS, true);

				// Update grid
				refreshDataRow(m_iDataItemIndex, m_pActiveDDS, false);

				// Update menu items
				updateButtons();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11334");
	}
	catch(UCLIDException& uex)
	{
		// Ensure the active confirmatio dialog flag is reset
		m_bActiveConfirmationDlg = false;

		uex.display();
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonPreviousItem() 
{
	try
	{
		// Check enabled/disabled state + visibility of Confirmation dialog
		if (!m_bEnablePrevious || m_bActiveConfirmationDlg)
		{
			return;
		}

		// Check if previous items in Data grid 
		long lCount = m_vecVerifyAttributes.size();

		// Next item with attribute
		// initialize to the current item index
		int iNextDataItemIndex = m_iDataItemIndex;

		// If need to visit all pages and active page is
		// between (current/previous) and next item
		if (m_UISettings.getReviewAllPages() && 
			m_iActivePage > getGridItemPageNumber( m_iDataItemIndex ))
		{
			iNextDataItemIndex =  getNextUnviewedItem( m_iDataItemIndex );
			// If passed the last item, set the value to the (last index +1)
			// "shift + Tab" will display the last item
			if (iNextDataItemIndex == 0)
			{
				iNextDataItemIndex = lCount + 1;
			}
		}

		if ((iNextDataItemIndex > 1) && (lCount > 0))
		{
			// Get Page Number text for previous Data grid item
			long lPage = getGridItemPageNumber( iNextDataItemIndex - 1 );

			// Just move to this item if still on the same page
			if (m_iActivePage == lPage)
			{
				selectDataRow( iNextDataItemIndex - 1 );
			}
			// Any pages in between these Display items
			else if (m_iActivePage - lPage > 1)
			{
				// Force review of previous page even without Attribute
				// if desired AND page has not yet been reviewed
				if (m_UISettings.getReviewAllPages())
				{
					for (int i = m_iActivePage - 1; i >= lPage; --i)
					{
						if (!isPageViewed( i ) )
						{
							setPage( i, false );
							break;
						}
						// If all the pages between active page and next redacted page has
						// already been visited
						else if (i == lPage)
						{
							selectDataRow( iNextDataItemIndex -1 );
							break;
						}
					}
				}
				// No, just skip intervening pages
				else
				{
					setPage( lPage, false );
					selectDataRow( iNextDataItemIndex - 1 );
				}
			}
			// Previous item is on the previous page, move to that item
			else
			{
				setPage( lPage, false );
				selectDataRow( iNextDataItemIndex - 1 );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11335")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonNextItem() 
{
	try
	{
		// Check enabled/disabled state + visibility of Confirmation dialog
		if (!m_bEnableGeneral || m_bActiveConfirmationDlg)
		{
			return;
		}

		bool	bDoPromptAutoSave = false;

		// Check if additional items in Data grid 
		long lCount = m_vecVerifyAttributes.size();

		// Check if there are pages after last page with attributes.
		bool bNoMorePages = true;
		int iFirstUnviewedPage = 0;
		
		int iLastAttributedPage = 0;
		if (lCount > 0)
		{
			iLastAttributedPage = getGridItemPageNumber( lCount );
		}

		if (iLastAttributedPage < m_iTotalPages)
		{
			bNoMorePages = false;
			for (int i = iLastAttributedPage + 1; i <= m_iTotalPages; ++i)
			{
				if (!isPageViewed(i))
				{
					iFirstUnviewedPage = i;
					break;
				}
			}
		}
		// Two cases when need to tab to the next item
		bool caseOneContinueTab = (lCount > 0) && (m_iDataItemIndex < lCount);
		bool caseTwoContinueTab = (lCount > 0) && (!bNoMorePages) && 
			 (iFirstUnviewedPage !=0) && (m_UISettings.getReviewAllPages());

		// Processing if there are unviewed items in the grid or if
		// forced to visit all pages and there are unviewed pages after
		// last page with attribute
		if ( caseOneContinueTab || caseTwoContinueTab && (m_iDataItemIndex == lCount))
		{
			// Determine next unviewed item
			long lNextItem = getNextUnviewedItem( m_iDataItemIndex );
			// If lTempNextItem is zero, which means there is no next unviewed item in the grid
			long lTempNextItem = lNextItem;

			if (lNextItem == 0)
			{
				// No more unviewed items, consider the next item
				if ((m_iDataItemIndex == -1) && (lCount > 0))
				{
					// No selected item yet, move to the first row
					lNextItem = 1;
				}
				else
				{
					lNextItem = m_iDataItemIndex + 1;
				}
			}

			// Get Page Number of this unviewed or next item
			long lPage = getGridItemPageNumber( lNextItem );

			// Check if no next unviewed item in the grid and there are any 
			// unviewed pages after last attributed page. If forced to 
			// visit all pages, those pages should be visited.
			if (lTempNextItem == 0 && caseTwoContinueTab)
			{
				lPage = iFirstUnviewedPage;
			}
			
			// Just move to this item if still on the same page
			if (m_iActivePage == lPage)
			{
				selectDataRow( lNextItem );
			}
			// Any pages in between these Display items
			else if (lPage - m_iActivePage > 1 )
			{
				// Force review of next pages even without Attribute
				// if desired AND pages has not yet been reviewed
				if (m_UISettings.getReviewAllPages())
				{
					for (int i = m_iActivePage + 1; i <= lPage; ++i)
					{
						if (!isPageViewed( i ) && i != lPage)
						{
							setPage( i, false );
							break;
						}
						// If all the pages between active page and next attributed page have
						// already been visited
						else if (i == lPage)
						{
							// Check if lPage is the next item in the Grid
							if (lPage != getGridItemPageNumber( lNextItem ))
							{
								setPage( i, false );
							}
							else
							{
								selectDataRow( lNextItem );
							}
							break;
						}
					}
				}
				// No, just skip intervening pages
				else
				{
					if (lPage != getGridItemPageNumber( lNextItem ))
					{
						setPage( lPage, false );
					}
					else
					{
						selectDataRow( lNextItem );
					}
				}
			}
			// Next item is on the next page, move to that item
			else
			{
				if (lPage != getGridItemPageNumber( lNextItem ))
				{
					setPage( lPage, false );
				}
				else
				{
					selectDataRow( lNextItem );
				}
			}
		}
		// No additional data items
		else
		{
			// Determine next page to be viewed
			int iNextPage = getNextPageToView();
			if (iNextPage > 0)
			{
				// Just move to this page and return
				setPage( iNextPage, true );
				return;
			}
			else
			{
				// Ensure required data is specified before moving to the next document
				if (promptBeforeNewDocument("move to next document"))
				{
					return;
				}

				// Create beginning portion of prompt text
				CString zPrompt;
				if (lCount == 0)
				{
					// No data found or no VOA file provided
					zPrompt += "You have visited ";
				}
				else
				{
					// All data reviewed
					zPrompt += "All found sensitive data and clues have been displayed to you.\n"
							   "You have visited ";
				}

				// Add variable text
				if (m_UISettings.getReviewAllPages())
				{
					zPrompt += "all";
				}
				else
				{
					// Determine how many pages have been viewed
					int iNumViewed = 0;
					int iIndex;
					for (iIndex = 1; iIndex <= m_iTotalPages; iIndex++)
					{
						if (isPageViewed( iIndex ))
						{
							iNumViewed++;
						}
					}

					// Create "x of y" text
					CString zText;
					zText.Format( "%d of %d", iNumViewed, m_iTotalPages );

					// Add to prompt
					zPrompt += zText;
				}

				// Back to regular text
				zPrompt += " pages in this document.\n\n"
					"Would you like to save this document and advance to the next?";

				int iResult = 0;
				{
					// Disable the SRIR window
					WindowDisabler wd(getSRIRWindowHandle());

					m_bActiveConfirmationDlg = true;
					iResult = MessageBox( zPrompt, "Confirm Save Redactions", 
						MB_YESNO | MB_ICONQUESTION);
					m_bActiveConfirmationDlg = false;
				}
				if (iResult == IDYES)
				{
#ifdef _VERIFICATION_LOGGING
					// Add entry to default log file
					ThreadSafeLogFile tslf;
					tslf.writeLine( "Calling Save() from Next() after user confirmation" );
#endif

					OnButtonSave();
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11336")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonZoom() 
{
	try
	{
		// Check enabled/disabled state
		if (!m_bEnableGeneral || m_bActiveConfirmationDlg)
		{
			return;
		}

		// Check page number of current Data grid item
		long lPage = getGridItemPageNumber( m_iDataItemIndex );

		// Compare to active page
		if (lPage == m_iActivePage)
		{	
			// Get bounding box of first raster zone
			int iIndex = m_vecVerifyAttributes[m_iDataItemIndex - 1];
			IRasterZonePtr ipZone = m_vecDataItems[iIndex].getFirstRasterZone();
			if (ipZone != NULL)
			{
				// Get elements of bounding rectangle
				ILongRectanglePtr ipBounds = ipZone->GetRectangularBounds(NULL);
				long nWidth = ipBounds->Right - ipBounds->Left;
				long nX = (ipBounds->Right + ipBounds->Left) / 2;
				long nY = (ipBounds->Bottom + ipBounds->Top) / 2;

				// Pad the width by the size of the selection border [FlexIDSCore #3462]
				nWidth += (giZONE_BORDER_WIDTH * 2);

				// Get the zoom multiplier and apply it
				int iZoomMultiplier = m_OptionsDlg.getAutoZoomScale();
				long nZoomWidth = nWidth * iZoomMultiplier;

				// Zoom around the bounding rectangle
				m_ipSRIR->ZoomPointWidth(nX, nY, nZoomWidth);
			}
		}
		// Else no Attributes on the active page and 
		// zoom should do nothing
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11337")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonStop() 
{
	try
	{
		// Check for active confirmation dialog
		if (m_bActiveConfirmationDlg)
		{
			return;
		}

		// Check dirty flag to see if any changes have been made
		// and prompt the user before stopping verification. [FlexIDSCore #3331]
		if (m_bChangesMadeForHistory || m_bChangesMadeToMostRecentDocument)
		{
			int iResult = 0;
			{
				// Disable the SRIR window
				WindowDisabler wd(getSRIRWindowHandle());

				m_bActiveConfirmationDlg = true;
				// Prompt user before stopping verification
				iResult = MessageBox("Current document has not been saved."
					" Are you sure you want to close and lose all changes?",
					"Close And Lose Changes?", MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2);

				m_bActiveConfirmationDlg = false;
			}

			// If NO then just return, do not stop the verification UI
			if (iResult == IDNO)
			{
				return;
			}
		}

		// Display wait cursor
		CWaitCursor	wait;

		// Disable the stop button
		m_apToolBar->GetToolBarCtrl().EnableButton(ID_BUTTON_STOP, FALSE);

		// Reset SRIR and Dialog while waiting for file processing to cancel
		reset();

		// Tell the thread to stop verification
		PostMessage(WM_CLOSE_VERIFICATION_DLG, 0, 0);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23467")
}

//-------------------------------------------------------------------------------------------------
LRESULT CDataAreaDlg::OnLButtonLClkRowCol(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// Extract Row and Column
		int nRow = LOWORD( lParam );
		int nCol = HIWORD( lParam );

		// Check ID
		if (wParam == IDC_SUMMARY_GRID)
		{
			int iPage = (nRow - 1) * giPAGES_PER_ROW + nCol;

			// Make sure that click is within the defined pages (P16 #2054)
			if (iPage <= m_iTotalPages)
			{
				// Move to new page and select first item on the page
				setPage( iPage, true );
			}
		}
		else if (wParam == IDC_DATA_GRID)
		{
			// Compute selected page - ignore clicks in header
			if (nRow > 0)
			{
				selectDataRow( nRow );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11310")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDataAreaDlg::OnDoubleClickRowCol(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// Check if:
		// 1) the data grid was clicked
		// 2) a row other than the header was clicked
		// 3) the exemption codes column was clicked
		if (wParam == IDC_DATA_GRID && LOWORD(lParam) > 0 && 
			HIWORD(lParam) == giDATAGRID_EXEMPTION_COLUMN) 
		{
			// Allow the user to select exemption codes for this row
			selectExemptionCodes();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24908")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDataAreaDlg::OnModifyCell(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// Extract Row and Column
		int nRow = LOWORD( lParam );
		int nCol = HIWORD( lParam );

		// Check ID  and column #
		// Only care about modifications in the type column of the data grid
		// [p16 #2722]
		if (wParam == IDC_DATA_GRID && nCol == giDATAGRID_TYPE_COLUMN)
		{
			// get the new type value from the grid
			string strNewType = m_GridData.GetCellValue(nRow, nCol);

			// set the last redaction type seen value - [p16 #2835] - JDS 01/29/2008
			m_strLastSelectedRedactionType = strNewType;

			// update the attribute with the new type
			updateCurrentlyModifiedAttribute(nRow, strNewType);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19856");

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonOptions() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{	
		// back-up the settings
		bool bAutoZoom = m_OptionsDlg.getAutoZoom();
		int iAutoZoomScale = m_OptionsDlg.getAutoZoomScale();

		// If the user clicks ok
		if ( m_OptionsDlg.DoModal() == IDOK )
		{
			// Save settings
			saveOptions();
		}
		else
		{
			// User clicked cancel, restore the previous settings
			m_OptionsDlg.setAutoZoom( bAutoZoom );
			m_OptionsDlg.setAutoZoomScale( iAutoZoomScale );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11325")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonHelpAbout() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Display the About box with version information
		m_ipEngine->ShowHelpAboutBox( kIDShieldHelpAbout, _bstr_t("") );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13157")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonPreviousDoc() 
{
	try
	{
		// Check visibility of Confirmation dialog
		if (m_bActiveConfirmationDlg)
		{
			return;
		}

		// moving from document, make sure we reset the value of the cell to not highlight
		m_GridData.UpdateCellToNotHighlight(-1, -1);

		// Check queueing status, must have non-empty queue and not be re-reviewing 
		// the oldest document
		if ( (m_nNumPreviousDocsToQueue > 0) && (m_nNumPreviousDocsQueued > 0) && 
			(m_nPositionInQueue > 0))
		{
			// If this is not the newest (current) document, ensure required 
			// data is specified before moving to the previous document
			if (isInHistoryQueue())
			{
				if (promptBeforeNewDocument("move to previous document"))
				{
					return;
				}
			}

			// Check dirty flag to see if any changes have been made 
			// if this is not the newest (current) document
			if (m_bChangesMadeForHistory && isInHistoryQueue())
			{
				// Prompt the user to save or ignore the changes
				int iResult = 0;
				{
					// Disable the SRIR window
					WindowDisabler wd(getSRIRWindowHandle());

					m_bActiveConfirmationDlg = true;
					iResult = MessageBox( gstrSAVE_CHANGES_PROMPT.c_str(), 
					gstrSAVE_CHANGES_CAPTION.c_str(), MB_ICONQUESTION | MB_YESNOCANCEL );
					m_bActiveConfirmationDlg = false;
				}

				// Do nothing if user Cancels
				if (iResult == IDCANCEL)
				{
					// Update the history items but do not save changes to data items(P16 2902)
					updateCurrentHistoryItems(false);
					return;
				}
				// Update history items
				else if (iResult == IDYES)
				{
					// Save the output files
					createOutputFiles();

					// Update the history items for recent changes or for 
					// moving from the current document to a history document
					updateCurrentHistoryItems();
				}
				
				// Save the IDShield Data(P16 2901)
				saveIDShieldData();

				// Else no update to history items but continue the navigation
			}
			// We are either moving to a history document from the current document or
			// there were no changes to the current document.
			else
			{
				// Need to save IDShield data only if this is not the current "new" doc(P16 2901)
				// Also, only need to update the metadata if this is not the current "new" doc
				if (isInHistoryQueue())
				{
					// Update the verification time in the XML file [FlexIDSCore #3453]
					writeMetadata(getCurrentDocument());

					// Save the IDShield Data
					saveIDShieldData();
				}
				else
				{
					m_bChangesMadeToMostRecentDocument |= m_bChangesMadeForHistory;
				}

				// History data must be updated
				updateCurrentHistoryItems();
			}

			// Decrement queue position
			m_nPositionInQueue--;

			// Load and display this previous document in history
			navigateToHistoryItem( m_nPositionInQueue );

			// Clear the dirty flag
			m_bChangesMadeForHistory = false;
		}

		// Update the toolbar
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14739")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonNextDoc() 
{
	try
	{
		// Check visibility of Confirmation dialog
		if (m_bActiveConfirmationDlg)
		{
			return;
		}

		// moving from document, make sure we reset the value of the cell to not highlight
		m_GridData.UpdateCellToNotHighlight(-1, -1);

		// Check queueing status, must have non-empty queue and not be re-reviewing 
		// the newest document
		if ( (m_nNumPreviousDocsToQueue > 0) && (m_nNumPreviousDocsQueued > 0) && 
			isInHistoryQueue())
		{
			// Ensure required data is specified before moving to the next document
			if (promptBeforeNewDocument("move to next document"))
			{
				return;
			}

			// Check dirty flag to see if any changes have been made 
			if (m_bChangesMadeForHistory)
			{
				int iResult = 0;
				{
					// Disable the SRIR window
					WindowDisabler wd(getSRIRWindowHandle());

					// Prompt the user to save or ignore the changes
					m_bActiveConfirmationDlg = true;
					iResult = MessageBox( gstrSAVE_CHANGES_PROMPT.c_str(), 
						gstrSAVE_CHANGES_CAPTION.c_str(), MB_ICONQUESTION | MB_YESNOCANCEL );
					m_bActiveConfirmationDlg = false;
				}

				// Do nothing if user Cancels
				if (iResult == IDCANCEL)
				{
					// Update the history items but do not save changes to data items(P16 2902)
					updateCurrentHistoryItems(false);
					return;
				}
				// Update history items
				else if (iResult == IDYES)
				{
					// Save the output files
					createOutputFiles();

					// Update the history items for recent changes
					updateCurrentHistoryItems();
				}
				// Else no update to history items but continue the navigation
			}
			else
			{
				// Update the verification time in the XML file [FlexIDSCore #3453]
				writeMetadata(getCurrentDocument());

				// Update the history items but do not save changes to data items(P16 2902)
				updateCurrentHistoryItems(false);
			}

			// Save the IDShield Data(P16 2901)
			saveIDShieldData();

			// Increment queue position
			m_nPositionInQueue++;

			// Load and display this next document in history
			navigateToHistoryItem( m_nPositionInQueue );

			// Clear the dirty flag
			m_bChangesMadeForHistory = false;
		}

		// Update the toolbar
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14740")
}
//-------------------------------------------------------------------------------------------------
BOOL CDataAreaDlg::OnNcActivate(BOOL bActive)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{	
		// added as per P16 #2720
		// Code from: ogkb.chm
		// Section 4: Advanced questions - Comboboxes - How can I avoid the title
		// bar flashing when a CGXComboBox is dropped down?
		if (GXDiscardNcActivate())
		{
			return TRUE;
		}

		return CDialog::OnNcActivate(bActive);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20222");

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonApplyExemptions()
{
	try
	{
		selectExemptionCodes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24989")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonApplyAllExemptions()
{
	try
	{
		selectExemptionCodes(true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24994")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::OnButtonLastExemptions()
{
	try
	{
		applyLastExemptionCodes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24993")
}
//-------------------------------------------------------------------------------------------------

