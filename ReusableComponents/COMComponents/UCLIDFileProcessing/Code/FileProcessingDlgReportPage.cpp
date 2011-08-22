// FileProcessingDlgReportPage.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "uclidfileprocessing.h"
#include "FileProcessingDlgReportPage.h"

#include <FileProcessingConfigMgr.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <fstream>
#include <LoadFileDlgThread.h>
#include <SuspendWindowUpdates.h>

#include <cpputil.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Column numbers
static const int g_iCOLUMN_ONE = 0;
static const int g_iCOLUMN_TWO = 1;
static const int g_iCOLUMN_THREE = 2;

// Globals to keep track of what goes in what row
// Global Stats ListCtrl
const int g_iGLOBAL_BYTES_PROC_ROW = 0;
const int g_iGLOBAL_PAGES_PROC_ROW = 1;
const int g_iGLOBAL_DOCS_PROC_ROW = 2;
const int g_iGLOBAL_EST_TIME_REM_ROW = 3;
const int g_iGLOBAL_EST_COMP_TIME_ROW = 4;

// Local Stats ListCtrl
const int g_iLOCAL_BYTES_PROC_ROW = 0;
const int g_iLOCAL_PAGES_PROC_ROW = 1;
const int g_iLOCAL_DOCS_PROC_ROW = 2;
const int g_iLOCAL_TOTAL_PROC_TIME_ROW = 3;
const int g_iLOCAL_TOTAL_RUN_TIME_ROW = 4;

const int g_iLABEL_COL_WIDTH = 170;
const int g_iPERCENT_COL_WIDTH = 200;
const int g_iTHROUGHPUT_COL_WIDTH = 200;

const int g_iPADDING_AROUND_CONTROLS = 10;

// Controls the maximum size of the vector of DB snapshots
const int g_iMAX_SNAPSHOTS_IN_VECTOR = 1000;

// Control for the maximum number of estimated completion times in the vector
const int g_iMAX_COMPLETION_RATES_IN_VECTOR = 60;

// Interpret cautiously label
const std::string gstrINTERPRET_CAUTIOUSLY_LABEL = "Interpret statistics cautiously*";

// The amount of time to wait before displaying the interpret statistics label
const int giINTERPRET_STATS_DELAY = 60;

