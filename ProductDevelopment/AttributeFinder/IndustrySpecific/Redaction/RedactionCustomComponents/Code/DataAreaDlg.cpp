// DataAreaDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "resource.h"
#include "DataAreaDlg.h"
#include "Settings.h"
#include "Shortcuts.h"
#include "RedactionCCUtils.h"
#include "IDShieldData.h"
#include "ExemptionCodesDlg.h"

#include <SRWShortcuts.h>
#include <SpecialStringDefinitions.h>
#include <Common.h> // for registry path information
#include <UCLIDException.h>
#include <cpputil.h>
#include <IconControl.h>
#include <COMUtils.h>
#include <INIFilePersistenceMgr.h>
#include <LicenseMgmt.h>
#include <MiscLeadUtils.h>
#include <RegistryPersistenceMgr.h>
#include <Win32Util.h>

#ifdef _VERIFICATION_LOGGING
#include <ThreadSafeLogFile.h>
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// String constant for default title bar caption
const string	gstrTITLE = "ID Shield";

// Number of pages per row in Summary grid
const int	giPAGES_PER_ROW					= 8;

const long  gnREDACTED_ZONE_COLOR = RGB(100, 149, 237);

// Directory where exemption code xml files are stored
const string gstrEXEMPTION_DIRECTORY = 
#ifdef _DEBUG
	"..\\..\\ProductDevelopment\\AttributeFinder\\IndustrySpecific\\Redaction\\RedactionCustomComponents\\ExemptionCodes";
#else
	"..\\IDShield\\ExemptionCodes";
#endif

// File name of the VOA File Viewer application
const string gstrVOA_VIEWER_FILENAME = "VoaFileViewer.exe";

// String constants for registry key
const string gstrREDACTIONCC_PATH = gstrAF_REG_ROOT_FOLDER_PATH + string("\\IndustrySpecific\\Redaction\\RedactionCustomComponents");
const string gstrIDSHIELD_FOLDER = "\\IDShield";
const string gstrQUEUE_SIZE = "NumPreviousDocsToQueue";

// Default max queue size
const long gnDEFAULT_QUEUE_SIZE = 5;

// constants to refer to the columns in the data grid
const int giDATAGRID_TYPE_COLUMN = 3;
const int giDATAGRID_PAGE_COLUMN = 4;
const int giDATAGRID_STATUS_COLUMN = 5;

// String constant for manual redaction
const string gstrMANUALREDACTIONTEXT = "Manual";

