// SpatialStringViewerDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SpatialStringViewer.h"

#include "SpatialStringViewerDlg.h"
#include "SpatialStringViewerCfg.h"
#include "FindRegExDlg.h"
#include "FontSizeDistributionDlg.h"
#include "USSPropertyDlg.h"
#include "CharInfoDlg.h"
#include "WordLengthDistributionDlg.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <LoadFileDlgThread.h>
#include <TemporaryResourceOverride.h>
#include <COMUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// (P16 #2629) - Change title from "Extract Systems Spatial String (USS) Viewer"
const string strWINDOW_TITLE = "USS File Viewer";

static const int g_nStatusBarHeight = 18;

// font constants
const char gcBLANK = '_';
const char gcITALIC = 'I';
const char gcBOLD = 'D';
const char gcSANSERIF = 'N';
const char gcSERIF = 'S';
const char gcPROPORTIONAL = 'O';
const char gcUNDERLINE = 'U';
const char gcSUPERSCRIPT = 'P';
const char gcSUBSCRIPT = 'B';
//-------------------------------------------------------------------------------------------------

static UINT indicators[] =
{
	//	ID_SEPARATOR,           // status line indicator
	ID_INDICATOR_PAGE,
	ID_INDICATOR_START,
	ID_INDICATOR_END,
	ID_INDICATOR_PERCENT
};

//-------------------------------------------------------------------------------------------------
// CAboutDlg dialog used for App About
//-------------------------------------------------------------------------------------------------
class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

	// Dialog Data
	//{{AFX_DATA(CAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAboutDlg)
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

	// Implementation
protected:
	//{{AFX_MSG(CAboutDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
//-------------------------------------------------------------------------------------------------
CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CAboutDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CAboutDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		CDialog::OnInitDialog();

		// Set the caption
		string strCaption = "About USS File Viewer";
		SetWindowText( strCaption.c_str() );

		// Get module path and filename
		char zFileName[MAX_PATH];
		int ret = ::GetModuleFileName( NULL, zFileName, MAX_PATH );
		if (ret == 0)
		{
			throw UCLIDException( "ELI12753", "Unable to retrieve module file name!" );
		}

		// Retrieve the Version string
		string strVersion = "USS File Viewer Version ";
		strVersion += ::getFileVersion( string( zFileName ) );

		// Provide the Version string
		SetDlgItemText( IDC_EDIT_VERSION, strVersion.c_str() );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12752")

	return TRUE;  // return TRUE unless you set the focus to a control
				  // EXCEPTION: OCX Property Pages should return FALSE
}

