// CFlexDataEntryDlg.cpp : implementation file

#include "stdafx.h"
#include "FlexDataEntry.h"
#include "FlexDataEntryDlg.h"

#include <UCLIDException.h>
#include <Notifications.h>
#include <INIFilePersistenceMgr.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <CommentedTextFileReader.h>
#include <VariableRegistry.h>
#include <TextFunctionExpander.h>

#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// String constant for default INI file
const std::string	gstrINI_FILE = "FlexDataEntry.ini";

//////////////////////////////////////////////
//// Constants for FlexDataEntry Shortcut keys
//////////////////////////////////////////////

// Key to Find data in the open image (plus CONTROL)
const unsigned int	guiFDE_FIND = 'F';
const unsigned int	guiFDE_FIND_VK = VK_CONTROL;

// Key to Save results (plus CONTROL)
const unsigned int	guiFDE_SAVE = 'S';
const unsigned int	guiFDE_SAVE_VK = VK_CONTROL;

// Key to Clear results (plus CONTROL)
const unsigned int	guiFDE_CLEAR = 'W';
const unsigned int	guiFDE_CLEAR_VK = VK_CONTROL;

// Key to Move to the Next cell (to the Right)
const unsigned int	guiFDE_RIGHT = VK_TAB;
//const unsigned int	guiFDE_RIGHT_VK = 0;

// Key to Move to the Next cell (to the Left)
//const unsigned int	guiFDE_LEFT = VK_TAB;
const unsigned int	guiFDE_LEFT_VK = VK_SHIFT;

// Key to Move to the Next cell (Upward)
const unsigned int	guiFDE_UP = VK_UP;
//const unsigned int	guiFDE_UP_VK = 0;

// Key to Move to the Next cell (Downward)
const unsigned int	guiFDE_DOWN = VK_DOWN;
//const unsigned int	guiFDE_DOWN_VK = 0;

/////////////////////////////////////////////////////
//// Constants for [General] Settings within INI file
/////////////////////////////////////////////////////

// String constant for main RSD file
const std::string	gstrMAIN_RSD_FILE = "RSDFile";

// String constant for main RSD file
const std::string	gstrTOOLBAR_FIND = "Toolbar_Find";

// String constant for filename or path to Output Template File
const std::string	gstrOUTPUT_TEMPLATE = "OutputTemplateFile";

// String constant for filename or relative path to Output File
// Can include TextFunctionExpander items:
//   <SourceDocName>
//   $DirOf(), $DriveOf(), $FileOf(), $FileNoExtOf()
const std::string	gstrOUTPUT_FILENAME = "OutputFileName";

// String constant for Confirmation Message after Save
const std::string	gstrCONFIRMATION_AFTER_SAVE = "ConfirmationAfterSave";

// String constant for Automatic Close of FDE after Save
const std::string	gstrAUTO_CLOSE_AFTER_SAVE = "AutomaticCloseAfterSave";

// Number of milliseconds to wait between Save and Auto-Close
const std::string	gstrMILLISECONDS_BETWEEN_SAVE_AND_CLOSE = "MillisecondsBetweenSaveAndClose";

// String constant for Dialog-based Vertical Scroll Bar
const std::string	gstrVERTICAL_SCROLL_PIXELS = "PixelsForVerticalScrollBar";

// SRIR Toolbar Buttons
const std::string	gstrSRIR_ROTATE_CCW = "ImageToolbar_RotateCCW";
const std::string	gstrSRIR_ROTATE_CW = "ImageToolbar_RotateCW";
const std::string	gstrSRIR_PAGE_FIRST = "ImageToolbar_PageFirst";
const std::string	gstrSRIR_PAGE_LAST = "ImageToolbar_PageLast";
const std::string	gstrSRIR_DEL_HIGHLIGHT = "ImageToolbar_DeleteHighlight";

