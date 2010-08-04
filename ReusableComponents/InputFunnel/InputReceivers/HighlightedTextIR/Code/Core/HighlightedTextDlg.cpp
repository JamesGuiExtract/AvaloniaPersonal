//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HighlightedTextDlg.cpp
//
// PURPOSE:	Implementation of HighlightedTextDlg class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "resource.h"
#include "HighlightedTextDlg.h"
#include "HTCategories.h"

#include <RegistryPersistenceMgr.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <MRUList.h>
#include <RegConstants.h>

#include <afxcmn.h>				// For CToolTipCtrl
#include <algorithm>
#include <io.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-----------------------------------------------------------------------------------------------99
//-----------------------------------------------------------------------------------------------101
// G L O B A L / S T A T I C   V A R I A B L E S
//--------------------------------------------------------------------------------------------------
vector<HighlightedTextDlg *> HighlightedTextDlg::ms_vecInstances;
const COLORREF HighlightedTextDlg::ms_USED_ENTITY_COLOR = RGB(255, 192, 192);
const COLORREF HighlightedTextDlg::ms_UNUSED_ENTITY_COLOR = RGB(255, 255, 0);
IStrToStrMapPtr HighlightedTextDlg::ms_mapInputFinderNameToProgId;

//--------------------------------------------------------------------------------------------------
// HighlightedTextDlg dialog
//--------------------------------------------------------------------------------------------------
HighlightedTextDlg::HighlightedTextDlg(IInputEntityManager *pInputEntityManager, 
									   CWnd* pParent /*=NULL*/)
	: CDialog(HighlightedTextDlg::IDD, pParent),
	m_pInputEntityManager(pInputEntityManager),
	m_pParent(pParent),
	m_iStatusHeight(18),
	m_iBarHeight(0),
	m_bTextEnabled(false),
	m_bPersistentSource(false),
	m_bInitialized(false),
	m_strIndirectSourceName("")
{
	//{{AFX_DATA_INIT(HighlightedTextDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT

	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
		m_hIcon = AfxGetApp()->LoadIcon( IDI_TVDLG_ICON );

		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, 
			gstrREG_ROOT_KEY + "\\InputFunnel\\InputReceivers"));

		ma_pCfgHTIRMgr = auto_ptr<ConfigManager>(new ConfigManager(ma_pUserCfgMgr.get(), "\\HighlightedTextIR"));
		// Create the MRU List object
		ma_pRecentFiles = auto_ptr<MRUList>(new MRUList(ma_pUserCfgMgr.get(), 
				"\\HighlightedTextIR\\MRUList", "File_%d", 8 ));
		
		// Add this instance to the list of currently running instances
		ms_vecInstances.push_back( this );
		
		// as a one-time operation, load the description and progids of
		// inputfinders
		if (ms_mapInputFinderNameToProgId == NULL)
		{
			// create an instance of the category manager
			ICategoryManagerPtr ipCatMgr(CLSID_CategoryManager);
			if (ipCatMgr == NULL)
			{
				throw UCLIDException("ELI04294", "Unable to create instance of Category Manager!");
			}

			// get the description to progid map for input finders from the
			// category manager
			ms_mapInputFinderNameToProgId = ipCatMgr->GetDescriptionToProgIDMap1(
				_bstr_t(HT_IF_CATEGORYNAME.c_str()));

			if (ms_mapInputFinderNameToProgId == NULL)
			{
				UCLIDException ue("ELI04295", "Unable to retrieve components registered in specified category!");
				ue.addDebugInfo("Category", HT_IF_CATEGORYNAME);
				throw ue;
			}
		}

		// create instances of the input finder objects
		if (ms_mapInputFinderNameToProgId->Size > 0)
		{
			IVariantVectorPtr ipIFDescriptions = ms_mapInputFinderNameToProgId->GetKeys();

			int iNumInputFinders = ipIFDescriptions->Size;
			for (int i = 0; i < iNumInputFinders; i++)
			{
				_bstr_t _bstrDescription = ipIFDescriptions->GetItem(i);
				string stdstrDescription = _bstrDescription;
				_bstr_t _bstrProgID = ms_mapInputFinderNameToProgId->GetValue(
					_bstrDescription);
				string stdstrProgID = _bstrProgID;

				IInputFinderPtr ipInputFinder(stdstrProgID.c_str());
				if (ipInputFinder == NULL)
				{
					UCLIDException ue("ELI04299", "Unable to create Input Finder object!");
					ue.addDebugInfo("InputFinder name", stdstrDescription);
					ue.addDebugInfo("ProgID", stdstrProgID);
					throw ue;
				}

				m_mapIFNameToObj[stdstrDescription] = ipInputFinder;
			}
		}

		m_nTPOptionSpanLen = ID_ENTIRE_TEXT_02 - ID_ENTIRE_TEXT_01;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03187")
}
//--------------------------------------------------------------------------------------------------
HighlightedTextDlg::~HighlightedTextDlg()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{		
		// Remove this instance from the list of currently running instances
		ms_vecInstances.erase( find( ms_vecInstances.begin(), 
			ms_vecInstances.end(), this ) );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03188")	
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(HighlightedTextDlg)
	DDX_Control(pDX, IDC_UCLIDMCRTEXTVIEWERCTRL1, m_TV);
	//}}AFX_DATA_MAP
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(HighlightedTextDlg, CDialog)
	//{{AFX_MSG_MAP(HighlightedTextDlg)
	ON_WM_SIZE()
	ON_COMMAND(ID_APPEND, OnAppend)
	ON_COMMAND(ID_EDIT_DECREASE, OnEditDecrease)
	ON_COMMAND(ID_EDIT_INCREASE, OnEditIncrease)
	ON_COMMAND(ID_EDIT_PASTE, OnEditPaste)
	ON_COMMAND(ID_EDIT_COPY, OnEditCopy)
	ON_COMMAND(ID_FILE_NEW, OnFileNew)
	ON_COMMAND(ID_FILE_OPEN, OnFileOpen)
	ON_COMMAND(ID_FILE_PRINT, OnFilePrint)
	ON_COMMAND(ID_FILE_SAVE, OnFileSave)
	ON_WM_CLOSE()
	ON_NOTIFY(TBN_DROPDOWN, AFX_IDW_TOOLBAR, OnToolbarDropDown)
	ON_COMMAND(ID_BTN_InputFinder, OnBTNInputFinder)
	ON_COMMAND(ID_PROCESS_TEXT, OnProcessText)
	//}}AFX_MSG_MAP
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTW, 0, 0xFFFF, OnToolTipText)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTA, 0, 0xFFFF, OnToolTipText)
	ON_COMMAND_RANGE(ID_MRU_FILE1, ID_MRU_FILE8, OnSelectMRUPopupMenu)
	ON_COMMAND_RANGE(ID_MNU_Finder1, ID_MNU_Finder10, OnSelectInputFinderPopupMenu)
	ON_COMMAND_RANGE(ID_ENTIRE_TEXT_01, ID_MAX_TP-1, OnTPMenuItemSelected)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