//-------------------------------------------------------------------------------------------------
// CSpatialStringViewerDlg dialog
//-------------------------------------------------------------------------------------------------
CSpatialStringViewerDlg::CSpatialStringViewerDlg(CWnd* pParent /*=NULL*/)
: CDialog(CSpatialStringViewerDlg::IDD, pParent), 
m_bInitialized(false),
m_strUSSFileName(""),
m_nTotalNumChars(0)
{
	try
	{
		// create instance of the configuration settings object
		m_apSettings = auto_ptr<SpatialStringViewerCfg>(new SpatialStringViewerCfg(this));

		//{{AFX_DATA_INIT(CSpatialStringViewerDlg)
		m_zText = _T("");
		//}}AFX_DATA_INIT
		// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
		m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

		// create a spatial string object
		m_ipSpatialString.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI06719", m_ipSpatialString != NULL);

		// parse the command line argument and obtain the name of the USS file
		/*		if (__argc > 2)
		{
		UCLIDException ue("ELI06721", "Please pass exactly one argument (the name of the USS file) to this application!");
		ue.addDebugInfo("# of arguments", __argc - 1);
		throw ue;
		}
		else */if (__argc == 2)
		{
			// make sure the file name is a long path
			::getLongPathName(__argv[1], m_strUSSFileName); 
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06718")
}
//-------------------------------------------------------------------------------------------------
CSpatialStringViewerDlg::~CSpatialStringViewerDlg()
{
	try
	{
		// make sure we have destroyed each of the dialog boxes that were created in OnInit
		if (ma_pFindRegExDlg.get())
		{
			ma_pFindRegExDlg->DestroyWindow();
			ma_pFindRegExDlg.reset(NULL);
		}
		if (ma_pCharInfoDlg.get())
		{
			ma_pCharInfoDlg->DestroyWindow();
			ma_pCharInfoDlg.reset(NULL);
		}
		if(ma_pFontSizeDistDlg.get())
		{
			ma_pFontSizeDistDlg->DestroyWindow();
			ma_pFontSizeDistDlg.reset(NULL);
		}
		if (ma_pWordLengthDistDlg.get())
		{
			ma_pWordLengthDistDlg->DestroyWindow();
			ma_pWordLengthDistDlg.reset(NULL);
		}

		if (m_apSettings.get())
		{
			m_apSettings.reset(NULL);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16494");
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CSpatialStringViewerDlg)
	DDX_Control(pDX, IDC_EDIT_TEXT, m_editText);
	DDX_Text(pDX, IDC_EDIT_TEXT, m_zText);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSpatialStringViewerDlg, CDialog)
	//{{AFX_MSG_MAP(CSpatialStringViewerDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_DROPFILES()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_SIZE()
	ON_WM_HELPINFO()
	ON_COMMAND(ID_FILE_CLOSE, OnFileClose)
	ON_COMMAND(ID_FILE_EXIT, OnFileExit)
	ON_COMMAND(ID_FILE_OPEN, OnFileOpen)
	ON_COMMAND(ID_HELP_ABOUTUCLIDSPATIALSTRINGVIEWER, OnHelpAboutUclidSpatialStringViewer)
	ON_COMMAND(ID_FILE_PROPERTIES, OnFileProperties)
	ON_COMMAND(ID_MNU_FIND_REGEXPR, OnMnuFindRegexpr)
	ON_COMMAND(ID_MNU_FONTSIZEDISTRIBUTION, OnMnuFontsizedistribution)
	ON_COMMAND(ID_MNU_WORDLENGTHDISTRIBUTION, OnMnuWordLengthDistribution)
	ON_COMMAND(ID_MNU_OPEN_CHAR_INFO, &CSpatialStringViewerDlg::OnMnuOpenCharInfo)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CSpatialStringViewerDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CSpatialStringViewerDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// call the base class member for initialization
		CDialog::OnInitDialog();

		// get pointers to modeless dialogs
		ma_pFindRegExDlg = auto_ptr<FindRegExDlg>(new FindRegExDlg(this, m_apSettings.get(), this));
		ma_pCharInfoDlg = auto_ptr<CharInfoDlg>(new CharInfoDlg(this));
		ma_pFontSizeDistDlg = auto_ptr<FontSizeDistributionDlg>(
			new FontSizeDistributionDlg(this, m_apSettings.get(), this));
		ma_pWordLengthDistDlg = auto_ptr<WordLengthDistributionDlg>(
			new WordLengthDistributionDlg(this, m_apSettings.get(), this));

		// create modeless dialogs
		ma_pFindRegExDlg->Create(FindRegExDlg::IDD, NULL);
		ma_pCharInfoDlg->Create(CharInfoDlg::IDD, NULL);
		ma_pFontSizeDistDlg->Create(FontSizeDistributionDlg::IDD, NULL);
		ma_pWordLengthDistDlg->Create(WordLengthDistributionDlg::IDD, NULL);

		// if there is no spatial string object, then just close this window
		if (m_ipSpatialString == NULL)
		{
			PostMessage(WM_CLOSE);
			return TRUE;
		}

		// Add "About..." menu item to system menu.

		// IDM_ABOUTBOX must be in the system command range.
		ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
		ASSERT(IDM_ABOUTBOX < 0xF000);

		CMenu* pSysMenu = GetSystemMenu(FALSE);
		if (pSysMenu != NULL)
		{
			CString strAboutMenu;
			strAboutMenu.LoadString(IDS_ABOUTBOX);
			if (!strAboutMenu.IsEmpty())
			{
				pSysMenu->AppendMenu(MF_SEPARATOR);
				pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
			}
		}

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon

		// update the member variable for the spatial text
		string strText = m_ipSpatialString->String;
		m_editText.SetWindowText(strText.c_str());

		// note that the window has been initialized;
		m_bInitialized = true;

		// create status bar
		m_statusBar.Create(this);
		m_statusBar.GetStatusBarCtrl().SetMinHeight(g_nStatusBarHeight);
		m_statusBar.SetIndicators(indicators, sizeof(indicators)/sizeof(UINT));
		RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);

		// resize the edit control to be the size of the client area of
		// this window
		resizeEditControl();

		// restore this window to its last position (if applicable)
		// only if no other instances of this window exist.
		// check to see if a window with this name already exists
		CString zWindowTitle;
		GetWindowText(zWindowTitle);
		HWND hWnd = ::FindWindowEx(NULL, m_hWnd, NULL, zWindowTitle);
		if (hWnd == NULL)
		{
			m_apSettings->restoreLastWindowPosition();
		}

		loadSpatialStringFromFile();

		// Clear any selection
		m_editText.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06733");

	return FALSE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnDropFiles(HDROP hDropInfo)
{
	try
	{
		unsigned int iNumFiles = DragQueryFile(hDropInfo, 0xFFFFFFFF, NULL, NULL);
		if (iNumFiles > 1)
		{
			MessageBox("Please only drag-and-drop one file into this window!");
			return;
		}

		// get the full path to the dragged filename
		char pszFile[MAX_PATH+1];
		DragQueryFile(hDropInfo, 0, pszFile, MAX_PATH);

		// Get the associated USS file
		m_strUSSFileName = getUSSFileName( pszFile );

		// notify the input property page that a file has been dragged-and-dropped
		loadSpatialStringFromFile();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06722")
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	try
	{
		if ((nID & 0xFFF0) == IDM_ABOUTBOX)
		{
			CAboutDlg dlgAbout;
			dlgAbout.DoModal();
		}
		else
		{
			CDialog::OnSysCommand(nID, lParam);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16839")
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CSpatialStringViewerDlg::OnPaint() 
{
	try
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
			CDialog::OnPaint();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16841")
}
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CSpatialStringViewerDlg::OnQueryDragIcon()
{
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16842")

	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnSize(UINT nType, int cx, int cy) 
{
	try
	{
		CDialog::OnSize(nType, cx, cy);

		// resize the edit control to be the size of the client area of
		// this window and save the position of this window
		if (m_bInitialized)
		{
			resizeEditControl();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06730")
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnCancel() 
{
	try
	{
		// save the position of this window
		m_apSettings->saveCurrentWindowPosition();

		RECT r;
		if (ma_pFindRegExDlg.get())
		{
			ma_pFindRegExDlg->GetWindowRect(&r);
			m_apSettings->saveLastFindWindowPos(r.left, r.top);
		}

		if (ma_pWordLengthDistDlg.get())
		{
			ma_pWordLengthDistDlg->GetWindowRect(&r);
			m_apSettings->saveLastDistributionWindowPos(r.left, r.top);
		}

		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06731")
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnFileClose() 
{
	try
	{
		// clear the file name and clear out the text.
		m_strUSSFileName = "";
		loadSpatialStringFromFile();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06727")
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnFileExit() 
{
	try
	{
		// close the application
		SendMessage(WM_CLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06728")
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnFileOpen() 
{
	try
	{
		// ask user to select file to load
		CFileDialog fileDlg(TRUE, ".uss", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST,
			"UCLID Spatial String Files (*.uss)|*.uss|All Files (*.*)|*.*||", this);

		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		if (tfd.doModal() != IDOK)
		{
			return;
		}

		// Get the filename
		m_strUSSFileName = getUSSFileName( (LPCTSTR) fileDlg.GetPathName() );

		// Load the string
		loadSpatialStringFromFile();

		// refresh the font size distribution
		if (ma_pFontSizeDistDlg.get())
		{
			ma_pFontSizeDistDlg->refreshDistribution();
		}

		// reset and refresh the word length distribution
		if (ma_pWordLengthDistDlg.get())
		{
			ma_pWordLengthDistDlg->resetAndRefresh();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06729")
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnHelpAboutUclidSpatialStringViewer() 
{
	try
	{
		// display the about dialog box
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06726")
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnFileProperties() 
{
	try
	{
		// Retrieve the source document information
		string strSourceDocName = asString(m_ipSpatialString->SourceDocName);

		// display the source document information
		if (strSourceDocName.empty())
		{
			MessageBox("No source document information is available!", "Properties", 
				MB_ICONINFORMATION);
		}
		else
		{
			// retrieve the collected SP Info objects
			ILongToObjectMapPtr ipCollOfPageInfo;
			if (m_ipSpatialString->HasSpatialInfo() == VARIANT_TRUE)
			{
				ipCollOfPageInfo = m_ipSpatialString->SpatialPageInfos;
			}
			else
			{
				// Create an empty spatial page info for empty spatial strings [LRCU #5313]
				ipCollOfPageInfo.CreateInstance(CLSID_LongToObjectMap);
			}
			ASSERT_RESOURCE_ALLOCATION("ELI16819", ipCollOfPageInfo != NULL);

			// Display the property dialog box
			USSPropertyDlg dlg(strSourceDocName, m_strOriginalSourceDoc, 
				m_strUSSFileName, asString(m_ipSpatialString->OCREngineVersion),
				ipCollOfPageInfo, this);
			dlg.DoModal();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06808")
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnMnuFindRegexpr() 
{
	try
	{
		if (ma_pFindRegExDlg.get() && asCppBool(::IsWindow(ma_pFindRegExDlg->m_hWnd)))
		{
			// show the find dialog
			ma_pFindRegExDlg->ShowWindow(SW_SHOW);
		}
		else
		{
			throw UCLIDException("ELI18609", 
				"Find regular expression dialog has not been initialized!");
		}
	}

	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07401")
}
//--------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnMnuOpenCharInfo()
{
	try
	{
		if (ma_pCharInfoDlg.get() && asCppBool(::IsWindow(ma_pCharInfoDlg->m_hWnd)))
		{
			ma_pCharInfoDlg->ShowWindow(SW_SHOW);
		}
		else
		{
			throw UCLIDException("ELI18610", 
				"Character information dialog has not been initialized!");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16843")
}
//--------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnMnuFontsizedistribution() 
{
	// TODO: Add your command handler code here
	try
	{
		if (ma_pFontSizeDistDlg.get() && asCppBool(::IsWindow(ma_pFontSizeDistDlg->m_hWnd)))
		{
			// show the find dialog
			ma_pFontSizeDistDlg->ShowWindow(SW_SHOW);
		}
		else
		{
			throw UCLIDException("ELI18611", 
				"Font size distribution dialog has not been initialized!");
		}
	}

	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10616")

}
//--------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::OnMnuWordLengthDistribution()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (ma_pWordLengthDistDlg.get() && asCppBool(::IsWindow(ma_pWordLengthDistDlg->m_hWnd)))
		{
			// show the distribution dialog
			ma_pWordLengthDistDlg->ShowWindow(SW_SHOW);
		}
		else
		{
			throw UCLIDException("ELI20630", 
				"Word length distribution dialog has not been initialized!");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20631");
}
//--------------------------------------------------------------------------------------------------
BOOL CSpatialStringViewerDlg::PreTranslateMessage(MSG* pMsg) 
{
	BOOL bRetCode(FALSE);
	try
	{
		TemporaryResourceOverride ro(AfxGetApp()->m_hInstance);

		// Catch and eat Escape characters
		if ((pMsg->message == WM_KEYDOWN) && (pMsg->wParam == VK_ESCAPE))
		{
			// Do nothing and return
			return TRUE;
		}

		// only if the message is for the edit box
		if (pMsg->hwnd == m_editText.m_hWnd)
		{
			long nPage = -1;
			long nFontSize = 0;
			long nConfidence = 100;
			long nTop(0), nBottom(0), nLeft(0), nRight(0);
			bool bEndOfParagraph(false), bEndOfZone(false);
			unsigned short usGuess1(0), usGuess2(0), usGuess3(0);

			int nStart, nEnd;
			m_editText.GetSel(nStart, nEnd);

			CString zPage(""), zStart(""), zEnd(""), zPercentage("");

			// refresh the current word length distribution if it is visible
			if (ma_pWordLengthDistDlg.get() && asCppBool(ma_pWordLengthDistDlg->IsWindowVisible()))
			{
				ma_pWordLengthDistDlg->refreshDistribution(nStart, nEnd);
			}

			// if there is spatial information, then collect that information
			if (m_ipSpatialString->GetMode() == kSpatialMode)
			{
				// Get the page number of the character following the cursor
				if(nEnd < m_ipSpatialString->Size)
				{
					ILetterPtr ipLetter = NULL;
					m_ipSpatialString->GetNextOCRImageSpatialLetter(nEnd, &ipLetter);

					if(ipLetter)
					{
						nPage = ipLetter->GetPageNumber();
					}
				}

				// check to see if the user has selected a single character
				int nDifference = nEnd - nStart;
				// check for a difference between -1 and 1 which indicates a single
				// character is selected
				if (nDifference == -1 || nDifference == 1 || nDifference == 0)
				{
					if (nEnd < m_ipSpatialString->Size)
					{
						// set the character we should get to be nStart
						// then check to see if the selection was right to left
						// in which case the character we want is at nEnd, not
						// nStart.
						int nCharacter = nStart;
						if (nDifference == -1)
						{
							nCharacter = nEnd;
						}

						ILetterPtr ipLetter = m_ipSpatialString->GetOCRImageLetter(nCharacter);

						// check to be sure we have a letter
						if (ipLetter != NULL)
						{
							// update the Character Info window if it is visible
							if (ma_pCharInfoDlg.get() && 
								asCppBool(ma_pCharInfoDlg->IsWindowVisible()))
							{
								ma_pCharInfoDlg->setCharacterData(ipLetter);
							}
						}
					}
				}
				// check to see if looking at a selection of multiple characters
				else if(nStart < nEnd &&
					nStart >= 0 && nEnd <= m_ipSpatialString->Size)
				{
					// Get the mode font size and mean confidence of the selected text
					ISpatialStringPtr ipSubString = m_ipSpatialString->GetSubString(nStart, nEnd-1);
					ASSERT_RESOURCE_ALLOCATION("ELI20673", ipSubString != NULL);

					if(ipSubString->HasSpatialInfo() == VARIANT_TRUE)
					{
						// Get the confidence
						ipSubString->GetCharConfidence(NULL, NULL, &nConfidence);
						// calculate the mode font size of the characters
						ILongToLongMapPtr ipMap = ipSubString->GetFontSizeDistribution();

						long nModeFontSize = 0;
						long nMaxNumChars = 0;

						long nMapSize = ipMap->Size;
						int i;
						for (i = 0; i < nMapSize; i++)
						{
							long nFontSize, nNumFontChars;
							ipMap->GetKeyValue(i, &nFontSize, &nNumFontChars);
							// DO NOT count Non-Spatial characters
							if(nFontSize == 0)
							{
								continue;
							}
							if(nModeFontSize == 0 || nMaxNumChars < nNumFontChars)
							{
								nModeFontSize = nFontSize;
								nMaxNumChars = nNumFontChars;
							}
						}
						nFontSize = nModeFontSize;

						// Get the font attributes
						VARIANT_BOOL bItalic, bBold, bSansSerif, bSerif, bProportional, bUnderline, 
							bSuperScript, bSubScript;
						ipSubString->GetFontInfo(1, &bItalic, &bBold, &bSansSerif, &bSerif, 
							&bProportional, &bUnderline, &bSuperScript, &bSubScript);

						// now set our font string based on the return values
						CString zFont = "";
						zFont.AppendChar( asCppBool(bItalic) ? gcITALIC : gcBLANK );
						zFont.AppendChar( asCppBool(bBold) ? gcBOLD : gcBLANK );
						zFont.AppendChar( asCppBool(bSansSerif) ? gcSANSERIF : gcBLANK );
						zFont.AppendChar( asCppBool(bSerif) ? gcSERIF : gcBLANK );
						zFont.AppendChar( asCppBool(bProportional) ? gcPROPORTIONAL : gcBLANK ); 
						zFont.AppendChar( asCppBool(bUnderline) ? gcUNDERLINE : gcBLANK );
						zFont.AppendChar( asCppBool(bSuperScript) ? gcSUPERSCRIPT : gcBLANK );
						zFont.AppendChar( asCppBool(bSubScript) ? gcSUBSCRIPT : gcBLANK );

						// update the Character Info window if it is visible
						if (ma_pCharInfoDlg.get() && asCppBool(ma_pCharInfoDlg->IsWindowVisible()))
						{
							ma_pCharInfoDlg->setCharacterData(nModeFontSize, nConfidence, 
								nPage, zFont);
						}
					}
				}
			}

			// Update page number
			zPage.Format(ID_INDICATOR_PAGE, nPage);

			// comma format and update the start and end position
			string strStartAndEndString("Start Pos: ");
			strStartAndEndString += commaFormatNumber(static_cast<LONGLONG>(nStart));
			zStart = strStartAndEndString.c_str();
			strStartAndEndString = "End Pos: ";
			strStartAndEndString += commaFormatNumber(static_cast<LONGLONG>(nEnd));
			zEnd = strStartAndEndString.c_str();

			int nLastPos = m_nTotalNumChars - 1;
			if (nLastPos <= 0)
			{
				nLastPos = 1;
			}

			// Compute and update the percentage
			int nPercentage = (int)(((double)nStart/(double)nLastPos) * 100);
			zPercentage.Format(ID_INDICATOR_PERCENT, nPercentage);

			// Update the status bar
			setStatusBarText(zPage, zStart, zEnd, zPercentage);

			return CDialog::PreTranslateMessage(pMsg);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10627");
	return bRetCode;
}
//-------------------------------------------------------------------------------------------------
BOOL CSpatialStringViewerDlg::OnHelpInfo(HELPINFO* pHelpInfo)
{
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16844")
	// Do not show any Help
	return TRUE;
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
int CSpatialStringViewerDlg::getCurrentCursorPosition()
{
	int nCurPos = 0, nStartPos = -1, nEndPos = -1;
	m_editText.GetSel(nStartPos, nEndPos);

	if (nStartPos >= 0 && nEndPos < 0)
	{
		nCurPos = nStartPos;
	}
	else if (nStartPos >=0 && nEndPos >= 0 && nEndPos >= nStartPos)
	{
		nCurPos = nEndPos;
	}

	return nCurPos;
}
//-------------------------------------------------------------------------------------------------
string CSpatialStringViewerDlg::getEntireDocumentText()
{
	UpdateData(TRUE);
	return (LPCTSTR)m_zText;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::selectText(int nStartPos, int nEndPos)
{
	m_editText.SetSel(nStartPos, nEndPos);
	m_editText.SetFocus();
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CSpatialStringViewerDlg::getSpatialString()
{
	return m_ipSpatialString;
}
//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
std::string CSpatialStringViewerDlg::getUSSFileName(const std::string strInputName) const
{
	// Test the file extension
	string strExt = getExtensionFromFullPath( strInputName, true );
	if (strExt == ".uss")
	{
		return strInputName;
	}

	// create a string to hold the file name of associated input file
	// NOTE: this is set to the original input name, so if no associated document
	// is found, we will simply validate and return the original input file
	string strAssociatedFile = strInputName;

	// Check if the extension is an image file or numeric extension [LRCAU #5208]
	if( isImageFileExtension(strExt) || isNumericExtension(strExt))
	{
		// Append ".uss" and look for this file
		strAssociatedFile += string( ".uss" );
	}
	else
	{
		// get the file name without the outermost extension (eg. 123.tif.voa becomes 123.tif)
		string strInputFileWithoutExt = getPathAndFileNameWithoutExtension(strInputName);

		// get the sub-extension of the original file name (eg. 123.tif.voa's sub-extension is .tif)
		strExt = getExtensionFromFullPath(strInputFileWithoutExt);

		// check if the original file's sub-extension is an image file or
		// numeric extension [LRCAU #5210]
		if( isImageFileExtension(strExt) || isNumericExtension(strExt))
		{
			strAssociatedFile = strInputFileWithoutExt + string(".uss");
		}
	}

	// check if the associated file is valid
	if( !isFileOrFolderValid(strAssociatedFile) )
	{
		// Associated USS file is not present, throw exception
		UCLIDException ue("ELI16269", "Unable to find and open associated USS file!");
		ue.addDebugInfo("Associated File", strAssociatedFile);
		ue.addDebugInfo("Input File", strInputName);
		throw ue;
	}

	return strAssociatedFile;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::loadSpatialStringFromFile()
{
	try
	{
		try
		{
			// display the wait cursor
			CWaitCursor wait;

			// load the spatial string from the specified file
			if (!m_strUSSFileName.empty())
			{
				m_strOriginalSourceDoc = m_ipSpatialString->LoadFrom(
					get_bstr_t(m_strUSSFileName), VARIANT_FALSE);
				// get the size of current document
				m_nTotalNumChars = m_ipSpatialString->String.length();
			}
			else
			{
				// the caller wants the spatial string in memory to be cleared out
				// clear the related strings and names
				m_ipSpatialString->Clear();
				m_strOriginalSourceDoc = "";
				m_nTotalNumChars = 0;
			}

			// Update the caption
			updateWindowCaption( m_strUSSFileName );

		}
		catch (...)
		{
			// reset the spatial string to an empty string, and throw
			// the exception for it to be displayed
			try
			{
				m_ipSpatialString->Clear();
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25969");

			m_strOriginalSourceDoc = "";
			throw;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06723")

		// if the window has already been initialized, then update
		// the text in the window
		if (m_bInitialized)
		{
			string strText = m_ipSpatialString->String;
			m_editText.SetWindowText(strText.c_str());
		}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::resizeEditControl()
{
	// resize the editbox to be the size of the client area
	CRect rect;
	GetClientRect(rect);
	rect.bottom = rect.Height() - g_nStatusBarHeight;
	m_editText.MoveWindow(rect);

	// make sure the status bar exists before accessing it
	if (asCppBool(::IsWindow(m_statusBar.m_hWnd)))
	{
		m_statusBar.GetStatusBarCtrl().SetMinHeight(g_nStatusBarHeight);
		m_statusBar.SetIndicators(indicators, sizeof(indicators)/sizeof(UINT));
		RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::setStatusBarText(const CString& zPage,
											   const CString& zStartPos, 
											   const CString& zEndPos,
											   const CString& zPercentage)
{
	// make sure the status bar exists before accessing it
	if (asCppBool(::IsWindow(m_statusBar.m_hWnd)))
	{
		m_statusBar.SetPaneText(m_statusBar.CommandToIndex(ID_INDICATOR_PAGE), zPage);
		m_statusBar.SetPaneText(m_statusBar.CommandToIndex(ID_INDICATOR_START), zStartPos);
		m_statusBar.SetPaneText(m_statusBar.CommandToIndex(ID_INDICATOR_END), zEndPos);
		m_statusBar.SetPaneText(m_statusBar.CommandToIndex(ID_INDICATOR_PERCENT), zPercentage);
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringViewerDlg::updateWindowCaption(const string& strFileName)
{
	// Compute the window caption
	string strResult;
	if (!strFileName.empty())
	{
		// if a file is currently loaded, then only display the filename and
		// not the full path.
		strResult = getFileNameFromFullPath( strFileName );
		strResult += " - ";
		strResult += strWINDOW_TITLE;
	}
	else
	{
		strResult = strWINDOW_TITLE;
	}

	// Update the window caption
	SetWindowText( strResult.c_str() );
}
//-------------------------------------------------------------------------------------------------