//-------------------------------------------------------------------------------------------------
// DocumentData
//-------------------------------------------------------------------------------------------------
CDataAreaDlg::DocumentData::DocumentData(const string& strOriginal, const string& strInput, 
	const string& strOutput, const string& strVoa, const string& strDocType, long lFileId)
	: m_strOriginal(strOriginal),
	  m_strInput(strInput),
	  m_strOutput(strOutput),
	  m_strVoa(strVoa),
	  m_strDocType(strDocType),
	  m_lFileId(lFileId)
{
}
//-------------------------------------------------------------------------------------------------
// DataItem
//-------------------------------------------------------------------------------------------------
CDataAreaDlg::DataItem::DataItem()
	: m_ipAttribute(NULL),
	  m_pDisplaySettings(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CDataAreaDlg::DataItem::~DataItem()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18524");
}
//-------------------------------------------------------------------------------------------------
IRasterZonePtr CDataAreaDlg::DataItem::getFirstRasterZone()
{
	// Note that we are assuming the data item has at least one raster zone. If a data item has 
	// been created for a non-spatial attribute or an attribute with a NULL value, then it should 
	// not appear in the data grid and so it is a logic error.

	// Get the attribute's value
	ISpatialStringPtr ipValue = m_ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI23555", ipValue != NULL);

	// Get the raster zones associated with the attribute
	IIUnknownVectorPtr ipRasterZones = ipValue->GetOriginalImageRasterZones();
	ASSERT_RESOURCE_ALLOCATION("ELI23556", ipRasterZones != NULL);

	// Return the first raster zone
	return ipRasterZones->At(0);
}
//-------------------------------------------------------------------------------------------------
ExemptionCodeList CDataAreaDlg::DataItem::getExemptionCodes()
{
	return m_codes;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::DataItem::setExemptionCodes(const ExemptionCodeList& codes)
{
	m_codes = codes;
	if (m_pDisplaySettings != NULL)
	{
		m_pDisplaySettings->setExemptionCodes(m_codes.getAsString());
	}
}

//-------------------------------------------------------------------------------------------------
// SortDataItems
//-------------------------------------------------------------------------------------------------
CDataAreaDlg::CompareDataItems::CompareDataItems(vector<DataItem>* pvecDataItems)
	: m_pvecDataItems(pvecDataItems)
{
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::CompareDataItems::operator()(long& nIndex0, long& nIndex1)
{
	// Retrieve two DataItem objects to be compared
	DataItem& item0 = (*m_pvecDataItems)[nIndex0];
	DataItem& item1 = (*m_pvecDataItems)[nIndex1];

	// Get and compare page numbers
	long nFirstPage0 = item0.m_pDisplaySettings->getPageNumber();
	long nFirstPage1 = item1.m_pDisplaySettings->getPageNumber();

	if (nFirstPage0 < nFirstPage1)
	{
		return true;
	}
	else if(nFirstPage1 < nFirstPage0)
	{
		return false;
	}

	//////////////////////////////////////////
	// Compare associated LongRectangle fields - removed 01/13/06 WEL P16 #1706
	// Replaced 02/28/06 WEL
	//////////////////////////////////////////
	// Get associated LongRectangles
	IRasterZonePtr ipZone0 = item0.getFirstRasterZone();
	if ( ipZone0 == NULL )
	{
		return true;
	}

	IRasterZonePtr ipZone1 = item1.getFirstRasterZone();
	if ( ipZone1 == NULL )
	{
		return false;
	}

	// NOTE: GetRectangularBounds may exceed page boundaries,
	// but this is not a problem for comparison purposes.
	ILongRectanglePtr ipRectS0 = ipZone0->GetRectangularBounds(NULL);
	ILongRectanglePtr ipRectS1 = ipZone1->GetRectangularBounds(NULL);

	// Since they are on the same page we will compare their spatial rectangles
	// if the boxes do not over lap, the one "on-top" (higher on the page) 
	// will be considered less
	long nTopS0 = ipRectS0->Top;
	long nBottomS0 = ipRectS0->Bottom;
	long nTopS1 = ipRectS1->Top;
	long nBottomS1 = ipRectS1->Bottom;

	if (nBottomS0 < nTopS1)
	{
		return true;
	}
	else if(nBottomS1 < nTopS0)
	{
		return false;
	}

	// if the overlap is less than X% of smaller zone then
	// we will say the zone that starts higher on the page is less
	long nHeightS0 = nBottomS0 - nTopS0;
	long nHeightS1 = nBottomS1 - nTopS1;

	long nMinHeight = nHeightS0 < nHeightS1 ? nHeightS0 : nHeightS1;
	long nMaxHeight = nHeightS0 < nHeightS1 ? nHeightS1 : nHeightS0;

	long nDiff0 = nBottomS0 - nTopS1;
	long nDiff1 = nBottomS1 - nTopS0;

	// Note that this overlap can be greater than the nMinHeight 
	// in the case where one zone is totally contained (in y) 
	// within the other.  We will cap it at nMinHeight.
	long nOverlap = nDiff0 < nDiff1 ? nDiff0 : nDiff1;
	nOverlap = nMinHeight < nOverlap ? nMinHeight : nOverlap;

	double dPercentOverlap = 100.0 * (double)nOverlap / (double)nMinHeight;

	if (dPercentOverlap < 20)
	{
		if (nTopS0 < nTopS1)
		{
			return true;
		}
		else if(nTopS1 < nTopS0)
		{
			return false;
		}
	}

	long nLeftS0 = ipRectS0->Left;
	long nLeftS1 = ipRectS1->Left;
	if (nLeftS0 < nLeftS1)
	{
		return true;
	}
	else if(nLeftS1 < nLeftS0)
	{
		return false;
	}

	if (nTopS0 < nTopS1)
	{
		return true;
	}
	else if(nTopS1 < nTopS0)
	{
		return false;	
	}

	return false;
}

//-------------------------------------------------------------------------------------------------
// CDataAreaDlg dialog
//-------------------------------------------------------------------------------------------------
CDataAreaDlg::CDataAreaDlg(RedactionUISettings UISettings, CWnd* pParent /*=NULL*/)
	: CDialog(CDataAreaDlg::IDD, pParent),
	  m_strINIPath(""),
	  m_bInitialized(false),
	  m_iTotalPages(0),
	  m_ipSRIR(NULL),
	  m_ipOCX(NULL),
	  m_pActiveDDS(NULL),
	  m_crSelection(RGB(255,0,0)),
	  m_bOpenVoaOnShiftSave(false),
	  m_iDataItemIndex(-1),
	  m_iActivePage(0),
	  m_lRefCount(0),
	  m_bVerified(false),
	  m_bEnableGeneral(false),
	  m_bEnableToggle(false),
	  m_bEnablePrevious(false),
	  m_bEnableApplyExemptions(false),
	  m_bEnableLastExemptions(false),
	  m_bEnableApplyAllExemptions(false),
  	  m_ipEngine(NULL),
	  m_apExemptionsDlg(NULL),
	  m_bHasLastExemptions(false),
	  m_UISettings(UISettings),
	  m_bActiveConfirmationDlg(false),
	  m_bChangesMadeForHistory(false),
	  m_bChangesMadeToMostRecentDocument(false),
	  m_nNumPreviousDocsToQueue(gnDEFAULT_QUEUE_SIZE),
	  m_nNumPreviousDocsQueued(0),
	  m_nPositionInQueue(0),
	  m_bDocCommittedToHistory(false),
	  m_ipFAMDB(NULL),
	  m_strDefaultRedactionType(""),
	  m_strLastSelectedRedactionType(""),
	  m_hWndSRIR(NULL),
	  m_ipIDShieldDB(NULL)
{
	// initialize the current file to an empty string
	setCurrentFileName("");

	//{{AFX_DATA_INIT(CFlexDataEntryDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
CDataAreaDlg::~CDataAreaDlg()
{
	try
	{
		// Release interface pointers here so that we can catch any exceptions that
		// may be generated in the process
		m_ipSRIR = NULL;
		m_ipOCX = NULL;

		// Nothing to clean up if INI file not valid
		if (!IsValidINI())
		{
			return;
		}

		// Delete the DataConfidenceLevel objects and empty the map
		for (map<string, DataConfidenceLevel*>::iterator it = m_mapDCLevels.begin(); 
			it != m_mapDCLevels.end(); it++)
		{
			DataConfidenceLevel* pDCL = it->second;
			delete pDCL;
		}
		m_mapDCLevels.clear();

		// Delete the display settings objects stored in history
		int iSize = m_vecDataItemHistory.size();
		for (int i = 0; i < iSize; i++)
		{
			clearDataItemVector(m_vecDataItemHistory[i]);
		}
		m_vecDataItemHistory.clear();

		// If we have not yet committed the current m_vecDataItems to history,
		// its items need to be deleted
		if (m_bDocCommittedToHistory == false)
		{
			clearDataItemVector(m_vecDataItems);
		}

		// Clear the Grids
		m_GridData.Clear();
		m_GridPages.Clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15344")
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDataAreaDlg)
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDataAreaDlg, CDialog)
	//{{AFX_MSG_MAP(CDataAreaDlg)
	ON_WM_PAINT()
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_COMMAND(ID_BUTTON_SAVE, OnButtonSave)
	ON_COMMAND(ID_BUTTON_TOGGLE_REDACTION, OnButtonToggleRedact)
	ON_COMMAND(ID_BUTTON_PREVIOUS_ITEM, OnButtonPreviousItem)
	ON_COMMAND(ID_BUTTON_NEXT_ITEM, OnButtonNextItem)
	ON_COMMAND(ID_BUTTON_ZOOM_TO_ITEM, OnButtonZoom)
	ON_MESSAGE(WM_NOTIFY_GRID_LCLICK, OnLButtonLClkRowCol)
	ON_MESSAGE(WM_NOTIFY_CELL_DBLCLK, OnDoubleClickRowCol)
	ON_MESSAGE(WM_NOTIFY_CELL_MODIFIED, OnModifyCell)
	ON_COMMAND(ID_BUTTON_OPTIONS, OnButtonOptions)
	ON_COMMAND(ID_BUTTON_ABOUT_ID_SHIELD, OnButtonHelpAbout)
	ON_WM_CLOSE()
	ON_WM_TIMER()
	ON_COMMAND(ID_BUTTON_PREVIOUS, OnButtonPreviousDoc)
	ON_COMMAND(ID_BUTTON_NEXT, OnButtonNextDoc)
	ON_WM_NCACTIVATE()
	ON_COMMAND(ID_BUTTON_STOP, OnButtonStop)
	ON_COMMAND(ID_BUTTON_APPLY_EXEMPTION, OnButtonApplyExemptions)
	ON_COMMAND(ID_BUTTON_ALL_EXEMPTION, OnButtonApplyAllExemptions)
	ON_COMMAND(ID_BUTTON_LAST_EXEMPTION, OnButtonLastExemptions)
	//}}AFX_MSG_MAP
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXT,0x0000,0xFFFF,OnToolTipNotify)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::IsValidINI()
{
	bool bValid = false;

	try
	{
		// Get file - existence is also tested in getINIPath()
		string strINI = getINIPath();

		// Check that INI file exists
		if (isFileOrFolderValid( strINI ))
		{
			bValid = true;
		}
	}
	catch (...) 
	{
		// Ignore any exception and just return false
	}

	return bValid;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::SetInputFile(const string& strInputFile)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	// Update the displaying filename so the user knows which document is being 
	// processed. This also informs the user if files are being skipped as in the
	// case of using only HCData or LCData for verification. 
	setCurrentFileName( getFileNameFromFullPath(strInputFile) );
	SetDlgItemText( IDC_EDIT_DOC_NAME, m_strSourceDocName.c_str() );

	// Invalidate the window and force it to redraw now instead of the next WM_PAINT message
	GetDlgItem( IDC_EDIT_DOC_NAME )->Invalidate();
	GetDlgItem( IDC_EDIT_DOC_NAME )->UpdateWindow();

	////////////////
	// Load VOA
	////////////////

	// Bring the name of the VOA file
	string strVoa = getVoaFileName(strInputFile);
	
	// Load VOA file
	IIUnknownVectorPtr ipVOA(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI11247", ipVOA != NULL);
	if ( isFileOrFolderValid(strVoa) )
	{
		ipVOA->LoadFrom(_bstr_t( strVoa.c_str() ), VARIANT_FALSE);
	}

	///////////////////////////
	// Load image file
	///////////////////////////

	long lFileId = getIDShieldDB()->GetFileID(strInputFile.c_str());
	m_document = getDocumentData(strInputFile, strVoa, ipVOA, lFileId);

	// Set Start time and clear dirty flag
	m_tmStarted = COleDateTime::GetCurrentTime();
	m_bChangesMadeForHistory = false;
	m_bChangesMadeToMostRecentDocument = false;
	
	// Hide certain SRIR toolbar buttons
	m_ipSRIR->ShowToolbarCtrl( kBtnOpenImage, VARIANT_FALSE );
	m_ipSRIR->ShowToolbarCtrl( kBtnSave, VARIANT_FALSE );
	m_ipSRIR->ShowToolbarCtrl( kBtnDeleteEntities, VARIANT_FALSE );
	m_ipSRIR->ShowToolbarCtrl( kBtnOpenSubImgInWindow, VARIANT_FALSE );
	m_ipSRIR->ShowToolbarCtrl( kBtnPTH, VARIANT_FALSE );
	m_ipSRIR->ShowToolbarCtrl( kBtnEditZoneText, VARIANT_FALSE );

	// Open the image file in Spot Recognition Window and update title bar
	loadImage(m_document);

	// Initialize the Pages grid 
	long lPageCount =  m_ipSRIR->GetTotalPages();
	initPages( lPageCount );

	// Populate the Data Grid
	populateDataGrid( ipVOA );

	// Specify that the data we just loaded has not yet been committed to history
	m_bDocCommittedToHistory = false;

	// Set the active page to the first page if it is zero
	if (m_iActivePage == 0 && lPageCount > 0)
	{
		m_iActivePage = 1;
	}

	// Update toolbar button
	updateButtons();
	
	// Reset and start the stopwatch since this is a new file
	m_swCurrTimeViewed.reset();
	m_swCurrTimeViewed.start();
	
	// Initialize m_idsCurrData(P16 #2901)
	m_idsCurrData.clear();

	// Set flag
	m_bVerified = true;
}
//-------------------------------------------------------------------------------------------------
CDataAreaDlg::DocumentData CDataAreaDlg::getDocumentData(const string& strOriginalImage, 
	const string& strVoa, IIUnknownVectorPtr ipVOA, long lFileId)
{
	// Retrieve document type from VOA file
	string strDocType = getDocumentType(ipVOA);
	if ( strDocType.empty() )
	{
		strDocType = gstrSPECIAL_UNCLASSIFIED;
	}

	// Prepare variable to hold source document name if necessary
	string strSourceDocName = "";
	bool bLoadedSourceDoc = false;

	// Get the name of the output file
	string strOutput = getOutputFileName(strOriginalImage);

	// Check for option to use redacted image
	if (m_UISettings.getInputRedactedImage())
	{
		// If this is valid image we are done
		string strExt2 = getExtensionFromFullPath(strOutput, true);
		if (isImageFileExtension(strExt2) || isThreeDigitExtension(strExt2))
		{
			if (isFileOrFolderValid(strOutput))
			{
				return DocumentData(strOriginalImage, strOutput, strOutput, strVoa, strDocType, lFileId);
			}
		}

		// Load source document name from VOA
		strSourceDocName = getSourceDocName(ipVOA);
		bLoadedSourceDoc = true;

		// Check if the redacted source document is valid
		if (!strSourceDocName.empty())
		{
			string strRedactedSourceDoc = getOutputFileName(strSourceDocName);
			if (isFileOrFolderValid(strRedactedSourceDoc))
			{
				return DocumentData(strOriginalImage, strRedactedSourceDoc, strOutput, strVoa, strDocType, lFileId);
			}
		}
	}

	// Check if original image is valid
	string strExt2 = getExtensionFromFullPath(strOriginalImage, true);
	if (isImageFileExtension(strExt2) || isThreeDigitExtension(strExt2))
	{
		if (isFileOrFolderValid(strOriginalImage))
		{
			return DocumentData(strOriginalImage, strOriginalImage, strOutput, strVoa, strDocType, lFileId);
		}
	}

	// Load source document name if not already loaded
	if (!bLoadedSourceDoc)
	{
		strSourceDocName = getSourceDocName(ipVOA);
		bLoadedSourceDoc = true;
	}

	// Check if the source document exists
	if (!strSourceDocName.empty() && isFileOrFolderValid(strSourceDocName))
	{
		return DocumentData(strOriginalImage, strSourceDocName, strOutput, strVoa, strDocType, lFileId);
	}

	// Cannot find image file, throw exception
	UCLIDException ue("ELI11248", "Cannot find image file.");
	ue.addDebugInfo("Image File", strOriginalImage);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::InitFAMTagManager(IFAMTagManagerPtr ipFAMTagManager)
{
	if (!m_ipFAMTagManager)
	{
		m_ipFAMTagManager = ipFAMTagManager;
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::SetFAMDB(IFileProcessingDBPtr ipFAMDB)
{
	m_ipFAMDB = ipFAMDB;
	getIDShieldDB()->FAMDB = ipFAMDB;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::setCurrentFileName(string strFile)
{
	m_strSourceDocName = strFile;
	m_timeCurrentFileLastUpdated = time(NULL);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::createOutputFiles()
{
	INIT_EXCEPTION_AND_TRACING("MLI02718");
	try
	{
		// Get open image
		DocumentData& document = getCurrentDocument();
		_lastCodePos = "20";

		// Apply Redactions to image. Store whether any redactions have been performed.
		bool bRedacted = doRedaction(document);
		_lastCodePos = "30";

		// Update Metadata
		writeMetadata(document);
		_lastCodePos = "40";

		// Handle feedback collection
		handleFeedback(document, bRedacted);
		_lastCodePos = "50";

		// Clear the dirty flag
		m_bChangesMadeForHistory = false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25224");
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::createToolBar()
{
	// Create the toolbar
	m_apToolBar = auto_ptr<CToolBar>(new CToolBar());
	if (m_apToolBar->CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
    | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) )
	{
		m_apToolBar->LoadToolBar( IDR_DATAAREA_TOOLBAR );
	}

	m_apToolBar->SetBarStyle(m_apToolBar->GetBarStyle() |
		CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC);

	// Must set TBSTYLE_TOOLTIPS here in order to get tool tips
	m_apToolBar->ModifyStyle( 0, TBSTYLE_TOOLTIPS );

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

	// Refresh display
	UpdateWindow();
	Invalidate();
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::doRedaction(const DocumentData& document)
{
	INIT_EXCEPTION_AND_TRACING("MLI02720");
	try
	{
		try
		{
			// The file is no longer being viewed so stop the stop watch
			m_swCurrTimeViewed.stop();
			_lastCodePos = "10";

			// Create vector for zones to redact
			vector<PageRasterZone> vecZones;

			// Initialize m_idsCurrData(P16 2901)
			m_idsCurrData.clear();
			_lastCodePos = "20";

			// Get the text color
			COLORREF crTextColor = invertColor(m_UISettings.getFillColor());
			_lastCodePos = "30";

			// Step through Data items
			vector<DataItem>::iterator iter = m_vecDataItems.begin();
			for (; iter != m_vecDataItems.end(); iter++)
			{
				// Retrieve this Settings object
				DataDisplaySettings* pDDS = iter->m_pDisplaySettings;
				ASSERT_RESOURCE_ALLOCATION("ELI11340", pDDS != NULL);

				// Retrieve Attribute it will be needed to update the 
				// IDShieldData even if not redacted
				IAttributePtr ipAttr = iter->m_ipAttribute;
				ASSERT_RESOURCE_ALLOCATION( "ELI11342", ipAttr != NULL );

				// Process item only if Redact is True
				if (pDDS->getRedactChoice() == kRedactYes)
				{
					// Get the Text
					ISpatialStringPtr ipValue = ipAttr->Value;
					ASSERT_RESOURCE_ALLOCATION( "ELI11343", ipValue != NULL );
					// Only cover area if value is spatial
					if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
					{
						// Get the exemption codes for this zone as a string
						ExemptionCodeList& exemptions = iter->getExemptionCodes();
						string strExemptions = exemptions.getAsString();

						// Get the text associated with this zone
						string strText = CRedactionCustomComponentsUtils::ExpandRedactionTags(
							m_UISettings.getRedactionText(), strExemptions, asString(ipAttr->Type));

						// Get the Raster zones to redact
						IIUnknownVectorPtr ipRasterZones = ipValue->GetOriginalImageRasterZones();
						ASSERT_RESOURCE_ALLOCATION( "ELI11344", ipRasterZones != NULL );

						// Iterate through the raster zones for this redaction 
						long lRasterZones = ipRasterZones->Size();
						for (long i=0; i < lRasterZones; i++)
						{
							// Bring raster zone
							IRasterZonePtr ipRasterZone = ipRasterZones->At(i);
							ASSERT_RESOURCE_ALLOCATION("ELI24857", ipRasterZone != NULL);

							// Construct the page raster zone
							PageRasterZone zone;
							zone.m_crBorderColor = m_UISettings.getBorderColor();
							zone.m_crFillColor = m_UISettings.getFillColor();
							zone.m_crTextColor = crTextColor;
							zone.m_font = m_UISettings.getFont();
							zone.m_iPointSize = m_UISettings.getFontSize();
							zone.m_strText = strText;
							ipRasterZone->GetData(&(zone.m_nStartX), &(zone.m_nStartY),
								&(zone.m_nEndX), &(zone.m_nEndY), &(zone.m_nHeight),
								&(zone.m_nPage));

							// Add to the vector of zones to redact
							vecZones.push_back(zone);
						}

						// Add to Redacted counts
						m_idsCurrData.countRedacted(ipAttr);
						continue;
					}
				}

				m_idsCurrData.countNotRedacted(ipAttr);
			}
			_lastCodePos = "40";

			// Redact the Zones
			bool bRedacted = vecZones.size() > 0;
			if (bRedacted || m_UISettings.getAlwaysOutputImage())
			{
				_lastCodePos = "40_A";

				// Save redactions (even if their are no redactions we need to call this
				// method if we are always outputting an image, this will take care
				// of issues related to annotations not being removed [FlexIDSCore #3584 & #3585]
				fillImageArea(document.m_strInput, document.m_strOutput, vecZones, 
					m_UISettings.getCarryForwardAnnotations(),
					m_UISettings.getApplyRedactionsAsAnnotations());
			}
			_lastCodePos = "50";

			// return whether redactions were performed
			return bRedacted;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25226");
	}
	catch(UCLIDException& ue)
	{
		// Add debug data for the redaction task
		ue.addDebugInfo("Input Document", document.m_strInput);
		ue.addDebugInfo("Output Document", document.m_strOutput);
		ue.addDebugInfo("Original Document", document.m_strOriginal);
		ue.addDebugInfo("VOA File", document.m_strVoa);
		ue.addDebugInfo("DocType", document.m_strDocType);
		ue.addDebugInfo("FAM File ID", document.m_lFileId);

		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::doResize(int cx, int cy)
{
	if (m_bInitialized)
	{
		// If minimized, do nothing
		if (IsIconic())
		{
			return;
		}

		////////////////////////////
		// Prepare controls for move
		////////////////////////////
		CRect	rectDlg;
		CRect	rectLabel;
		CRect	rectEditType;
		CRect	rectEditName;
		CRect	rectDataList;
		CRect	rectSummaryList;

		// Get total dialog size
		GetWindowRect( &rectDlg );
		ScreenToClient( &rectDlg );

		// Get original position of controls
		GetDlgItem( IDC_EDIT_DOCTYPE )->GetWindowRect( &rectEditType );
		GetDlgItem( IDC_EDIT_DOC_NAME )->GetWindowRect( &rectEditName );
		GetDlgItem( IDC_DATA_GRID )->GetWindowRect( &rectDataList );
		GetDlgItem( IDC_STATIC_SUMMARY )->GetWindowRect( &rectLabel );
		GetDlgItem( IDC_SUMMARY_GRID )->GetWindowRect( &rectSummaryList );

		// Convert to client coordinates to facilitate the move
		ScreenToClient( &rectEditType );
		ScreenToClient( &rectEditName );
		ScreenToClient( &rectDataList );
		ScreenToClient( &rectLabel );
		ScreenToClient( &rectSummaryList );

		// Get control spacing
		int iDiffXType = rectEditType.left - rectDlg.left;
		int iDiffXName = rectEditName.left - rectDlg.left;
		int iDiffY = rectSummaryList.Height() + (int)(0.5*iDiffXType);
		int iDiffZ = rectSummaryList.top - rectLabel.bottom;

		///////////////
		// Do the moves
		///////////////
		// Edit boxes
		// Doc Type
		GetDlgItem( IDC_EDIT_DOCTYPE )->MoveWindow( 
			rectEditType.left, rectEditType.top, 
			cx - iDiffXType, rectEditType.Height(), TRUE );

		// Doc Name
		GetDlgItem( IDC_EDIT_DOC_NAME )->MoveWindow( 
			rectEditName.left, rectEditName.top, 
			cx - iDiffXName, rectEditName.Height(), TRUE );

		// Summary Label
		GetDlgItem( IDC_STATIC_SUMMARY )->MoveWindow( 
			rectLabel.left, cy - iDiffY - rectLabel.Height() - iDiffZ, 
			cx - iDiffXType, rectLabel.Height(), TRUE );

		// Summary Grid
		GetDlgItem( IDC_SUMMARY_GRID )->MoveWindow( 
			rectSummaryList.left, cy - iDiffY, 
			cx - iDiffXType, rectSummaryList.Height(), TRUE );

		// Data Grid
		GetDlgItem( IDC_DATA_GRID )->MoveWindow( 
			rectDataList.left, rectDataList.top, cx - iDiffXType, 
			cy - iDiffY - rectLabel.Height() - iDiffZ - (int)(0.5*iDiffXType) - rectDataList.top, 
			TRUE );

		// Update column widths in grids
		m_GridData.DoResize();
		m_GridPages.DoResize();

		///////////////////////
		// Position the toolbar
		///////////////////////
		RepositionBars( AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);
	}
}
//-------------------------------------------------------------------------------------------------
CDataAreaDlg::DocumentData CDataAreaDlg::getCurrentDocument()
{
	return isInHistoryQueue() ? m_vecDocumentHistory[m_nPositionInQueue] : m_document;
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getDocumentType(IIUnknownVectorPtr ipVOA)
{
	string strType;

	// Apply Query to get DocType Attribute
	string strQuery = "DocumentType";
	IIUnknownVectorPtr	ipResults = m_ipAFUtility->QueryAttributes( 
		ipVOA, _bstr_t( strQuery.c_str() ), VARIANT_FALSE );
	ASSERT_RESOURCE_ALLOCATION( "ELI11758", ipResults != NULL );

	// Check the result
	if (ipResults->Size() > 0)
	{
		// Retrieve this Attribute
		IAttributePtr	ipAttr = ipResults->At( 0 );
		ASSERT_RESOURCE_ALLOCATION( "ELI11759", ipAttr != NULL );

		// Get the text
		ISpatialStringPtr	ipSpatial = ipAttr->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI11760", ipSpatial != NULL );
		strType = ipSpatial->String;
	}

	return strType;
}
//-------------------------------------------------------------------------------------------------
CExemptionCodesDlg* CDataAreaDlg::getExemptionCodesDlg()
{
	// Create the dialog if not already created
	CExemptionCodesDlg* pDialog = m_apExemptionsDlg.get();
	if (pDialog == NULL)
	{
		MasterExemptionCodeList codeList(getExemptionCodeDirectory());
		m_apExemptionsDlg = auto_ptr<CExemptionCodesDlg>(new CExemptionCodesDlg(codeList, this));
		pDialog = m_apExemptionsDlg.get();
	}

	// Get the currently selected data item
	DataItem& dataItem = m_vecDataItems[m_vecVerifyAttributes[m_iDataItemIndex - 1]];

	// Set the initial exemption codes
	pDialog->setExemptionCodes( dataItem.getExemptionCodes() );

	// Set the last applied exemptions if available
	pDialog->enableApplyLastExemption(m_bHasLastExemptions);
	if (m_bHasLastExemptions)
	{
		pDialog->setLastAppliedExemption(m_lastExemptions);
	}

	return pDialog;
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getFeedbackImageFileName(const DocumentData& document)
{
	// get the feedback data folder
	string strFeedbackDir = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(
		m_ipFAMTagManager, m_UISettings.getFeedbackDataFolder(), document.m_strOriginal);

	// create the directory if it doesn't exist
	createDirectory(strFeedbackDir);

	// construct the feedback image path
	string strFeedbackImage = removeLastSlashFromPath(strFeedbackDir) + "\\" +
		(m_UISettings.getFeedbackOriginalFilenames() ? getFileNameFromFullPath(document.m_strOriginal) : 
		asString(m_lFileID) + getExtensionFromFullPath(document.m_strOriginal) );	
	simplifyPathName(strFeedbackImage);

	return strFeedbackImage;
}
//-------------------------------------------------------------------------------------------------
int CDataAreaDlg::getNextUnviewedItem(int iRow)
{
	int iNextRow = 0;

	// Check for no selected item
	if (iRow == -1)
	{
		// Just return 0 and let downstream code figure out which page
		// should be the navigation target
		return 0;
	}

	// Loop through subsequent verifiable items
	int iCount = m_vecVerifyAttributes.size();
	int i;
	for (i = iRow; i < iCount; i++)
	{
		// Retrieve this Settings object
		DataDisplaySettings* pDDS = m_vecDataItems[m_vecVerifyAttributes[i]].m_pDisplaySettings;
		ASSERT_RESOURCE_ALLOCATION( "ELI11867", pDDS != NULL );

		// Check reviewed state of this item
		if (!pDDS->getReviewed())
		{
			// Row number is 1-relative
			iNextRow = i + 1;
			break;
		}
	}

	// If unviewed item not found, check previous items
	if (iNextRow == 0)
	{
		for (i = 0; i < iRow; i++)
		{
			// Retrieve this Settings object
			DataDisplaySettings* pDDS = m_vecDataItems[m_vecVerifyAttributes[i]].m_pDisplaySettings;
			ASSERT_RESOURCE_ALLOCATION( "ELI11871", pDDS != NULL );

			// Check reviewed state of this item
			if (!pDDS->getReviewed())
			{
				// Row number is 1-relative
				iNextRow = i + 1;
				break;
			}
		}
	}

	return iNextRow;
}
//-------------------------------------------------------------------------------------------------
int CDataAreaDlg::getGridItemPageNumber(int nRow)
{
	// Check row number
	if (nRow > 0 && nRow <= (int)m_GridData.GetRowCount())
	{
		// Get Page Number text for grid item
		CGXStyle	style;
		m_GridData.GetStyleRowCol( nRow, giDATAGRID_PAGE_COLUMN, style, gxCopy, 0 );
		CString	zValue = style.GetValue();
		if (!zValue.IsEmpty())
		{
			// Get actual page number
			return asLong( LPCTSTR(zValue) );
		}
	}

	// No page number available in this row
	return 0;
}
//-------------------------------------------------------------------------------------------------
int CDataAreaDlg::getKeyState()
{
	int iReturn = 0;
	bool bFound = false;

	// Check for Control key
	SHORT	shCode = GetAsyncKeyState( VK_CONTROL );
	if (shCode & 0x8000)
	{
		// Check for Shift key in addition to Control key
		shCode = GetAsyncKeyState( VK_SHIFT );
		if (shCode & 0x8000)
		{
			iReturn = VK_CONTROL + VK_SHIFT;
			bFound = true;
		}
		// Found Control key by itself
		else
		{
			iReturn = VK_CONTROL;
			bFound = true;
		}
	}

	// Check for Shift key
	if (!bFound)
	{
		shCode = GetAsyncKeyState( VK_SHIFT );
		if (shCode & 0x8000)
		{
			iReturn = VK_SHIFT;
			bFound = true;
		}
	}

	// Check for Alt key
	if (!bFound)
	{
		shCode = GetAsyncKeyState( VK_MENU );
		if (shCode & 0x8000)
		{
			iReturn = VK_MENU;
		}
	}

	return iReturn;
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getSourceDocName(IIUnknownVectorPtr ipVOA)
{
	string strName = "";
	long lCount = ipVOA->Size();

	// Check each Spatial String inside ipVOA
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this Attribute
		IAttributePtr	ipAttr = ipVOA->At( i );
		ASSERT_RESOURCE_ALLOCATION( "ELI11267", ipAttr != NULL );

		// Get the Spatial String
		ISpatialStringPtr	ipSpatial = ipAttr->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI11268", ipSpatial != NULL );

		// Save the image name if this is a Spatial string
		if (ipSpatial->HasSpatialInfo() == VARIANT_TRUE)
		{
			// Retrieve and store the source doc name
			string strDoc = asString(ipSpatial->GetSourceDocName());
			if (strName.length() == 0)
			{
				strName = strDoc;
			}
			// Compare with existing name
			else if (strName.compare( strDoc ) == 0)
			{
				// Names match, just return now
				break;
			}
			// No match, overwrite existing name with this one
			else
			{
				strName = strDoc;
			}
		}
	}

	return strName;
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getVoaFileName(const string& strSourceDocName)
{
	return CRedactionCustomComponentsUtils::ExpandTagsAndTFE(
		m_ipFAMTagManager, m_UISettings.getInputDataFile(), strSourceDocName);
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getExemptionCodeDirectory()
{
	return getModuleDirectory(_Module.m_hInst) + "\\" + gstrEXEMPTION_DIRECTORY;
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getINIPath()
{
	// Check full path to INI file
	if (m_strINIPath.length() == 0)
	{
		string strDir = getModuleDirectory(_Module.m_hInst);
		strDir += "\\";

		// Make fully-qualified path
		m_strINIPath = strDir.c_str();
		m_strINIPath += gstrINI_FILE;
	}

	return m_strINIPath;
}
//-------------------------------------------------------------------------------------------------
int CDataAreaDlg::getNextPageToView()
{
	int iNextPage = 0;
	int i;

	// Confirm that the last data item is selected
	long lCount = m_vecVerifyAttributes.size();
	if ((lCount > 0) && (m_iDataItemIndex < lCount))
	{
		// Throw exception
		UCLIDException ue("ELI11866", 
			"Cannot view next page before advancing to the last data item.");
		throw ue;
	}

	// Check for additional pages 
	if (m_iActivePage < m_iTotalPages)
	{
		// Force review of next page even without Attribute
		// if desired AND page has not yet been reviewed
		if (m_UISettings.getReviewAllPages())
		{
			// Check subsequent pages
			bool bFoundPageToView = false;
			for (i = m_iActivePage + 1; i <= m_iTotalPages; i++)
			{
				// Check status of this page
				if (!isPageViewed( i ))
				{
					bFoundPageToView = true;
					break;
				}
			}

			// Do we need to keep looking for an unviewed page
			if (!bFoundPageToView)
			{
				// Check previous pages (from the top)
				for (i = 1; i < m_iActivePage - 1; i++)
				{
					// Check status of this page
					if (!isPageViewed( i ))
					{
						bFoundPageToView = true;
						break;
					}
				}
			}

			// Check search results
			if (bFoundPageToView)
			{
				iNextPage = i;
			}
		}
		// Check for unviewed items
		else
		{
			// Find first unviewed item
			int iNewRow = getNextUnviewedItem( 0 );
			if (iNewRow > 0)
			{
				iNextPage = getGridItemPageNumber( iNewRow );
			}
		}
	}
	// Currently on last page, check for any unviewed items or pages
	else
	{
		// Should all pages be viewed?
		if (m_UISettings.getReviewAllPages())
		{
			bool bFoundPageToView = false;
			for (i = 1; i < m_iTotalPages; i++)
			{
				// Check status of this page
				if (!isPageViewed( i ))
				{
					iNextPage = i;
					break;
				}
			}
		}
		// Do not need to view all pages, just all items
		else
		{
			// Find first unviewed item
			int iNewRow = getNextUnviewedItem( 0 );
			if (iNewRow > 0)
			{
				iNextPage = getGridItemPageNumber( iNewRow );
			}
		}
	}

	return iNextPage;
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getOutputFileName(string strInputFile)
{
	string strOut = m_UISettings.getOutputImageName();

	// Call ExpandTagsAndTFE() to expand tags and functions
	strOut = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(
		m_ipFAMTagManager, strOut, strInputFile);

	string strDir = getDirectoryFromFullPath(strOut, false);

	// Create the redacted directory if necessary
	createDirectory(strDir);

	return strOut;
}
//-------------------------------------------------------------------------------------------------
string	CDataAreaDlg::getSetting(const string& strSection, const string& strKey, 
									 bool bRequired)
{
	// Create temporary PersistenceMgr
	INIFilePersistenceMgr	mgrSettings( getINIPath() );

	// Create folder name from strSection
	string strFolder = getINIPath();
	strFolder += "\\";
	strFolder += strSection;

	// Retrieve the value
	string strValue = mgrSettings.getKeyValue( strFolder, strKey );

	// Check result
	if (strValue.size() == 0 && bRequired)
	{
		UCLIDException ue( "ELI11220", "Required setting not defined." );
		ue.addDebugInfo( "Required Setting", strKey );
		ue.addDebugInfo( "Section Name", strSection );
		throw ue;
	}

	return strValue;
}
//-------------------------------------------------------------------------------------------------
IRasterZonePtr CDataAreaDlg::getZoneFromID(long lID)
{
	// Get Zone parameters
	long lStartX, lStartY, lEndX, lEndY, lHeight, lPage;
	m_ipOCX->getZoneEntityParameters( lID, &lStartX, &lStartY, &lEndX, &lEndY, &lHeight );
	string strPage = asString( m_ipOCX->getEntityAttributeValue( lID, "Page" ) );
	lPage = asLong( strPage );

	// Create RasterZone object
	IRasterZonePtr	ipZone( CLSID_RasterZone );
	ASSERT_RESOURCE_ALLOCATION( "ELI11376", ipZone != NULL );

	// Apply parameters
	ipZone->StartX = lStartX;
	ipZone->StartY = lStartY;
	ipZone->EndX = lEndX;
	ipZone->EndY = lEndY;
	ipZone->Height = lHeight;
	ipZone->PageNumber = lPage;

	return ipZone;
}
//-------------------------------------------------------------------------------------------------
long CDataAreaDlg::getRedactionsCount()
{
	// Step through Data items
	long lCount = 0;
	long lSize = m_vecDataItems.size();
	int i;
	for (i = 0; i < lSize; i++)
	{
		// Retrieve this Settings object
		DataDisplaySettings* pDDS = m_vecDataItems[i].m_pDisplaySettings;
		ASSERT_RESOURCE_ALLOCATION("ELI11830", pDDS != NULL);

		// Increment counter if Redact is True
		if (pDDS->getRedactChoice() == kRedactYes)
		{
			lCount++;
		}
	}

	return lCount;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::getRegistrySettings()
{
	// Create a persistence manager
	RegistryPersistenceMgr rpm( HKEY_CURRENT_USER, gstrREDACTIONCC_PATH );

	// Check for registry key
	if (!rpm.keyExists( gstrIDSHIELD_FOLDER, gstrQUEUE_SIZE ))
	{
		// Create key if not found
		rpm.createKey( gstrIDSHIELD_FOLDER, gstrQUEUE_SIZE, asString( gnDEFAULT_QUEUE_SIZE ) );
	}

	// Retrieve max queue size
	string strMaxQueue = rpm.getKeyValue( gstrIDSHIELD_FOLDER, gstrQUEUE_SIZE );
	if (strMaxQueue.length() > 0)
	{
		// Use the registry value for max queue size
		m_nNumPreviousDocsToQueue = asLong( strMaxQueue );
	}
	else
	{
		// Use the default value and update the registry setting
		m_nNumPreviousDocsToQueue = gnDEFAULT_QUEUE_SIZE;
		rpm.setKeyValue( gstrIDSHIELD_FOLDER, gstrQUEUE_SIZE, asString( gnDEFAULT_QUEUE_SIZE ) );
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::handleCharacter(WPARAM wParam)
{
	// Ignore all characters if in between files
	if (m_iActivePage == 0)
	{
		return;
	}

	// if user pressed tab, down, or up then ensure that any cell that was currently
	// being edited has its value committed and the highlight state is reset
	// (this is part of the fix for [p16 #2833])
	if (wParam == VK_TAB || wParam == VK_DOWN 
		|| wParam == VK_UP || wParam == guiSHOW_DROP_DOWN_LIST)
	{
		// get the current active row and column
		ROWCOL nRow(0), nCol(0);
		m_GridData.GetCurrentCell(nRow, nCol);

		// if the current active column is the type column and the cell is active, transfer
		// the current cell so that its value is stored and the cell is deactivated
		// then reset the highlight state for the cell
		if (nCol == giDATAGRID_TYPE_COLUMN && asCppBool(m_GridData.IsActiveCurrentCell()))
		{
			// end editing on the current cell
			m_GridData.TransferCurrentCell();

			// return to default highlight state
			m_GridData.UpdateCellToNotHighlight(-1, -1);
		}
	}

	// Handle desired characters
	switch (wParam)
	{
	case guiSHOW_DROP_DOWN_LIST:
		{
			if (getKeyState() == guiSHOW_DROP_DOWN_LIST_VK)
			{
				// check if the selected row is on the same page as the currently
				// displayed page [p16 #2864]
				if (m_iActivePage == getGridItemPageNumber(m_iDataItemIndex))
				{
					// user pressed drop down list hot key, show drop down list
					// for this row [p16 #2836]
					showDropDownList(m_iDataItemIndex);
				}
			}
		}
		break;

	// guiPREVIOUS_ITEM == guiNEXT_ITEM == 'TAB'
	// guiPREVIOUS_DOC == guiNEXT_DOC == 'TAB'
	case guiPREVIOUS_ITEM:
		{
			// Check for special key
			if (getKeyState() == guiPREVIOUS_ITEM_VK)
			{
				// Shift-Tab for Previous item
				OnButtonPreviousItem();
			}
			else if (getKeyState() == guiNEXT_ITEM_VK)
			{
				// Tab for Next item
				OnButtonNextItem();
			}
			else if (getKeyState() == guiNEXT_DOC_VK)
			{
				// Ctrl+Tab for Next document
				OnButtonNextDoc();
			}
			else if (getKeyState() == guiPREVIOUS_DOC_VK)
			{
				// Ctrl+Shift+Tab for Previous document
				OnButtonPreviousDoc();
			}
		}
		break;

	case guiSAVE:
		{
			// Check for special key
			if (GetKeyState(guiSAVE_VK) < 0)
			{
#ifdef _VERIFICATION_LOGGING
				// Add entry to default log file
				ThreadSafeLogFile tslf;
				tslf.writeLine( "Calling Save() from HandleCharacter()" );
#endif

				OnButtonSave();
			}
		}
		break;

	case guiTOGGLE_REDACTION:
		{
			// Check for special key
			if (getKeyState() == guiTOGGLE_REDACTION_VK)
			{
				OnButtonToggleRedact();
			}
		}
		break;

	// Same as guiALL_EXEMPTION
	case guiAPPLY_EXEMPTION:
		{
			// Check for modifier
			if (getKeyState() == guiAPPLY_EXEMPTION_VK)
			{
				selectExemptionCodes();
			}
			else if (getKeyState() == guiALL_EXEMPTION_VK)
			{
				selectExemptionCodes(true);
			}
		}
		break;

	case guiLAST_EXEMPTION:
		{
			// Check for modifier
			if (getKeyState() == guiLAST_EXEMPTION_VK)
			{
				applyLastExemptionCodes();
			}
		}
		break;

	case guiAUTO_ZOOM:
		{
			// Check for special key
			if (getKeyState() == guiAUTO_ZOOM_VK)
			{
				OnButtonZoom();
			}
		}
		break;

	case guiSRW_ACTIVATE_PAN:
		{
			// Set the current tool to Pan tool
			m_ipSRIR->SetCurrentTool(kBtnPan);
		}
		break;

	case VK_UP:
		{
			// Also Up Arrow for Previous item
			OnButtonPreviousItem();
		}
		break;

	case VK_DOWN:
		{
			// Also Down Arrow for Next item
			OnButtonNextItem();
		}
		break;

	case guiSRW_SELECT_ENTITIES:
		{
			m_ipSRIR->SetCurrentTool(kBtnSelectHighlight);
		}
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::handleFeedback(const DocumentData& document, bool bRedacted)
{
	// check if feedback should be collected for the current document
	if(shouldCollectFeedback(bRedacted))
	{
		// get the full destination path of the image in the feedback folder
		const string& strFeedbackImage = getFeedbackImageFileName(document);

		// check if the original image file should be collected
		if( m_UISettings.getCollectFeedbackImage() )
		{
			// copy the file if the source and destination differ
			if (document.m_strOriginal != strFeedbackImage)
			{
				copyFile(document.m_strOriginal, strFeedbackImage);
			}

			// create a VOA file relative to the image collected for feedback
			writeVOAFile(strFeedbackImage, strFeedbackImage + ".voa");
		}
		else
		{
			// create a VOA file relative to the original image
			writeVOAFile(document.m_strOriginal, strFeedbackImage + ".voa");
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::initPages(int iPageCount)
{
	// Validate page count
	if (iPageCount < 0)
	{
		UCLIDException ue( "ELI11206", "Invalid page count!" );
		ue.addDebugInfo( "Number of pages", iPageCount );
		throw ue;
	}

	// Clear existing
	m_GridPages.Clear();

	// Store count
	m_iTotalPages = iPageCount;

	// Determine number of full pages and remainder
	int iFullRows = iPageCount / giPAGES_PER_ROW;
	int iRemainder = iPageCount % giPAGES_PER_ROW;

	// Build each full row
	int i, j;
	vector<string> vecStrings;
	for (i = 0; i < iFullRows; i++)
	{
		// Build the vector of pages for this row
		vecStrings.clear();
		for (j = 1; j <= giPAGES_PER_ROW; j++)
		{
			int iPage = giPAGES_PER_ROW * i + j;
			vecStrings.push_back( asString( iPage ) );
		}

		// Set the row of pages with bold font and white background
		m_GridPages.SetRowInfo( i + 1, vecStrings, 0 );
		m_GridPages.SetRowBoldBackground( i + 1, true, false );
	}

	// Build the remainder row
	if (iRemainder > 0)
	{
		vecStrings.clear();
		for (j = 1; j <= iRemainder; j++)
		{
			int iPage = giPAGES_PER_ROW * iFullRows + j;
			vecStrings.push_back( asString( iPage ) );
		}

		// Set the row of pages with bold font and white background
		m_GridPages.SetRowInfo( iFullRows + 1, vecStrings, 0 );
		m_GridPages.SetRowBoldBackground( iFullRows + 1, true, false );
	}
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::isManualRow(int iRow)
{
	// Check row number
	if ((iRow < 1) || (iRow > (int)m_vecDataItems.size()))
	{
		return false;
	}

	// Get a temporary Data Confidence Level object - for manual settings
	DataConfidenceLevel	dcl( "" );
	dcl.applyManualSettings( 0 );

	// Retrieve desired Setting object
	DataDisplaySettings* pDDS = m_vecDataItems[m_vecVerifyAttributes[iRow-1]].m_pDisplaySettings;
	ASSERT_RESOURCE_ALLOCATION( "ELI11868", pDDS != NULL );

	// Check for a Manual redaction
	if (pDDS->getCategory() == dcl.getShortName())
	{
		return true;
	}
	else
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::isPageViewed(int iPage)
{
	bool bReturn = false;

	// Check the vector
	long lSize = m_vecPagesReviewed.size();
	int i;
	for (i = 0; i < lSize; i++)
	{
		if (m_vecPagesReviewed[i] == iPage)
		{
			bReturn = true;
			break;
		}
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::loadImage(const DocumentData& document)
{
	// Validate file presence
	validateFileOrFolderExistence(document.m_strInput);

	// Load the image in Spot Recognition Window
	updateTitleBar(document.m_strInput);
	m_ipSRIR->OpenImageFile(document.m_strInput.c_str());

	// Store this file's FAM ID(P16 2894)
	m_lFileID = document.m_lFileId;

	// Show document type
	SetDlgItemText(IDC_EDIT_DOCTYPE, document.m_strDocType.c_str());

	// Update the document name edit box (P16 #2175)
	SetDlgItemText(IDC_EDIT_DOC_NAME, getFileNameFromFullPath(document.m_strInput).c_str());

	// Initialize the Pages grid 
	long lPageCount =  m_ipSRIR->GetTotalPages();
	initPages(lPageCount);
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::openVoa(const string& strVoaFile)
{
	// Get the path of the VOA File Viewer app
	string strExePath = getModuleDirectory(_Module.m_hInst) + "\\" + gstrVOA_VIEWER_FILENAME;
	simplifyPathName(strExePath);

	// Ensure path exists
	if (!isFileOrFolderValid(strExePath))
	{
		string strMsg = "Could not find " + gstrVOA_VIEWER_FILENAME;
		UCLIDException ue("ELI25192", strMsg);
		ue.addDebugInfo("Path", strExePath);
		throw ue;
	}

	// Start the Voa File Viewer
	runEXE(strExePath, '"' + strVoaFile + '"');
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::navigateToHistoryItem(int nIndex)
{
	// Validate the index
	int nCount = m_vecDocumentHistory.size();
	if ((nIndex < 0) || (nIndex >= nCount))
	{
		// Create and throw exception
		UCLIDException ue("ELI14765", "Unable to navigate to invalid history record!" );
		ue.addDebugInfo( "Requested Index", nIndex );
		ue.addDebugInfo( "Records Count", nCount );
		throw ue;
	}

	// Load the appropriate image
	loadImage(m_vecDocumentHistory[nIndex]);

	// reset the redaction type drop down list values to default set
	resetAttributeTypes();

	// Replace collection of Data Items with appropriate set
	m_vecDataItems.clear();
	m_vecDataItems = m_vecDataItemHistory[nIndex];

	// Set the current stop watch to the one saved in the history list
	m_swCurrTimeViewed = m_vecDurationsHistory[nIndex];

	// Set the current IDShield data(P16 2901)
	m_idsCurrData = m_vecIDShieldDataHistory[nIndex];

	// Clear and update collection of Data Item indices
	m_vecVerifyAttributes.clear();
	int nSize = m_vecDataItems.size();
	int i;
	for (i = 0; i < nSize; i++)
	{
		// Data is sorted before being added to the History
		m_vecVerifyAttributes.push_back( i );

		// get the attribute from the data item 
		IAttributePtr ipAttribute = m_vecDataItems[i].m_ipAttribute;
		ASSERT_RESOURCE_ALLOCATION("ELI20315", ipAttribute != NULL);

		// add the type to the drop down list set
		m_setAttributeTypes.insert(asString(ipAttribute->Type));
	}

	// make sure the last seen redaction type is valid for this document
	// added as per [p16 #2858]
	if (m_setAttributeTypes.find(m_strLastSelectedRedactionType) == m_setAttributeTypes.end())
	{
		m_strLastSelectedRedactionType = "";
	}

	// Refresh the Data Grid
	refreshDataGrid();

	// Initialize the Pages grid 
	long lPageCount =  m_ipSRIR->GetTotalPages();
	initPages( lPageCount );

	// Set the active page to the first page if it is zero
	if (m_iActivePage == 0 && lPageCount > 0)
	{
		m_iActivePage = 1;
	}

	// Create Zone Entity items in image for each Data Item
	for (i = 0; i < nSize; i++)
	{
		// Retrieve this Data Item
		DataItem di = m_vecDataItems[i];

		// Get the DCL for this item 
		DataConfidenceLevel* pDCL = m_mapDCLevels[di.m_pDisplaySettings->getCategory()];
		ASSERT_RESOURCE_ALLOCATION("ELI20427", pDCL != NULL);

		// get the display color
		long lColor = pDCL->getDisplayColor();

		// Build new collection of zone IDs
		vector<long> vecZoneIDs;

		// Iterate through the raster zones of the attribute
		ISpatialStringPtr ipValue = di.m_ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI23557", ipValue != NULL);
		IIUnknownVectorPtr ipRasterZones = ipValue->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI23558", ipRasterZones != NULL);
		long size = ipRasterZones->Size();
		for (long j = 0; j < size; j++)
		{
			// Get the jth raster zone
			IRasterZonePtr ipZone = ipRasterZones->At(j);
			ASSERT_RESOURCE_ALLOCATION("ELI23559", ipZone != NULL);
			
			// Get the data from the raster zone
			long lStartX, lStartY, lEndX, lEndY, lHeight, lPageNumber;
			ipZone->GetData(&lStartX, &lStartY, &lEndX, &lEndY, &lHeight, &lPageNumber);

			// Create Zone Entity using the OCX and Color from INI
			long lID = m_ipOCX->addZoneEntity(lStartX, lStartY, lEndX, lEndY, lHeight, lPageNumber,
				TRUE, TRUE, lColor );

			// Handle success case if zone entity was added
			if (lID > 0)
			{
				// Add this ID to collection, 
				vecZoneIDs.push_back( lID );
			}
		}

		// Check if any zones were added for this data item
		if (!vecZoneIDs.empty())
		{
			// Add the collection to the Display Settings member of the Data Item,
			di.m_pDisplaySettings->setHighlightIDs( vecZoneIDs );
			
			// Update the drawn highlight
			updateDataItemHighlights( di.m_pDisplaySettings );
		}

	}	// end for each Data Item

	// Select and highlight first item
	m_iDataItemIndex = 0;
	long lCount = m_vecVerifyAttributes.size();
	if (m_UISettings.getReviewAllPages())
	{
		// Select first item on page 1
		setPage( 1, true );
	}
	else
	{
		// Select first item on grid
		if (lCount > 0)
		{
			selectDataRow( 1 );
		}
	}

	// Restore collection of viewed pages
	m_vecPagesReviewed = m_vecReviewedPagesHistory[nIndex];
	for (unsigned int index = 0; index < m_vecPagesReviewed.size(); index++)
	{
		// Get the page number of this previously reviewed page
		int iPage = m_vecPagesReviewed[index];

		// Set the background of this item in Summary grid since it 
		// was previously reviewed
		setPageBackground( iPage );
	}

	// Update toolbar buttons
	updateButtons();

	// Start the stop watch since file is being viewed
	m_swCurrTimeViewed.start();
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::populateDataGrid(IIUnknownVectorPtr ipAllAttributes)
{
	// Clear the current collection
	m_vecVerifyAttributes.clear();
	m_vecDataItems.clear();

	// reset the attribute types list [p16 #2385]
	resetAttributeTypes();

	// Step through each level from INI file
	for (map<string, DataConfidenceLevel*>::iterator it = m_mapDCLevels.begin(); 
		it != m_mapDCLevels.end(); it++)
	{
		// Retrieve this Settings object
		DataConfidenceLevel*	pDCL = it->second;
		ASSERT_RESOURCE_ALLOCATION( "ELI11252", pDCL != NULL );

		// Retrieve the Query for this Level
		string strQuery = pDCL->getQuery();

		// Apply Query to get subset of ipAllAttributes
		IIUnknownVectorPtr	ipResults = m_ipAFUtility->QueryAttributes( 
			ipAllAttributes, _bstr_t( strQuery.c_str() ), VARIANT_FALSE );
		ASSERT_RESOURCE_ALLOCATION( "ELI11255", ipResults != NULL );

		// Review each Attribute in ipResults
		long lCount = ipResults->Size();
		for (int j = 0; j < lCount; j++)
		{
			// Retrieve this Attribute
			IAttributePtr	ipAttribute = ipResults->At( j );
			ASSERT_RESOURCE_ALLOCATION( "ELI11257", ipAttribute != NULL );

			// get the attribute type
			string strAttributeType = asString(ipAttribute->Type);

			// add the type to our set 
			m_setAttributeTypes.insert(strAttributeType);

			// Retrieve associated SpatialString
			ISpatialStringPtr	ipSpatial = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION( "ELI11258", ipSpatial != NULL );

			// Make the highlight(s) in Spot Recognition Window
			if (pDCL->isMarkedForDisplay())
			{
				// Get collected zones for this Attribute
				IIUnknownVectorPtr	ipZones;
				
				if (ipSpatial->HasSpatialInfo() == VARIANT_TRUE)
				{
					ipZones = ipSpatial->GetOriginalImageRasterZones();
					long lZoneCount = ipZones->Size();

					if (lZoneCount > 0)
					{
						// Default initial Redaction choice to Yes
						ERedactChoice eChoice = kRedactYes;
						if (!pDCL->isMarkedForOutput())
						{
							// Output = 0, therefore N/A
							eChoice = kRedactNA;
						}

						// make new lines appear as \r\n 
						string	strText = asString( ipSpatial->String );
						convertCppStringToNormalString(strText);

						// Create a DataDisplaySettings object
						DataDisplaySettings*	pDDS = 
							new DataDisplaySettings( strText, pDCL->getShortName(), 
							strAttributeType, ipSpatial->GetFirstPageNumber(), 
							eChoice, pDCL->isNonRedactWarningEnabled(), 
							pDCL->isRedactWarningEnabled() );
						ASSERT_RESOURCE_ALLOCATION( "ELI11260", pDDS != NULL );

						// Create a highlight for each RasterZone
						vector<long>	vecZoneIDs;
						for (int k = 0; k < lZoneCount; k++)
						{
							// Get this Zone
							IRasterZonePtr	ipZone = ipZones->At( k );
							ASSERT_RESOURCE_ALLOCATION( "ELI11262", ipZone != NULL );

							// Get the data from the raster zone
							long lStartX, lStartY, lEndX, lEndY, lHeight, lPageNumber;
							ipZone->GetData(&lStartX, &lStartY, &lEndX, &lEndY,
								&lHeight, &lPageNumber);

							// Create Zone Entity using the OCX and Color from INI
							long lID = m_ipOCX->addZoneEntity(lStartX, lStartY, lEndX, lEndY,
								lHeight, lPageNumber, TRUE, TRUE, pDCL->getDisplayColor() );

							// Add this ID to collection
							if (lID > 0)
							{
								vecZoneIDs.push_back( lID );
							}
						}	// end for each Raster Zone

						// Provide collected IDs to Settings object
						pDDS->setHighlightIDs( vecZoneIDs );

						// update the highlight colors
						updateDataItemHighlights(pDDS);

						// create the data item
						DataItem item;
						item.m_ipAttribute = ipAttribute;
						item.m_pDisplaySettings = pDDS;

						m_vecDataItems.push_back( item );

						// we will only add this attribute to the verify list
						// if the attributes confidence level requires it
						if (pDCL->isMarkedForVerify())
						{
							// Add this Attribute to collection for Data Grid
							m_vecVerifyAttributes.push_back(m_vecDataItems.size()-1);
						}
					}		// end if zones exist
				}			// end if is spatial
			}				// end if this Attribute to be displayed
		}					// end for each Attribute

		// make sure the last redaction data type set is a valid setting for the new document
		// if it is not a valid setting for this document, then reset to empty string
		// added as per [p16 #2858]
		if (m_setAttributeTypes.find(m_strLastSelectedRedactionType) == m_setAttributeTypes.end())
		{
			m_strLastSelectedRedactionType = "";
		}
	}						// end for each Level

	// Sort collected Attributes before display in Data Grid
	sort(m_vecVerifyAttributes.begin(), m_vecVerifyAttributes.end(),
		CompareDataItems(&m_vecDataItems));

	refreshDataGrid();

	///////////////////////////
	// Determine starting point to Data Grid & SRIR
	///////////////////////////

	long lCount = m_vecVerifyAttributes.size();
	if (m_UISettings.getReviewAllPages())
	{
		// Select first item on page 1
		setPage(1, true);
	}
	else if (lCount > 0)
	{
		// Select first item on grid
		selectDataRow(1);
	}
	else
	{
		// Set the first page [LegacyRCAndUtils #4925]
		setPage(1, false);
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::prepareAndShowSRIR(int iWidth, int iHeight, int iXOffSet, int iYOffSet)
{
	// Create new SRIR
	IInputReceiverPtr ipSpotRecIR( CLSID_SpotRecognitionWindow );
	ASSERT_RESOURCE_ALLOCATION("ELI11208", ipSpotRecIR != NULL);

	// Move SRIR to right two-thirds of screen with minimum border
	// [p16 #2627] - x,y start position should be moved iXOffSet right and iYOffSet down
	HWND	hwndSRIR = (HWND)(long)ipSpotRecIR->GetWindowHandle();
	::SetWindowPos( hwndSRIR, NULL, (iWidth / 3) + iXOffSet, 1 + iYOffSet, 2*iWidth / 3, 
		iHeight - 1, SWP_NOZORDER );

	// Show the Spot Recognition Window 
	m_ipSRIR = ipSpotRecIR;
	IInputReceiverPtr ipIR = m_ipSRIR;
	ASSERT_RESOURCE_ALLOCATION( "ELI11401", ipIR != NULL );
	ipIR->ShowWindow( VARIANT_TRUE );

	// Enable Text input
	ipIR->EnableInput( "Text", "Select" );

	// Create an instance of the OCR engine
	IOCREnginePtr	ipOCREngine( CLSID_ScansoftOCR );
	ASSERT_RESOURCE_ALLOCATION( "ELI11407", ipOCREngine != NULL );

	// Initialize the private license
	IPrivateLicensedComponentPtr ipScansoftEngine = ipOCREngine;
	ASSERT_RESOURCE_ALLOCATION( "ELI11408", ipScansoftEngine != NULL );
	ipScansoftEngine->InitPrivateLicense( LICENSE_MGMT_PASSWORD.c_str() );

	// Provide OCR Engine to SRIR
	ipIR->SetOCREngine( ipOCREngine );

	// Set this dialog as event handler for SRIR events
	m_ipSRIR->SetSRWEventHandler( this );

	m_ipSRIR->EnableAutoOCR(VARIANT_FALSE);

	m_ipSRIR->HighlightsAdjustableEnabled = VARIANT_TRUE;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::prepareGrids() 
{
	////////////////////
	// Prepare Data grid
	////////////////////
	m_GridData.SubclassDlgItem( IDC_DATA_GRID, this );

	// Prepare Header labels
	vector<string>	vecHeader;
	vecHeader.push_back("Text");
	vecHeader.push_back("Category");
	vecHeader.push_back("Type");
	vecHeader.push_back("Page");
	vecHeader.push_back("Redact");
	vecHeader.push_back("Exemptions");

	// Set fixed column width
	long	lWidth = 50;

	// Set initial width, overwritten inside resizeGrid()
	vector<int> vecWidths;
	vecWidths.push_back(0);		// First column will get extra width
	vecWidths.push_back(lWidth + 20);  // Category column needs extra room
	vecWidths.push_back(lWidth + 20);  // Type column needs extra room
	vecWidths.push_back(lWidth); 
	vecWidths.push_back(lWidth);
	vecWidths.push_back(lWidth + 60); // Exemptions need extra room

	m_GridData.PrepareGrid( vecHeader, vecWidths );

	// Set Control ID for Notification messages
	m_GridData.SetControlID( IDC_DATA_GRID );

	// Disable response to click in row header (P16 #1599)
	m_GridData.ChangeColHeaderStyle( CGXStyle().SetEnabled(FALSE) );

	// set the type column of the grid to be a drop down list [p16 #2385]
	m_GridData.SetStyleRange(CGXRange().SetCols(giDATAGRID_TYPE_COLUMN), 
		CGXStyle().SetControl(GX_IDS_CTRL_CBS_DROPDOWNLIST));
	m_GridData.SetDropDownListColumn(giDATAGRID_TYPE_COLUMN);

	// fill the list in the drop down list control
	setGridTypeColumnList();

	///////////////////////
	// Prepare Summary grid
	///////////////////////
	m_GridPages.SubclassDlgItem( IDC_SUMMARY_GRID, this );

	// Prepare Header labels
	vecHeader.clear();
	vecHeader.push_back("");
	vecHeader.push_back("");
	vecHeader.push_back("");
	vecHeader.push_back("");
	vecHeader.push_back("");
	vecHeader.push_back("");
	vecHeader.push_back("");
	vecHeader.push_back("");

	// Column widths - all the same
	lWidth = 42;
	vecWidths.clear();
	vecWidths.push_back( lWidth );
	vecWidths.push_back( lWidth );
	vecWidths.push_back( lWidth );
	vecWidths.push_back( lWidth );
	vecWidths.push_back( lWidth );
	vecWidths.push_back( lWidth );
	vecWidths.push_back( lWidth );
	vecWidths.push_back( lWidth );

	m_GridPages.PrepareGrid( vecHeader, vecWidths );

	// Hide column headers, turn off full-row selection
	m_GridPages.HideRows( 0, 0, TRUE, NULL, GX_UPDATENOW, gxDo );
	m_GridPages.GetParam()->EnableSelection((WORD) (~GX_SELFULL & ~GX_SELCOL & ~GX_SELTABLE));

	// Entire grid has horizontally-centered items
	m_GridPages.SetStyleRange(CGXRange().SetTable(),
					CGXStyle()
						.SetHorizontalAlignment( DT_CENTER )
					);

	// Set Special background color to Lime Green
	m_GridPages.SetSpecialBackgroundColor( 0x32CD32 );

	// Set Control ID for Notification messages
	m_GridPages.SetControlID( IDC_SUMMARY_GRID );
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::processINIFile()
{
	// Retrieve number of Confidence Levels
	long lLevels = asLong( getSetting( gstrGENERAL_SECTION, gstrNUM_LEVELS, true ) );

	/////////////////////////////////////////////
	// Retrieve necessary settings for each level
	/////////////////////////////////////////////
	for (long i = 1; i <= lLevels; i++)
	{
		// Which level?
		string strLevel = gstrLEVEL_SECTION_PREFIX;
		strLevel += asString( i );

		// Create a DataConfidenceLevel object
		DataConfidenceLevel*	pDCL = new DataConfidenceLevel( getINIPath() );
		ASSERT_RESOURCE_ALLOCATION( "ELI11230", pDCL != NULL );

		// Read settings for this level
		pDCL->readSettings( strLevel );

		// insert value into the map, if it already existed in the map 
		// then throw an exception. [p16 #2950]
		if (!m_mapDCLevels.insert(
			pair<string, DataConfidenceLevel*>(pDCL->getShortName(),pDCL)).second) 
		{
			// build the exception
			UCLIDException ue("ELI20426", 
				"IDShield.ini file contains duplicate Redaction confidence level!");
			ue.addDebugInfo("Level", i);
			ue.addDebugInfo("LongName", pDCL->getLongName());
			ue.addDebugInfo("ShortName", pDCL->getShortName());
			ue.addDebugInfo("Query", pDCL->getQuery());
			ue.addDebugInfo("FilePath", getINIPath());

			// delete the duplicate DCL
			delete pDCL;

			// throw the exception
			throw ue;
		}
	}

	// check if a manual DataConfidenceLevel exists, if not throw an exception [p16 #2946]
	if (m_mapDCLevels.find("Man") == m_mapDCLevels.end())
	{
		UCLIDException ue("ELI20425", 
			"IDShield.ini file missing Manual Redaction confidence level!");
		ue.addDebugInfo("FilePath", getINIPath());
		throw ue;
	}

	// Get the color for the selection border
	string strSelectionColor = getSetting(gstrGENERAL_SECTION, gstrSELECTION_COLOR, false);
	if (!strSelectionColor.empty())
	{
		m_crSelection = getRGBFromString(strSelectionColor);
	}

	// Get whether or not to open the voa file when saving with SHIFT key held down
	string strOpenVoa = getSetting(gstrGENERAL_SECTION, gstrOPEN_VOA_ON_SHIFT_SAVE, false);
	if (strOpenVoa.length() > 0)
	{
		m_bOpenVoaOnShiftSave = asLong(strOpenVoa) == 1;
	}

	////////////////////////////////
	// Retrieve General Tab settings
	////////////////////////////////
	// Auto-Zoom - Default to True
	bool bCheck = true;
	string strValue = getSetting( gstrGENERAL_SECTION, gstrAUTO_ZOOM, false );
	if (strValue.length() > 0)
	{
		bCheck = (asLong( strValue ) == 1);
	}
	m_OptionsDlg.setAutoZoom( bCheck );

	// Auto-Zoom Scale
	int iAutoZoomScale = giZOOM_LEVEL[giDEFAULT_ZOOM_LEVEL_INDEX];
	strValue = getSetting( gstrGENERAL_SECTION, gstrAUTO_ZOOM_SCALE, false );
	if (strValue.length() > 0)
	{
		// Get the number value from the found string
		iAutoZoomScale = static_cast<int>(asLong( strValue ));
	}
	m_OptionsDlg.setAutoZoomScale( iAutoZoomScale );

	// Auto Select Tool - Default to Pan
	EAutoSelectTool eTool = kPan;
	strValue = getSetting(gstrGENERAL_SECTION, gstrAUTO_SELECT_TOOL, false);
	if (strValue.length() > 0)
	{
		eTool = (EAutoSelectTool) asLong(strValue);
	}
	m_OptionsDlg.setAutoSelectTool(eTool);

	////////////////////////////////
	// Retrieve RedactionDataTypes Tab settings
	////////////////////////////////
	// retrieve the default redaction setting - [p16 #2834]
	m_strDefaultRedactionType = getSetting(gstrREDACTION_DATA_TYPES_SECTION, 
		gstrREDACTION_DEFAULT_TYPE, false);
	
	// flag used to verify that the default redaction type matches a type from
	// the list of types or is empty
	bool bDefaultMatchesChoices = m_strDefaultRedactionType.empty();

	// get the count of data types 
	strValue = getSetting(gstrREDACTION_DATA_TYPES_SECTION, 
		gstrREDACTION_TYPES_COUNT, true);

	// retrieve each type from the settings file
	long lCount = asLong(strValue);
	for (long i=0; i < lCount; i++)
	{
		// build the key (RedactionDataType#)
		string strKey = gstrREDACTION_TYPES_PREFIX + asString(i+1);

		// get the value
		strValue = getSetting(gstrREDACTION_DATA_TYPES_SECTION, strKey, true);

		// check if the setting matches the default setting
		bDefaultMatchesChoices = bDefaultMatchesChoices || (strValue == m_strDefaultRedactionType);
		
		// store the value in the vector of types from the INI file
		m_vecAttributesFromINIFile.push_back(strValue);
	}

	if (!bDefaultMatchesChoices)
	{
		UCLIDException ue("ELI20270", 
			"DefaultRedactionType must be blank or match one of the redaction data types!");
		ue.addDebugInfo("DefaultRedactionType", m_strDefaultRedactionType);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::promptBeforeApplyAllExemptions(const ExemptionCodeList& codesToApply)
{			
	// Check if any row is non-empty and doesn't have the selected exemption code.
	int iCount = m_vecVerifyAttributes.size();
	for (int i = 0; i < iCount; i++)
	{
		// Retrieve this Settings object
		DataItem& dataItem = m_vecDataItems[m_vecVerifyAttributes[i]];

		// Check this row
		ExemptionCodeList& codes = dataItem.getExemptionCodes();
		if (!codes.isEmpty() && codes != codesToApply)
		{
			// Display a warning message to the user
			string strWarning = "This will overwrite exemption codes and reasons\n";
			strWarning += "for all redactions in the document.\n\n";
			strWarning += "Are you sure you want to continue?";

			int iResult = MessageBox(strWarning.c_str(), "Overwrite exemption codes?", 
				MB_YESNO | MB_ICONERROR);
			
			// Return the user's response
			return iResult == IDYES;
		}
	}

	// If we reached this point, no exemptions codes are being overwritten
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::promptBeforeNewDocument(const string& strAction)
{
	return checkAndPromptForUnwantedExemptionCodes(strAction) || 
		checkAndPromptForEmptyExemptionCodes(strAction) || 
		checkAndPromptForEmptyRedactionTypes(strAction);
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::refreshDataGrid()
{
	// Clear the grid
	m_GridData.Clear();

	// Add row to grid for each Attribute
	long lCount = m_vecVerifyAttributes.size();
	int i;
	for (i = 0; i < lCount; i++)
	{
		// Retrieve this Settings object
		DataDisplaySettings* pDDS = m_vecDataItems[m_vecVerifyAttributes[i]].m_pDisplaySettings;
		ASSERT_RESOURCE_ALLOCATION( "ELI11266", pDDS != NULL );

		// Get settings and refresh the row
		bool bIsManual = isManualRow( i + 1 );
		bool bBold = !pDDS->getReviewed();
		refreshDataRow( i + 1, pDDS, bBold, bIsManual );
	}

	// set the type column data list
	setGridTypeColumnList();
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::refreshDataRow(int nRow, DataDisplaySettings* pDDS, bool bBold, bool bManual)
{
	ASSERT_ARGUMENT("ELI18563", pDDS != NULL);

	///////////////////
	// Build collection of strings for this row
	///////////////////
	vector<string>	vecStrings;

	// Attribute text
	vecStrings.push_back( pDDS->getText() );

	// Category text, Type text,  and Page number
	vecStrings.push_back( pDDS->getCategory() );
	vecStrings.push_back( pDDS->getType() );
	vecStrings.push_back( asString( pDDS->getPageNumber() ) );
	vecStrings.push_back("");
	vecStrings.push_back( pDDS->getExemptionCodes() );

	// Add the row to the grid
	m_GridData.SetRowInfo( nRow, vecStrings, 0 );

	// Add the appropriate icon to the Redact column
	setDataStatus( nRow, pDDS->getRedactChoice() );

	// Set text to Bold or Normal
	m_GridData.SetRowBoldBackground( nRow, bManual ? false : bBold, false );
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::reset()
{
	// Clear grids and flag
	m_GridData.Clear();
	m_GridPages.Clear();
	m_bVerified = false;

	// Clear pages reviewed vector
	m_vecPagesReviewed.clear();

	// Clear active index and settings objects
	m_iActivePage = 0;
	m_pActiveDDS = NULL;
	m_iDataItemIndex = 0;

	// Clear SRIR
	m_ipSRIR->Clear();

	// Clear document type
	SetDlgItemText(IDC_EDIT_DOCTYPE, "");

	// Clear the document name
	setCurrentFileName("");
	SetDlgItemText(IDC_EDIT_DOC_NAME, "");

	// Clear the title bar and the dirty flag
	updateTitleBar( "" );
	m_bChangesMadeForHistory = false;
	m_bChangesMadeToMostRecentDocument = false;

	// Clear times
	m_tmStarted.SetStatus( COleDateTime::null );

	// Reset Screen time stopwatch(P16 2901)
	m_swCurrTimeViewed.reset();

	// Update button states
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::saveOptions()
{
	/////////////////////////////
	// Write General Tab settings
	/////////////////////////////
	// Auto Zoom
	bool bCheck =  m_OptionsDlg.getAutoZoom();
	string strValue = bCheck ? "1" : "0";
	setSetting( gstrGENERAL_SECTION, gstrAUTO_ZOOM, strValue );

	// Auto Zoom Scale
	int iZoomScale = m_OptionsDlg.getAutoZoomScale();
	setSetting( gstrGENERAL_SECTION, gstrAUTO_ZOOM_SCALE, asString( iZoomScale ) );
	
	// Auto Select Tool
	strValue = asString((int) m_OptionsDlg.getAutoSelectTool());
	setSetting( gstrGENERAL_SECTION, gstrAUTO_SELECT_TOOL, strValue );
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::shouldCollectFeedback(bool bDocContainsRedactions)
{
	// check if feedback collection is enabled
	if( !m_UISettings.getCollectFeedback() )
	{
		return false;
	}

	// get the feedback collection options
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption eFeedbackCollect =
		m_UISettings.getFeedbackCollectOption();

	// feedback should be collected for this document iff one of the following is true:
	// (A) All verified documents should be collected
	// (B) Documents containing redactions should be collected AND this document contained 
	// at least one redaction
	// (C) Documents containing user corrections should be collected AND at least one user 
	// correction was made to this document
	return eFeedbackCollect == UCLID_REDACTIONCUSTOMCOMPONENTSLib::kFeedbackCollectAll ||
		bDocContainsRedactions && 
		(eFeedbackCollect & UCLID_REDACTIONCUSTOMCOMPONENTSLib::kFeedbackCollectRedact) != 0 ||
		m_bChangesMadeForHistory &&
		(eFeedbackCollect & UCLID_REDACTIONCUSTOMCOMPONENTSLib::kFeedbackCollectCorrect) != 0;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::setSetting(const string& strSection, const string& strKey, 
							  const string& strValue)
{
	// Just write directly to the INI file
	BOOL bResult = ::WritePrivateProfileString( strSection.c_str(), strKey.c_str(), 
		strValue.c_str(), m_strINIPath.c_str() );
	if (bResult == FALSE)
	{
		// Create and throw exception
		UCLIDException ue( "ELI15338", 
			"Unable to write setting to INI file!  INI file may be read-only." );
		ue.addDebugInfo( "INI File", m_strINIPath );
		ue.addDebugInfo( "Key", strKey );
		ue.addDebugInfo( "Value", strValue );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::selectDataRow(int nRow, bool bAllowAutozoom)
{
	// Get page number of the new row first
	long lPage = getGridItemPageNumber(nRow);

	// Move to this page before updating highlights [FlexIDSCore #3239]
	if ((lPage != 0) && (lPage != m_iActivePage))
	{
		setPage(lPage, false);
	}

	// update the old one
	DataDisplaySettings* pDDS = NULL;
	// 0 is not a valid index here
	if (m_iDataItemIndex > 0)
	{
		pDDS = m_vecDataItems[m_vecVerifyAttributes[m_iDataItemIndex - 1]].m_pDisplaySettings;
		updateDataItemHighlights(pDDS, false);
	}
	// color the newly selected attr
	pDDS = m_vecDataItems[m_vecVerifyAttributes[nRow - 1]].m_pDisplaySettings;
	updateDataItemHighlights(pDDS, true);

	// update the dds to reflect that this item has been reviewed
	pDDS->setReviewed();

	// Set this row to Normal text
	m_GridData.SetRowBoldBackground( nRow, false, false );

	// Set this row to Selected and make sure it is visible
	m_GridData.SelectRow( nRow );
	m_GridData.ScrollCellInView( nRow, 1);

	// Update index
	m_iDataItemIndex = nRow;

	// Update associated settings
	updateSettings( nRow );
	updateButtons();

	// Handle auto-zoom setting
	if (bAllowAutozoom && m_OptionsDlg.getAutoZoom())
	{
		OnButtonZoom();
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::selectExemptionCodes(bool bApplyToAll)
{
	// If applying exemption codes is disabled, we are done
	// OR there is an active confirmation dialog
	bool bEnabled = bApplyToAll ? m_bEnableApplyAllExemptions : m_bEnableApplyExemptions;
	if (!bEnabled || m_bActiveConfirmationDlg)
	{
		return;
	}

	// Get the currently selected data item
	DataItem& dataItem = m_vecDataItems[m_vecVerifyAttributes[m_iDataItemIndex - 1]];

	// Set the active confirmation dialog flag to true
	m_bActiveConfirmationDlg = true;

	try
	{
		// Disable the SRIR window
		WindowDisabler wd(getSRIRWindowHandle());

		// Create the dialog
		CExemptionCodesDlg* pDialog = getExemptionCodesDlg();

		// Show the dialog
		if (pDialog->DoModal() == IDOK)
		{
			// Get the new exemption codes to apply
			ExemptionCodeList& codes = pDialog->getExemptionCodes();

			// Check whether the exemption codes are being applied to all rows
			if (bApplyToAll)
			{
				// Display a warning message to user if exemption codes will be overwritten
				if (!promptBeforeApplyAllExemptions(codes))
				{
					// Set the active confirmation dialog flag to false [FlexIDSCore #3491]
					m_bActiveConfirmationDlg = false;

					// The user chose to cancel
					return;
				}

				// Set the exemption codes for all the rows
				int iCount = m_vecVerifyAttributes.size();
				for (int i = 0; i < iCount; i++)
				{
					// Retrieve this Settings object
					DataItem& dataItem = m_vecDataItems[m_vecVerifyAttributes[i]];

					// Get the DataDisplaySettings for this row
					DataDisplaySettings* pDDS = dataItem.m_pDisplaySettings;
					ASSERT_RESOURCE_ALLOCATION("ELI25324", pDDS != NULL);

					// Only apply exemption codes to items being redacted
					if (pDDS->getRedactChoice() == kRedactYes)
					{
						// Set the exemption codes 
						dataItem.setExemptionCodes(codes);
					}
				}

				// Refresh the data grid
				refreshDataGrid();

				// Select the previously selected data row
				selectDataRow(m_iDataItemIndex);
			}
			else
			{
				// Get the currently selected data item
				DataItem& dataItem = m_vecDataItems[m_vecVerifyAttributes[m_iDataItemIndex - 1]];

				// Store the new exemption codes for this row only
				dataItem.setExemptionCodes(codes);
				m_bChangesMadeForHistory = true;

				// Refresh the data grid
				refreshDataRow(m_iDataItemIndex, m_pActiveDDS, false);
			}

			// This is now the last used exemptions
			m_lastExemptions = codes;
			m_bHasLastExemptions = true;

			// Update the buttons
			updateButtons();
		}

		// Clear the active confirmation dialog flag
		m_bActiveConfirmationDlg = false;
	}
	catch(...)
	{
		// Clear the active confirmation dialog flag
		m_bActiveConfirmationDlg = false;

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::setDataStatus(int nRow, ERedactChoice eChoice)
{
	// Prepare string with icon definition
	string strBMP("#ICO(");

	switch (eChoice)
	{
	// Show Redact image
	case kRedactYes:
		strBMP += asString( IDI_ICON_REDACT );
		strBMP += ")";
		break;

	// Show No Redact image
	case kRedactNo:
		strBMP += asString( IDI_ICON_NOREDACT );
		strBMP += ")";
		break;

	// Show N/A image
	case kRedactNA:
		strBMP += asString( IDI_ICON_NA );
		strBMP += ")";
		break;

	// Error condition
	default:
		THROW_LOGIC_ERROR_EXCEPTION( "ELI11182" );
		break;
	}

	// Display the status icon centered
	m_GridData.SetStyleRange(CGXRange( nRow, giDATAGRID_STATUS_COLUMN ),
								CGXStyle()
									.SetControl( CGXIconControl::IDS_CTRL_ICON )
									.SetValue( strBMP.c_str() )
									.SetHorizontalAlignment( DT_CENTER )
									.SetVerticalAlignment( DT_VCENTER ),
								gxOverride,
								0,
								GX_INVALIDATE
						); 
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::setPage(int iPage, bool bSelectFirst)
{
	// Open specified page in SRIR
	m_ipSRIR->SetCurrentPageNumber( iPage );

	// Set background color in Pages grid
	setPageBackground( iPage );

	// Clear any existing selection
	m_GridData.ClearSelection();

	// Set any items on previous page to Normal
	if ((iPage != m_iActivePage) && (m_iActivePage > 0))
	{
		setPageNormal( m_iActivePage );
	}

	// Save setting
	m_iActivePage = iPage;

	// Add new page to Reviewed vector
	m_vecPagesReviewed.push_back( iPage );

	// Step through each sorted Attribute
	long lCount = m_vecVerifyAttributes.size();
	bool bFound = false;
	int i;
	for (i = 1; i <= lCount; i++)
	{
		// Get Page Number text for this Data grid item
		long lPage = getGridItemPageNumber( i );

		// Test for first item on this page
		if (lPage == iPage)
		{
			if (bSelectFirst)
			{
				// Select this item
				selectDataRow( i );
			}

			bFound = true;
			break;
		}
	}

	// Clear pointer if not found
	if (!bFound)
	{
		m_pActiveDDS = NULL;
		updateButtons();
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::setPageBackground(int iPage)
{
	if ((iPage < 0) || (iPage > m_iTotalPages))
	{
		UCLIDException ue( "ELI11205", "Invalid page for setting background color!" );
		ue.addDebugInfo( "Page number", iPage );
		ue.addDebugInfo( "Pages in document", m_iTotalPages );
		throw ue;
	}

	// Determine row and column within Pages grid
	int iWhichRow = (iPage - 1) / giPAGES_PER_ROW + 1;
	int iWhichCol;
	int iRemainder = iPage % giPAGES_PER_ROW;
	if (iRemainder == 0)
	{
		// Last cell in row
		iWhichCol = giPAGES_PER_ROW;
	}
	else
	{
		iWhichCol = iRemainder;
	}

	// Set the cell to normal font and gray background
	m_GridPages.SetCellBoldBackground( iWhichRow, iWhichCol, false, true );
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::setPageNormal(int iPage)
{
	// Step through each sorted Attribute
	long lCount = m_vecVerifyAttributes.size();
	bool bFound = false;
	int i;
	for (i = 1; i <= lCount; i++)
	{
		// Get Page Number text for this Data grid item
		long lPage = getGridItemPageNumber( i );

		// Test for desired page
		if (lPage == iPage)
		{
			// Set to Normal font
			m_GridData.SetRowBoldBackground( i, false, false );

			// Set DDS object to reviewed
			DataDisplaySettings* pDDS = 
				m_vecDataItems[m_vecVerifyAttributes[i - 1]].m_pDisplaySettings;
			ASSERT_RESOURCE_ALLOCATION( "ELI11876", pDDS != NULL );
			pDDS->setReviewed();
			break;
		}
		else if (lPage > iPage)
		{
			// Now past the desired page
			break;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::setSourceDocName(IAttributePtr& ripAttribute, const string& strSourceDocName)
{
	// get the ripAttribute's value
	ISpatialStringPtr ipValue(ripAttribute->Value);
	ASSERT_RESOURCE_ALLOCATION("ELI20114", ipValue != NULL);

	// set its SourceDocName
	ipValue->SourceDocName = strSourceDocName.c_str();

	// check if this ripAttribute has any sub attributes
	IIUnknownVectorPtr ipSubAttributes(ripAttribute->SubAttributes);
	if(ipSubAttributes != NULL)
	{
		// set the source document name of each subattribute recursively
		long lSize = ipSubAttributes->Size();
		for(long i=0; i < lSize; i++)
		{
			setSourceDocName( (IAttributePtr) ipSubAttributes->At(i), strSourceDocName);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::updateSettings(int iDataIndex)
{
	// Retrieve this Attribute
	IAttributePtr	ipAttr = m_vecDataItems[m_vecVerifyAttributes[iDataIndex - 1]].m_ipAttribute;
	ASSERT_RESOURCE_ALLOCATION("ELI11311", ipAttr != NULL);

	// Get the associated DDS object
	int iIndex = m_vecVerifyAttributes[iDataIndex - 1];
	if (iIndex != -1)
	{
		m_pActiveDDS = m_vecDataItems[iIndex].m_pDisplaySettings;
	}
	else
	{
		m_pActiveDDS = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::updateTitleBar(string strFile)
{
	// "Filename - ID Shield"
	if (strFile.length() > 0)
	{
		string strName = getFileNameFromFullPath( strFile );
		strName += " - ";
		strName += gstrTITLE.c_str();

		SetWindowText( strName.c_str() );
	}
	// Just "ID Shield"
	else
	{
		SetWindowText( gstrTITLE.c_str() );
	}
}
//-------------------------------------------------------------------------------------------------
long CDataAreaDlg::getGridIndex(long nItem)
{
	int i;
	long nSize = m_vecVerifyAttributes.size();
	for (i = 0; i < nSize; i++)
	{
		if(m_vecVerifyAttributes[i] == nItem)
		{
			return i;
		}
	}
	return -1;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::updateButtons()
{
	CToolBarCtrl& toolbar = m_apToolBar->GetToolBarCtrl();

	// Check for no image loaded
	if (m_iActivePage == 0)
	{
		// Just disable all the buttons - except Options
		toolbar.EnableButton(ID_BUTTON_PREVIOUS_ITEM, FALSE );
		toolbar.EnableButton(ID_BUTTON_NEXT_ITEM, FALSE );
		toolbar.EnableButton(ID_BUTTON_SAVE, FALSE );
		toolbar.EnableButton(ID_BUTTON_TOGGLE_REDACTION, FALSE );
		toolbar.EnableButton(ID_BUTTON_ZOOM_TO_ITEM, FALSE );
		toolbar.EnableButton(ID_BUTTON_PREVIOUS, FALSE );
		toolbar.EnableButton(ID_BUTTON_NEXT, FALSE );
		toolbar.EnableButton(ID_BUTTON_APPLY_EXEMPTION, FALSE);
		toolbar.EnableButton(ID_BUTTON_ALL_EXEMPTION, FALSE);
		toolbar.EnableButton(ID_BUTTON_LAST_EXEMPTION, FALSE);

		// Disable shortcut keys
		m_bEnableGeneral = false;
		m_bEnableToggle = false;
		m_bEnablePrevious = false;
		m_bEnableApplyExemptions = false;
		m_bEnableLastExemptions = false;
		m_bEnableApplyAllExemptions = false;
	}
	// An image is loaded
	else
	{
		// Always enable Next and Save and Zoom
		toolbar.EnableButton( ID_BUTTON_NEXT_ITEM, TRUE );
		toolbar.EnableButton( ID_BUTTON_SAVE, TRUE );
		toolbar.EnableButton( ID_BUTTON_ZOOM_TO_ITEM, TRUE );
		m_bEnableGeneral = true;

		// Disable the toggle and exemption code buttons by default
		m_bEnableToggle = false;
		m_bEnableApplyExemptions = false;
		m_bEnableLastExemptions = false;
		m_bEnableApplyAllExemptions = false;

		// Check if an item is selected
		if (m_pActiveDDS != NULL)
		{
			// Enable toggle if an item is selected
			m_bEnableToggle = true;

			// Enable exemption code buttons if the current item is toggled on
			if (m_pActiveDDS->getRedactChoice() == kRedactYes)
			{
				m_bEnableApplyExemptions = true;
				m_bEnableLastExemptions = m_bHasLastExemptions;
				m_bEnableApplyAllExemptions = true;
			}
			else
			{
				// Enable apply all exemptions codes iff there is at least one toggled redaction
				m_bEnableApplyAllExemptions = findFirstToggledRedaction() != -1;
			}
		}

		// Enable/disable toggle and exemption code buttons
		toolbar.EnableButton(ID_BUTTON_TOGGLE_REDACTION, asMFCBool(m_bEnableToggle));
		toolbar.EnableButton(ID_BUTTON_APPLY_EXEMPTION, asMFCBool(m_bEnableApplyExemptions));
		toolbar.EnableButton(ID_BUTTON_LAST_EXEMPTION, asMFCBool(m_bEnableLastExemptions));
		toolbar.EnableButton(ID_BUTTON_ALL_EXEMPTION, asMFCBool(m_bEnableApplyAllExemptions));

		// Enable Previous item if not at first entry
		m_bEnablePrevious = true;
		if ((m_iDataItemIndex == 1) && (m_iActivePage == 1))
		{
			// First item, if on the first page
			m_bEnablePrevious = false;
		}
		else if (m_vecVerifyAttributes.size() == 0)
		{
			// No items
			m_bEnablePrevious = false;
		}
		toolbar.EnableButton( ID_BUTTON_PREVIOUS_ITEM, asMFCBool(m_bEnablePrevious) );

		// Enable and disable Previous / Next document buttons based on 
		// position in and size of queue
		if ((m_nNumPreviousDocsToQueue > 0) && (m_nNumPreviousDocsQueued > 0))
		{
			// Queue has items, are we at the oldest end of the queue
			if (m_nPositionInQueue > 0)
			{
				// Can still move to a previous document
				toolbar.EnableButton( ID_BUTTON_PREVIOUS, TRUE );
			}
			else
			{
				// Currently reviewing oldest previous document
				toolbar.EnableButton( ID_BUTTON_PREVIOUS, FALSE );
			}

			// Check for new end of queue
			if (!isInHistoryQueue())
			{
				// Cannot move to a newer document
				toolbar.EnableButton( ID_BUTTON_NEXT, FALSE );
			}
			else
			{
				// Currently reviewing a previous document, can move newer
				toolbar.EnableButton( ID_BUTTON_NEXT, TRUE );
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::updateCurrentHistoryItems(bool bSaveDataItemsToHistory)
{
	// Validate index against number of items stored
	if (m_nPositionInQueue > m_nNumPreviousDocsQueued)
	{
		UCLIDException ue("ELI14917", "Invalid position in history collection!");
		ue.addDebugInfo("History Size", m_nNumPreviousDocsQueued );
		ue.addDebugInfo("Position", m_nPositionInQueue );
		throw ue;
	}

	// Stop the stopwatch so when it is saved in the history records it will not 
	// still be in a running state and add time while it is not the active document
	// P16 #2935
	m_swCurrTimeViewed.stop();

	// Add source document to collection, if not already present
	// Note that this entry should not change with repeated navigation 
	// either with or without Save.
	bool bRepeatedEntry = false;
	if (!isInHistoryQueue())
	{
		// Check existing count in vector
		if ((int)m_vecDocumentHistory.size() < m_nNumPreviousDocsQueued)
		{
			// The Queued count has already been incremented, just add the name
			m_vecDocumentHistory.push_back(m_document);
		}
		else
		{
			// Get name
			string strName = m_document.m_strOriginal;

			// Check last existing name
			string strLast = m_vecDocumentHistory[m_vecDocumentHistory.size() - 1].m_strOriginal;

			// Add to collection if not present
			if (strName != strLast)
			{
				m_vecDocumentHistory.push_back(m_document);
			}
			else
			{
				// This item is already present in the history
				bRepeatedEntry = true;
			}
		}

		// Append this list to the collection, if needed
		if (!bRepeatedEntry)
		{
			m_vecReviewedPagesHistory.push_back( m_vecPagesReviewed );
			
			// Add the current stopwatch to the history list 
			m_vecDurationsHistory.push_back(m_swCurrTimeViewed);

			// Add the current IDShield data record(P16 2901)
			m_vecIDShieldDataHistory.push_back(m_idsCurrData);
		}
		else
		{
			// Update the last entry with the current collection
			m_vecReviewedPagesHistory[m_vecReviewedPagesHistory.size() - 1] = m_vecPagesReviewed;
			
			// Update the last entry with the current stopwatch 
			m_vecDurationsHistory[m_vecDurationsHistory.size() - 1] = m_swCurrTimeViewed;
			
			// Update the last entry with the current IDShieldData record(P16 2901)
			m_vecIDShieldDataHistory[m_vecIDShieldDataHistory.size() - 1] = m_idsCurrData;
		}
	}
	else
	{
		// Replace existing list in the collection
		m_vecReviewedPagesHistory[m_nPositionInQueue] = m_vecPagesReviewed;

		// Replace existing stop watch with the current stopwatch in the history list 
		m_vecDurationsHistory[m_nPositionInQueue] = m_swCurrTimeViewed;

		// Replace existing IDShieldData with the current IDShieldData(P16 2901)
		m_vecIDShieldDataHistory[m_nPositionInQueue] = m_idsCurrData;
	}

	// Save the data items if required (P16 2902)
	if (bSaveDataItemsToHistory)
	{
		// Create local sorted collection of Data Items
		int nSortedCount = m_vecVerifyAttributes.size();
		vector<DataItem> vecSortedDataItems;
		for (int j = 0; j < nSortedCount; j++)
		{
			// Put Data Item from this row into this position in sorted collection
			vecSortedDataItems.push_back( m_vecDataItems[m_vecVerifyAttributes[j]] );
		}

		// Add collection of Data Items, if needed
		if (!bRepeatedEntry)
		{
			// Add sorted list of Data Items to history collection
			// Note that this entry can change with repeated navigation 
			// either with or without Save.
			if (!isInHistoryQueue())
			{
				// Append this list to the collection
				m_vecDataItemHistory.push_back( vecSortedDataItems );
			}
			else
			{
				// Replace existing list in the collection
				m_vecDataItemHistory[m_nPositionInQueue] = vecSortedDataItems;
			}
		}
		else
		{
			// Update the last entry with the current collection
			m_vecDataItemHistory[m_vecDataItemHistory.size() - 1] = vecSortedDataItems;
		}

		// Indicate that the document data items have now been committed to history
		m_bDocCommittedToHistory = true;
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::updateDataItemHighlights(DataDisplaySettings* pDDS, bool bIsSelected)
{
	ASSERT_ARGUMENT("ELI14113", pDDS != NULL);

	vector<long> vecIDs = pDDS->getHighlightIDs();
	ERedactChoice eChoice = pDDS->getRedactChoice();

	BOOL mbSelected = asMFCBool(bIsSelected);

	unsigned int ui;
	for (ui = 0; ui < vecIDs.size(); ui++)
	{
		// set the zone fill color depending upon whether it is redacted or not
		if (eChoice == kRedactYes)
		{
			m_ipOCX->setEntityColor(vecIDs[ui], gnREDACTED_ZONE_COLOR);
			m_ipOCX->setZoneFilled(vecIDs[ui], TRUE);
		}
		else
		{
			m_ipOCX->setZoneFilled(vecIDs[ui], FALSE);
		}

		// Select or deselect this entity as requested
		m_ipOCX->selectEntity(vecIDs[ui], mbSelected);
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::writeVOAFile(const string& strImageFile, const string& strVOAFile)
{
	// Put the m_vecDataItems into an IUnknown Vector
	int iSizeOfVector = m_vecDataItems.size();
	IIUnknownVectorPtr ipVecVOAData( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION("ELI15164", ipVecVOAData != NULL );

	// Push each object from the Data Items vector into the IUnknown vector
	for (int i = iSizeOfVector; i > 0; i--)
	{
		// Get each data item
		DataItem dataItemTemp = m_vecDataItems.at(i-1);
		ASSERT_RESOURCE_ALLOCATION("ELI14654", &dataItemTemp != NULL );

		// Get the display settings
		DataDisplaySettings* pDDS = dataItemTemp.m_pDisplaySettings;
		ERedactChoice eRedact = pDDS->getRedactChoice();

		// If the item is marked as a redaction item add it to the items to save
		if( eRedact == kRedactYes )
		{
			// Get the attribute from the data item
			IAttributePtr ipAttrPtr = dataItemTemp.m_ipAttribute;
			ASSERT_RESOURCE_ALLOCATION("ELI14647", ipAttrPtr != NULL );

			// set the source document name of this attribute and all its child attributes
			setSourceDocName(ipAttrPtr, strImageFile);

			// Push the data item's attribute onto the vector
			ipVecVOAData->PushBack( ipAttrPtr );
		}
	}

	// Save the IUnknown vector to the specified VOA file
	ipVecVOAData->SaveTo(get_bstr_t(strVOAFile), VARIANT_TRUE);

	// Check whether the voa file should be opened
	if (m_bOpenVoaOnShiftSave && GetKeyState(VK_SHIFT) < 0)
	{
		openVoa(strVOAFile);
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::clearDataItemVector(vector<DataItem>& rvecDataItems)
{
	int iSize = rvecDataItems.size();
	for (int i = 0; i < iSize; i++)
	{
		DataDisplaySettings* pDDS = rvecDataItems[i].m_pDisplaySettings;
		delete pDDS;
	}
	rvecDataItems.clear();
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::setGridTypeColumnList()
{
	// build the list of values to be displayed in the drop down list control
	string strListValues = "";
	for (set<string>::iterator it = m_setAttributeTypes.begin();
		it != m_setAttributeTypes.end(); it++)
	{
		// do not add the empty string to the list since we have already added it
		// [p16 #2867]
		if (!(*it).empty())
		{
			strListValues += "\n" + *it;
		}
	}

	// add the list of values to the style setting for the drop down list control
	CGXStyle gxListStyle;
	gxListStyle.SetChoiceList(strListValues.c_str());

	// set the drop down list (in the type column) with the new list of values
	m_GridData.SetStyleRange(CGXRange().SetCols(giDATAGRID_TYPE_COLUMN), gxListStyle);
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::resetAttributeTypes()
{
	// clear the set of attributes first
	m_setAttributeTypes.clear();

	// now refill the set with the types from the INI file
	for (vector<string>::iterator it = m_vecAttributesFromINIFile.begin();
		it != m_vecAttributesFromINIFile.end(); it++)
	{
		m_setAttributeTypes.insert(*it);
	}
}
//-------------------------------------------------------------------------------------------------
UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr CDataAreaDlg::getIDShieldDB()
{
	// Check if IDShieldDB instance has been created
	if (m_ipIDShieldDB == NULL)
	{
		m_ipIDShieldDB.CreateInstance(CLSID_IDShieldProductDBMgr);
		ASSERT_RESOURCE_ALLOCATION("ELI19078", m_ipIDShieldDB != NULL );
	}

	// Return the instance
	return m_ipIDShieldDB;
}
//-------------------------------------------------------------------------------------------------
long CDataAreaDlg::findFirstUnwantedExemptionCode()
{
	// Default return value to -1
	long nReturn = -1;

	// Get the size of the verify attributes vector
	long lCount = m_vecVerifyAttributes.size();
	for (long i = 0; i < lCount; i++)
	{
		// Get the DataDisplaySettings for this row
		DataDisplaySettings* pDDS = m_vecDataItems[m_vecVerifyAttributes[i]].m_pDisplaySettings;
		ASSERT_RESOURCE_ALLOCATION("ELI25323", pDDS != NULL);

		// Check if this data item's exemption codes field is non-empty and 
		// its redact choice is not yes
		if (pDDS->getExemptionCodes().length() > 0 && pDDS->getRedactChoice() != kRedactYes)
		{
			// Exemption code is empty, set the row index to this item+1 (grid is 1 based)
			nReturn = i+1;

			// Found an non-empty exemption code, no need to look for more
			break;
		}
	}

	return nReturn;
}
//-------------------------------------------------------------------------------------------------
long CDataAreaDlg::findFirstEmptyExemptionCode()
{
	// Default return value to -1
	long nReturn = -1;

	// Get the size of the verify attributes vector
	long lCount = m_vecVerifyAttributes.size();
	for (long i=0; i < lCount; i++)
	{
		// Get the DataDisplaySettings for this row
		DataDisplaySettings* pDDS = m_vecDataItems[m_vecVerifyAttributes[i]].m_pDisplaySettings;
		ASSERT_RESOURCE_ALLOCATION("ELI24985", pDDS != NULL);

		// Check if this data item's exemption codes field is empty and its redact choice is yes
		if (pDDS->getExemptionCodes().empty() && pDDS->getRedactChoice() == kRedactYes)
		{
			// Exemption code is empty, set the row index to this item+1 (grid is 1 based)
			nReturn = i+1;

			// Found an empty exemption code, no need to look for more break from loop
			break;
		}
	}

	return nReturn;
}
//-------------------------------------------------------------------------------------------------
long CDataAreaDlg::findFirstEmptyRedactionType()
{
	// default return value to -1
	long nReturn = -1;

	// get the size of the verify attributes vector
	long lCount = m_vecVerifyAttributes.size();
	for (long i=0; i < lCount; i++)
	{
		// get the DataDisplaySettings for this row
		DataDisplaySettings* pDDS = m_vecDataItems[m_vecVerifyAttributes[i]].m_pDisplaySettings;
		ASSERT_RESOURCE_ALLOCATION("ELI20267", pDDS != NULL);

		// check if this data items type is empty 
		// [p16 #2868] and its redact choice is yes
		if (pDDS->getType().empty() && pDDS->getRedactChoice() == kRedactYes)
		{
			// type is empty, set the row index to this item+1 (grid is 1 based)
			nReturn = i+1;

			// found an empty type, no need to look for more break from loop
			break;
		}
	}

	return nReturn;
}
//-------------------------------------------------------------------------------------------------
long CDataAreaDlg::findFirstToggledRedaction()
{
	// Default return value to -1
	long nReturn = -1;

	// Get the size of the verify attributes vector
	long lCount = m_vecVerifyAttributes.size();
	for (long i=0; i < lCount; i++)
	{
		// Get the DataDisplaySettings for this row
		DataDisplaySettings* pDDS = m_vecDataItems[m_vecVerifyAttributes[i]].m_pDisplaySettings;
		ASSERT_RESOURCE_ALLOCATION("ELI25326", pDDS != NULL);

		if (pDDS->getRedactChoice() == kRedactYes)
		{
			// Set the row index to this item+1 (Grid is 1 based)
			nReturn = i+1;

			break;
		}
	}

	return nReturn;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::showDropDownList(int nRow)
{
	// if there is no value set for the column, set the appropriate default value [p16 #2835]
	if (m_GridData.GetCellValue(nRow, giDATAGRID_TYPE_COLUMN).empty())
	{
		// choose the appropriate default value for the cell [p16 #2835] - JDS 01/29/2008
		string strValueToSet = m_strDefaultRedactionType.empty() ? 
			m_strLastSelectedRedactionType : m_strDefaultRedactionType;

		// set the appropriate default value for the cell [p16 #2835] - JDS 01/29/2008
		m_GridData.SetValueRange(CGXRange(nRow, giDATAGRID_TYPE_COLUMN), strValueToSet.c_str());

		// also need to ensure that the default setting is stored in the attribute
		// [p16 #2859] - JDS - 01/29/2008
		updateCurrentlyModifiedAttribute(nRow, strValueToSet);
	}

	// move the user to the type cell for this row 
	m_GridData.SetCurrentCell(nRow, giDATAGRID_TYPE_COLUMN); 

	// ensure that the cell will have a white background while the list is
	// displayed
	m_GridData.UpdateCellToNotHighlight(nRow, giDATAGRID_TYPE_COLUMN);

	// get the control for this cell
	CGXControl* pCGXControl = m_GridData.GetCurrentCellControl();
	ASSERT_ARGUMENT("ELI20269", pCGXControl != NULL);

	// show the drop down list for this cell
	pCGXControl->OnClickedButton(NULL);
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::updateCurrentlyModifiedAttribute(int nRow, string strNewType)
{
	// get the currently selected display setting
	DataDisplaySettings* pDDS = 
		m_vecDataItems[m_vecVerifyAttributes[nRow - 1]].m_pDisplaySettings;
	ASSERT_RESOURCE_ALLOCATION("ELI20272", pDDS != NULL);

	if (pDDS->getType() != strNewType)
	{
		// set the new type in the data display setting
		pDDS->setType(strNewType);

		// get the currently selected attribute
		IAttributePtr ipAttribute = 
			m_vecDataItems[m_vecVerifyAttributes[nRow - 1]].m_ipAttribute;
		ASSERT_RESOURCE_ALLOCATION("ELI19857", ipAttribute != NULL);

		// set the new type value in the attribute
		ipAttribute->Type = get_bstr_t(strNewType);
	
		// Set dirty flag [p16 #2866]
		m_bChangesMadeForHistory = true;
	}
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::checkAndPromptForEmptyRedactionTypes(const string& strAction)
{
	// If empty redaction types are allowed, no need to check for them
	if (!m_UISettings.getRequireRedactionTypes())
	{
		return false;
	}

	bool bReturn = false;

	// check for empty redaction type
	long nRow = findFirstEmptyRedactionType();
	
	// if nRow != -1 then there is an empty redaction type, prompt the user
	// and set the currently selected row to the empty row
	if (nRow != -1)
	{
		bReturn = true;

		// Display the error message
		displayMissingDataMessage(strAction, "redaction types", nRow);

		// Show the drop down list for this row
		showDropDownList(nRow);
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::checkAndPromptForEmptyExemptionCodes(const string& strAction)
{
	// If empty exemption codes are allowed, no need to check for them
	if (!m_UISettings.getRequireExemptionCodes())
	{
		return false;
	}

	bool bReturn = false;

	// Check for missing exemption codes
	long nRow = findFirstEmptyExemptionCode();
	
	// If nRow != -1 then there is an empty exemption code, prompt the user
	// and set the currently selected row to the empty row
	if (nRow != -1)
	{
		bReturn = true;

		// Display the error message
		displayMissingDataMessage(strAction, "exemption codes", nRow);
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
bool CDataAreaDlg::checkAndPromptForUnwantedExemptionCodes(const string& strAction)
{
	bool bReturn = false;

	// Check for exemption codes on unredacted items
	long nRow = findFirstUnwantedExemptionCode();
	
	// If nRow != -1 then there is an inappropriate exemption code, prompt the user
	// and set the currently selected row to the non-empty row
	if (nRow != -1)
	{
		bReturn = true;

		// Display the error message and disable other window activity
		{
			WindowDisabler wd(getSRIRWindowHandle());
			m_bActiveConfirmationDlg = true;
			string strError = "Cannot " + strAction + 
				" with exemption codes applied to unredacted items.";
			MessageBox(strError.c_str(), "Inappropriate exemption codes", MB_OK | MB_ICONERROR);
			m_bActiveConfirmationDlg = false;
		}

		// Select the row
		selectDataRow(nRow);
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::displayMissingDataMessage(const string& strAction, const string& strData, int iRow)
{
	if (m_bActiveConfirmationDlg)
	{
		return;
	}

	// Construct the error dialog's title and caption
	string strTitle = "Missing " + strData;
	string strError = "Cannot " + strAction + " while there are empty " + strData + ".";

	{
		WindowDisabler wd(getSRIRWindowHandle());
		m_bActiveConfirmationDlg = true;
		// Display the error message
		MessageBox(strError.c_str(), strTitle.c_str(), MB_OK | MB_ICONERROR);
		m_bActiveConfirmationDlg = false;
	}

	// Select the row
	selectDataRow(iRow);
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::saveIDShieldData()
{
	// Stop the stop watch before saving it in the history list
	m_swCurrTimeViewed.stop();

	// Save the new IDShield data to the database
	// Set the FAMDB
	getIDShieldDB()->FAMDB = m_ipFAMDB;

	// Add the IDShield data record
	getIDShieldDB()->AddIDShieldData(m_lFileID, VARIANT_TRUE, m_swCurrTimeViewed.getElapsedTime(), 
		m_idsCurrData.m_lNumHCDataFound,	m_idsCurrData.m_lNumMCDataFound, m_idsCurrData.m_lNumLCDataFound, 
		m_idsCurrData.m_lNumCluesFound, m_idsCurrData.m_lTotalRedactions, m_idsCurrData.m_lTotalManualRedactions);
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::addManualRedactions(vector<long> &vecZoneIDs)
{
	// Create a text to represent the new zone
	string strText = "[No text]";

	///////////////////////////////
	// Create Manual redaction item
	///////////////////////////////

	// Get manual DCLevel - for Display type
	DataConfidenceLevel*	pDCL = m_mapDCLevels["Man"];
	ASSERT_RESOURCE_ALLOCATION( "ELI11372", pDCL != NULL );

	// Get page number
	long nPageNum = m_ipSRIR->GetCurrentPageNumber();

	// Create a new Data Display Settings object and set Zone ID
	DataDisplaySettings* pDDS = new DataDisplaySettings(strText, pDCL->getShortName(), "", 
		nPageNum, kRedactYes, pDCL->isNonRedactWarningEnabled(), pDCL->isRedactWarningEnabled());
	ASSERT_RESOURCE_ALLOCATION( "ELI11373", pDDS != NULL );
	pDDS->setHighlightIDs(vecZoneIDs);

	// Set object as viewed
	pDDS->setReviewed();

	// Create a vector of the raster zones that were created
	IIUnknownVectorPtr ipRZones(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI15165", ipRZones != NULL);

	// Iterate over each created highlight
	unsigned int uiSize = vecZoneIDs.size();
	for (unsigned int i = 0; i < uiSize; i++)
	{
		long nZoneID = vecZoneIDs[i];

		// Set the redaction fill color and the zoneFilled as TRUE [p13 #4865]
		m_ipOCX->setEntityColor(nZoneID, gnREDACTED_ZONE_COLOR);
		m_ipOCX->setZoneFilled(nZoneID, TRUE);

		// Set the border color for the zone
		m_ipOCX->setZoneBorderColor(nZoneID, pDCL->getDisplayColor());
		m_ipOCX->showZoneBorder(nZoneID, true);

		// Get the Raster Zone for the swipe and put it into the vector
		IRasterZonePtr ipRasterZone = getZoneFromID( nZoneID );
		ASSERT_RESOURCE_ALLOCATION( "ELI14651", ipRasterZone != NULL );
		ipRZones->PushBack( ipRasterZone );
	}

	// Get the image file name
	string strFile = asString(m_ipSRIR->GetImageFileName());

	// Store page height and width
	int nHeight(0), nWidth(0);
	getImagePixelHeightAndWidth(strFile, nHeight, nWidth, nPageNum);

	// Set the page info for the new spatial string
	UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
	ASSERT_RESOURCE_ALLOCATION("ELI09164", ipPageInfo != NULL);

	ipPageInfo->SetPageInfo(nWidth, nHeight, kRotNone, 0.0);

	// Create the spatial page info map
	ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI20239", ipPageInfoMap != NULL);

	// Add the local SpatialPageInfo object as the appropriate entry
	ipPageInfoMap->Set(nPageNum, ipPageInfo);

	// Create a new spatial string (value) for the Attribute
	ISpatialStringPtr ipSS(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI15154", ipSS != NULL);

	// Build the spatial string from the vector of Raster Zones.
	ipSS->CreateHybridString(ipRZones, gstrMANUALREDACTIONTEXT.c_str(), strFile.c_str(),
		ipPageInfoMap);

	// Create a new Attribute
	IAttributePtr	ipAttr( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION( "ELI11379", ipAttr != NULL );

	// Set the type, the name, and the value of the manual redaction's Attribute
	ipAttr->Type = _bstr_t("");
	ipAttr->Name = _bstr_t(gstrMANUALREDACTIONTEXT.c_str());
	ipAttr->Value = ipSS;

	// Add new items to vectors
	DataItem item;
	item.m_ipAttribute = ipAttr;
	item.m_pDisplaySettings = pDDS;
	m_vecDataItems.push_back(item);

	// check if m_iDataItemIndex == -1 (no items in the grid, or the first item
	// has not been displayed yet (i.e. currently viewing page 1 but first item is
	// on page 3).  if m_iDataItemIndex == -1 then set to 0 [p16 #2893]
	if (m_iDataItemIndex == -1)
	{
		m_iDataItemIndex = 0;
	}

	// get an iterator to the current location in the vector
	// if m_iDataitemIndex is the last element in the vector
	// set the iterator to end
	vector<long>::iterator vecIt;
	if (m_iDataItemIndex == m_vecVerifyAttributes.size())
	{
		vecIt = m_vecVerifyAttributes.end();
	}
	else
	{
		// set iterator to point to the m_iDataItemIndex
		vecIt = m_vecVerifyAttributes.begin()+m_iDataItemIndex;
	}

	// insert the data item index after the current selected row
	m_vecVerifyAttributes.insert(vecIt, m_vecDataItems.size()-1);

	// store the previous row value
	int iPreviousRow = m_iDataItemIndex;

	// Refresh the grid
	refreshDataGrid();

	// Set the dirty flag
	m_bChangesMadeForHistory = true;

	// Retrieve old page number
	long lOldPage = 0;
	if (iPreviousRow > 0)
	{
		lOldPage = getGridItemPageNumber( iPreviousRow );
	}

	// select the newly added manual redaction row
	// this will also update the m_iDataItemIndex to point to our new row
	// Do not auto-zoom around the newly created redaction [FlexIDSCore #3448]
	selectDataRow(iPreviousRow+1, false);

	// check if the required redaction types flag is set
	// and if so, show the drop down list [p16 #2833]
	if (m_UISettings.getRequireRedactionTypes())
	{
		showDropDownList(m_iDataItemIndex);
	}

	// Change tool, if desired
	EAutoSelectTool eTool = m_OptionsDlg.getAutoSelectTool();
	if (eTool != kNoTool)
	{
		if (eTool == kPan)
		{
			// Set the current tool to Pan tool
			m_ipSRIR->SetCurrentTool(kBtnPan);
		}
		else if (eTool == kSelectHighlight)
		{
			// Set the current tool to Select Highlight tool
			m_ipSRIR->SetCurrentTool(kBtnSelectHighlight);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::applyLastExemptionCodes()
{
	// If applying the last exemption codes is disabled, we are done
	if (!m_bEnableLastExemptions)
	{
		return;
	}

	// Get the currently selected data item
	DataItem& dataItem = m_vecDataItems[m_vecVerifyAttributes[m_iDataItemIndex - 1]];

	// Store the new exemption codes
	dataItem.setExemptionCodes(m_lastExemptions);
	m_bChangesMadeForHistory = true;
	m_bHasLastExemptions = true;

	// Refresh the data grid
	refreshDataRow(m_iDataItemIndex, m_pActiveDDS, false);
}
//-------------------------------------------------------------------------------------------------
HWND CDataAreaDlg::getSRIRWindowHandle()
{
	if (m_hWndSRIR == NULL)
	{
		IInputReceiverPtr ipSR = m_ipSRIR;
		ASSERT_RESOURCE_ALLOCATION("ELI25306", ipSR != NULL);

		m_hWndSRIR = (HWND) ipSR->WindowHandle;
	}

	return m_hWndSRIR;
}
//-------------------------------------------------------------------------------------------------