BEGIN_EVENTSINK_MAP(HighlightedTextDlg, CDialog)
    //{{AFX_EVENTSINK_MAP(HighlightedTextDlg)
	ON_EVENT(HighlightedTextDlg, IDC_UCLIDMCRTEXTVIEWERCTRL1, 1 /* TextSelected */, OnTextSelected, VTS_I4)
	ON_EVENT(HighlightedTextDlg, IDC_UCLIDMCRTEXTVIEWERCTRL1, 2 /* SelectedText */, OnSelectedText, VTS_BSTR)
	//}}AFX_EVENTSINK_MAP
END_EVENTSINK_MAP()

//--------------------------------------------------------------------------------------------------
// HighlightedTextDlg message handlers
//--------------------------------------------------------------------------------------------------
BOOL HighlightedTextDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CDialog::OnInitDialog();
		
		// Set the icon for the dialog
		SetIcon( m_hIcon, TRUE );			// Set big icon
		SetIcon( m_hIcon, FALSE );			// Set small icon
				
		// Create and setup the toolbar
		createToolBar();
		
		// Create the tooltip control
		ma_pToolTipCtrl = auto_ptr<CToolTipCtrl>(new CToolTipCtrl);
		ma_pToolTipCtrl->Create( this, TTS_ALWAYSTIP );
		
		// Prepare and create the single-part status bar
		CRect	rect;
		m_statusBar.Create( this );
		
		// Determine status bar height
		m_statusBar.GetClientRect( &rect );
		if (rect.Height() > 0)
		{
			m_iStatusHeight = rect.Height();
		}
		m_statusBar.GetStatusBarCtrl().SetMinHeight( m_iStatusHeight );
		
		// Set flag indicating that controls exist
		m_bInitialized = true;
		
		if (ms_vecInstances.size() == 1)
		{
			// Retrieve persistent size and position of dialog
			long	lLeft = 0;
			long	lTop = 0;
			long	lWidth = 0;
			long	lHeight = 0;
			ma_pCfgHTIRMgr->getWindowPos( lLeft, lTop );
			ma_pCfgHTIRMgr->getWindowSize( lWidth, lHeight );
			
			// Adjust window position based on retrieved settings
			MoveWindow( lLeft, lTop, lWidth, lHeight, TRUE );
		}

		// Restore text size
		string	strSize = ma_pCfgHTIRMgr->getTextSizeInTwips();
		long	lSize = asLong( strSize );
		if (lSize > 0)
		{
			m_TV.setTextSize( lSize );
		}
		
		// And force a resize call to adjust OCX
		resizeItems();
		
		// Clear the status bar text
		setStatusText( "" );
		
		// Set the default title
		updateTitle();

		// update state of toolbar buttons
		updateStateOfProcessTextButton();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03004")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::updateStateOfProcessTextButton()
{
	if (m_ipTextProcessors == NULL)
	{
		m_toolBar.GetToolBarCtrl().EnableButton(ID_PROCESS_TEXT, FALSE);
	}
	else if (m_ipTextProcessors->Size() > 0)
	{
		m_toolBar.GetToolBarCtrl().EnableButton(ID_PROCESS_TEXT, TRUE);
	}
}
//--------------------------------------------------------------------------------------------------
BOOL HighlightedTextDlg::Create(UINT nIDTemplate, CWnd* pParentWnd) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	return CDialog::Create(nIDTemplate, pParentWnd);
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	CDialog::OnSize(nType, cx, cy);
	
	// Move controls around
	resizeItems();
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnFinalRelease() 
{
	// When the last reference for an automation object is released
	// OnFinalRelease is called.  The base class will automatically
	// deletes the object.  Add additional cleanup required for your
	// object before calling the base class.

	CDialog::OnFinalRelease();
}
//--------------------------------------------------------------------------------------------------
BOOL HighlightedTextDlg::OnToolTipText(UINT id, NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

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
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnSelectMRUPopupMenu(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{	
		if (nID >= ID_MRU_FILE1 && nID <= ID_MRU_FILE8)
		{
			// Check if the current contents have been saved?
			if (m_TV.isModified())
			{
				// Present MessageBox to user
				int iRes = MessageBox("Do you want to save changes you made to this document?", 
					"Confirmation", MB_ICONQUESTION | MB_YESNOCANCEL);
				
				if (iRes == IDCANCEL)
				{
					// User does not want to continue with this operation.
					return;
				}
				else if (iRes == IDYES)
				{
					// Save the document
					Save();
				}
			}

			// Get the current selected file index of MRU list
			int nCurrentSelectedFileIndex = nID - ID_MRU_FILE1;

			// Get file name string
			string strFileToOpen(ma_pRecentFiles->at( nCurrentSelectedFileIndex));

			// Open the file
			Open( strFileToOpen );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03206")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnSelectInputFinderPopupMenu(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{	
		if (nID >= ID_MNU_Finder1 && nID <= ID_MNU_Finder10)
		{
			// just double check to make sure that the index of the
			// the selected entry in the map is valid.
			unsigned long ulIndex = nID - ID_MNU_Finder1;
			if (!(ulIndex < m_mapIFNameToObj.size()))
			{
				UCLIDException ue("ELI04306", "Invalid selection!");
				ue.addDebugInfo("ulIndex", ulIndex);
				ue.addDebugInfo("MapSize", m_mapIFNameToObj.size());
				throw ue;
			}

			// Determine name of chosen Input Finder
			CString	zFinderName;
			map<string, IInputFinderPtr>::const_iterator iter;
			iter = m_mapIFNameToObj.begin();
			for (unsigned int ui = 0; ui < ulIndex; ui++)
			{
				iter++;
			}

			// Use the new finder
			SetInputFinder(iter->first);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03284")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnToolbarDropDown(NMHDR* pNMHDR, LRESULT *plr)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{	
		NMTOOLBAR *pTB = (NMTOOLBAR *)pNMHDR;
		UINT nID = pTB->iItem;
		
		// Switch on button command id's
		switch (nID)
		{
		case ID_FILE_OPEN:
			{
				// Load the popup MRU file list menu
				CMenu menuLoader;
				if (!menuLoader.LoadMenu(IDR_MENU_MRU))
				{
					throw UCLIDException("ELI03208", 
						"Failed to load Most Recently Used File list.");
				}
				
				CMenu* pPopup = menuLoader.GetSubMenu(0);
				if (pPopup)
				{
					ma_pRecentFiles->readFromPersistentStore();
					int nSize = ma_pRecentFiles->getCurrentListSize();
					if (nSize > 0)
					{
						// remove the "No File" item from the menu
						pPopup->RemoveMenu( ID_MNU_MRU, MF_BYCOMMAND );
					}

					// Add each filename to the menu for mru list
					for (int i = nSize - 1; i >= 0 ; i--)
					{
						CString pszFile( ma_pRecentFiles->at(i).c_str() );

						if (!pszFile.IsEmpty())
						{
							pPopup->InsertMenu( 0, MF_BYPOSITION, 
								ID_MRU_FILE1 + i, pszFile );
						}
					}

					CRect rc;
					m_toolBar.SendMessage( TB_GETRECT, pTB->iItem, (LPARAM)&rc );
					m_toolBar.ClientToScreen( &rc );
					
					pPopup->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
											rc.left, rc.bottom, this, &rc );
				}
			}
			break;

		default:
			throw UCLIDException("ELI03209", 
				"Unknown or unexpected dropdown button.");
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03207")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnClose() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{		
		int iResponse = 0;
		if (m_TV.isModified())
		{
			iResponse = MessageBox("Do you want to save changes you made to this document?", 
				"Confirmation", MB_YESNOCANCEL | MB_ICONQUESTION );
			
			if (iResponse == IDYES)
			{
				OnFileSave();
			}
			else if (iResponse == IDCANCEL)
			{
				// No save, no close, just return
				return;
			}
		}

		// remember window positions
		WINDOWPLACEMENT windowPlacement;
		if (GetWindowPlacement(&windowPlacement) != 0)
		{
			RECT	rect;
			rect = windowPlacement.rcNormalPosition;
			
			ma_pCfgHTIRMgr->setWindowPos( rect.left, rect.top );
			ma_pCfgHTIRMgr->setWindowSize( rect.right - rect.left, rect.bottom - rect.top );
		}

		// Save text size
		long lSize = m_TV.getTextSize();
		string	strSize = asString( lSize );
		ma_pCfgHTIRMgr->setTextSizeInTwips( strSize );

		// When the user wants to close the window, send a message to the event 
		// handler telling it that the user wants to close the window.  Do not 
		// actually close the IR window.  The Window will automatically disappear 
		// when this dialog object is destructed by the owning ATL object.
		if (m_ipEventHandler)
		{
			IInputReceiverPtr ipReceiver = m_pInputEntityManager;
			m_ipEventHandler->NotifyAboutToDestroy( ipReceiver );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03003")
}

//--------------------------------------------------------------------------------------------------
// Public HighlightedTextIR methods
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::AddInputFinder(std::string strInputFinderName, 
										IInputFinder* ipInputFinder)
{
	if (ipInputFinder)
	{
		// make sure the input finder with same name doesn't exist yet
		std::map<std::string, IInputFinderPtr>::iterator iter;
		iter = m_mapIFNameToObj.find(strInputFinderName);
		if (iter != m_mapIFNameToObj.end())
		{
			UCLIDException ue("ELI04296", "An input finder has already been registered with the specified name!");
			ue.addDebugInfo("InputFinderName", strInputFinderName);
			throw ue;
		}		

		// add the input finder entry to the map
		m_mapIFNameToObj[strInputFinderName] = ipInputFinder;
	}
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::Clear() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	// Remove text
	m_TV.clear();
	
	// Parse the text using the current Finder
	// This will just clear the internal data structures
	parseText();
	
	// Reset flag
	m_bPersistentSource = false;
	
	// Reset indirect source name
	m_strIndirectSourceName = "";
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::DisableText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// Disable text selection
	m_TV.enableTextSelection( 0 );
	m_bTextEnabled = false;
	
	// Clear status bar text
	setStatusText( "" );
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::EnableText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	// Enable text selection
	m_TV.enableTextSelection( 1 );
	m_bTextEnabled = true;
	
	// Set status bar text
	setStatusText( "Enable Text Selection" );
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::EnableText(const std::string& strPrompt) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	// Enable text selection
	m_TV.enableTextSelection( 1 );
	m_bTextEnabled = true;
	
	// Set status bar text
	setStatusText( strPrompt );
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::Open(const std::string& strFilename) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	try
	{
		CWaitCursor wait;

		// Call the control class method to open the file
		m_TV.open( strFilename.c_str() );	
		
		// Parse the text using the current Finder
		parseText();
		
		// Set flag
		m_bPersistentSource = true;
		
		// Store the path information
		CString	zFilePath = strFilename.c_str();
		int iSlash = zFilePath.ReverseFind( '\\' );
		zFilePath = zFilePath.Left( iSlash );
		ma_pCfgHTIRMgr->setLastFileOpenDirectory( zFilePath.operator LPCTSTR() );
		
		// Reset the title bar text
		updateTitle();
		
		// Reset indirect source name
		m_strIndirectSourceName = "";
		
		// Add currently opend file to the mru list
		addFileToMRUList(strFilename);
	}
	catch (...)
	{
		removeFileFromMRUList(strFilename);
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::Save() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// Call the control class method to save the file
	m_TV.save();	
	
	// Set flag (source is at least persistent NOW)
	m_bPersistentSource = true;
	
	// Store the path information
	CString	zFilePath = m_TV.getFileName();
	int iSlash = zFilePath.ReverseFind( '\\' );
	zFilePath = zFilePath.Left( iSlash );
	ma_pCfgHTIRMgr->setLastFileOpenDirectory( zFilePath.operator LPCTSTR() );
	
	// Reset the title bar text
	updateTitle();
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::SaveAs(const std::string& strFilename) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// Call the control class method to save the file
	m_TV.saveAs( strFilename.c_str() );	

	addFileToMRUList( strFilename );
	
	// Set flag (source is at least persistent NOW)
	m_bPersistentSource = true;
	
	// Store the path information
	CString	zFilePath = strFilename.c_str();
	int iSlash = zFilePath.ReverseFind( '\\' );
	zFilePath = zFilePath.Left( iSlash );
	ma_pCfgHTIRMgr->setLastFileOpenDirectory( zFilePath.operator LPCTSTR() );	
	
	// Reset the title bar text
	updateTitle();
}
//--------------------------------------------------------------------------------------------------
std::string HighlightedTextDlg::GetEntityText(long lEntityID) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	string	strEntity = "";
	
	// Validate the entity ID
	if (IsEntityValid( lEntityID ))
	{
		// Call the control class method to retrieve the text
		CString	zText = m_TV.getEntityText( lEntityID );
		
		// Copy to std::string
		strEntity = zText.operator LPCTSTR();
	}
	
	// Provide entity text or empty string
	return strEntity;
}
//--------------------------------------------------------------------------------------------------
std::string HighlightedTextDlg::GetFinderName() 
{
	// Provide string to caller
	return m_strFinderName;
}
//--------------------------------------------------------------------------------------------------
std::string HighlightedTextDlg::GetSourceName() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	string	strSourceName = "";
	
	// Check for source
	if (m_bPersistentSource)
	{
		// Call the control class method to retrieve the text
		CString	zName;
		zName = m_TV.getFileName();
		
		// Copy to std::string
		strSourceName = zName.operator LPCTSTR();
	}
	
	// Provide string to caller
	return strSourceName;
}
//--------------------------------------------------------------------------------------------------
std::string HighlightedTextDlg::GetText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	string	strText = "";
	
	// Call the control class method to retrieve the text
	CString	zText;
	zText = m_TV.getText();
	
	// Copy to std::string
	strText = zText.operator LPCTSTR();
	
	// Provide string to caller
	return strText;
}
//--------------------------------------------------------------------------------------------------
bool HighlightedTextDlg::IsEntityValid(long lEntityID) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// Check the control
	return (m_TV.isEntityIDValid( lEntityID )) ? true : false;
}
//--------------------------------------------------------------------------------------------------
bool HighlightedTextDlg::IsInputEnabled() 
{
	// Return state variable
	return m_bTextEnabled;
}
//--------------------------------------------------------------------------------------------------
bool HighlightedTextDlg::IsMarkedAsUsed(long lEntityID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// Validate the entity ID
	if (IsEntityValid( lEntityID ))
	{
		// An entity is marked as unused as long as its color is not the color
		// of n used entity.
		COLORREF entityColor = m_TV.getEntityColor( lEntityID );
		if (entityColor == ms_USED_ENTITY_COLOR)
		{
			return true;
		}
		else 
		{
			return false;
		}
	}
	else
	{
		// Throw exception - Cannot get used status for invalid entity ID
		UCLIDException ue( "ELI02782", 
			"Cannot call IsMarkedAsUsed() with an invalid ID." );
		ue.addDebugInfo( "Entity ID", lEntityID );
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::MarkAsUsed(long lEntityID, bool bMarkAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	// Validate the entity ID
	if (IsEntityValid( lEntityID ))
	{
		if (bMarkAsUsed)
		{
			// Set as used
			m_TV.setTextHighlightColor( lEntityID, ms_USED_ENTITY_COLOR );
		}
		else
		{
			// Set as unused
			m_TV.setTextHighlightColor( lEntityID, ms_UNUSED_ENTITY_COLOR);
		}
	}
	else
	{
		// Throw exception - Cannot mark as used for invalid entity ID
		UCLIDException ue( "ELI02783", 
			"Cannot MarkAsUsed() with an invalid ID." );
		ue.addDebugInfo( "Entity ID", lEntityID );
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
bool HighlightedTextDlg::IsModified() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// Check the control
	return (m_TV.isModified()) ? true : false;
}
//--------------------------------------------------------------------------------------------------
bool HighlightedTextDlg::IsPersistentSource() 
{
	// Check the flag
	return m_bPersistentSource;
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::SetEntityText(long lEntityID, const string& strNewText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// Confirm ID is valid
	if (m_TV.isEntityIDValid( lEntityID ))
	{
		// Call the control class method to set the text
		m_TV.setEntityText( lEntityID, strNewText.c_str() );	
		
		// Do not reparse the text - WEL 05/23/02
	}
	else
	{
		// Throw exception - Cannot set text for invalid entity ID
		UCLIDException ue( "ELI02780", 
			"Cannot SetEntityText() with an invalid ID." );
		ue.addDebugInfo( "Entity ID", lEntityID );
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::SetEventHandler(IIREventHandler* ipEventHandler)
{
	m_ipEventHandler = ipEventHandler;
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::SetInputFinder(const string& strNewName) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	map<std::string, IInputFinderPtr>::const_iterator iter;
	iter = m_mapIFNameToObj.find(strNewName);
	if (iter == m_mapIFNameToObj.end())
	{
		// the entry could not be found in the map.
		UCLIDException uclidException("ELI03286", "Failed to find specified input finder.");
		uclidException.addDebugInfo("strInputFinderName", strNewName);
		uclidException.addPossibleResolution("Please exit the application, delete InputFinders.lst file from the bin directory, and try it again.");
		throw uclidException;
	}

	// get the input finder object
	IInputFinderPtr ipNewInputFinder = m_mapIFNameToObj[strNewName];
	
	// Send the Finder to the OCX
	m_TV.setInputFinder( ipNewInputFinder );

	// update the internal name for the current input finder
	m_strFinderName = strNewName;

	// store the current selected input finder name
	ma_pCfgHTIRMgr->setLastInputFinderName(strNewName);
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::SetText(std::string strNewText) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	// Call the control class method to set the text
	m_TV.setText( strNewText.c_str() );	
	
	// Parse the text using the current Finder
	parseText();
	
	// Reset flag
	m_bPersistentSource = false;
	
	// Reset the title bar text
	updateTitle();
	
	// Reset indirect source name
	m_strIndirectSourceName = "";
}
//--------------------------------------------------------------------------------------------------
BOOL HighlightedTextDlg::ShowWindow(int nCmdShow) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (nCmdShow == SW_SHOW)
		{
			// set input finder for the current instance to be the same as the one
			// used last.
			string strInputFinderName = ma_pCfgHTIRMgr->getLastInputFinderName();
			
			// if there exists an entry for the last used input finder, and if
			// that input finder is currently available to us in our map of 
			// name-to-object, then set that as the input finder.
			if (strInputFinderName != "" && 
				m_mapIFNameToObj.find(strInputFinderName) != m_mapIFNameToObj.end())
			{
				SetInputFinder(strInputFinderName);
			}
			else if (m_mapIFNameToObj.size() > 0)
			{
				// either there is no entry for the last used input finder,
				// or that entry was not found in our map - so just use the
				// first entry in our map
				SetInputFinder(m_mapIFNameToObj.begin()->first);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03882")

	return CDialog::ShowWindow(nCmdShow);	
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::showOpenDialog()
{
	OnFileOpen();
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::setTextProcessors(IIUnknownVector *pvecTextProcessors)
{
	m_ipTextProcessors = pvecTextProcessors;

	// ensure that the number of PTHs provided is within acceptable limits.
	if (m_ipTextProcessors)
	{
		long nNumTPs = m_ipTextProcessors->Size();
		long nMaxAllowedTPs = (ID_MAX_TP - ID_ENTIRE_TEXT_01)/m_nTPOptionSpanLen;
		if (nNumTPs >= nMaxAllowedTPs)
		{
			UCLIDException ue("ELI04836", "Too many Text Processor objects provided!");
			ue.addDebugInfo("# of TPs", nNumTPs);
			ue.addDebugInfo("Max allowed", nMaxAllowedTPs);
			throw ue;
		}
	}

	// update UI
	updateStateOfProcessTextButton();
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::clearTextProcessors()
{
	m_ipTextProcessors = NULL;
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::setIndirectSourceName(const string& strIndirectSourceName) 
{
	m_strIndirectSourceName = strIndirectSourceName;

	// update the window title
	setTitleText(strIndirectSourceName);
}

//--------------------------------------------------------------------------------------------------
// Event handlers
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnTextSelected(long lEntityID) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Create an Input Entity object
		IInputEntityPtr ipInputEntity;
		ipInputEntity.CreateInstance( CLSID_InputEntity );
		
		// Initialize the Input Entity object
		_bstr_t bstrEntityID = asString( lEntityID ).c_str();
		ipInputEntity->InitInputEntity( m_pInputEntityManager, 
			bstrEntityID);
		
		// Create a Text Input object
		ITextInputPtr ipTextInput;
		ipTextInput.CreateInstance( CLSID_TextInput );
		
		// Initialize the Text Input object
		_bstr_t bstrText = GetEntityText( lEntityID ).c_str();
		ipTextInput->InitTextInput( ipInputEntity, bstrText );
		
		// Fire the event
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
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03038")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnSelectedText(LPCTSTR strText) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Create a Text Input object
		ITextInputPtr ipTextInput;
		ipTextInput.CreateInstance( CLSID_TextInput );
		
		// Initialize the Text Input object
		_bstr_t bstrText(strText);
		ipTextInput->InitTextInput( NULL, bstrText );
		
		// Fire the event
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
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03039")
}

//--------------------------------------------------------------------------------------------------
// Public toolbar methods
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnAppend() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Add text to rich edit control
		m_TV.appendTextFromClipboard();
		
		// Parse the text using the current Finder
		parseText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03045")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnEditDecrease() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Decrease the text size
		m_TV.decreaseTextSize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03044")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnEditIncrease() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Increase the text size
		m_TV.increaseTextSize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03043")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnEditPaste() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Paste text into rich edit control
		m_TV.pasteTextFromClipboard();
		
		// Parse the text using the current Finder
		parseText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03042")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnEditCopy() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Call the control class method to copy entire text to clipboard
		m_TV.copyTextToClipboard();	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03041")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnFileNew() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check to see if the text has been modified
		if (m_TV.isModified())
		{
			CString zMsgBuf;
			
			// Check to see if the text is associated with a file
			CString zName = m_TV.getFileName();
			
			// Compose message prompt for user
			if (!zName.IsEmpty())
			{
				zMsgBuf.Format( "Do you want to save the changes you made to %s?", zName );
			}
			else
			{
				zMsgBuf = "Do you want to save the changes you made to this document?";
			}
			
			// Prompt user to save
			int iNew = MessageBox( zMsgBuf, "Save", 
				MB_YESNOCANCEL | MB_ICONEXCLAMATION );
			
			if (iNew == IDYES)
			{
				// Save the changes
				m_TV.save();
				
				// Clear the contents
				m_TV.clear();
				
				// Parse the text using the current Finder
				parseText();
				
				// Reset the title bar text
				updateTitle();
			}
			else if (iNew == IDNO)
			{
				// DO NOT save the changes, just clear the contents and return
				m_TV.clear();
				
				// Parse the text using the current Finder
				parseText();
				
				// Reset the title bar text
				updateTitle();
			}
			// else iNew == IDCANCEL
			// just return without any action
		}
		else
		{
			// No changes to text so save is not needed, 
			// just clear the contents
			m_TV.clear();
			
			// Parse the text using the current Finder
			parseText();
			
			// Reset flag
			m_bPersistentSource = false;
			
			// Reset the title bar text
			updateTitle();
		}
	
		// reset indirect source name
		m_strIndirectSourceName = "";
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03040")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnFileOpen() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		////////////////////
		// File save needed?
		////////////////////
		
		// Has text changed?
		if (m_TV.isModified())
		{
			// Check the filename
			CString	zName = m_TV.getFileName();
			
			// Message prompt for saving
			CString	zMsgBuf;
			
			// Check for a filename
			if (!zName.IsEmpty()) 
			{
				// Filename known, check for save
				zMsgBuf.Format ("Do you want to save the changes you made to %s?", zName);
			}
			else
			{
				// Filename unknown
				zMsgBuf = "Do you want to save the changes you made to this document?";
			}
			
			// Ask user for save decision
			int iNew = MessageBox( zMsgBuf, "Save", 
				MB_YESNOCANCEL | MB_ICONEXCLAMATION );
			
			if (iNew == IDCANCEL)
			{
				// User cancel, just return without save or open
				return;
			}
			
			if (iNew == IDYES)
			{
				OnFileSave();
			}
		}
		
		/////////////////////////////
		// Prepare a File Open dialog
		/////////////////////////////
		
		// Create the filters string with all the supported extensions
		char pszStrFilter[] = "Text Files (*.txt)|*.txt|Microsoft Word Documents (*.doc)|*.doc|Works 4.0 for Windows (*.wps)|*.wps|Word Perfect 5.x Documents (*.doc)|*.doc|Word Perfect 6.x Documents (*.wpd;*.doc)|*.wpd;*.doc|Rich Text Format (*.rtf)|*.rtf|Microsoft Word for Macintosh (*.mcw)|*.mcw|All Files (*.*)|*.*||";
		
		// Create a buffer pszData to receive the key word of Word.Application from the registry.
		LONG lSize = 128;
		char pszData [128];
		
		// Query registry whether key word of Word Application exists or not
		LONG lRet = RegQueryValue( HKEY_CLASSES_ROOT, "Word.Application", pszData, 
			&lSize );
		
		if (lRet != ERROR_SUCCESS)
		{
			// Microsoft word is not installed, change the filters to show in the FileDialog
			// to open only text files
			strcpy_s( (char*)pszStrFilter, sizeof(pszData), 
				"Text Files (*.txt)|*.txt|All Files (*.*)|*.*||" );
		}
		
		// Show the file dialog to select the file to be opened
		CFileDialog fileDlg( TRUE, NULL, NULL, OFN_READONLY | OFN_HIDEREADONLY, 
			pszStrFilter, this );
		
		// Modify the initial directory
		string strTemp = ma_pCfgHTIRMgr->getLastFileOpenDirectory();
		fileDlg.m_ofn.lpstrInitialDir = strTemp.c_str();
		
		if (fileDlg.DoModal() == IDOK)
		{
			// Get the selected file complete path
			CString zFilePath = fileDlg.GetPathName();

			Open((LPCTSTR)zFilePath);
		}

		// reset indirect source name
		m_strIndirectSourceName = "";
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03046")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnFilePrint() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Call the control class method to print the text in the control
		m_TV.print();	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03047")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnFileSave() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check to see if there is a filename
		CString zFilePath;
		zFilePath = m_TV.getFileName();
		
		// Check for empty filename string
		if ((zFilePath.GetLength() == 0) && 
			(m_strIndirectSourceName.length() != 0))
		{
			// Default to the indirect source name
			zFilePath = m_strIndirectSourceName.c_str();
		}

		// Check existing file for an extension
		string strExt = getExtensionFromFullPath( zFilePath.operator LPCTSTR() );

		// Look for an extension that isn't .txt
		if (strExt.compare( string(".txt") ) != 0)
		{
			// Check that file already has a name
			if (!zFilePath.IsEmpty())
			{
				// Append a .txt since it will be saved in this format
				zFilePath += ".txt";
			}
		}

		// Create the SAVE AS file dialog
		// with existing file name, do not allow read-only files to be chosen
		CFileDialog fileDlg
			( FALSE, 
			"*.txt", 
			zFilePath.operator LPCTSTR(), 
			OFN_NOREADONLYRETURN | OFN_READONLY,
			"Text Files (*.txt)|*.txt|"
			"All files (*.*)|*.*|"
			"||", 
			this );
		
		// Open the dialog
		if (fileDlg.DoModal() == IDOK)
		{
			// Retrieve the selected file complete path
			zFilePath = fileDlg.GetPathName();
			
			// Check for file already existing
			if (_access( zFilePath.operator LPCTSTR(), 00 ) == 0)
			{
				// Get user confirmation before overwriting the file
				int iRes = MessageBox( "Do you want to overwrite the existing file?", 
					"Confirmation", MB_ICONQUESTION | MB_YESNO );
				
				if (iRes == IDNO)
				{
					// Just return without saving
					return;
				}
			}

			// Call the control class method to save the current text
			m_TV.saveAs( zFilePath.operator LPCTSTR() );
			
			// Set flag
			m_bPersistentSource = true;
			
			// Reset the title bar text
			updateTitle();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03048")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnTPMenuItemSelected(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		ETextProcessingScope eTextProcessingScope 
			= (ETextProcessingScope)((nID - ID_ENTIRE_TEXT_01)%m_nTPOptionSpanLen);
		// get the text, depending upon the currently selected scope
		string strText;
		switch (eTextProcessingScope)
		{
		case kProcessEntireText:
			// process the entire text
			strText = (LPCTSTR) m_TV.getText();
			break;
			
		case kProcessSelectedText:
			// process the selected text
			// get selected text from mcr text viewer
			strText = (LPCTSTR) m_TV.getSelectedText();
			break;
			
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI04834")
		}
		
		// get access to the selected text processor
		int nSelectedTPIndex = (int)((nID - ID_ENTIRE_TEXT_01)/m_nTPOptionSpanLen);
		UCLID_HIGHLIGHTEDTEXTIRLib::IHTIRTextProcessorPtr ipTP = 
						m_ipTextProcessors->At(nSelectedTPIndex);
		if (ipTP)
		{
			// invoke the text processor
			ipTP->ProcessText(_bstr_t(strText.c_str()));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04835")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnProcessText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	try
	{
		// get the current cursor position and show the TP choice
		// menu at the current cursor location
		GetCursorPos(&m_lastTPMenuPos);
		showTPMenu(m_lastTPMenuPos);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04822")
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::showTPMenu(POINT menuPos) 
{
	// Create the top level menu which will have all PTH descriptions as the menu items
	CMenu topMenu;
	topMenu.CreatePopupMenu();

	static const CString zEntireText("Process entire text");
	static const CString zSelectedText("Process selected text");

	// for each TP, add a menu item with the description
	int nNumTPs = m_ipTextProcessors->Size();
	// if there's only one TP, no sub menu is needed.
	if (nNumTPs == 1)
	{
		// get the text description of the PTH
		UCLID_HIGHLIGHTEDTEXTIRLib::IHTIRTextProcessorPtr ipTP;
		ipTP = m_ipTextProcessors->At(0);
		
		// add a menu item with the description of the TP
		_bstr_t _bstrTPDescription = ipTP->GetTPDescription();
		string stdstrTPDescription = _bstrTPDescription;
		CString zItem1 = stdstrTPDescription.c_str();
		CString zItem2 = zItem1 + " (" + zSelectedText + ")";
		zItem1 += " (" + zEntireText + ")";
		// create an item that has a popup menu
		topMenu.AppendMenu(MF_BYPOSITION, ID_ENTIRE_TEXT_01, zItem1);
		topMenu.AppendMenu(MF_BYPOSITION, ID_ENTIRE_TEXT_01+1, zItem2);
	}
	else
	{
		for (int i = 0; i < nNumTPs; i++)
		{
			// create sub menu for TP options
			CMenu subMenu;
			subMenu.CreatePopupMenu();
			// the id number for process entire text
			int nEntireTextValue = ID_ENTIRE_TEXT_01 + i * m_nTPOptionSpanLen;
			// add the types of TP to the sub menu
			subMenu.AppendMenu(MF_BYPOSITION, nEntireTextValue, zEntireText);
			subMenu.AppendMenu(MF_BYPOSITION, nEntireTextValue+1, zSelectedText);
			HMENU hmenu = subMenu.m_hMenu;

			// get the text description of the TP
			UCLID_HIGHLIGHTEDTEXTIRLib::IHTIRTextProcessorPtr ipTP;
			ipTP = m_ipTextProcessors->At(i);
			
			// add a menu item with the description of the TP
			_bstr_t _bstrTPDescription = ipTP->GetTPDescription();
			string stdstrTPDescription = _bstrTPDescription;
			topMenu.AppendMenu(MF_BYPOSITION|MF_POPUP, (int)hmenu, stdstrTPDescription.c_str());
		}
	}

	// show the popup menu at the specified location
	topMenu.TrackPopupMenu(TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
		menuPos.x, menuPos.y, this, NULL);
}
//-------------------------------------------------------------------------------------------------
void HighlightedTextDlg::OnBTNInputFinder() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	try
	{
		// press down the buttun
		m_toolBar.GetToolBarCtrl().PressButton(ID_BTN_InputFinder, TRUE);

		// Load the popup MRU file list menu
		CMenu menuLoader;
		if (!menuLoader.LoadMenu(IDR_MENU_InputFinder))
		{
			throw UCLIDException("ELI03283", "Failed to load Highlight Finder list.");
		}
		
		CMenu* pPopup = menuLoader.GetSubMenu(0);
		if (pPopup)
		{
			long nTotalInputFinders = m_mapIFNameToObj.size();
			// Retrieve Finder descriptions from map
			if (nTotalInputFinders > 0)
			{
				// remove the "No Finder" item from the menu
				pPopup->RemoveMenu( ID_MNU_NOFINDER, MF_BYCOMMAND );
				
				int iIndex = 0;
				int iCurrentInputFinderIndex = 0;
				map<string, IInputFinderPtr>::const_iterator iter;
				for (iter = m_mapIFNameToObj.begin();
					iter != m_mapIFNameToObj.end(); iter++)
				{
					// Add string to menu
					pPopup->InsertMenu(iIndex, MF_BYPOSITION, 
						ID_MNU_Finder1 + iIndex, iter->first.c_str() );

					// we need the ID of the current input finder to 
					// check the corresponding menu item in the code
					// below.  If this input finder is the currently 
					// used input finder to parse/highlight the text
					// then note it.
					if (iter->first == m_strFinderName)
						iCurrentInputFinderIndex = iIndex;

					iIndex++;
				}

				// Select desired Finder
				pPopup->CheckMenuItem(ID_MNU_Finder1 + iCurrentInputFinderIndex, 
					MF_CHECKED);				
			}
			
			CRect rc;
			m_toolBar.SendMessage( TB_GETRECT, ID_BTN_InputFinder, (LPARAM)&rc );
			m_toolBar.ClientToScreen( &rc );
			
			pPopup->TrackPopupMenu(TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
									rc.left, rc.bottom, this, &rc );
		}

		// release the button
		m_toolBar.GetToolBarCtrl().PressButton(ID_BTN_InputFinder, FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03282")
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::createToolBar()
{
	// Create the toolbar object with desired options
	if (m_toolBar.CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
    | CBRS_GRIPPER | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) )
	{
		// Load the bitmap resource
		m_toolBar.LoadToolBar(IDR_TVTOOLBAR);
	}

	// Add tooltips to style
	m_toolBar.SetBarStyle(m_toolBar.GetBarStyle() |
		CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC);

	m_toolBar.ModifyStyle(0, TBSTYLE_TOOLTIPS);

	// Add a drop down arrow next to the Open button
	m_toolBar.GetToolBarCtrl().SetExtendedStyle(TBSTYLE_EX_DRAWDDARROWS);
	DWORD dwStyle = m_toolBar.GetButtonStyle(m_toolBar.CommandToIndex(ID_FILE_OPEN));
	dwStyle |= TBBS_DROPDOWN;
	m_toolBar.SetButtonStyle(m_toolBar.CommandToIndex(ID_FILE_OPEN), dwStyle);

	// Determine and save height of toolbar
	CRect rcClientNow;
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST,
				   0, reposQuery, rcClientNow);
	m_iBarHeight = rcClientNow.top;

	// How far does control window need to move?
	CPoint ptOffset( 0, m_iBarHeight );

	// Move control window to make space for the toolbar
	CRect  rcChild;
	CWnd* pwndChild = GetWindow( GW_CHILD );
	while (pwndChild)
	{
		// Get this window's position
		pwndChild->GetWindowRect( rcChild );
		// ... in client coordinates
		ScreenToClient( rcChild );
		// Adjust position to allow space for toolbar
		rcChild.OffsetRect( ptOffset );
		// Apply the changed position
		pwndChild->MoveWindow( rcChild, FALSE );
		// Retrieve next child window
		pwndChild = pwndChild->GetNextWindow();
	}

	// Grow the dialog window to account for toolbar
	CRect rcWindow;
	GetWindowRect( rcWindow );
	rcWindow.bottom += ptOffset.y;
	MoveWindow( rcWindow, FALSE );

	// Position the toolbar in the dialog
	RepositionBars( AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0 );
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::parseText() 
{
	// Tell OCX to parse the text using the current Input Finder
	m_TV.parseText();
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::resizeItems() 
{
	// Ensure that the dlg's controls are realized before moving them.
	if (m_bInitialized)		
	{
		// Set the MCR text viewer control to be the same size as the 
		// dialog's client area less the height of the toolbar
		// and the status bar
		RECT rect;
		GetClientRect( &rect );
		rect.top = m_iBarHeight;		// room for toolbar at top
		rect.bottom -= m_iStatusHeight;	// room for status bar at bottom
		m_TV.MoveWindow( &rect );

		// Position the toolbar in the dialog
		RepositionBars( AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0 );
	}
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::setStatusText(const string& strText) 
{
	if (m_bInitialized)
	{
		// Modify the text in the first status bar pane
		m_statusBar.GetStatusBarCtrl().SetText( strText.c_str(), 0, 
			SBT_NOBORDERS );
	}
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::updateTitle()
{
	string strTitle("");

	// Check for existing filename
	CString	zFilename = m_TV.getFileName();
	if (!zFilename.IsEmpty())
	{
		// Check for backslash indicating a complete path
		int iPos = zFilename.ReverseFind( '\\' );

		// Remove any folder information
		if (iPos != -1)
		{
			// Remove path and backslash by keeping just the filename
			zFilename = zFilename.Right( zFilename.GetLength() - iPos - 1 );
		}

		// Prepend filename and dash to basic title
		strTitle = (LPCTSTR)zFilename;
	}

	// Set the title bar text
	setTitleText(strTitle);
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::setTitleText(const string& strTitleText)
{
	CString zTitle("");
	if (!strTitleText.empty())
	{
		zTitle = strTitleText.c_str();
		zTitle += " - ";
	}

	zTitle += "Highlighted Text";

	// Set the title bar text
	SetWindowText(zTitle);
}
//--------------------------------------------------------------------------------------------------
string HighlightedTextDlg::getListFileName(const string& strModuleName, 
										   const string& strListFileName)
{
	return getModuleDirectory(strModuleName) + "\\" + strListFileName;
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::addFileToMRUList(const std::string& strFileToBeAdded)
{
	// we wish to have updated items all the time
	ma_pRecentFiles->readFromPersistentStore();
	ma_pRecentFiles->addItem(strFileToBeAdded);
	ma_pRecentFiles->writeToPersistentStore();	
}
//--------------------------------------------------------------------------------------------------
void HighlightedTextDlg::removeFileFromMRUList(const std::string& strFileToBeRemoved)
{
	// remove the bad file from MRU List
	ma_pRecentFiles->readFromPersistentStore();
	ma_pRecentFiles->removeItem(strFileToBeRemoved);
	ma_pRecentFiles->writeToPersistentStore();	
}
//--------------------------------------------------------------------------------------------------
