#include "stdafx.h"
#include "resource.h"
#include "SpotRecognitionDlg.h"
#include "ZoomWindowDragOperation.h"
#include "PanWindowDragOperation.h"
#include "DeleteEntitiesDragOperation.h"
#include "SetHighlightHeightDragOperation.h"
#include "SpotRecDlgToolBar.h"
#include "DragOperation.h"
#include "RecognizeTextInWindowDragOperation.h"
#include "RecognizeTextInPolygonDragOperation.h"
#include "OpenSubImgInWindowDragOperation.h"
#include "GDDFileManager.h"
#include "SRIRConstants.h"

#include <PromptDlg.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <StringTokenizer.h>
#include <RegistryPersistenceMgr.h>
#include <MRUList.h>
#include <CursorToolTipCtrl.h>
#include <TemporaryFileName.h>
#include <l_bitmap.h>
#include <Win32Util.h>
#include <ComUtils.h>
#include <RegConstants.h>
#include <MiscLeadUtils.h>
#include <ltWrappr.h>
#include <LicenseMgmt.h>
#include <AfxAppMainWindowRestorer.h>
#include <LeadToolsBitmapFreeer.h>

#include <io.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// static/global variables
const char *gpszTEXT_ATTRIBUTE_NAME = "Text";
COLORREF SpotRecognitionDlg::ms_USED_ENTITY_COLOR = RGB(255, 192, 192);
COLORREF SpotRecognitionDlg::ms_UNUSED_ENTITY_COLOR = RGB(255, 255, 0);
vector<SpotRecognitionDlg *> SpotRecognitionDlg::ms_vecInstances;
map<HWND, HHOOK> SpotRecognitionDlg::ms_mapSRIRToHook;
bool SpotRecognitionDlg::ms_bSizingInProgress = false;
static const long gnNoProgessDisplayPercentageThreshold = 5;

static const long gnZoomToTemporaryHighlightWidthMultiplier = 4;

static const long TEMP_HIGHLIGHT_COLOR = RGB(128, 255, 128);

//-------------------------------------------------------------------------------------------------
SpotRecognitionDlg::SpotRecognitionDlg(IInputEntityManager *pInputEntityManager,
									   bool bOCRIsLicensed, CWnd* pParent)
: CDialog(SpotRecognitionDlg::IDD, pParent), DEFAULT_TOOL(kPan), 
  m_pInputEntityManager(pInputEntityManager),
  m_strLastZoneImageFile(""),
  m_ulLastCreatedEntity(0),
  m_bAlwaysAllowHighlighting(false),
  m_strSubImageFileName(""),
  m_strOriginalImageFileName(""),
  m_ipSubImageZone(NULL),
  m_strFileToBeDeleted(""),
  m_nLastSelectedPTHIndex(0),
  m_bIsCurrentImageAnImagePortion(false),
  m_eCurrentTool(kNone),
  m_ePreviousTool(kNone),
  m_ipEventHandler(NULL),
  m_bAutoOCR(bOCRIsLicensed),
  m_bUserSavingAllowed(true),
  m_bCanOpenFiles(true),
  m_cfgFileReader(this),
  m_eFitToStatus(kFitToPage),
  m_bHighlightsAdjustable(false)
{
	//{{AFX_DATA_INIT(SpotRecognitionDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT

	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	try
	{
		// If PDF support is licensed initialize Leadtools
		// NOTE: no exception is thrown or logged if PDF support is not licensed.
		initPDFSupport();
		
		m_hIcon = AfxGetApp()->LoadIcon(IDI_SPOT_REC_ICON);
		m_bInitialized = false;
		m_bEnableTextSelection = false;
		m_strSelectPrompt = "";
		m_mapPageToViews.clear();

		m_ipTempRasterZone.CreateInstance(__uuidof(RasterZone));
		ASSERT_RESOURCE_ALLOCATION("ELI03392", m_ipTempRasterZone != NULL);

		// create a registry config mgr
		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER,
			gstrREG_ROOT_KEY + "\\InputFunnel\\InputReceivers"));

		// create config mgr to store SRIR stuff
		ma_pSRIRCfgMgr = auto_ptr<ConfigMgrSRIR>(new ConfigMgrSRIR(ma_pUserCfgMgr.get(), "\\SpotRecIR"));

		// Create the MRU List object
		ma_pRecentFiles = auto_ptr<MRUList>(new MRUList(ma_pUserCfgMgr.get(), "\\SpotRecIR\\MRUList", "File_%d", 8));

		// create GDDFileManager
		ma_pGDDFileManager = auto_ptr<GDDFileManager>(new GDDFileManager(this));
		
		// add this instance of the dialog to the list of alive instances
		ms_vecInstances.push_back(this);

		// init zone entity color once for all
		static bool bInit = false;
		if (!bInit)
		{
			ma_pSRIRCfgMgr->getZoneColor( ms_UNUSED_ENTITY_COLOR );
			ma_pSRIRCfgMgr->getUsedZoneColor( ms_USED_ENTITY_COLOR );

			bInit = true;
		}

		m_CurrentView.bIsRightBeforeRotation = false;

		m_nOCROptionsSpanLen = ID_OCR_ENTIRE_IMAGE_02 - ID_OCR_ENTIRE_IMAGE_01;

		m_eCurrSelectionTool = ma_pSRIRCfgMgr->getLastSelectionTool();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03066")
}
//-------------------------------------------------------------------------------------------------
SpotRecognitionDlg::~SpotRecognitionDlg()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// erase this instance from the map/hooks
		map<HWND, HHOOK>::iterator iter2;
		iter2 = ms_mapSRIRToHook.find(m_hWndCopy);
		if (iter2 != ms_mapSRIRToHook.end())
		{
			UnhookWindowsHookEx(iter2->second);
			ms_mapSRIRToHook.erase(iter2);
		}

		if (m_strFileToBeDeleted.empty() && !m_strSubImageFileName.empty())
		{
			m_strFileToBeDeleted = m_strSubImageFileName;
		}
		// delete the temp file if any before the spot rec dectroys
		deleteFile(m_strFileToBeDeleted);

		// erase this instance from the list of static instances
		vector<SpotRecognitionDlg *>::iterator iter = find(ms_vecInstances.begin(), 
			ms_vecInstances.end(), this);
		if (iter != ms_vecInstances.end())
		{
			ms_vecInstances.erase(iter);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03067")	

}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(SpotRecognitionDlg)
	DDX_Control(pDX, IDC_GENERICDISPLAYCTRL, m_UCLIDGenericDisplayCtrl);
	//}}AFX_DATA_MAP
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SpotRecognitionDlg, CDialog)
	//{{AFX_MSG_MAP(SpotRecognitionDlg)
	ON_WM_SIZE()
	ON_COMMAND(IDC_BTN_OpenImage, OnBTNOpenImage)
	ON_COMMAND(IDC_BTN_Save, OnBTNSave)
	ON_COMMAND(IDC_BTN_Pan, OnBTNPan)
	ON_COMMAND(IDC_BTN_ZoomIn, OnBTNZoomIn)
	ON_COMMAND(IDC_BTN_ZoomOut, OnBTNZoomOut)
	ON_COMMAND(IDC_BTN_ZoomWindow, OnBTNZoomWindow)
	ON_COMMAND(IDC_BTN_FitPage, OnBTNFitPage)
	ON_COMMAND(IDC_BTN_FitWidth, OnBTNFitWidth)
	ON_COMMAND(IDC_BTN_FirstPage, OnBTNFirstPage)
	ON_COMMAND(IDC_BTN_LastPage, OnBTNLastPage)
	ON_COMMAND(IDC_BTN_NextPage, OnBTNNextPage)
	ON_COMMAND(IDC_BTN_PreviousPage, OnBTNPreviousPage)
	ON_EN_CHANGE(IDC_BTN_GoToPage, OnChangeGoToPageText)
	ON_EN_KILLFOCUS(IDC_BTN_GoToPage, OnKillFocusGoToPageText)
	ON_COMMAND(IDC_BTN_EditZoneText, OnBTNEditZoneText)
	ON_COMMAND(IDC_BTN_DeleteEntities, OnBTNDeleteEntities)
	ON_COMMAND(IDC_BTN_SELECT_ENTITIES, OnBtnSelectHighlight)
	ON_COMMAND(IDC_BTN_SelectText, OnBTNSelectText)
	ON_COMMAND(IDC_BTN_SetHighlightHeight, OnBTNSetHighlightHeight)
	ON_COMMAND(IDC_BTN_RecognizeTextAndProcess, OnBTNRecognizeTextAndProcess)
	ON_NOTIFY(TBN_DROPDOWN, AFX_IDW_TOOLBAR, OnToolbarDropDown)
	ON_WM_CLOSE()
	ON_COMMAND(IDC_BTN_ZoomPrev, OnBTNZoomPrev)
	ON_COMMAND(IDC_BTN_ZoomNext, OnBTNZoomNext)
	ON_COMMAND(IDC_BTN_RotateLeft, OnBTNRotateLeft)
	ON_COMMAND(IDC_BTN_RotateRight, OnBTNRotateRight)
	ON_WM_CONTEXTMENU()
	ON_COMMAND(ID_MNU_HIGHLIGHTER, OnMnuHighlighter)
	ON_COMMAND(ID_MNU_PAN, OnMnuPan)
	ON_COMMAND(ID_MNU_ZOOMWINDOW, OnMnuZoomwindow)
	ON_COMMAND(ID_MNU_CANCEL, OnMnuCancel)
	ON_COMMAND(IDC_BTN_OPENSUBIMAGE, OnBtnOpenSubImage)
	ON_WM_DESTROY()
	ON_COMMAND(ID_MENU_RECT_SELECTION_TOOL, OnMenuRectSelectionTool)
	ON_COMMAND(ID_MENU_SWIPE_SELECTION_TOOL, OnMenuSwipeSelectionTool)
	ON_COMMAND(IDC_BTN_Print, OnBTNPrint)
	ON_WM_DROPFILES()
	//}}AFX_MSG_MAP
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXT, 0x0000, 0xFFFF, OnToolTipNotify)
	ON_COMMAND_RANGE(ID_OCR_ENTIRE_IMAGE_01, ID_MAX_OCR_OPTION-1, OnPTHMenuItemSelected)
	ON_COMMAND_RANGE(ID_MRU_FILE1, ID_MRU_FILE8, OnSelectMRUPopupMenu)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BEGIN_EVENTSINK_MAP(SpotRecognitionDlg, CDialog)
    //{{AFX_EVENTSINK_MAP(SpotRecognitionDlg)
	ON_EVENT(SpotRecognitionDlg, IDC_GENERICDISPLAYCTRL, -606 /* MouseMove */, OnMouseMoveGenericDisplayCtrl, VTS_I2 VTS_I2 VTS_I4 VTS_I4)
	ON_EVENT(SpotRecognitionDlg, IDC_GENERICDISPLAYCTRL, -605 /* MouseDown */, OnMouseDownGenericDisplayCtrl, VTS_I2 VTS_I2 VTS_I4 VTS_I4)
	ON_EVENT(SpotRecognitionDlg, IDC_GENERICDISPLAYCTRL, -607 /* MouseUp */, OnMouseUpGenericDisplayCtrl, VTS_I2 VTS_I2 VTS_I4 VTS_I4)
	ON_EVENT(SpotRecognitionDlg, IDC_GENERICDISPLAYCTRL, 1 /* EntitySelected */, OnEntitySelectedGenericDisplayCtrl, VTS_I4)
	ON_EVENT(SpotRecognitionDlg, IDC_GENERICDISPLAYCTRL, 4 /* ZoneEntityMoved */, OnZoneEntityMovedGenericDisplayCtrl, VTS_I4)
	ON_EVENT(SpotRecognitionDlg, IDC_GENERICDISPLAYCTRL, 5 /* ZoneEntitiesCreated */, OnZoneEntitiesCreatedGenericDisplayCtrl, VTS_UNKNOWN)
	ON_EVENT(SpotRecognitionDlg, IDC_GENERICDISPLAYCTRL, -601 /* DblClick */, OnDblClickGenericdisplayctrl, VTS_NONE)
	ON_EVENT(SpotRecognitionDlg, IDC_GENERICDISPLAYCTRL, -602 /* KeyDown */, OnKeyDownGenericdisplayctrl, VTS_PI2 VTS_I2)
	//}}AFX_EVENTSINK_MAP
