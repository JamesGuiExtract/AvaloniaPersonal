//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapDlg.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Duan Wang (June 2001 - present)
//			Arvind Ganesan (Aug 2001 - present)
//			John Hurd (till July 2001)
//
//==================================================================================================
#include "stdafx.h"
#include "IcoMapDlg.h" 
#include "IcoMapApp.h"

#include "DrawingToolFSM.h"
#include "IcoMapOptionsDlg.h"
#include "CfgIcoMapDlg.h"
#include "IcoMapMessages.h"
#include "CurveToolManager.h"
#include "CurrentCurveTool.h"
#include "DynamicInputGridWnd.h"

#include <IcoMapOptions.h>
#include <SplashWindow.h>
#include <CoreEvents.h>
#include <TemporaryResourceOverride.h>
#include <CfgAttributeViewer.h>
#include <UCLIDException.h>

#include "cpputil.h"

extern HINSTANCE gModuleResource;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants and globals
//-------------------------------------------------------------------------------------------------
const string IcoMapDlg::ICOMAP_TOOL_NAME = "IcoMap";

const int ICOMAP_DLG_MIN_WIDTH = 446;

HHOOK ghHook = NULL;			// Windows message hook
IcoMapDlg *IcoMapDlg::ms_pInstance = NULL;