//-------------------------------------------------------------------------------------------------
// Snapshot class
//-------------------------------------------------------------------------------------------------
Snapshot::Snapshot()
{
}
//-------------------------------------------------------------------------------------------------
Snapshot::Snapshot( ATL::CTime currentTime, UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipNewActionStats )
: m_curTime(currentTime),
m_ipActionStats(ipNewActionStats)
{
}
//-------------------------------------------------------------------------------------------------
Snapshot::~Snapshot()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16526");
}
//-------------------------------------------------------------------------------------------------
CTime Snapshot::getTime()
{
	return m_curTime;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IActionStatisticsPtr Snapshot::getActionStatistic()
{
	return m_ipActionStats;
}

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgReportPage property page
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(FileProcessingDlgReportPage, CPropertyPage)
//-------------------------------------------------------------------------------------------------
FileProcessingDlgReportPage::FileProcessingDlgReportPage()
 : 
CPropertyPage(FileProcessingDlgReportPage::IDD),
m_bInitialized(false),
m_bExportLocal(false),
m_pRecordMgr(NULL),
m_pFPM(NULL),
m_lCautiousSnapshotCount(30),
m_dTotalCompletionRate(0), 
m_ipActionStatsOld(NULL)
{
	//{{AFX_DATA_INIT(FileProcessingDlgReportPage)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
FileProcessingDlgReportPage::~FileProcessingDlgReportPage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16527");
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgReportPage::PreTranslateMessage(MSG* pMsg) 
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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15269")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(FileProcessingDlgReportPage)
	DDX_Control(pDX, IDC_LIST_GLOBAL_STATS, m_listGlobalStats);
	DDX_Control(pDX, IDC_LIST_LOCAL_STATS, m_listLocalStats);
	DDX_Control(pDX, IDC_BTN_EXPORT, m_btnExport);
	DDX_Control(pDX, IDC_INTERPRET_CAUTIOUSLY_HELP, m_editCautiously);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlgReportPage, CPropertyPage)
	//{{AFX_MSG_MAP(FileProcessingDlgReportPage)
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_BTN_EXPORT, &FileProcessingDlgReportPage::OnBnClickedBtnExport)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgReportPage message handlers
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::OnSize(UINT nType, int cx, int cy) 
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

		// Set the size for the list, list columns, and arrange the export button
		sizeList();
	}

	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10085");
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgReportPage::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
	
	CPropertyPage::OnInitDialog();
	try
	{
		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(this->GetSafeHwnd()));

		// set no delay.
		m_infoTip.SetShowDelay(0);

		// set the list control style
		m_listGlobalStats.SetExtendedStyle( LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		m_listLocalStats.SetExtendedStyle( LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);

		// need to keep the column headers [p13 #4886]
		//m_listGlobalStats.ModifyStyle(0, LVS_NOCOLUMNHEADER);
		//m_listLocalStats.ModifyStyle(0, LVS_NOCOLUMNHEADER);
		
		// need to resize lists to accomodate the headers [p13 #4886]
		resizeListForHeader();

		// Fill the left-most column with labels
		fillLabelColumn();		

		// Verify that the config manager is initialized
		ASSERT_RESOURCE_ALLOCATION( "ELI15689", m_pCfgMgr != __nullptr );

		// Add text to Interpret Cautiously help and initially hide the control
		m_editCautiously.SetWindowTextA( 
			"*Interpret cautiously can be prompted by a number of situations.\r\n"
			"Some of these situations indicate processing failure while others do\r\n"
			"not. Below are some sample causes.\r\n\r\n" 
			"Situations that do not indicate processing failure (i.e. harmless situations)\r\n"
			" - Processing of large files\r\n"
			" - Listening has not added new files recently\r\n\r\n"
			"Situations that indicate processing failure\r\n"
			" - Database lock needs to be reset\r\n"
			" - Processing has failed or stopped for some reason\r\n\r\n"
			"If there is no status change for extended amounts of time,\r\n"
			"ensure that all processing machines are working as expected.\r\n");
		m_editCautiously.ShowWindow( SW_HIDE );

		// Determine how many snapshots must be reviewed before concluding 
		// that statistics may need to be interpreted cautiously
		long lTickTimeMS = m_pCfgMgr->getTimerTickSpeed();
		m_lCautiousSnapshotCount = giINTERPRET_STATS_DELAY * 1000 / lTickTimeMS;

		// Disable the Local Stats until there are stats to put into it
		m_listLocalStats.EnableWindow(false);

		CRect	rectProp;
		GetParent()->GetWindowRect( &rectProp );
		ScreenToClient( &rectProp );

		int iListWidth = rectProp.Width() - (2* g_iPADDING_AROUND_CONTROLS );

		// Remove pixels to prevent a horizontal scrollbar issue (4 for column lines)
		iListWidth = iListWidth - (g_iLABEL_COL_WIDTH + 4 + (2 * g_iPADDING_AROUND_CONTROLS ));

		// Set up the Global stats ListCtrl
		m_listGlobalStats.InsertColumn(g_iCOLUMN_ONE, "", 0, g_iLABEL_COL_WIDTH);
		m_listGlobalStats.InsertColumn(g_iCOLUMN_TWO, "", 0, (iListWidth - g_iPERCENT_COL_WIDTH ) );
		m_listGlobalStats.InsertColumn(g_iCOLUMN_THREE, "", 0, g_iPERCENT_COL_WIDTH );

		// Set up the Local stats ListCtrl
		m_listLocalStats.InsertColumn(g_iCOLUMN_ONE, "", 0, g_iLABEL_COL_WIDTH);
		m_listLocalStats.InsertColumn(g_iCOLUMN_TWO, "", 0, (iListWidth - g_iPERCENT_COL_WIDTH ) );
		m_listLocalStats.InsertColumn(g_iCOLUMN_THREE, "", 0, g_iPERCENT_COL_WIDTH );

		// Set m_bInitialized to true so that 
		// next call to OnSize() will not be skipped
		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10086");
	
	UpdateData( FALSE );
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::OnBnClickedBtnExport()
{
	try
	{
		// Prompt user for the .xml file they want to export to
		const static string s_strAllFiles = "XML Files (*.xml)|*.xml"
												"|All Files (*.*)|*.*||";

		// bring open file dialog
		string strFileExtension(s_strAllFiles);		
		CFileDialog fileDlg(FALSE, 0, NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_OVERWRITEPROMPT,
			strFileExtension.c_str(), CWnd::FromHandle(m_hWnd));

		CString zXmlFilename = "";
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// if the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() != IDOK)
		{
			return;
		}

		// get the file name, make sure it ends with .xml
		zXmlFilename = fileDlg.GetPathName();
		if (zXmlFilename.Right( 4 ).CompareNoCase( ".xml" ) != 0)
		{
			// Append .xml to filename (P13 #3953)
			zXmlFilename.Append( ".xml" );
		}

		ofstream xmlOutFile;
		xmlOutFile.open(zXmlFilename);
		ASSERT_RESOURCE_ALLOCATION("ELI14080", xmlOutFile.is_open() );

		// The getTimeAsString() method  in cpputil returns seconds, 
		// and no am/pm, so we make our own.
		CTime currentTime = CTime::GetCurrentTime();

		// Format the time as HH:MM am/pm
		CString strTime = currentTime.Format("%I:%M %p");

		std::string strActionName = "Error: Null FPM!";
		if( getFPM() == NULL)
		{
			UCLIDException ue("ELI14084", "Export Statistics: NULL File Processing Manager!");
			ue.display();
		}
		else
		{
			_bstr_t bstrActionName = getFPM()->GetActionName();
			strActionName = bstrActionName;
		}

		// Write out to an XML document. 
		xmlOutFile << "<FPMStatistics>\n";

		// Export the general user info
		xmlOutFile << "  <General>\n";
		xmlOutFile << "    <Date> " + ::getDateAsString() + " </Date>\n";
		xmlOutFile << "    <Time> " + strTime + " </Time>\n";
		xmlOutFile << "    <Machine> " + ::getComputerName() + " </Machine>\n";
		xmlOutFile << "    <User> " + ::getCurrentUserName() + " </User>\n";
		xmlOutFile << "    <Action> " + strActionName + " </Action>\n";
		xmlOutFile << "  </General>\n";
		xmlOutFile << "  <Stats>\n";

		// Export the global stats
		xmlOutFile << "    <Global>\n";
		xmlOutFile << "      <BytesProcessed> " + 
								m_listGlobalStats.GetItemText(g_iGLOBAL_BYTES_PROC_ROW, g_iCOLUMN_TWO)
								+ " ( " + m_listGlobalStats.GetItemText(g_iGLOBAL_BYTES_PROC_ROW, g_iCOLUMN_THREE)
								+ ") " + " </BytesProcessed>\n";
		xmlOutFile << "      <DocsProcessed> " + 
								m_listGlobalStats.GetItemText(g_iGLOBAL_DOCS_PROC_ROW , g_iCOLUMN_TWO)
								+ " ( " + m_listGlobalStats.GetItemText(g_iGLOBAL_DOCS_PROC_ROW , g_iCOLUMN_THREE)
								+ ") " + " </DocsProcessed>\n";
		xmlOutFile << "      <PagesProcessed> " + 
								m_listGlobalStats.GetItemText(g_iGLOBAL_PAGES_PROC_ROW, g_iCOLUMN_TWO)
								+ " ( " + m_listGlobalStats.GetItemText(g_iGLOBAL_PAGES_PROC_ROW, g_iCOLUMN_THREE)
								+ ") " + " </PagesProcessed>\n";
		xmlOutFile << "      <EstimatedTimeRemaining> " + 
								m_listGlobalStats.GetItemText(g_iGLOBAL_EST_TIME_REM_ROW , g_iCOLUMN_TWO)
								+ " </EstimatedTimeRemaining>\n";
		xmlOutFile << "      <EstimatedCompletionTime> " + 
								m_listGlobalStats.GetItemText(g_iGLOBAL_EST_COMP_TIME_ROW , g_iCOLUMN_TWO)
								+ " </EstimatedCompletionTime>\n";
		xmlOutFile << "    </Global>\n";

		// If local stats are enabled and populated with something useful, add them to the XML file
		if( m_bExportLocal )
		{
			xmlOutFile << "    <Local>\n";
			xmlOutFile << "      <BytesProcessed> " + 
									m_listLocalStats.GetItemText(g_iLOCAL_BYTES_PROC_ROW, g_iCOLUMN_TWO)
									+ " ( " + m_listLocalStats.GetItemText(g_iLOCAL_BYTES_PROC_ROW, g_iCOLUMN_THREE)
									+ ") " + " </BytesProcessed>\n";
			xmlOutFile << "      <DocsProcessed> " + 
									m_listLocalStats.GetItemText(g_iLOCAL_DOCS_PROC_ROW , g_iCOLUMN_TWO)
									+ " ( " + m_listLocalStats.GetItemText(g_iLOCAL_DOCS_PROC_ROW , g_iCOLUMN_THREE)
									+ ") " + " </DocsProcessed>\n";
			xmlOutFile << "      <PagesProcessed> " + 
									m_listLocalStats.GetItemText(g_iLOCAL_PAGES_PROC_ROW, g_iCOLUMN_TWO)
									+ " ( " + m_listLocalStats.GetItemText(g_iLOCAL_PAGES_PROC_ROW, g_iCOLUMN_THREE)
									+ ") " + " </PagesProcessed>\n";
			xmlOutFile << "      <TotalProcessingTime> " + 
									m_listLocalStats.GetItemText(g_iLOCAL_TOTAL_PROC_TIME_ROW, g_iCOLUMN_TWO)
									+ " </TotalProcessingTime>\n";
			xmlOutFile << "      <TotalRunTime> " + 
									m_listLocalStats.GetItemText(g_iLOCAL_TOTAL_RUN_TIME_ROW, g_iCOLUMN_TWO)
									+ " </TotalRunTime>\n";
			xmlOutFile << "    </Local>\n";
		}// End local stats

		xmlOutFile << "  </Stats>\n";
		xmlOutFile << "</FPMStatistics>\n";

		// Close the file
		xmlOutFile.close();
		waitForFileToBeReadable((LPCTSTR)zXmlFilename);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14079");
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setRecordManager(FPRecordManager* pRecordMgr)
{
	
	ASSERT_ARGUMENT("ELI10145", pRecordMgr != __nullptr);
	m_pRecordMgr = pRecordMgr;
	
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::clear()
{
	if( m_bInitialized )
	{
		// Clear the list
		m_listGlobalStats.DeleteAllItems();
		m_listLocalStats.DeleteAllItems();

		// clear other variables
		m_ipActionStatsOld = __nullptr;
		m_TimeOfLastProcess = CTime::GetCurrentTime();
		m_vecSnapshots.clear();
		m_vecCompletionRates.clear();
		m_dTotalCompletionRate = 0;

		// Populate the labels in the first column
		fillLabelColumn();
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setConfigMgr(FileProcessingConfigMgr *pCfgMgr)
{
	m_pCfgMgr = pCfgMgr;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::ResetInitialized()
{
	m_bInitialized = false;

	// Destroy m_infoTip object [P13: 4193]
	// ensure it exists before destroying [P13: 4682]
	if (asCppBool(::IsWindow(m_infoTip.m_hWnd)))
	{
		m_infoTip.DestroyWindow();
	}
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlgReportPage::getInit()
{
	return m_bInitialized;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr FileProcessingDlgReportPage::getFPM()
{
	return UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr( m_pFPM );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setFPM( UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPM)
{
	ASSERT_ARGUMENT( "ELI14355",pFPM != __nullptr);
	m_pFPM = pFPM;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::populateStats(StopWatch sWatch,
						UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStatsNew,
						const LONGLONG nTotalBytes, const long nTotalDocs, 
						const long nTotalPages, const unsigned long nTotalProcTime )
{
	try 
	{
		// Make sure the ipActionStatsNew is valid
		ASSERT_ARGUMENT("ELI15061", ipActionStatsNew != __nullptr );

		if(! getInit() )
		{
			// If the stats page is not initialized, dont update anything
			return;
		}

		// Add the new snapshot to the vector of snapshots
		addSnapshotToVector( ipActionStatsNew );

		// Update the Global stats ListCtrl if the Stats have changed or this is the first
		// time thru
		if( m_ipActionStatsOld == __nullptr || setTimeRemainingRows() )
		{
			// Update the "Bytes Processed" row
			LONGLONG nBytes = ipActionStatsNew->GetNumBytes();
			LONGLONG nBytesCompleted = ipActionStatsNew->GetNumBytesComplete();
			LONGLONG nBytesFailed = ipActionStatsNew->GetNumBytesFailed();

			setGlobalBytesProcRow(nBytes, nBytesCompleted, nBytesFailed);

			// Update the "Documents Processed" row
			long nDocs  = ipActionStatsNew->GetNumDocuments();
			long nDocsCompleted  = ipActionStatsNew->GetNumDocumentsComplete();
			long nDocsFailed = ipActionStatsNew->GetNumDocumentsFailed();

			setGlobalProcessedRow(nDocs, nDocsCompleted, nDocsFailed, g_iGLOBAL_DOCS_PROC_ROW);
			
			// Update the "Pages Processed" row
			long nPages = ipActionStatsNew->GetNumPages();
			long nPagesCompleted = ipActionStatsNew->GetNumPagesComplete();
			long nPagesFailed = ipActionStatsNew->GetNumPagesFailed();

			setGlobalProcessedRow(nPages, nPagesCompleted, nPagesFailed, g_iGLOBAL_PAGES_PROC_ROW);
		}

		// Now update the local stats if the ProcessingLog page has actually done anything
		// (this will be 0 if the ProcessingLog is disabled)
		if( nTotalProcTime > 0 || ipActionStatsNew->GetNumBytes() > 0 )
		{
			// If the local process has processed something, update the local stats
			if( nTotalProcTime > 0 )
			{
				// Now the export button will export local stats as well
				m_bExportLocal = true;
				
				// Make sure the local window is enabled
				m_listLocalStats.EnableWindow(true);

				// Set the "Bytes Processed" row
				setLocalBytesProcRow( nTotalBytes, nTotalProcTime );

				// Update the Pages Processed
				setLocalPagesProcessedRow(nTotalPages, nTotalProcTime );

				// Update the Docs Processed
				setLocalDocsProcessedRow(nTotalDocs, nTotalProcTime );
				
				// Update the Estimated Completion Time
				setTotalProcTimeRow(nTotalProcTime);
			}
			
			// For either case, update the "Total Run Time" row
			setTotalRunTimeRow(sWatch);
		}
		else
		{
			// If the Total Processing time is 0, then the processing log is not enabled, so 
			// there is no work being done locally. There is no reason to keep track of the local
			// statistics, so disable the window.
			m_listLocalStats.EnableWindow(false);
		}

		// Set the old action statistics pointer
		m_ipActionStatsOld = ipActionStatsNew;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS( "ELI14351" );
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setLocalDocsProcessedRow(const long nTotalDocs, 
														   const unsigned long nTotalProcTime )
{
	try
	{
		// Set the CString for the total document count
		CString zProcessed;
		zProcessed.Format("%s Documents", commaFormatNumber( (LONGLONG)nTotalDocs ).c_str());

		// Update the Docs Processed
		m_listLocalStats.SetItemText(g_iLOCAL_DOCS_PROC_ROW , g_iCOLUMN_TWO, zProcessed);

		// Prevent Division by 0
		if( nTotalProcTime > 0 )
		{
			// Find the # of documents per second and multiply by 3600 to get documents per hour
			double dDocsPerHour = (static_cast<double>(nTotalDocs) / static_cast<double>(nTotalProcTime))
				* 3600;
			// Format the double so we dont get partial documents. 
			zProcessed = (commaFormatNumber( dDocsPerHour, 0) + " / hour").c_str();
		}
		else
		{
			// If processing time is 0, the stats arent useful yet
			zProcessed = "0 / hour";
		}

		// Set the listbox for the number per hour
		m_listLocalStats.SetItemText(g_iLOCAL_DOCS_PROC_ROW , g_iCOLUMN_THREE, zProcessed );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14304");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setLocalPagesProcessedRow(const long nTotalPages,
															const unsigned long nTotalProcTime )
{
	try
	{
		// Set the CString for the number of pages completed
		CString zProcessed;
		zProcessed.Format("%s Pages" , commaFormatNumber( (LONGLONG)nTotalPages ).c_str());
		
		// Update the Pages Processed
		m_listLocalStats.SetItemText(g_iLOCAL_PAGES_PROC_ROW , g_iCOLUMN_TWO, zProcessed);

		// Find the # of pages per second and multiply by 3600 to get pages per hour
		if(nTotalProcTime > 0 )
		{
			double dPagesPerHour = (static_cast<double>(nTotalPages) / static_cast<double>(nTotalProcTime))
									* 3600;
			zProcessed = (commaFormatNumber( dPagesPerHour, 0) + " / hour").c_str();
		}
		else
		{
			zProcessed = "0 / hour";
		}

		// Set the listbox for the number per hour
		m_listLocalStats.SetItemText(g_iLOCAL_PAGES_PROC_ROW, g_iCOLUMN_THREE, zProcessed );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14289");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setLocalBytesProcRow(const LONGLONG nTotalBytes, 
													   const unsigned long nTotalProcTime )
{
	try
	{
		const double oneMB = 1024.0 * 1024.0;

		// Find the number of MBs complete and format it
		double dMBComplete = static_cast<double>(nTotalBytes) / oneMB;
		std::string strComplete = commaFormatNumber( dMBComplete, 2) + " MB";

		// Update the MBs Processed
		m_listLocalStats.SetItemText(g_iLOCAL_BYTES_PROC_ROW, g_iCOLUMN_TWO, strComplete.c_str());

		CString zProcessed;
		// Find the # of MB per second and multiply by 3600 to get MBs per hour
		// prevent division by 0
		if( nTotalProcTime > 0 )
		{
			double dBytesPerHour = (static_cast<double>(nTotalBytes) / static_cast<double>(nTotalProcTime))
									* 3600;
			dMBComplete = dBytesPerHour / oneMB;
			zProcessed = (commaFormatNumber( dMBComplete, 2) + " MB / hour").c_str();
		}
		else
		{
			zProcessed = "0 / hour";
		}

		// Set the listbox for the number per hour
		m_listLocalStats.SetItemText(g_iLOCAL_BYTES_PROC_ROW, g_iCOLUMN_THREE, zProcessed );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14290");
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlgReportPage::setTimeRemainingRows()
{
	INIT_EXCEPTION_AND_TRACING("MLI00042");
	try
	{
		// Check the current time
		CTime currentTime = CTime::GetCurrentTime();
		_lastCodePos = "10";

		// Get the old stats and the latest stats from the vector of snapshots
		ASSERT_RESOURCE_ALLOCATION("ELI15686", m_vecSnapshots.size() > 0 );
		Snapshot &rOldestSnapshotInVector = m_vecSnapshots[0];
		_lastCodePos = "20";
		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStatsOld = 
			rOldestSnapshotInVector.getActionStatistic();
		ASSERT_RESOURCE_ALLOCATION("ELI15059", ipActionStatsOld != __nullptr);

		Snapshot &rLatestSnapshot = m_vecSnapshots.back();
		_lastCodePos = "40";
		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStatsNew = 
			rLatestSnapshot.getActionStatistic();
		ASSERT_RESOURCE_ALLOCATION("ELI15350", ipActionStatsNew != __nullptr);

		// First find out how many bytes have been completed since from oldest snapshot until now
		LONGLONG nBytesDone = (ipActionStatsNew->NumBytesComplete + ipActionStatsNew->NumBytesFailed) 
			- (ipActionStatsOld->NumBytesComplete + ipActionStatsOld->NumBytesFailed);
		_lastCodePos = "50";
		
		// Make sure that the value is > 0. It's possible that something may happen to reset the DB
		// while processing is in progress. This could give us negative bytes processed.
		if(nBytesDone < 0 )
		{
			// Since we have negative work taking place, our starting stats have been skewed
			nBytesDone = 0;

			// Log an exception.
			UCLIDException ue("ELI15688", "Application Trace: Invalid number of bytes processed");
			//ue.addDebugInfo("Bytes Done", asString(nBytesDone) );
			ue.addDebugInfo("New BytesCompleted", asString( ipActionStatsNew->NumBytesComplete) );
			ue.addDebugInfo("New NumBytesFailed", asString( ipActionStatsNew->NumBytesFailed) );
			ue.addDebugInfo("Old BytesCompleted", asString( ipActionStatsOld->NumBytesComplete) );
			ue.addDebugInfo("Old NumBytesFailed", asString( ipActionStatsOld->NumBytesFailed) );
			ue.log();

			// Clear the data used to calculate statistics and start over.
			clear();
			_lastCodePos = "60";

			// Since the stats have been cleared, do not update anything.
			return false;
		}

		// Find how many seconds have passed since the oldest snapshot
		SYSTEMTIME stOldest, stCurrent;
		rOldestSnapshotInVector.getTime().GetAsSystemTime(stOldest);
		currentTime.GetAsSystemTime(stCurrent);
		TimeInterval interval(stOldest, stCurrent);
		unsigned long nSeconds = interval.getTotalSeconds();
		_lastCodePos = "70";
		
		double dBytesPerSecond = 0;
		// Prevent divide by 0 errors
		if( nSeconds > 0 )
		{
			// Find out how many bytes have been done per second
			dBytesPerSecond = ( static_cast<double>( nBytesDone ) / static_cast<double>( nSeconds ));
		}
		else
		{
			dBytesPerSecond = 0;
		}
		_lastCodePos = "80";
		
		// Add the new completion rate to the vector
		addNewCompletionRateToVector( dBytesPerSecond ); 

		// get the average completion rate
		double dAverageBytesPerSecond = 
			(m_dTotalCompletionRate / (static_cast<double> (m_vecCompletionRates.size())));

		// Figure out how many bytes are left
		LONGLONG nBytesLeft = ipActionStatsNew->NumBytes - (ipActionStatsNew->NumBytesComplete 
			+ ipActionStatsNew->NumBytesFailed);
		double dSecondsLeft = 0;
		_lastCodePos = "90";
				
		// Prevent divide by 0 error
		if( dAverageBytesPerSecond > 0 )
		{
			// Find how many seconds are left
			dSecondsLeft = ( static_cast<double>( nBytesLeft ) / dAverageBytesPerSecond );
		}
		else
		{
			dSecondsLeft = 0;
		}
		_lastCodePos = "100";
		
		// In debug mode, write information to a log file to help with analysis
		// of the time estimation algorithm.
#ifdef _DEBUG
		bool bAddHeader = false;

		if( !isFileOrFolderValid( getLogFileFullPath() ) )
		{
			bAddHeader = true;
		}
		ofstream estimatedTimeLogFile(getLogFileFullPath().c_str(), ios::app);		

		// Check that the statistics file was successfully opened [LRCAU #5111]
		if (estimatedTimeLogFile.is_open())
		{
			if ( bAddHeader )
			{
				estimatedTimeLogFile << "Minutes Left" << "," 
									 << "BytesPerSecond" << "," 
									 << "NumBytesComplete" << ","
									 << "TotalBytes" << ","
									 << "NumFilesComplete" << ","
									 << "TotalFiles" << ","
									 << "NumPagesComplete" << ","
									 << "TotalPages" << ","
									 << "Vector.Size" << endl;
			}

			estimatedTimeLogFile << dSecondsLeft / 60 << ","
								 << dBytesPerSecond << "," 
								 << ipActionStatsNew->NumBytesComplete << ","
								 << ipActionStatsNew->NumBytes << ","
								 << ipActionStatsNew->NumDocumentsComplete << ","
								 << ipActionStatsNew->NumDocuments << ","
								 << ipActionStatsNew->NumPagesComplete << ","
								 << ipActionStatsNew->NumPages << ","
								 << asString( m_vecSnapshots.size() ) << endl;

			estimatedTimeLogFile.close();
			waitForFileToBeReadable(getLogFileFullPath());
		}
#endif

		// Split the amount of time remaining
		long nDays, nHours, nMinutes, nSec = 0;
		splitTime(static_cast<long>(dSecondsLeft), nDays, nHours, nMinutes, nSec);
		_lastCodePos = "110";
								
		// Get the amount of time left
		CTimeSpan estTimeRemaining = CTimeSpan(nDays, nHours, nMinutes, nSec);
		_lastCodePos = "120";
		
		// Add the current time to the estimated time left, and we get the time of completion
		CTime estCompletionTime = currentTime + estTimeRemaining;
		_lastCodePos = "130";
				
		long lSecondsRemain = static_cast<long> (estTimeRemaining.GetTotalSeconds());

		splitTime( lSecondsRemain, nDays, nHours, nMinutes, nSec );
		_lastCodePos = "160";
								
		// Then set the estimated time remaining spot in the table if it is > 0 seconds from now.
		CString zEstTimeRemaining = "";
		bool bDisplayCompletionTime = false;
		_lastCodePos = "170";
		if( lSecondsRemain > 0 )
		{
			_lastCodePos = "180";
			zEstTimeRemaining = getFormattedTime(nDays, nHours, nMinutes, nSec, true);
			bDisplayCompletionTime = true;
			_lastCodePos = "190";
		}
		m_listGlobalStats.SetItemText(g_iGLOBAL_EST_TIME_REM_ROW, g_iCOLUMN_TWO, zEstTimeRemaining );
		_lastCodePos = "200";
		
		// Format the completionTime and put it in the table.
		CString zCompletionTime = "";
		_lastCodePos = "210";
		if ( (estCompletionTime != currentTime) && bDisplayCompletionTime)
		{
			_lastCodePos = "220";
			// If the completion time is not today, then display the day and the hour
			// with no minute resolution.
			if( estCompletionTime.Format("%A %B %d") != currentTime.Format("%A %B %d") )
			{	
				_lastCodePos = "230";
				zCompletionTime = estCompletionTime.Format("%A, %B %d at %#I %p ");
				_lastCodePos = "240";
			}
			else
			{
				//It completes today, just display the completion hour and minute.
				_lastCodePos = "250";
				zCompletionTime = estCompletionTime.Format("%#I:%M %p ");
				_lastCodePos = "260";
			}
		}
		_lastCodePos = "270";

		// Then update the list itself with the returned value
		m_listGlobalStats.SetItemText(g_iGLOBAL_EST_COMP_TIME_ROW, g_iCOLUMN_TWO, zCompletionTime );
		_lastCodePos = "280";

		// Look for enough snapshots to do the Interpret Cautiously check
		long lSnapshotCount = m_vecSnapshots.size();
		_lastCodePos = "290";
		if (lSnapshotCount >= m_lCautiousSnapshotCount)
		{
			Snapshot &rRecentSnapshot = m_vecSnapshots[lSnapshotCount - m_lCautiousSnapshotCount];
			_lastCodePos = "300";
			UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStatsRecent = 
				rRecentSnapshot.getActionStatistic();
			ASSERT_RESOURCE_ALLOCATION("ELI15690", ipActionStatsRecent != __nullptr);

			// Check if anything has changed between the recent and the last snapshots 
			// in the vector. If no work has been done, add a note to inform the user. Then
			// check if any additional data has been added to the queue.
			_lastCodePos = "310";
			if ( ipActionStatsRecent->NumBytesComplete == ipActionStatsNew->NumBytesComplete )
			{
				// Get the amount of time that has passed since reliable processing has taken place
				SYSTEMTIME stTimeOfLastProcess, stCurrentTime;
				m_TimeOfLastProcess.GetAsSystemTime(stTimeOfLastProcess);
				currentTime.GetAsSystemTime(stCurrentTime);
				TimeInterval interval(stTimeOfLastProcess, stCurrentTime);
				_lastCodePos = "320";

				// Split the time into a meaningful format
				long nDays, nHours, nMins, nSecs;
				splitTime( interval.getTotalSeconds() + giINTERPRET_STATS_DELAY, nDays, nHours, nMins, nSecs);
				_lastCodePos = "330";

				// If no work has been detected, display the label.
				CString zLabelText= "";
				zLabelText.Format("No files completed processing in the last %s", 
					getFormattedTime(nDays, nHours, nMins, nSecs, true) );
				_lastCodePos = "340";

				// Add a static label for the "No processing detected in XYZ seconds"
				SetDlgItemText( IDC_STATIC_INTERPRET_CAUTIOUSLY, zLabelText );
				_lastCodePos = "350";

				if ( ipActionStatsRecent->NumBytes == ipActionStatsNew->NumBytes &&
					ipActionStatsRecent->NumBytesFailed == ipActionStatsNew->NumBytesFailed &&
					bDisplayCompletionTime)
				{
					// Special case: If the stats between the recent and the last snapshots 
					// have not changed, add an entry to the global stats estimated time 
					// remaining column telling the user to 'interpret stats carefully.' This 
					// means that no processing has been detected in the last X seconds and 
					// the user should take that into account.
					
					// Set the message in the global stats list box
					m_listGlobalStats.SetItemText(g_iGLOBAL_EST_TIME_REM_ROW, g_iCOLUMN_THREE, 
						gstrINTERPRET_CAUTIOUSLY_LABEL.c_str());
					_lastCodePos = "360";

					// Show the interpret cautiously explanation
					m_editCautiously.ShowWindow( SW_SHOW );
					_lastCodePos = "370";
				}
			}
			else
			{
				// If work has been done, remove the 'interpret cautiously' message 
				// and clear the static label comment
				m_listGlobalStats.SetItemText(g_iGLOBAL_EST_TIME_REM_ROW, g_iCOLUMN_THREE, "");
				_lastCodePos = "380";
				
				// Clear the static label
				SetDlgItemText( IDC_STATIC_INTERPRET_CAUTIOUSLY, "" );
				_lastCodePos = "390";

				// Hide the interpret cautiously explanation
				m_editCautiously.ShowWindow( SW_HIDE );
				_lastCodePos = "400";

				// Set the time of the last process
				m_TimeOfLastProcess = currentTime;
			}
		}
		// Not enough snapshots available yet, hide the items
		else
		{
			m_listGlobalStats.SetItemText(g_iGLOBAL_EST_TIME_REM_ROW, g_iCOLUMN_THREE, "");
			_lastCodePos = "410";
			SetDlgItemText( IDC_STATIC_INTERPRET_CAUTIOUSLY, "" );
			_lastCodePos = "420";
			m_editCautiously.ShowWindow( SW_HIDE );
		}

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS( "ELI14352" );

	// Requires a return here to prevent warnings about not all paths returning a value
	// should only reach this point if an exception was thrown
	return false;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setTotalRunTimeRow(StopWatch sWatch )
{
	try
	{
		// Check the duration of the timer
		double dElapsedTime = sWatch.getElapsedTime();

		// Split the elapsed time into Days, hours, etc...
		long nDays, nHours, nMinutes, nSeconds = 0;
		splitTime( static_cast<long>(dElapsedTime), nDays, nHours, nMinutes, nSeconds);

		// Format the time
		CString zTotalRunTime = getFormattedTime(nDays, nHours, nMinutes, nSeconds, true);

		// Update the "Total Run Time" row
		m_listLocalStats.SetItemText(g_iLOCAL_TOTAL_RUN_TIME_ROW , g_iCOLUMN_TWO, zTotalRunTime );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS( "ELI14353" );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setTotalProcTimeRow( const unsigned long nTotalProcTime)
{
	try
	{
		// Split time requires a long
		long nProcTime = static_cast<long>(nTotalProcTime);

		// Split the amount of processing time
		long nDays, nHours, nMinutes, nSeconds = 0;
		splitTime(nProcTime, nDays, nHours, nMinutes, nSeconds);

		CString zProcTime = getFormattedTime(nDays, nHours, nMinutes, nSeconds, true);

		m_listLocalStats.SetItemText(g_iLOCAL_TOTAL_PROC_TIME_ROW , g_iCOLUMN_TWO, zProcTime );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS( "ELI14354" );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setGlobalProcessedRow(const long nTotal,const  long nCompleted, 
														const long nFailed, const int ROWNUM)
{
	try
	{
		// Calculate the total amount done
		long nDone = nCompleted + nFailed;

		// Set the xx / xx column
		CString zDocSlashCol;
		zDocSlashCol.Format("%s / %s", 
			commaFormatNumber( static_cast<double>(nDone) , 0).c_str(),
			commaFormatNumber( static_cast<double>(nTotal), 0).c_str());

		m_listGlobalStats.SetItemText(ROWNUM, g_iCOLUMN_TWO, zDocSlashCol);

		// Set the xx.xx% column
		CString zDocsPercentCol;
		// Prevent divide by zero error
		if( nTotal > 0 )
		{
			zDocsPercentCol.Format("%s", 
				commaFormatNumber(  static_cast<double>(nDone) 
				/ static_cast<double>(nTotal) *100.0, 2).c_str());
		}
		else
		{
			zDocsPercentCol = "0.00";
		}

		// Then update the list to the found value
		m_listGlobalStats.SetItemText(ROWNUM, g_iCOLUMN_THREE, 
								LPCSTR(zDocsPercentCol + '%'));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14076");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::setGlobalBytesProcRow(const LONGLONG nBytes, 
													  const LONGLONG nBytesCom, 
													  const LONGLONG nBytesFailed)
{
	try
	{
		LONGLONG nBytesDone = nBytesCom + nBytesFailed;
		
		const double oneMB = 1024.0 * 1024.0;
		// Find the "xx.xx MB / xx.xx MB" value for the second column
		CString zBytesSlashCol;
		zBytesSlashCol.Format("%s MB / %s MB",
			commaFormatNumber( nBytesDone / oneMB, 2).c_str(), 
			commaFormatNumber( nBytes / oneMB, 2).c_str() );

		// Set the global statistics table first row, second column to the found value
		m_listGlobalStats.SetItemText(g_iGLOBAL_BYTES_PROC_ROW, g_iCOLUMN_TWO, 
									LPCSTR(zBytesSlashCol) );
		
		// Find the xx.xx% value for the third column
		CString zBytesPercentCol;
		// Prevent divide by zero error
		if(nBytes > 0)
		{
			zBytesPercentCol.Format("%s", 
					commaFormatNumber( static_cast<double>(nBytesDone)/
									static_cast<double>(nBytes)* 100.0, 2 ).c_str() );
		}
		else
		{
			zBytesPercentCol = "0.00";
		}

		// Set the global statistics table first row, third column to the found value
		// Note: % sign must be added here to prevent an assert error and a warning about
		//		 attempting to escape with a '\'
		m_listGlobalStats.SetItemText(g_iGLOBAL_BYTES_PROC_ROW, g_iCOLUMN_THREE, 
									LPCSTR(zBytesPercentCol + '%') );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14075");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::splitTime(long nTime, long& nDays, long& nHours, long& nMins, long& nSecs)
{
	nDays = nTime / (24*3600);
	nTime -= (nDays * 24 * 3600);
	nHours = nTime / 3600;
	nTime -= (nHours * 3600);
	nMins = nTime / 60;
	nTime -= (nMins * 60);
	nSecs = nTime;
}
//-------------------------------------------------------------------------------------------------
CString FileProcessingDlgReportPage::getFormattedTime(const long nDays, const long nHours, 
													  const long nMins, const long nSecs, 
													  const bool bDisplayMinRes)
{
	CString zDays = nDays == 1 ? "day" : "days";
	CString zHours = nHours == 1 ? "hour" : "hours";
	CString zMins = nMins == 1 ? "min" : "mins";
	CString zSecs = nSecs == 1 ? "sec" : "secs";

	CString zTime;
	if (nDays > 0)
	{
		if (!bDisplayMinRes)
		{
			zTime.Format("%d %s %d %s %d %s %d %s", nDays, zDays, nHours, zHours, nMins, zMins, nSecs, zSecs);
		}
		else
		{
			zTime.Format("%d %s %d %s", nDays, zDays, nHours, zHours);
		}
	}
	else if (nHours > 0)
	{
		if (!bDisplayMinRes)
		{
			zTime.Format("%d %s %d %s %d %s", nHours, zHours, nMins, zMins, nSecs, zSecs);
		}
		else
		{
			zTime.Format("%d %s %d %s", nHours, zHours, nMins, zMins);
		}
	}
	else if (nMins > 0)
	{
		zTime.Format("%d %s %d %s", nMins, zMins, nSecs, zSecs);
	}
	else if (nSecs > 0)
	{
		zTime.Format("%d %s", nSecs, zSecs);
	}

	return zTime;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::fillLabelColumn()
{
	try
	{
		// Set up the left-most column for the global stats
		m_listGlobalStats.InsertItem(g_iGLOBAL_BYTES_PROC_ROW, "Bytes Processed:");
		m_listGlobalStats.InsertItem(g_iGLOBAL_PAGES_PROC_ROW, "Pages Processed:");
		m_listGlobalStats.InsertItem(g_iGLOBAL_DOCS_PROC_ROW, "Documents Processed:");
		m_listGlobalStats.InsertItem(g_iGLOBAL_EST_TIME_REM_ROW, "Estimated Time Remaining:");
		m_listGlobalStats.InsertItem(g_iGLOBAL_EST_COMP_TIME_ROW, "Estimated Completion Time:");

		// Set up the left-most column for the local stats
		m_listLocalStats.InsertItem(g_iLOCAL_BYTES_PROC_ROW, "Bytes Processed:");
		m_listLocalStats.InsertItem(g_iLOCAL_PAGES_PROC_ROW, "Pages Processed:");
		m_listLocalStats.InsertItem(g_iLOCAL_DOCS_PROC_ROW, "Documents Processed:");
		m_listLocalStats.InsertItem(g_iLOCAL_TOTAL_PROC_TIME_ROW, "Total Processing Time:");
		m_listLocalStats.InsertItem(g_iLOCAL_TOTAL_RUN_TIME_ROW, "Total Run Time:");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14127");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::sizeList()
{
	try
	{
		// suspend window updates to prevent flickering
		SuspendWindowUpdates swuTemp(*this);

		CRect rectProp;
		GetParent()->GetWindowRect( &rectProp );
		ScreenToClient( &rectProp );

		// Get the rectangles for each control
		CRect rectGlobalList, rectLocalList, rectLocalLabel, rectStaticLabel, rectExport, rectHelp;
		m_listGlobalStats.GetWindowRect( &rectGlobalList );
		m_listLocalStats.GetWindowRect( &rectLocalList );
		GetDlgItem(IDC_STATIC_STATS_THIS_RUN)->GetWindowRect(&rectLocalLabel);
		GetDlgItem( IDC_STATIC_INTERPRET_CAUTIOUSLY )->GetWindowRect( &rectStaticLabel );
		GetDlgItem( IDC_BTN_EXPORT )->GetWindowRect( rectExport );
		GetDlgItem( IDC_INTERPRET_CAUTIOUSLY_HELP )->GetWindowRect( &rectHelp );

		// Translate coordinates from screen to client
		ScreenToClient( &rectGlobalList );
		ScreenToClient( &rectLocalList );
		ScreenToClient( &rectLocalLabel );
		ScreenToClient( &rectStaticLabel );
		ScreenToClient( &rectExport );
		ScreenToClient( &rectHelp );
		
		// LRCAU #4908, #4922 - Change to programmatically compute the
		// header and row heights and adjust the list sizes accordingly

		// Get the height of the header row (this should be the same for both lists)
		CRect rectHeader;
		m_listGlobalStats.GetHeaderCtrl()->GetWindowRect(&rectHeader);
		ScreenToClient(&rectHeader);
		int iHeaderHeight = rectHeader.Height() + 4; // 4 to account for border

		// Get the height of a row (this should be the same for both lists)
		// Get the y position of row 1 and row 2 and substract to compute
		// the height of row 1
		// NOTE: This computation assumes that we have at least two rows in the
		// grid.  If in the future we change the grid and it does not have at
		// least 2 rows at this point, this code will break
		POINT ptRowA, ptRowB;
		m_listGlobalStats.GetItemPosition(0, &ptRowA);
		m_listGlobalStats.GetItemPosition(1, &ptRowB);
		ScreenToClient(&ptRowA);
		ScreenToClient(&ptRowB);
		int iRowHeight = ptRowB.y - ptRowA.y;

		// Move the global list
		// Align the left/right of the global list
		// Add the 2*Padding to account for the gap between the property page and the 
		// dialog window in addition to the padding itself.
		// Also, compute the height change (store the height, modify the list and then
		// subtract to get height change).
		int iGlobalHeightChange = rectGlobalList.Height();
		rectGlobalList.left = rectProp.left + 2* g_iPADDING_AROUND_CONTROLS;
		rectGlobalList.right = rectProp.right - (2* g_iPADDING_AROUND_CONTROLS );
		rectGlobalList.bottom = rectGlobalList.top + iHeaderHeight + (iRowHeight * 5);
		iGlobalHeightChange = rectGlobalList.Height() - iGlobalHeightChange;

		// Move the label over the local list
		rectLocalLabel.top += iGlobalHeightChange;
		rectLocalLabel.bottom += iGlobalHeightChange;

		// Move the local list
		// Align the left/right of the local list
		// Add the 2*Padding to account for the gap between the property page and the 
		// dialog window in addition to the padding itself.
		rectLocalList.left = rectProp.left + 2* g_iPADDING_AROUND_CONTROLS;
		rectLocalList.right = rectProp.right - (2* g_iPADDING_AROUND_CONTROLS );
		rectLocalList.top += iGlobalHeightChange;
		rectLocalList.bottom = rectLocalList.top + iHeaderHeight + (iRowHeight * 5);
		
		// Set the static text label's position
		int iLabelHeight = rectStaticLabel.Height();
		int iLabelWidth = rectStaticLabel.Width();
		rectStaticLabel.right = rectLocalList.right;
		rectStaticLabel.left = rectLocalList.right - iLabelWidth;
		rectStaticLabel.bottom = rectLocalList.top - 5;
		rectStaticLabel.top = rectStaticLabel.bottom - iLabelHeight;

		// Set up the columns
		// The -4 is for lost pixels due to column lines
		int iStatsColWidth = (rectGlobalList.Width() - g_iLABEL_COL_WIDTH - g_iPERCENT_COL_WIDTH - 4);
		m_listGlobalStats.SetColumnWidth( g_iCOLUMN_ONE, g_iLABEL_COL_WIDTH );
		m_listGlobalStats.SetColumnWidth( g_iCOLUMN_TWO, iStatsColWidth );
		m_listGlobalStats.SetColumnWidth( g_iCOLUMN_THREE, g_iPERCENT_COL_WIDTH );

		iStatsColWidth = (rectLocalList.Width() - g_iLABEL_COL_WIDTH - g_iTHROUGHPUT_COL_WIDTH - 4);
		m_listLocalStats.SetColumnWidth( g_iCOLUMN_ONE, g_iLABEL_COL_WIDTH );
		m_listLocalStats.SetColumnWidth( g_iCOLUMN_TWO, iStatsColWidth );
		m_listLocalStats.SetColumnWidth( g_iCOLUMN_THREE, g_iTHROUGHPUT_COL_WIDTH );

		// set up the export button's position based on the LocalStats list
		int height = rectExport.Height();
		rectExport.right = rectLocalList.right;
		rectExport.left = rectExport.right - 100;
		rectExport.top = rectLocalList.bottom + g_iPADDING_AROUND_CONTROLS;
		rectExport.bottom = rectExport.top + height;

		// Set the help text position
		rectHelp.left = rectGlobalList.left;
		rectHelp.right = rectExport.left - 10;
		rectHelp.top = rectExport.top;
		rectHelp.bottom = rectProp.bottom - (g_iPADDING_AROUND_CONTROLS * 2);


		// Move the controls
		GetDlgItem( IDC_INTERPRET_CAUTIOUSLY_HELP )->MoveWindow( rectHelp );
		GetDlgItem( IDC_STATIC_INTERPRET_CAUTIOUSLY )->MoveWindow( rectStaticLabel );
		GetDlgItem(IDC_STATIC_STATS_THIS_RUN)->MoveWindow( rectLocalLabel );
		m_btnExport.MoveWindow( rectExport );
		m_listGlobalStats.MoveWindow( rectGlobalList );
		m_listLocalStats.MoveWindow( rectLocalList );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14128");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::addSnapshotToVector(UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStatsNew)
{
	// Get the current time for this snapshot
	CTime theTime = CTime::GetCurrentTime();

	// Create a snapshot from the current time and the supplied statistics
	Snapshot currentSnapshot(theTime, ipActionStatsNew);
	m_vecSnapshots.push_back( currentSnapshot );

	// If this makes the vector larger than the maximum allowed size, remove the front snapshot.
	if( m_vecSnapshots.size() > g_iMAX_SNAPSHOTS_IN_VECTOR )
	{
		// Remove the front snapshot
		m_vecSnapshots.erase( m_vecSnapshots.begin() );
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::addNewCompletionRateToVector(double dNewCompletionRate)
{
	// Add this rate to the total rate
	m_dTotalCompletionRate += dNewCompletionRate;
	m_vecCompletionRates.push_back( dNewCompletionRate );

	// If this makes the vector larger than the maximum allowed size, remove the front snapshot.
	if( m_vecCompletionRates.size() > g_iMAX_COMPLETION_RATES_IN_VECTOR )
	{
		// Update the total rate and remove the front snapshot
		m_dTotalCompletionRate -= m_vecCompletionRates[0];
		m_vecCompletionRates.erase( m_vecCompletionRates.begin() );
	}
}
//-------------------------------------------------------------------------------------------------
const string& FileProcessingDlgReportPage::getLogFileFullPath()
{
	static string ls_strLogFileName;

	// If the log file has not yet been found, calculate it
	if (ls_strLogFileName.empty())
	{
		// Calculate the path to the LogFiles folder
		string strDir1 = getExtractApplicationDataPath() + "\\LogFiles";

		// Check if the folder exists and create if necessary [LRCAU #5111]
		if (!isValidFolder(strDir1))
		{
			createDirectory(strDir1, true);
		}

		ls_strLogFileName = strDir1 + "\\Statistics.csv";	
	}

	return ls_strLogFileName;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgReportPage::resizeListForHeader()
{
	try
	{
		// get the header height (since both lists are the same the header will be the same)
		long lHeaderHeight(0), lDoubleHeight(0);

		CHeaderCtrl* pHeader = m_listGlobalStats.GetHeaderCtrl();
		if (pHeader != __nullptr)
		{
			// get the rectangle for the header
			CRect rectHeader;
			pHeader->GetWindowRect(&rectHeader);

			// get the height of the header control
			lHeaderHeight = rectHeader.Height();

			// compute twice the height
			lDoubleHeight = 2 * lHeaderHeight;
		}

		// get each list controls dimensions
		RECT rectGlobalList, rectLocalList;
		m_listGlobalStats.GetWindowRect(&rectGlobalList);
		m_listLocalStats.GetWindowRect(&rectLocalList);

		ScreenToClient(&rectGlobalList);
		ScreenToClient(&rectLocalList);

		// resize the global stats
		rectGlobalList.bottom = rectGlobalList.bottom + lHeaderHeight;

		// resize the local stats
		rectLocalList.top = rectLocalList.top + lHeaderHeight;
		rectLocalList.bottom = rectLocalList.bottom + lDoubleHeight;

		// Set the static stats text label's position
		RECT rectStaticStats;
		GetDlgItem( IDC_STATIC_STATS_THIS_RUN )->GetWindowRect( &rectStaticStats );
		ScreenToClient(&rectStaticStats);
		rectStaticStats.top = rectStaticStats.top + lHeaderHeight;
		rectStaticStats.bottom = rectStaticStats.bottom + lHeaderHeight;

		// Set the static cautious text label's position
		RECT rectStaticCautiousLabel;
		GetDlgItem( IDC_STATIC_INTERPRET_CAUTIOUSLY )->GetWindowRect( &rectStaticCautiousLabel );
		ScreenToClient( &rectStaticCautiousLabel );
		rectStaticCautiousLabel.top = rectStaticCautiousLabel.top + lHeaderHeight;
		rectStaticCautiousLabel.bottom = rectStaticCautiousLabel.bottom + lHeaderHeight;

		// move the export button based on the expansion of the list
		RECT rectExport;
		m_btnExport.GetWindowRect(&rectExport);
		ScreenToClient( &rectExport );
		rectExport.top = rectExport.top + lDoubleHeight;
		rectExport.bottom = rectExport.bottom + lDoubleHeight;

		// Set the help text position
		RECT rectHelp;
		GetDlgItem( IDC_INTERPRET_CAUTIOUSLY_HELP )->GetWindowRect( &rectHelp );
		ScreenToClient( &rectHelp );
		rectHelp.top = rectHelp.top + lDoubleHeight;
		rectHelp.bottom = rectHelp.bottom + lDoubleHeight;

		// now move each of the controls to their new positions
		GetDlgItem( IDC_INTERPRET_CAUTIOUSLY_HELP )->MoveWindow( &rectHelp );
		GetDlgItem( IDC_STATIC_INTERPRET_CAUTIOUSLY )->MoveWindow( &rectStaticCautiousLabel );
		GetDlgItem( IDC_STATIC_STATS_THIS_RUN )->MoveWindow( &rectStaticStats );
		m_btnExport.MoveWindow( &rectExport );
		m_listGlobalStats.MoveWindow( &rectGlobalList );
		m_listLocalStats.MoveWindow( &rectLocalList );
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20438");
}
//-------------------------------------------------------------------------------------------------