// DecompositionViewerDlg.cpp : implementation file
//

#include "stdafx.h"
#include "DecompositionViewer.h"
#include "DecompositionViewerDlg.h"


#include <LicenseMgmt.h>
#include <UCLIDException.hpp>
#include <CppUtil.hpp>
#include <FileDirectorySearcher.hpp>
#include <XBrowseForFolder.h>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <RegistryPersistenceMgr.h>

#include <afxdlgs.h>
#include <dlgs.h>

#include <stdio.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

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
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CDecompositionViewerDlg dialog
//-------------------------------------------------------------------------------------------------
CDecompositionViewerDlg::CDecompositionViewerDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CDecompositionViewerDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDecompositionViewerDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDecompositionViewerDlg)
	DDX_Control(pDX, IDC_TEXT_SELECT2, m_SpatialStringListBox2);
	DDX_Control(pDX, IDC_TEXT_SELECT, m_SpatialStringListBox);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDecompositionViewerDlg, CDialog)
	//{{AFX_MSG_MAP(CDecompositionViewerDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_LOADUSSFILE, OnLoadUssFileClicked)
	ON_BN_CLICKED(IDC_SHOW_WORDS, OnShowWordsClicked)
	ON_BN_CLICKED(IDC_SHOW_ZONES, OnShowZonesClicked)
	ON_BN_CLICKED(IDC_SHOW_LINES, OnShowLinesClicked)
	ON_BN_CLICKED(IDC_SHOW_PARAGRAPHS, OnShowParagraphsClicked)
	ON_LBN_SELCHANGE(IDC_TEXT_SELECT, OnTextSelectSelectionChange)
	ON_BN_CLICKED(IDC_LOADUSSFILE2, OnLoadUssFile2Clicked)
	ON_LBN_SELCHANGE(IDC_TEXT_SELECT2, OnTextSelect2SelectionChange)
	ON_BN_CLICKED(IDC_SHOW_LETTERS, OnShowLettersClicked)
	ON_BN_CLICKED(IDC_CALCLINEHEIGHT, OnCalclineheight)
	ON_BN_CLICKED(IDC_SELECTALL, OnSelectall)
	ON_BN_CLICKED(IDC_UNSELECTALL, OnUnselectall)
	ON_BN_CLICKED(IDC_CALCLINEHEIGHT2, OnCalclineheight2)
	ON_BN_CLICKED(IDC_VIEWLINEHEIGHTS, OnViewlineheights)
	ON_LBN_DBLCLK(IDC_TEXT_SELECT, OnDblclkTextSelect)
	ON_BN_CLICKED(IDC_SELECTALL2, OnSelectall2)
	ON_BN_CLICKED(IDC_UNSELECTALL2, OnUnselectall2)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// CDecompositionViewerDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CDecompositionViewerDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

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
		SetIcon(m_hIcon, FALSE);		// Set small icon

		// My code
		//LicenseManagement::sGetInstance().initializeLicenseFromFile(string("D:\\Engineering\\Binaries\\Debug\\UCLID_FlexIndexSDK.lic"), 1933642460, 1255700392, 1734099078, 1545892908);
		
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();

		// Create the viewing windows
		HRESULT hr = m_ipSRIR1.CreateInstance(CLSID_SpotRecognitionWindow);
		ASSERT_RESOURCE_ALLOCATION("ELI07511", m_ipSRIR1 != NULL);
		hr = m_ipSRIR2.CreateInstance(CLSID_SpotRecognitionWindow);
		ASSERT_RESOURCE_ALLOCATION("ELI07512", m_ipSRIR2 != NULL);

		hr = m_ipInputManager.CreateInstance(CLSID_InputManager);
		ASSERT_RESOURCE_ALLOCATION("ELI07513", m_ipInputManager != NULL);

		m_ipInputReceiver1 = m_ipSRIR1;
		m_ipInputReceiver2 = m_ipSRIR2;

		m_ipInputManager->ConnectInputReceiver(m_ipInputReceiver1);
		m_ipInputManager->ConnectInputReceiver(m_ipInputReceiver2);
		
		m_ipSpatialString1.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI07515", m_ipSpatialString1 != NULL);
		m_ipSpatialString2.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI07516", m_ipSpatialString2 != NULL);

		m_eCurrentRegionType = kNone;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07510");

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnCancel()
{

	try
	{
		m_ipSpatialString1.Release();
		m_ipSpatialString2.Release();
	
		m_ipInputManager.Release();

		m_ipInputReceiver1.Release();
		m_ipInputReceiver2.Release();

		m_ipSRIR1.Release();
		m_ipSRIR2.Release();

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07803");
	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnSysCommand(UINT nID, LPARAM lParam)
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
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CDecompositionViewerDlg::OnPaint() 
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
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CDecompositionViewerDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnShowWordsClicked() 
{
	m_eCurrentRegionType = kWords;
	m_ipInputManager->ShowWindows(VARIANT_FALSE);
	populateList(m_eCurrentRegionType, m_ipSRIR1, m_ipSpatialString1, &m_SpatialStringListBox);
	populateList(m_eCurrentRegionType, m_ipSRIR2, m_ipSpatialString2, &m_SpatialStringListBox2);
	m_ipInputManager->ShowWindows(VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnShowZonesClicked() 
{
	m_eCurrentRegionType = kZones;
	m_ipInputManager->ShowWindows(VARIANT_FALSE);
	populateList(m_eCurrentRegionType, m_ipSRIR1, m_ipSpatialString1, &m_SpatialStringListBox);
	populateList(m_eCurrentRegionType, m_ipSRIR2, m_ipSpatialString2, &m_SpatialStringListBox2);
	m_ipInputManager->ShowWindows(VARIANT_TRUE);
	// TODO: Add your control notification handler code here
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnShowLinesClicked() 
{
	m_eCurrentRegionType = kLines;
	m_ipInputManager->ShowWindows(VARIANT_FALSE);
	populateList(m_eCurrentRegionType, m_ipSRIR1, m_ipSpatialString1, &m_SpatialStringListBox);
	populateList(m_eCurrentRegionType, m_ipSRIR2, m_ipSpatialString2, &m_SpatialStringListBox2);
	m_ipInputManager->ShowWindows(VARIANT_TRUE);
	// TODO: Add your control notification handler code here
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnShowParagraphsClicked() 
{
	m_eCurrentRegionType = kParagraphs;
	m_ipInputManager->ShowWindows(VARIANT_FALSE);
	populateList(m_eCurrentRegionType, m_ipSRIR1, m_ipSpatialString1, &m_SpatialStringListBox);
	populateList(m_eCurrentRegionType, m_ipSRIR2, m_ipSpatialString2, &m_SpatialStringListBox2);
	m_ipInputManager->ShowWindows(VARIANT_TRUE);
	// TODO: Add your control notification handler code here
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnShowLettersClicked() 
{
	m_eCurrentRegionType = kLetters;
	m_ipInputManager->ShowWindows(VARIANT_FALSE);
	//populateList(m_eCurrentRegionType, m_ipSRIR1, m_ipSpatialString1, &m_SpatialStringListBox);
	//populateList(m_eCurrentRegionType, m_ipSRIR2, m_ipSpatialString2, &m_SpatialStringListBox2);
	clear(m_ipSRIR1, &m_SpatialStringListBox);
	clear(m_ipSRIR2, &m_SpatialStringListBox2);
	m_ipInputManager->ShowWindows(VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnLoadUssFileClicked() 
{	
	static char szFilter[] = "UCLID Spatial String Files (*.uss)|*.uss|";
	CFileDialog dlg(true, ".uss", NULL, OFN_OVERWRITEPROMPT, szFilter);
	if (dlg.DoModal() == IDOK) // load the file
	{
		clear(m_ipSRIR1, &m_SpatialStringListBox);
		m_ipSpatialString1->LoadFrom(dlg.GetPathName().AllocSysString(), VARIANT_FALSE);
		m_ipSRIR1->OpenImageFile(m_ipSpatialString1->GetSourceDocName());
		populateList(m_eCurrentRegionType, m_ipSRIR1, m_ipSpatialString1, &m_SpatialStringListBox);
	}
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnLoadUssFile2Clicked() 
{
	static char szFilter[] = "UCLID Spatial String Files (*.uss)|*.uss|";
	CFileDialog dlg(true, ".uss", NULL, OFN_OVERWRITEPROMPT, szFilter);
	if(dlg.DoModal() == IDOK) // load the file
	{
		clear(m_ipSRIR2, &m_SpatialStringListBox2);
		m_ipSpatialString2->LoadFrom(dlg.GetPathName().AllocSysString(), VARIANT_FALSE);
		m_ipSRIR2->OpenImageFile(m_ipSpatialString2->GetSourceDocName());
		populateList(m_eCurrentRegionType, m_ipSRIR2, m_ipSpatialString2, &m_SpatialStringListBox2);
	}
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnTextSelectSelectionChange() 
{
	int i = 0;
	for(i = 0; i < m_SpatialStringListBox.GetCount(); i++)
	{
		int isSel =  m_SpatialStringListBox.GetSel(i);
		ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox.GetItemDataPtr(i);
		if(isSel && !info->m_bWasSelected)
		{
			info->m_id = createZoneEntity(m_ipSRIR1, info->m_ipSpatialString, info->m_page);
			info->m_bWasSelected = true;
			
		}
		else if(!isSel && info->m_bWasSelected)
		{
			deleteZoneEntity(m_ipSRIR1, info->m_id);
			info->m_bWasSelected = false;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnTextSelect2SelectionChange() 
{
	// TODO: Add your control notification handler code here
	int i = 0;
	for(i = 0; i < m_SpatialStringListBox2.GetCount(); i++)
	{
		int isSel =  m_SpatialStringListBox2.GetSel(i);
		ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox2.GetItemDataPtr(i);
		if(isSel && !info->m_bWasSelected)
		{
			info->m_id = createZoneEntity(m_ipSRIR2, info->m_ipSpatialString, info->m_page);
			info->m_bWasSelected = true;
			
		}
		else if(!isSel && info->m_bWasSelected)
		{
			deleteZoneEntity(m_ipSRIR2, info->m_id);
			info->m_bWasSelected = false;
		}
	}	
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnCalclineheight() 
{
	// Currently only uses the first window
	// TODO: Add your control notification handler code here
	try
	{
		// compute an weighted average of every selected region
		float totalRatio = 0;
		int totalNumLines = 0;

		int i = 0;
		for(i = 0; i < m_SpatialStringListBox.GetCount(); i++)
		{
			if(m_SpatialStringListBox.GetSel(i) == 0)
			{
				continue;
			}
			ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox.GetItemDataPtr(i);
			ISpatialStringPtr ipSS = info->m_ipSpatialString;

			int iParagraph = 0;
			IIUnknownVectorPtr ipParagraphs;
			ipParagraphs = ipSS->GetParagraphs();
			for(iParagraph = 0; iParagraph < ipParagraphs->Size(); iParagraph++)
			{

				ISpatialStringPtr ipParagraph = (ISpatialStringPtr)ipParagraphs->At(iParagraph);
			
				// get the tallest character in the paragraph
				int maxLetterHeight = 0;
				ILetterPtr maxLetter;
				
				IIUnknownVectorPtr ipLetters = ipParagraph->GetLetters();
				int iLetter = 0;
				for(iLetter = 0; iLetter < ipLetters->Size(); iLetter++)
				{
					ILetterPtr ipLetter = (ILetterPtr)ipLetters->At(iLetter);
					
					if(ipLetter->GetIsSpatialChar() == VARIANT_FALSE)
					{
						continue;
					}
					char c = ipLetter->GetGuess1();
					if ((c <'A' || c > 'Z') && 
						c != 'i' &&
						c != 'l' &&
						c != 'f' &&
						c != 'd' &&
						c != 'b' &&
						c != 'g' &&
						c != 'y' )
						continue;
					int height = ipLetter->GetBottom() - ipLetter->GetTop();
					if (height > maxLetterHeight)
					{	
						maxLetterHeight = height;
						maxLetter = ipLetter;
					}	
				}
				if (maxLetterHeight <= 0)
				{
					continue;
				}
				int totalParagraphLineHeight = 0;
				int numParagraphLines = 0;

				IIUnknownVectorPtr ipLines = ipParagraph->GetLines();
				if(ipLines->Size() < 2)
				{
					continue;
				}
				int iLine = 0;
				for (iLine = 0; iLine < ipLines->Size() - 1; iLine++)
				{
					ISpatialStringPtr ipLine1 = (ISpatialStringPtr)ipLines->At(iLine);
					ISpatialStringPtr ipLine2 = (ISpatialStringPtr)ipLines->At(iLine + 1);

					ILongRectanglePtr ipBounds1 = ipLine1->GetBounds();
					ILongRectanglePtr ipBounds2 = ipLine2->GetBounds();

					float dh = ipBounds2->GetBottom() - ipBounds1->GetBottom();

					// Skip Data points that are probably Outliers
					if(dh > 2.5*(float)maxLetterHeight)
					{
						return;
					}
					if(dh < (float)maxLetterHeight)
					{
						return;
					}
					totalParagraphLineHeight += dh;
					numParagraphLines++;
				}
				
				int avgLineHeight = totalParagraphLineHeight / numParagraphLines;
				float ratio = ((float)avgLineHeight / (float)maxLetterHeight);
				char buf[1024];

				long retHeight = ipParagraph->GetAverageLineHeight();

				sprintf(buf, "Ratio %f\nMaxLetterHeight %d\nMaxLetter %c\nReRatio %f\n", 
					ratio, maxLetterHeight, maxLetter->GetGuess1(), 
					(float)retHeight / (float)maxLetterHeight);
				if (MessageBox(buf, NULL, MB_OKCANCEL) == IDCANCEL)
				{
					break;
				}
				totalRatio += ratio*numParagraphLines;
				totalNumLines += numParagraphLines;
			}	
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07796")
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnSelectall() 
{
	try
	{
		int i = 0;
		for (i = 0; i < m_SpatialStringListBox.GetCount(); i++)
		{
			int isSel =  m_SpatialStringListBox.GetSel(i);
			ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox.GetItemDataPtr(i);
			if (isSel == 0)
			{
				m_SpatialStringListBox.SetSel(i, TRUE);
				OnTextSelectSelectionChange();	
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07798")
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnUnselectall() 
{
	try
	{
		int i = 0;
		for (i = 0; i < m_SpatialStringListBox.GetCount(); i++)
		{
			int isSel =  m_SpatialStringListBox.GetSel(i);
			ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox.GetItemDataPtr(i);
			if (isSel != 0)
			{
				m_SpatialStringListBox.SetSel(i, FALSE);
				OnTextSelectSelectionChange();	
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07799")
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnSelectall2() 
{
	try
	{
		int i = 0;
		for (i = 0; i < m_SpatialStringListBox2.GetCount(); i++)
		{
			int isSel =  m_SpatialStringListBox2.GetSel(i);
			ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox2.GetItemDataPtr(i);
			if (isSel == 0)
			{
				m_SpatialStringListBox2.SetSel(i, TRUE);
				OnTextSelect2SelectionChange();	
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08044")
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnUnselectall2() 
{
	try
	{
		int i = 0;
		for (i = 0; i < m_SpatialStringListBox2.GetCount(); i++)
		{
			int isSel =  m_SpatialStringListBox2.GetSel(i);
			ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox2.GetItemDataPtr(i);
			if (isSel != 0)
			{
				m_SpatialStringListBox2.SetSel(i, FALSE);
				OnTextSelect2SelectionChange();	
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08045")
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnCalclineheight2() 
{
	// TODO: Add your control notification handler code here
	// Currently only uses the first window
	// TODO: Add your control notification handler code here
	try
	{
		// compute an weighted average of every selected region
		float totalRatio = 0;
		int totalNumLines = 0;

		int i = 0;
		for(i = 0; i < m_SpatialStringListBox.GetCount(); i++)
		{
			if(m_SpatialStringListBox.GetSel(i) == 0)
			{
				continue;
			}

			ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox.GetItemDataPtr(i);
			ISpatialStringPtr ipSS = info->m_ipSpatialString;

			int iParagraph = 0;
			IIUnknownVectorPtr ipParagraphs;
			ipParagraphs = ipSS->GetParagraphs();
			for(iParagraph = 0; iParagraph < ipParagraphs->Size(); iParagraph++)
			{

				ISpatialStringPtr ipParagraph = (ISpatialStringPtr)ipParagraphs->At(iParagraph);
			
				// get the tallest character in the paragraph
				int totLetterHeight = 0;
				int numLetters = 0;
				
				IIUnknownVectorPtr ipLetters = ipParagraph->GetLetters();
				int iLetter = 0;
				for(iLetter = 0; iLetter < ipLetters->Size(); iLetter++)
				{
					ILetterPtr ipLetter = (ILetterPtr)ipLetters->At(iLetter);
					
					if(ipLetter->GetIsSpatialChar() == VARIANT_FALSE)
					{
						continue;
					}
					char c = ipLetter->GetGuess1();
					if( !((c <'A' || c > 'Z') && 
						c != 'i' &&
						c != 'l' &&
						c != 'f' &&
						c != 'd' &&
						c != 'b' &&
						c != 'g' &&
						c != 'y' ))
						continue;
					if( c < 'a' || c > 'z')
						continue;
					int height = ipLetter->GetBottom() - ipLetter->GetTop();
					totLetterHeight += height;
					numLetters++;
					
				}
				if(numLetters <= 0)
					continue;
				int avgLetterHeight = totLetterHeight / numLetters;
				if(avgLetterHeight == 0)
				{
					continue;
				}

				int totalParagraphLineHeight = 0;
				int numParagraphLines = 0;

				IIUnknownVectorPtr ipLines = ipParagraph->GetLines();
				if(ipLines->Size() < 2)
				{
					continue;
				}
				int iLine = 0;
				for(iLine = 0; iLine < ipLines->Size() - 1; iLine++)
				{
					ISpatialStringPtr ipLine1 = (ISpatialStringPtr)ipLines->At(iLine);
					ISpatialStringPtr ipLine2 = (ISpatialStringPtr)ipLines->At(iLine + 1);

					ILongRectanglePtr ipBounds1 = ipLine1->GetBounds();
					ILongRectanglePtr ipBounds2 = ipLine2->GetBounds();

					float dh = ipBounds2->GetBottom() - ipBounds1->GetBottom();

					totalParagraphLineHeight += dh;
					numParagraphLines++;
				}

				int avgLineHeight = totalParagraphLineHeight / numParagraphLines;
				float ratio = ((float)avgLineHeight / (float)avgLetterHeight);
				char buf[1024];

				long retHeight = ipParagraph->GetAverageLineHeight();

				sprintf(buf, "Ratio %f\nAvgLetterHeight %d\nReRatio %f\n", ratio, avgLetterHeight, float((float)retHeight / (float)avgLetterHeight));
				if(MessageBox(buf, NULL, MB_OKCANCEL) == IDCANCEL)
				{
					break;
				}
				totalRatio += ratio*numParagraphLines;
				totalNumLines += numParagraphLines;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19495")
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnViewlineheights() 
{
	try
	{
		int i = 0;
		for(i = 0; i < m_SpatialStringListBox.GetCount(); i++)
		{
			if(m_SpatialStringListBox.GetSel(i) == 0)
			{
				continue;
			}
			ListItemInfo* info = (ListItemInfo*)m_SpatialStringListBox.GetItemDataPtr(i);
			ISpatialStringPtr ipSS = info->m_ipSpatialString;

			int lineHeight = ipSS->GetAverageLineHeight();
			int charWidth = ipSS->GetAverageCharWidth();
			deleteZoneEntity(m_ipSRIR1, info->m_id);
			info->m_id = createZoneEntity(m_ipSRIR1, ipSS, info->m_page, 0xff1111);

			char buf[1024];
			sprintf(buf, "Line Height %d\nCharWidth %d\n", lineHeight, charWidth);
			if(MessageBox(buf, NULL, MB_OKCANCEL) == IDCANCEL)
			{
				break;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07804")
}
//-------------------------------------------------------------------------------------------------
// Private Helper functions
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::clearHighlights(const ISpotRecognitionWindowPtr ipSRIR, CListBox* pListBox)
{
	int i = 0;
	for(i = 0; i < pListBox->GetCount(); i++)
	{
		ListItemInfo* info = (ListItemInfo*)pListBox->GetItemDataPtr(i);
		deleteZoneEntity(ipSRIR, info->m_id);
		info->m_bWasSelected = false;
		pListBox->SetSel(i, false);
	}
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::clear(const ISpotRecognitionWindowPtr ipSRIR, CListBox* pListBox)
{
	clearHighlights(ipSRIR, pListBox);
	int i = 0;
	for(i = 0; i < pListBox->GetCount(); i++)
	{
		ListItemInfo* info = (ListItemInfo*)pListBox->GetItemDataPtr(i);
		delete info;
		info = NULL;
	}
	pListBox->ResetContent();
}
//-------------------------------------------------------------------------------------------------
long CDecompositionViewerDlg::createZoneEntity(const ISpotRecognitionWindowPtr ipSRIR, const ISpatialStringPtr ipSpatialString, long page, long color)
{
	IRasterZonePtr pZone;
	pZone.CreateInstance(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI08042", pZone != NULL);
	
	ILongRectanglePtr pRect = ipSpatialString->GetBounds();
	long left, right, top, bottom;
	pRect->get_Left(&left);
	pRect->get_Right(&right);
	pRect->get_Top(&top);
	pRect->get_Bottom(&bottom);

	pZone->put_StartX(left);
	pZone->put_StartY((top+bottom)/2);

	pZone->put_EndX(right);
	pZone->put_EndY((top+bottom)/2);

	// Goofy restrictions on ZoneEntities
	long height = (long)bottom-top;
	if(height < 5)
		height = 5;
	if(height % 2 == 0)
		height++;
	pZone->put_Height(height);
	pZone->put_PageNumber(page);

	long id = ipSRIR->CreateZoneEntity(pZone, color );

	return id;
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::deleteZoneEntity(const ISpotRecognitionWindowPtr ipSRIR, long id)
{
	ipSRIR->DeleteZoneEntity(id);
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::populateList(EListRegionType eType, const ISpotRecognitionWindowPtr ipSRIR, ISpatialStringPtr ipSpatialString, CListBox* pListBox)
{
	try
	{
		if(ipSpatialString->IsSpatial() == VARIANT_FALSE)
		{
			return;
		}

		clear(ipSRIR, pListBox);

		IIUnknownVectorPtr pages;
		pages = ipSpatialString->GetPages();

		int i;
		for(i = 0; i < pages->Size(); i++)
		{
			ISpatialStringPtr pPage =  (ISpatialStringPtr)pages->At(i);

			IIUnknownVectorPtr words;

			switch(eType)
			{
			case kWords:
				words = pPage->GetWords();
				break;
			case kZones:
				words = pPage->GetZones();
				break;
			case kLines:
				words = pPage->GetLines();
				break;
			case kParagraphs:
				words = pPage->GetParagraphs();
				break;
			case kLetters:
				words = pPage->GetZones();
				break;
			case kNone:
				return;
			}

			int j;
			for(j = 0; j < words->Size(); j++)
			{
				ISpatialStringPtr pWord = NULL;
				pWord = (ISpatialStringPtr)words->At(j);
				if(pWord->IsSpatial() == VARIANT_FALSE)
					continue;
				int index = pListBox->AddString((const char*)(pWord->GetString()));
				
				ListItemInfo* info = new ListItemInfo;
				
				info->m_bWasSelected = true;
				info->m_ipSpatialString = pWord;
				info->m_page = i+1;
				pListBox->SetItemDataPtr(index, info);
				pListBox->SetSel(index, info->m_bWasSelected);
				int id = createZoneEntity(ipSRIR, pWord, i+1);
				info->m_id = id;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07514");
}
//-------------------------------------------------------------------------------------------------
void CDecompositionViewerDlg::OnDblclkTextSelect() 
{
	// TODO: Add your control notification handler code here
	
}
//-------------------------------------------------------------------------------------------------

