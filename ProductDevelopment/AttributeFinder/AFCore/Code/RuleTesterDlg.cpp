#include "stdafx.h"
#include "afcore.h"
#include "RuleTesterDlg.h"
#include "TesterConfigMgr.h"
#include "TesterDlgEdit.h"
#include "AFAboutDlg.h"
#include "AFInternalUtils.h"
#include "Common.h"

#include <SpecialStringDefinitions.h>
#include <RegistryPersistenceMgr.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <FileRecoveryManager.h>
#include <cpputil.h>
#include <Win32Util.h>
#include <FileDialogEx.h>
#include <ComUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Spacing for splitter and other controls
const int giMIN_LISTBOX_PLUS_TOOLBAR_HEIGHT = 100;
const int giCONTROL_SPACING = 10;
// class ID for this Dlg which is also a CoClass
// {1EDA97D0-BFBC-46d7-B8D9-40538AE238CD}
const CLSID CLSID_RuleTesterDlg = { 0x1eda97d0, 0xbfbc, 0x46d7, { 0xb8, 0xd9, 0x40, 0x53, 0x8a, 0xe2, 0x38, 0xcd } };
const string gstrUNKNOWN_TYPE = "[Unknown Type]";

//-------------------------------------------------------------------------------------------------
// RuleTesterDlg
//-------------------------------------------------------------------------------------------------
RuleTesterDlg::RuleTesterDlg(FileRecoveryManager *pFRM,
							 UCLID_AFCORELib::IRuleSetPtr& ipRuleSet,
							 CWnd* pParent /*=NULL*/)