//-------------------------------------------------------------------------------------------------
// Hook procedure for WH_GETMESSAGE hook type.
//-------------------------------------------------------------------------------------------------
LRESULT CALLBACK IcoMapDlgMsgProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	//	See KB Article: PRB: ActiveX Control Is the Parent Window of Modeless Dialog

	// Switch the module state for the correct handle to be used.
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Check whether or not to process the message.
	if (nCode >= 0 && PM_REMOVE == wParam)
	{
		// Translate specific messages in controls' PreTranslateMessage().
		LPMSG lpMsg = (LPMSG) lParam;
		UINT nMsg = lpMsg->message;
		if ((nMsg >= WM_KEYFIRST && nMsg <= WM_KEYLAST) 
			|| nMsg == WM_MOUSEWHEEL 
			|| nMsg == WM_COMMAND)
		{
			if (AfxGetApp()->PreTranslateMessage(lpMsg))
			{
				// The value returned from this hookproc is ignored, and it cannot
				// be used to tell Windows the message has been handled. To avoid
				// further processing, convert the message to WM_NULL before
				// returning.
				lpMsg->message = WM_NULL;
				lpMsg->lParam = 0L;
				lpMsg->wParam = 0;
			}
		}
		else if (nMsg == WM_MOUSEMOVE)
		{
			AfxGetApp()->PreTranslateMessage(lpMsg);
		}
	}
	
	// Pass the hook information to the next hook procedure in the current hook chain.
	return ::CallNextHookEx(ghHook, nCode, wParam, lParam);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(IcoMapDlg)
	DDX_Control(pDX, IDC_EDIT_Input, m_commandLine);
	DDX_Control(pDX, IDC_DSTEXT, m_staticInfo);
	//}}AFX_DATA_MAP
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(IcoMapDlg, CDialog)
	//{{AFX_MSG_MAP(IcoMapDlg)
	ON_WM_CLOSE()
	ON_COMMAND(IDC_BTN_Options, OnBTNOptions)
	ON_COMMAND(IDC_BTN_DrawingTools, OnBTNDrawingTools)
	ON_COMMAND(IDC_BTN_ReverseMode, OnBTNReverseMode)
	ON_COMMAND(IDC_BTN_ToggleCurveDirection, OnBTNToggleCurveDirection)
	ON_COMMAND(IDC_BTN_ToggleDeltaAngle, OnBTNToggleDeltaAngle)
	ON_COMMAND(ID_HELP_ABOUTICOMAP, OnHelpAbouticomap)
	ON_COMMAND(ID_TOOLS_ACTIONS_DELETESKETCH, OnToolsActionsDeletesketch)
	ON_COMMAND(ID_FILE_CLOSE, OnFileClose)
	ON_COMMAND(ID_HELP_ICOMAPHELP, OnHelpIcomaphelp)
	ON_COMMAND(ID_TOOLS_ACTIONS_FINISHPART, OnToolsActionsFinishpart)
	ON_COMMAND(ID_TOOLS_ACTIONS_FINISHSKETCH, OnToolsActionsFinishsketch)
	ON_NOTIFY(TBN_DROPDOWN, AFX_IDW_TOOLBAR, OnToolbarDropDown)
	ON_COMMAND(ID_TOOLS_OPTIONS, OnToolsOptions)
	ON_COMMAND(IDC_BTN_SelectFeatures, OnBTNSelectFeatures)
	ON_COMMAND(ID_TOOLS_EDITFEATURES, OnToolsEditfeatures)
	ON_COMMAND(ID_TOOLS_SELECTFEATURES, OnToolsSelectfeatures)
	ON_COMMAND(ID_TOOLS_DRAWINGDIRECTION_REVERSE, OnToolsDrawingdirectionReverse)
	ON_COMMAND(ID_TOOLS_DRAWINGDIRECTION_NORMAL, OnToolsDrawingdirectionNormal)
	ON_COMMAND(ID_CURVEOPERATIONS_CURVEDIRECTION_CONCAVELEFT, OnCurveoperationsCurvedirectionConcaveleft)
	ON_COMMAND(ID_CURVEOPERATIONS_CURVEDIRECTION_CONCAVERIGHT, OnCurveoperationsCurvedirectionConcaveright)
	ON_COMMAND(ID_CURVEOPERATIONS_DELTAANGLE_GREATERTHAN180, OnCurveoperationsDeltaangleGreaterthan180)
	ON_COMMAND(ID_CURVEOPERATIONS_DELTAANGLE_LESSTHAN180, OnCurveoperationsDeltaangleLessthan180)
	ON_COMMAND(IDC_BTN_ViewEditAttributes, OnBTNViewEditAttributes)
	ON_COMMAND(IDC_BTN_FinishSketch, OnBTNFinishSketch)
	ON_COMMAND(IDC_BTN_DeleteSketch, OnBTNDeleteSketch)
	ON_WM_DESTROY()
	ON_COMMAND(ID_TOOLS_OCRSCHEMES, OnToolsOcrschemes)
	ON_WM_ACTIVATE()
	ON_WM_MOVE()
	ON_WM_PAINT()
	ON_WM_SHOWWINDOW()
	ON_WM_SIZE()
	ON_COMMAND(IDC_BTN_DIG, OnBtnDig)
	ON_WM_MOUSEMOVE()
	ON_COMMAND(ID_VIEW_DIG, OnViewDig)
	ON_COMMAND(ID_VIEW_STATUS, OnViewStatus)
	ON_WM_GETMINMAXINFO()
	//}}AFX_MSG_MAP
	ON_COMMAND_RANGE(ID_TOOLS_DRAWINGTOOLS_LINE, ID_TOOLS_DRAWINGTOOLS_GENIE, OnSelectToolsDrawToolsMenu)
	ON_COMMAND_RANGE(ID_DRAWINGTOOL_LINE, ID_DRAWINGTOOL_GENIE, OnSelectDrawToolsPopupMenu)
	ON_WM_MENUSELECT()
	ON_MESSAGE(WM_GET_WINDOW_IR_MANAGER, OnGetWindowIRManager)
	ON_MESSAGE(WM_EXECUTE_COMMAND, OnExecuteCommand)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXT,0x0000,0xFFFF,OnToolTipNotify)
	//ON_WM_SYSCOMMAND()
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
BEGIN_EVENTSINK_MAP(IcoMapDlg, CDialog)
    //{{AFX_EVENTSINK_MAP(IcoMapDlg)
	//}}AFX_EVENTSINK_MAP