END_EVENTSINK_MAP()
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::addCurrentViewToStack(bool bRightBeforeRotation)
{
	// if current view is the view right before rotation occured, 
	// then do not add this view to the stack
	if (!bRightBeforeRotation || !m_CurrentView.bIsRightBeforeRotation)
	{
		// get current view extents, and add it to the view stack
		PageExtents currentView;
		m_UCLIDGenericDisplayCtrl.getCurrentViewExtents(&currentView.dBottomLeftX, &currentView.dBottomLeftY,
			&currentView.dTopRightX, &currentView.dTopRightY);
		m_mapPageToViews[m_UCLIDGenericDisplayCtrl.getCurrentPageNumber()].addView(currentView);

		m_CurrentView.theView = m_mapPageToViews[m_UCLIDGenericDisplayCtrl.getCurrentPageNumber()].getCurrentView();
		m_CurrentView.bIsRightBeforeRotation = bRightBeforeRotation;
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::createToolBar()
{
	m_apToolBar = auto_ptr<SpotRecDlgToolBar>(new SpotRecDlgToolBar());

	if (m_apToolBar->CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
    | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) )
	{
		m_apToolBar->LoadToolBar(IDR_TOOLBAR);
	}
	
	m_apToolBar->SetBarStyle(m_apToolBar->GetBarStyle() |
		CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC);

	// must set TBSTYLE_TOOLTIPS here in order to get tool tips
	m_apToolBar->ModifyStyle(0, TBSTYLE_TOOLTIPS);

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

	static UINT nButtonIds[] = {
		IDC_BTN_OpenImage,
		IDC_BTN_Save,
		IDC_BTN_Print,
		ID_SEPARATOR,
		IDC_BTN_ZoomWindow,
		IDC_BTN_ZoomIn,
		IDC_BTN_ZoomOut,
		IDC_BTN_ZoomPrev,
		IDC_BTN_ZoomNext,
		IDC_BTN_FitPage,
		IDC_BTN_FitWidth,
		IDC_BTN_Pan,
		ID_SEPARATOR,
		IDC_BTN_SELECT_ENTITIES,
		IDC_BTN_SelectText,
		IDC_BTN_SetHighlightHeight,
		IDC_BTN_EditZoneText,
		IDC_BTN_DeleteEntities,
		ID_SEPARATOR,
		IDC_BTN_RecognizeTextAndProcess,
		ID_SEPARATOR,
		IDC_BTN_OPENSUBIMAGE,
		ID_SEPARATOR,
		IDC_BTN_RotateLeft,
		IDC_BTN_RotateRight,
		ID_SEPARATOR,
		IDC_BTN_FirstPage,
		IDC_BTN_PreviousPage,
		IDC_BTN_GoToPage,
		IDC_BTN_NextPage,
		IDC_BTN_LastPage
	};

	// number of buttons (including sperators) for toolbar buttons
	int nNumButtons = sizeof(nButtonIds)/sizeof(nButtonIds[0]);

	m_apToolBar->SetButtons(nButtonIds, nNumButtons);

	// create the edit control in the toolbar for navigating to a certain page.
	m_apToolBar->createGoToPageEditBox();

	// add a drop down arrow next to the Open button
	m_apToolBar->GetToolBarCtrl().SetExtendedStyle(TBSTYLE_EX_DRAWDDARROWS);
	DWORD dwStyle = m_apToolBar->GetButtonStyle(m_apToolBar->CommandToIndex(IDC_BTN_OpenImage));
	dwStyle |= TBBS_DROPDOWN; 
	m_apToolBar->SetButtonStyle(m_apToolBar->CommandToIndex(IDC_BTN_OpenImage), dwStyle);

	// add a drop down arrow next HighlightTool
	dwStyle = m_apToolBar->GetButtonStyle(m_apToolBar->CommandToIndex(IDC_BTN_SelectText));
	dwStyle |= TBBS_DROPDOWN; 
	m_apToolBar->SetButtonStyle(m_apToolBar->CommandToIndex(IDC_BTN_SelectText), dwStyle);

}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::positionUGDControl(void)
{
	if (m_bInitialized)		// Ensure that the dlg's controls exist before moving them.
	{
		if (!ms_bSizingInProgress)
		{
			RECT rect;
			const int iOffsetTop(25);	// Shift the UCLIDGenericDisplayControl down by this amount to avoid the dialog's buttons.
			GetClientRect(&rect);
			rect.top += iOffsetTop;
			m_UCLIDGenericDisplayCtrl.MoveWindow(&rect);
		}
	}	
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::configureToolBarButtonsAndUGD()
{
	// if the dialog has not yet initialized, then just return
	if (!m_bInitialized)
	{
		return;
	}

	// determine whether image dependent buttons are to be enabled
	BOOL bEnableImageDependentButtons = m_UCLIDGenericDisplayCtrl.getImageName() != "";

	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_ZoomWindow, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_ZoomIn, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_ZoomOut, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_FitPage, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_FitWidth, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_Pan, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_SetHighlightHeight, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_DeleteEntities, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_EditZoneText, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_RotateLeft, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_RotateRight, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_SelectText, bEnableImageDependentButtons);
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_Print, bEnableImageDependentButtons);

	BOOL bEnableSaveButton = bEnableImageDependentButtons;
	// Disable save button if current image file opened is not a persistent file, 
	// ex. portion of the image file
	if (m_bIsCurrentImageAnImagePortion)
	{
		bEnableSaveButton = FALSE;
	}
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_Save, bEnableSaveButton);

	// configure zoom prev/next buttons.
	configureZoomPrevNextButtons();

	// Enable the select entities tool if highlights should be adjustable
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_SELECT_ENTITIES, 
		bEnableImageDependentButtons && m_bHighlightsAdjustable);

	// enable the recognize-paragraph-text button only if at least one
	// paragraph text handler has been specified
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_RecognizeTextAndProcess,
		bEnableImageDependentButtons && m_ipParagraphTextHandlers != NULL &&
		m_ipParagraphTextHandlers->Size() > 0 && m_bAutoOCR);

	// enable the open sub image button if sub image handler has been set
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_OPENSUBIMAGE, 
		bEnableImageDependentButtons && m_ipSubImageHandler != NULL);

	// enable the page navigation buttons only if the currently loaded image is a multi-page
	// image.
	if (bEnableImageDependentButtons &&  m_UCLIDGenericDisplayCtrl.getTotalPages() > 1)
	{
		// enable the goto page button
		m_apToolBar->enableGoToEditBox(TRUE);

		// reset the Go To Page text field in the toolbar
		resetGoToPageText();

		long lTotalPages = m_UCLIDGenericDisplayCtrl.getTotalPages();
		long lCurrentPage = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
		
		// if the current page number is the first page, then only enable the last two
		// navigation buttons.  If the current page is the last page, then only enable
		// the first two navigation buttons.  If the current page is a middle page, then
		// enable all the buttons.
		if (lCurrentPage == 1)
		{
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_FirstPage, FALSE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_PreviousPage, FALSE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_NextPage, TRUE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_LastPage, TRUE);
		}
		else if (lCurrentPage == lTotalPages)
		{
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_FirstPage, TRUE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_PreviousPage, TRUE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_NextPage, FALSE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_LastPage, FALSE);
		}
		else
		{
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_FirstPage, TRUE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_PreviousPage, TRUE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_NextPage, TRUE);
			m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_LastPage, TRUE);
		}
	}
	else
	{
		// an image is not currently loaded.  So, disable the page navigation buttons
		m_strLastGoToPageText = "";
		m_apToolBar->clearGoToPageText();
		m_apToolBar->enableGoToEditBox(FALSE);
		m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_FirstPage, FALSE);
		m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_PreviousPage, FALSE);
		m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_NextPage, FALSE);
		m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_LastPage, FALSE);
	}

	// set the Appropriate selectionToolgraphic on the toolbar
	int nImage;
	UINT nResourceID, nStyle;
	long nIndex = m_apToolBar->CommandToIndex(IDC_BTN_SelectText);
	m_apToolBar->GetButtonInfo(nIndex, nResourceID, nStyle, nImage);
	switch(m_eCurrSelectionTool)
	{
	case kSelectText:
	case kSelectWordText:
		nImage = kSRBmpSwipe;
		break;

	case kSelectRectText:
		nImage = kSRBmpRect;
		break;

	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI11365");
	}

	m_apToolBar->SetButtonInfo(nIndex, nResourceID, nStyle,  nImage);

	// the UCLIDGenericDisplay control should be in the zone selection / creation mode only
	// if the select text button is enabled and if the current tool is kSelectText
	BOOL bEnableZoneCreationInUGD = (m_bEnableTextSelection || m_bAlwaysAllowHighlighting)
									&& bEnableImageDependentButtons 
									&& (m_eCurrentTool == kSelectText || 
									    m_eCurrentTool == kSelectRectText);
	BOOL bEnableZoneSelectionInUGD = bEnableImageDependentButtons 
									&& (m_eCurrentTool == kEditZoneText 
									    || m_eCurrentTool == kSelectHighlight);
	BOOL bEnableZoneAdjustmentInUGD = bEnableImageDependentButtons && m_bHighlightsAdjustable 
									&& m_eCurrentTool == kSelectHighlight;
	m_UCLIDGenericDisplayCtrl.enableZoneEntityCreation(bEnableZoneCreationInUGD);
	m_UCLIDGenericDisplayCtrl.enableZoneEntityAdjustment(bEnableZoneAdjustmentInUGD);
	m_UCLIDGenericDisplayCtrl.enableEntitySelection(bEnableZoneSelectionInUGD);

	if (bEnableZoneCreationInUGD && m_eCurrentTool == kSelectText)
	{
		m_UCLIDGenericDisplayCtrl.setZoneEntityCreationType(1); // kAnyAngleZone
	}
	else if(bEnableZoneCreationInUGD && m_eCurrentTool == kSelectRectText)
	{
		m_UCLIDGenericDisplayCtrl.setZoneEntityCreationType(2); // kRectZone
	}

	// update the status prompt in the UGD to be appropriate
	switch (m_eCurrentTool)
	{
	case kZoomWindow:
		m_UCLIDGenericDisplayCtrl.setStatusBarText("Specify region to zoom into...");
		break;

	case kPan:
		// display the select prompt if applicable
		if (m_bEnableTextSelection)
			m_UCLIDGenericDisplayCtrl.setStatusBarText(m_strSelectPrompt.c_str());
		else
			m_UCLIDGenericDisplayCtrl.setStatusBarText("Use mouse cursor to pan image view...");
		break;

	case kSelectText:
	case kSelectRectText:
		// display the select prompt
		m_UCLIDGenericDisplayCtrl.setStatusBarText(m_strSelectPrompt.c_str());
		break;

	case kInactiveSelectText:
	case kInactiveSelectRectText:
		// display the select prompt
		m_UCLIDGenericDisplayCtrl.setStatusBarText("Select text tool is currently inactive...");
		break;

	case kSetHighlightHeight:
		m_UCLIDGenericDisplayCtrl.setStatusBarText("Specify default highlight height using mouse cursor...");
		break;

	case kDeleteEntities:
		m_UCLIDGenericDisplayCtrl.setStatusBarText("Select highlights to delete...");
		break;

	case kSelectHighlight:
		m_UCLIDGenericDisplayCtrl.setStatusBarText("Select a highlight to move or resize it...");
		break;

	case kEditZoneText:
		m_UCLIDGenericDisplayCtrl.setStatusBarText("Select a highlight to edit its associated text...");
		break;

	case kRecognizeTextInRectRegion:
		m_UCLIDGenericDisplayCtrl.setStatusBarText("Specify rectangular region within which to recognize text...");
		break;

	case kRecognizeTextInPolyRegion:
		m_UCLIDGenericDisplayCtrl.setStatusBarText("Specify polygonal region within which to recognize text...");
		break;

	default:
		m_UCLIDGenericDisplayCtrl.setStatusBarText("");
		break;
	};
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::configureZoomPrevNextButtons()
{
	if (!m_UCLIDGenericDisplayCtrl.getImageName().IsEmpty())
	{
		// enable zoom previous/next only if has previos/next views
		BOOL bEnableZoomPrev = FALSE;
		BOOL bEnableZoomNext = FALSE;
		map<unsigned long, ZoomViewsManager>::iterator iter = m_mapPageToViews.find(m_UCLIDGenericDisplayCtrl.getCurrentPageNumber());
		if (iter != m_mapPageToViews.end())
		{
			if (iter->second.getNumberOfPreviousViews() > 0)
			{
				bEnableZoomPrev = TRUE;
			}
			
			if (iter->second.getNumberOfNextViews() > 0)
			{
				bEnableZoomNext = TRUE;
			}
		}
		m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_ZoomPrev, bEnableZoomPrev);
		m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_ZoomNext, bEnableZoomNext);
	}
	else
	{
		m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_ZoomPrev, FALSE);
		m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_ZoomNext, FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::resetGoToPageText()
{
	if (m_UCLIDGenericDisplayCtrl.getImageName().IsEmpty() == FALSE)
	{
		long lCurrentPage = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
		long lNumPages = m_UCLIDGenericDisplayCtrl.getTotalPages();
		string strCurrentGoToPageText = asString(lCurrentPage) + " of " + asString(lNumPages);
		m_strLastGoToPageText = strCurrentGoToPageText;
		m_apToolBar->setCurrentGoToPageText(strCurrentGoToPageText);
	}
	else
	{
		m_apToolBar->setCurrentGoToPageText("");
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::updateWindowTitle(const string& strFileName) 
{
	string strTitle("");
	if (!strFileName.empty())
	{
		strTitle += strFileName;
		strTitle += " - ";
	}
	
	strTitle += gstrSPOT_RECOGNITION_WINDOW_TITLE;
	SetWindowText(strTitle.c_str());
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::updateCursorHandle(ETool eTool)
{
	static map<ETool, LPCTSTR> mapToolToCursor;
	static bool bInitialized = false;
	if (!bInitialized)
	{
		mapToolToCursor[kNone] = IDC_ARROW; 
		mapToolToCursor[kOpenImage] = IDC_ARROW;
		mapToolToCursor[kSave] = IDC_ARROW;
		mapToolToCursor[kZoomWindow] = MAKEINTRESOURCE(IDC_CUR_ZOOMWINDOW);
		mapToolToCursor[kZoomIn] = IDC_ARROW; 
		mapToolToCursor[kZoomOut] = IDC_ARROW; 
		mapToolToCursor[kFitPage] = IDC_ARROW; 
		mapToolToCursor[kFitWidth] = IDC_ARROW; 
		mapToolToCursor[kPan] = MAKEINTRESOURCE(IDC_CUR_PAN);
		mapToolToCursor[kSelectText] = MAKEINTRESOURCE(IDC_CUR_SELECTTEXT);
		mapToolToCursor[kSelectRectText] = MAKEINTRESOURCE(IDC_CUR_SELECTRECTTEXT);
		mapToolToCursor[kInactiveSelectText] = MAKEINTRESOURCE(IDC_CUR_NOSELECT);
		mapToolToCursor[kInactiveSelectRectText] = MAKEINTRESOURCE(IDC_CUR_NOSELECTRECT);
		mapToolToCursor[kSetHighlightHeight] = MAKEINTRESOURCE(IDC_CUR_TEXTHEIGHT);
		mapToolToCursor[kEditZoneText] = MAKEINTRESOURCE(IDC_CUR_EDITTEXT);
		mapToolToCursor[kDeleteEntities] = MAKEINTRESOURCE(IDC_CUR_ERASER);
		mapToolToCursor[kRecognizeTextInRectRegion] = MAKEINTRESOURCE(IDC_CUR_RECTEXT_RECT);
		mapToolToCursor[kRecognizeTextInPolyRegion] = MAKEINTRESOURCE(IDC_CUR_RECTEXT_POLY);
		mapToolToCursor[kOpenSubImgInWindow] = MAKEINTRESOURCE(IDC_CUR_OPENSUBIMAGE);
		mapToolToCursor[kSelectHighlight] = MAKEINTRESOURCE(IDC_ARROW);
		bInitialized = true;
	}

	if (mapToolToCursor.find(eTool) == mapToolToCursor.end())
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI02386");
	}

	HINSTANCE hInstance = NULL;
	switch (eTool)
	{
	case kZoomWindow:
	case kPan:
	case kSelectText:
	case kSelectRectText:
	case kInactiveSelectText:
	case kInactiveSelectRectText:
	case kSetHighlightHeight:
	case kEditZoneText:
	case kDeleteEntities:
	case kRecognizeTextInRectRegion:
	case kRecognizeTextInPolyRegion:
	case kOpenSubImgInWindow:
		hInstance = _Module.m_hInstResource;
		break;
	}

	HCURSOR hCursor = ::LoadCursor(hInstance, mapToolToCursor[eTool]);
	m_UCLIDGenericDisplayCtrl.setCursorHandle((long*)hCursor);
}
//-------------------------------------------------------------------------------------------------
ETool SpotRecognitionDlg::getCurrentTool()
{
	return m_eCurrentTool;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setCurrentTool(ETool eTool)
{
	// never store one-time tool as previous tool
	if (m_eCurrentTool != kDeleteEntities 
		&& m_eCurrentTool != kRecognizeTextInRectRegion
		&& m_eCurrentTool != kRecognizeTextInPolyRegion
		&& m_eCurrentTool != kOpenSubImgInWindow)
	{
		// store the current tool for later use
		m_ePreviousTool = m_eCurrentTool;
	}

	// if eTool is Fit To Page
	if (eTool == kFitPage)
	{
		// Set the m_eFitToStatus to kFitToPage
		m_eFitToStatus = kFitToPage;
		// Press the fit to page button and set image to fit to page status
		setFitToStatus();
		return;
	}
	// if eTool is Fit To Width
	if (eTool == kFitWidth)
	{
		// Set the m_eFitToStatus to kFitToPage
		m_eFitToStatus = kFitToWidth;
		// Press the fit to width button and set image to fit to width status
		setFitToStatus();
		return;
	}

	if (eTool == kZoomIn || eTool == kZoomOut)
	{
		// Set the m_eFitToStatus to kFitToPage
		m_eFitToStatus = kFitToNothing;
		// Unpress the fit-to buttons on the toolbar by calling setFitToStatus()
		setFitToButtonsStatus();
	}

	// store the current tool identifier
	m_eCurrentTool = eTool;

	// reset the cursor to the one that's designated for the current tool
	// if for some reason the cursor shape could not be reset, 
	// just ignore the problem.
	try
	{
		updateCursorHandle(eTool);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI02909");
	
	// Untoggle all the toolbar buttons
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_ZoomWindow, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_ZoomIn, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_ZoomOut, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_Pan, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SelectText, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SetHighlightHeight, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_DeleteEntities, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SELECT_ENTITIES, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_EditZoneText, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_RecognizeTextAndProcess, FALSE);
	m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_OPENSUBIMAGE, FALSE);
	
	// if any drag operation object is currently in memory, release it
	releaseCurrentDragOperationMemory();
	
	switch (eTool)
	{
	case kZoomWindow:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_ZoomWindow, TRUE);
			// initialize a new zoom-window operation
			initDragOperation(new ZoomWindowDragOperation(m_UCLIDGenericDisplayCtrl));
		}
		break;
		
	case kZoomIn:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_ZoomIn, TRUE);

			// lStatus is used to get the fit-to status after zooming in
			// If the return lStatus is 1, then the image after zooming is in fit to page status
			// If the return lSatatus is 0, then the image after zooming is in fit to width status
			long lStatus = -1;
			m_UCLIDGenericDisplayCtrl.zoomIn(&lStatus);

			if (lStatus == 1)
			{
				// Press the fit to page button
				m_eFitToStatus = kFitToPage;
				setFitToButtonsStatus();
			}
			else if (lStatus == 0)
			{
				// Press the fit to width button
				m_eFitToStatus = kFitToWidth;
				setFitToButtonsStatus();
			}
			// set the current tool to the default tool after a zoom out operation
			setCurrentTool(m_ePreviousTool);
		}
		break;
		
	case kZoomOut:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_ZoomOut, TRUE);

			// lStatus is used to get the fit-to status after zooming out
			// If the return lStatus is 1, then the image after zooming is in fit to page status
			// If the return lSatatus is 0, then the image after zooming is in fit to width status
			long lStatus = -1;
			m_UCLIDGenericDisplayCtrl.zoomOut(&lStatus);

			if (lStatus == 1)
			{
				// Press the fit to page button
				m_eFitToStatus = kFitToPage;
				setFitToButtonsStatus();
			}
			else if (lStatus == 0)
			{
				// Press the fit to width button
				m_eFitToStatus = kFitToWidth;
				setFitToButtonsStatus();
			}
			// set the current tool to the default tool after a zoom out operation
			setCurrentTool(m_ePreviousTool);
		}
		break;

	case kPan:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_Pan, TRUE);
			// initialize a new pan window operation
			initDragOperation(new PanWindowDragOperation(m_UCLIDGenericDisplayCtrl));
		}
		break;
		
	case kSelectText:
		{
			m_eCurrSelectionTool = kSelectText;
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SelectText, TRUE);
			ma_pSRIRCfgMgr->setLastSelectionTool(m_eCurrSelectionTool);
		}
		break;

	case kSelectRectText:
		{
			m_eCurrSelectionTool = kSelectRectText;
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SelectText, TRUE);
			ma_pSRIRCfgMgr->setLastSelectionTool(m_eCurrSelectionTool);
		}
		break;

	case kInactiveSelectText:
		// The highlighter tool shall still stay enabled and pressed down
		// when input is disabled.
		m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SelectText, TRUE);
		break;
	case kInactiveSelectRectText:
		// The highlighter tool shall still stay enabled and pressed down
		// when input is disabled.
		m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SelectText, TRUE);
		break;
	case kSetHighlightHeight:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SetHighlightHeight, TRUE);
			// initialize a set-highlight-height window operation
			initDragOperation(new SetHighlightHeightDragOperation(m_UCLIDGenericDisplayCtrl));
			if (m_bEnableTextSelection || m_bAlwaysAllowHighlighting)
			{
				m_eCurrentTool = kSelectText;
			}
			else
			{
				m_eCurrentTool = kInactiveSelectText;
			}
		}
		break;
		
	case kDeleteEntities:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_DeleteEntities, TRUE);
			// initialize a delete-entities drag operation
			initDragOperation(new DeleteEntitiesDragOperation(m_UCLIDGenericDisplayCtrl));
		}
		break;

	case kSelectHighlight:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_SELECT_ENTITIES, TRUE);
			m_UCLIDGenericDisplayCtrl.enableEntitySelection(TRUE);
		}
		break;
		
	case kEditZoneText:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_EditZoneText, TRUE);
			// enable zone entity selection and disable entity creation
			m_UCLIDGenericDisplayCtrl.enableEntitySelection(TRUE);
		}
		break;
		
	case kRecognizeTextInRectRegion:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_RecognizeTextAndProcess, TRUE);

			// ensure that an OCR engine object has been associated with this window
			if (m_ipOCREngine == NULL)
			{
				// reset the tool to be the previous tool, and throw an exception
				m_eCurrentTool = m_ePreviousTool;

				// throw exception 
				throw UCLIDException("ELI03592", "Missing OCR engine.");
			}

			// now that we have ensured that an OCR engine object is associated with this
			// window, go ahead and initialize the drag operation
			initDragOperation(new RecognizeTextInWindowDragOperation(
				m_UCLIDGenericDisplayCtrl, this));			
		}
		break;

	case kRecognizeTextInPolyRegion:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_RecognizeTextAndProcess, TRUE);

			// ensure that an OCR engine object has been associated with this window
			if (m_ipOCREngine == NULL)
			{
				// reset the tool to be the previous tool, and throw an exception
				m_eCurrentTool = m_ePreviousTool;

				// throw exception 
				throw UCLIDException("ELI04814", "Missing OCR engine.");
			}

			// now that we have ensured that an OCR engine object is associated with this
			// window, go ahead and initialize the drag operation
			DragOperation* pPolygonDrag = new RecognizeTextInPolygonDragOperation(
				m_UCLIDGenericDisplayCtrl, this, m_ePreviousTool);
			initDragOperation(pPolygonDrag);			
		}
		break;

	case kOpenSubImgInWindow:
		{
			m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_OPENSUBIMAGE, TRUE);

			UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr ipSRIR;
			ipSRIR = m_pInputEntityManager;
			initDragOperation(new OpenSubImgInWindowDragOperation(
				m_UCLIDGenericDisplayCtrl, ipSRIR)); 
		}
		break;

	};

	// configure the buttons and the UGD
	configureToolBarButtonsAndUGD();
}
//-------------------------------------------------------------------------------------------------
string SpotRecognitionDlg::getImageFileName()
{
	// if portion of image is currently open, it shall return original image file
	if (m_bIsCurrentImageAnImagePortion && !m_strOriginalImageFileName.empty())
	{
		return m_strOriginalImageFileName;
	}

	return string((LPCTSTR) m_UCLIDGenericDisplayCtrl.getImageName());
}
//-------------------------------------------------------------------------------------------------
string SpotRecognitionDlg::getGDDFileName()
{
	string strResult = "";
	
	if (getImageFileName() != "")
		strResult = ma_pGDDFileManager->getCurrentGDDFileName();

	return strResult;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::openFile(const string& strFileName)
{
	const string strMAIN_ERROR_MSG = "Unable to load requested image.  The referenced image does not exist, is not readable, or is not in a format that the application understands.";

	// string to hold the file name, if strFileName is a uss file, then strFileToOpen will hold the
	// path to the associated tiff image and this is the file that will be opened
	string strFileToOpenName = strFileName;
	try
	{	
		// check to see if this is a uss file 
		if ( getExtensionFromFullPath(strFileName, true) == ".uss" )
		{
			// since this is a uss file, we need to try to get the associated
			// tiff image (P13 #4416)
			// first get a spatial string
			ISpatialStringPtr ipSpatial(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI16813", ipSpatial != NULL);

			// load the spatial string from the file
			ipSpatial->LoadFrom(get_bstr_t(strFileName), VARIANT_FALSE);

			// get the associated file name and validate its existence
			strFileToOpenName = asString(ipSpatial->SourceDocName);
			validateFileOrFolderExistence(strFileToOpenName);
		}

		try
		{
			// Make sure that if the file being opened is a pdf file that PDF support is licensed
			LicenseManagement::sGetInstance().verifyFileTypeLicensed( strFileToOpenName );
			// clear content of the page number edit box
			m_apToolBar->setCurrentGoToPageText("");

			// delete any temporary highlights
			deleteTemporaryHighlight();

			// reset the m_strSubImageFileName and m_ipSubImageZone 
			// if the file to open is not the temp image file
			if (_strcmpi(strFileToOpenName.c_str(), m_strSubImageFileName.c_str()) != 0)
			{
				m_strFileToBeDeleted = m_strSubImageFileName;
				m_strSubImageFileName = "";
				m_bIsCurrentImageAnImagePortion = false;
				m_ipSubImageZone = NULL;
			}

			// retrieve the name of the file to load, and load the file in the
			// GenericDisplay control depending upon whether it is a GDD file or
			// an image file
			if (strFileToOpenName != "")
			{
				if (GDDFileManager::sIsGDDFile(strFileToOpenName))
				{
					// ensure that the image pointed to by the GDD file actually
					// exists
					string strImageName = GDDFileManager::sGetImageNameFromGDDFile(strFileToOpenName);

					if (!fileExistsAndIsReadable(strImageName))
					{
						if (isValidFile(strImageName))
						{
							UCLIDException ue("ELI02347", "The referenced image exists, but is not readable!");
							ue.addDebugInfo("strFileToOpen", strFileToOpenName);
							ue.addDebugInfo("strImageName", strImageName);
							ue.addWin32ErrorInfo();
							throw ue;
						}
						else
						{
							UCLIDException ue("ELI02348", "The referenced image does not exist!");
							ue.addDebugInfo("strFileToOpen", strFileToOpenName);
							ue.addDebugInfo("strImageName", strImageName);
							ue.addWin32ErrorInfo();
							throw ue;
						}
					}
					
					// The following call must be made here in case
					// the file is invalid, we don't want to clear
					// out any currently opened image file.
					// clear the current image in the ocx control
					m_UCLIDGenericDisplayCtrl.clear();

					// open the GDD file using GDDFileManager
					ma_pGDDFileManager->openGDDFile(strFileToOpenName);
				}
				else
				{
					// before opening the file, check to see if the file is in the right format
					// Get initialized FILEINFO struct
					FILEINFO FileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

					// Get the file information 
					L_INT nRetCode = L_FileInfo(const_cast<char*>(strFileToOpenName.c_str()), 
						&FileInfo, sizeof(FILEINFO), 0, NULL);
					throwExceptionIfNotSuccess(nRetCode, "ELI03089", 
						"Unable to open image file.", strFileToOpenName);

					// The following call must be made here in case
					// the file is invalid, we don't want to clear
					// out any currently opened image file.
					// clear the current image in the ocx control
					m_UCLIDGenericDisplayCtrl.clear();

					m_UCLIDGenericDisplayCtrl.setImage(strFileToOpenName.c_str());
				}

				// Notify observers of the SpotRecognitionDlg that we opened a file
				if (m_ipSRWEventHandler != NULL)
				{
					// the file we wanted to open is strFileName, what we actually
					// opened is strFileToOpenName (which is the same, unless strFileName
					// was a uss file)
					m_ipSRWEventHandler->NotifyFileOpened( get_bstr_t( strFileToOpenName ) );
				}

				// get the image file that is loaded in the m_UCLIDGenericDisplayCtrl and
				// load the same image in the text recognition server
				// Note: this is the actual image file that's loaded in UGD no matter it
				// is an original image file or a temporary image file created from 
				// a certain image file.
				string strImageName(m_UCLIDGenericDisplayCtrl.getImageName());

				// Store the path information
				// we use strFileName since this is the file the user told us to open, 
				// we may have opened a tiff file from another directory if the user gave 
				// us a uss file
				string	strPathName = getDirectoryFromFullPath(strFileName);

				// set last file open directory if this opened file is not
				// a temporary image file
				if (!m_bIsCurrentImageAnImagePortion && m_strSubImageFileName.empty())
				{
					ma_pSRIRCfgMgr->setLastFileOpenDirectory( strPathName.c_str() );
				}

				// Get the fit-to status from registry
				bool bFitToPage = ma_pSRIRCfgMgr->getFitToStatus();

				// reset the control to be in zoom window mode
				setCurrentTool(kZoomWindow);
				// Set the image to the selected fit-to status
				setCurrentTool(bFitToPage ? kFitPage : kFitWidth);
			}

			// congfigure the enabled/disabled state of the toolbar buttons
			configureToolBarButtonsAndUGD();

			// update the caption of the window
			if (strFileToOpenName != "")
			{
				string strTitle(strFileToOpenName);

				if (m_bIsCurrentImageAnImagePortion)
				{
					// show original source name on the title bar 
					// if current image opened is a portion of image
					strTitle = m_strOriginalImageFileName;
				}

				string strShortFileName = getFileNameFromFullPath(strTitle);

				if (m_bIsCurrentImageAnImagePortion)
				{
					strShortFileName += " - Portion";
				}

				updateWindowTitle(strShortFileName);
			}
			else
			{
				setCurrentTool(kNone);
				// clear the current image in the ocx control
				m_UCLIDGenericDisplayCtrl.clear();
				// maps must be cleared if a new file is opened
				m_mapPageToViews.clear();
				updateWindowTitle("");

				resetGoToPageText();

				configureToolBarButtonsAndUGD();
				// since there's no file to be opened, return here.
				return;
			}

			// if only current image is not a portion of an image can current
			// file be added to the MRU list
			if (!m_bIsCurrentImageAnImagePortion && m_strSubImageFileName.empty())
			{
				addFileToMRUList(strFileToOpenName);
			}

			// maps must be cleared if a new file is opened
			m_mapPageToViews.clear();
			// get current view extents, and add it to the view stack
			addCurrentViewToStack();

			// delete temp file
			deleteFile(m_strFileToBeDeleted);
		}
		catch (COleDispatchException* pEx)
		{
			UCLIDException ue("ELI02349", strMAIN_ERROR_MSG);
			ue.addDebugInfo("strFileToOpenName", strFileToOpenName);
			ue.addDebugInfo("m_strDescription", (LPCTSTR) pEx->m_strDescription);
			pEx->Delete();
			ue.addDebugInfo("m_wCode", pEx->m_wCode);
			throw ue;
		}
		catch (COleDispatchException& e)
		{
			UCLIDException ue("ELI19276", strMAIN_ERROR_MSG);
			ue.addDebugInfo("strFileToOpenName", strFileToOpenName);
			ue.addDebugInfo("m_strDescription", (LPCTSTR) e.m_strDescription);
			ue.addDebugInfo("m_wCode", e.m_wCode);
			throw ue;
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter("ELI02350", strMAIN_ERROR_MSG, ue);
			uexOuter.addDebugInfo("strFileToOpenName", strFileToOpenName);
			throw uexOuter;
		}
		catch (...)
		{
			UCLIDException ue("ELI02351", strMAIN_ERROR_MSG);
			ue.addDebugInfo("strFileToOpenName", strFileToOpenName);
			throw ue;
		}
	}
	catch (...)
	{
		removeFileFromMRUList(strFileToOpenName);

		// since there's a problem with loading the image, leave the
		// control in a known state - clear its contents
		// P13 #4621 - JDS
		// image window may be in a bad state if it almost loaded the image (i.e. L_FileInfo
		// did not fail, but the image is corrupt and so cannot be loaded), if so calling
		// clear will throw a COleDispatchException
		try
		{
			m_UCLIDGenericDisplayCtrl.clear();
		}
		catch(COleDispatchException* pEx)
		{
			// P13 #4621 - JDS
			// the error message is already encapsulated in the outer exception, it is
			// safe to eat this exception

			// free the memory in the exception
			pEx->Delete();
		}

		// maps must be cleared if a new file is opened
		m_mapPageToViews.clear();

		deleteFile(m_strFileToBeDeleted);

		// reset toolbar buttons
		configureToolBarButtonsAndUGD();

		updateWindowTitle("");

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::openImagePortion(const string& strOriginalImageFileName, 
										  IRasterZone *pImagePortionInfo, double dRotationAngle)
{
	// the image portion is not a persistent image source.
	m_bIsCurrentImageAnImagePortion = true;
	m_strOriginalImageFileName = strOriginalImageFileName;
	m_ipSubImageZone = pImagePortionInfo;

	// create the temp sub image file
	m_strSubImageFileName = createSubImage(strOriginalImageFileName, pImagePortionInfo, 
		dRotationAngle);
	
	// open the sub image file
	openFile(m_strSubImageFileName);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::saveAs(const std::string& strFileName)
{
	// use GDDFileManager to save the gdd file
	ma_pGDDFileManager->saveAs(strFileName);
	
	// add current saved gdd file to the MRU list
	addFileToMRUList(strFileName);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::save()
{
	// use GDDFileManager to save the gdd file
	ma_pGDDFileManager->saveAs(ma_pGDDFileManager->getCurrentGDDFileName());
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::initDragOperation(DragOperation *pNewDragOperation)
{
	// release memory of current drag operation, if applicable
	releaseCurrentDragOperationMemory();

	// store a pointer to the new drag operation, and initialize it.
	m_apCurrentDragOperation.reset(pNewDragOperation);
	m_apCurrentDragOperation->init();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::releaseCurrentDragOperationMemory()
{
	m_apCurrentDragOperation.reset(NULL);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::gotoPage(long lPageNum)
{
	// if we are navigating to the specified page number for the first time, show the whold page
	m_UCLIDGenericDisplayCtrl.setCurrentPageNumber(lPageNum);
		
	// if the new page to be opened doesn't have a current view,
	// then add the current view to the stack
	if (!m_mapPageToViews[lPageNum].hasCurrentView())
	{
		// Set the image to fit to page status if it is not in fit to
		// page or fit to width status
		// TODO, Always set to fit to page the first time??
		if (m_eFitToStatus == kFitToNothing)
		{
			m_eFitToStatus = kFitToPage;
		}
		setFitToStatus();
		addCurrentViewToStack();
	}
	else
	{
		PageExtents pageExtents = m_mapPageToViews[lPageNum].getCurrentView();

		// lStatus is used to get the fit-to status after zooming window
		// If the return lStatus is 1, then the image after zooming is in fit to page status
		// If the return lSatatus is 0, then the image after zooming is in fit to width status
		long lStatus = -1;
		
		// zoom into the same view of the page before we navigated out of it the last time
		m_UCLIDGenericDisplayCtrl.zoomWindow(pageExtents.dBottomLeftX, pageExtents.dBottomLeftY, 
			pageExtents.dTopRightX, pageExtents.dTopRightY, &lStatus);

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
	}
	
	// configure the toolbar buttons, etc.
	configureToolBarButtonsAndUGD();
}
//-------------------------------------------------------------------------------------------------
string SpotRecognitionDlg::getZoneEntityText(long lEntityID)
{
	string strZoneText = "";
	
	try
	{
		// it may appear strange that we are catching, rethrowing, and then catching
		// and ignoring.  the issue is that the exception that is thrown may
		// contain allocated memory and thus needs to be cleaned up.  the catch
		// and rethrow macro contains code to clean up any exceptions that contain
		// allocated memory, once the exception is rethrown it is then safe
		// to just ignore it.
		// [p16 #2845]
		try
		{
			strZoneText = m_UCLIDGenericDisplayCtrl.getEntityAttributeValue(lEntityID, 
				gpszTEXT_ATTRIBUTE_NAME);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20377");
	}
	catch(...)
	{
		// ignore any errors, as not all zones have text associated with them.
	}

	return strZoneText;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setZoneEntityText(long lEntityID, const string& strNewText)
{
	// TODO: add setEntityAttributeValue() method to the OCX
	try
	{
		m_UCLIDGenericDisplayCtrl.addEntityAttribute(lEntityID, 
			gpszTEXT_ATTRIBUTE_NAME, strNewText.c_str());
	}
	catch (...)
	{
		// we may be getting this exception because the attribute already
		// exists - so modify it
		m_UCLIDGenericDisplayCtrl.modifyEntityAttribute(lEntityID, 
			gpszTEXT_ATTRIBUTE_NAME, strNewText.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::removeCommasFromTextAttributes() 
{
	// first do a query and find all the entities
	string strEntities = (LPCTSTR) m_UCLIDGenericDisplayCtrl.queryEntities("");

	// use the string tokenizer and tokenize the entities
	vector<string> vecTokens;
	StringTokenizer tokenizer;
	tokenizer.parse(strEntities, vecTokens);
	unsigned long ulNumZoneEntities = 0;
	vector<unsigned long> vecEntityIDs;
	long ulEntityID;
	
	unsigned int ui;
	for (ui = 0; ui < vecTokens.size(); ui++)
	{
		string strTemp = vecTokens[ui];
		if (!strTemp.empty())
		{
			ulEntityID = asLong(strTemp);
			vecEntityIDs.push_back(ulEntityID);
		}
	}

	for (ui = 0; ui < vecEntityIDs.size(); ui++)
	{			
		unsigned long ulEntityID = vecEntityIDs[ui];
		// determine the type of the entity
		// if the entity is of Zone type, then write its information
		// to the test script file.
		if (m_UCLIDGenericDisplayCtrl.getEntityType(ulEntityID) == "ZONE")
		{
			string strText = getZoneEntityText(ulEntityID);
			replaceVariable(strText, ",", " ");
			setZoneEntityText(ulEntityID, strText);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setEventHandler(IIREventHandler* ipEventHandler)
{
	m_ipEventHandler = ipEventHandler;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::fireOnInputReceived(const vector<unsigned long> &vecZoneIDs)
{
	// Must have zone entity ids to fire input received
	ASSERT_ARGUMENT("ELI23800", vecZoneIDs.size() > 0);

	// Iterate through each zone entity
	unsigned long ulZoneID = vecZoneIDs[0];
	string strInputID = asString(ulZoneID);
	string strText = getZoneEntityText(ulZoneID);
	for (unsigned int i = 1; i < vecZoneIDs.size(); i++)
	{
		ulZoneID = vecZoneIDs[i];

		// The input entity ID is a comma delimited list of zone entity ids.
		strInputID += "," + asString(ulZoneID);

		// The input entity text is the zone text separated by line breaks.
		strText += "\r\n" + getZoneEntityText(ulZoneID);
	}

	// create input entity object
	IInputEntityPtr ipInputEntity(CLSID_InputEntity);
	ASSERT_RESOURCE_ALLOCATION("ELI23767", ipInputEntity != NULL);
	ipInputEntity->InitInputEntity(m_pInputEntityManager, strInputID.c_str());

	// create text input object
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI23768", ipTextInput != NULL);
	ipTextInput->InitTextInput(ipInputEntity, strText.c_str());

	// fire the event
	if (m_ipEventHandler)
	{
		// temporarily set the parent wnd of the input correction
		// dlg to this dialog. We'll set it back once the following
		// call is finished
		IInputManagerPtr ipInputManager(m_ipEventHandler);
		if (ipInputManager)
		{
			long originalParentWnd = ipInputManager->ParentWndHandle;
			try
			{
				ipInputManager->ParentWndHandle = (long)m_hWnd;
				m_ipEventHandler->NotifyInputReceived( ipTextInput );
				ipInputManager->ParentWndHandle = originalParentWnd;
			}
			catch(...)
			{
				ipInputManager->ParentWndHandle = originalParentWnd;
				throw;
			}
		}
		else
		{
			// if the InputManager is not the event handler,
			// then just notify the event handler about the input received,
			// and don't worry about the input correction dialog box.
			m_ipEventHandler->NotifyInputReceived( ipTextInput );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::enableInput(BSTR bstrInputType, BSTR bstrPrompt)
{
	// store the parameters for later use
	m_strSelectPrompt = asString( bstrPrompt );
	m_strInputType = asString( bstrInputType );

	// enable text selection and re-configure toolbar buttons, etc.
	m_bEnableTextSelection = true;
	if (m_eCurrentTool == kInactiveSelectText || m_eCurrentTool == kSelectText)
	{
		// set cursor to active highlighter
		setCurrentTool(kSelectText);
	}
	if (m_eCurrentTool == kInactiveSelectRectText || m_eCurrentTool == kSelectRectText)
	{
		// set cursor to active highlighter
		setCurrentTool(kSelectRectText);
	}
	configureToolBarButtonsAndUGD();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::disableInput()
{
	// the highlighter tool shall stay enabled even though the input is disabled
	m_bEnableTextSelection = false;
	if (!m_bAlwaysAllowHighlighting && m_eCurrentTool == kSelectText)
	{
		// set cursor to inactive highlighter
		setCurrentTool(kInactiveSelectText);
	}
	if (!m_bAlwaysAllowHighlighting && m_eCurrentTool == kSelectRectText)
	{
		// set cursor to inactive highlighter
		setCurrentTool(kInactiveSelectRectText);
	}
	configureToolBarButtonsAndUGD();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setOCRFilter(IOCRFilter *pFilter)
{
	m_ipOCRFilter = pFilter;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setOCREngine(IOCREngine *pEngine)
{
	m_ipOCREngine = pEngine;
}
//-------------------------------------------------------------------------------------------------
bool SpotRecognitionDlg::inputIsEnabled() const
{
	return m_bEnableTextSelection;
}
//-------------------------------------------------------------------------------------------------
bool SpotRecognitionDlg::isModified()
{
	// if current opened image is a portion of image, since it
	// can't be saved at this time, then no modification flag
	// shall be set.
	return !m_bIsCurrentImageAnImagePortion && m_UCLIDGenericDisplayCtrl.documentIsModified() == TRUE;
}
//-------------------------------------------------------------------------------------------------
long SpotRecognitionDlg::getCurrentPageNumber()
{
	return m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setCurrentPageNumber(long lPageNumber)
{
	long lMaxPages = getTotalPages();

	if (lPageNumber >0 && lPageNumber <= lMaxPages)
	{
		gotoPage(lPageNumber);
	}
	else
	{
		string strMsg = "You have specified an invalid page number.  ";
		strMsg += "Valid page numbers range from 1 to ";
		strMsg += asString(lMaxPages);
		strMsg += ".";
		UCLIDException ue("ELI02911", strMsg);
		ue.addDebugInfo("Page number", lPageNumber);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
long SpotRecognitionDlg::getTotalPages()
{
	return m_UCLIDGenericDisplayCtrl.getTotalPages();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::clear()
{
	openFile("");
}
//-------------------------------------------------------------------------------------------------
bool SpotRecognitionDlg::isMarkedAsUsed(long lEntityID)
{
	// consider an entity to be marked as used as long as its color is not the color
	// of an unused entity.
	COLORREF entityColor = m_UCLIDGenericDisplayCtrl.getEntityColor(lEntityID);
	if (entityColor == ms_UNUSED_ENTITY_COLOR)
		return false;
	else 
		return true;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::markAsUsed(long lEntityID, bool bMarkAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	if (bMarkAsUsed)
	{
		m_UCLIDGenericDisplayCtrl.setEntityColor(lEntityID, ms_USED_ENTITY_COLOR);

		// if m_ulLastCreatedEntity is not 0, then a zone entity as recenty been created
		// and the application has marked it as used.  This is the time to provide visual feedback
		// with the recognized text and to reset the m_ulLastCreatedEntity variable
		if (m_ulLastCreatedEntity != 0)
		{
			// get the text associated with this entity
			string strText = getZoneEntityText(lEntityID);

			// get the centerpoint of the zone
			long nStartPosX, nStartPosY, nEndPosX, nEndPosY, nHeight;
			m_UCLIDGenericDisplayCtrl.getZoneEntityParameters(lEntityID, &nStartPosX, &nStartPosY,
				&nEndPosX, &nEndPosY, &nHeight);
			POINT p;
			p.x = (nStartPosX + nEndPosX) / 2;
			p.y = (nStartPosY + nEndPosY) / 2;
			long nPageNum = asLong((LPCTSTR) 
				m_UCLIDGenericDisplayCtrl.getEntityAttributeValue(lEntityID, "Page"));
			double dWorldX, dWorldY;
			m_UCLIDGenericDisplayCtrl.convertImagePixelToWorldCoords(p.x, p.y, &dWorldX, &dWorldY, 
				nPageNum);
			m_UCLIDGenericDisplayCtrl.convertWorldToClientWindowPixelCoords(dWorldX, dWorldY, &p.x, 
			&p.y, nPageNum);
			m_apCursorToolTipCtrl->updateTipText(strText, p.x, p.y);
			
			// reset the m_ulLastCreatedEntity variable
			m_ulLastCreatedEntity = 0;
		}

	}
	else
	{
		m_UCLIDGenericDisplayCtrl.setEntityColor(lEntityID, ms_UNUSED_ENTITY_COLOR);
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::createModeless(CWnd* pParentWnd) 
{
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::Create(IDD, pParentWnd);

	// Removed to fix problems associated with [p16: 2592 and 2518].  It appears that this
	// code is no longer necessary to solve the problem from [p16: 2261] - JDS 12/03/2007
	//
	// Assign this to m_pMainWnd to fix [p16: 2261]
	// Read http://www.experts-exchange.com/Programming/Programming_Languages/MFC/Q_20102369.html
	//AfxGetApp()->m_pMainWnd = this;
}
//-------------------------------------------------------------------------------------------------
BOOL SpotRecognitionDlg::OnToolTipNotify(UINT id, NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	ASSERT(pNMHDR->code == TTN_NEEDTEXTA || pNMHDR->code == TTN_NEEDTEXTW);

     // if there is a top level routing frame then let it handle the message
     if (GetRoutingFrame() != NULL) return FALSE;

     // to be thorough we will need to handle UNICODE versions of the message also !!
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

     if (nID != 0) // will be zero on a separator
     {
		 strTipText.LoadString(nID);
		 switch(nID)
		 {
		 case IDC_BTN_OPENSUBIMAGE:
			 strTipText = m_zOpenImagePortionToolTip;
			 break;
		 }
		 
		 if (pNMHDR->code == TTN_NEEDTEXTA)
		 {
			 lstrcpyn(pTTTA->szText, strTipText, sizeof(pTTTA->szText));
		 }
		 else
		 {
			 _mbstowcsz(pTTTW->szText, strTipText, sizeof(pTTTW->szText));
		 }
		 
		 *pResult = 0;
		 
		 // bring the tooltip window above other popup windows
		 ::SetWindowPos(pNMHDR->hwndFrom, HWND_TOP, 0, 0, 0, 0,SWP_NOACTIVATE|
			 SWP_NOSIZE|SWP_NOMOVE|SWP_NOOWNERZORDER); 
		 
		 return TRUE;
     }
	 
	 return TRUE;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::getLineTextEvaluator(UCLID_SPOTRECOGNITIONIRLib::ILineTextEvaluator **pLineTextEvaluator)
{
	if (m_ipLineTextEvaluator == NULL)
	{
		*pLineTextEvaluator = NULL;
		return;
	}

	UCLID_SPOTRECOGNITIONIRLib::ILineTextEvaluatorPtr ipShallowCopy = m_ipLineTextEvaluator;
	*pLineTextEvaluator = ipShallowCopy.Detach();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::getLineTextCorrector(UCLID_SPOTRECOGNITIONIRLib::ILineTextCorrector **pLineTextCorrector)
{
	if (m_ipLineTextCorrector == NULL)
	{
		*pLineTextCorrector = NULL;
		return;
	}

	UCLID_SPOTRECOGNITIONIRLib::ILineTextCorrectorPtr ipShallowCopy = m_ipLineTextCorrector;
	*pLineTextCorrector = ipShallowCopy.Detach();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::getParagraphTextCorrector(UCLID_SPOTRECOGNITIONIRLib::IParagraphTextCorrector **pParagraphTextCorrector)
{
	if (m_ipParagraphTextCorrector == NULL)
	{
		*pParagraphTextCorrector = NULL;
		return;
	}

	UCLID_SPOTRECOGNITIONIRLib::IParagraphTextCorrectorPtr ipShallowCopy = m_ipParagraphTextCorrector;
	*pParagraphTextCorrector = ipShallowCopy.Detach();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::getParagraphTextHandlers(IIUnknownVector **pParagraphTextHandlers)
{
	if (m_ipParagraphTextHandlers == NULL)
	{
		*pParagraphTextHandlers = NULL;
		return;
	}

	IIUnknownVectorPtr ipShallowCopy = m_ipParagraphTextHandlers;
	*pParagraphTextHandlers = ipShallowCopy.Detach();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::getSRWEventHandler(UCLID_SPOTRECOGNITIONIRLib::ISRWEventHandler **pHandler)
{
	if (m_ipSRWEventHandler == NULL)
	{
		*pHandler = NULL;
		return;
	}

	UCLID_SPOTRECOGNITIONIRLib::ISRWEventHandlerPtr ipShallowCopy = m_ipSRWEventHandler;
	*pHandler = ipShallowCopy.Detach();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::getSubImageHandler(UCLID_SPOTRECOGNITIONIRLib::ISubImageHandler **pHandler, 
											BSTR *pstrToolbarBtnTooltip,
											BSTR *pstrTrainingFile)
{
	if (m_ipSubImageHandler == NULL)
	{
		*pHandler = NULL;
		return;
	}

	UCLID_SPOTRECOGNITIONIRLib::ISubImageHandlerPtr ipShallowCopy = m_ipSubImageHandler;
	*pHandler = ipShallowCopy.Detach();
	*pstrToolbarBtnTooltip = _bstr_t(m_zOpenImagePortionToolTip).Detach();
	*pstrTrainingFile = _bstr_t(m_zTrainingFile).Detach();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setLineTextEvaluator(ILineTextEvaluator *pLineTextEvaluator)
{
	m_ipLineTextEvaluator = pLineTextEvaluator;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setLineTextCorrector(ILineTextCorrector *pLineTextCorrector)
{
	m_ipLineTextCorrector = pLineTextCorrector;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setParagraphTextCorrector(IParagraphTextCorrector *pParagraphTextCorrector)
{
	m_ipParagraphTextCorrector = pParagraphTextCorrector;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::clearParagraphTextHandlers()
{
	m_ipParagraphTextHandlers = NULL;
	configureToolBarButtonsAndUGD();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setParagraphTextHandlers(IIUnknownVector *pParagraphTextHandlers)
{
	if (pParagraphTextHandlers)
	{
		m_ipParagraphTextHandlers = pParagraphTextHandlers;

		// ensure that the number of PTHs provided is within acceptable limits.
		if (m_ipParagraphTextHandlers)
		{
			long nNumPTHs = m_ipParagraphTextHandlers->Size();
			long nMaxAllowedPTHs = (ID_MAX_OCR_OPTION - ID_OCR_ENTIRE_IMAGE_01)/ m_nOCROptionsSpanLen;
			if (nNumPTHs >= nMaxAllowedPTHs)
			{
				UCLIDException ue("ELI04766", "Too many Paragraph Text Handler objects provided!");
				ue.addDebugInfo("# of PTHs", nNumPTHs);
				ue.addDebugInfo("Max allowed", nMaxAllowedPTHs);
				throw ue;
			}
		}

		configureToolBarButtonsAndUGD();
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setSRWEventHandler(ISRWEventHandler *pHandler)
{
	m_ipSRWEventHandler = pHandler;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setSubImageHandler(ISubImageHandler *pHandler, 
											const CString& zTooltip,
											const CString& zTrainingFile)
{
	if (pHandler)
	{
		m_ipSubImageHandler = pHandler;
		m_zOpenImagePortionToolTip = zTooltip;
		m_zTrainingFile = zTrainingFile;
		configureToolBarButtonsAndUGD();
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::getImagePortion(IRasterZone **pRasterZone)
{
	// if current image is not a portion of an image
	if (!m_bIsCurrentImageAnImagePortion || m_ipSubImageZone == NULL)
	{
		throw UCLIDException("ELI03288", "Current image is not a portion of a image.");
	}

	IRasterZonePtr ipRasterZone(m_ipSubImageZone);
	ASSERT_RESOURCE_ALLOCATION("ELI17022", ipRasterZone != NULL);
	*pRasterZone = ipRasterZone.Detach();
}
//-------------------------------------------------------------------------------------------------
bool SpotRecognitionDlg::isHighlightsAdjustableEnabled()
{
	return m_bHighlightsAdjustable;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::enableHighlightsAdjustable(bool bEnable)
{
	m_bHighlightsAdjustable = bEnable;
}
//-------------------------------------------------------------------------------------------------
long SpotRecognitionDlg::getFittingMode()
{
	return m_UCLIDGenericDisplayCtrl.getFittingMode();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setFittingMode(long eFittingMode)
{
	m_UCLIDGenericDisplayCtrl.setFittingMode(eFittingMode);
}
//-------------------------------------------------------------------------------------------------
SpotRecognitionDlg* SpotRecognitionDlg::getDlgInstanceWithImage(const string& strPathName)
{
	string strImageName = strPathName;
	if (GDDFileManager::sIsGDDFile(strPathName))
	{
		// determine the image that is in the .gdd file
		strImageName = GDDFileManager::sGetImageNameFromGDDFile(strPathName);
	}

	vector<SpotRecognitionDlg *>::const_iterator iter;
	for (iter = ms_vecInstances.begin(); iter != ms_vecInstances.end(); iter++)
	{
		SpotRecognitionDlg* pDlg = *iter;
		string strCurrImageName = pDlg->m_UCLIDGenericDisplayCtrl.getImageName();
		if (_strcmpi(strCurrImageName.c_str(), strImageName.c_str()) == 0)
		{
			return pDlg;
		}
	}

	return NULL;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::loadWindowPosition()
{
	long	lLeft = 0;
	long	lTop = 0;
	long	lWidth = 0;
	long	lHeight = 0;
	
	ma_pSRIRCfgMgr->getWindowPos( lLeft, lTop );
	ma_pSRIRCfgMgr->getWindowSize( lWidth, lHeight );
	
	// Adjust window position based on retrieved settings
	MoveWindow( lLeft, lTop, lWidth, lHeight, TRUE );
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::saveWindowPosition()
{
	WINDOWPLACEMENT windowPlacement;
	GetWindowPlacement(&windowPlacement);
	
	RECT	rect;
	rect = windowPlacement.rcNormalPosition;
	
	ma_pSRIRCfgMgr->setWindowPos(rect.left, rect.top);
	ma_pSRIRCfgMgr->setWindowSize(rect.right - rect.left, rect.bottom - rect.top);
}
//-------------------------------------------------------------------------------------------------
string SpotRecognitionDlg::getCurrentZoneImageFileName()
{
	return m_strLastZoneImageFile;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::openFile2(const std::string& strFileToOpen)
{
	// make sure that the image is not already open in another ImageRecognitionDlg window
/*
	SpotRecognitionDlg *pDlgInstance = getDlgInstanceWithImage(strFileToOpen);
	if (pDlgInstance != NULL)
	{
		// force a repaint of the window as it may need repainting
		pDlgInstance->RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW 
			| RDW_ERASE | RDW_ALLCHILDREN);
	
		// flash the window
		flashWindow(pDlgInstance->m_hWnd, true);
		
		// prompt the user if they want to reload the image - if not, just return.
		string strMsg = "The requested image has already been opened.  Would you like to re-load the image?";
		if (MessageBox(strMsg.c_str(), "Warning", MB_ICONQUESTION | MB_YESNO) == IDNO)
		{
			return;
		}
	}
	else
	{
		// all of the code below is common to whether we are opening
		// an image in this window, or whether we are opening an image
		// in another window.  If we did not find a window already
		// open with the requested image, then all the code below should
		// run for this window object.
		pDlgInstance = this;
	}
	*/
	SpotRecognitionDlg *pDlgInstance = this;
	
	// loading the image may take time - so show the wait cursor
	CWaitCursor waitCursor;
	
	// load the image
	pDlgInstance->openFile(strFileToOpen);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::rotateCurrentPage(ERotationAngle eRotationAngle)
{
	// record current view before rotation
	addCurrentViewToStack(true);
	// each page maintain its own base rotation angle
	long nCurrentPageNum = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
	// base rotation shall be per-page base
	double dBaseRotationAngle = m_UCLIDGenericDisplayCtrl.getBaseRotation(nCurrentPageNum);

	switch(eRotationAngle)
	{
	case kRotateLeft:
		{
			dBaseRotationAngle += 90.0;
		}
		break;
	case kRotateRight:
		{
			dBaseRotationAngle = dBaseRotationAngle - 90.0;
		}
		break;
	}

	// make sure the angle is between -360 ~ 360°
	if (dBaseRotationAngle >= 360.0)
	{
		dBaseRotationAngle -= 360.0;
	}
	else if (dBaseRotationAngle <= -360.0)
	{
		dBaseRotationAngle += 360.0;
	}
			
	m_UCLIDGenericDisplayCtrl.setBaseRotation(nCurrentPageNum, dBaseRotationAngle);

	// after rotation, set view extents to the one right before rotation
	PageExtents prevView = m_mapPageToViews[m_UCLIDGenericDisplayCtrl.getCurrentPageNumber()].getCurrentView();

	// Set to fit page or width according to current setting
	if (m_eFitToStatus == kFitToWidth || m_eFitToStatus == kFitToPage)
	{
		zoomToFit();
	}
	else
	{
		// set zoom window if it is not in fit to page or fit to width status
		long lStatus = -1;
		m_UCLIDGenericDisplayCtrl.zoomWindow(prevView.dBottomLeftX, prevView.dBottomLeftY,
												 prevView.dTopRightX, prevView.dTopRightY, &lStatus);
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::showOpenDialog()
{
	OnBTNOpenImage();
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr SpotRecognitionDlg::recognizeParagraphTextInImage(
	const string& strImageFileName, long lStartPage, long lEndPage, LPRECT pRect)
{
	// display wait cursor
	CWaitCursor waitCursor;

	// ensure the the OCR engine exists
	if (!m_ipOCREngine)
	{
		throw UCLIDException("ELI16152", "Cannot perform text recognition. OCR engine is undefined.");
	}

	// create the progress status object for this task
	IProgressStatusPtr ipProgressStatus;
	
	// check if we are recognizing text in a particular region
	if(pRect)
	{
		// this area will be used in determinig whether or not to show the progress bar
		long nArea = abs((pRect->right - pRect->left) * (pRect->bottom - pRect->top));
	
		// initialize the progress status object, if the area is the appropriate size
		ipProgressStatus = getShowOCRProgress(nArea);
	}
	else // we are recognizing text on at least one whole page
	{
		// create the ipProgressStatus object
		ipProgressStatus.CreateInstance(CLSID_ProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI16823", ipProgressStatus != NULL);
	}
/*	
	// create the progress status dialog object
	IProgressStatusDialogPtr ipProgressStatusDlg;

	// if a progress status object was created
	if (ipProgressStatus)
	{
		// create a progress status dialog object
		ipProgressStatusDlg.CreateInstance(CLSID_ProgressStatusDialog);
		ASSERT_RESOURCE_ALLOCATION("ELI16824", ipProgressStatusDlg != NULL);

		// show the progress status dialog
		ipProgressStatusDlg->ShowModelessDialog((long)m_hWnd, 
			_bstr_t("Progress status for " + getFileNameFromFullPath(strImageFileName)),
			ipProgressStatus, 3, 250, VARIANT_TRUE);
	}
*/
	// pre-recognition steps
	m_UCLIDGenericDisplayCtrl.setStatusBarText("Performing text recognition...");
	m_ipOCREngine->LoadTrainingFile(_bstr_t(m_zTrainingFile)); // use-specified training file
	
	// notify the observers of the SpotRecognitionDlg that we are about
	// to perform paragraph-text-recognition in a window
	// if the method call returns a FAILED HRESULT or if the method throws
	// an exception, we are to understand that paragraph text recognition is not
	// to be performed.
	if (m_ipSRWEventHandler != NULL && FAILED(m_ipSRWEventHandler->AboutToRecognizeParagraphText()))
	{
		throw UCLIDException("ELI06542", "Cannot perform paragraph text recognition!");
	}

	ISpatialStringPtr ipText = NULL;
	if(!pRect)
	{
		// perform recognition and get spatial information
		ipText = m_ipOCREngine->RecognizeTextInImage(
			get_bstr_t(strImageFileName), lStartPage, lEndPage, kNoFilter, "", 
			UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, ipProgressStatus);
		
	}
	else
	{
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI10667", ipRect != NULL);
		ipRect->SetBounds(pRect->left, pRect->top, pRect->right, pRect->bottom);

		ipText = m_ipOCREngine->RecognizeTextInImageZone(_bstr_t(strImageFileName.c_str()), 
			lStartPage, lEndPage, ipRect, 0, kNoFilter, "", VARIANT_FALSE, VARIANT_FALSE, 
			VARIANT_TRUE, ipProgressStatus);
	}
	ASSERT_RESOURCE_ALLOCATION("ELI06541", ipText != NULL);

	// if a paragraph text corrector has been specified, use it to
	// correct the recognized text.
	if (m_ipParagraphTextCorrector)
	{
		m_ipParagraphTextCorrector->CorrectText(ipText);
	}

	// return the recognized spatial string
	return ipText;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::processImageForParagraphText(const string& strImageFileToOCR, 
	long lStartPage, long lEndPage, bool bSetCurrentPageNumber, long nOffsetX, long nOffsetY, 
	LPRECT pRect)
{
	// get the spatial string associated with this image and in doing so
	// follow all the usual steps associated with paragraph text recognition
	ISpatialStringPtr ipText = recognizeParagraphTextInImage(strImageFileToOCR, 
		lStartPage, lEndPage, pRect);

	// only operate on non-empty spatial string
	if (ipText->IsEmpty() == VARIANT_FALSE && ipText->HasSpatialInfo() == VARIANT_TRUE)
	{		
		// the image may be a temporary image, such as when recognize-text-in-window
		// operation is performed. Overwrite the source document name with the name
		// of the image that is currently open
		ipText->SourceDocName = _bstr_t(m_UCLIDGenericDisplayCtrl.getImageName());
		
		// if the bSetCurrentPageNumber is true, then update all the page number of
		// all recognized letter objects to be the current page number
		if (bSetCurrentPageNumber)
		{
			ipText->UpdatePageNumber(getCurrentPageNumber());
		}
		
		// offset the spatial text by the prescribed amount if applicable
		if (nOffsetX != 0 || nOffsetY != 0)
		{
			ipText->Offset(nOffsetX, nOffsetY);
		}
	}

	// get access to the current paragraph text handler
	UCLID_SPOTRECOGNITIONIRLib::IParagraphTextHandlerPtr ipPTH;
	ipPTH = m_ipParagraphTextHandlers->At(m_nLastSelectedPTHIndex);
	
	// If we wrote the code correctly, ipPTH should never be NULL
	if (ipPTH == NULL)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI06540");
	}

	// get access to the COM object wrapping this CDialog class
	UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr ipSRIR;
	ipSRIR = m_pInputEntityManager;
	ASSERT_RESOURCE_ALLOCATION("ELI06543", ipSRIR != NULL);

	// send the recognized text to the paragraph text handler
	ipPTH->NotifyParagraphTextRecognized(ipSRIR, ipText);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::deleteFile(const std::string& strToBeDeleted)
{
	if (!strToBeDeleted.empty())
	{
		try
		{
			::deleteFile(strToBeDeleted, true);
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25129");
	}
}
//-------------------------------------------------------------------------------------------------
string SpotRecognitionDlg::createSubImage(const std::string& strOriginalImageFileName, 
										  IRasterZone* pImagePortionInfo, double dRotationAngle)
{
	// Validate Raster Zone
	ASSERT_ARGUMENT("ELI15837", pImagePortionInfo != NULL);

	// Make sure that if the original image is pdf, PDF support is licensed
	LicenseManagement::sGetInstance().verifyFileTypeLicensed( strOriginalImageFileName );

	// get top left corner coordinates in image pixels as the offset
	long nOffsetX = pImagePortionInfo->StartX;
	long nOffsetY = pImagePortionInfo->StartY - (pImagePortionInfo->Height/2);	
	long nSubImageWidth = abs(pImagePortionInfo->EndX - pImagePortionInfo->StartX);
	
	// handle for the image that creates the sub image
	BITMAPHANDLE origImageHandle, subImageHandle; 
	LeadToolsBitmapFreeer freeerOrig( origImageHandle, true );
	LeadToolsBitmapFreeer freeerSub( subImageHandle, true );

	// Get initialized FILEINFO struct
	FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

	// Get file info
	L_INT nRet = L_FileInfo(const_cast<char*>(strOriginalImageFileName.c_str()), &fileInfo, 
		sizeof(FILEINFO), 0, NULL);

	// get original file info
	if (nRet < 1)
	{
		UCLIDException uclidException("ELI03245", "Failed to get original image info.");
		uclidException.addDebugInfo("strImageFileName", strOriginalImageFileName);
		uclidException.addDebugInfo("Error Code", nRet);
		throw uclidException;
	}

	// Get initialized FILEPDFOPTIONS struct
	FILEPDFOPTIONS pdfOptions = GetLeadToolsSizedStruct<FILEPDFOPTIONS>(0);

	// Retrieve current PDF load options
	nRet = L_GetPDFOptions( &pdfOptions, sizeof(pdfOptions) );
	throwExceptionIfNotSuccess(nRet, "ELI16870", "Could not retrieve PDF load options.");

	// Get initialized LOADFILEOPTION struct. 
	// IgnoreViewPerspective to avoid a black region at the bottom of the image
	LOADFILEOPTION loadOptions = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);

	// Add Page, Image Resolution and IgnoreViewPerspective to default load options
	nRet = L_GetDefaultLoadFileOption(&loadOptions, sizeof(LOADFILEOPTION));
	loadOptions.PageNumber = pImagePortionInfo->PageNumber;
	loadOptions.XResolution = pdfOptions.nXResolution;
	loadOptions.YResolution = pdfOptions.nYResolution;

	// Load this page
	nRet = L_LoadFile( _bstr_t(strOriginalImageFileName.c_str()), &origImageHandle, 
		sizeof(BITMAPHANDLE), 0, 0, LOADFILE_ALLOCATE | LOADFILE_STORE, NULL, NULL, 
		&loadOptions, &fileInfo );
	throwExceptionIfNotSuccess(nRet, "ELI16948", "Could not load the file.",
		strOriginalImageFileName);

	// Define associated rectangle
	RECT rc;
	rc.left = nOffsetX;
	rc.top = nOffsetY;
	rc.right = nSubImageWidth + nOffsetX;
	rc.bottom = pImagePortionInfo->Height + nOffsetY;

	// Make sure coordinates are in View Perspective of the Bitmap
	nRet = L_RectToBitmap( &origImageHandle, TOP_LEFT, &rc );
	throwExceptionIfNotSuccess(nRet, "ELI16871", "Could not adjust coordinates.");
	nOffsetX = rc.left;
	nOffsetY = rc.top;
	nSubImageWidth = rc.right - rc.left;
	pImagePortionInfo->Height = rc.bottom - rc.top;

	// get the rect image inside the original one
	nRet = L_CopyBitmapRect(&subImageHandle, &origImageHandle, sizeof(BITMAPHANDLE), 
		nOffsetX, nOffsetY, nSubImageWidth, pImagePortionInfo->Height);
	if (nRet < 1)
	{
		UCLIDException uclidException("ELI03247", "Failed to get the sub image from original image.");
		uclidException.addDebugInfo("strImageFileName", strOriginalImageFileName);
		uclidException.addDebugInfo("Error Code", nRet);
		throw uclidException;
	}

	// Rotate the copied bitmap to match the current view
	// Allow the image to be resized with new pixels stored as white
	int nRotationAngle = (int)(-1 * dRotationAngle);
	if (nRotationAngle != 0)
	{
		nRet = L_RotateBitmap( &subImageHandle, nRotationAngle * 100, ROTATE_RESIZE, 
			RGB(255,255,255) );
		if (nRet < 1)
		{
			UCLIDException uclidException("ELI16712", "Failed to rotate the bitmap.");
			uclidException.addDebugInfo("Rotation Angle", nRotationAngle);
			uclidException.addDebugInfo("Error Code", nRet);
			throw uclidException;
		}
	}

	// get extension from original file
	string strFileExtension( getExtensionFromFullPath( strOriginalImageFileName, true ) );

	// generate a temp file, but do not delete it automatically
	TemporaryFileName subImageFile(NULL, strFileExtension.c_str(), false);

	// Prepare the default save options
	int nFileFormat = fileInfo.Format;
	int nBitsPerPixel = subImageHandle.BitsPerPixel;

	// Get initialized SAVEFILEOPTION struct
	SAVEFILEOPTION sfo = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);

	// Special settings for PDF input / output image
	if (strFileExtension == ".pdf")
	{
		nFileFormat = FILE_RAS_PDF_G4;
		nBitsPerPixel = 1;
		sfo.Flags = ESO_PDF_SAVE_USE_BITMAP_DPI;
	}

	try
	{
		// save the new image file
		nRet = L_SaveBitmap(const_cast<char*>(subImageFile.getName().c_str()), 
			&subImageHandle, nFileFormat, nBitsPerPixel, 2, &sfo);
		throwExceptionIfNotSuccess(nRet, "ELI03248",
			"Failed to save the sub image into a file.", subImageFile.getName());
		waitForFileToBeReadable(subImageFile.getName());
	}
	catch(UCLIDException& uex)
	{
		uex.addDebugInfo("strOriginalImageFileName", strOriginalImageFileName);
		throw uex;
	}

	return subImageFile.getName();
}
//-------------------------------------------------------------------------------------------------
IRasterZonePtr SpotRecognitionDlg::getOCRZone(long lID)
{
	// convert the ID from string to long, and get the zone parameters
	long nStartPosX, nStartPosY, nEndPosX, nEndPosY, nTotalZoneHeight;
	m_UCLIDGenericDisplayCtrl.getZoneEntityParameters(lID, &nStartPosX, &nStartPosY, &nEndPosX, 
		&nEndPosY, &nTotalZoneHeight);

	// determine offset in the current image if the current image is a subimage
	long nOffsetX = 0, nOffsetY = 0;
	if (m_bIsCurrentImageAnImagePortion && m_ipSubImageZone != NULL)
	{
		nOffsetX = m_ipSubImageZone->StartX;
		nOffsetY = m_ipSubImageZone->StartY - m_ipSubImageZone->Height / 2;
	}

	// create a new raster zone object
	IRasterZonePtr ipRasterZone(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI03324", ipRasterZone != NULL);

	// populate the raster zone object with the parameters
	ipRasterZone->StartX = nStartPosX + nOffsetX;
	ipRasterZone->StartY = nStartPosY + nOffsetY;
	ipRasterZone->EndX = nEndPosX + nOffsetX;
	ipRasterZone->EndY = nEndPosY + nOffsetY;
	ipRasterZone->Height = nTotalZoneHeight;

	// determine the page number of the zone entity
	CString zPageNumber = m_UCLIDGenericDisplayCtrl.getEntityAttributeValue(lID, "Page");
	ipRasterZone->PageNumber = asLong((LPCTSTR) zPageNumber);

	// Return the raster zone object to the caller
	return ipRasterZone;
}
//-------------------------------------------------------------------------------------------------
long SpotRecognitionDlg::createZoneEntity(IRasterZone *pZone, long nColor)
{
	// ensure proper argument
	IRasterZonePtr ipZone(pZone);
	ASSERT_ARGUMENT("ELI06559", ipZone != NULL);

	// get the individual attributes of the raster zone into long's
	long nStartX, nStartY, nEndX, nEndY, nHeight, nPageNum;
	ipZone->GetData(&nStartX, &nStartY, &nEndX, &nEndY, 
		&nHeight, &nPageNum);

	// create the specified zone entity
	long nID = m_UCLIDGenericDisplayCtrl.addZoneEntity(
		nStartX, nStartY, nEndX, nEndY, nHeight, nPageNum, TRUE, FALSE, 0);
	
	// set the color of the zone as specified
	m_UCLIDGenericDisplayCtrl.setEntityColor(nID, nColor);

	// return the ID of the newly created zone entity
	return nID;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::deleteZoneEntity(long nID)
{
	// make sure the entity exists before deleting it
	if (nID != -1 && !m_UCLIDGenericDisplayCtrl.getEntityInfo(nID).IsEmpty())
	{
		// delete the specified entity
		m_UCLIDGenericDisplayCtrl.deleteEntity(nID);
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::zoomAroundZoneEntity(long nID)
{
	// get the data associated with the zone entity
	long nStartX, nStartY, nEndX, nEndY, nHeight, nPage;
	m_UCLIDGenericDisplayCtrl.getZoneEntityParameters(nID, &nStartX, &nStartY,
		&nEndX, &nEndY, &nHeight);

	// get the page number associated with the zone
	nPage = asLong((LPCTSTR) 
		m_UCLIDGenericDisplayCtrl.getEntityAttributeValue(nID, "Page"));

	// update the fit to status
	m_eFitToStatus = kFitToNothing;

	// now update the fit button states
	setFitToButtonsStatus();

	// build a rasterzone object corresponding to the entity
	IRasterZonePtr ipZone(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI06575", ipZone != NULL);
	ipZone->StartX = nStartX;
	ipZone->StartY = nStartY;
	ipZone->EndX = nEndX;
	ipZone->EndY = nEndY;
	ipZone->Height = nHeight;
	ipZone->PageNumber = nPage;

	// get the rectangular bounds of the raster zone and the
	// center of the rectangle
	// NOTE: Although GetRectangularBounds can return bounds that exceed page limits,
	// the resultant center point is the midpoint of the RasterZone's line, which
	// is always contained on the page. Therefore, the rectangular bounds don't need
	// to be validated.
	ILongRectanglePtr ipBounds = ipZone->GetRectangularBounds(NULL);
	ASSERT_RESOURCE_ALLOCATION("ELI06578", ipBounds != NULL);

	long nCenterX = (ipBounds->Left + ipBounds->Right) / 2;
	long nCenterY = (ipBounds->Top + ipBounds->Bottom) / 2;
	
	// center the current view at the center of the bounds
	double dCenterX, dCenterY;
	m_UCLIDGenericDisplayCtrl.convertImagePixelToWorldCoords(nCenterX, nCenterY,
		&dCenterX, &dCenterY, nPage);
	m_UCLIDGenericDisplayCtrl.zoomCenter(dCenterX, dCenterY);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::createTemporaryHighlight(ISpatialString *pText)
{
	// verify valid argument
	ISpatialStringPtr ipText(pText);
	ASSERT_RESOURCE_ALLOCATION("ELI06580", ipText != NULL);

	// delete the previously created zone entity if any
	deleteTemporaryHighlight();

	// ensure that the string is spatial and not empty
	if (ipText->IsEmpty() == VARIANT_TRUE || ipText->HasSpatialInfo() == VARIANT_FALSE)
	{
		return;
	}

	long nStartPage = ipText->GetFirstPageNumber();
	// open the page where this string is located if the currently loaded
	// page is different than where this string is located
	if (nStartPage != getCurrentPageNumber())
	{
		gotoPage(nStartPage);
	}

	// divide the text into multiple lines
	IIUnknownVectorPtr ipLines = ipText->GetLines();
	ASSERT_RESOURCE_ALLOCATION("ELI09210", ipLines != NULL);

	// Create temporary highlights for the lines
	createTemporaryHighlightsForLines( ipLines );

	// Get a raster Zone for the Entire Text that we can zoom around
	IIUnknownVectorPtr ipZones = ipText->GetOriginalImageRasterZones();
	ASSERT_RESOURCE_ALLOCATION("ELI09307", ipZones != NULL);

	if(ipZones->Size() > 0)
	{
		
		IRasterZonePtr ipZone = ipZones->At(0);
		ASSERT_RESOURCE_ALLOCATION("ELI09535", ipZone != NULL);

		// get the rectangular bounds of the raster zone and the
		// center of the rectangle
		// NOTE: Although GetRectangularBounds can return bounds that exceed page limits,
		// the resultant center point is the midpoint of the RasterZone's line, which
		// is always contained on the page. Therefore, the rectangular bounds don't need
		// to be validated.
		ILongRectanglePtr ipRect = ipZone->GetRectangularBounds(NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI19888", ipRect != NULL);
		long lLeft, lTop, lRight, lBottom;
		ipRect->GetBounds(&lLeft, &lTop, &lRight, &lBottom);
		
		long nCenterX = (lLeft + lRight) / 2;
		long nCenterY = (lTop + lBottom) / 2;
		
		// center the current view at the center of the bounds
		double dCenterX, dCenterY;
		m_UCLIDGenericDisplayCtrl.convertImagePixelToWorldCoords(nCenterX, nCenterY,
			&dCenterX, &dCenterY, nStartPage);
		m_UCLIDGenericDisplayCtrl.zoomCenter(dCenterX, dCenterY);
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::addTemporaryHighlight( long nStartX, long nStartY, 
												long nEndX, long nEndY, 
												long nHeight, long nPage)
{
	BOOL bDocModified = m_UCLIDGenericDisplayCtrl.documentIsModified();

	IRasterZonePtr ipRasterZone(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI12077", ipRasterZone != NULL);

	ipRasterZone->StartX = nStartX;
	ipRasterZone->StartY = nStartY;
	ipRasterZone->EndX = nEndX;
	ipRasterZone->EndY = nEndY;
	ipRasterZone->Height = nHeight;
	ipRasterZone->PageNumber = nPage;

	if (nPage != getCurrentPageNumber())
	{
		gotoPage(nPage);
	}

	// create the zone entity
	long nTempHighlightEntityID = createZoneEntity(
		ipRasterZone, TEMP_HIGHLIGHT_COLOR);

	// store in the vec
	m_vecTempHighlightEntityIDs.push_back(nTempHighlightEntityID);
	
	// restore the "modified" state of the generic display control
	// to what it used to be be at the beginning of this method
	m_UCLIDGenericDisplayCtrl.setDocumentModified(bDocModified);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::deleteTemporaryHighlight()
{
	// remember the state of the modified flag of the generic display
	// control, as we need to restore this state after we create
	// the temporary highlight
	BOOL bDocModified = m_UCLIDGenericDisplayCtrl.documentIsModified();

	try
	{
		for (unsigned int ui = 0; ui < m_vecTempHighlightEntityIDs.size(); ui++)
		{
			long nEntityID = m_vecTempHighlightEntityIDs[ui];
			deleteZoneEntity(nEntityID);
		}
	}
	catch (...)
	{
		// ignore any exceptions
	}

	// restore the "modified" state of the generic display control
	// to what it used to be be at the beginning of this method
	m_UCLIDGenericDisplayCtrl.setDocumentModified(bDocModified);

	// always restore m_vecTempHighlightEntityIDs to empty meaning "no temporary
	// highlight currently exists"
	m_vecTempHighlightEntityIDs.clear();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::centerOnTemporaryHighlight()
{
	long nStartPage;

	long nLeft, nRight, nTop, nBottom;
	getFirstTempHighlightPageBounds(nStartPage, nLeft, nTop, nRight, nBottom);


	long nCenterX = (nLeft + nRight) / 2;
	long nCenterY = (nTop + nBottom) / 2;


	// center the current view at the center of the bounds
	double dCenterX, dCenterY;
	setCurrentPageNumber(nStartPage);

	m_UCLIDGenericDisplayCtrl.convertImagePixelToWorldCoords(nCenterX, nCenterY,
		&dCenterX, &dCenterY, nStartPage);
	m_UCLIDGenericDisplayCtrl.zoomCenter(dCenterX, dCenterY);

}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::zoomToTemporaryHighlight()
{
	long nStartPage;
	long nLeft, nRight, nTop, nBottom;
	getFirstTempHighlightPageBounds(nStartPage, nLeft, nTop, nRight, nBottom);

	long nCenterX = (nLeft + nRight) / 2;
	long nCenterY = (nTop + nBottom) / 2;

	setCurrentPageNumber(nStartPage);

	zoomPointWidth(nCenterX, nCenterY, (nRight - nLeft) * gnZoomToTemporaryHighlightWidthMultiplier);

}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::addFileToMRUList(const std::string& strFileToBeAdded)
{
	// we wish to have updated items all the time
	ma_pRecentFiles->readFromPersistentStore();
	ma_pRecentFiles->addItem(strFileToBeAdded);
	ma_pRecentFiles->writeToPersistentStore();	
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::removeFileFromMRUList(const std::string& strFileToBeRemoved)
{
	// remove the bad file from MRU List
	ma_pRecentFiles->readFromPersistentStore();
	ma_pRecentFiles->removeItem(strFileToBeRemoved);
	ma_pRecentFiles->writeToPersistentStore();	
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::showToolbarCtrl(ESRIRToolbarCtrl eCtrl, bool bShow)
{
	m_apToolBar->showToolbarCtrl(eCtrl, bShow);
	if (eCtrl == kBtnSave)
	{
		m_bUserSavingAllowed = bShow;
	}
	if (eCtrl == kBtnOpenImage)
	{
		DragAcceptFiles( asMFCBool(bShow) );
		m_bCanOpenFiles = bShow;
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::showTitleBar(bool bShow)
{
	DWORD dwFlags = WS_CAPTION | WS_BORDER;

	if (bShow)
	{
		ModifyStyle(0, dwFlags, SWP_FRAMECHANGED);
	}
	else
	{
		ModifyStyle(dwFlags, 0, SWP_FRAMECHANGED);
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::zoomPointWidth(long nX, long nY, long nWidth)
{
	double dWorldWidth, dWorldHeight;
	m_UCLIDGenericDisplayCtrl.getImageExtents(&dWorldWidth, &dWorldHeight);
	long nImageWidth, nImageHeight;
	m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(dWorldWidth, 0, &nImageWidth, &nImageHeight, getCurrentPageNumber());

	// update the fit to status
	m_eFitToStatus = kFitToNothing;

	// now update the fit button states [p16 #2925]
	setFitToButtonsStatus();

	CRect rect;
	GetClientRect(&rect);
	long nWndWidth = rect.Width();

	if (nWidth > nImageWidth)
	{
		nWidth = nImageWidth;
	}

	double dPercentage = 100.0 * (double)nWndWidth / (double)nWidth;
	m_UCLIDGenericDisplayCtrl.setZoom(dPercentage);

	double dX, dY;
	m_UCLIDGenericDisplayCtrl.convertImagePixelToWorldCoords(nX, nY, &dX, &dY, getCurrentPageNumber());
	m_UCLIDGenericDisplayCtrl.zoomCenter(dX, dY);

	addCurrentViewToStack();
}
//-------------------------------------------------------------------------------------------------

void SpotRecognitionDlg::createSelectionToolMenu()
{
	// this code is no longer necessary with the new version of BCMenu (3.036) - JDS 12/03/2007
	//
	// If AfxGetApp()->m_pMainWnd returns a NULL inside BCMenu.cpp and CWnd LoadMenu will 
	// throw an exception so we will temporarily set the CWinApp mainWnd ptr to this so 
	// that AfxGetApp()->m_pMainWnd never returns NULL (P16: 2261)
	// Please read http://www.experts-exchange.com/Programming/Programming_Languages/MFC/Q_20102369.html
	// for detailed description
	//AfxAppMainWindowRestorer tempRestorer;
	//AfxGetApp()->m_pMainWnd = this;
	//
	//m_menuSelectionTools.ModifyODMenu(NULL, ID_MENU_SWIPE_SELECTION_TOOL, IDB_SWIPE_SELECTION_TOOL);
	//m_menuSelectionTools.ModifyODMenu(NULL, ID_MENU_RECT_SELECTION_TOOL, IDB_RECT_SELECTION_TOOL);

	// preferred method of loading the toolbar in the new version of BCMenu (3.036 - JDS 12/03/2007)
	m_menuSelectionTools.LoadMenu(IDR_MENU_SELECTION_TOOL);
	m_menuSelectionTools.LoadToolbar(IDR_TOOLBAR_SELECTION);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::enableAutoOCR(bool bEnable)
{
	m_bAutoOCR = bEnable;
	configureToolBarButtonsAndUGD();
}
//-------------------------------------------------------------------------------------------------
IProgressStatusPtr SpotRecognitionDlg::getShowOCRProgress(long nRecArea)
{
	// Here we will determine if progress should be shown or not
	// this will be determined by the area of the zone to be recognized
	// as compared to the overall size of the document

	long nPageWidth, nPageHeight;
	m_UCLIDGenericDisplayCtrl.getCurrentPageSize(&nPageWidth, &nPageHeight);
	long nDocumentArea = nPageWidth * nPageHeight;
	long nPercent = (100*nRecArea) / nDocumentArea;
	
	// create a progress status object if the 
	// area is large enough to be appropriate
	IProgressStatusPtr ipProgress;
	if (nPercent > gnNoProgessDisplayPercentageThreshold)
	{
		ipProgress.CreateInstance(CLSID_ProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI16825", ipProgress != NULL);
	}
	
	return ipProgress;
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::loadOptionsFromFile(const std::string& strFileName)
{
	m_cfgFileReader.loadSettingsFromFile(strFileName, this);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::zoomIn()
{
	OnBTNZoomIn();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::zoomOut()
{
	OnBTNZoomOut();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::zoomExtents()
{
	OnBTNFitPage();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::zoomFitToWidth()
{
	OnBTNFitWidth();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::getFirstTempHighlightPageBounds(long& nStartPage, long& nLeft, long& nTop, long& nRight, long& nBottom)
{
	if (m_vecTempHighlightEntityIDs.size() == 0)
	{
		UCLIDException ue("ELI12244", "No temporary highlight available for zooming.");
		throw ue;
	}
	
	nStartPage = -1;

	double dLeft = -1, dRight = -1, dTop = -1, dBottom = -1;
	unsigned int ui;
	for (ui = 0; ui < m_vecTempHighlightEntityIDs.size(); ui++)
	{
		long nEntityID = m_vecTempHighlightEntityIDs[ui];
		long nTmpPage = m_UCLIDGenericDisplayCtrl.getEntityPage(nEntityID);
		if (nStartPage == -1)
		{
			nStartPage = nTmpPage;
		}

		if (nTmpPage != nStartPage)
		{
			continue;
		}
		double dTempLeft = -1, dTempRight = -1, dTempTop = -1, dTempBottom = -1;
		m_UCLIDGenericDisplayCtrl.getEntityExtents(nEntityID, &dTempLeft, 
			&dTempBottom, &dTempRight, &dTempTop);

		if (dLeft == -1 || dTempLeft < dLeft)
		{
			dLeft = dTempLeft;
		}
		if (dTop == -1 || dTempTop < dTop)
		{
			dTop = dTempTop;
		}
		if (dRight == -1 || dTempRight > dRight)
		{
			dRight = dTempRight;
		}
		if (dBottom == -1 || dTempBottom > dRight)
		{
			dBottom = dTempBottom;
		}
	}
	
	m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(dLeft, dTop, &nLeft, &nTop, nStartPage);
	m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(dRight, dBottom, &nRight, &nBottom, nStartPage);
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::zoomToFit()
{
	// if the dialog has not yet initialized, then just return
	if (!m_bInitialized)
	{
		return;
	}

	// determine whether an image is opened
	if (m_UCLIDGenericDisplayCtrl.getImageName() == "")
	{
		// Just return if there is no file open
		return;
	}

	if (m_eFitToStatus == kFitToNothing)
	{
		// Just return if it is not in fit to page or fit to width status
		return;
	}
	else if (m_eFitToStatus == kFitToPage)
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_FitPage) == FALSE) || 
			(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_FitPage) == TRUE))
		{
			return;
		}

		// Zoom the image to fit the page
		m_UCLIDGenericDisplayCtrl.zoomExtents();
	}
	else if (m_eFitToStatus == kFitToWidth)
	{
		// Ignore this command if toolbar button is disabled or hidden
		if ((m_apToolBar->GetToolBarCtrl().IsButtonEnabled(IDC_BTN_FitWidth) == FALSE) || 
				(m_apToolBar->GetToolBarCtrl().IsButtonHidden(IDC_BTN_FitWidth) != 0))
		{
			return;
		}

		// Zoom the image to fit the width
		m_UCLIDGenericDisplayCtrl.zoomFitToWidth();
	}
	else
	{
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI14803");
	}

	// push current view on to the view stack
	addCurrentViewToStack();
	configureZoomPrevNextButtons();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setFitToButtonsStatus()
{
	if (m_eFitToStatus == kFitToPage)
	{
		// Press the fit to page button and unpress the fit to width button
		m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_FitPage, true);
		m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_FitWidth, false);
	}
	else if (m_eFitToStatus == kFitToWidth)
	{
		// Press the fit to width button and unpress the fit to page button
		m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_FitWidth, true);
		m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_FitPage, false);
	}
	else
	{
		// Unpress both the fit to page button and the fit to width button
		m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_FitPage, false);
		m_apToolBar->GetToolBarCtrl().PressButton(IDC_BTN_FitWidth, false);
	}
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::setFitToStatus()
{
	// Set the status of two buttons
	setFitToButtonsStatus();

	// Call zoomToFit() to fit the page
	zoomToFit();
}
//-------------------------------------------------------------------------------------------------
void SpotRecognitionDlg::createTemporaryHighlightsForLines(IIUnknownVectorPtr ipLines )
{
	// remember the state of the modified flag of the generic display
	// control, as we need to restore this state after we create
	// the temporary highlight
	BOOL bDocModified = m_UCLIDGenericDisplayCtrl.documentIsModified();

	long nSize = ipLines->Size();
	for (long n=0; n<nSize; n++)
	{
		ISpatialStringPtr ipLine = ipLines->At(n);
		ASSERT_RESOURCE_ALLOCATION("ELI09211", ipLine != NULL);

		// only get bounds on the string that's spatial
		if (ipLine->HasSpatialInfo() == VARIANT_FALSE)
		{
			continue;
		}

		long nPage = ipLine->GetFirstPageNumber();

		// Get the raster Zone(s) for this line
		IIUnknownVectorPtr ipRasterZones = ipLine->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI09212", ipRasterZones != NULL);

		if(ipRasterZones->Size() <= 0)
		{
			UCLIDException ue("ELI09534",
				"Raster zone info missing; unable to display highlight.");
			throw ue;
		}

		// Create a highlight for each raster zone that is associated 
		// with this spatial string.
		long nZones = ipRasterZones->Size();
		for (long x = 0; x < nZones; x++)
		{
			IRasterZonePtr ipRasterZone = ipRasterZones->At(x);
			ASSERT_RESOURCE_ALLOCATION("ELI09213", ipRasterZone != NULL);

			// create the zone entity
			long nTempHighlightEntityID = createZoneEntity(
				ipRasterZone, TEMP_HIGHLIGHT_COLOR);

			// store in the vec
			m_vecTempHighlightEntityIDs.push_back(nTempHighlightEntityID);
		}

		// restore the "modified" state of the generic display control
		// to what it used to be be at the beginning of this method
		m_UCLIDGenericDisplayCtrl.setDocumentModified(bDocModified);
	}
}
//-------------------------------------------------------------------------------------------------