//-------------------------------------------------------------------------------------------------
// CFlexDataEntryDlg
//-------------------------------------------------------------------------------------------------
CFlexDataEntryDlg::CFlexDataEntryDlg(string strCommand, CWnd* pParent /*=NULL*/)
	: CDialog(CFlexDataEntryDlg::IDD, pParent),
	m_ipEngine(NULL),
	m_ipSRIR(NULL),
	m_strCommandLine(strCommand),
	m_lRefCount(0), 
	m_bInitialized(false),
	m_bOutputFormatDefined(false),
	m_bOpenVOAFile(false),
	m_pActiveGrid(NULL),
	m_nScrollPos(0),
	m_lTotalGridHeight(0),
	m_lMinScrollPos(0),
	m_lScrollStep(0)
{
	try
	{
		//{{AFX_DATA_INIT(CFlexDataEntryDlg)
			// NOTE: the ClassWizard will add member initialization here
		//}}AFX_DATA_INIT
		// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
		m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

		// Initiate use of singleton input manager.
		UseSingletonInputManager();
		ASSERT_RESOURCE_ALLOCATION("ELI11096", getInputManager() != NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10952")
}
//-------------------------------------------------------------------------------------------------
CFlexDataEntryDlg::~CFlexDataEntryDlg()
{
	try
	{
		// Delete the singleton input manager
		IInputManagerSingletonPtr ipInputMgrSingleton(CLSID_InputManagerSingleton);
		ipInputMgrSingleton->DeleteInstance();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16441");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFlexDataEntryDlg, CDialog)
	//{{AFX_MSG_MAP(CFlexDataEntryDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_CLOSE()
	ON_COMMAND(IDC_BTN_CLEAR, OnBtnClear)
	ON_COMMAND(IDC_BTN_FIND, OnBtnFind)
	ON_COMMAND(IDC_BTN_SAVE, OnBtnSave)
	ON_COMMAND(IDC_BTN_HELP, OnBtnHelpAbout)
	ON_WM_VSCROLL()
	ON_WM_SYSCOMMAND()
	//}}AFX_MSG_MAP
	ON_MESSAGE( WM_NOTIFY_GRID_LCLICK, OnLClickGrid )
	ON_CONTROL_RANGE(BN_CLICKED, IDC_CONTROL_BASE+IDC_OFFSET_ADD*IDC_GROUP_MAX, IDC_CONTROL_BASE+(IDC_OFFSET_ADD+1)*IDC_GROUP_MAX-1, OnAddButton)
	ON_CONTROL_RANGE(BN_CLICKED, IDC_CONTROL_BASE+IDC_OFFSET_DELETE*IDC_GROUP_MAX, IDC_CONTROL_BASE+(IDC_OFFSET_DELETE+1)*IDC_GROUP_MAX-1, OnDeleteButton)
	ON_CONTROL_RANGE(BN_CLICKED, IDC_CONTROL_BASE+IDC_OFFSET_PREVIOUS*IDC_GROUP_MAX, IDC_CONTROL_BASE+(IDC_OFFSET_PREVIOUS+1)*IDC_GROUP_MAX-1, OnPreviousButton)
	ON_CONTROL_RANGE(BN_CLICKED, IDC_CONTROL_BASE+IDC_OFFSET_NEXT*IDC_GROUP_MAX, IDC_CONTROL_BASE+(IDC_OFFSET_NEXT+1)*IDC_GROUP_MAX-1, OnNextButton)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXT,0x0000,0xFFFF,OnToolTipNotify)
	ON_MESSAGE( WM_NOTIFY_MOVE_CELL_LEFT, OnCellLeft )
	ON_MESSAGE( WM_NOTIFY_MOVE_CELL_RIGHT, OnCellRight )
	ON_MESSAGE( WM_NOTIFY_MOVE_CELL_UP, OnCellUp )
	ON_MESSAGE( WM_NOTIFY_MOVE_CELL_DOWN, OnCellDown )
	ON_COMMAND(ID_FILE_EXIT, OnFileExit)
END_MESSAGE_MAP()


//-------------------------------------------------------------------------------------------------
// IUnknown
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) CFlexDataEntryDlg::AddRef()
{
	InterlockedIncrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) CFlexDataEntryDlg::Release()
{
	InterlockedDecrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::QueryInterface( REFIID iid, void FAR* FAR* ppvObj)
{
	IParagraphTextHandler *pTmp = this;

	if (iid == IID_IUnknown)
		*ppvObj = static_cast<IUnknown *>(pTmp);
	else if (iid == IID_IDispatch)
		*ppvObj = static_cast<IDispatch *>(pTmp);
	else if (iid == IID_ISRWEventHandler)
		*ppvObj = static_cast<ISRWEventHandler *>(this);
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
// ISRWEventHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_AboutToRecognizeParagraphText()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_AboutToRecognizeLineText()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_NotifyKeyPressed(long nKeyCode, short shiftState)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Handle or ignore the character
		handleCharacter( nKeyCode );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12993")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_NotifyCurrentPageChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12994")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_NotifyEntitySelected(long nZoneID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12988")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_NotifyFileOpened(BSTR bstrFileFullPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Clear grid contents
		OnBtnClear();

		// Enable the Find button, if present
		if (!isFindHidden())
		{
			enableButton( IDC_BTN_FIND, true );
		}

		// Enable the Save button, if present
		if (!isSaveHidden())
		{
//			m_apToolBar->GetToolBarCtrl().EnableButton( IDC_BTN_SAVE, TRUE );
			enableButton( IDC_BTN_SAVE, true );
		}

		// Open VOA file
		if (m_bOpenVOAFile)
		{
			// Get VOA name by appending VOA file extension
			string strFile = asString( bstrFileFullPath );
			string strVOA( strFile );
			strVOA += ".voa";

			if (isFileOrFolderValid( strVOA ))
			{
				// Load entire collection of Attributes
				IIUnknownVectorPtr	ipAttributes( CLSID_IUnknownVector );
				ASSERT_RESOURCE_ALLOCATION( "ELI13087", ipAttributes != NULL );
				ipAttributes->LoadFrom( get_bstr_t( strVOA.c_str() ), VARIANT_FALSE );

				// Pass Attributes to grids
				populateGrids( ipAttributes );

				// Select first cell
				OnVScroll( SB_PAGEUP, 0, NULL );
				activateFirstCell();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12992")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_NotifyOpenToolbarButtonPressed(VARIANT_BOOL *pbContinueWithOpen)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbContinueWithOpen == NULL)
		{
			return E_POINTER;
		}

		// Always allow File - Open if Save button is hidden
		if (isSaveHidden())
		{
			*pbContinueWithOpen = VARIANT_TRUE;
			return S_OK;
		}

		bool bAllowOpen = true;
		bool bGridsEmpty = true;

		// Check Modified and Empty states for each grid
		unsigned long ulSize = m_vecGrids.size();
		for (unsigned int ui = 0; ui < ulSize; ui++)
		{
			// Retrieve this grid
			CDataEntryGrid*	pGrid = m_vecGrids[ui];

			// Check Modified state for grid contents
			if (pGrid->IsModified())
			{
				bAllowOpen = false;
			}

			// Check Empty state
			if (!pGrid->IsEmpty())
			{
				bGridsEmpty = false;
			}

			// Quit checking if both flags are modified
			if (!bAllowOpen && !bGridsEmpty)
			{
				break;
			}
		}

		// Allow user to save changes
		if (!bAllowOpen && !isSaveHidden())
		{
			// Provide prompt to user
			int iResult = MessageBox( "Save edits to PXT file before opening another image?", 
				"Confirm Save", MB_ICONEXCLAMATION | MB_YESNOCANCEL );
			if (iResult == IDYES)
			{
				// Save the changes and allow new file to be opened
				if (validateSave())
				{
					// Do not allow auto close because we want to open another image
					doSave( false );
					bAllowOpen = true;
				}
			}
			else if (iResult == IDNO)
			{
				// Do not save the changes, but allow new file to be opened
				bAllowOpen = true;
			}
			// else do not allow file open
		}
		// Allow user to Save unmodified results if no output file exists
		else if (bAllowOpen && !isSaveHidden())
		{
			// Check for existence of output file
			string strOutput = getOutputFile();
			if (!isFileOrFolderValid( strOutput ))
			{
				// Make sure that at least one grid has information
				if (!bGridsEmpty)
				{
					// Provide prompt to user
					int iResult = MessageBox( "Save results to PXT file before opening another image?", 
						"Confirm Save", MB_ICONEXCLAMATION | MB_YESNOCANCEL );
					if (iResult == IDYES)
					{
						// Save the changes and allow new file to be opened
						if (validateSave())
						{
							// Do not allow auto close because we want to open another image
							doSave( false );
							bAllowOpen = true;
						}
					}
					else if (iResult == IDNO)
					{
						// Do not save the changes, but allow new file to be opened
						bAllowOpen = true;
					}
					else
					{
						// Do not allow file open
						bAllowOpen = false;
					}
				}
			}
		}

		*pbContinueWithOpen = (bAllowOpen) ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13107")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_NotifyZoneEntityMoved(long nZoneID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Do nothing of consequence

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_NotifyZoneEntitiesCreated(IVariantVector *pZoneIDs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Do nothing of consequence

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IParagraphTextHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_NotifyParagraphTextRecognized(
	ISpotRecognitionWindow *pSourceSRWindow, ISpatialString *pText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		IParagraphTextHandlerPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI22008", ipThis != NULL);

		// Continue iff PTH is enabled
		if (ipThis->IsPTHEnabled() == VARIANT_TRUE)
		{
			// Get the spatial text
			ISpatialStringPtr ipText(pText);
			ASSERT_RESOURCE_ALLOCATION("ELI10944", ipText != NULL);

			// Pass text to active Grid
			CDataEntryGrid*	pGrid = getActiveGrid();
			if (pGrid != NULL)
			{
				int iNew = pGrid->HandleRubberband( ipText );
				updateNavigationControls( pGrid );
				if (iNew != -1)
				{
					// Get the new (visible) Attribute and highlight it
					IAttributePtr	ipNew = pGrid->GetAttributeFromRow( iNew + 1 );
					ASSERT_RESOURCE_ALLOCATION("ELI13159", ipNew != NULL);
					highlightAttribute( ipNew );
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10941")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_GetPTHDescription(BSTR *pstrDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pstrDescription = get_bstr_t("Send text to dialog").copy();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10942")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFlexDataEntryDlg::raw_IsPTHEnabled(VARIANT_BOOL *pbEnabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Default to disabled
		*pbEnabled = VARIANT_FALSE;

		// Get active grid
		CDataEntryGrid*	pGrid = getActiveGrid();
		if ((pGrid != NULL) && pGrid->IsRubberbandEnabled())
		{
			*pbEnabled = VARIANT_TRUE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10943")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Input Funnel Events
//-------------------------------------------------------------------------------------------------
HRESULT CFlexDataEntryDlg::NotifyInputReceived(ITextInput* pTextInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI10949", pTextInput != NULL);

		// Retrieve the input text
		ITextInputPtr ipTextInput = pTextInput;
		string	strInput = ipTextInput->GetText();

		// Handle the input
		handleSwipeInput( strInput );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10950")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::activateFirstCell()
{
	// Check each grid for non-empty rows
	unsigned long ulCount = m_vecGrids.size();
	for (unsigned int ui = 0; ui < ulCount; ui++)
	{
		// Retrieve this Grid
		CDataEntryGrid* pGrid = m_vecGrids[ui];
		ASSERT_RESOURCE_ALLOCATION("ELI13149", pGrid != NULL);

		// Skip Type D grids
		if (pGrid->GetType() == "D")
		{
			continue;
		}

		// Check for non-empty rows
		int nCount = pGrid->GetNonEmptyRowCount();
		if (nCount > 0)
		{
			//////////////////////////
			// Activate the first cell - even if empty
			//////////////////////////
			// This call causes an error message - use an alternate scheme
			// MFC code ASSERTS with afxCurrentInstanceHandle == NULL
			// if the cursor has not previously been active in a specific grid
//			pGrid->SetCurrentCell( 1, 1 );

			// Fake a left mouse click into this cell
			CPoint pt;
			pGrid->OnStartSelection( 1, 1, MK_LBUTTON, pt );
			pGrid->SetFocus();

			// Get and highlight the Attribute for this row - if any
			IAttributePtr	ipAttr = pGrid->GetAttributeFromRow( 1 );
//			ASSERT_RESOURCE_ALLOCATION("ELI13152", ipAttr != NULL);
			highlightAttribute( ipAttr );
			return;
		}			// end if non-empty rows exist in this grid
	}				// end for each grid
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::addDefaultMapItems(std::string strText, std::string strLeft, 
										   std::string strRight, std::string strDefault)
{
	// Confirm that strLeft and strRight are non-empty
	if (strLeft.empty() || strRight.empty())
	{
		UCLIDException ue("ELI13013", "Cannot use empty string as tag delimiter.");
		ue.addDebugInfo("strLeft", strLeft);
		ue.addDebugInfo("strRight", strRight);
		throw ue;
	}

	// Locate left delimiter within strText
	unsigned long ulStart = 0;
	unsigned long ulLeft = strText.find( strLeft, ulStart );
	unsigned long ulLength = strText.length();

	// Continue as long as left delimiter is available and not at end-of-string
	while ((ulLeft != string::npos) && (ulLeft < ulLength - 1))
	{
		// Locate next right delimiter
		unsigned long ulRight = strText.find( strRight, ulLeft + 1 );
		if (ulRight != string::npos)
		{
			// Extract the substring - with both delimiters
			string strTag = strText.substr( ulLeft, ulRight - ulLeft + 1 );

			// Check for any other left delimiters within the substring
			unsigned long ulPos = strTag.rfind( strLeft, strTag.length() - 1 );
			if ((ulPos > 0) && (ulPos < strTag.length() - 2))
			{
				// Adjust the substring to use the rightmost left delimiter
				strTag = strTag.substr( ulPos );
			}

			// Add the map item
			m_mapGridCellsToValues[strTag] = strDefault;

			// Keep searching unless right delimiter is at end-of-string
			if (ulRight == ulLength - 1)
			{
				ulLeft = string::npos;
			}
			else
			{
				// Search for next left delimiter after current right delimiter
				ulStart = ulRight + 1;
				ulLeft = strText.find( strLeft, ulStart );
			}
		}
		else
		{
			// Else no matching right delimiter, reset position to allow exit from while()
			ulLeft = string::npos;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::buildPXTFile(std::string strPXTFile)
{
	// Open PXT file - always overwrite if the file exists
	ofstream ofs( strPXTFile.c_str(), ios::out | ios::trunc );
	if (!ofs.is_open())
	{
		// Create and throw exception
		UCLIDException ue("ELI13090", "Unable to open PXT file for writing.");
		ue.addDebugInfo( "PXTFile", strPXTFile );
		throw ue;
	}

	// Build translation object
	VariableRegistry localVariables;
	localVariables.addAllVariables( m_mapGridCellsToValues );

	TextFunctionExpander tfe;

	// Loop through m_vecPXTTags
	unsigned int uiCount = m_vecPXTTags.size();
	for (unsigned int ui = 0; ui < uiCount; ui++)
//	map<string, string>::iterator iter = m_mapOutputTagsToDefinitions.begin();
//	while (iter != m_mapOutputTagsToDefinitions.end())
	{
		// Retrieve tag name and definition
//		string strTag = iter->first;
//		string strDef = iter->second;
		string strTag = m_vecPXTTags[ui];
		string strDef = m_vecPXTDefs[ui];

		// Translate variables to appropriate cell values
		localVariables.replaceVariablesInString( strDef );

		// Check for one or more included TFE functions
		string strOut = strDef;
		if (containsTFEFunction( strDef ))
		{
			// Expand the TextFunctionExpander items
			strOut = tfe.expandFunctions( strDef );
		}

		// Provide result to PXT file as strTag=strOut
		ofs << strTag << "=" << strOut << endl;
//		++iter;
	}

	// Close the file
	ofs.close();
	waitForFileToBeReadable(strPXTFile);
}
//-------------------------------------------------------------------------------------------------
bool CFlexDataEntryDlg::containsTFEFunction(std::string strText)
{
	// Get collection of defined functions
	TextFunctionExpander	tfe;
	vector<string> vecFunctions = tfe.getAvailableFunctions();
	unsigned long	ulCount = vecFunctions.size();

	// Search strText for each function, prefaced by $ and followed by (
	for (unsigned int ui = 0; ui < ulCount; ui++)
	{
		// Retrieve this function
		string strFunction = vecFunctions[ui];

		// Add preceding dollar sign and following parenthesis
		strFunction.insert( 0, "$" );
		strFunction += string( "(" );

		// Search strText for this text
		if (strText.find( strFunction.c_str() ) != string::npos)
		{
			return true;
		}
	}

	// No TFE function found
	return false;
}
//-------------------------------------------------------------------------------------------------
long CFlexDataEntryDlg::createGrids()
{
	// Determine count, names and heights of grids defined in INI file
	long lBottomGridPos = 0;
	long lGridCount = getGridSections( getINIPath() );
	if (lGridCount == 0)
	{
		UCLIDException ue( "ELI11630", "Unable to parse INI file." );
		ue.addDebugInfo( "INI File", getINIPath().c_str() );
		throw ue;
	}

	// Get height of one row of a grid
	long lSingleRowHeight = getRowHeightInPixels();
	m_lScrollStep = lSingleRowHeight;

	// Initial grid position allows space for label and button space
	long lStartOffset = giOFFSET_EDGE + giGROUP_SIZE;
	m_lMinScrollPos = lStartOffset;

	// Create each grid along with associated controls
	// including: Label, Add button, Delete button
	// also: Previous, Page, Next controls for Type D
	for (int i = 0; i < lGridCount; i++)
	{
		// Construct the grid and associates
		CDataEntryGrid*	pGrid = new CDataEntryGrid();

		// Define control ID for Grid
		long lGridID = IDC_CONTROL_BASE + IDC_OFFSET_GRID * IDC_GROUP_MAX + i;

		// Configure each grid based on INI settings
		lStartOffset = prepareGrid( pGrid, lGridID, i, lSingleRowHeight, 
			lStartOffset, this );

		// Store grid pointer in vector
		m_vecGrids.push_back( pGrid );

		// Save Y-position of this grid (plus height of single row)
		CRect	rectThisGrid;
		pGrid->GetWindowRect( &rectThisGrid );
		lBottomGridPos = rectThisGrid.bottom + lSingleRowHeight;
	}

	// Return Y-position of bottom of last grid (plus offset)
	return lBottomGridPos;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::createToolBar()
{
	// Create the toolbar
	m_apToolBar = auto_ptr<CToolBar>(new CToolBar());
	if (m_apToolBar->CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
    | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) )
	{
		m_apToolBar->LoadToolBar( IDR_FLEXDATAENTRY_TOOLBAR );
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

	// Hide toolbar buttons if appropriate
	m_apToolBar->GetToolBarCtrl().HideButton( IDC_BTN_FIND, isFindHidden() );
	m_apToolBar->GetToolBarCtrl().HideButton( IDC_BTN_SAVE, isSaveHidden() );

	// Disable Save button if present
	if (!isSaveHidden())
	{
		m_apToolBar->GetToolBarCtrl().EnableButton( IDC_BTN_SAVE, FALSE );
	}

	// Refresh display
	UpdateWindow();
	Invalidate();
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::doAutoClose()
{
	// Check for auto-close - not required, default to FALSE
	string strConfirm = getSetting( gstrCONFIRMATION_AFTER_SAVE, false );
	if (!strConfirm.empty() && (asLong( strConfirm ) == 1))
	{
		// Display confirmation message, if desired
		string strOutputFile = getOutputFile();
		string strMessage = string("File \"") + strOutputFile.c_str() + string("\" was saved.");
		MessageBox( strMessage.c_str(), "Status", MB_OK );
	}

	// Check for auto-close - not required, default to FALSE
	string strClose = getSetting( gstrAUTO_CLOSE_AFTER_SAVE, false );
	if (!strClose.empty() && (asLong( strClose ) == 1))
	{
		// Clear the grids and the Image Window
		OnBtnClear();
		m_ipSRIR->OpenImageFile( "" );

		// Pause so the user can see that something happened, then exit
		// Default wait is 1000 ms
		string strWait = getSetting( gstrMILLISECONDS_BETWEEN_SAVE_AND_CLOSE, false );
		long lWait = 1000;
		if (!strWait.empty())
		{
			lWait = asLong( strWait );
		}

		Sleep( lWait );

		OnClose();
	} // else, INI has not specified to auto close file after saving
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::doSave(bool bIsAutoCloseAllowed)
{
	// Create temporary PersistenceMgr
	INIFilePersistenceMgr	mgrSettings( getINIPath() );

	///////////////////////////////
	// Build m_mapGridCellsToValues
	///////////////////////////////

	// Delete the existing items inside the map
	map<string, string>::iterator gridCellIter = m_mapGridCellsToValues.begin();
	while (gridCellIter != m_mapGridCellsToValues.end())
	{
		gridCellIter->second = "";
		gridCellIter++;
	}

	// Step through each grid ---> 1-relative for grid numbers
	unsigned long ulCount = m_vecGrids.size();
	for (unsigned int ui = 1; ui <= ulCount; ui++)
	{
		// Retrieve this Grid ---> zero-relative in vector object
		CDataEntryGrid* pGrid = m_vecGrids[ui-1];

		// Get number of rows and columns for this grid
		unsigned int uiNumRows = pGrid->GetRowCount();
		unsigned int uiNumCols = pGrid->GetColCount();

		// Step through each row
		for (unsigned int uj = 1; uj <= uiNumRows; uj++)
		{
			// Begin to build GridX string ---> <Grid1
			CString zVariable;
			zVariable.Format( "<%s", pGrid->GetID().c_str() );

			// Retrieve grid type
			string strType = pGrid->GetType();

			// Add row information, if available
			if (strType == "A")
			{
				// Append row number to variable ---> <Grid1[1]
				zVariable.AppendFormat( "[%d]", uj );

				// Step through each column
				for (unsigned int uk = 1; uk <= uiNumCols; uk++)
				{
					// Retrieve this column header label
					CString zColumnLabel = pGrid->GetCellValue( 0, uk, true );

					// Append column label and angle bracket ---> <Grid1[1][First]>
					CString zColumnVariable = zVariable;
					zColumnVariable.AppendFormat( "[%s]>", zColumnLabel );
					string strVariable( zColumnVariable.operator LPCTSTR() );

					// Retrieve value in this cell
					CString zCellValue = pGrid->GetCellValue( uj, uk, true );

					// Update default map item
					m_mapGridCellsToValues[strVariable] = zCellValue.operator LPCTSTR();

				}	// end for each column
			}		// end else Type A
			else if ((strType == "B") || (strType == "C"))
			{
				// Retrieve this row header label
				CString zRowLabel = pGrid->GetCellValue( uj, 0, true );

				// Append row label and angle bracket ---> <Grid5[City]>
				zVariable.AppendFormat( "[%s]>", zRowLabel );
				string strVariable( zVariable.operator LPCTSTR() );

				// Retrieve value in this cell
				CString zCellValue = pGrid->GetCellValue( uj, 1, true );

				// Update default map item
				m_mapGridCellsToValues[strVariable] = zCellValue.operator LPCTSTR();

			}		// end else Type B or Type C
			else
			{
				// Append final angle bracket to variable name ---> <Grid4>
				zVariable.AppendChar( '>' );
				string strVariable( zVariable.operator LPCTSTR() );

				// Retrieve value in this cell (expecting only uj = 1)
				CString zCellValue = pGrid->GetCellValue( uj, 1, true );

				// Update default map item
				m_mapGridCellsToValues[strVariable] = zCellValue.operator LPCTSTR();

			}		// end else Type D
		}			// end for each row in this grid
		pGrid->SetModified(false);     // set modified to false for each grid
	}				// end for each grid

	////////////////////
	// Build output file
	// - PXT file from m_vecPXTTags, m_vecPXTDefs, m_mapGridCellsToValues
	////////////////////
	string strPXTFile = getOutputFile();
	buildPXTFile( strPXTFile );

	// If auto close is allowed, the program will call doAutoClose() 
	// But whether the program will close the application or not depend on
	// the setting in INI file.
	if (bIsAutoCloseAllowed)
	{
		doAutoClose();
	}
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::enableButton(int nID, bool bEnable)
{
	// Check for toolbar
	if (m_apToolBar.get())
	{
		// Check for this button ID
		UINT uiIndex = m_apToolBar->GetToolBarCtrl().CommandToIndex( nID );
		if (uiIndex == 0xffffffff)
		{
			return;
		}

		// Enable or disable this button
		m_apToolBar->GetToolBarCtrl().EnableButton( nID, bEnable ? TRUE : FALSE );
	}

	// Check for menu
	CMenu* pMenu = GetMenu();
	if (pMenu != NULL)
	{
		// Get count of sub-menus
		UINT uiCount = pMenu->GetMenuItemCount();
		for (unsigned int ui = 0; ui < uiCount; ui++)
		{
			// Get this sub-menu
			CMenu* pSubMenu = pMenu->GetSubMenu( ui );
			if (pSubMenu)
			{
				// Get count of menu items
				UINT uiSubCount = pMenu->GetMenuItemCount();
				for (unsigned int uj = 0; uj < uiSubCount; uj++)
				{
					// Retrieve this item ID
					int nSubID = pSubMenu->GetMenuItemID( uj );
					
					// Compare item ID
					if (nSubID == nID)
					{
						// Enable or disable this item
						pSubMenu->EnableMenuItem( nSubID, 
							bEnable ? MF_ENABLED | MF_BYCOMMAND : MF_GRAYED | MF_BYCOMMAND );
						return;
					}	// end if nID matches nSubID
				}		// end for each item in this menu
			}			// end if pSubMenu != NULL
		}				// end for each sub-menu
	}
 }
//-------------------------------------------------------------------------------------------------
CDataEntryGrid* CFlexDataEntryDlg::getActiveGrid()
{
	// Check for previous choice
	if (m_pActiveGrid != NULL)
	{
		return m_pActiveGrid;
	}

	// Determine which Grid is active
	long lCount = m_vecGrids.size();
	int i;
	for (i = 0; i < lCount; i++)
	{
		// Retrieve this grid
		CDataEntryGrid*	pGrid = m_vecGrids[i];

		// Check state - for grids with a selected record
		if (pGrid->IsActive())
		{
			return pGrid;
		}
	}

	return NULL;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::setActiveGrid(CDataEntryGrid* pGrid)
{
	// Check each Grid
	long lCount = m_vecGrids.size();
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this grid
		CDataEntryGrid*	pThisGrid = m_vecGrids[i];

		// Compare
		if (pGrid != pThisGrid)
		{
			// Remove any selection from "other" grid
			pThisGrid->UnselectRecords();
		}
		// else indicate selected state
		else
		{
			// enable/disable the rubberbanding depending upon the grid configuration
			if (pThisGrid->IsRubberbandEnabled())
			{
				m_ipSRIR->SetParagraphTextHandlers(m_ipParagraphTextHandlers);
			}
			else
			{
				m_ipSRIR->ClearParagraphTextHandlers();
			}

			// enable/disable the swiping depending upon the grid configuration
			if (pThisGrid->IsSwipingEnabled())
			{
				getInputManager()->EnableInput1( "Text", "Select", NULL );
			}
			else
			{
				getInputManager()->DisableInput();
			}

			// Save this pointer
			m_pActiveGrid = pThisGrid;
		}
	}

	updateButtons();
}
//-------------------------------------------------------------------------------------------------
std::string CFlexDataEntryDlg::getFolder()
{
	// Build General folder from INI file and section name
	string strFolder = getINIPath();
	strFolder += "\\General";

	return strFolder;
}
//-------------------------------------------------------------------------------------------------
long CFlexDataEntryDlg::getGridSections(std::string strINIFile)
{
	// Create temporary PersistenceMgr
	INIFilePersistenceMgr	mgrSettings( strINIFile );

	// Check for defined section names
	for (int i = 1; i <= IDC_GROUP_MAX; i++)
	{
		// Create this section name
		CString	zName;
		zName.Format( "Grid%d", i );

		// Search for Grid Type within this section of INI file
		// Also search for Height
		string strFolder = getINIPath();
		strFolder += "\\";
		strFolder += zName.operator LPCTSTR();
		string strType = mgrSettings.getKeyValue( strFolder, gstrGRID_TYPE );
		string strHeight = mgrSettings.getKeyValue( strFolder, gstrGRID_HEIGHT );

		// Save the name and height, if Type and Height were found
		if ((strType.length() > 0) && (strHeight.length() > 0))
		{
			m_vecSectionNames.push_back( zName.operator LPCTSTR() );
			m_vecSectionHeights.push_back( strHeight );
		}
	}

	// Count of defined grids = size of Sections vector
	return m_vecSectionNames.size();
}
//-------------------------------------------------------------------------------------------------
std::string CFlexDataEntryDlg::getINIPath()
{
	// Get full path to INI file
	string strINI = getCurrentProcessEXEDirectory();
	strINI += "\\";

//	// Use command-line INI file, if defined
//	if (m_strCommandLine.length() > 0)
//	{
//		strINI += m_strCommandLine;
//	}
//	else
//	{
		// Use default INI file
		strINI += gstrINI_FILE;
//	}

	validateFileOrFolderExistence( strINI );
	return strINI;
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CFlexDataEntryDlg::getMiscUtils()
{
	if (m_ipMiscUtils == NULL)
	{
		m_ipMiscUtils.CreateInstance( CLSID_MiscUtils );
		ASSERT_RESOURCE_ALLOCATION( "ELI11103", m_ipMiscUtils != NULL );
	}
	
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
std::string CFlexDataEntryDlg::getOutputFile()
{
	// Retrieve and process name of output file from INI 
	string strOutputFile;
	string strText = getSetting( gstrOUTPUT_FILENAME, false );

	if (strText.empty())
	{
		// No INI definition, default to $DirOf(<SourceDocName>)\$FileNoExtOf(<SourceDocName>).pxt
		strText = "$DirOf(<SourceDocName>)\\$FileNoExtOf(<SourceDocName>).pxt";
	}

	// Replace each "<SourceDocName>" with filename of open image
	string strImage = asString( m_ipSRIR->GetImageFileName() );
	string strSearch = "<SourceDocName>";
	unsigned int uiPos = strText.find( strSearch.c_str() );
	while (uiPos != string::npos)
	{
		// Remove this <SourceDocName> and replace it with strImage
		strText.erase( uiPos, strSearch.length() );
		strText.insert( uiPos, strImage.c_str() );

		// Look for another <SourceDocName>
		uiPos = strText.find( strSearch.c_str() );
	}

	// Process input string to build output name
	TextFunctionExpander	tfe;
	strOutputFile = tfe.expandFunctions( strText );

	return strOutputFile;
}
//-------------------------------------------------------------------------------------------------
long CFlexDataEntryDlg::getRowHeightInPixels()
{
	// Construct temporary grid
	CDataEntryGrid*	pTempGrid = new CDataEntryGrid();

	// Create grid rectange
	CRect	rectDlg;
	GetWindowRect( &rectDlg );
	RECT rect;
	rect.left = giOFFSET_EDGE;
	rect.top = 100;
	rect.right = rectDlg.Width() - 2 * giOFFSET_EDGE;
	rect.bottom = rect.top + 200;

	// Create the grid - with border and both scroll bars
	pTempGrid->Create( 0x50b10000, rect, this, 500 );

	std::vector<std::string>	vecHeaders;
	vecHeaders.push_back( "Header1" );
	vecHeaders.push_back( "Header2" );

	std::vector<int>	vecWidths;
	vecWidths.push_back( 100 );
	vecWidths.push_back( 100 );

	// Configure the grid
	pTempGrid->PrepareGrid( vecHeaders, vecWidths, vecHeaders, 100, true, true );

	long lHeight = pTempGrid->GetRowHeight( 1 );
	delete pTempGrid;

	return lHeight;
}
//-------------------------------------------------------------------------------------------------
std::string	CFlexDataEntryDlg::getRSDFile()
{
	string strPath;

	// Get setting from INI file
	string strFile = getSetting( gstrMAIN_RSD_FILE, true );

	// Create fully qualified path to this file
	if (strFile.length() > 0)
	{
		strPath = getCurrentProcessEXEDirectory();
		strPath += "\\";
		strPath += strFile;
	}
	else
	{
		UCLIDException ue("ELI13115", "RSD file not defined in INI file!");
		ue.addDebugInfo("INI file", strPath);
		throw ue;
	}

	return strPath;
}
//-------------------------------------------------------------------------------------------------
std::string	CFlexDataEntryDlg::getSetting(std::string strKey, bool bRequired)
{
	// Create temporary PersistenceMgr
	INIFilePersistenceMgr	mgrSettings( getINIPath() );

	// Get value from key
	string strTemp = mgrSettings.getKeyValue( getFolder(), strKey );

	// Remove any leading and trailing whitespace
	strTemp = trim( strTemp, " \t", " \t" );

	// Check result
	if (strTemp.size() == 0 && bRequired)
	{
		UCLIDException ue( "ELI10815", "Required setting not defined." );
		ue.addDebugInfo( "Required Setting", strKey );
		throw ue;
	}

	return strTemp;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::handleCharacter(long nKeyCode)
{
	// Ignore all characters if in between files
	if (m_ipSRIR)
	{
		// Get path to the open image
		string strSRIRFile = asString( m_ipSRIR->GetImageFileName() );
		if (strSRIRFile.empty())
		{
			return;
		}
	}

	// Get ID of active grid
	CDataEntryGrid* pGrid = getActiveGrid();
	if (pGrid == NULL)
	{
		return;
	}
	int nID = pGrid->GetControlID();

	// Handle desired characters
	switch (nKeyCode)
	{
	// guiFDE_RIGHT = guiFDE_LEFT
	case guiFDE_RIGHT:
		{
			// Check for special key
			if (isVirtKeyCurrentlyPressed( guiFDE_LEFT_VK ))
			{
				// Shift+Tab for Previous cell
				//    lParam == 1 to indicate that this is a Tab and should 
				//    navigate out of this grid regardless of IsArrowNavigationDisabled()
				OnCellLeft( nID , 1 );
			}
			else
			{
				// Tab for Next cell
				//    lParam == 1 to indicate that this is a Tab and should 
				//    navigate out of this grid regardless of IsArrowNavigationDisabled()
				OnCellRight( nID , 1 );
			}
		}
		break;

	case guiFDE_UP:
		{
			// Up Arrow for cell Up
			OnCellUp( nID , 0 );
		}
		break;

	case guiFDE_DOWN:
		{
			// Down Arrow for cell Down
			OnCellDown( nID , 0 );
		}
		break;
	}
}
//-------------------------------------------------------------------------------------------------
bool CFlexDataEntryDlg::handleShortcutKey(WPARAM wParam)
{
	// Handle desired characters
	switch (wParam)
	{
	case guiFDE_FIND:
		{
			// Check for special key
			if (isVirtKeyCurrentlyPressed( guiFDE_FIND_VK ))
			{
				OnBtnFind();

				// Keypress has been handled
				return true;
			}
		}
		break;

	case guiFDE_SAVE:
		{
			// Check for special key
			if (isVirtKeyCurrentlyPressed( guiFDE_SAVE_VK ))
			{
				OnBtnSave();

				// Keypress has been handled
				return true;
			}
		}
		break;

	case guiFDE_CLEAR:
		{
			// Check for special key
			if (isVirtKeyCurrentlyPressed( guiFDE_CLEAR_VK ))
			{
				OnBtnClear();

				// Keypress has been handled
				return true;
			}
		}
		break;
	}

	// Not a defined shortcut key - or expected special key not pressed
	return false;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::handleSwipeInput(string strInput)
{
	// Get active grid
	CDataEntryGrid*	pGrid = getActiveGrid();
	if (pGrid != NULL && pGrid->IsSwipingEnabled())
	{
		int iRecord = pGrid->HandleSwipe( strInput );
	}
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::highlightAttribute(IAttributePtr ipAttr)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (ipAttr)
		{
			// Get spatial string of attribute value
			ISpatialStringPtr ipValue = ipAttr->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI11067", ipValue != NULL);

			// Get name of source document
			string strImage = ipValue->SourceDocName;

			// if the value is not a spatial string, or if the spatial string
			// does not have a source document associated with it, then just
			// delete all highlights and return
			if (ipValue->HasSpatialInfo() == VARIANT_FALSE || strImage.empty())
			{
				if (m_ipSRIR)
				{
					m_ipSRIR->DeleteTemporaryHighlight();
				}
				return;
			}

			if (m_ipSRIR)
			{
				// Get path to the open image
				string strSRIRFile = asString( m_ipSRIR->GetImageFileName() );

				// Is SourceDocName from USS the exact path to the open image?
				if (strSRIRFile.compare( strImage.c_str() ) == 0)
				{
					// Highlight the spatial string in the SRIR window
					m_ipSRIR->CreateTemporaryHighlight( ipValue );
				}
				// Else maybe image and VOA files have been moved together
				else
				{
					// Get the filenames
					string strVOAImageName = getFileNameFromFullPath( strImage.c_str(), true );
					string strSRIRImageName = getFileNameFromFullPath( strSRIRFile.c_str(), true );

					// Compare the filenames
					if (strSRIRImageName.compare( strVOAImageName.c_str() ) == 0)
					{
						// Highlight the spatial string in the SRIR window
						m_ipSRIR->CreateTemporaryHighlight( ipValue );
					}
				}
			}
		}
		else if (m_ipSRIR)
		{
			m_ipSRIR->DeleteTemporaryHighlight();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11064")
}
//-------------------------------------------------------------------------------------------------
bool CFlexDataEntryDlg::isFindHidden()
{
	// Default to visible
	bool bHidden = false;

	// Get setting from INI file - not required
	string strValue = getSetting( gstrTOOLBAR_FIND, false );
	if ((!strValue.empty()) && (asLong( strValue ) == 0))
	{
		bHidden = true;
	}

	return bHidden;
}
//-------------------------------------------------------------------------------------------------
bool CFlexDataEntryDlg::isSaveHidden()
{
	// Hide Save button if no output format is defined in INI file
	return !m_bOutputFormatDefined;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::loadMenu()
{
	// Load the menu and retrieve the File submenu
	CMenu menu;
	menu.LoadMenu( IDR_MENU1 );

	// Update File menu
	if (isSaveHidden())
	{
		// Retrieve File menu
		CMenu *pMenu = menu.GetSubMenu( 0 );
		// Remove separator
		pMenu->RemoveMenu( 1, MF_BYPOSITION );
		// Remove Save
		pMenu->RemoveMenu( 0, MF_BYPOSITION );
	}

	// Update Edit menu
	if (isFindHidden())
	{
		// Retrieve Edit menu
		CMenu *pMenu = menu.GetSubMenu( 1 );
		// Remove Find
		pMenu->RemoveMenu( 0, MF_BYPOSITION );
	}

	// Associate the modified menu with the window
	SetMenu( &menu );
	menu.Detach();
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::parseOutputTemplate(std::string strPath)
{
	// File must exist
	validateFileOrFolderExistence( strPath );

	// Use CommentedTextFileReader to read format lines and skip blank lines
	ifstream ifs( strPath.c_str() );
	CommentedTextFileReader fileReader( ifs, "//", true );
	while (!ifs.eof())
	{
		string strLine( "" );
		strLine = fileReader.getLineText();
		processFormatLine( strLine );
	}

	// Set flag
	m_bOutputFormatDefined = true;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::populateGrids(IIUnknownVectorPtr ipAttributes) 
{
	// Provide Attributes to each grid
	long lCount = m_vecGrids.size();
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this grid
		CDataEntryGrid*	pGrid = m_vecGrids[i];

		// Each grid will apply the appropriate Query
		pGrid->Populate( ipAttributes );

		// Update state of navigation controls, if any
		updateNavigationControls( pGrid );
	}
}
//-------------------------------------------------------------------------------------------------
long CFlexDataEntryDlg::prepareGrid(CDataEntryGrid* pGrid, long lGridControlID, int iIndex, 
									long lRowHeight, long lStartOffset, CWnd* pParent)
{
	// Get dialog dimensions
	CRect	rectDlg;
	GetWindowRect( &rectDlg );

	// Get grid height as number of rows from INI file * row height
	long lNumRows = asLong( m_vecSectionHeights[iIndex] );
	long lGridHeight = lNumRows * lRowHeight;

	// Determine height of grid, buttons and label group
	long	lGroupHeight = giGROUP_SIZE + lGridHeight;

	// Retrieve desired space for vertical scrollbar that may be needed in the dialog
	string strPixels = getSetting( gstrVERTICAL_SCROLL_PIXELS, false );
	long lScrollPixels = 0;
	if (!strPixels.empty())
	{
		lScrollPixels = asLong( strPixels );
	}

	// Define position of grid rectangle - allowing space for dialog scrollbar
	RECT rect;
	rect.left = giOFFSET_EDGE;
	rect.top = lStartOffset;
	rect.right = rectDlg.Width() - 2 * giOFFSET_EDGE - lScrollPixels;
	rect.bottom = rect.top + lGridHeight;

	// Create the grid - with border and both scroll bars
	pGrid->Create( 0x50b10000, rect, this, lGridControlID );

	// Apply INI settings
	pGrid->SetID( m_vecSectionNames[iIndex], lGridControlID, getINIPath(), 
		pParent );

	// Disable row and column dragging (P16 #1514)
	CGXGridParam* pParam = pGrid->GetParam();
	ASSERT_RESOURCE_ALLOCATION("ELI15624", pParam != NULL);
	pParam->EnableMoveCols( FALSE );
	pParam->EnableMoveRows( FALSE );

	pGrid->DoResize( pGrid->GetGridScrollPixels() );

	return lStartOffset + giGROUP_SIZE + lGridHeight + giOFFSET_EDGE;
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::processFormatLine(std::string strLine)
{
	if (!strLine.empty())
	{
		unsigned long ulLength = strLine.length();
		unsigned long ulPos = strLine.find_first_of( "=" );

		// Skip any line with missing, leading, or trailing equal sign
		if ((ulPos != string::npos) && (ulPos != 0) && (ulPos != ulLength - 1))
		{
			// Retrieve tag and definition
			string strTag = strLine.substr( 0, ulPos );
			string strDef = strLine.substr( ulPos + 1, ulLength - ulPos - 1 );

			// Add to PXT vectors
//			m_mapOutputTagsToDefinitions[strTag] = strDef;
			m_vecPXTTags.push_back( strTag );
			m_vecPXTDefs.push_back( strDef );

			// Add default map entries
			addDefaultMapItems( strDef, "<", ">", " " );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::readOutputFormatTemplate(std::string strINIFile)
{
	// Get filename to Output Template - not required
	string strFile = getSetting( gstrOUTPUT_TEMPLATE, false );

	// Create fully qualified path to this file
	string	strPath;
	if (!strFile.empty())
	{
		// Make full path if not already an absolute path
		if (isAbsolutePath( strFile ))
		{
			strPath = strFile;
		}
		else
		{
			// Assume that the template file is in the folder with the EXE
			strPath = getCurrentProcessEXEDirectory();
			strPath += "\\";
			strPath += strFile;
		}
	}

	// Parse the file
	if (!strPath.empty())
	{
		parseOutputTemplate( strPath );
	}
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::updateButtons()
{
	// Get pointer to active grid
	CDataEntryGrid*	pActiveGrid = getActiveGrid();

	// Check each Grid
	long lCount = m_vecGrids.size();
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this grid
		CDataEntryGrid*	pThisGrid = m_vecGrids[i];

		// Define control ID for Remove and Add button
		long lID = pThisGrid->GetControlID();
		long lRemoveID = IDC_CONTROL_BASE + IDC_OFFSET_DELETE * IDC_GROUP_MAX + i;
		long lAddID = IDC_CONTROL_BASE + IDC_OFFSET_ADD * IDC_GROUP_MAX + i;
		CButton *pButton = (CButton *)GetDlgItem( lRemoveID );
		CButton *pButton2 = (CButton *)GetDlgItem( lAddID );

		// Enable or disable the Add button
		string strFile = m_ipSRIR->GetImageFileName();
		if (pButton2 != NULL)
		{
			// Must have image loaded PLUS must not disable Add button
			pButton2->EnableWindow( 
				((strFile.length() > 0) && (!pThisGrid->IsAddButtonDisabled())) ? TRUE : FALSE );
		}

		// Compare against active grid
		if (pActiveGrid != pThisGrid)
		{
			// Inactive, disable the Remove button
			if (pButton != NULL)
			{
				pButton->EnableWindow( FALSE );
			}
		}
		else
		{
			// This is the active grid, check record selection
			IAttributePtr ipSel = pActiveGrid->GetSelectedAttribute();

			// Enable or disable the Remove button
			if (pButton != NULL)
			{
				pButton->EnableWindow( (ipSel != NULL) ? TRUE : FALSE );
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::updateNavigationControls(CDataEntryGrid* pGrid)
{
	// Get control IDs for buttons
	long lPreviousID = pGrid->GetControlID() + IDC_GROUP_MAX * (IDC_OFFSET_PREVIOUS - IDC_OFFSET_GRID);
	long lNextID = pGrid->GetControlID() + IDC_GROUP_MAX * (IDC_OFFSET_NEXT - IDC_OFFSET_GRID);
	if (pGrid->AllowRecordNavigation())
	{
		long lCount = pGrid->GetRecordCount();
		if (lCount == 0)
		{
			// Clear the text
			long lActualID = pGrid->GetControlID() + 
				IDC_GROUP_MAX * (IDC_OFFSET_ACTUAL - IDC_OFFSET_GRID);
			CEdit*	pEdit = (CEdit *)GetDlgItem( lActualID );
			if (pEdit != NULL)
			{
				pEdit->SetWindowTextA( "" );
			}

			// Disable the buttons
			CButton* pButton = (CButton *)GetDlgItem( lPreviousID );
			if (pButton != NULL)
			{
				pButton->EnableWindow( FALSE );
			}

			pButton = (CButton *)GetDlgItem( lNextID );
			if (pButton != NULL)
			{
				pButton->EnableWindow( FALSE );
			}
		}
		else
		{
			// Compute and update the Actual text
			long lActive = pGrid->GetActiveRecord();
			string strText = asString( lActive + 1 );
			strText += " of ";
			strText += asString( lCount );

			long lActualID = pGrid->GetControlID() + 
				IDC_GROUP_MAX * (IDC_OFFSET_ACTUAL - IDC_OFFSET_GRID);
			CEdit*	pEdit = (CEdit *)GetDlgItem( lActualID );
			if (pEdit != NULL)
			{
				pEdit->SetWindowTextA( strText.c_str() );
			}

			// Enable or disable the Previous button
			CButton* pButton = (CButton *)GetDlgItem( lPreviousID );
			if (pButton != NULL)
			{
				pButton->EnableWindow( (lActive > 0) ? TRUE : FALSE );
			}

			// Enable or disable the Next button
			pButton = (CButton *)GetDlgItem( lNextID );
			if (pButton != NULL)
			{
				pButton->EnableWindow( (lActive < lCount - 1) ? TRUE : FALSE );
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFlexDataEntryDlg::updateSRIRToolbar()
{
	//////////////////////////////////
	// These buttons are always hidden
	//////////////////////////////////
	m_ipSRIR->ShowToolbarCtrl( kBtnSave, VARIANT_FALSE );
	m_ipSRIR->ShowToolbarCtrl( kBtnEditZoneText, VARIANT_FALSE );
	m_ipSRIR->ShowToolbarCtrl( kBtnOpenSubImgInWindow, VARIANT_FALSE );

	//////////////////////////////////
	// The following buttons are configured via INI file
	//////////////////////////////////
	// Rotate Counterclockwise
	string strText = getSetting( gstrSRIR_ROTATE_CCW, false );
	long lValue = 1;
	if (!strText.empty())
	{
		lValue = asLong( strText );
	}
	if (lValue == 0)
	{
		// Hide the button
		m_ipSRIR->ShowToolbarCtrl( kBtnRotateCounterClockwise, VARIANT_FALSE );
	}

	// Rotate Clockwise
	strText = getSetting( gstrSRIR_ROTATE_CW, false );
	lValue = 1;
	if (!strText.empty())
	{
		lValue = asLong( strText );
	}
	if (lValue == 0)
	{
		// Hide the button
		m_ipSRIR->ShowToolbarCtrl( kBtnRotateClockwise, VARIANT_FALSE );
	}

	// First Page
	strText = getSetting( gstrSRIR_PAGE_FIRST, false );
	lValue = 1;
	if (!strText.empty())
	{
		lValue = asLong( strText );
	}
	if (lValue == 0)
	{
		// Hide the button
		m_ipSRIR->ShowToolbarCtrl( kBtnFirstPage, VARIANT_FALSE );
	}

	// Last Page
	strText = getSetting( gstrSRIR_PAGE_LAST, false );
	lValue = 1;
	if (!strText.empty())
	{
		lValue = asLong( strText );
	}
	if (lValue == 0)
	{
		// Hide the button
		m_ipSRIR->ShowToolbarCtrl( kBtnLastPage, VARIANT_FALSE );
	}

	// Delete Highlights
	strText = getSetting( gstrSRIR_DEL_HIGHLIGHT, false );
	lValue = 1;
	if (!strText.empty())
	{
		lValue = asLong( strText );
	}
	if (lValue == 0)
	{
		// Hide the button
		m_ipSRIR->ShowToolbarCtrl( kBtnDeleteEntities, VARIANT_FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
bool CFlexDataEntryDlg::validateSave()
{
	// Throw exception if Save button is hidden
	if (isSaveHidden())
	{
		throw UCLIDException("ELI13147", "Cannot validate Save if button is hidden!");
	}

	// Create temporary PersistenceMgr
	INIFilePersistenceMgr	mgrSettings( getINIPath() );

	// Check each grid for warning levels
	long lCount = m_vecGrids.size();
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this Grid
		CDataEntryGrid* pGrid = m_vecGrids[i];
		ASSERT_RESOURCE_ALLOCATION("ELI13148", pGrid != NULL);

		// Search for Min/Max Count Warnings within this section of INI file
		string strFolder = getINIPath();
		strFolder += "\\";
		strFolder += pGrid->GetID().c_str();
		string strMin = mgrSettings.getKeyValue( strFolder, gstrWARN_ON_SAVE_IF_ROWS_LESS_THAN );
		string strMax = mgrSettings.getKeyValue( strFolder, gstrWARN_ON_SAVE_IF_ROWS_MORE_THAN );

		// Get actual number of rows - except for Type D
		if (pGrid->GetType() == "D")
		{
			continue;
		}
		int nActualRows = pGrid->GetNonEmptyRowCount();

		////////////////
		// Check Minimum
		////////////////
		if (!strMin.empty())
		{
			// Compare defined minimum against number of rows in this grid
			long lMin = asLong( strMin );
			if ((lMin > 0) && (nActualRows < lMin))
			{
				// Provide prompt to user
				CString zPrompt;
				if (nActualRows == 1)
				{
					zPrompt.Format( 
						"%s grid contains 1 row instead of the expected minimum %d.\r\n\r\nAccept this data for output?", 
						pGrid->GetGridLabel().c_str(), lMin );
				}
				else
				{
					zPrompt.Format( 
						"%s grid contains %d rows instead of the expected minimum %d.\r\n\r\nAccept this data for output?", 
						pGrid->GetGridLabel().c_str(), nActualRows, lMin );
				}

				int iResult = MessageBox( zPrompt, "Allow Save", 
					MB_ICONWARNING | MB_YESNO );
				if (iResult == IDNO)
				{
					// Present results are not acceptable, 
					// invalidate this Save request and return
					return false;
				}
				// else "too few rows" is accepted, allow save
			}	// end if too few rows found in this grid
		}		// end if Min Num Rows defined in INI file for this grid

		////////////////
		// Check Maximum
		////////////////
		if (!strMax.empty())
		{
			// Compare defined maximum against number of rows in this grid
			long lMax = asLong( strMax );
			if ((lMax > 0) && (nActualRows > lMax))
			{
				// Provide prompt to user
				CString zPrompt;
				if (nActualRows == 1)
				{
					zPrompt.Format( 
						"%s grid contains 1 row instead of the expected maximum %d.\r\n\r\nIgnore additional row and accept this data for output?", 
						pGrid->GetGridLabel().c_str(), lMax );
				}
				else
				{
					zPrompt.Format( 
						"%s grid contains %d rows instead of the expected maximum %d.\r\n\r\nIgnore additional rows and accept this data for output?", 
						pGrid->GetGridLabel().c_str(), nActualRows, lMax );
				}

				int iResult = MessageBox( zPrompt, "Allow Save", 
					MB_ICONWARNING | MB_YESNO );
				if (iResult == IDNO)
				{
					// Present results are not acceptable, 
					// invalidate this Save request and return
					return false;
				}
				// else "too many rows" is accepted, allow save
			}	// end if too many rows found in this grid
		}		// end if Max Num Rows defined in INI file for this grid
	}			// end for each grid

	// If we have reached here, either 
	// - no out-of-bounds settings were defined
	// - no out-of-bounds settings were exceeded
	// - each out-of-bounds condition has been accepted
	return true;
}
//-------------------------------------------------------------------------------------------------