END_EVENTSINK_MAP()

//--------------------------------------------------------------------------------------------------
// IcoMapDlg
//-------------------------------------------------------------------------------------------------
IcoMapDlg::IcoMapDlg(CWnd* pParent)
	: CDialog(IcoMapDlg::IDD, pParent),
	m_bDeltaAngleGT180(false),
	m_bInitialized(false),
	m_bPointInputEnabled(true),
	m_bTextInputEnabled(false),
	m_bToggleDirectionEnabled(false),
	m_bToggleDeltaAngleEnabled(false),
	m_bToggleLeft(true),
	m_bFeatureCreationEnabled(false),
	m_bDeflectionAngleToolEnabled(false),
	m_ECurrentSelectedToolID(kBtnLine),
	m_EPreviousSelectedToolID(kBtnLine),
	m_ipDisplayAdapter(NULL),
	m_ipAttributeManager(NULL),
	m_ipIcoMapInputTarget(NULL),
/*	ma_pDrawingTool(NULL),
	ma_pToolTipCtrl(NULL),
	ma_pCfgIcoMapDlg(NULL),
	ma_pCfgAttributeViewer(NULL),*/
	m_ipIREventHandler(NULL),
	m_ipInputEntityMgr(NULL),
	m_bViewEditFeatureSelected(false),
	m_bIsIcoMapCurrentTool(false),
	m_bIcoMapIsActiveInputTarget(false),
	m_bFinishSketchEnabled(false),
	m_bDeleteSketchEnabled(false),
	m_nMinDlgHeight(0),
	m_nMinDIGHeight(0),
	m_nStatusBarHeight(0),
	m_nCommandLineHeight(0),
	m_nToolBarHeight(0),
	m_nDlgHeight1(0),
	m_nDlgHeight2(0),
	m_nDlgHeight3(0),
	m_nDlgHeight4(0)
{
	try
	{
		EnableAutomation();
						
		// Set the singelton InputManager as the object from which we are expecting to
		// receive input events
		UseSingletonInputManager();
		
		// get access to the singleton highlight window
		if (FAILED(m_ipHighlightWindow.CreateInstance(CLSID_HighlightWindow)))
		{
			throw UCLIDException("ELI04055", "Unable to access highlight window!");
		}
		// set parent window handle
		IInputManagerPtr ipInputManager = getInputManager();
		m_ipHighlightWindow->ParentWndHandle = ipInputManager->ParentWndHandle;
		
		if (FAILED(m_ipInputTargetManager.CreateInstance(CLSID_InputTargetManager)))
		{
			throw UCLIDException("ELI04070", "Unable to access the Input Target Manager!");
		}
		
		// create instance of the icomap input context.
		if (FAILED(m_ipIcoMapInputContext.CreateInstance(CLSID_IcoMapInputContext)))
		{
			throw UCLIDException("ELI04130", "Unable to create IcoMap Input Context object!");
		}

		// set the IcoMap input context to be the default input context
		ipInputManager->SetInputContext(m_ipIcoMapInputContext);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07600")
}
//--------------------------------------------------------------------------------------------------
IcoMapDlg::~IcoMapDlg()
{
	try
	{
		// Disconnect from the event sink of the input manager class right now
		// (otherwise, when the input manager goes out of scope, it will try to release
		// its reference to this object, and cause a debug error on exit)
		SetInputManager(NULL);

		// destroy BCMenu
		m_menuDrawingTools.DestroyMenu();

		// Clean up static objects
		CurveToolManager::sDeleteInstance();
		CurrentCurveTool::sDeleteInstance();
		IcoMapOptions::sDeleteInstance();
		
		// since this is a singleton class, if it is being deleted, then we need to reset the singleton
		// object pointer to NULL
		ms_pInstance = NULL;
		
		// unhook any hook procedures
		if (ghHook)
		{
			VERIFY(::UnhookWindowsHookEx(ghHook));
			ghHook = NULL;
		}

		// Clean up auto pointers
		ma_pDrawingTool.reset(NULL);
		ma_pDIGWnd.reset(NULL);
		//ma_pToolTipCtrl.reset(NULL);
		//ma_pCfgIcoMapDlg.reset (NULL);
		//ma_pCfgAttributeViewer.reset(NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07601")
}

//-------------------------------------------------------------------------------------------------
// IUnknown
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) IcoMapDlg::AddRef()
{
	InterlockedIncrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) IcoMapDlg::Release()
{
	InterlockedDecrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP IcoMapDlg::QueryInterface( REFIID iid, void FAR* FAR* ppvObj)
{
	IUnknown *pUknwn = this;
	
	if (iid == IID_IUnknown)
		*ppvObj = static_cast<IUnknown *>(pUknwn);
	else if (iid == IID_IDispatch)
		*ppvObj = static_cast<IDispatch *>(pUknwn);
	else
		*ppvObj = NULL;
	
	if (*ppvObj != NULL)
	{
		AddRef();
		return S_OK;
	}
	else
	{
		return E_NOINTERFACE;
	}
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
IcoMapDlg* IcoMapDlg::sGetInstance()
{
	if (ms_pInstance == NULL)
	{
		ms_pInstance = new IcoMapDlg();
		ASSERT_RESOURCE_ALLOCATION("ELI02142", ms_pInstance != NULL);
	}

	return ms_pInstance;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
BOOL IcoMapDlg::createModelessDlg(void)
{
	return Create(IcoMapDlg::IDD,NULL);
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::createToolBar()
{
	if (m_toolBar.CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
    | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC ) )
	{
		m_toolBar.LoadToolBar(IDR_TOOLBAR_ICOMAP);
	}

	m_toolBar.SetBarStyle(m_toolBar.GetBarStyle() |
		CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC );

	// must set TBSTYLE_TOOLTIPS here in order to get tool tips
	m_toolBar.ModifyStyle(0, TBSTYLE_TOOLTIPS);

	// If there's new button added to the toolbar, the button
	// id needs to be added here
	static UINT nButtonIds[] = {
		IDC_BTN_SelectFeatures,
		IDC_BTN_ViewEditAttributes,
		ID_SEPARATOR,
		IDC_BTN_DrawingTools,
		IDC_BTN_ReverseMode,
		IDC_BTN_ToggleCurveDirection,
		IDC_BTN_ToggleDeltaAngle,
		ID_SEPARATOR,
		IDC_BTN_FinishSketch,
		IDC_BTN_DeleteSketch,
		ID_SEPARATOR,
		IDC_BTN_DIG,
		ID_SEPARATOR,
		IDC_BTN_Options,
		ID_SEPARATOR,
		ID_HELP_ICOMAPHELP
	};

	// number of buttons (including sperators) for toolbar buttons
	int nNumButtons = sizeof(nButtonIds)/sizeof(nButtonIds[0]);

	m_toolBar.SetButtons(nButtonIds, nNumButtons);

	m_toolBar.SendMessage(TB_SETEXTENDEDSTYLE, 0, TBSTYLE_EX_DRAWDDARROWS);

	m_toolBar.GetToolBarCtrl().SetExtendedStyle(TBSTYLE_EX_DRAWDDARROWS);
	DWORD dwStyle = m_toolBar.GetButtonStyle(m_toolBar.CommandToIndex(IDC_BTN_DrawingTools));
	dwStyle |= TBBS_DROPDOWN; // TBSTYLE_DROPDOWN;
	m_toolBar.SetButtonStyle(m_toolBar.CommandToIndex(IDC_BTN_DrawingTools), dwStyle);
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::createStatusBar()
{
	// create rect for status bar 
	CRect rect;
	GetWindowRect(&rect);
	rect.top = rect.bottom - 25;
	
	m_statusBar.Create(WS_CHILD | WS_BORDER | WS_VISIBLE, rect, this, IDC_STATUS_BAR);
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::repositionControlBars()
{
	// We need to resize the dialog to make room for control bars.
	// First, figure out how big the control bars are.
	CRect rcClientStart;
	CRect rcClientNow;
	GetClientRect(rcClientStart);
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST,
				   0, reposQuery, rcClientNow);

	// Now move all the controls so they are in the same relative
	// position within the remaining client area as they would be
	// with no control bars.
	CPoint ptOffset(rcClientNow.left - rcClientStart.left,
					rcClientNow.top - rcClientStart.top);

	CRect  rcChild;
	CWnd* pwndChild = GetWindow(GW_CHILD);
	while (pwndChild)
	{
		pwndChild->GetWindowRect(rcChild);
		ScreenToClient(rcChild);
		rcChild.OffsetRect(ptOffset);
		pwndChild->MoveWindow(rcChild, FALSE);
		pwndChild = pwndChild->GetNextWindow();
	}

	// Adjust the dialog window dimensions
	CRect rcWindow;
	GetWindowRect(rcWindow);
	rcWindow.right += rcClientStart.Width() - rcClientNow.Width();
	rcWindow.bottom += rcClientStart.Height() - rcClientNow.Height();
	MoveWindow(rcWindow, FALSE);

	// And position the control bars
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::initGrid()
{
	if (ma_pDIGWnd.get() == NULL)
	{
		ma_pDIGWnd = auto_ptr<DynamicInputGridWnd>(new DynamicInputGridWnd);
	}

	CWnd* wnd = (CWnd*)this;
	ma_pDIGWnd->SubclassDlgItem(IDC_RW_DIG, wnd);

	ma_pDIGWnd->initDIG();
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::updateWindowPosition()
{
	bool bDIGVisible = IcoMapOptions::sGetInstance().isDIGVisible();
	bool bStatusBarVisible = IcoMapOptions::sGetInstance().isStatusBarVisible();

	//get the rect of the grid
	CRect rectDIG;
	ma_pDIGWnd->GetWindowRect(&rectDIG);
	ScreenToClient(&rectDIG);

	// Get rect of command line window
	CRect rectCommandLine;
	m_commandLine.GetWindowRect(&rectCommandLine);
	ScreenToClient(&rectCommandLine);

	// rect of status bar
	CRect rectStatusBar;
	m_statusBar.GetWindowRect(&rectStatusBar);
	ScreenToClient(&rectStatusBar);
	
	// Get icomap dialog window rect
	CRect dlgCurrentRect, dlgClientRect;		
	GetWindowRect(&dlgCurrentRect);
	GetClientRect(&dlgClientRect);

	// get toolbar rect
	CRect rectToolbar;
	m_toolBar.GetClientRect(&rectToolbar);

	if (!m_bInitialized)
	{
		m_nStatusBarHeight = rectStatusBar.Height();
		m_nToolBarHeight = rectToolbar.Height();

		// both DIG and Status bar are invisible
		m_nDlgHeight1 = m_nMinDlgHeight + m_nCommandLineHeight + m_nToolBarHeight;
		// DIG visible, status bar invisible
		m_nDlgHeight2 = m_nMinDIGHeight + m_nDlgHeight1;
		// DIG invisible, status bar visible
		m_nDlgHeight3 = m_nStatusBarHeight + m_nDlgHeight1;
		// when DIG and status bar are visible
		m_nDlgHeight4 = m_nMinDIGHeight + m_nStatusBarHeight + m_nDlgHeight1;
	}

	if (bDIGVisible)
	{
		// whether or not we are currently tracking the segments
		bool bTrackingOn = ma_pDIGWnd->needTracking() 
						   || (!m_bFinishSketchEnabled && !m_bDeleteSketchEnabled);
		m_staticInfo.ShowWindow(bTrackingOn ? SW_HIDE : SW_SHOW);
		ma_pDIGWnd->ShowWindow(bTrackingOn ? SW_SHOW : SW_HIDE);

		if (bStatusBarVisible)
		{
			m_statusBar.ShowWindow(SW_SHOW);

			if (dlgCurrentRect.Height() < m_nDlgHeight4)
			{
				SetWindowPos(&wndTop, dlgCurrentRect.left, dlgCurrentRect.top, 
					dlgCurrentRect.Width(), m_nDlgHeight4, SWP_NOZORDER);
				
				GetClientRect(&dlgClientRect);
			}
			
			// move status bar right above the border of the dialog
			rectStatusBar.bottom = dlgClientRect.bottom;
			rectStatusBar.right = dlgClientRect.right;
			rectStatusBar.top = rectStatusBar.bottom - m_nStatusBarHeight;
			m_statusBar.MoveWindow(rectStatusBar);
			
			// move command line above the status bar
			rectCommandLine.bottom = rectStatusBar.top - 1;
			rectCommandLine.right = dlgClientRect.right;
			rectCommandLine.top = rectCommandLine.bottom - m_nCommandLineHeight;
			m_commandLine.MoveWindow(rectCommandLine);
		}
		else
		{
			// hide status bar
			m_statusBar.ShowWindow(SW_HIDE);
			
			if (dlgCurrentRect.Height() < m_nDlgHeight2)
			{
				SetWindowPos(&wndTop, dlgCurrentRect.left, dlgCurrentRect.top, 
					dlgCurrentRect.Width(), m_nDlgHeight2, SWP_NOZORDER);
				
				GetClientRect(&dlgClientRect);
			}
			
			// move command line above the border of the dialog
			rectCommandLine.bottom = dlgClientRect.bottom;
			rectCommandLine.right = dlgClientRect.right;
			rectCommandLine.top = rectCommandLine.bottom - m_nCommandLineHeight;
			m_commandLine.MoveWindow(rectCommandLine);
		}

		// move grid above the command line
		rectDIG.bottom = rectCommandLine.top - 2;
		rectDIG.right = dlgClientRect.right;
		ma_pDIGWnd->MoveWindow(rectDIG);

		// resize the grid
		ma_pDIGWnd->doResize();

		// reposition the static info box
		CRect rectInfo;
		m_staticInfo.GetWindowRect(&rectInfo);
		ScreenToClient(&rectInfo);

		// reposition the static message box
		m_staticInfo.SetWindowPos(&wndTopMost, (rectDIG.Width() - rectInfo.Width())/2, rectDIG.top + (rectDIG.Height() - rectInfo.Height())/2, 
			rectInfo.Width(), rectInfo.Height(), SWP_NOZORDER | SWP_NOSIZE);
	}
	else
	{
		// hide grid
		ma_pDIGWnd->ShowWindow(SW_HIDE);
		// hide message text, too
		m_staticInfo.ShowWindow(SW_HIDE);

		// we need to shrink the dialog accordingly
		int nDlgHeight = m_nDlgHeight1;

		// move command line just below the toolbar
		rectCommandLine.top = rectDIG.top;
		rectCommandLine.right = dlgClientRect.right;
		rectCommandLine.bottom = rectCommandLine.top + m_nCommandLineHeight;
		m_commandLine.MoveWindow(rectCommandLine);

		if (bStatusBarVisible)
		{
			nDlgHeight += m_nStatusBarHeight;

			m_statusBar.ShowWindow(SW_SHOW);
			
			// move status bar right below the command line
			rectStatusBar.top = rectCommandLine.bottom;
			rectStatusBar.right = dlgClientRect.right;
			rectStatusBar.bottom = rectStatusBar.top + m_nStatusBarHeight;
			m_statusBar.MoveWindow(rectStatusBar);
		}
		else
		{
			// hide status bar
			m_statusBar.ShowWindow(SW_HIDE);
		}

		SetWindowPos(&wndTop, dlgCurrentRect.left, dlgCurrentRect.top, 
					dlgCurrentRect.Width(), nDlgHeight, SWP_NOZORDER);
	}

	// And position the control bars
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);

	// refresh the highlight frame
	m_ipHighlightWindow->Refresh();
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::init()
{
	// show the splash screen
	{
		TemporaryResourceOverride temporaryResourceOverride(gModuleResource);
		CBitmap bitmap;
		bitmap.LoadBitmap(IDB_BITMAP_SPLASH);
		SplashWindow::sShowSplashScreen(&bitmap);
	}

	// Create the dialog configuration object
	if (!ma_pCfgIcoMapDlg.get())
	{
		// lazy initialization
		ma_pCfgIcoMapDlg = auto_ptr<CfgIcoMapDlg> ( new CfgIcoMapDlg(
			IcoMapOptions::sGetInstancePtr()->getUserPersistenceMgr(),
			"\\IcoMap for ArcGIS\\Dialogs\\IcoMapMainDialog") );
		ASSERT_RESOURCE_ALLOCATION("ELI02208", ma_pCfgIcoMapDlg.get() != NULL);
	}

	// Initialize input process, i.e. set DrawingToolFSM, Input manager. 
	// Note that Command prompt will be set inside input manager, therefore
	// this initialization shall not be done in constructor of IcoMapDlg since it
	// is dependent upon the initialization of command prompt, 
	// which is only initialized inside OnInitDialog().
	initializeInputProcessing();
}
//-------------------------------------------------------------------------------------------------
BOOL IcoMapDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);
	
	try
	{
		CDialog::OnInitDialog();
		
		// disable maximize and size items from system menu
		GetSystemMenu(FALSE)->EnableMenuItem(SC_MAXIMIZE, (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED));
		GetSystemMenu(FALSE)->EnableMenuItem(SC_SIZE, (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED));
		
		//set icon for the icomap dlg
		HICON hIcon = LoadIcon(gModuleResource, MAKEINTRESOURCE(IDI_ICON_ICOMAP));
		SetIcon(hIcon, FALSE);
		
		ghHook = ::SetWindowsHookEx(WH_GETMESSAGE,IcoMapDlgMsgProc, AfxGetInstanceHandle(),::GetCurrentThreadId());
		ASSERT(ghHook);
		
		//create tool tip
		EnableToolTips(true);
		ma_pToolTipCtrl = auto_ptr<CToolTipCtrl>(new CToolTipCtrl);
		ASSERT_RESOURCE_ALLOCATION("ELI02212", ma_pToolTipCtrl.get() != NULL);
		ma_pToolTipCtrl->Create(this,TTS_ALWAYSTIP);
		
		// initialize DIG
		initGrid();

		// initialize window height before inserting toolbar and statusbar
		CRect rectDIG;
		ma_pDIGWnd->GetWindowRect(&rectDIG);
		m_nMinDIGHeight = rectDIG.Height();

		// Get rect of command line window
		CRect rectCommandLine;
		m_commandLine.GetWindowRect(&rectCommandLine);
		m_nCommandLineHeight = rectCommandLine.Height();

		// Get icomap dialog window rect
		CRect dlgCurrentRect;		
		GetWindowRect(&dlgCurrentRect);

		m_nMinDlgHeight = dlgCurrentRect.Height() - m_nMinDIGHeight - m_nCommandLineHeight;

		//load tool bar
		createToolBar();
		
		// load status bar
		createStatusBar();

		repositionControlBars();

		// Initialize buttons and menu bitmaps/item descriptions
		initButtonsAndMenus();
		
		// Now the status text is upon the title bar (not the status bar)
		setStatusText( "" );
		
		// do the necessary initialization steps, such as showing the
		// splash screen, license checking, etc.
		init();

		// initially enable the command prompt, we'll set it to the correct state later
		m_commandLine.EnableWindow(true);
		m_commandLine.SetDIG(ma_pDIGWnd.get());
			
		// Retrieve persistent size and position of dialog
		long lLeft = 0, lTop = 0, lWidth = 0, lHeight = 0;
		ma_pCfgIcoMapDlg->getWindowPos(lLeft, lTop);
		ma_pCfgIcoMapDlg->getWindowSize(lWidth, lHeight);
		
		// Adjust window position based on retrieved settings
		if (lWidth > 10 && lHeight > 10)
		{
			MoveWindow(lLeft, lTop, lWidth, lHeight, TRUE);
		}

		// adjust all control windows to fit in the dialog
		updateWindowPosition();

		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01896")

	return TRUE;  // return TRUE unless you set the focus to a control
				  // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);
	
	CDialog::OnSize(nType, cx, cy);
	
	try
	{
		if (!m_bInitialized)
		{
			return;
		}

		// adjust all control windows to fit in the dialog
		updateWindowPosition();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11973")
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	try
	{
		if (!m_bInitialized)
		{
			return;
		}

		// Minimum width to allow display of buttons
		lpMMI->ptMinTrackSize.x = ICOMAP_DLG_MIN_WIDTH;

		bool bDIGVisible = IcoMapOptions::sGetInstance().isDIGVisible();
		bool bStatusBarVisible = IcoMapOptions::sGetInstance().isStatusBarVisible();

		// if both DIG and status bar are visible
		if (bDIGVisible && bStatusBarVisible)
		{
			lpMMI->ptMinTrackSize.y = m_nDlgHeight4;
		}
		// if DIG is visible and status bar is hiding
		else if (bDIGVisible && !bStatusBarVisible)
		{
			lpMMI->ptMinTrackSize.y = m_nDlgHeight2;
		}
		// if DIG is hiding and status bar is visible
		else if (!bDIGVisible && bStatusBarVisible)
		{
			// if DIG is hidden, dialog's height shall stay the same
			lpMMI->ptMinTrackSize.y = m_nDlgHeight3;
			lpMMI->ptMaxTrackSize.y = m_nDlgHeight3;
		}
		// if both DIG and status bar are hiding
		else
		{
			// if DIG is hidden, dialog's height shall stay the same
			lpMMI->ptMinTrackSize.y = m_nDlgHeight1;
			lpMMI->ptMaxTrackSize.y = m_nDlgHeight1;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11979")
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::startCurveDrawing(void)
{
	updateStateOfCurveToggleButtons();

	if (ma_pDrawingTool.get() != NULL)
	{
		ma_pDrawingTool->startCurveDrawing();	
	}
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::verifyInitialization()
{
	if (!m_bInitialized)
	{
		throw UCLIDException("ELI01382", "IcoMap functionality cannot be used at this time.");
	}
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::enableInput(IInputValidator *pInputValidator, const char *pszPrompt)
{
	if (m_bIcoMapIsActiveInputTarget && m_bFeatureCreationEnabled && !m_bViewEditFeatureSelected)
	{
		// show highlight window in "enabled" color
		m_ipHighlightWindow->UseDefaultColor();
		
		// enable input
		IInputManagerPtr ipInputManager = getInputManager();
		ipInputManager->EnableInput2(pInputValidator, pszPrompt, m_ipIcoMapInputContext);
	}
}
//-------------------------------------------------------------------------------------------------
void IcoMapDlg::disableInput()
{
	if (m_bIcoMapIsActiveInputTarget)
	{
		// show highlight window in "enabled" color
		m_ipHighlightWindow->UseAlternateColor1();
		
		// enable input
		IInputManagerPtr ipInputManager = getInputManager();
		ipInputManager->DisableInput();
	}
}
//-------------------------------------------------------------------------------------------------