:CDialog(RuleTesterDlg::IDD, pParent),	
 m_bInitialized(false), 
 m_iBarHeight(0),
 m_iBrowseSpace(0), 
 m_ipRuleSet(ipRuleSet), 
 m_lRefCount(0), 
 m_ipAttributes(NULL),
 m_ipEngine(NULL),
 m_nCurrentBottomOfPropPage(0),
 m_pFRM(pFRM),
 m_eMode(kWithSettingsTab),
 m_bNoHighlight( false ),
 m_bNoSpotRecognition(false)
{
	try
	{
		//{{AFX_DATA_INIT(RuleTesterDlg)
		//}}AFX_DATA_INIT

		// Get Configuration Manager for dialog
		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAF_AFCORE_KEY_PATH ) );
		
		ma_pCfgTesterMgr = auto_ptr<TesterConfigMgr>(new TesterConfigMgr( 
			ma_pUserCfgMgr.get(), "\\RuleTester" ) );

		if ( pFRM == NULL )
		{
			m_eMode = kWithRulesetTab;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04905")
}
//-------------------------------------------------------------------------------------------------
RuleTesterDlg::~RuleTesterDlg()
{
	try
	{
		// if we have successfully connected to the input manager,
		// release the reference and also destroy all currently open input receivers.
		if (getInputManager())
		{
			getInputManager()->Destroy();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16303");
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(RuleTesterDlg)
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(RuleTesterDlg, CDialog)
	//{{AFX_MSG_MAP(RuleTesterDlg)
	ON_WM_CLOSE()
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_WM_DROPFILES()
	ON_WM_PAINT()
	ON_COMMAND(ID_BUTTON_SRIR, OnButtonSRIR)
	ON_COMMAND(ID_BUTTON_CLEAR, OnButtonClear)
	ON_COMMAND(ID_BUTTON_EXECUTE, OnButtonExecute)
	ON_COMMAND(ID_BUTTON_FEEDBACK, OnButtonFeedback)
	ON_COMMAND(ID_BUTTON_VOA, OnButtonVoa)
	ON_COMMAND(ID_BUTTON_KEY_SR, OnButtonEnableSRIR)
	ON_COMMAND(ID_BUTTON_ABOUT, OnButtonAbout)
	//}}AFX_MSG_MAP
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXT,0x0000,0xFFFF,OnToolTipNotify)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Message Handlers and overridden methods
//-------------------------------------------------------------------------------------------------
LRESULT RuleTesterDlg::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam) 
{
	if (message == WM_NOTIFY)
	{
		if (wParam == IDC_SPLITTER)
		{	
			doResize();
		}
		// Check for notification from the Tree List
		else if (wParam == ID_TREE_LIST_CTRL)
		{
			NMHDR	*pnmhdr = (NMHDR *)lParam;

			// Check for Selection Changed notification
			if (pnmhdr->code == TVN_SELCHANGED)
			{
				if (!m_bNoSpotRecognition)
				{
					if ( !m_bNoHighlight )
					{
						highlightAttributeInRow();
					}
				}
			}
		}
	}
	else if (message == WM_LBUTTONDBLCLK)
	{
		editSelectedAttribute();
	}
	
	return CDialog::DefWindowProc(message, wParam, lParam);
}
//-------------------------------------------------------------------------------------------------
BOOL RuleTesterDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		try
		{
			CDialog::OnInitDialog();

			////////////
			// load icon
			////////////
			m_hIcon = AfxGetApp()->LoadIcon(IDI_ICON_RULE_TESTER);
			SetIcon(m_hIcon, TRUE);			// Set big icon
			SetIcon(m_hIcon, FALSE);			// Set small icon

			// Prepare the tree list
			prepareTreeList();

			// create the toolbar associated with this window
			createToolBar();

			// set the configuration persistence obj in each of
			// the property pages
			m_testerDlgInputPage.setTesterConfigMgr(ma_pCfgTesterMgr.get());
			m_testerDlgSettingsPage.setTesterConfigMgr(ma_pCfgTesterMgr.get());

			// create the property sheet based on Mode
			if (m_eMode == kWithSettingsTab)
			{
				m_propSheet.AddPage(&m_testerDlgSettingsPage);
			}
			else if (m_eMode == kWithRulesetTab)
			{
				m_propSheet.AddPage(&m_testerDlgRulesetPage);
			}

			m_propSheet.AddPage(&m_testerDlgInputPage);
			m_propSheet.Create(this, WS_CHILD | WS_VISIBLE, 0);
			m_propSheet.ModifyStyleEx (0, WS_EX_CONTROLPARENT);

			// activate each of the property pages so that the controls
			// on each of the pages are initialized
			int iNumpages = m_propSheet.GetPageCount();
			for (int i = iNumpages - 1; i >= 0; i--)
			{
				m_propSheet.SetActivePage(i);
			}

			// Set the File name if one was given with the ruleset
			if (m_eMode == kWithRulesetTab)
			{
				string strFileName = m_ipRuleSet->FileName;
				m_testerDlgRulesetPage.setRulesFileName( strFileName );
			}

			// get the current size of the property sheet, and remember the
			// width and height.  We need to ensure that the property
			// sheet is never made smaller than this size calculated
			// by CPropertySheet.  CPropertySheet sets itself up to be
			// the size of the largest page it contains.
			CRect rectPropSheet;
			m_propSheet.GetWindowRect(&rectPropSheet);
			ScreenToClient(&rectPropSheet);
			m_propPageMinSize.cx = rectPropSheet.Width();
			m_propPageMinSize.cy = rectPropSheet.Height();

			// show the splitter control where it belongs
			CRect rectDlg;
			GetWindowRect(rectDlg);
			ScreenToClient(rectDlg);

			CRect rectToolBar;
			m_apToolBar->GetWindowRect(&rectToolBar);
			ScreenToClient(rectToolBar);

			// Retrieve stored splitter position
			long	lPosition = 0;
			lPosition = ma_pCfgTesterMgr->getSplitterPosition();

			// Protect against negative or unusually small value from registry (P16 #2510)
			if (lPosition < m_propPageMinSize.cy)
			{
				lPosition = m_propPageMinSize.cy;
			}

			CRect rectSplitter;
			rectSplitter.left = 0;
			rectSplitter.right = rectDlg.Width();
			rectSplitter.top = lPosition;
			rectSplitter.bottom = rectSplitter.top;
			m_splitterCtrl.Create(WS_CHILD | WS_VISIBLE, rectSplitter, this, IDC_SPLITTER);

			// Set flag indicating that controls exist
			m_bInitialized = true;

			///////////////////////////
			// Retrieve persistent size and position of dialog
			///////////////////////////
			long	lLeft = 0;
			long	lTop = 0;
			long	lWidth = 0;
			long	lHeight = 0;
			ma_pCfgTesterMgr->getWindowPos( lLeft, lTop );
			ma_pCfgTesterMgr->getWindowSize( lWidth, lHeight );
			
			// Adjust window position based on retrieved settings
			MoveWindow( lLeft, lTop, lWidth, lHeight, TRUE );

			// Refresh the window
			doResize();

			m_ToolTipCtrl.Create(this,TTS_ALWAYSTIP);

			updateButtonStates();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI04884")
	}
	catch (UCLIDException& ue)
	{
		ue.display();

		// check for tool bar and splitter control creation, if not created then
		// exit the application
		if (!m_apToolBar.get()
			|| !asCppBool(::IsWindow(m_splitterCtrl.m_hWnd))) 
		{
			CDialog::OnCancel();
		}
	}

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnClose() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		//////////////////
		// Remember window positions
		//////////////////
		WINDOWPLACEMENT windowPlacement;
		if (GetWindowPlacement( &windowPlacement ) != 0)
		{
			RECT	rect;
			rect = windowPlacement.rcNormalPosition;
			
			ma_pCfgTesterMgr->setWindowPos( rect.left, rect.top );
			ma_pCfgTesterMgr->setWindowSize( rect.right - rect.left, 
				rect.bottom - rect.top );
		}

		//////////////////
		// Remember splitter position
		//////////////////
		CRect	rectSplitter;
		m_splitterCtrl.GetWindowRect( rectSplitter );
		ScreenToClient( &rectSplitter );
		ma_pCfgTesterMgr->setSplitterPosition( rectSplitter.top );

		//////////////////
		// Retrieve and store width of Name column
		//////////////////
		long lWidth = m_wndTreeList.m_tree.GetColumnWidth( 0 );
		ma_pCfgTesterMgr->setNameColumnWidth( lWidth );

		//////////////////
		// Retrieve and store width of Type column
		//////////////////
		lWidth = m_wndTreeList.m_tree.GetColumnWidth( 2 );
		ma_pCfgTesterMgr->setTypeColumnWidth( lWidth );

		//////////////////
		// When the user wants to close the window, just hide it.
		//////////////////
		if ( m_eMode != kWithRulesetTab )
		{
			// Hide if not stand alone
			ShowWindow( SW_HIDE );
		}
		else
		{
			// if Stand alone close
			CDialog::OnCancel();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04881")
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnButtonExecute() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CWaitCursor wait;

		// Disable the tool bar buttons while executing rules
		updateButtonStates(true);

		// if using the ruleset tab load the current file listed
		if ( m_eMode == kWithRulesetTab )
		{
			string strFileName = m_testerDlgRulesetPage.getRulesFileName();
			if ( strFileName != "" )
			{
				m_ipRuleSet->LoadFrom( strFileName.c_str(), VARIANT_FALSE);
			}
			else
			{
				throw UCLIDException("ELI08822", "No Rules File selected." );
			}
		}

		// get the current text from the input property page
		// also get the current source document name
		ISpatialStringPtr ipInputText = m_testerDlgInputPage.getText();

		// Redraw the window to update button states and to display new text if the input file has
		// changed since the text was last loaded.
		RedrawWindow();

		// if no input text is available, clear the results grid, and return
		// Allow processing even if ipInputText->IsEmpty() == VARIANT_TRUE
		// [FlexIDSCore #3716]
		if (ipInputText == NULL)
		{
			OnButtonClear();
			return;
		}

		// The third argument to FindAttributesInText() below should not be
		// NULL when we are just testing the rules for the current attribute.
		// When testing just the current attribute, a variant vector
		// must be created with that attribute's name and this vector
		// should be passed in as the third argument.
		IVariantVectorPtr ipVecAttributeNames;
		if (!m_testerDlgSettingsPage.isAllAttributesScopeSet() && m_eMode != kWithRulesetTab )
		{
			ipVecAttributeNames.CreateInstance(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI04811", ipVecAttributeNames != NULL);

			const string& strAttributeName = 
				m_testerDlgSettingsPage.getCurrentAttributeName();
			ipVecAttributeNames->PushBack(get_bstr_t(strAttributeName.c_str()));
		}

		// sometimes, the search may cause the program the crash.  This
		// can happen if one or more of the components are poorly written.
		// The user may not have saved the ruleset...save the ruleset in
		// a temporary file so that it can be recovered in case of a crash
		// NOTE: we are passing VARIANT_FALSE as the second argument
		// here because we don't want the internal dirty flag to be
		// effected by this SaveTo() call.

		if (m_pFRM != NULL && m_ipRuleSet->CanSave == VARIANT_TRUE)
		{
			m_ipRuleSet->SaveTo(get_bstr_t(m_pFRM->getRecoveryFileName().c_str()), VARIANT_FALSE);
		}

		// make a copy of the input text in case the string will be
		// modified by the following rules
		ICopyableObjectPtr ipCopyObj(ipInputText);
		ASSERT_RESOURCE_ALLOCATION("ELI18406", ipCopyObj != NULL);
		ISpatialStringPtr ipCopyInputText = ipCopyObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI06671", ipCopyInputText != NULL);
		UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI18407", ipAFDoc != NULL);
		ipAFDoc->Text = ipCopyInputText;

		// Use temporary Vector to hold the attributes that are currently being displayed
		IIUnknownVectorPtr ipTmpAttrs(NULL);
		if ( m_ipAttributes != NULL )
		{
			ipTmpAttrs.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI09754" , ipTmpAttrs != NULL );
			// Temporarly hold the attributes for the last run;
			ipTmpAttrs->Append( m_ipAttributes );
		}

		// make a copy of the rule set to send to the attribute finder engine
		// if the rule set is not encrypted [p16 #2678]
		UCLID_AFCORELib::IRuleSetPtr ipTempRuleSet;
		if (!asCppBool(m_ipRuleSet->IsEncrypted))
		{
			ICopyableObjectPtr ipCopy(m_ipRuleSet);
			ASSERT_RESOURCE_ALLOCATION("ELI19588", ipCopy != NULL);

			ipTempRuleSet = ipCopy->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI19589", ipTempRuleSet != NULL);
		}
		else
		{
			ipTempRuleSet = m_ipRuleSet;
		}

		// execute the search
		IUnknownPtr ipUnknown = ipTempRuleSet;
		_variant_t varRuleSet = (IUnknown *) ipUnknown;

		m_ipAttributes = getAFEngine()->FindAttributes( ipAFDoc, get_bstr_t( "" ), 
			-1, varRuleSet, ipVecAttributeNames, VARIANT_TRUE, NULL );

		// Refresh the list to display latest results
		updateList(ipAFDoc);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04726");

	try
	{
		// Update the button states (perform update outside of first try catch block
		// so that buttons will be re-enabled even if an exception occurred)
		updateButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25562");
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnButtonClear() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Set no highlight flag
		m_bNoHighlight = true;
		// Clear the list
		m_wndTreeList.m_tree.DeleteAllItems();
		// Reset no highlight flag
		m_bNoHighlight = false;

		// clear attributes
		if (m_ipAttributes)
		{
			m_ipAttributes->Clear();
		}

		updateButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04888")
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnDropFiles(HDROP hDropInfo)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// setup hDropInfo to automatically be released when we go out of scope
		DragDropFinisher finisher(hDropInfo);

		unsigned int iNumFiles = DragQueryFile(hDropInfo, 0xFFFFFFFF, NULL, NULL);
		for (unsigned int ui = 0; ui < iNumFiles; ui++)
		{
			// get the full path to the dragged filename
			char pszFile[MAX_PATH + 1];
			DragQueryFile(hDropInfo, ui, pszFile, MAX_PATH);

			// process the dropped file
			processDroppedFile(pszFile);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19142");
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CDialog::OnSize(nType, cx, cy);
		
		doResize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04885")
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Minimum width to allow display of buttons
		lpMMI->ptMinTrackSize.x = m_propPageMinSize.cx + 2 * 
			GetSystemMetrics(SM_CYSIZEFRAME);

		// Minimum height to allow display of list
		lpMMI->ptMinTrackSize.y = m_propPageMinSize.cy + 
			giMIN_LISTBOX_PLUS_TOOLBAR_HEIGHT + 
			GetSystemMetrics(SM_CXSIZEFRAME) + GetSystemMetrics(SM_CYCAPTION) + 1;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04886")
}
//-------------------------------------------------------------------------------------------------
BOOL RuleTesterDlg::PreTranslateMessage(MSG* pMsg) 
{
	// translate accelerators
	static HACCEL hAccel = LoadAccelerators(_Module.m_hInst, 
		MAKEINTRESOURCE(IDR_ACCELERATOR_TESTERDLG));
	if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
	{
		// since the message has been handled, no further dispatch is needed
		return TRUE;
	}

	// make sure the tool tip control is a valid window before passing messages to it
	if (asCppBool(::IsWindow(m_ToolTipCtrl.m_hWnd)))
	{
		// show tooltips
		m_ToolTipCtrl.RelayEvent(pMsg);
	}
	
	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnPaint() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPaintDC dc(this); // device context for painting
		
		// get the toolbar height and the dialog width
		CRect rectDlg;
		GetWindowRect(&rectDlg);
		CRect rectToolBar;
		m_apToolBar->GetWindowRect(&rectToolBar);
		int iToolBarHeight = rectToolBar.Height();
		int iDialogWidth = rectDlg.Width();
		
		// with gray and white pens, draw horizontal lines that span the entire width
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04887")
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnButtonSRIR() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// bring up a new instance of the spot recognition window
		createNewSRIRWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04759")
}
//-------------------------------------------------------------------------------------------------
BOOL RuleTesterDlg::OnToolTipNotify(UINT id, NMHDR * pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

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
void RuleTesterDlg::OnButtonFeedback() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Get Feedback Manager from Attribute Finder Engine
		UCLID_AFCORELib::IFeedbackMgrPtr ipFeedback = getAFEngine()->FeedbackManager;
		ASSERT_RESOURCE_ALLOCATION("ELI09056", ipFeedback != NULL);

		// Provide modified results to the Feedback Manager
		ipFeedback->RecordCorrectData(get_bstr_t(m_strRuleID.c_str()), m_ipAttributes);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08794")
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnButtonVoa() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Create a File - Save dialog
		CFileDialogEx dlgSave( FALSE, ".voa", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_OVERWRITEPROMPT,
			"VOA files (*.voa)|*.voa||", this );

		// Modify the initial directory
		string strTemp = ma_pCfgTesterMgr->getLastFileSaveDirectory();
		dlgSave.m_ofn.lpstrInitialDir = strTemp.c_str();

		// Only continue if user actually selected a file
		if (dlgSave.DoModal() == IDOK)
		{
			CString zFileName = dlgSave.GetPathName();

			// Save the Attributes to the file
			m_ipAttributes->SaveTo( get_bstr_t( LPCTSTR(zFileName) ), VARIANT_TRUE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08795")
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnButtonEnableSRIR() 
{
	m_bNoSpotRecognition = !m_bNoSpotRecognition;
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::OnButtonAbout() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Display the About box with version information
		CAFAboutDlg dlgAbout( kRuleTesterHelpAbout, "", getAFEngine() );
		dlgAbout.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14545")
}

//-------------------------------------------------------------------------------------------------
// IUnknown
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) RuleTesterDlg::AddRef()
{
	InterlockedIncrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) RuleTesterDlg::Release()
{
	InterlockedDecrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP RuleTesterDlg::QueryInterface( REFIID iid, void FAR* FAR* ppvObj)
{
	IParagraphTextHandler *pTmp = this;

	if (iid == IID_IUnknown)
		*ppvObj = static_cast<IUnknown *>(pTmp);
	else if (iid == IID_IDispatch)
		*ppvObj = static_cast<IDispatch *>(pTmp);
	else if (iid == IID_IParagraphTextHandler)
		*ppvObj = static_cast<IParagraphTextHandler *>(this);
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
// IParagraphTextHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP RuleTesterDlg::raw_NotifyParagraphTextRecognized(
	ISpotRecognitionWindow *pSourceSRWindow, ISpatialString *pText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// make the input page the current property page
		m_propSheet.SetActivePage(&m_testerDlgInputPage);

		// get the spatial text
		ISpatialStringPtr ipText(pText);
		ASSERT_RESOURCE_ALLOCATION("ELI06546", ipText != NULL);

		m_testerDlgInputPage.notifyImageWindowInputReceived(ipText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04757")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP RuleTesterDlg::raw_GetPTHDescription(BSTR *pstrDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pstrDescription = _bstr_t("Send text to Rule Tester window").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04758")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP RuleTesterDlg::raw_IsPTHEnabled(VARIANT_BOOL *pbEnabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// this PTH is always enabled.
		*pbEnabled = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04756")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private / helper methods
//-------------------------------------------------------------------------------------------------
ISpotRecognitionWindowPtr RuleTesterDlg::openImageOrGDDFile(char *pszFile,
															bool bIsGDD)
{
	// the user dropped a file that is an image or a gdd file
	// so open it in the image window
	ISRIRUtilsPtr ipSRIRUtils(CLSID_SRIRUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI07998", ipSRIRUtils != NULL);

	// try to get access to any spot recognition windows
	// that may already have the specified image or gdd file open
	ISpotRecognitionWindowPtr ipSRIR;
	ipSRIR = ipSRIRUtils->GetSRIRWithImage(pszFile, getInputManager(),
		VARIANT_FALSE);
	
	// if the image is already open, just flash the window, display
	// an error message and exit.
	if (ipSRIR)
	{
		// flash the window
		IInputReceiverPtr ipIR = ipSRIR;
		ASSERT_RESOURCE_ALLOCATION("ELI08013", ipIR != NULL);
		flashWindow((HWND) ipIR->WindowHandle, true);

		// display error message
		string strMsg;
		if (bIsGDD)
		{
			strMsg = "The file \"";
			strMsg += pszFile;
			strMsg += "\" refers to an image that is already open.";
		}
		else
		{
			strMsg = "The image \"";
			strMsg += pszFile;
			strMsg += "\" is already open.";
		}
		strMsg += " Only one instance of an image can be open at any given time.";
		MessageBox(strMsg.c_str());
	}
	else
	{
		// the referenced image is not open
		// create a new window and open the specified file
		ISpotRecognitionWindowPtr ipSRIR = createNewSRIRWindow();
		if (bIsGDD)
		{
			ipSRIR->OpenGDDFile(pszFile);
		}
		else
		{
			ipSRIR->OpenImageFile(pszFile);
		}
	}

	// return the pointer to the window that has the file open
	return ipSRIR;
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::processDroppedFile(char *pszFile)
{
	// get the extension of the dropped file
	string strExt = getExtensionFromFullPath(pszFile, true);
	bool bIsGDD = strExt == ".gdd";

	// NOTE: if the extension is 3 digits, then assume it's an image
	// (Many customers store tif images with a 3 digit extension where
	// the three digits represent the number of pages in the image)

	if (isImageFileExtension(strExt) || bIsGDD || isThreeDigitExtension(strExt))
	{
		openImageOrGDDFile(pszFile, bIsGDD);
	}

	// notify the input property page that a file has been dragged-and-dropped
	m_testerDlgInputPage.inputFileChanged(pszFile);

	// Set the input page as the active page so that the user can see what the file contents are.
	m_propSheet.SetActivePage(&m_testerDlgInputPage);
}
//-------------------------------------------------------------------------------------------------
ISpotRecognitionWindowPtr RuleTesterDlg::createNewSRIRWindow()
{
	ISpotRecognitionWindowPtr ipSRIR;

	// create new spot recognition window
	long nID = getInputManager()->CreateNewInputReceiver("Image viewer");
	ipSRIR = getInputManager()->GetInputReceiver(nID);
	ASSERT_RESOURCE_ALLOCATION("ELI06555", ipSRIR != NULL);
	
	IIUnknownVectorPtr ipVecPTHs(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI04760", ipVecPTHs != NULL);
	
	// this tester dialog is a paragraph text handler
	IParagraphTextHandler *pPTH = this;
	ipVecPTHs->PushBack(pPTH);
	ipSRIR->SetParagraphTextHandlers(ipVecPTHs);

	return ipSRIR;
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::createToolBar()
{
	m_apToolBar = auto_ptr<CToolBar>(new CToolBar());
	if (m_apToolBar->CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
    | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) )
	{
		m_apToolBar->LoadToolBar(IDR_TESTERDLG_TOOLBAR);
	}

	m_apToolBar->SetBarStyle(m_apToolBar->GetBarStyle() |
		CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC);

	// must set TBSTYLE_TOOLTIPS here in order to get tool tips
	m_apToolBar->ModifyStyle(0, TBSTYLE_TOOLTIPS);

	m_apToolBar->SetButtonStyle( m_apToolBar->CommandToIndex(ID_BUTTON_KEY_SR), TBBS_CHECKBOX );

	// We need to resize the dialog to make room for control bars.
	// First, figure out how big the control bars are.
	CRect rcClientStart;
	CRect rcClientNow;
	GetClientRect(rcClientStart);
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST,
				   0, reposQuery, rcClientNow);

	// Save toolbar height for use in dialog resizing
	m_iBarHeight = rcClientNow.top;

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

	UpdateWindow();
	Invalidate();
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::updateList(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc) 
{
	// Set no highlight flag
	m_bNoHighlight = true;
	// Clear the Tree List
	m_wndTreeList.m_tree.DeleteAllItems();
	// Reset no highlight flag
	m_bNoHighlight = false;

	// Check whether the attributes should auto expand
	bool bAutoExpand = ma_pCfgTesterMgr->getAutoExpandAttributes();
	
	// Define root-level Tree List item
	HTREEITEM	hTop = NULL;

	//////////////////////////////
	// Display desired String Tags
	//////////////////////////////
	// Before any attributes in the grid, add a record 
	// to display the document probability, if any
	IStrToStrMapPtr ipStrMap = ipAFDoc->StringTags;
	ASSERT_RESOURCE_ALLOCATION("ELI20193", ipStrMap != NULL);

	long lStrMapSize = ipStrMap->Size;
	for (long i = 0; i < lStrMapSize; i++)
	{
		_bstr_t bstrKey;
		_bstr_t bstrValue;

		// Retrieve the next entry from the string map
		ipStrMap->GetKeyValue(i, bstrKey.GetAddress(), bstrValue.GetAddress());

		string strKey = asString(bstrKey);
		string strValue = asString(bstrValue);

		if (strKey == DOC_PROBABILITY)
		{
			// If this entry is a doc probability, convert it to a textual name (sure, probable, etc)
			strValue = docProbabilityAsText(strValue);
		}

		// Insert the string tag into the grid
		strKey = "<" + strKey + ">";
		hTop = addRootItem(strKey, strValue, "" );
	}

	// Display desired Object Tags
	IStrToObjectMapPtr ipObjMap = ipAFDoc->ObjectTags;
	ASSERT_RESOURCE_ALLOCATION("ELI20201", ipObjMap != NULL);

	long lObjMapSize = ipObjMap->Size;
	for (long i = 0; i < lObjMapSize; i++)
	{
		_bstr_t bstrKey;
		IUnknownPtr ipValue;

		// Retrieve the next entry from the string map
		ipObjMap->GetKeyValue(i, bstrKey.GetAddress(), &ipValue);

		string strKey = "<" + asString(bstrKey) + ">";

		// Attempt to cast the value as an IVariantVector
		IVariantVectorPtr ipVector(ipValue);
		if (ipVector)
		{
			// If successful, extact each value from the vector
			long lVecSize = ipVector->Size;
			for (long j = 0; j < lVecSize; j++)
			{
				// Convert the variant to a string and add it to the grid
				string strValue = asString(_bstr_t(ipVector->GetItem(j)));
				hTop = addRootItem(strKey, strValue, "");
			}
		}
		else
		{
			// If we could not convert the object to a vector, don't throw an exception, 
			// rather add gstrUNKNOWN_TYPE to go the grid
			hTop = addRootItem(strKey, gstrUNKNOWN_TYPE, "");
		}
	}

	/////////////////////
	// Display Attributes
	/////////////////////
	long nSize = m_ipAttributes->Size();
	string strKey("");
	string strValue("");
	string strType("");
	UCLID_AFCORELib::IAttributePtr ipAttr(NULL);
	for (long i = 0; i < nSize; i++)
	{
		// get individual attribute
		ipAttr = m_ipAttributes->At(i);
		if (ipAttr)
		{
			// Retrieve Value object
			ISpatialStringPtr ipValue = ipAttr->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15518", ipValue != NULL);

			// Retrieve Name, Value and Type strings
			strKey = asString(ipAttr->Name);
			strValue = asString(ipValue->String);
			strType = asString(ipAttr->Type);

			// Insert this Attribute as root-level item
			HTREEITEM hTmp = m_wndTreeList.m_tree.InsertItem( strKey.c_str(), 0, 0 );

			// Add Value text - converted to Normal format
			convertCppStringToNormalString( strValue );
			m_wndTreeList.m_tree.SetItemText( hTmp, 1, strValue.c_str() );

			// Add Type text, if present
			if (strType.length() > 0)
			{
				m_wndTreeList.m_tree.SetItemText( hTmp, 2, strType.c_str() );
			}

			// Set the Item Data
			m_wndTreeList.m_tree.SetItemData( hTmp, (DWORD)&(*ipAttr) );

			// Add any sub attributes
			addSubAttributes( ipAttr, hTmp );

			// If auto expand is true, expand the attribute
			if (bAutoExpand)
			{
				m_wndTreeList.m_tree.Expand(hTmp, TVE_EXPAND);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
string RuleTesterDlg::docProbabilityAsText(const string &strValueAsLong)
{
	long lValue = asLong(strValueAsLong);
	string strTextValue;

	// Convert integer to Value string
	switch (lValue)
	{
	case 3:
		strTextValue = "Sure";
		break;

	case 2:
		strTextValue = "Probable";
		break;

	case 1:
		strTextValue = "Maybe";
		break;

	case 0:
		strTextValue = "Zero";
		break;

	default:
		{
			CString	zTemp;
			zTemp.Format( "Unknown (%d)", lValue );
			strTextValue = (LPCTSTR) zTemp;
		}
		break;
	}

	return strTextValue;
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::addSubAttributes(UCLID_AFCORELib::IAttributePtr ipAttribute, HTREEITEM hItem)
{
	// Retrieve collection of sub-attributes
	IIUnknownVectorPtr	ipSubAttributes = ipAttribute->GetSubAttributes();
	long lCount = 0;
	if (ipSubAttributes != NULL)
	{
		lCount = ipSubAttributes->Size();
	}

	// Add each sub-attribute to list
	for (long i = 0; i < lCount; i++)
	{
		//////////////////////////////
		// Retrieve this sub-attribute
		//////////////////////////////
		UCLID_AFCORELib::IAttributePtr	ipThisSub = ipSubAttributes->At( i );
		ASSERT_RESOURCE_ALLOCATION("ELI26066", ipThisSub != NULL);

		// Retrieve Value object
		ISpatialStringPtr ipValue = ipThisSub->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15519", ipValue != NULL);

		// Get Name, Value (converted to Cpp-style), and Type strings
		string	strName = asString(ipThisSub->Name);

		string	strValue = asString(ipValue->String);
		convertCppStringToNormalString( strValue );

		string	strType = asString(ipThisSub->Type);

		////////////////////////
		// Insert this Attribute
		////////////////////////
		HTREEITEM hTmp = m_wndTreeList.m_tree.InsertItem( strName.c_str(), hItem, hItem );

		// Add Value text
		m_wndTreeList.m_tree.SetItemText( hTmp, 1, strValue.c_str() );

		// Add Type text, if present
		if (strType.length() > 0)
		{
			m_wndTreeList.m_tree.SetItemText( hTmp, 2, strType.c_str() );
		}

		// Set the Item Data
		m_wndTreeList.m_tree.SetItemData( hTmp, (DWORD)&(*ipThisSub) );

		//////////////////////////////
		// Add any grandchildren items
		//////////////////////////////
		addSubAttributes( ipThisSub, hTmp );
	}
}
//-------------------------------------------------------------------------------------------------
HTREEITEM RuleTesterDlg::addRootItem(const string& strName, const string& strValue,
									 const string& strType)
{
	// Default to NULL
	HTREEITEM hTmp = NULL;

	// Add Name text
	if (strName.length() > 0)
	{
		hTmp = m_wndTreeList.m_tree.InsertItem( strName.c_str(), TVI_ROOT, TVI_LAST );

		// Add Value text
		if (strValue.length() > 0)
		{
			m_wndTreeList.m_tree.SetItemText( hTmp, 1, strValue.c_str() );
		}

		// Add Type text
		if (strType.length() > 0)
		{
			m_wndTreeList.m_tree.SetItemText( hTmp, 2, strType.c_str() );
		}

		// Set Item Data to 0
		m_wndTreeList.m_tree.SetItemData( hTmp, 0 );
	}

	return hTmp;
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::setCurrentAttributeName(const string& strAttributeName)
{
	// update the UI to reflect the current attribute name
	m_testerDlgSettingsPage.setCurrentAttributeName(strAttributeName);
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::doResize()
{
	// if it's minimized, do nothing
	if (IsIconic())
	{
		return;
	}

	// Ensure that the dlg's controls are realized before moving them.
	if (m_bInitialized)		
	{
		// Get total dialog size
		CRect rectDlg;
		GetClientRect(&rectDlg);
		int i = GetSystemMetrics(SM_CYCAPTION);
		int k = GetSystemMetrics(SM_CXSIZEFRAME);

		// get the size of the toolbar control
		CRect rectToolBar;
		m_apToolBar->GetWindowRect(&rectToolBar);
		ScreenToClient(&rectToolBar);

		// Resize the splitter control
		CRect rectSplitter;
		m_splitterCtrl.GetWindowRect(&rectSplitter);
		ScreenToClient(&rectSplitter);

		long nMin = rectToolBar.Height() + m_propPageMinSize.cy;
		long nMax = rectDlg.bottom - (giMIN_LISTBOX_PLUS_TOOLBAR_HEIGHT - 
			rectToolBar.Height());
		if (rectSplitter.bottom > nMax)
		{
			rectSplitter.bottom = nMax;
			rectSplitter.top = nMax - 5;
		}
		rectSplitter.right = rectDlg.Width();
		m_splitterCtrl.SetRange(nMin, nMax);
		m_splitterCtrl.ResizeSplitter(rectSplitter);

		// Position the Tree List two pixels below the splitter and 
		// leave 1 additional pixel of space around rect
		m_splitterCtrl.GetWindowRect( &rectSplitter );
		ScreenToClient( &rectSplitter );
		CRect rectGridCtrl( 1, rectSplitter.bottom + 3,
			rectSplitter.Width() - 2, rectDlg.Height() - 4);
		GetDlgItem( IDC_TREE_LIST )->MoveWindow( &rectGridCtrl );

		// Adjust columns in Tree List
		m_wndTreeList.DoResize();

		// Resize the Picture control around the Grid
		CWnd*	pPicture = GetDlgItem( IDC_PICTURE );
		if (pPicture != NULL)
		{
			// Get the Tree List dimensions
			CRect	rectGrid;
			m_wndTreeList.GetWindowRect( rectGrid );

			// Adjust rect size
			ScreenToClient( rectGrid );
			rectGrid.left -= 1;
			rectGrid.top -= 1;
			rectGrid.right += 1;
			rectGrid.bottom += 1;

			// Set Picture size
			pPicture->MoveWindow( &rectGrid );
		}

		// Resize the property sheet
		CRect rectPropSheet;
		rectPropSheet.left = 0;
		// leave space below toolbar for lines from OnPaint()
		rectPropSheet.top = rectToolBar.bottom + 2;
		rectPropSheet.right = rectDlg.right - 5;
		rectPropSheet.bottom = rectSplitter.top - 1;
		m_propSheet.resize(rectPropSheet);
		m_nCurrentBottomOfPropPage = rectPropSheet.bottom;

		// Position the toolbar in the dialog
		RepositionBars( AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);
	}

	Invalidate();
	UpdateWindow();
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::editSelectedAttribute()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Get the selected Tree List item
		HTREEITEM	hItem = m_wndTreeList.m_tree.GetSelectedItem();
		if (hItem == NULL)
		{
			// This is not a Tree List item, just return
			return;
		}

		// Retrieve Item Data
		DWORD dwTemp = m_wndTreeList.m_tree.GetItemData( hItem );
		if (dwTemp == 0)
		{
			// This must be a document tag item, just return
			return;
		}

		// Retrieve Tree List strings
		string strName = m_wndTreeList.m_tree.GetItemText( hItem, 0 );
		string strValue = m_wndTreeList.m_tree.GetItemText( hItem, 1 );
		string strType = m_wndTreeList.m_tree.GetItemText( hItem, 2 );

		// Convert Value string to CPP-style format
		convertNormalStringToCppString( strValue );

		// Provide strings to Edit dialog
		CTesterDlgEdit dlgEdit( strName.c_str(), strValue.c_str(), strType.c_str() );
		if (dlgEdit.DoModal() == IDOK)
		{
			// Retrieve strings from dialog
			CString zName = dlgEdit.GetName();
			CString zValue = dlgEdit.GetValue();
			CString zType = dlgEdit.GetType();

			// Check for an update
			bool bUpdated = false;
			bool bValueUpdated = false;
			if (zName.Compare( strName.c_str() ) != 0)
			{
				bUpdated = true;
			}

			if (zValue.Compare( strValue.c_str() ) != 0)
			{
				bValueUpdated = true;
			}

			if (zType.Compare( strType.c_str() ) != 0)
			{
				bUpdated = true;
			}

			if (bUpdated || bValueUpdated)
			{
				// Retrieve Attribute
				IAttribute	*pAttr = (IAttribute *)dwTemp;
				UCLID_AFCORELib::IAttributePtr ipAttribute( pAttr );
				ASSERT_RESOURCE_ALLOCATION("ELI09254", ipAttribute != NULL);

				// Update Name and/or Type information
				if (bUpdated)
				{
					// Update Attribute items
					ipAttribute->PutName( get_bstr_t( zName ) );
					ipAttribute->PutType( get_bstr_t( zType ) );

					// Update Tree List items
					m_wndTreeList.m_tree.SetItemText( hItem, 0, zName );
					m_wndTreeList.m_tree.SetItemText( hItem, 2, zType );
				}

				// Update Value information
				if (bValueUpdated)
				{
					// Update SpatialString
					ISpatialStringPtr ipValue = ipAttribute->GetValue();
					ASSERT_RESOURCE_ALLOCATION("ELI09255", ipValue != NULL);
					ipValue->Replace(ipValue->String, get_bstr_t( LPCTSTR(zValue) ), VARIANT_FALSE, 
						0, NULL);

					// Convert new Value string to Normal format
					string strNewValue = LPCTSTR(zValue);
					convertCppStringToNormalString( strNewValue );

					// Update Tree List
					m_wndTreeList.m_tree.SetItemText( hItem, 1, strNewValue.c_str() );
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09386")
}
//-------------------------------------------------------------------------------------------------
UCLID_AFCORELib::IAttributeFinderEnginePtr RuleTesterDlg::getAFEngine()
{
	if (m_ipEngine == NULL)
	{
		m_ipEngine.CreateInstance(CLSID_AttributeFinderEngine);
		ASSERT_RESOURCE_ALLOCATION("ELI04738", m_ipEngine != NULL);
	}
	
	return m_ipEngine;
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::highlightAttributeInRow()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Get the selected Tree List item
		HTREEITEM	hItem = m_wndTreeList.m_tree.GetSelectedItem();

		// Retrieve Item Data
		DWORD dwTemp = m_wndTreeList.m_tree.GetItemData( hItem );
		if (dwTemp == 0)
		{
			// This must be a document tag item, just return
			return;
		}

		// Get pointer to associated Attribute
		IAttribute	*pAttr = (IAttribute *)dwTemp;
		UCLID_AFCORELib::IAttributePtr ipAttribute( pAttr );
		ASSERT_RESOURCE_ALLOCATION("ELI06567", ipAttribute != NULL);

		// get the spatial string representing the attribute value
		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI06568", ipValue != NULL);

		string strImage = ipValue->SourceDocName;
		

		ISRIRUtilsPtr ipSRIRUtils(CLSID_SRIRUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI06996", ipSRIRUtils != NULL);

		ISpotRecognitionWindowPtr ipSRIR(NULL);

		// if the value is not a spatial string, or if the spatial string
		// does not have a source document associated with it, then just
		// delete all highlights and return
		if (ipValue->HasSpatialInfo() == VARIANT_FALSE || strImage.empty())
		{
			if (!m_strCurHighlightImage.empty())
			{
				ipSRIR = ipSRIRUtils->GetSRIRWithImage(get_bstr_t(m_strCurHighlightImage.c_str()),
								getInputManager(), VARIANT_FALSE);
				if(ipSRIR)
				{
					ipSRIR->DeleteTemporaryHighlight();
				}
			}
			return;
		}
		
		ipSRIR = ipSRIRUtils->GetSRIRWithImage(ipValue->SourceDocName,
								getInputManager(), VARIANT_TRUE);
		m_strCurHighlightImage = strImage;
		
		if (ipSRIR)
		{
			// highlight the spatial string in the SRIR window
			ipSRIR->CreateTemporaryHighlight(ipValue);

			IIUnknownVectorPtr ipVecPTHs = ipSRIR->GetParagraphTextHandlers();
			if (ipVecPTHs == NULL)
			{
				ipVecPTHs.CreateInstance(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI06997", ipVecPTHs != NULL);
			}
			if (ipVecPTHs->Size() == 0)
			{
				// this tester dialog is a paragraph text handler
				IParagraphTextHandler *pPTH = this;
				ipVecPTHs->PushBack(pPTH);
				ipSRIR->SetParagraphTextHandlers(ipVecPTHs);
			}			
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09354")
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::prepareTreeList() 
{
	// Subclass the control
	m_wndTreeList.SubclassDlgItem( IDC_TREE_LIST, this );

	// Get total width of control
	CRect	rect;
	m_wndTreeList.GetWindowRect( &rect );

	// Prepare Column widths
	long lNameWidth = ma_pCfgTesterMgr->getNameColumnWidth();
	long lTypeWidth = ma_pCfgTesterMgr->getTypeColumnWidth();
	long lValueWidth = rect.Width() - lNameWidth - lTypeWidth;

	// Create three columns
	m_wndTreeList.m_tree.InsertColumn( 0, "Name", LVCFMT_LEFT, lNameWidth );
	m_wndTreeList.m_tree.InsertColumn( 1, "Value", LVCFMT_LEFT, lValueWidth );
	m_wndTreeList.m_tree.InsertColumn( 2, "Type", LVCFMT_LEFT, lTypeWidth );

	// Set Value column as stretchable
	m_wndTreeList.SetStretchColumn( 1 );
}
//-------------------------------------------------------------------------------------------------
void RuleTesterDlg::updateButtonStates(bool bRulesExecuting)
{
	try
	{
		// Set default enable/disable values for VOA and Feedback
		BOOL bEnableVOA = FALSE;
		BOOL bEnableFeedback = FALSE;

		// All others are enabled/disabled based on rule execution
		BOOL bEnableOthers = asMFCBool(!bRulesExecuting);

		// If the rules are not currently executing then enable/disable the
		// VOA and Feedback buttons based on the whether there are attributes
		// and a ruleID
		if (!bRulesExecuting)
		{
			if (m_ipAttributes != NULL && m_ipAttributes->Size() > 0)
			{
				bEnableVOA = TRUE;

				if (!m_strRuleID.empty())
				{
					bEnableFeedback = TRUE;
				}
			}
		}

		if (m_apToolBar.get())
		{
			// Enable/disable tool bar buttons
			m_apToolBar->GetToolBarCtrl().EnableButton(ID_BUTTON_SRIR, bEnableOthers);
			m_apToolBar->GetToolBarCtrl().EnableButton(ID_BUTTON_CLEAR, bEnableOthers);
			m_apToolBar->GetToolBarCtrl().EnableButton(ID_BUTTON_EXECUTE, bEnableOthers);
			m_apToolBar->GetToolBarCtrl().EnableButton(ID_BUTTON_KEY_SR, bEnableOthers);
			m_apToolBar->GetToolBarCtrl().EnableButton(ID_BUTTON_ABOUT, bEnableOthers);
			m_apToolBar->GetToolBarCtrl().EnableButton(ID_BUTTON_VOA, bEnableVOA);
			m_apToolBar->GetToolBarCtrl().EnableButton(ID_BUTTON_FEEDBACK, bEnableFeedback);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25563");
}
//-------------------------------------------------------------------------------------------------
IInputManagerPtr RuleTesterDlg::getInputManager()
{
	IInputManagerSingletonPtr ipInputMgrSingleton(CLSID_InputManagerSingleton);
	ASSERT_RESOURCE_ALLOCATION("ELI10376", ipInputMgrSingleton != NULL);
	return ipInputMgrSingleton->GetInstance();
}
//-------------------------------------------------------------------------------------------------