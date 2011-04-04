#include "stdafx.h"
#include "resource.h"
#include "SpotRecognitionDlg.h"
#include "DragOperation.h"
#include "SpotRecDlgToolBar.h"
#include "PromptDlg.h"
#include "GDDFileManager.h"
#include "SRWShortcuts.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <TemporaryResourceOverride.h>
#include <MRUList.h>
#include <CursorToolTipCtrl.h>
#include <Win32Util.h>
#include <MiscLeadUtils.h>
#include <COMUtils.h>

#include <l_bitmap.h>
#include <ltwrappr.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

static const string gstrAllDigitsString = "0123456789";

//--------------------------------------------------------------------------------------------------
LRESULT CALLBACK CallWndProc(int nCode,      // hook code
							 WPARAM wParam,  // current-process flag
							 LPARAM lParam)   // address of structure with message data 
{
	// Switch the module state for the correct handle to be used.
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CWPSTRUCT *pCWPSTRUCT = (CWPSTRUCT *) lParam;
	map<HWND, HHOOK>::const_iterator iter;
	iter = SpotRecognitionDlg::ms_mapSRIRToHook.find(pCWPSTRUCT->hwnd);
	HHOOK hHook = NULL;
	if (iter != SpotRecognitionDlg::ms_mapSRIRToHook.end())
		hHook = iter->second;

	try
	{
		static int nState = 1;
		static WPARAM lastWParam;
		static LPARAM lastLParam;
		static HWND lastHwnd;

		if (pCWPSTRUCT->message == WM_ENTERSIZEMOVE && nState == 1)
		{
			if (SpotRecognitionDlg::ms_mapSRIRToHook.find(pCWPSTRUCT->hwnd) != 
				SpotRecognitionDlg::ms_mapSRIRToHook.end())
			{
				lastHwnd = pCWPSTRUCT->hwnd;
				nState = 2;
				SpotRecognitionDlg::ms_bSizingInProgress = true;
			}
		}
		else if (pCWPSTRUCT->message == WM_EXITSIZEMOVE && nState == 2)
		{
			nState = 1;
			SpotRecognitionDlg::ms_bSizingInProgress = false;

			SendMessage(lastHwnd, WM_SIZE, lastWParam, lastLParam);
		}
		else if (pCWPSTRUCT->message == WM_SIZE)
		{
			lastWParam = wParam;
			lastLParam = lParam;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03231")

	return CallNextHookEx(hHook, nCode, wParam, lParam);
}
//--------------------------------------------------------------------------------------------------
BOOL SpotRecognitionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		try
		{
			// base class initialization
			CDialog::OnInitDialog();
			SetIcon(m_hIcon, TRUE);			// Set big icon
			SetIcon(m_hIcon, FALSE);		// Set small icon
			
			// create the toolbar associated with this window
			createToolBar();
			
			// update the window title
			updateWindowTitle("");
			
			// Positioning is done if this is the only instance
			if (ms_vecInstances.size() == 1)
			{
				// Retrieve persistent size and position of dialog
				loadWindowPosition();
			}
			
			// Load the default text height and color from persistent store
			long		lZoneHeight = 0;
			ma_pSRIRCfgMgr->getZoneHeight( lZoneHeight );
			
			// Apply the default text height and color
			m_UCLIDGenericDisplayCtrl.setZoneHighlightHeight( lZoneHeight );
			m_UCLIDGenericDisplayCtrl.setZoneHighlightColor( ms_UNUSED_ENTITY_COLOR );
			
			// set the units for the x/y position indicators in the status bar
			m_UCLIDGenericDisplayCtrl.setXYTrackingOption(2);

			// set the flag to control whether we want to display the percentage in statusbar
			m_UCLIDGenericDisplayCtrl.enableDisplayPercentage(ma_pSRIRCfgMgr->getDisplayPercentageEnabled());

			// congfigure the enabled/disabled state of the toolbar buttons
			configureToolBarButtonsAndUGD();

			// by default, no tool is selected
			setCurrentTool(kNone);
			
			// resize the OCX to be the size of the dialog box
			m_bInitialized = true;
			positionUGDControl();

			// setup a hook to monitor messages to this window
			// this hook is used to detect the WM_ENTERSIZEMOVE and the WM_EXITSIZEMOVE messages
			// and accordingly prevent unnecessary refreshes of the UGD until the re-sizing is 
			// completed.
			m_hWndCopy = m_hWnd;
			ms_mapSRIRToHook[m_hWnd] = SetWindowsHookEx(WH_CALLWNDPROC, CallWndProc, NULL, GetCurrentThreadId());

			createSelectionToolMenu();

			// create a new cursor tooltip ctrl associated with this window.
			m_apCursorToolTipCtrl = unique_ptr<CursorToolTipCtrl>(new CursorToolTipCtrl(this));
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI03008")
	}
	catch (UCLIDException& ue)
	{
		ue.display();

		// if essential controls have not been initialized then just exit
		if (!(m_apToolBar.get()))
		{
			CDialog::OnCancel();
		}

	}
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnSize(UINT nType, int cx, int cy) 
{
	CDialog::OnSize(nType, cx, cy);

	positionUGDControl();
	
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);	

	// Call zoomToFit() to make the page fit to window when resizing
	zoomToFit();
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnOK()
{
	// override the OnOk method so that the ImageRecognition dialog does
	// not disappear when enter or esc keys are pressed
	return;
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnCancel( )
{
	// override the OnCancel method so that the ImageRecognition dialog does
	// not disappear when enter or esc keys are pressed
	return;
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnChangeGoToPageText()
{
	try
	{

		// This Handler will be called any time a change is made to the text.  The trick is
		// that we only want to update the page when the user presses enter.  The way this is done
		// is by saving the text in the edit box every time this method is called.  We know enter has 
		// been pressed when the text is the same across two notifications.  There's got to be a better way 
		// to do this but for now it's OK

		string strCurrentGoToPageText = m_apToolBar->getCurrentGoToPageText();
		if (m_strLastGoToPageText == strCurrentGoToPageText)
		{
			// we have got a change event, but the text has not changed.  This is the
			// exact symptom of the ENTER key having been pressed.  So, process the input
			// and navigate to the specified page number if appropriate.

			long nNumberEnd = strCurrentGoToPageText.find_first_not_of(gstrAllDigitsString, 0);
			if (nNumberEnd != 0)
			{
				string strFirstNumber = strCurrentGoToPageText.substr(0, nNumberEnd);

				long lEnteredValue = asLong(strFirstNumber);
				long lTotalPages = m_UCLIDGenericDisplayCtrl.getTotalPages();

				if (lEnteredValue > 0 && lEnteredValue <= lTotalPages)
				{
					// the user has entered a valid page number - navigate to the 
					// specified page.
					gotoPage(lEnteredValue);
				
					if (m_ipSRWEventHandler)
					{
						m_ipSRWEventHandler->NotifyCurrentPageChanged();
					}
				}
			}
			// This will reformat the edit box to the proper page display
			resetGoToPageText();
		}
		else
		{
			m_strLastGoToPageText = strCurrentGoToPageText;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12021")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnKillFocusGoToPageText()
{
	// reset the go-to-page text every time the go-to-page edit box loses focus
	resetGoToPageText();
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNOpenImage() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CWaitCursor wait;

		// Notify observers of the SpotRecognitionDlg that we want to open a file
		if (m_ipSRWEventHandler != __nullptr)
		{
			VARIANT_BOOL	vbAllowOpen;
			vbAllowOpen = m_ipSRWEventHandler->NotifyOpenToolbarButtonPressed();
			if (vbAllowOpen == VARIANT_FALSE)
			{
				// Observer says NO, just return
				return;
			}
		}

		// check to see if the current contents have been saved?
		if (m_bUserSavingAllowed && isModified())
		{
			// Present MessageBox to user
			int iRes = MessageBox("Do you want to save changes you made to this document?", 
				"Confirmation", MB_ICONQUESTION | MB_YESNOCANCEL);

			if (iRes == IDCANCEL)
			{
				// the user does not want to continue with this operation.
				return;
			}
			else if (iRes == IDYES)
			{
				// save the document
				OnBTNSave();
			}
		}

		// Retrieve the last opened directory
		string strDefaultDir = (ma_pSRIRCfgMgr->getLastFileOpenDirectory()).c_str();

		// bring up the open dialog box here....
		CFileDialog openDialog(TRUE, NULL, NULL, OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			"All image files|*.bmp*;*.rle*;*.dib*;*.rst*;*.gp4*;*.mil*;*.cal*;*.cg4*;*.flc*;*.fli*;*.gif*;*.jpg*;*.pcx*;*.pct*;*.png*;*.tga*;*.tif*;*.pdf*|"
			"GDD files|*.gdd|"
			"BMP files (*.bmp;*.rle;*.dib)|*.bmp*;*.rle*;*.dib*|"
			"CALS1 files (*.rst;*.gp4;*.mil;*.cal;*.cg4)|*.rst*;*.gp4*;*.mil*;*.cal*;*.cg4*|"
			"FLIC files (*.flc;*.fli)|*.flc*;*.fli*|"
			"GIF files (*.gif)|*.gif*|"
			"JFIF files (*.jpg)|*.jpg*|"
			"PCX files (*.pcx)|*.pcx*|"
			"PICT files (*.pct)|*.pct*|"
			"PNG files (*.png)|*.png*|"
			"TGA files (*.tga)|*.tga*|"
			"TIFF files (*.tif)|*.tif*|"
			"PDF files (*.pdf)|*.pdf*|"
			"All files (*.*)|*.*"
			"||", NULL);

		// Modify the initial directory
		openDialog.m_ofn.lpstrInitialDir = strDefaultDir.c_str();
	
		if (openDialog.DoModal() == IDOK)
		{
			// get the path to the image that the user wants to open
			string strPathName = openDialog.GetPathName();

			openFile2(strPathName);

		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03049")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNSave() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (!m_bUserSavingAllowed)
		{
			return;
		}
		// TODO: remember the last save directory
		string strDefaultFileName = m_UCLIDGenericDisplayCtrl.getImageName() + ".gdd";

		// delete any temporary highlights before saving
		deleteTemporaryHighlight();

		// bring up the save dialog box here....
		CFileDialog saveDialog
			(FALSE, 
			"gdd", 
			strDefaultFileName.c_str(), 
			OFN_NOREADONLYRETURN | OFN_OVERWRITEPROMPT | OFN_NOCHANGEDIR,	// no read only file can be overwritten
			"GDD files (*.gdd)|*.gdd|"
			"All files (*.*)|*.*|"
			"||", 
			NULL);

		if (saveDialog.DoModal() == IDOK)
		{
			// save the graphical contents of this control to the specified file
			string strSaveFileName = saveDialog.GetPathName();

			CWaitCursor wait;

			saveAs(strSaveFileName);
		}

		// reset the control to be in pan mode
		setCurrentTool(kPan);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03050")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNZoomWindow() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ZoomWindow) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_ZoomWindow) != 0))
		{
			return;
		}
		setCurrentTool(kZoomWindow);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03051")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNZoomIn() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ZoomIn) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_ZoomIn) != 0))
		{
			return;
		}
		setCurrentTool(kZoomIn);
		// push current view on to the view stack 
		addCurrentViewToStack();
		configureZoomPrevNextButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03052")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNZoomOut() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ZoomOut) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_ZoomOut) != 0))
		{
			return;
		}
		setCurrentTool(kZoomOut);
		// push current view on to the view stack
		addCurrentViewToStack();
		configureZoomPrevNextButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03053")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNFitPage() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_FitPage) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_FitPage) != 0))
		{
			return;
		}

		if (m_eFitToStatus == kFitToPage)
		{
			// If the Fit to page button is already pressed, set it to unpressed
			m_eFitToStatus = kFitToNothing;
			setFitToButtonsStatus();
		}
		else
		{
			// If the Fit to page button is unpressed, press it and update the image
			setCurrentTool(kFitPage);

			// Set the Fit to status to registry
			ma_pSRIRCfgMgr->setFitToStatus(true);
			// push current view on to the view stack
			addCurrentViewToStack();
			configureZoomPrevNextButtons();
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14655")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNFitWidth() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_FitWidth) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_FitWidth) != 0))
		{
			return;
		}
		if (m_eFitToStatus == kFitToWidth)
		{
			// If the Fit to width button is already pressed, set it to unpressed
			m_eFitToStatus = kFitToNothing;
			setFitToButtonsStatus();
		}
		else
		{
			// If the Fit to page button is unpressed, press it and update the image
			setCurrentTool(kFitWidth);

			// Set the Fit to status to registry
			ma_pSRIRCfgMgr->setFitToStatus(false);
			// push current view on to the view stack
			addCurrentViewToStack();
			configureZoomPrevNextButtons();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14658")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNZoomPrev() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ZoomPrev) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_ZoomPrev) != 0))
		{
			return;
		}
		// get previous view
		PageExtents prevView = m_mapPageToViews[m_UCLIDGenericDisplayCtrl.getCurrentPageNumber()].gotoPreviousView();

		// lStatus is used to get the fit-to status after zooming window
		// If the return lStatus is 1, then the image after zooming is in fit to page status
		// If the return lSatatus is 0, then the image after zooming is in fit to width status
		long lStatus = -1;
		// set zoom window
		m_UCLIDGenericDisplayCtrl.zoomWindow(prevView.dBottomLeftX, prevView.dBottomLeftY,
											 prevView.dTopRightX, prevView.dTopRightY, &lStatus);

		if (lStatus == 1)
		{
			// Set to fit to page
			m_eFitToStatus = kFitToPage;
		}
		else if(lStatus == 0)
		{
			// Set to fit to width
			m_eFitToStatus = kFitToWidth;
		}
		else
		{
			// Set to fit to nothing
			m_eFitToStatus = kFitToNothing;
		}

		// Unpress or press the fit to buttons
		setFitToButtonsStatus();

		// update toolbar buttons for zoom prev and zoom next
		configureZoomPrevNextButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03211")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNZoomNext() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ZoomNext) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_ZoomNext) != 0))
		{
			return;
		}
		// get next view
		PageExtents nextView = m_mapPageToViews[m_UCLIDGenericDisplayCtrl.getCurrentPageNumber()].gotoNextView();

		// lStatus is used to get the fit-to status after zooming window
		// If the return lStatus is 1, then the image after zooming is in fit to page status
		// If the return lSatatus is 0, then the image after zooming is in fit to width status
		long lStatus = -1;
		// set zoom window
		m_UCLIDGenericDisplayCtrl.zoomWindow(nextView.dBottomLeftX, nextView.dBottomLeftY,
											 nextView.dTopRightX, nextView.dTopRightY, &lStatus);

		if (lStatus == 1)
		{
			// Set to fit to page
			m_eFitToStatus = kFitToPage;
		}
		else if(lStatus == 0)
		{
			// Set to fit to width
			m_eFitToStatus = kFitToWidth;
		}
		else
		{
			// Set to fit to nothing
			m_eFitToStatus = kFitToNothing;
		}

		// Unpress or press the fit to buttons
		setFitToButtonsStatus();

		// update toolbar buttons for zoom prev and zoom next
		configureZoomPrevNextButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03212")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNPan() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_Pan) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_Pan) != 0))
		{
			return;
		}
		setCurrentTool(kPan);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03055")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNRotateLeft() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_RotateLeft) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_RotateLeft) != 0))
		{
			return;
		}
		CWaitCursor wait;
		rotateCurrentPage(kRotateLeft);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03215")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNRotateRight() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_RotateRight) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_RotateRight) != 0))
		{
			return;
		}
		CWaitCursor wait;
		rotateCurrentPage(kRotateRight);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03216")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMouseMoveGenericDisplayCtrl(short Button, short Shift, long x, long y) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// in the following if statement, we are checking the value of the Button variable because
		// a drag operation only makes sense when Button != 0
		if (m_apCurrentDragOperation.get() && Button != 0)
		{
			// allow the current drag operation object to process the MouseMove event
			m_apCurrentDragOperation->onMouseMove(Button, Shift, x, y);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03056")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMouseDownGenericDisplayCtrl(short Button, short Shift, long x, long y) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (m_apCurrentDragOperation.get())
		{
			// allow the current drag operation object to process the MouseDown event
			m_apCurrentDragOperation->onMouseDown(Button, Shift, x, y);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03057")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMouseUpGenericDisplayCtrl(short Button, short Shift, long x, long y) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (m_apCurrentDragOperation.get())
		{
			// allow the current drag operation object to process the MouseUp event
			m_apCurrentDragOperation->onMouseUp(Button, Shift, x, y);
			
			// once a pan or a zoom window operation is completed, 
			// record the current view extents
			if (Button == 1 && (m_eCurrentTool == kPan || m_eCurrentTool == kZoomWindow))
			{
				addCurrentViewToStack();
				configureZoomPrevNextButtons();

				if (m_eCurrentTool == kZoomWindow)
				{
					// Unpress the fit-to buttons on the toolbar if the current tool is kZoomWindow
					m_eFitToStatus = kFitToNothing;
					setFitToButtonsStatus();
				}
			}

			if (Button == MK_LBUTTON)
			{
				// If the drag operation is to be auto-repeated, then reinitialize it, otherwise,
				// delete the drag operation object
				if (m_apCurrentDragOperation->autoRepeat())
				{
					m_apCurrentDragOperation->init();
				}
				else
				{
					// set to previous tool
					setCurrentTool(m_ePreviousTool);
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03058")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnKeyDownGenericdisplayctrl(short FAR* KeyCode, short Shift) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		bool bHandled = handleShortCutKeys((long)*KeyCode);
		// TODO: Add your control notification handler code here
		if (!bHandled && m_ipSRWEventHandler != __nullptr)
		{
			m_ipSRWEventHandler->NotifyKeyPressed(*KeyCode, Shift);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11169")
	
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnDblClickGenericdisplayctrl() 
{
/*	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (m_apCurrentDragOperation)
		{
			m_apCurrentDragOperation->onDblClick();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04812")*/
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNFirstPage() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_FirstPage) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_FirstPage) != 0))
		{
			return;
		}
		gotoPage(1);
		if (m_ipSRWEventHandler)
		{
			m_ipSRWEventHandler->NotifyCurrentPageChanged();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03009")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNLastPage() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_LastPage) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_LastPage) != 0))
		{
			return;
		}
		long lTotalPages = m_UCLIDGenericDisplayCtrl.getTotalPages();
		gotoPage(lTotalPages);

		if (m_ipSRWEventHandler)
		{
			m_ipSRWEventHandler->NotifyCurrentPageChanged();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03010")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNNextPage() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_NextPage) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_NextPage) != 0))
		{
			return;
		}
		long lCurrentPage = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
		gotoPage(lCurrentPage+1);

		if (m_ipSRWEventHandler)
		{
			m_ipSRWEventHandler->NotifyCurrentPageChanged();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03011")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNPreviousPage() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_PreviousPage) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_PreviousPage) != 0))
		{
			return;
		}
		long lCurrentPage = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
		gotoPage(lCurrentPage-1);

		if (m_ipSRWEventHandler)
		{
			m_ipSRWEventHandler->NotifyCurrentPageChanged();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03012")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNEditZoneText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_EditZoneText) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_EditZoneText) != 0))
		{
			return;
		}
		setCurrentTool(kEditZoneText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03059")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNDeleteEntities() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_DeleteEntities) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_DeleteEntities) != 0))
		{
			return;
		}

		setCurrentTool(kDeleteEntities);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03060")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBtnSelectHighlight() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_SELECT_ENTITIES) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_SELECT_ENTITIES) != 0))
		{
			return;
		}

		setCurrentTool(kSelectHighlight);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23472")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNSelectText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_SelectText) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_SelectText) != 0))
		{
			return;
		}
		if (m_bEnableTextSelection || m_bAlwaysAllowHighlighting)
		{
			if (m_apToolBar->GetToolBarCtrl().IsButtonPressed(IDC_BTN_SelectText) == FALSE)
			{
				// Set the current select text tool as the current tool
				// if the select text button is not pressed
				// [LRCAU #5959] - Add special handling for word selection tool.
				setCurrentTool(
					m_eCurrSelectionTool == kSelectWordText ? kSelectRectText : m_eCurrSelectionTool);
			}
			else
			{
				// If the select text button is already pressed
				// switch to the other select text tool.
				// E.g. If the current select text tool is swipe tool and it is pressed,
				// click "h" will toggle the rect select tool
				setCurrentTool((m_eCurrSelectionTool == kSelectText) ? kSelectRectText : kSelectText);
			}
		}
		else
		{
			if (m_apToolBar->GetToolBarCtrl().IsButtonPressed(IDC_BTN_SelectText) == FALSE)
			{
				// Set the current select text tool as the current tool
				// if the select text button is not pressed
				if (m_eCurrSelectionTool == kSelectRectText)
				{
					setCurrentTool(kInactiveSelectRectText);
				}
				else
				{
					setCurrentTool(kInactiveSelectText);
				}
			}
			else
			{
				// If the select text button is already pressed
				// switch to the other select text tool.
				// E.g. If the current select text tool is swipe tool and it is pressed,
				// click "h" will toggle the rect select tool
				if (m_eCurrSelectionTool == kSelectText)
				{
					m_eCurrSelectionTool = kSelectRectText;
					setCurrentTool(kInactiveSelectRectText);
				}
				else
				{
					m_eCurrSelectionTool = kSelectText;
					setCurrentTool(kInactiveSelectText);
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03061")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNSetHighlightHeight() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_SetHighlightHeight) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_SetHighlightHeight) != 0))
		{
			return;
		}
		setCurrentTool(kSetHighlightHeight);

		// Retrieve and store zone height
		long		lHeight = 0;
		lHeight = m_UCLIDGenericDisplayCtrl.getZoneHighlightHeight();
		
		ma_pSRIRCfgMgr->setZoneHeight( lHeight );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03062")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::performOCROnEntireImage()
{
	// show the wait cursor as this operation may take some time
	CWaitCursor waitCursor;

	// process the image for paragraph text as appropriate
	string strImageName = m_UCLIDGenericDisplayCtrl.getImageName();
	long lEndPage = m_UCLIDGenericDisplayCtrl.getTotalPages();
	processImageForParagraphText(strImageName, 1, lEndPage);
}
//--------------------------------------------------------------------------------------------------
ISpatialStringPtr SpotRecognitionDlg::getCurrentPageText()
{
	// get information about the current image and current page number
	_bstr_t _bstrImageFile = m_UCLIDGenericDisplayCtrl.getImageName();
	string stdstrImageFile = _bstrImageFile;
	long nCurrentPageNum = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();

	// get the text associated with the current page, and in doing so follow
	// the usual procedure related to paragraph text recognition
	ISpatialStringPtr ipText = recognizeParagraphTextInImage(stdstrImageFile, nCurrentPageNum, 
		nCurrentPageNum);
	ASSERT_RESOURCE_ALLOCATION("ELI06545", ipText != __nullptr);

	return ipText;
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::performOCROnCurrentPage()
{
	// get information about the current image and current page number
	_bstr_t _bstrImageFile = m_UCLIDGenericDisplayCtrl.getImageName();
	string stdstrImageFile = _bstrImageFile;
	long nCurrentPageNum = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();

	// do paragraph text recognition on the current page
	processImageForParagraphText(stdstrImageFile, nCurrentPageNum, nCurrentPageNum);
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnPTHMenuItemSelected(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// remember the index of the selected Paragraph text handler 
		m_nLastSelectedPTHIndex = (int)((nID - ID_OCR_ENTIRE_IMAGE_01)/m_nOCROptionsSpanLen);

		// get the remainder
		EOCRRegionType eOCRRegionType = (EOCRRegionType)((nID - ID_OCR_ENTIRE_IMAGE_01)%m_nOCROptionsSpanLen);

		// perform / invoke the type of OCR region operation the
		// user has requested.
		switch (eOCRRegionType)
		{
		case kOCREntireImage:
			performOCROnEntireImage();
			break;

		case kOCRCurrentPage:
			performOCROnCurrentPage();
			break;
			
		case kOCRRectRegion:
			setCurrentTool(kRecognizeTextInRectRegion);
			break;
			
		case kOCRPolyRegion:
			setCurrentTool(kRecognizeTextInPolyRegion);
			break;
			
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI04771")
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04772")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::showPTHMenu(POINT menuPos)
{
	// Create the top level menu which will have all PTH descriptions as the menu items
	CMenu topMenu;
	topMenu.CreatePopupMenu();

	static CString zEntireImage("Process Entire Image");
	static CString zCurrentPage("Process Current Page");
	static CString zRectangle("Process Rectangular Area");
// TODO: Once figure out the problem with creating polygon in XP/2000, 
// uncomment out the following
//	static CString zPolygon("Process POLYGONAL area");
	
	// for each PTH, add a menu item with the description
	int nNumPTHs = m_ipParagraphTextHandlers->Size();
	// if there's only one paragraph text handler, no need for any sub menu
	if (nNumPTHs == 1)
	{
		// get the text description of the PTH
		UCLID_SPOTRECOGNITIONIRLib::IParagraphTextHandlerPtr ipPTH;
		ipPTH = m_ipParagraphTextHandlers->At(0);
		ASSERT_RESOURCE_ALLOCATION("ELI19193", ipPTH != __nullptr);

		if (ipPTH->IsPTHEnabled() == VARIANT_TRUE)
		{
			// add a menu item with the description of the PTH
			_bstr_t _bstrPTHDescription = ipPTH->GetPTHDescription();
			string stdstrPTHDescription = _bstrPTHDescription;
			CString zDescription = stdstrPTHDescription.c_str();
			CString zItem1 = zDescription + " (" + zEntireImage + ")";
			CString zItem2 = zDescription + " (" + zCurrentPage + ")";
			CString zItem3 = zDescription + " (" + zRectangle + ")";
	// TODO: Once figure out the problem with creating polygon in XP/2000, 
	// uncomment out the following
	//		CString zItem4 = zDescription + " (" + zPolygon + ")";
			// create an item that has a popup menu
			topMenu.AppendMenu(MF_BYPOSITION, ID_OCR_ENTIRE_IMAGE_01+0, zItem1);
			topMenu.AppendMenu(MF_BYPOSITION, ID_OCR_ENTIRE_IMAGE_01+1, zItem2);
			topMenu.AppendMenu(MF_BYPOSITION, ID_OCR_ENTIRE_IMAGE_01+2, zItem3);
	// TODO: Once figure out the problem with creating polygon in XP/2000, 
	// uncomment out the following
	//		topMenu.AppendMenu(MF_BYPOSITION, ID_OCR_CURRENT_PAGE_01+3, zItem3);
		}
	}
	else
	{
		for (int i = 0; i < nNumPTHs; i++)
		{
			// get the text description of the PTH
			UCLID_SPOTRECOGNITIONIRLib::IParagraphTextHandlerPtr ipPTH;
			ipPTH = m_ipParagraphTextHandlers->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI19194", ipPTH != __nullptr);

			if (ipPTH->IsPTHEnabled() == VARIANT_TRUE)
			{
				// create sub menu for ocr options
				CMenu subMenu;
				subMenu.CreatePopupMenu();
				// the id number for process current page
				int nCurrentPageValue = ID_OCR_ENTIRE_IMAGE_01 + i * m_nOCROptionsSpanLen;
				// add the types of OCRing to the sub menu
				subMenu.AppendMenu(MF_BYPOSITION, nCurrentPageValue+0, zEntireImage);
				subMenu.AppendMenu(MF_BYPOSITION, nCurrentPageValue+1, zCurrentPage);
				subMenu.AppendMenu(MF_BYPOSITION, nCurrentPageValue+2, zRectangle);
	// TODO: Once figure out the problem with creating polygon in XP/2000, 
	// uncomment out the following
	//			subMenu.AppendMenu(MF_BYPOSITION, nCurrentPageValue+2, zPolygon);
				HMENU hmenu = subMenu.m_hMenu;
				
				// add a menu item with the description of the PTH
				_bstr_t _bstrPTHDescription = ipPTH->GetPTHDescription();
				string stdstrPTHDescription = _bstrPTHDescription;
				// create an item that has a popup menu
				topMenu.AppendMenu(MF_BYPOSITION|MF_POPUP, (int) hmenu, stdstrPTHDescription.c_str());
			}
		}
	}
	
	// show the popup menu at the specified location
	topMenu.TrackPopupMenu(TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
		menuPos.x, menuPos.y, this, NULL);
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNRecognizeTextAndProcess() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// get the current cursor position and show the PTH choice
		// menu at the current cursor location
		GetCursorPos(&m_lastPTHMenuPos);
		showPTHMenu(m_lastPTHMenuPos);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03063")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBtnOpenSubImage() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_OPENSUBIMAGE) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_OPENSUBIMAGE) != 0))
		{
			return;
		}
		setCurrentTool(kOpenSubImgInWindow);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03243")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMenuRectSelectionTool() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_SelectText) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_SelectText) != 0))
		{
			return;
		}

		if (m_apToolBar->GetToolBarCtrl().IsButtonPressed(IDC_BTN_SelectText) == FALSE)
		{
			// Set the tool as select rect text if the current tool is not select text.
			m_eCurrSelectionTool = kSelectRectText;
		}
		else
		{
			// OnBTNSelectText() always toggles between selection tools if the current tool is 
			// a selection tool, so if the user selects rect tool on the menu, we will first
			// set the current tool to swipe tool and let OnBTNSelectText() toggle it.
			m_eCurrSelectionTool = kSelectText;
		}

		// Call OnBTNSelectText() to set the current select text tool
		OnBTNSelectText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11349")
	
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMenuSwipeSelectionTool() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_SelectText) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_SelectText) != 0))
		{
			return;
		}

		if (m_apToolBar->GetToolBarCtrl().IsButtonPressed(IDC_BTN_SelectText) == FALSE)
		{
			// Set the tool as select swipe text if the current tool is not select text.
			m_eCurrSelectionTool = kSelectText;
		}
		else
		{
			// OnBTNSelectText() always toggles between selection tools if the current tool is 
			// a selection tool, so if the user selects swipe tool on the menu, we will first
			// set the current tool to rect tool and let OnBTNSelectText() toggle it.
			m_eCurrSelectionTool = kSelectRectText;
		}

		// Call OnBTNSelectText() to set the current select text tool
		OnBTNSelectText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11350")
	
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnEntitySelectedGenericDisplayCtrl(long ulEntityID) 
{
	try
	{
		// an entity may be selected under several circumstances.
		// do the appropriate for each circumstance.
		if (m_bEnableTextSelection && (m_eCurrentTool == kSelectText || m_eCurrentTool == kSelectRectText))
		{
			vector<unsigned long> vecZoneID(1, ulEntityID);
			fireOnInputReceived(vecZoneID);
		}
		else if (m_eCurrentTool == kEditZoneText)
		{
			string strEntityText = getZoneEntityText(ulEntityID);
			PromptDlg dlg("Set Text", "&Text", strEntityText.c_str(), true,
				true, true, this);
			if (dlg.DoModal() == IDOK)
			{
				// TODO: text input validation?
				setZoneEntityText(ulEntityID, (LPCTSTR) dlg.m_zInput);
			}
		}

		// Send out Entity Selected event (if applicable)
		if (m_ipSRWEventHandler != __nullptr)
		{
			m_ipSRWEventHandler->NotifyEntitySelected( ulEntityID );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03064")
}
//--------------------------------------------------------------------------------------------------
long SpotRecognitionDlg::getTextScore(const string& strText) 
{
	long lScore = 0;
	
	// if a line-text-evaluator has been specified, use it to determine a 
	// score for the specified text
	if (m_ipLineTextEvaluator)
	{
		_bstr_t _bstrText = strText.c_str();
		lScore = m_ipLineTextEvaluator->GetTextScore( _bstrText, m_strInputType.c_str() );
	}

	return lScore;
}
//--------------------------------------------------------------------------------------------------
string SpotRecognitionDlg::getZoneText(IRasterZonePtr& ripZone, IProgressStatus* pProgressStatus) 
{
	// recognize the text in the zone
	string strRecognizedText("");

	// first extract the zone into a temp image 
	string strExtension(::getExtensionFromFullPath((LPCTSTR)m_UCLIDGenericDisplayCtrl.getImageName()));
	TemporaryFileName tempImgFile(NULL, strExtension.c_str(), true);
	
	m_UCLIDGenericDisplayCtrl.extractZoneImage(ripZone->StartX, ripZone->StartY,
		ripZone->EndX, ripZone->EndY, ripZone->Height, ripZone->PageNumber,
		tempImgFile.getName().c_str());	
	
	// ensure that the OCR engine object pointer is not NULL
	if (m_ipOCREngine == __nullptr)
	{
		throw UCLIDException("ELI03591", "Missing OCR engine.");
	}

	// Create temporary file for bitmap output
	TemporaryFileName tempOutFile( NULL, ".bmp", true );

	L_INT nRet = L_FileConvert( (char *)tempImgFile.getName().c_str(),
		(char *)tempOutFile.getName().c_str(), FILE_BMP, 0, 0, 8, 0, NULL, NULL, NULL);
	throwExceptionIfNotSuccess(nRet, "ELI03419", "Unable to convert image.",
		(LPCTSTR) m_UCLIDGenericDisplayCtrl.getImageName());

	// specify the use of a training file associated with the current input type
	// if any errors are encountered, ignore them
	try
	{
		m_ipOCREngine->LoadTrainingFile( m_strInputType.c_str() );
	}
	catch (...)
	{
	}

	// perform recognition on the extracted image
	ISpatialStringPtr ipText;
	if(m_ipOCRFilter)
	{
		// a filter exists, use it
		ipText = m_ipOCREngine->RecognizeTextInImage(
			get_bstr_t(tempOutFile.getName()), 1, -1, kCustomFilter, 
			m_ipOCRFilter->GetValidChars( m_strInputType.c_str() ), 
			UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, pProgressStatus);
	}
	else
	{
		// no filter exists
		ipText = m_ipOCREngine->RecognizeTextInImage(
			get_bstr_t(tempOutFile.getName()), 1, -1, kNoFilter, "", 
			UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, pProgressStatus);
	}
	ASSERT_RESOURCE_ALLOCATION("ELI06503", ipText != __nullptr);

	strRecognizedText = ipText->String;

	// correct the recognized text, if a line-text-corrector has been
	// specified
	if (m_ipLineTextCorrector)
	{
		// correct the text;
		_bstr_t _bstrRecognizedText = strRecognizedText.c_str();
		_bstr_t _bstrCorrectedText = m_ipLineTextCorrector->CorrectText( 
			_bstrRecognizedText, m_strInputType.c_str() );

		// store the corrected text as the string to return
		strRecognizedText = _bstrCorrectedText;
	}

	return strRecognizedText;
}
//--------------------------------------------------------------------------------------------------
string SpotRecognitionDlg::getBestTextAroundZone(IRasterZonePtr& ripZone, IProgressStatus* pProgressStatus)
{
	long lBestAngle = 0;
	long lBestScore = 0;
	string strBestRecognizedText = "";
	
	// if a line text evaluator has been defined, then
	// do the automatic line text evaluation at multiple angles
	bool bEvaluatorExists = m_ipLineTextEvaluator != __nullptr;
	if (bEvaluatorExists)
	{
		const int iStepSize = ma_pSRIRCfgMgr->getAutoRotateStepSize();
		const int iNumSteps = ma_pSRIRCfgMgr->getNumAutoRotateSteps();

		for (int i = 0; i < iNumSteps; i++)
		{
			try
			{
				long lAngle;
				IRasterZonePtr ipTempZone(CLSID_RasterZone);
				ASSERT_RESOURCE_ALLOCATION("ELI03447", ipTempZone != __nullptr);

				long lTextScore;
				string strRecognizedText;

				// try recognizing the text with the zone rotated at a positive angle
				lAngle = (i + 1) * iStepSize;
				ripZone->CopyDataTo(ipTempZone);
				ipTempZone->RotateBy(lAngle);			
				strRecognizedText = getZoneText(ipTempZone, pProgressStatus);
				lTextScore = getTextScore(strRecognizedText);
				if (lTextScore > lBestScore)
				{
					lBestAngle = lAngle;
					lBestScore = lTextScore;
					strBestRecognizedText = strRecognizedText;
				}

				// try recognizing the text with the zone rotated at a negative angle
				lAngle *= -1;
				ripZone->CopyDataTo(ipTempZone);
				ipTempZone->RotateBy(lAngle);			
				strRecognizedText = getZoneText(ipTempZone, pProgressStatus);
				lTextScore = getTextScore(strRecognizedText);
				if (lTextScore > lBestScore)
				{
					lBestAngle = lAngle;
					lBestScore = lTextScore;
					strBestRecognizedText = strRecognizedText;
				}
			}
			catch (COleDispatchException* pEx)
			{
				pEx->Delete();
			}
			catch (...)
			{
			}
		}
	}
	
	// recognize the text in the zone at zero-degree angle rotation
	{
		string strRecognizedText;
		strRecognizedText = getZoneText(ripZone, pProgressStatus);
		
		if (bEvaluatorExists)
		{
			long lTextScore = getTextScore(strRecognizedText);

			// NOTE: in the following line we are using the >= operator because if there
			// is any other text with the same score as the text recognized at zero degrees,
			// then the zero-degree text gets preference.
			if (lTextScore >= lBestScore)
			{
				// set the best recognized text to equal the recently recognized text
				lBestAngle = 0;
				lBestScore = lTextScore;
				strBestRecognizedText = strRecognizedText;
			}
		}
		else
		{
			// if no line text evaluator is present, then the best
			// text is the one found at zero degrees.
			lBestAngle = 0;
			lBestScore = 0;
			strBestRecognizedText = strRecognizedText;
		}
	}

	// return the best recognized text
	return strBestRecognizedText;
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnZoneEntityMovedGenericDisplayCtrl(long ulEntityID) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Send move event
		if (m_ipSRWEventHandler != __nullptr)
		{
			m_ipSRWEventHandler->NotifyZoneEntityMoved(ulEntityID);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23550")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnZoneEntitiesCreatedGenericDisplayCtrl(IUnknown* pZoneIDs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	IVariantVectorPtr ipZoneIDs(pZoneIDs);
	ASSERT_RESOURCE_ALLOCATION("ELI23764", ipZoneIDs != __nullptr);

	try
	{
		long lSize = ipZoneIDs->Size;
		bool bExceptionCaught;
		if (m_bAutoOCR)
		{
			for (long i = 0; i < lSize; i++)
			{
				_variant_t var = ipZoneIDs->Item[i];
				unsigned long ulEntityID = var.lVal;

				bExceptionCaught = true;
				try
				{
					// Display the wait cursor while we OCR
					CWaitCursor waitCursor;

					// send out event (if applicable) to notify that we are about to
					// recognize line text
					if (m_ipSRWEventHandler != __nullptr)
					{
						m_ipSRWEventHandler->AboutToRecognizeLineText();
					}

					// get the zone entity parameters
					long lStartPosX, lStartPosY, lEndPosX, lEndPosY, lTotalZoneHeight;
					m_UCLIDGenericDisplayCtrl.getZoneEntityParameters(ulEntityID, &lStartPosX, 
						&lStartPosY, &lEndPosX, &lEndPosY, &lTotalZoneHeight);

					double dPadFactor = ma_pSRIRCfgMgr->getZonePadFactor();

					m_ipTempRasterZone->Clear();
					m_ipTempRasterZone->StartX = lStartPosX;
					m_ipTempRasterZone->StartY = lStartPosY;
					m_ipTempRasterZone->EndX = lEndPosX;
					m_ipTempRasterZone->EndY = lEndPosY;
					m_ipTempRasterZone->Height = (long)(lTotalZoneHeight * dPadFactor);

					// Padding can create Zone Coordinates that are outside the bounds of the image 
					// If that happens we will not pad the height
					// we only check the top and left because it's easier and bottom and right 
					// seem to be clipped (or somehow appropriately handled further
					// down the pipeline) It would be ideal to calculate the maximum allowed
					// padded height but that would be overkill for this minimal problem
					ILongRectanglePtr ipRect = m_ipTempRasterZone->GetRectangularBounds(NULL);
					if (ipRect->Top < 0 || ipRect->Left < 0)
					{
						m_ipTempRasterZone->Height = lTotalZoneHeight;
					}

					m_ipTempRasterZone->PageNumber =
						m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();

					// create a temp file as the OCR source in case if the input is invalid,
					// the zone image shall be displayed in InputCorrectionDlg.
					// Needs to be in the format of a bitmap since the input correction dlg
					// can only display bitmap.
					TemporaryFileName tempImgFile(NULL, ".bmp", true);
					m_strLastZoneImageFile = tempImgFile.getName();

					// extract the zone from original image into a temp image file
					// this image will be needed in case the InputCorrection dlg needs
					// to be displayed along with the zone image.
					m_UCLIDGenericDisplayCtrl.extractZoneEntityImage(ulEntityID, 
						tempImgFile.getName().c_str());

					long nZoneArea = m_ipTempRasterZone->Area;
					// get the best text that is approximately in the zone's area
					string strText = getBestTextAroundZone(m_ipTempRasterZone,
						getShowOCRProgress(nZoneArea));

					// record the id of the created entity so that a tooltip
					// can be displayed at the time it is marked as used.
					m_ulLastCreatedEntity = ulEntityID;

					// store the Text attribute for this Zone entity,
					// and fire an event
					setZoneEntityText(ulEntityID, strText);

					// reset the last zone image file name be nothing, as the
					// zone-image name returned by the IInputEntity.getOCRImage()
					// is only valid during the scope of the event being handled.
					m_strLastZoneImageFile = "";

					// if we have come so far, then no exceptions were thrown by the above code
					bExceptionCaught = false;
				}
				CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03598");

				// text recognition could not be performed on the the newly created zone.
				// so, delete it, beep, and (if no items left) return.
				if (bExceptionCaught)
				{
					m_UCLIDGenericDisplayCtrl.deleteEntity(ulEntityID);
					ipZoneIDs->Remove(i, 1);
					i--;
					lSize--;
					Beep(1000,100);

					// Check for size 0
					if (lSize == 0)
					{
						return;
					}
				}
			}
		}

		// Reset exception caught flag
		bExceptionCaught = true;
		try
		{
			// only send input received message to the input manager if the input is enabled
			if (m_bEnableTextSelection)
			{
				// Construct a vector of zone entity IDs
				vector<unsigned long> vecZoneIDs;
				vecZoneIDs.reserve(lSize);
				for (long i = 0; i < lSize; i++)
				{
					_variant_t var = ipZoneIDs->Item[i];
					vecZoneIDs.push_back(var.lVal);
				}

				// Fire the input received event
				fireOnInputReceived(vecZoneIDs);
			}

			// Send create event
			if (m_ipSRWEventHandler != __nullptr)
			{
				m_ipSRWEventHandler->NotifyZoneEntitiesCreated(ipZoneIDs);
			}

			// if we have come this far, then no exceptions were thrown by the above code
			bExceptionCaught = false;
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24113");

		// If an exception was thrown, need to empty the collection and
		// remove the zone entities from the generic display
		if (bExceptionCaught && lSize > 0)
		{
			for (int i = 0; i < lSize; i++)
			{
				// Get the entityID from the collection
				unsigned long ulEntityID = ipZoneIDs->Item[i].lVal;
				m_UCLIDGenericDisplayCtrl.deleteEntity(ulEntityID);
			}
			ipZoneIDs->Clear();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24112");
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnClose() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// delete any temporary highlights so that they disappear from the
		// window before the user is prompted to save (if there have been
		// modifications made)
		deleteTemporaryHighlight();

		// prompt the user...depending upon whether the document is in a modified state
		// check to see if the current contents have been saved?
		if (m_bUserSavingAllowed && isModified())
		{
			// Present MessageBox to user
			int iRes = MessageBox("Do you want to save changes you made to this document?", 
				"Confirmation", MB_ICONQUESTION | MB_YESNOCANCEL);

			if (iRes == IDCANCEL)
			{
				// the user does not want to continue with this operation.
				return;
			}
			else if (iRes == IDYES)
			{
				// save the document
				OnBTNSave();
			}
		}
		
		// if a drag operation is currently in progress, delete the drag operation
		// object
		if (m_apCurrentDragOperation.get())
		{
			m_apCurrentDragOperation.reset(__nullptr);
		}

		// remember current zone height
		ma_pSRIRCfgMgr->setZoneHeight(m_UCLIDGenericDisplayCtrl.getZoneHighlightHeight());
		// remember window position
		saveWindowPosition();
		
		IInputReceiverPtr ipReceiver = m_pInputEntityManager;
			
		// [LegacyRCAndUtils:5743]
		// Allow the m_ipSubImageHandler to handle the pending destruction to ensure child
		// subImage windows to ipReceiver are cleaned up to prevent memory leaks or a crash on
		// close.
		if (m_ipSubImageHandler != __nullptr)
		{
			m_ipSubImageHandler->NotifyAboutToDestroy(ipReceiver);
		}

		// when the user wants to close the window, send a message to the event handler
		// telling it that the user wants to close the window.  Do not actually close the IR window
		// the Window will automatically disappear when this dialog object is destructed by the 
		// owning ATL object.
		if (m_ipEventHandler != __nullptr)
		{
			m_ipEventHandler->NotifyAboutToDestroy(ipReceiver);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03065")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnSelectMRUPopupMenu(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		if (nID >= ID_MRU_FILE1 && nID <= ID_MRU_FILE8)
		{
			// Notify observers of the SpotRecognitionDlg that we want to open a file
			if (m_ipSRWEventHandler != __nullptr)
			{
				VARIANT_BOOL	vbAllowOpen;
				vbAllowOpen = m_ipSRWEventHandler->NotifyOpenToolbarButtonPressed();
				if (vbAllowOpen == VARIANT_FALSE)
				{
					// Observer says NO, just return
					return;
				}
			}

			// check to see if the current contents have been saved?
			if (m_bUserSavingAllowed && isModified())
			{
				// Present MessageBox to user
				int iRes = MessageBox("Do you want to save changes you made to this document?", 
					"Confirmation", MB_ICONQUESTION | MB_YESNOCANCEL);
				
				if (iRes == IDCANCEL)
				{
					// the user does not want to continue with this operation.
					return;
				}
				else if (iRes == IDYES)
				{
					// save the document
					OnBTNSave();
				}
			}

			// get the current selected file index of MRU list
			int nCurrentSelectedFileIndex = nID - ID_MRU_FILE1;
			// get file name string
			string strFileToOpen(ma_pRecentFiles->at(nCurrentSelectedFileIndex));

			openFile2(strFileToOpen);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03185")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnToolbarDropDown(NMHDR* pNMHDR, LRESULT *plr)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		NMTOOLBAR *pTB = (NMTOOLBAR *)pNMHDR;
		UINT nID = pTB->iItem;
		
		// Switch on button command id's.
		if (nID == IDC_BTN_OpenImage)
		{
			CWnd *pWnd = m_apToolBar.get();
			// load the popup MRU file list menu
			CMenu menuLoader;
			if (!menuLoader.LoadMenu(IDR_MENU_MRU))
			{
				throw UCLIDException("ELI03186", "Failed to load Most Recent Used File list.");
			}
			
			CMenu* pPopup = menuLoader.GetSubMenu(0);
			if (pPopup)
			{
				ma_pRecentFiles->readFromPersistentStore();
				int nSize = ma_pRecentFiles->getCurrentListSize();
				if (nSize > 0)
				{
					// remove the "No File" item from the menu
					pPopup->RemoveMenu(ID_MNU_MRU, MF_BYCOMMAND);
				}

				for(int i = nSize-1; i >=0 ; i--)
				{
					CString pszFile(ma_pRecentFiles->at(i).c_str());
					if (!pszFile.IsEmpty())
					{
						pPopup->InsertMenu(0, MF_BYPOSITION, ID_MRU_FILE1+i, pszFile);
					}
				}

				CRect rc;
				pWnd->SendMessage(TB_GETRECT, pTB->iItem, (LPARAM)&rc);
				pWnd->ClientToScreen(&rc);
				
				pPopup->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
					rc.left, rc.bottom, this, &rc);
			}
		}
		else if(nID == IDC_BTN_SelectText)
		{
			CWnd *pWnd = m_apToolBar.get();
			CMenu* pPopup = m_menuSelectionTools.GetSubMenu(0);

			CRect rc;
			pWnd->SendMessage(TB_GETRECT, pTB->iItem, (LPARAM)&rc);
			pWnd->ClientToScreen(&rc);
			
			pPopup->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
				rc.left, rc.bottom, this, &rc);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03184")
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnContextMenu(CWnd* pWnd, CPoint point) 
{
	if (m_apCurrentDragOperation.get())
	{
		// if current drag operation is in process, do not
		// display the following context menu
		if (m_apCurrentDragOperation->isInProcess())
		{
			return;
		}
	}

	// Create and load the context menu
	CMenu menu;
	menu.LoadMenu(IDR_MENU_CONTEXT);
	CMenu *pContextMenu = menu.GetSubMenu(0);
	
	// clear all checks if any 
	UINT nCheck = MF_BYCOMMAND | MF_CHECKED;
	UINT nUncheck = MF_BYCOMMAND | MF_UNCHECKED;
	pContextMenu->CheckMenuItem(ID_MNU_HIGHLIGHTER, nUncheck);
	pContextMenu->CheckMenuItem(ID_MNU_PAN, nUncheck);
	pContextMenu->CheckMenuItem(ID_MNU_ZOOMWINDOW, nUncheck);
	
	// set current active tool with a check mark
	switch (m_eCurrentTool)
	{
	case kSelectText:
	case kInactiveSelectText:
	case kSelectRectText:
	case kInactiveSelectRectText:
		pContextMenu->CheckMenuItem(ID_MNU_HIGHLIGHTER, nCheck);
		break;
	case kPan:
		pContextMenu->CheckMenuItem(ID_MNU_PAN, nCheck);
		break;
	case kZoomWindow:
		pContextMenu->CheckMenuItem(ID_MNU_ZOOMWINDOW, nCheck);
		break;
	}
	
	// enable/disable menu items accordingly.
	UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
	UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
	
	// Context menu items Enabled IFF toolbar button is Enabled AND NOT Hidden
	BOOL bEnable = (m_apToolBar->GetToolBarCtrl().IsButtonEnabled( IDC_BTN_ZoomWindow ) && 
		!m_apToolBar->GetToolBarCtrl().IsButtonHidden( IDC_BTN_ZoomWindow ));
	pContextMenu->EnableMenuItem(ID_MNU_ZOOMWINDOW, bEnable ? nEnable : nDisable);

	bEnable = (m_apToolBar->GetToolBarCtrl().IsButtonEnabled( IDC_BTN_Pan ) && 
		!m_apToolBar->GetToolBarCtrl().IsButtonHidden( IDC_BTN_Pan ));
	pContextMenu->EnableMenuItem(ID_MNU_PAN, bEnable ? nEnable : nDisable);

	bEnable = (m_apToolBar->GetToolBarCtrl().IsButtonEnabled( IDC_BTN_SelectText ) && 
		!m_apToolBar->GetToolBarCtrl().IsButtonHidden( IDC_BTN_SelectText ));
	pContextMenu->EnableMenuItem(ID_MNU_HIGHLIGHTER, bEnable ? nEnable : nDisable);
	
	// Display and manage the context menu
	pContextMenu->TrackPopupMenu(TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
								point.x, point.y, this);
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMnuHighlighter() 
{
	OnBTNSelectText();
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMnuPan() 
{
	OnBTNPan();
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMnuZoomwindow() 
{
	OnBTNZoomWindow();
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnMnuCancel() 
{
	// TODO: Add your command handler code here
	
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnDestroy() 
{
	// if a drag operation is currently in progress, delete the drag operation
	// object
	if (m_apCurrentDragOperation.get())
	{
		m_apCurrentDragOperation.reset(__nullptr);
	}

	CDialog::OnDestroy();
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnBTNPrint() 
{

	// Ignore this command if toolbar button is disabled or hidden
	if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_Print) == FALSE) || 
		(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_Print) != 0))
	{
		return;
	}

	// get the filename of the currently displayed image in the SRIR ImageViewer
	string strFileName = m_UCLIDGenericDisplayCtrl.getImageName();

	LFile file;
	LBitmapBase bitmap;
	file.SetBitmap(&bitmap);
	file.SetFileName(const_cast<char*>(strFileName.c_str()));

	// Get number of pages in image
	long nNumPages = m_UCLIDGenericDisplayCtrl.getTotalPages();
	
	CPrintDialog printDialog(false, PD_ALLPAGES | PD_USEDEVMODECOPIES | PD_HIDEPRINTTOFILE | PD_NOSELECTION);

	// Set up the page range restrictions
	printDialog.m_pd.nMinPage = 1;
	printDialog.m_pd.nMaxPage = (WORD)nNumPages;

	// display the printer dialog
	// if the user cancels we will return without printing
	if (printDialog.DoModal() != IDOK)
	{
		return;
	}

	// get the device constext for the printer
	HDC hDC = printDialog.GetPrinterDC();

	int nFromPage;
	int nToPage;

	// if print all pages was specified
	if (printDialog.PrintAll() == TRUE)
	{
		nFromPage = 1;
		nToPage = nNumPages;
	}
	// if a page range was specified we will use it instead
	else if (printDialog.PrintRange() == TRUE)
	{
		nFromPage = printDialog.GetFromPage();
		nToPage = printDialog.GetToPage();
	}
	// if print selection was specified
	else if (printDialog.PrintSelection() == TRUE)
	{
		nFromPage = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
		nToPage = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
	}
	
	// attach the device context handle to a CDC
	// so it will be deleted automatically
	CDC dc;
	dc.Attach(hDC);

	// Fill in a DOCINFO struct
	DOCINFO docinfo;
	memset(&docinfo, 0, sizeof(docinfo));
	docinfo.cbSize = sizeof(docinfo);
	docinfo.lpszDocName = _T(strFileName.c_str());

	// Tell the printer to start a new document
	dc.StartDoc(&docinfo);
	
	// Print each page in the 
	int i;
	for(i = nFromPage; i <= nToPage; i++)
	{
		// Tell the printer a new page is starting
		dc.StartPage();

		// Draw the page on the Printers device context
		file.Load(0, ORDER_RGB, i);
		LPrint print(&bitmap);
		print.Print(dc.GetSafeHdc(), 0, 0, 0, 0, false);
		bitmap.Free();
	
		// tell the printer that the page is done
		dc.EndPage();
	}

	// Tell the printer this document is finished
	dc.EndDoc();
	
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnDropFiles(HDROP hDropInfo)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Notify observers of the SpotRecognitionDlg that we want to open a file
		if (m_ipSRWEventHandler != __nullptr)
		{
			VARIANT_BOOL	vbAllowOpen;
			vbAllowOpen = m_ipSRWEventHandler->NotifyOpenToolbarButtonPressed();
			if (vbAllowOpen == VARIANT_FALSE)
			{
				// Observer says NO, just return
				return;
			}
		}

		// setup hDropInfo to automatically be released when we go out of scope
		DragDropFinisher finisher(hDropInfo);

		CWaitCursor wait;
		// check to see if the current contents have been saved?
		if (m_bUserSavingAllowed && isModified())
		{
			// Present MessageBox to user
			int iRes = MessageBox("Do you want to save changes you made to this document?", 
				"Confirmation", MB_ICONQUESTION | MB_YESNOCANCEL);

			if (iRes == IDCANCEL)
			{
				// the user does not want to continue with this operation.
				return;
			}
			else if (iRes == IDYES)
			{
				// save the document
				OnBTNSave();
			}
		}

		unsigned int iNumFiles = DragQueryFile(hDropInfo, 0xFFFFFFFF, NULL, NULL);
		for (unsigned int ui = 0; ui < iNumFiles; ui++)
		{
			// get the full path to the dragged filename
			char pszFile[MAX_PATH + 1];
			DragQueryFile(hDropInfo, ui, pszFile, MAX_PATH);

			string strFile = pszFile;
		
			string strExtension = getExtensionFromFullPath(strFile, true);
			// if this is an image file
			if (isImageFileExtension( strExtension )
				|| isNumericExtension( strExtension ))
			{
				// Attempt to open the file
				openFile2(strFile);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19390")
}
//--------------------------------------------------------------------------------------------------
bool SpotRecognitionDlg::handleShortCutKeys(long nKeyCode)
{
	bool bHandled = true;
	switch (nKeyCode)
	{
	case guiSRW_REFRESH_FILE:
		{
			OnRefreshImage();
		}
		break;
	case guiSRW_ACTIVATE_PAN:
		{
			OnBTNPan();
		}
		break;
	case guiSRW_ZOOM:
		{
			OnBTNZoomWindow();
		}
		break;
	case guiSRW_HIGHLIGHT:
		{
			OnBTNSelectText();
		}
		break;
	case guiSRW_ZOOM_IN:
		{
			OnBTNZoomIn();
		}
		break;
	case guiSRW_ZOOM_OUT:
		{
			OnBTNZoomOut();
		}
		break;
	case guiSRW_PREVIOUS_PAGE:
	case guiSRW_PREVIOUS_PAGE2:
		{
			OnBTNPreviousPage();
		}
		break;
	case guiSRW_NEXT_PAGE:
	case guiSRW_NEXT_PAGE2:
		{
			OnBTNNextPage();
		}
		break;
	case guiSRW_LAST_PAGE:
		{
			// See [FlexIDSCore:3443]
			if ((GetKeyState(VK_CONTROL) & 0x8000) != 0)
			{
				OnBTNLastPage();
			}
		}
		break;
	case guiSRW_FIRST_PAGE:
		{
			// See [FlexIDSCore:3443]
			if ((GetKeyState(VK_CONTROL) & 0x8000) != 0)
			{
				OnBTNFirstPage();
			}
		}
		break;
	case guiSRW_ZOOM_EXTENTS:
		{
			OnBTNFitPage();
		}
		break;
	case guiSRW_FIT_PAGE:
		{
			OnBTNFitPage();
		}
		break;
	case guiSRW_FIT_WIDTH:
		{
			OnBTNFitWidth();
		}
		break;
	case guiSRW_DELETE_ENTITIES:
		{
			OnBTNDeleteEntities();
		}
		break;
	case guiSRW_SUBIMAGE:
		{
			OnBtnOpenSubImage();
		}
		break;
	case guiSRW_ZOOM_PREVIOUS:
		{
			OnBTNZoomPrev();
		}
		break;
	case guiSRW_SELECT_ENTITIES:
		{
			OnBtnSelectHighlight();
		}
		break;
	default:
		bHandled = false;
	};

	return bHandled;
}
//--------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::OnRefreshImage()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// if opening new files is disabled, refresh is disabled too. [P16 #2765]
		if(!m_bCanOpenFiles)
		{
			return;
		}

		// check to see if the current contents have been saved?
		if (m_bUserSavingAllowed && isModified())
		{
			// Present MessageBox to user
			int iRes = MessageBox("Do you want to save changes you made to this document?", 
				"Confirmation", MB_ICONQUESTION | MB_YESNOCANCEL);

			if (iRes == IDCANCEL)
			{
				// the user does not want to continue with this operation.
				return;
			}
			else if (iRes == IDYES)
			{
				// save the document
				OnBTNSave();
			}
		}

		// get the image file name
		string strImageFileName = m_UCLIDGenericDisplayCtrl.getImageName();

		// if the file name is not empty string then open the file
		if (strImageFileName != "")
		{
			openFile2(strImageFileName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17573");
}
//--------------------------------------------------------------------------------------------------
